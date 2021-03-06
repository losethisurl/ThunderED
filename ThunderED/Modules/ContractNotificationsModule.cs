﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Discord;
using ThunderED.API;
using ThunderED.Classes;
using ThunderED.Helpers;
using ThunderED.Json;
using ThunderED.Modules.Sub;
using Color = System.Drawing.Color;

namespace ThunderED.Modules
{
    public class ContractNotificationsModule : AppModuleBase
    {
        public override LogCat Category => LogCat.ContractNotification;
        private readonly int _checkInterval;
        private DateTime _lastCheckTime = DateTime.MinValue;

        public ContractNotificationsModule()
        {
            _checkInterval = Settings.ContractNotificationsModule.CheckIntervalInMinutes;
            if (_checkInterval == 0)
                _checkInterval = 1;
            WebServerModule.ModuleConnectors.Add(Reason, OnAuthRequest);
        }

        private async Task<bool> OnAuthRequest(HttpListenerRequestEventArgs context)
        {
            if (!Settings.Config.ModuleContractNotifications) return false;

            var request = context.Request;
            var response = context.Response;

            try
            {
                var extPort = Settings.WebServerModule.WebExternalPort;
                var port = Settings.WebServerModule.WebListenPort;

                if (request.HttpMethod == HttpMethod.Get.ToString())
                {
                    if (request.Url.LocalPath == "/callback.php" || request.Url.LocalPath == $"{extPort}/callback.php" || request.Url.LocalPath == $"{port}/callback.php")
                    {
                        var clientID = Settings.WebServerModule.CcpAppClientId;
                        var secret = Settings.WebServerModule.CcpAppSecret;
                        var prms = request.Url.Query.TrimStart('?').Split('&');
                        var code = prms[0].Split('=')[1];
                        var state = prms.Length > 1 ? prms[1].Split('=')[1] : null;
                        if (string.IsNullOrEmpty(state) || !state.StartsWith("cauth")) return false;
                        var groupName = HttpUtility.UrlDecode(state.Replace("cauth", ""));

                        var result = await WebAuthModule.GetCharacterIdFromCode(code, clientID, secret);
                        if (result == null)
                        {
                            await WebServerModule.WriteResponce(WebServerModule.GetAccessDeniedPage("Contracts Module", LM.Get("accessDenied")), response);
                            return true;
                        }

                        var lCharId = Convert.ToInt64(result[0]);
                        var group = Settings.ContractNotificationsModule.Groups[groupName];
                        if (!group.CharacterIDs.Contains(lCharId))
                        {
                            await WebServerModule.WriteResponce(WebServerModule.GetAccessDeniedPage("Contracts Module", LM.Get("accessDenied")), response);
                            return true;
                        }

                        await SQLHelper.SQLiteDataInsertOrUpdateTokens("", result[0], "", result[1]);
                        await WebServerModule.WriteResponce(File.ReadAllText(SettingsManager.FileTemplateMailAuthSuccess)
                                .Replace("{header}", "authTemplateHeader")
                                .Replace("{body}", LM.Get("contractAuthSuccessHeader"))
                                .Replace("{body2}", LM.Get("contractAuthSuccessBody"))
                                .Replace("{backText}", LM.Get("backText")), response
                        );
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                await LogHelper.LogEx(ex.Message, ex, Category);
            }
            return false;
        }

        private readonly Dictionary<long, List<JsonClasses.Contract>> _lastContracts = new Dictionary<long, List<JsonClasses.Contract>>();
        private readonly Dictionary<long, List<JsonClasses.Contract>> _lastCorpContracts = new Dictionary<long, List<JsonClasses.Contract>>();
        private readonly List<string> _finishedStatuses = new List<string> {"finished_issuer", "finished_contractor", "finished", "cancelled", "rejected", "failed", "deleted", "reversed"};

        public override async Task Run(object prm)
        {
            if (IsRunning || !Settings.Config.ModuleContractNotifications) return;
            IsRunning = true;
            try
            {
                if ((DateTime.Now - _lastCheckTime).TotalMinutes < _checkInterval) return;
                _lastCheckTime = DateTime.Now;
                await LogHelper.LogModule("Running Contracts module check...", Category);

                foreach (var group in Settings.ContractNotificationsModule.Groups.Values)
                {
                    foreach (var characterID in group.CharacterIDs)
                    {
                        try
                        {
                            var rtoken = await SQLHelper.GetRefreshTokenForContracts(characterID);
                            if(rtoken == null)
                                continue;
                            var token = await APIHelper.ESIAPI.RefreshToken(rtoken, Settings.WebServerModule.CcpAppClientId, Settings.WebServerModule.CcpAppSecret);
                            if(token == null)
                                continue;

                            if (group.FeedPersonalContracts)
                            {
                                var contracts = await SQLHelper.LoadContracts(characterID, false);
                                await ProcessContracts(false, contracts, group, characterID, token);
                            }
                            if (group.FeedCorporateContracts)
                            {
                                var contracts = await SQLHelper.LoadContracts(characterID, true);
                                await ProcessContracts(true, contracts, group, characterID, token);
                            }

                        }
                        catch (Exception ex)
                        {
                            await LogHelper.LogEx("Contracts", ex, Category);
                        }
                    }
                }


                await LogHelper.LogModule("Completed", Category);
            }
            catch (Exception ex)
            {
                await LogHelper.LogEx(ex.Message, ex, Category);
                await LogHelper.LogModule("Completed", Category);
            }
            finally
            {
                IsRunning = false; 
            }
        }

        private async Task ProcessContracts(bool isCorp, List<JsonClasses.Contract> lst, ContractNotifyGroup group, long characterID, string token)
        {
            var maxContracts = Settings.ContractNotificationsModule.MaxTrackingCount > 0 ? Settings.ContractNotificationsModule.MaxTrackingCount : 150;
            var contracts = isCorp ? 
                (await APIHelper.ESIAPI.GetCorpContracts(Reason, characterID, token))?.OrderByDescending(a => a.contract_id).ToList() :
                (await APIHelper.ESIAPI.GetCharacterContracts(Reason, characterID, token))?.OrderByDescending(a => a.contract_id).ToList();
            if (contracts == null)
                return;
            var lastContractId = contracts.FirstOrDefault()?.contract_id ?? 0;
            if (lastContractId == 0) return;

            if (lst == null)
            {
                //initial cache - only progressing contracts
                await SQLHelper.SaveContracts(characterID, contracts.Where(a=> !_finishedStatuses.Contains(a.status)).TakeSmart(maxContracts).ToList(), isCorp);
                return;
            }

            //process cache
            foreach (var contract in lst.ToList())
            {
                var freshContract = contracts.FirstOrDefault(a => a.contract_id == contract.contract_id);
                //check if it present
                if (freshContract == null)
                {
                    lst.Remove(contract);
                    continue;                                        
                }
                //check for completion
                if (_finishedStatuses.Contains(freshContract.status))
                {
                    await PrepareFinishedDiscordMessage(group.DiscordChannelId, freshContract, group.DefaultMention, isCorp, characterID, token);
                    lst.Remove(contract);
                    continue;
                }
                //check for accepted
                if (contract.type == "courier" && contract.status == "outstanding" && freshContract.status == "in_progress")
                {
                    await PrepareAcceptedDiscordMessage(group.DiscordChannelId, freshContract, group.DefaultMention, isCorp, characterID, token);
                    var index = lst.IndexOf(contract);
                    lst.Remove(contract);
                    lst.Insert(index, freshContract);
                    continue;
                }
            }

            //update cache list and look for new contracts
            var lastRememberedId = lst.FirstOrDefault()?.contract_id ?? 0;
            if (lastContractId > lastRememberedId)
            {
                //get and report new contracts, forget already finished
                var list = contracts.Where(a => a.contract_id > lastRememberedId && !_finishedStatuses.Contains(a.status)).ToList();
                foreach (var contract in list)
                {
                    try
                    {
                        await PrepareDiscordMessage(group.DiscordChannelId, contract, group.DefaultMention, isCorp, characterID, token);
                    }
                    catch (Exception ex)
                    {
                        await LogHelper.LogEx($"Contract {contract.contract_id}", ex, Category);

                    }
                }

                if (list.Count > 0)
                {
                    lst.InsertRange(0, list);
                    //cut
                    if (lst.Count >= maxContracts)
                    {
                        var count = lst.Count - maxContracts;
                        lst.RemoveRange(lst.Count - count, count);
                    }
                }
            }

            await SQLHelper.SaveContracts(characterID, lst, isCorp);

        }


        private async Task PrepareAcceptedDiscordMessage(ulong channelId, JsonClasses.Contract contract, string mention, bool isCorp, long characterId, string token)
        {
            await PrepareDiscordMessage(channelId, contract, mention, isCorp, characterId, token);

        }

        private async Task PrepareFinishedDiscordMessage(ulong channelId, JsonClasses.Contract contract, string mention, bool isCorp, long characterId, string token)
        {
            await PrepareDiscordMessage(channelId, contract, mention, isCorp, characterId, token);
        }

        private async Task PrepareDiscordMessage(ulong channelId, JsonClasses.Contract contract, string mention, bool isCorp, long characterId, string token)
        {
            var image = string.Empty;
            var typeName = string.Empty;
            uint color = 0xff0000;
            switch (contract.status)
            {
                //finished
                case var s when _finishedStatuses.Contains(s):
                    image = Settings.Resources.ImgContractDelete;
                    break;
                default:
                    image = Settings.Resources.ImgContract;
                    break;
            }

            var availName = string.Empty;
            switch (contract.availability)
            {
                case "public":
                    availName = "Public";
                    break;
                case "personal":
                    availName = "Personal";
                    break;
                case "corporation":
                    availName = "Corporation";
                    break;
                case "alliance":
                    availName = "Alliance";
                    break;
                default:
                    return;
            }

            var statusName = string.Empty;
            switch (contract.status)
            {
                case "finished_issuer":
                case "finished_contractor":
                case "finished":
                    statusName = "Completed";
                    color = 0x00ff00;
                    break;
                case "cancelled":
                    statusName = "Cancelled";
                    break;
                case "rejected":
                    statusName = "Rejected";
                    break;
                case "failed":
                    statusName = "Failed";
                    break;
                case "deleted":
                    statusName = "Deleted";
                    break;
                case "reversed":
                    statusName = "Reversed";
                    break;
                case "in_progress":
                    statusName = "In Progress";
                    color = 0xFFFF33;
                    break;
                case "outstanding":
                    statusName = "Outstanding";
                    color = 0xFFFF33;
                    break;
                default:
                    return;
            }

            var days = 0;
            var expire = 0;
            var endLocation = string.Empty;
            switch (contract.type)
            {
                case "item_exchange":
                    typeName = "Item Exchange";
                    break;
                case "auction":
                    typeName = "Auction";
                    break;
                case "courier":
                    typeName = "Courier";
                    days = contract.days_to_complete;
                    expire = (int)(contract.DateExpired - contract.DateIssued).Value.TotalDays;
                    endLocation = (await APIHelper.ESIAPI.GetStructureData(Reason, contract.end_location_id, token))?.name;
                    if(endLocation == null)
                        endLocation = (await APIHelper.ESIAPI.GetStationData(Reason, contract.end_location_id, token))?.name;
                    endLocation = string.IsNullOrEmpty(endLocation) ? "(Some Citadel)" : endLocation;
                    break;
                default: 
                    return;
            }
            var subject = $"{typeName} {LM.Get("contractSubject")}";


            var ch = await APIHelper.ESIAPI.GetCharacterData(Reason, contract.issuer_id);
            var issuerName = ch.name;
            
            //location

            var startLocation = (await APIHelper.ESIAPI.GetStructureData(Reason, contract.start_location_id, token))?.name;
            if(startLocation == null)
                startLocation = (await APIHelper.ESIAPI.GetStationData(Reason, contract.start_location_id, token))?.name;
            startLocation = string.IsNullOrEmpty(startLocation) ? "(Some Citadel)" : startLocation;

            var sbNames = new StringBuilder();
            var sbValues = new StringBuilder();

            sbNames.Append("Type: \nIssued By: \nAvailability: ");
            if(contract.acceptor_id > 0)
                sbNames.Append("\nContractor: ");
            sbNames.Append("\nStatus: ");
            if (contract.type == "courier")
                sbNames.Append("\nCollateral: \nReward: \nComplete in: \n Expire in: ");
            else
            {
                sbNames.Append(contract.price > 0 ? "\nPrice: " : "\nREWARD: ");
                if(contract.type == "auction")
                    sbNames.Append("\nBuyout: ");
            }

            sbValues.Append($"{typeName}\n[{issuerName}](https://zkillboard.com/character/{contract.issuer_id}/)\n{availName}");
            if (contract.acceptor_id > 0)
            {
                ch = await APIHelper.ESIAPI.GetCharacterData(Reason, contract.acceptor_id);
                sbValues.Append($"\n[{ch.name}](https://zkillboard.com/character/{contract.acceptor_id}/)");
            }
            sbValues.Append($"\n{statusName}");
            if (contract.type == "courier")
                sbValues.Append($"\n{contract.collateral:N}\n{contract.reward:N}\n{days} days\n{expire} days");
            else
            {
                sbValues.Append(contract.price > 0 ? $"\n{contract.price:N}" : $"\n{contract.reward:N}");
                if(contract.type == "auction")
                    sbValues.Append($"\n{contract.buyout:N}");
            }

            var title = string.IsNullOrEmpty(contract.title) ? "-" : contract.title;
            var stampIssued = contract.DateIssued?.ToString(Settings.Config.ShortTimeFormat);
            var stampAccepted = contract.DateAccepted?.ToString(Settings.Config.ShortTimeFormat);
            var stampCompleted = contract.DateCompleted?.ToString(Settings.Config.ShortTimeFormat);
            var stampExpired = contract.DateExpired?.ToString(Settings.Config.ShortTimeFormat);

            var items = isCorp ?
                await APIHelper.ESIAPI.GetCorpContractItems(Reason, contract.issuer_corporation_id, contract.contract_id, token) :
                await APIHelper.ESIAPI.GetCharacterContractItems(Reason, characterId, contract.contract_id, token);

            var sbItemsSubmitted = new StringBuilder();
            var sbItemsAsking = new StringBuilder();
            if (items.Count > 0)
            {
                foreach (var item in items)
                {
                    var t = await APIHelper.ESIAPI.GetTypeId(Reason, item.type_id);
                    if(item.is_included)
                        sbItemsSubmitted.Append($"{t?.name} x{item.quantity}\n");
                    else sbItemsAsking.Append($"{t?.name} x{item.quantity}\n");
                }
            }

            var issuedText = $"Issued: {stampIssued}";


            var embed = new EmbedBuilder()
                .WithThumbnailUrl(image)
                .WithColor(color)
                .AddField(subject, title)
                .AddField("Location", startLocation);

            if (contract.type == "courier")
                embed.AddField("Destination", endLocation);

            embed.AddInlineField("Details", sbNames.ToString())
                .AddInlineField("-", sbValues.ToString())
                .WithFooter(issuedText);

            if (sbItemsSubmitted.Length > 0)
            {
                var fields = sbItemsSubmitted.ToString().Split(1023).TakeSmart(5).ToList();
                var head = fields.FirstOrDefault();
                fields.RemoveAt(0);
                embed.AddField($"Included Items", string.IsNullOrWhiteSpace(head) ? "---" : head);
                foreach (var field in fields)
                    embed.AddField($"-", string.IsNullOrWhiteSpace(field) ? "---" : field);
            }
            if (sbItemsAsking.Length > 0)
            {
                var fields = sbItemsAsking.ToString().Split(1023).TakeSmart(5).ToList();
                var head = fields.FirstOrDefault();
                fields.RemoveAt(0);
                embed.AddField($"Asking Items", string.IsNullOrWhiteSpace(head) ? "---" : head);
                foreach (var field in fields)
                    embed.AddField($"-", string.IsNullOrWhiteSpace(field) ? "---" : field);
            }

            await APIHelper.DiscordAPI.SendMessageAsync(APIHelper.DiscordAPI.GetChannel(channelId), $"{mention} >>>\n", embed.Build()).ConfigureAwait(false);
        }
    }
}
