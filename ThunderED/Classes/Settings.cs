﻿#if EDITOR
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ThunderED.Classes;

namespace TED_ConfigEditor.Classes
#else
using System.Collections.Generic;

namespace ThunderED.Classes
#endif
{
    public class ThunderSettings: SettingsBase<ThunderSettings>
    {
        [ConfigEntryName("")]
        public ConfigSettings Config { get; set; } = new ConfigSettings();
        [ConfigEntryName("moduleWebServer")]
        public WebServerModuleSettings WebServerModule { get; set; } = new WebServerModuleSettings();
        [ConfigEntryName("moduleAuthWeb")]
        public WebAuthModuleSettings WebAuthModule { get; set; } = new WebAuthModuleSettings();
        [ConfigEntryName("moduleChatRelay")]
        public ChatRelayModuleSettings ChatRelayModule { get; set; } = new ChatRelayModuleSettings();
        [ConfigEntryName("moduleIncursionNotify")]
        public IncursionNotificationModuleSettings IncursionNotificationModule { get; set; } = new IncursionNotificationModuleSettings();
        [ConfigEntryName("moduleIRC")]
        public IRCModuleSettings IrcModule { get; set; } = new IRCModuleSettings();
        [ConfigEntryName("moduleNotificationFeed")]
        public NotificationFeedSettings NotificationFeedModule { get; set; } = new NotificationFeedSettings();
        [ConfigEntryName("moduleNullsecCampaign")]
        public NullCampaignModuleSettings NullCampaignModule { get; set; } = new NullCampaignModuleSettings();
        [ConfigEntryName("moduleTelegram")]
        public TelegramModuleSettings TelegramModule { get; set; } = new TelegramModuleSettings();
        [ConfigEntryName("moduleMail")]
        public MailModuleSettings MailModule { get; set; } = new MailModuleSettings();
        [ConfigEntryName("moduleTimers")]
        public TimersModuleSettings TimersModule { get; set; } = new TimersModuleSettings();
        [ConfigEntryName("moduleRadiusKillFeed")]
        public RadiusKillFeedModuleSettings RadiusKillFeedModule { get; set; } = new RadiusKillFeedModuleSettings();
        [ConfigEntryName("moduleLiveKillFeed")]
        public LiveKillFeedModuleSettings LiveKillFeedModule { get; set; } = new LiveKillFeedModuleSettings();
        [ConfigEntryName("")]
        [StaticConfigEntry]
        public ResourcesSettings Resources { get; set; } = new ResourcesSettings();
        [ConfigEntryName("moduleFleetup")]
        public FleetupModuleSettings FleetupModule { get; set; } = new FleetupModuleSettings();
        [ConfigEntryName("moduleJabber")]
        public JabberModuleSettings JabberModule { get; set; } = new JabberModuleSettings();
        [ConfigEntryName("moduleHRM")]
        public HRMModuleSettings HRMModule { get; set; } = new HRMModuleSettings();
        [ConfigEntryName("ModuleSystemLogFeeder")]
        public SystemLogFeederSettings SystemLogFeederModule { get; set; } = new SystemLogFeederSettings();
        [ConfigEntryName("ModuleStats")]
        public StatsModuleSettings StatsModule { get; set; } = new StatsModuleSettings();

        [ConfigEntryName("")]
        [StaticConfigEntry]
        public ContinousCheckModuleSettings ContinousCheckModule { get; set; } = new ContinousCheckModuleSettings();

        [ConfigEntryName("ModuleContractNotifications")]
        public ContractNotificationsModuleSettings ContractNotificationsModule { get; set; } = new ContractNotificationsModuleSettings();

#if EDITOR
        public string Validate(List<string> usedModules)
        {
            var sb = new StringBuilder();

            if ((Config.ModuleNotificationFeed || Config.ModuleAuthWeb || Config.ModuleMail || Config.ModuleTimers) && !Config.ModuleWebServer)
            {
                sb.AppendLine("General Config Settings");
                sb.AppendLine("ModuleWebServer must be enabled if you plan to use Notifications, WebAuth, Mail or Timers modules!\n");
            }

           // var checkList = usedModules?.Select(a => a.ToString()).ToList() ?? new List<string>();
            foreach (var info in GetType().GetProperties().Where(a=> typeof(IValidatable).IsAssignableFrom(a.PropertyType)))
            {
                if(info.Name != "Config" && !usedModules.Contains(info.GetValue(this).GetAttributeValue<ConfigEntryNameAttribute>("Name"))) continue;

                var p = info.GetValue(this);
                var result = (string)info.PropertyType.GetMethod("Validate").Invoke(p, new object[] { false });
                if (!string.IsNullOrEmpty(result))
                {
                    sb.Append(result);
                    sb.Append("\n\n");
                }
            }

            return sb.ToString();
        }
#endif
    }

    public class ContractNotificationsModuleSettings: ValidatableSettings
    {
        [Required]
        [Comment("Check interval in minutes")]
        public int CheckIntervalInMinutes { get; set; } = 1;

        [Comment("Maximum number of last contracts to check")]
        public int MaxTrackingCount { get; set; } = 150;
#if EDITOR
        public ObservableDictionary<string, ContractNotifyGroup> Groups = new ObservableDictionary<string, ContractNotifyGroup>();
#else
        public Dictionary<string, ContractNotifyGroup> Groups = new Dictionary<string, ContractNotifyGroup>();
#endif
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(CheckIntervalInMinutes):
                        return CheckIntervalInMinutes == 0? Compose(nameof(CheckIntervalInMinutes), "CheckIntervalInMinutes must be greater than 0!") : null;
                }

                return null;
            }
        }
#endif
    }

    public class ContractNotifyGroup: ValidatableSettings
    {
#if EDITOR
        public ObservableCollection<long> CharacterIDs { get; set; } = new ObservableCollection<long>();
        public ObservableCollection<string> ContractAvailability { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ContractTypes { get; set; } = new ObservableCollection<string>();
#else
        public List<long> CharacterIDs { get; set; } = new List<long>();
        public List<string> ContractAvailability { get; set; } = new List<string>();
        public List<string> ContractTypes { get; set; } = new List<string>();
#endif
        public ulong DiscordChannelId { get; set; }
        public bool FeedPersonalContracts { get; set; } = true;
        public bool FeedCorporateContracts { get; set; } = true;
        public string ButtonText { get; set; } = "Default Contracts Auth";
        public string DefaultMention { get; set; }

#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(CharacterIDs):
                        return CharacterIDs.Count == 0? Compose(nameof(CharacterIDs), "CharacterIDs must be set!") : null;
                }

                return null;
            }
        }
#endif
    }

    public class StatsModuleSettings
    {
        public bool EnableStatsCommand { get; set; } = true;
#if EDITOR
        public ObservableDictionary<string, DailyStatsGroup> DailyStatsGroups { get; set; } = new  ObservableDictionary<string, DailyStatsGroup>();
#else
        public Dictionary<string, DailyStatsGroup> DailyStatsGroups { get; set; } = new  Dictionary<string, DailyStatsGroup>();
#endif

    }

    public class DailyStatsGroup: ValidatableSettings
    {
        [Comment("Numeric discord channel ID for auto posting daily stats upon new day")]
        public ulong DailyStatsChannel { get; set; }
        [Comment("Default numeric corporation ID to display stats for. Mutually exclusive with DailyStatsAlliance")]
        public long DailyStatsCorp { get; set; }
        [Comment("Default numeric alliance ID to display stats for. Mutually exclusive with DailyStatsCorp")]
        public long DailyStatsAlliance { get; set; }
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(DailyStatsChannel):
                        return DailyStatsChannel == 0? Compose(nameof(DailyStatsChannel), "DailyStatsChannel must be specified!") : null;
                    case nameof(DailyStatsCorp):
                        return DailyStatsCorp == 0 && DailyStatsAlliance == 0? Compose(nameof(DailyStatsAlliance), "DailyStatsCorp or DailyStatsAlliance must be specified!") : null;
                }

                return null;
            }
        }
#endif
    }


    public class HRMModuleSettings: ValidatableSettings
    {
#if EDITOR
        public ObservableCollection<long> UsersAccessList { get; set; } = new ObservableCollection<long>();
        public ObservableCollection<string> RolesAccessList { get; set; } = new ObservableCollection<string>();
#else
        public List<long> UsersAccessList { get; set; } = new List<long>();
        public List<string> RolesAccessList { get; set; } = new List<string>();
#endif
        [Comment("Authentication timeout in minutes")]
        public int AuthTimeoutInMinutes { get; set; } = 10;
        [Comment("Number of entries to display in tables")]
        public int TableEntriesPerPage { get; set; } = 10;

        [Comment("Number of skill entries to display in tables")]
        public int TableSkillEntriesPerPage { get; set; } = 20;
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(UsersAccessList):
                        return UsersAccessList.Count == 0 && RolesAccessList.Count == 0? Compose(nameof(UsersAccessList), "Either UsersAccessList or RolesAccessList must be specified to protect sensitive data!") : null;
                    case nameof(AuthTimeoutInMinutes):
                        return AuthTimeoutInMinutes <= 0 ? Compose(nameof(AuthTimeoutInMinutes), "It is security unwise to set unlimited HR session") : null;
                    case nameof(TableEntriesPerPage):
                        return TableEntriesPerPage <= 0 ? Compose(nameof(TableEntriesPerPage), "Number of entries in a table must be greater than 0") : null;
                }

                return null;
            }
        }
#endif
    }

    public class ContinousCheckModuleSettings: ValidatableSettings
    {
        [Comment("Enable posting about TQ status into specified channels")]
        public bool EnableTQStatusPost { get; set; } = true;

        [Comment("Discord mention string to use for message")]
        public string TQStatusPostMention { get; set; } = "@here";
#if EDITOR
        public ObservableCollection<ulong> TQStatusPostChannels { get; set; } = new ObservableCollection<ulong>();
#else
        public List<ulong> TQStatusPostChannels { get; set; } = new List<ulong>();
#endif

#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                return null;
            }
        }
#endif
    }

    public class JabberModuleSettings: ValidatableSettings
    {
        public string Domain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Filter { get; set; }
        public bool Debug { get; set; }
        public ulong DefChan { get; set; }
#if EDITOR
        public ObservableDictionary<string, ulong> Filters { get; set; } = new ObservableDictionary<string, ulong>();
#else
        public Dictionary<string, ulong> Filters { get; set; } = new Dictionary<string, ulong>();
#endif
        public string Prepend { get; set; } = "@here";

#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Domain):
                        return string.IsNullOrEmpty(Domain)? Compose(nameof(Domain), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(Username):
                        return string.IsNullOrEmpty(Username)? Compose(nameof(Username), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(Password):
                        return string.IsNullOrEmpty(Password)? Compose(nameof(Password), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(DefChan):
                        return DefChan == 0? Compose(nameof(DefChan), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class FleetupModuleSettings: ValidatableSettings
    {
        [Comment("FleetUp user ID")]
        [Required]
        public string UserId { get; set; }
        [Comment("FleetUp API code")]
        [Required]
        public string APICode { get; set; }
        [Comment("FleetUp application key")]
        [Required]
        public string AppKey { get; set; }
        [Required]
        public string GroupID { get; set; }
        [Required]
        public ulong Channel { get; set; }
        public bool Announce_Post { get; set; }
        [Comment("Default Discord mention for report")]
        public string DefaultMention { get; set; }
#if EDITOR
        public ObservableCollection<long> Announce { get; set; } = new ObservableCollection<long>();
#else
        public List<long> Announce { get; set; } = new List<long>();
#endif
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(UserId):
                        return string.IsNullOrEmpty(UserId)? Compose(nameof(UserId), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(APICode):
                        return string.IsNullOrEmpty(APICode)? Compose(nameof(APICode), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(AppKey):
                        return string.IsNullOrEmpty(AppKey)? Compose(nameof(AppKey), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(GroupID):
                        return string.IsNullOrEmpty(GroupID)? Compose(nameof(GroupID), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(Channel):
                        return Channel == 0? Compose(nameof(Channel), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class ResourcesSettings
    {
        public string ImgCitLowPower { get; set; }
        public string ImgCitUnderAttack { get; set; }
        public string ImgCitAnchoring { get; set; }
        public string ImgCitDestroyed { get; set; }
        public string ImgCitLostShield { get; set; }
        public string ImgCitLostArmor { get; set; }
        public string ImgCitOnline { get; set; }
        public string ImgCitFuelAlert { get; set; }
        public string ImgCitServicesOffline { get; set; }
        public string ImgLowFWStand { get; set; }
        public string ImgMoonComplete { get; set; }
        public string ImgWarAssist { get; set; }
        public string ImgWarDeclared { get; set; }
        public string ImgWarInvalidate { get; set; }
        public string ImgWarSurrender { get; set; }
        public string ImgTimerAlert { get; set; }
        public string ImgMail { get; set; }
        public string ImgIncursion { get; set; }
        public string ImgFactionCaldari { get; set; }
        public string ImgFactionGallente { get; set; }
        public string ImgFactionAmarr { get; set; }
        public string ImgFactionMinmatar { get; set; }
        public string ImgEntosisAlert { get; set; }
        public string ImgContract { get; set; }
        public string ImgContractDelete { get; set; }
    }

    public class LiveKillFeedModuleSettings: ValidatableSettings
    {
        [Comment("Enable or disable caching. If you're getting many global KMs it might be better to disable it to free database from large chunks of one time data")]
        public bool EnableCache { get; set; }
        [Comment("Numeric value in ISK to consider the kill to be BIG enough")]
        public long BigKill { get; set; }
        [Comment("Numeric channel ID to post all GLOBAL big kills in EVE, 0 to skip")]
        public ulong BigKillChannel { get; set; }
#if EDITOR
        public ObservableDictionary<string, KillFeedGroup> GroupsConfig { get; set; } = new ObservableDictionary<string, KillFeedGroup>();
#else
        public Dictionary<string, KillFeedGroup> GroupsConfig { get; set; } = new Dictionary<string, KillFeedGroup>();
#endif
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(GroupsConfig):
                        return GroupsConfig.Count == 0? Compose(nameof(GroupsConfig), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class KillFeedGroup: ValidatableSettings
    {
        [Comment("Numeric ID of the Discord channel to post KMs of this group into")]
        [Required]
        public ulong DiscordChannel { get; set; }
        [Comment("Numeric corporation ID. Specify to fetch all KMs for this corporation. \nMutually exclusive with allianceID.")]
        [Required]
        public long CorpID { get; set; }
        [Comment("Numeric alliance ID. Specify to fetch all KMs for this alliance. Mutually exclusive with corpID")]
        [Required]
        public long AllianceID { get; set; }
        [Comment("Minimum KM ISK value")]
        public long MinimumValue { get; set; }
        [Comment("minimum loss KM ISK value. Set to very high value to disable losses")]
        public long MinimumLossValue { get; set; }
        [Comment("Minimum KM ISK value to be considered as BIG for this group")]
        public long BigKillValue { get; set; }
        [Comment("Post BIG KMs to separate channel. Numeric channel ID value")]
        public ulong BigKillChannel { get; set; }
        [Comment("Also post big kills to the channel specified in discordChannel setting")]
        public bool BigKillSendToGeneralToo { get; set; }
        [Comment("Post group name into notification message")]
        public bool ShowGroupName { get; set; }

        public bool FeedPvpKills { get; set; } = true;
        public bool FeedPveKills { get; set; } = true;

#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(DiscordChannel):
                        return DiscordChannel == 0? Compose(nameof(DiscordChannel), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(CorpID):
                        return CorpID == 0 && AllianceID == 0? Compose(nameof(AllianceID), "Either CorpID or AllianceID must be specified!") : null;
                }

                return null;
            }
        }
#endif
    }

    public class RadiusKillFeedModuleSettings: ValidatableSettings
    {
        [Comment("Enable or disable caching. If you're getting many global KMs it might be better \nto disable it to free database from large chunks of one time data.")]
        public bool EnableCache { get; set; }
#if EDITOR
        [Comment("Can have several feeds (groups) defined")]
        public ObservableDictionary<string, RadiusGroup> GroupsConfig { get; set; } = new ObservableDictionary<string, RadiusGroup>();
#else
        public Dictionary<string, RadiusGroup> GroupsConfig { get; set; } = new Dictionary<string, RadiusGroup>();
#endif
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(GroupsConfig):
                        return GroupsConfig.Count == 0 ? Compose(nameof(GroupsConfig), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class RadiusGroup: ValidatableSettings
    {
        [Comment("Numeric value of number of jump around specified system. Leave 0 and specify radiusSystemId to limit feed by only one system")]
        public int Radius { get; set; }
        [Comment("Discord channel ID to post messages")]
        public ulong RadiusChannel { get; set; }
        [Comment("Numeric radius central system ID (even wormhole J system can be specified). \nYou should specify only one of the following IDs: system, constellation or region")]
        public long RadiusSystemId { get; set; }
        public long RadiusConstellationId { get; set; }
        public long RadiusRegionId { get; set; }
        [Comment("Minimum ISK value to filter killmails")]
        public long MinimumValue { get; set; }
        [Comment("Post group name into notification message")]
        public bool ShowGroupName { get; set; }

        public bool FeedPvpKills { get; set; } = true;
        public bool FeedPveKills { get; set; } = true;

#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Radius):
                        return Radius > 0 && RadiusSystemId == 0 ? Compose(nameof(Radius), "Radius is greater than 0 but RadiusSystemId = 0 so it won't affect anything!") : null;
                    case nameof(RadiusConstellationId):
                        return RadiusConstellationId == 0 && RadiusSystemId == 0 && RadiusRegionId == 0 ? Compose(nameof(RadiusConstellationId), "You must specify either RadiusSystemId, RadiusConstellationId or RadiusRegionId!") : null;
                    case nameof(RadiusChannel):
                        return RadiusChannel == 0 ? Compose(nameof(RadiusChannel), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class TimersModuleSettings: ValidatableSettings
    {
        [Comment("Automatically add timer event upon receiving structure reinforce notification (if notifications feed module is enabled)")]
        public bool AutoAddTimerForReinforceNotifications { get; set; } = true;
        [Comment("Web session timeout in minutes")]
        public int AuthTimeoutInMinutes { get; set; } = 10;
        [Comment("Link to a tiny url representation which is created manually and overrides standard URL for !turl command")]
        public string TinyUrl { get; set; }

        [Comment("Time format for new timers input CAPS SENSITIVE. Default: DD.MM.YYYY HH:mm")]
        public string TimeInputFormat { get; set; } = "DD.MM.YYYY HH:mm";
        [Comment("Optional Discord defult mention for timer report")]
        public string DefaultMention { get; set; }

#if EDITOR
        [Comment("List of numeric values representing the time in minutes to send timer reminder message to discord when specified amount of minutes is left before the timer ends")]
        public ObservableCollection<int> Announces { get; set; } = new ObservableCollection<int>();
#else
        public List<int> Announces { get; set; } = new List<int>();
#endif
        [Comment("Discord channel ID for announce messages")]
        [Required]
        public ulong AnnounceChannel { get; set; }
        [Comment("Auto grant editor roles to Discord admins based on the config section")]
        public bool GrantEditRolesToDiscordAdmins { get; set; } = true;
#if EDITOR
        [Comment("List of entities which has view access to the timers page")]
        [Required]
        public ObservableDictionary<string, TimersAccessGroup> AccessList { get; set; } = new ObservableDictionary<string, TimersAccessGroup>();
        [Comment("List of entities which has edit access on the timers page")]
        [Required]
        public ObservableDictionary<string, TimersAccessGroup> EditList { get; set; } = new ObservableDictionary<string, TimersAccessGroup>();
#else
        public Dictionary<string, TimersAccessGroup> AccessList { get; set; } = new Dictionary<string, TimersAccessGroup>();
        public Dictionary<string, TimersAccessGroup> EditList { get; set; } = new Dictionary<string, TimersAccessGroup>();
#endif
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(AnnounceChannel):
                        return  AnnounceChannel == 0 ? Compose(nameof(AnnounceChannel), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(AccessList):
                        return AccessList.Count == 0 ? Compose(nameof(AccessList), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(EditList):
                        return EditList.Count == 0 ? Compose(nameof(EditList), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class TimersAccessGroup: ValidatableSettings
    {
        [Comment("Is this an alliance or corporation ID")]
        public bool IsAlliance { get; set; }
        [Comment("Is this a character ID. Has priority over **isAlliance** value")]
        public bool IsCharacter { get; set; }
        [Comment("Is this a corporation ID")]
        public bool IsCorporation { get; set; }
        [Comment("Numeric ID value of the entity")]
        [Required]
        public long Id { get; set; }

#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(IsAlliance):
                        return !IsAlliance && !IsCorporation && !IsCharacter ? Compose(nameof(IsAlliance), "Either IsAlliance, IsCorporation or IsCharacter must be selected!") : null;
                    case nameof(Id):
                        return Id == 0 ? Compose(nameof(Id), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class MailModuleSettings: ValidatableSettings
    {
        [Comment("Interval in minutes to check for new mail")]
        [Required]
        public int CheckIntervalInMinutes { get; set; } = 2;
#if EDITOR
        [Comment("Character groups allowed to auth as mail feeders")]
        [Required]
        public ObservableDictionary<string, MailAuthGroup> AuthGroups { get; set; } = new ObservableDictionary<string, MailAuthGroup>();
#else
        public Dictionary<string, MailAuthGroup> AuthGroups { get; set; } = new Dictionary<string, MailAuthGroup>();
#endif
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(CheckIntervalInMinutes):
                        return CheckIntervalInMinutes < 1 ? Compose(nameof(CheckIntervalInMinutes), "Value must be greater than 0 or the bot will blow up!") : null;
                    case nameof(AuthGroups):
                        return AuthGroups.Count == 0 ? Compose(nameof(AuthGroups), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class MailAuthGroup: ValidatableSettings
    {
        [Comment("EVE Online character ID")]
        [Required]
        public long Id { get; set; }

        [Comment("Include private mail to this feed")]
        public bool IncludePrivateMail { get; set; }
#if EDITOR
        public ObservableDictionary<string, MailAuthFilter> Filters { get; set; } = new ObservableDictionary<string, MailAuthFilter>();
#else
        public Dictionary<string, MailAuthFilter> Filters { get; set; } = new Dictionary<string, MailAuthFilter>();
#endif
        [Comment("Numeric Discord channel ID to post mail feed")]
        [Required]
        public ulong DefaultChannel { get; set; }

        [Comment("Optional Discord default mention for mail report")]
        public string DefaultMention { get; set; }


#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Id):
                        return Id == 0 ? Compose(nameof(Id), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(DefaultChannel):
                        return DefaultChannel == 0 ? Compose(nameof(DefaultChannel), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class MailAuthFilter: ValidatableSettings
    {
#if EDITOR
        [Comment("List of in game EVE mail label names which will be used to mark and fetch mails")]
        public ObservableCollection<string> FilterLabels { get; set; } = new ObservableCollection<string>();
        [Comment("List of 'FROM' character IDs to filter incoming mail")]
        public ObservableCollection<long> FilterSenders { get; set; } = new ObservableCollection<long>();
        [Comment("List of EVE MailList names to filter incoming mail")]
        public ObservableCollection<string> FilterMailList { get; set; } = new ObservableCollection<string>();
#else
        public List<string> FilterLabels { get; set; } = new List<string>();
        public List<long> FilterSenders { get; set; } = new List<long>();
        public List<string> FilterMailList { get; set; } = new List<string>();
#endif
        [Comment("Optional numeric Discord channel ID to post filtered mail feed")]
        public ulong FeedChannel { get; set; }
        
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(FilterLabels):
                        return FilterLabels.Count == 0 && FilterSenders.Count == 0 && FilterMailList.Count == 0 ? Compose(nameof(FilterSenders), "Labels, MailLists or Senders must be specified!") : null;
                }

                return null;
            }
        }
#endif
    }

    public class TelegramModuleSettings: ValidatableSettings
    {
        [Comment("Telegram bot token string obtained upon Telegram bot creation")]
        [Required]
        public string Token { get; set; }
        [Comment("Optional proxy IP for Telegram bot")]
        public string ProxyAddress { get; set; }
        [Comment("Optional proxy Port for Telegram bot")]
        public int ProxyPort { get; set; }
        [Comment("Optional proxy Username for Telegram bot")]
        public string ProxyUsername { get; set; }
        [Comment("Optional proxy Password  for Telegram bot")]
        public string ProxyPassword { get; set; }

#if EDITOR
        [Required]
        public ObservableCollection<TelegramRelay> RelayChannels { get; set; } = new ObservableCollection<TelegramRelay>();
#else
        public List<TelegramRelay> RelayChannels { get; set; } = new List<TelegramRelay>();
#endif
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Token):
                        return string.IsNullOrEmpty(Token)? Compose(nameof(Token), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class TelegramRelay: ValidatableSettings
    {
        [Comment("Telegram channel numeric ID")]
        [Required]
        public long Telegram { get; set; }
        [Comment("Discord channel numeric ID")]
        [Required]
        public ulong Discord { get; set; }
        [Comment("Relay only ThunderED bot messages from specified Discord channel")]
        public bool RelayFromDiscordBotOnly { get; set; }
#if EDITOR
        [Comment("Discord messages that contain these strings will be filtered from relay")]
        public ObservableCollection<string> DiscordFilters { get; set; } = new ObservableCollection<string>();
        [Comment("Discord messages that start with these strings will be filtered from relay")]
        public ObservableCollection<string> DiscordFiltersStartsWith { get; set; } = new ObservableCollection<string>();
        [Comment("Telegram messages that contain these strings will be filtered from relay")]
        public ObservableCollection<string> TelegramFilters { get; set; } = new ObservableCollection<string>();
        [Comment("Telegram messages that start with these strings will be filtered from relay")]
        public ObservableCollection<string> TelegramFiltersStartsWith { get; set; } = new ObservableCollection<string>();
        [Comment("Relay messages only from specified Telegram users. \nFirst check for Telegram @username then by First Name + Second Name")]
        public ObservableCollection<string> TelegramUsers { get; set; } = new ObservableCollection<string>();
        //[Comment("Relay messages only from specified Telegram nicknames. \nFirst check for Telegram @username then by First Name + Second Name")]
       // public ObservableCollection<string> TelegramNicks { get; set; } = new ObservableCollection<string>();
#else
        public List<string> DiscordFilters { get; set; } = new List<string>();
        public List<string> DiscordFiltersStartsWith { get; set; } = new List<string>();
        public List<string> TelegramFilters { get; set; } = new List<string>();
        public List<string> TelegramFiltersStartsWith { get; set; } = new List<string>();
        public List<string> TelegramUsers { get; set; } = new List<string>();
       // public List<string> TelegramNicks { get; set; } = new List<string>();
#endif
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Telegram):
                        return Telegram == 0 ? Compose(nameof(Telegram), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(Discord):
                        return Discord == 0 ? Compose(nameof(Discord), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class NullCampaignModuleSettings: ValidatableSettings
    {
        public int CheckIntervalInMinutes { get; set; } = 1;
#if EDITOR
        public ObservableDictionary<string, NullCampaignGroup> Groups { get; set; } = new ObservableDictionary<string, NullCampaignGroup>(); 
#else
        public Dictionary<string, NullCampaignGroup> Groups { get; set; } = new Dictionary<string, NullCampaignGroup>(); 
#endif
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(CheckIntervalInMinutes):
                        return CheckIntervalInMinutes < 1 ? Compose(nameof(CheckIntervalInMinutes), "Value must be greater than 0 or the bot will blow up!") : null;
                    case nameof(Groups):
                        return Groups.Count == 0 ? Compose(nameof(Groups), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class NullCampaignGroup: ValidatableSettings
    {
#if EDITOR
        public ObservableCollection<long> Regions { get; set; } = new ObservableCollection<long>();
        public ObservableCollection<long> Constellations { get; set; } = new ObservableCollection<long>();
        [Comment("List of time marks in minutes before the event starts to send notifications. E.g. 15, 30 - will send notifications when 15 and 30 minutes left for event start.")]
        public ObservableCollection<int> Announces { get; set; } = new ObservableCollection<int>();
        [Comment("The list of Discord mentions to use for this notifications, default is @everyone")]
        public ObservableCollection<string> Mentions { get; set; } = new ObservableCollection<string>();
#else
        public List<long> Regions { get; set; } = new List<long>();
        public List<long> Constellations { get; set; } = new List<long>();
        public List<int> Announces { get; set; } = new List<int>();
        public List<string> Mentions { get; set; } = new List<string>();
#endif
        [Comment("Default mention to use for module notification messages")]
        public string DefaultMention { get; set; } = "@everyone";
        [Comment("Send notification message when new campaign has been discovered")]
        public bool ReportNewCampaign { get; set; } = true;
        [Comment("Discord numeric channel ID")]
        public ulong DiscordChannelId { get; set; }

#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(DiscordChannelId):
                        return DiscordChannelId == 0 ? Compose(nameof(DiscordChannelId), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(Regions):
                        return Regions.Count == 0 && Constellations.Count == 0? Compose(nameof(Regions), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class NotificationFeedSettings: ValidatableSettings
    {
        [Comment("Time interval in minutes to run new notifications check. \nDue to natural delay in notifications on CCP side it is not wise to set it lower than 2 minutes")]
        public int CheckIntervalInMinutes { get; set; } = 2;
#if EDITOR
        [Comment("The list of character keys which will be authorized to share their notifications")]
        [Required]
        public ObservableDictionary<string, NotificationSettingsGroup> Groups { get; set; } = new ObservableDictionary<string, NotificationSettingsGroup>();
#else
        public Dictionary<string, NotificationSettingsGroup> Groups { get; set; } = new Dictionary<string, NotificationSettingsGroup>();
#endif
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(CheckIntervalInMinutes):
                        return CheckIntervalInMinutes < 1 ? Compose(nameof(CheckIntervalInMinutes), "Value must be greater than 0 or the bot will blow up!") : null;
                    case nameof(Groups):
                        return Groups.Count == 0 ? Compose(nameof(Groups), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class NotificationSettingsGroup: ValidatableSettings
    {
        [Comment("Numeric EVE character ID")]
        [Required]
        public long CharacterID { get; set; }
        [Comment("Numeric default Discord channel ID. All notification filters will use this channel to send messages by default")]
        [Required]
        public ulong DefaultDiscordChannelID { get; set; }
        [Comment("Numeric number of days to fetch old notifications for newly registered feeder. \n0 by default meaning no old notifications will be feeded")]
        public int FetchLastNotifDays { get; set; }

#if EDITOR
        [Comment("The list of filters to sort incoming notifications")]
        [Required]
        public ObservableDictionary<string, NotificationSettingsFilter> Filters { get; set; } = new ObservableDictionary<string, NotificationSettingsFilter>();
#else
        public Dictionary<string, NotificationSettingsFilter> Filters { get; set; } = new Dictionary<string, NotificationSettingsFilter>();
#endif
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Filters):
                        return Filters.Count == 0 ? Compose(nameof(Filters), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(CharacterID):
                        return CharacterID == 0 ? Compose(nameof(CharacterID), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(DefaultDiscordChannelID):
                        return DefaultDiscordChannelID == 0 ? Compose(nameof(DefaultDiscordChannelID), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class NotificationSettingsFilter: ValidatableSettings
    {
        [Comment("Numeric Discord channel ID to redirect messages. Leave 0 to use group default channel")]
        public ulong ChannelID { get; set; }
        [Comment("Default Discord mention command to use for this group")]
        public string DefaultMention { get; set; } = "@everyone";
#if EDITOR
        [Comment("List of text notification types this filter has access to")]
        [Required]
        public ObservableCollection<string> Notifications { get; set; } = new ObservableCollection<string>();
        [Comment("List of numeric EVE CHARACTER IDs to mention them in the message. Characters must be authed on the server for this to work \nthus allowing to get their Discord IDs. Leave empty to use **@everyone** mention")]
        public ObservableCollection<long> CharMentions { get; set; } = new ObservableCollection<long>();
        [Comment("List of Discord role names to mention. Role must be configured in Discord to be mentionable")]
        public ObservableCollection<string> RoleMentions { get; set; } = new ObservableCollection<string>();
#else
        public List<string> Notifications { get; set; } = new List<string>();
        public List<long> CharMentions { get; set; } = new List<long>();
        public List<string> RoleMentions { get; set; } = new List<string>();
#endif
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Notifications):
                        return Notifications.Count == 0 ? Compose(nameof(Notifications), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class IRCModuleSettings: ValidatableSettings
    {
        [Comment("IRC server address")]
        [Required]
        public string Server { get; set; } = "chat.freenode.net";
        [Comment("IRC server port")]
        [Required]
        public int Port { get; set; } = 6667;
        public bool UseSSL { get; set; } = false;
        public string Password { get; set; }
        [Required]
        public string Nickname { get; set; } = "DefaultUser-TH";
        [Comment("Secondary IRC nickname in case the first one is in use")]
        public string Nickname2 { get; set; }
        [Required]
        public string Username { get; set; } = "username";
        public string Realname { get; set; } = "realname";
        public bool Invisible { get; set; } = true;
        public bool AutoReconnect { get; set; } = true;
        public int AutoReconnectDelay { get; set; } = 5000;
        public bool AutoRejoinOnKick { get; set; }
        public string QuitReason { get; set; } = "Leaving";
        public bool SuppressMOTD { get; set; } = false;
        public bool SuppressPing { get; set; } = false;
#if EDITOR
        [Comment("The list of IRC commands to perform upon successful chat login")]
        public ObservableCollection<string> ConnectCommands { get; set; } = new ObservableCollection<string>();
        [Comment("Groups of settings for message relay")]
        public ObservableCollection<IRCRelayItem> RelayChannels { get; set; } = new ObservableCollection<IRCRelayItem>();
#else
        public List<string> ConnectCommands { get; set; } = new List<string>();
        public List<IRCRelayItem> RelayChannels { get; set; } = new List<IRCRelayItem>();
#endif
        public bool AutoJoinWaitIdentify { get; set; }   
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(RelayChannels):
                        return RelayChannels.Count == 0 ? Compose(nameof(RelayChannels), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(Server):
                        return string.IsNullOrEmpty(Server) ? Compose(nameof(Server), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(Port):
                        return Port == 0 ? Compose(nameof(Port), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(Nickname):
                        return string.IsNullOrEmpty(Nickname) ? Compose(nameof(Nickname), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(Username):
                        return string.IsNullOrEmpty(Username) ? Compose(nameof(Username), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class IRCRelayItem: ValidatableSettings
    {
        [Comment("IRC channel name string prefixed with #")]
        [Required]
        public string IRC { get; set; }
        [Comment("Discord numeric channel ID")]
        [Required]
        public ulong Discord { get; set; }
        [Comment("Relay only ThunderED bot messages from specified Discord channel")]
        public bool RelayFromDiscordBotOnly { get; set; }
#if EDITOR
        [Comment("Discord messages that contain these strings will be filtered from relay")]
        public ObservableCollection<string> DiscordFilters { get; set; } = new ObservableCollection<string>();
        [Comment("Discord messages that start with these strings will be filtered from relay")]
        public ObservableCollection<string> DiscordFiltersStartsWith { get; set; } = new ObservableCollection<string>();
        [Comment("IRC messages that contain these strings will be filtered from relay")]
        public ObservableCollection<string> IRCFilters { get; set; } = new ObservableCollection<string>();
        [Comment("IRC messages that start with these strings will be filtered from relay")]
        public ObservableCollection<string> IRCFiltersStartsWith { get; set; } = new ObservableCollection<string>();
        [Comment("Relay messages only from specified IRC usernames")]
        public ObservableCollection<string> IRCUsers { get; set; } = new ObservableCollection<string>();
#else
        public List<string> DiscordFilters { get; set; } = new List<string>();
        public List<string> DiscordFiltersStartsWith { get; set; } = new List<string>();
        public List<string> IRCFilters { get; set; } = new List<string>();
        public List<string> IRCFiltersStartsWith { get; set; } = new List<string>();
        public List<string> IRCUsers { get; set; } = new List<string>();
#endif
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Discord):
                        return Discord == 0 ? Compose(nameof(Discord), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(IRC):
                        return string.IsNullOrEmpty(IRC) ? Compose(nameof(IRC), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class IncursionNotificationModuleSettings: ValidatableSettings
    {
        [Comment("Numeric Discord channel ID")]
        [Required]
        public ulong DiscordChannelId { get; set; }
        [Comment("Set to **True** if you want bot to post status update about existing incursions. \nSet to **False** to report only new incursions")]
        public bool ReportIncursionStatusAfterDT { get; set; }
        [Comment("Optional default mention for Incursions report. Default: @everyone")]
        public string DefaultMention { get; set; } = "@everyone";
#if EDITOR
        [Comment("List of numeric region IDs to filter incursions")]
        public ObservableCollection<long> Regions { get; set; } = new ObservableCollection<long>();
        [Comment("List of numeric constellation IDs to filter incursions")]
        public ObservableCollection<long> Constellations { get; set; } = new ObservableCollection<long>();
#else
        public List<long> Regions { get; set; } = new List<long>();
        public List<long> Constellations { get; set; } = new List<long>();
#endif
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(DiscordChannelId):
                        return DiscordChannelId == 0 ? Compose(nameof(DiscordChannelId), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(Regions):
                        return Regions.Count == 0 && Constellations.Count == 0 ? Compose(nameof(Regions), "Regions or Constellations must be filled!") : null;
                }

                return null;
            }
        }
#endif
    }

    public class ChatRelayChannel: ValidatableSettings
    {
        [Comment("Name of the EVE chat channel")]
        [Required]
        public string EVEChannelName { get; set; }
        [Comment("Numeric Discord channel ID")]
        [Required]
        public ulong DiscordChannelId { get; set; }
        [Comment("Unique string code to identify this relay block. \nYou have to specify this code in TED_ChatRelay app settings too")]
        [Required]
        public string Code { get; set; }
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(DiscordChannelId):
                        return DiscordChannelId == 0 ? Compose(nameof(DiscordChannelId), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(EVEChannelName):
                        return string.IsNullOrEmpty(EVEChannelName) ? Compose(nameof(EVEChannelName), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(Code):
                        return string.IsNullOrEmpty(Code) ? Compose(nameof(Code), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class ChatRelayModuleSettings: ValidatableSettings
    {
#if EDITOR
        public ObservableCollection<ChatRelayChannel> RelayChannels {get; set; } = new ObservableCollection<ChatRelayChannel>();
#else
        public List<ChatRelayChannel> RelayChannels {get; set; } = new List<ChatRelayChannel>();
#endif
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(RelayChannels):
                        return RelayChannels.Count == 0 ? Compose(nameof(RelayChannels), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class ConfigSettings: ValidatableSettings
    {
        [Comment("Discord bot token value")]
        [Required]
        public string BotDiscordToken { get; set; }
        [Comment("The name of the bot to display in Discord")]
        [Required]
        public string BotDiscordName { get; set; }
        [Comment("This string will be displayed in Discord under the bots name")]
        public string BotDiscordGame { get; set; }
        [Comment("Single symbol which will represent bot command")]
        [Required]
        public string BotDiscordCommandPrefix { get; set; } = "!";
        [Comment("Numeric ID value of your Discord group (guild)")]
        [Required]
        public ulong DiscordGuildId { get; set; }
#if EDITOR
        [Required]
        public ObservableCollection<string> DiscordAdminRoles { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<ulong> ComForbiddenChannels { get; set; } = new ObservableCollection<ulong>();
#else
        [Comment("At least one role name from Discord with admin privilegies")]
        [Required]
        public List<string> DiscordAdminRoles { get; set; } = new List<string>();
        [Comment("The list of numeric channel IDs in which bot commands will be ignored")]
        public List<ulong> ComForbiddenChannels { get; set; } = new List<ulong>();
#endif
        [Comment("Interface and message language. Note that console text and logs will always be in english. You can add your own translation files in Languages directory.")]
        public string Language { get; set; } = "en-US";
        [Comment("Specifies if queries and results from ESI should be received only in english or using the language settings")]
        public bool UseEnglishESIOnly { get; set; } = true;

        public bool ModuleWebServer { get; set; } = false;
        [Comment("Enable auth check module which checks all authenticated users and strips roles if user has left your corp or alliance")]
        public bool ModuleAuthCheck { get; set; } = false;
        public bool ModuleAuthWeb { get; set; } = false;
        [Comment("Enable char and corp query module (!char and !corp commands)")]
        public bool ModuleCharCorp { get; set; } = false;
        public bool ModuleLiveKillFeed { get; set; } = false;
        public bool ModuleRadiusKillFeed { get; set; } = false;
       // public bool ModuleReliableKillFeed { get; set; } = false;
        [Comment("Enable price check module (!pc, !jita etc. commands)")]
        public bool ModulePriceCheck { get; set; } = false;
        [Comment("Enable EVE time module (!time command)")]
        public bool ModuleTime { get; set; } = false;
        public bool ModuleFleetup { get; set; } = false;
        public bool ModuleJabber { get; set; } = false;
        public bool ModuleMOTD { get; set; } = false;
        public bool ModuleNotificationFeed { get; set; } = false;
        public bool ModuleStats { get; set; } = false;
        public bool ModuleTimers { get; set; } = false;
        public bool ModuleMail { get; set; } = false;
        public bool ModuleIRC { get; set; } = false;
        public bool ModuleTelegram { get; set; } = false;
        public bool ModuleChatRelay { get; set; } = false;
        public bool ModuleIncursionNotify { get; set; } = false;
        public bool ModuleNullsecCampaign { get; set; } = false;
        public bool ModuleFWStats { get; set; } = true;
        public bool ModuleLPStock { get; set; } = true;
        public bool ModuleHRM { get; set; } = false;
        public bool ModuleSystemLogFeeder { get; set; } = false;
        public bool ModuleContractNotifications { get; set; } = false;

        [Comment("Optional ZKill RedisQ queue name to fetch kills from. Could be any text value but make sure it is not simple and is quite unique")]
        public string ZkillLiveFeedRedisqID { get; set; }
        public string TimeFormat { get; set; } = "dd.MM.yyyy HH:mm:ss";
        public string ShortTimeFormat { get; set; } = "dd.MM.yyyy HH:mm";
        [Comment("Display welcome message with authentication offer to all new users joining your Discord group hallway")]
        public bool WelcomeMessage { get; set; } = true;
        [Comment("Welcome message Discord channel ID")]
        public ulong WelcomeMessageChannelId { get; set; }
        [Comment("Time interval in minutes to purge all outdated cache")]
        public int CachePurgeInterval { get; set; } = 30;
        [Comment("Memory usage limit in Mb. If app reaches that limit it will try to free some memory")]
        public int MemoryUsageLimitMb { get; set; } = 100;
        [Comment("Log all the app messages by specified severity and above (Values: Info, Error, Critical)")]
        public string LogSeverity { get; set; } = "Info";
        [Comment("FALSE by default. Set to TRUE if you want to log all raw notifications data the bot will fetch. This is needed to catch notifications which the bot could not yet process. Send me acquired data to add notifications you will like to be processed by the bot")]
        public bool LogNewNotifications { get; set; } = true;
        [Comment("Database provider. Default value is 'sqlite'")]
        public string DatabaseProvider { get; set; } = "sqlite";
        [Comment("Number of web-request retries before treating it as failed")]
        public int RequestRetries { get; set; } = 3;
        [Comment("The path to a database file. Default value is 'edb.db'")]
        [Required]
        public string DatabaseFile { get; set; } = "edb.db";


#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(BotDiscordToken):
                        return string.IsNullOrEmpty(BotDiscordToken) ? Compose(nameof(BotDiscordToken), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(BotDiscordName):
                        return string.IsNullOrEmpty(BotDiscordName) ? Compose(nameof(BotDiscordName), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(BotDiscordGame):
                        return string.IsNullOrEmpty(BotDiscordGame) ? Compose(nameof(BotDiscordGame), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(BotDiscordCommandPrefix):
                        return string.IsNullOrEmpty(BotDiscordCommandPrefix) ? Compose(nameof(BotDiscordCommandPrefix), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(DiscordGuildId):
                        return DiscordGuildId == 0 ? Compose(nameof(DiscordGuildId), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(DiscordAdminRoles):
                        return DiscordAdminRoles.Count == 0 ? Compose(nameof(DiscordAdminRoles), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(DatabaseFile):
                        return string.IsNullOrEmpty(DatabaseFile) ? Compose(nameof(DatabaseFile), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class SystemLogFeederSettings : ValidatableSettings
    {
        [Comment("Discord channel ID")]
        [Required]
        public ulong DiscordChannelId { get; set; }

        [Comment("Feed only critical and error messages")]
        public bool OnlyErrors { get; set; }

#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(DiscordChannelId):
                        return DiscordChannelId == 0 ? Compose(nameof(DiscordChannelId), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }
                return null;
            }
        }
#endif
    }

    public class WebServerModuleSettings: ValidatableSettings
    {
        [Comment("Text IP address or domain name which the bot will use to listen for connections. \nIf the machine the bot running on have direct access to the internet then it should be equal\n to **webExternalIP** overwise it is the intrAnet address of your machine")]
        [Required]
        public string WebListenIP { get; set; }
        [Comment("Numeric port value")]
        [Required]
        public int WebListenPort { get; set; }
        [Comment("Text IP address or domain name which is used to receive connections from the internet")]
        [Required]
        public string WebExternalIP { get; set; }
        [Comment("Numeric port value")]
        [Required]
        public int WebExternalPort { get; set; }
        [Comment("Discord group invitation url")]
        public string DiscordUrl { get; set; }
        [Comment("Text client ID from the CCP application")]
        [Required]
        public string CcpAppClientId { get; set; }
        [Comment("Text client code from the CCP application")]
        [Required]
        public string CcpAppSecret { get; set; }

#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(WebListenIP):
                        return string.IsNullOrEmpty(WebListenIP) ? Compose(nameof(WebListenIP), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(WebListenPort):
                        return WebListenPort == 0 ? Compose(nameof(WebListenPort), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(WebExternalIP):
                        return string.IsNullOrEmpty(WebExternalIP) ? Compose(nameof(WebExternalIP), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(WebExternalPort):
                        return WebExternalPort == 0 ? Compose(nameof(WebExternalPort), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(CcpAppClientId):
                        return string.IsNullOrEmpty(CcpAppClientId) ? Compose(nameof(CcpAppClientId), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(CcpAppSecret):
                        return string.IsNullOrEmpty(CcpAppSecret) ? Compose(nameof(CcpAppSecret), Extensions.ERR_MSG_VALUEEMPTY) : null;
                }

                return null;
            }
        }
#endif
    }

    public class WebAuthModuleSettings: ValidatableSettings
    {
        [Comment("Numeric time interval in minutes to run auth checks of existing users")]
        [Required]
        public int AuthCheckIntervalMinutes { get; set; } = 30;
        [Comment("Numeric ID of the Discord channel to report bot auth actions. Preferably for admins only. Leave 0 to disable")]
        public ulong AuthReportChannel { get; set; }
        [Comment("Automatically assign corp tickers to users")]
        public bool EnforceCorpTickers { get; set; }
        [Comment("Automatically assign character names to users (setup Discord group to disallow name change also)")]
        public bool EnforceCharName { get; set; }
        [Comment("Default group to use for auth url display")]
        public string DefaultAuthGroup { get; set; }

#if EDITOR
        [Comment("The list of Discord role names which will not be checked for authentication (admins etc.)")]
        public ObservableCollection<string> ExemptDiscordRoles { get; set; } = new ObservableCollection<string>();
        [Comment("The list of Discord roles which will not be stripped if character is authed. This will allow you to add custom roles manually.")]
        public ObservableCollection<string> AuthCheckIgnoreRoles { get; set; } = new ObservableCollection<string>();
        [Comment("The list of channels where !auth command is allowed")]
        [Required]
        public ObservableCollection<ulong> ComAuthChannels { get; set; } = new ObservableCollection<ulong>();
        [Comment("The list of groups to filter auth requests")]
        [Required]
        public ObservableDictionary<string, WebAuthGroup> AuthGroups { get; set; } = new ObservableDictionary<string, WebAuthGroup>();        
#else
        public List<string> ExemptDiscordRoles { get; set; } = new List<string>();
        public List<string> AuthCheckIgnoreRoles { get; set; } = new List<string>();
        public List<ulong> ComAuthChannels { get; set; } = new List<ulong>();
        public Dictionary<string, WebAuthGroup> AuthGroups { get; set; } = new Dictionary<string, WebAuthGroup>();
#endif
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(AuthCheckIntervalMinutes):
                        return AuthCheckIntervalMinutes < 1 ? Compose(nameof(AuthCheckIntervalMinutes), "Value must be greater than 0!") : null;
                    case nameof(ComAuthChannels):
                        return ComAuthChannels.Count == 0 ? Compose(nameof(ComAuthChannels), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(AuthGroups):
                        return AuthGroups.Count == 0 ? Compose(nameof(AuthGroups), Extensions.ERR_MSG_VALUEEMPTY) : null;
                    case nameof(DefaultAuthGroup):
                        return !string.IsNullOrEmpty(DefaultAuthGroup) && AuthGroups.All(a => a.Key != DefaultAuthGroup) ? Compose(nameof(DefaultAuthGroup), "DefaultAuthGroup do not equal to any of the AuthGroups names!") : null;
                }

                return null;
            }
        }
#endif
    }

    public class WebAuthGroup: ValidatableSettings
    {
#if EDITOR
        [Comment("Dictionary containing text corporation ID and the list of Discord roles which to assign on successful auth. Additive with AllowedAlliances.")]
       // [Required]
        public ObservableDictionary<string, AuthRoleEntity> AllowedCorporations { get; set; } = new ObservableDictionary<string, AuthRoleEntity>();
        [Comment("Dictionary containing text alliance ID and the list of Discord roles which to assign on successful auth.  Additive with AllowedCorporations.")]
       // [Required]
        public ObservableDictionary<string, AuthRoleEntity> AllowedAlliances { get; set; } = new ObservableDictionary<string, AuthRoleEntity>();

        //[Comment("The list of exact Discord role names to assign")]
       // [Required]
       // public ObservableCollection<string> MemberRoles { get; set; } = new ObservableCollection<string>();
        [Comment("The list of exact Discord role names authorized to manually approve applicants")]
        public ObservableCollection<string> AuthRoles { get; set; } = new ObservableCollection<string>();
        //[Comment("Numeric alliance ID list. Checked ADDITIVELY after the corpIDList")]
        //[Required]
        //public ObservableCollection<int> AllianceIDList { get; set; } = new ObservableCollection<int>();
        //[Comment("Numeric corporation ID list. You can leave it empty if you want to filter by entire alliance.")]
        //[Required]
        //public ObservableCollection<int> CorpIDList { get; set; } = new ObservableCollection<int>();
        
        [Comment("Text Discord roles list which can be added manually to a user and will persist while he passes auth check for this group. Stripped when he is not in a specified corp/ally anymore.")]
        public ObservableCollection<string> ManualAssignmentRoles { get; set; } = new ObservableCollection<string>();

#else
       // public List<string> MemberRoles { get; set; } = new List<string>();
        public List<string> AuthRoles { get; set; } = new List<string>();
        //public List<int> CorpIDList { get; set; } = new List<int>();
       // public List<int> AllianceIDList { get; set; } = new List<int>();
        public Dictionary<string, AuthRoleEntity> AllowedCorporations { get; set; } = new Dictionary<string, AuthRoleEntity>();
        public Dictionary<string, AuthRoleEntity> AllowedAlliances { get; set; } = new Dictionary<string, AuthRoleEntity>();
        public List<string> ManualAssignmentRoles { get; set; } = new List<string>();
#endif
        [Comment("Enable auth to require manual acceptance from authorized members")]
        public bool PreliminaryAuthMode { get; set; }

        [Comment("Enable automatic applications invalidation and cleanup in specified amount of hours")]
        public int AppInvalidationInHours { get; set; } = 48;

        [Comment("Optional text to display in a web-server group auth button if separate button is generated for this group")]
        public string CustomButtonText { get; set; }
        [Comment("Optional default Discord mention for auth report")]
        public string DefaultMention { get; set; }

#if EDITOR
        [Comment("The list of ESI access role names to check on auth")]
        public ObservableCollection<string> ESICustomAuthRoles { get; set; } = new ObservableCollection<string>();
#else
        public List<string> ESICustomAuthRoles { get; set; } = new List<string>();
#endif
#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(PreliminaryAuthMode):
                        return ESICustomAuthRoles.Count == 0 ? Compose(nameof(ESICustomAuthRoles), "ESICustomAuthRoles must contain at least one value when PreliminaryAuthMode is true!") : null;
                    case nameof(ESICustomAuthRoles):
                        var wrong = ESICustomAuthRoles.Where(a => !SettingsManager.ESIScopes.Contains(a)).ToList();
                        return wrong.Count > 0 ? Compose(nameof(ESICustomAuthRoles), $"ESICustomAuthRoles contains unidentified ESI scopes: {string.Join(", ", wrong)}") : null;
                }

                return null;
            }
        }
#endif
    }

    public class AuthRoleEntity: ValidatableSettings
    {
        [Comment("Id of the entity (corp or alliance)")]
        [Required]
        public long Id { get; set; }
        
        [Comment("The list of Discord role names to assign after successful auth")]
#if EDITOR
        public ObservableCollection<string> DiscordRoles { get; set; } = new ObservableCollection<string>();
#else
        public List<string> DiscordRoles { get; set; } = new List<string>();
#endif

#if EDITOR
        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Id):
                        return Id <= 0 ? Compose(nameof(Id), "Id must be greater than zero!") : null;
                }

                return null;
            }
        }
#endif
    }
    
}
