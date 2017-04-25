using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oxide;
using Oxide.Plugins;
using UnityEngine;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;

namespace Oxide.Plugins
{
    [Info("BanSystem", "PsychoTea&Spicy", "1.0.0")]

    class BanSystem : CovalencePlugin
    {
        const string permAdmin = "bansystem.admin";
        const string permBan = "bansystem.ban";
        const string permTempban = "bansystem.tempban";
        const string permKick = "bansystem.kick";
        const string permConfig = "bansystem.config";
        const string permShowBans = "bansystem.showbans";
        const string permShowBansCmd = "bansystem.showbanscmd";

        readonly string[] validTimeFields = new string[] { "s", "m", "h", "d", "w", "mo", "y" };

        #region Classes

        class BanInfo
        {
            public string Name { get; private set; }
            public string Id { get; private set; }
            public string Reason { get; private set; }
            public bool IsTempban { get; private set; }
            public string BanLength { get; private set; }

            public static BanInfo PermaBan(string Name, string Id, string Reason)
            {
                BanInfo ban = new BanInfo();
                ban.Name = Name;
                ban.Id = Id;
                ban.Reason = Reason;
                ban.IsTempban = false;
                ban.BanLength = "";
                return ban;
            }

            public static BanInfo TempBan(string Name, string Id, string Reason, string BanLength)
            {
                BanInfo ban = new BanInfo();
                ban.Name = Name;
                ban.Id = Id;
                ban.Reason = Reason;
                ban.IsTempban = true;
                ban.BanLength = BanLength;
                return ban;
            }
        }

        #endregion

        #region Oxide Hooks

        void Init()
        {
            //Register our permissions
            permission.RegisterPermission(permAdmin, this);
            permission.RegisterPermission(permBan, this);
            permission.RegisterPermission(permConfig, this);
            permission.RegisterPermission(permShowBans, this);
            permission.RegisterPermission(permShowBansCmd, this);

            //Register our messages
            lang.RegisterMessages(new Dictionary<string, string>()
            {
                { "NoPermission", "You don't have permission to use this command." },
                { "ChatPrefix", "[#ff3333]BanSystem: [/#]" },
                { "NoPlayersFound", "Error! {0} players were found with the name or ID {1}." },

                { "IncorrectUsage", "Incorrect usage! /bs {ban/kick/mirror/show/config/banlist/remotebanlist}" },
                { "IncorrectUsage-Ban", "Incorrect usage! /bs ban {name/userid} {reason}" },
                { "IncorrectUsage-Tempban", "Incorrect usage! /bs tempban {name/userid} {30s/10m/3h/1d/2w/4mo/1y} {reason}" },
                { "IncorrectUsage-Kick", "Incorrect usage! /bs kick {name/userid} {reason}" },

                { "Ban-Permaban", "{0} was permabanned from the server by {1} for {2}." },
                { "Ban-Tempban", "{0} was tempbanned from the server by {1} for {2}, for {3}." },

                { "BanList", "Showing all bans:" }
            }, this, "en");

            ReadData();
        }

        #endregion

        #region Config

        private ConfigFile config;

        public class ConfigFile
        {
            [JsonProperty(PropertyName = "Check Prefix Enabled")]
            public bool ChatPrefixEnabled;
            
            [JsonProperty(PropertyName = "Print Bans To Chat")]
            public bool PrintBansToChat;

            [JsonProperty(PropertyName = "Print Tempbans To Chat")]
            public bool PrintTempbansToChat;

            [JsonProperty(PropertyName = "Print Kicks To Chat")]
            public bool PrintKicksToChat;

            public static ConfigFile DefaultConfig()
            {
                return new ConfigFile
                {
                    ChatPrefixEnabled = true,

                    PrintBansToChat = true,
                    PrintTempbansToChat = true,
                    PrintKicksToChat = true
                };
            }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            config = Config.ReadObject<ConfigFile>();
        }

        protected override void LoadDefaultConfig() => config = ConfigFile.DefaultConfig();

        protected override void SaveConfig() => Config.WriteObject(config);

        #endregion

        #region Data

        class StoredData
        {
            public List<BanInfo> Bans = new List<BanInfo>();
        }
        StoredData storedData;

        void SaveData() => Interface.Oxide.DataFileSystem.WriteObject<StoredData>(this.Title, storedData);
        void ReadData() => storedData = Interface.Oxide.DataFileSystem.ReadObject<StoredData>(this.Title);

        #endregion

        #region Chat Commands

        [Command("bs")]
        void BSCommand(IPlayer player, string command, string[] args)
        {
            if (args.Length < 1)
            {
                player.Message(GetMessage("IncorrectUsage", player));
                return;
            }

            if (args[0].ToLower() == "ban")
            {
                if (!HasPermAdmin(player) && !HasPermRaw(player, permBan))
                {
                    player.Message(GetMessage("NoPermission", player));
                    return;
                }

                if (args.Length < 3)
                {
                    player.Message(GetMessage("IncorrectUsage-Ban", player));
                    return;
                }

                int count;
                IPlayer targetPlayer = FindPlayer(args[1], out count);
                if (count != 1 || targetPlayer == null)
                {
                    player.Message(GetMessage("NoPlayersFound", player, count.ToString(), args[1]));
                    return;
                }

                string reason = string.Join(" ", args.Skip(2));

                Permaban(targetPlayer.Id, player, reason);

                return;
            }

            if (args[0].ToLower() == "tempban")
            {
                if (!HasPermAdmin(player) && !HasPermRaw(player, permTempban))
                {
                    player.Message(GetMessage("NoPermission", player));
                    return;
                }

                if (args.Length < 4)
                {
                    player.Message(GetMessage("IncorrectUsage-Tempban", player));
                    return;
                }

                int count;
                IPlayer targetPlayer = FindPlayer(args[1], out count);
                if (count != 1 || targetPlayer == null)
                {
                    player.Message(GetMessage("NoPlayersFound", player, count.ToString(), args[1]));
                    return;
                }

                string time = args[2];
                string reason = string.Join(" ", args.Skip(3));

                Tempban(targetPlayer.Id, player, reason, time);

                return;
            }

            if (args[0].ToLower() == "kick")
            {
                if (!HasPermAdmin(player) && !HasPermRaw(player, permKick))
                {
                    player.Message(GetMessage("NoPermission", player));
                    return;
                }

                if (args.Length < 3)
                {
                    player.Message(GetMessage("IncorrectUsage-Kick", player));
                    return;
                }

                int count;
                IPlayer targetPlayer = FindPlayer(args[1], out count);
                if (count != 1 | targetPlayer == null)
                {
                    player.Message(GetMessage("NoPlayersFound", player, count.ToString(), args[1]));
                    return;
                }

                string reason = string.Join(" ", args.Skip(2));

                Kick(targetPlayer, player, reason);

                return;
            }

            if (args[0].ToLower() == "banlist")
            {
                if (!HasPermAdmin(player) && !HasPermRaw(player, permShowBansCmd))
                {
                    player.Message(GetMessage("NoPermission", player));
                    return;
                }

                string message = GetMessage("BanList", player);
                foreach (var ban in storedData.Bans)
                {
                    string nameOrId = string.IsNullOrEmpty(ban.Name) ? ban.Id : $"{ban.Name} ({ban.Id})";
                    message += $"\n - {nameOrId}; Reason: {ban.Reason}" + ((ban.IsTempban) ? $"; Time: {ban.BanLength}" : "");
                }
                player.Message(message);

                return;
            }

            player.Message(GetMessage("IncorrectUsage", player));
        }

        #endregion

        #region Functions

        #region Banning

        void Permaban(string targetId, IPlayer runnerPlayer, string reason)
        {
            string playerName = TryFindName(targetId) ?? targetId.ToString();
            string runnerName = runnerPlayer?.Name ?? "SERVER";

            if (config.PrintBansToChat)
            {
                BroadcastToChat("Ban-Permaban", playerName, runnerName, reason);
            }

            BanInfo ban = BanInfo.PermaBan(TryFindName(targetId) ?? "", targetId, reason);
            storedData.Bans.Add(ban);
            SaveData();

            server.Ban(targetId.ToString(), reason);
        }

        void Tempban(string targetId, IPlayer runnerPlayer, string reason, string time)
        {
            string playerName = TryFindName(targetId) ?? targetId.ToString();
            string runnerName = runnerPlayer?.Name ?? "SERVER";
            TimeSpan timeSpan = TimeSpan.FromSeconds(TimeToSeconds(time));

            if (config.PrintTempbansToChat)
            {
                BroadcastToChat("Ban-Tempban", playerName, runnerName, reason, time);
            }

            BanInfo ban = BanInfo.TempBan(TryFindName(targetId) ?? "", targetId, reason, time);
            storedData.Bans.Add(ban);
            SaveData();

            server.Ban(targetId, reason, timeSpan);
        }

        void Kick(IPlayer targetPlayer, IPlayer runnerPlayer, string reason)
        {
            string runnerName = runnerPlayer?.Name ?? "SERVER";

            if (config.PrintKicksToChat)
            {
                BroadcastToChat("Kick", targetPlayer.Name, runnerName, reason);
            }

            targetPlayer.Kick(reason);
        }

        #endregion

        IPlayer FindPlayer(string input, out int count)
        {
            count = 1;
            var bplSearch = covalence.Players.All.Where(x => x.Name.Contains(input));
            if (bplSearch.Count() > 1)
            {
                count = bplSearch.Count();
                return null;
            }
            else if (bplSearch.Count() == 1)
            {
                return bplSearch.FirstOrDefault();
            }

            var idSearch = covalence.Players.All.Where(x => x.Id == input);
            if (idSearch.Count() == 1) return idSearch.FirstOrDefault();

            return null;
        }

        string TryFindName(string userId) => covalence.Players.All.Where(x => x.Id == userId).FirstOrDefault()?.Name;

        void BroadcastToChat(string key, params string[] args)
        {
            foreach (var player in covalence.Players.Connected)
                player.Message(covalence.FormatText(GetMessage(key, player, args)));
        }

        int TimeToSeconds(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length < 2) return -1;
            string timeField = input.Last().ToString();
            if (input.ToArray()[input.Length - 2] == 'm')
                timeField = "mo";
            if (!validTimeFields.Contains(timeField)) return -1;

            int inputTime;
            if (!Int32.TryParse(input.Replace(timeField, ""), out inputTime)) return -1;

            int seconds = 0;
            switch (timeField)
            {
                case "s":
                    seconds = inputTime * 1;
                    break;
                case "m":
                    seconds = inputTime * 60 * 1;
                    break;
                case "h":
                    seconds = inputTime * 60 * 60 * 1;
                    break;
                case "d":
                    seconds = inputTime * 24 * 60 * 60 * 1;
                    break;
                case "w":
                    seconds = inputTime * 7 * 24 * 60 * 60 * 1;
                    break;
                case "mo":
                    seconds = inputTime * 4 * 7 * 24 * 60 * 60 * 1;
                    break;
                case "y":
                    seconds = inputTime * 12 * 4 * 7 * 24 * 60 * 60 * 1;
                    break;
            }

            return seconds;
        }

        #endregion

        #region Helpers

        bool HasPermAdmin(IPlayer player) => (player.IsAdmin || HasPermRaw(player, permAdmin));
        bool HasPermRaw(IPlayer player, string perm) => permission.UserHasPermission(player.Id, perm);

        string GetMessage(string key, IPlayer player, params string[] args) => (config.ChatPrefixEnabled) ? GetMessageRaw("ChatPrefix", player) : "" + GetMessageRaw(key, player, args);
        string GetMessageRaw(string key, IPlayer player, params string[] args) => string.Format(lang.GetMessage(key, this, player.Id), args);

        #endregion
    }
}