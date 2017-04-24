using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oxide;
using Oxide.Plugins;
using UnityEngine;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("BanSystem", "PsychoTea", "1.0.0")]

    class BanSystem : RustPlugin
    {
        const string permAdmin = "bansystem.admin";
        const string permBan = "bansystem.ban";
        const string permConfig = "bansystem.config";
        const string permShowBans = "bansystem.showbans";
        const string permShowBansCmd = "bansystem.showbanscmd";

        readonly string[] validTimeFields = new string[] { "s", "m", "h", "d", "w", "mo", "y" };

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
                { "ChatPrefix", "<color=#ff3333>BanSystem: </color>" },
                { "NoPlayersFound", "Error! {0} players were found with the name or ID {1}." },

                { "IncorrectUsage", "Incorrect usage! /bs {ban/kick/mirror/show/config/banlist/remotebanlist}" },
                { "IncorrectUsage-Ban", "Incorrect usage! /bs ban {name/userid} [time (s/m/h/d/w/mo/y)]" },
                { "IncorrectUsage-Ban-Time", "Incorrect time! /bs ban {name/userid} {30s/10m/3h/1d/2w/4mo/1y}" }
            }, this, "en");
        }

        #endregion

        #region Config

        private ConfigFile config;

        public class ConfigFile
        {
            [JsonProperty(PropertyName = "Check Prefix Enabled")]
            public bool ChatPrefixEnabled;
            
            public static ConfigFile DefaultConfig()
            {
                return new ConfigFile
                {
                    ChatPrefixEnabled = true
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

        #region Chat Commands

        [ChatCommand("bs")]
        void BSCommand(BasePlayer player, string command, string[] args)
        {
            if (args.Length < 1)
            {
                SendReply(player, GetMessage("IncorrectUsage", player));
                return;
            }

            if (args[0] == "ban")
            {
                if (!HasPermAdmin(player) && !HasPermRaw(player, permBan))
                {
                    SendReply(player, GetMessage("NoPermission", player));
                    return;
                }

                if (args.Length < 2)
                {
                    SendReply(player, GetMessage("IncorrectUsage-Ban", player));
                    return;
                }

                int seconds = -1;

                int count;
                BasePlayer targetPlayer = FindPlayer(args[1], out count);
                if (count != 1 || targetPlayer == null)
                {
                    SendReply(player, GetMessage("NoPlayersFound", player, count.ToString(), args[1]));
                    return;
                }

                if (args.Length >= 3)
                {
                    seconds = TimeToSeconds(args[3]);
                    if (seconds == -1)
                    {
                        SendReply(player, GetMessage("IncorrectUsage-Ban-Time", player));
                        return;
                    }
                }

                if (seconds != -1)
                {
                    //Tempban
                    return;
                }

                //Permaban

                return;
            }

            SendReply(player, GetMessage("IncorrectUsage", player));
        }

        #endregion

        #region Functions

        #region Banning

        //if runner is null, assume it's console/RCON

        void Permaban(BasePlayer target, BasePlayer runner)
        {

        }

        void Permaban(ulong targetid, BasePlayer runner)
        {

        }

        void Tempban(BasePlayer target, BasePlayer runner)
        {

        }

        void Tempban(ulong targetid, BasePlayer runner)
        {

        }

        #endregion

        BasePlayer FindPlayer(string input, out int count)
        {
            count = 1;
            var bplSearch = BasePlayer.activePlayerList.Where(x => x.displayName.Contains(input));
            if (bplSearch.Count() > 1)
            {
                count = bplSearch.Count();
                return null;
            }
            else if (bplSearch.Count() == 1)
            {
                return bplSearch.FirstOrDefault();
            }

            var idSearch = BasePlayer.activePlayerList.Where(x => x.UserIDString == input);
            if (idSearch.Count() == 1) return idSearch.FirstOrDefault();

            var sleeperSearch = BasePlayer.sleepingPlayerList.Where(x => x.displayName.Contains(input));
            if (sleeperSearch.Count() > 1)
            {
                count = sleeperSearch.Count();
                return null;
            }
            else if (sleeperSearch.Count() == 1)
            {
                return sleeperSearch.FirstOrDefault();
            }

            var sleeperIdSearch = BasePlayer.sleepingPlayerList.Where(x => x.UserIDString == input);
            if (sleeperIdSearch.Count() > 1)
            {
                count = sleeperIdSearch.Count();
                return null;
            }
            else if (sleeperIdSearch.Count() == 1)
            {
                return sleeperIdSearch.FirstOrDefault();
            }

            // TODO: Add support for offline players

            return null;
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

        bool HasPermAdmin(BasePlayer player) => (player.IsAdmin || HasPermRaw(player, permAdmin));
        bool HasPermRaw(BasePlayer player, string perm) => permission.UserHasPermission(player.UserIDString, perm);

        string GetMessage(string key, BasePlayer player, params string[] args) => (config.ChatPrefixEnabled) ? GetMessageRaw("ChatPrefix", player) : "" + GetMessageRaw(key, player, args);
        string GetMessageRaw(string key, BasePlayer player, params string[] args) => string.Format(lang.GetMessage(key, this, player.UserIDString), args);

        #endregion
    }
}
