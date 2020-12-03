using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OjamajoBot;
using OjamajoBot.Database;
using OjamajoBot.Database.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace Config
{
    public class Guild
    {
        public static void insertGuildData(ulong guildId)
        {
            try
            {
                DBC db = new DBC();
                Dictionary<string, object> columnFilter = new Dictionary<string, object>();
                columnFilter[DBM_Guild.Columns.id_guild] = guildId.ToString();
                db.insert(DBM_Guild.tableName, columnFilter);
            } catch(Exception e)
            {

            }
        }

        public static Dictionary<string,object> getGuildData(ulong guildId)
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();

            try
            {
                DBC db = new DBC();
                string query = $"SELECT * " +
                $" FROM {DBM_Guild.tableName} " +
                $" WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild}";

                Dictionary<string, object> colSelect = new Dictionary<string, object>();
                colSelect[DBM_Guild.Columns.id_guild] = guildId.ToString();

                DataTable dt = db.selectAll(query, colSelect);

                //create if not exists
                if (dt.Rows.Count <= 0)
                    insertGuildData(guildId);

                dt = db.selectAll(query, colSelect);

                foreach (DataRow row in dt.Rows)
                {
                    ret[DBM_Guild.Columns.id_guild] = row[DBM_Guild.Columns.id_guild];
                    ret[DBM_Guild.Columns.id_channel_birthday_announcement] = row[DBM_Guild.Columns.id_channel_birthday_announcement];
                    ret[DBM_Guild.Columns.id_channel_notification_chat_level_up] = row[DBM_Guild.Columns.id_channel_notification_chat_level_up];
                    ret[DBM_Guild.Columns.id_channel_notification_user_welcome] = row[DBM_Guild.Columns.id_channel_notification_user_welcome];
                    ret[DBM_Guild.Columns.id_channel_user_leaving_log] = row[DBM_Guild.Columns.id_channel_user_leaving_log];
                    ret[DBM_Guild.Columns.id_autorole_user_join] = row[DBM_Guild.Columns.id_autorole_user_join];
                    ret[DBM_Guild.Columns.welcome_message] = row[DBM_Guild.Columns.welcome_message];
                    ret[DBM_Guild.Columns.welcome_image] = row[DBM_Guild.Columns.welcome_image];
                    ret[DBM_Guild.Columns.birthday_announcement_date_last] = row[DBM_Guild.Columns.birthday_announcement_date_last];
                    ret[DBM_Guild.Columns.role_id_doremi] = row[DBM_Guild.Columns.role_id_doremi];
                    ret[DBM_Guild.Columns.role_id_hazuki] = row[DBM_Guild.Columns.role_id_hazuki];
                    ret[DBM_Guild.Columns.role_id_aiko] = row[DBM_Guild.Columns.role_id_aiko];
                    ret[DBM_Guild.Columns.role_id_onpu] = row[DBM_Guild.Columns.role_id_onpu];
                    ret[DBM_Guild.Columns.role_id_momoko] = row[DBM_Guild.Columns.role_id_momoko];
                    ret[DBM_Guild.Columns.role_detention] = row[DBM_Guild.Columns.role_detention];
                    ret[DBM_Guild.Columns.custom_prefix] = row[DBM_Guild.Columns.custom_prefix];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return ret;
        }

        public static void init(ulong guildId)
        {
            var guildData = getGuildData(guildId);
            //initialize custom prefix if exists
            if (guildData[DBM_Guild.Columns.custom_prefix].ToString() != "")
                Config.Core.customPrefix[guildId.ToString()] = guildData[DBM_Guild.Columns.custom_prefix].ToString();
            
            //check if directory exists
            if (!Directory.Exists($"attachments/{guildId.ToString()}"))
                Directory.CreateDirectory($"attachments/{guildId.ToString()}");
            if (!Directory.Exists($"attachments/{guildId.ToString()}/contribute"))
                Directory.CreateDirectory($"attachments/{guildId.ToString()}/contribute");//init contribute folder for submitted meme pictures
            //check if logs directory exists/not
            if (!Directory.Exists($"logs/{guildId.ToString()}"))
                Directory.CreateDirectory($"logs/{guildId.ToString()}");
            //check if config guild directory exists/not
            if (!Directory.Exists($"{Core.headConfigGuildFolder}{guildId.ToString()}"))
                Directory.CreateDirectory($"{Core.headConfigGuildFolder}{guildId.ToString()}");
            //check if garden core configuration folder exists/not
            //if (!Directory.Exists($"{Core.headConfigGuildFolder}{id_guild.ToString()}/{GardenCore.headUserConfigFolder}"))
            //    Directory.CreateDirectory($"{Core.headConfigGuildFolder}{id_guild.ToString()}/{GardenCore.headUserConfigFolder}");
            ////check trading cards configuration folder exists/not
            //if (!Directory.Exists($"{Core.headConfigGuildFolder}{id_guild.ToString()}/{Core.headTradingCardConfigFolder}"))
            //    Directory.CreateDirectory($"{Core.headConfigGuildFolder}{id_guild.ToString()}/{Core.headTradingCardConfigFolder}");
            ////check if patissier core configuration folder exists/not
            //if (!Directory.Exists($"{Core.headConfigGuildFolder}{id_guild.ToString()}/{PatissierCore.headUserConfigFolder}"))
            //    Directory.CreateDirectory($"{Core.headConfigGuildFolder}{id_guild.ToString()}/{PatissierCore.headUserConfigFolder}");
            ////check trading cards leaderboards file exists/not
            //if (!File.Exists($"{Config.Core.headConfigGuildFolder}{id_guild.ToString()}/trading_card_leaderboard_data.json"))
            //    File.Copy($@"{Config.Core.headConfigFolder}trading_card_leaderboard_template_data.json", $@"{Config.Core.headConfigGuildFolder}{id_guild.ToString()}/trading_card_leaderboard_data.json");

            //if (File.Exists($"{Core.headConfigGuildFolder}{id_guild}/{id_guild}.json")){
            //    JObject guildConfig = JObject.Parse(File.ReadAllText($"{Core.headConfigGuildFolder}{id_guild}/{id_guild}.json"));

            //    //id_random_event
            //    if (!guildConfig.ContainsKey("id_random_event"))
            //        guildConfig.Add(new JProperty("id_random_event", ""));

            //    //id_birthday_announcement
            //    if (!guildConfig.ContainsKey("id_birthday_announcement"))
            //        guildConfig.Add(new JProperty("id_birthday_announcement", ""));

            //    //leaving user message
            //    if (!guildConfig.ContainsKey("user_leaving_notification"))
            //        guildConfig.Add(new JProperty("user_leaving_notification", "1"));

            //    //user birthday
            //    if (!guildConfig.ContainsKey("user_birthday"))
            //        guildConfig.Add(new JProperty("user_birthday", new JObject()));

            //    //user birthday announcement
            //    if (!guildConfig.ContainsKey("birthday_announcement_date_last"))
            //        guildConfig.Add(new JProperty("birthday_announcement_date_last", ""));

            //    //doremi_role_id
            //    if (!guildConfig.ContainsKey("doremi_role_id"))
            //        guildConfig.Add(new JProperty("doremi_role_id", ""));

            //    //hazuki_role_id
            //    if (!guildConfig.ContainsKey("hazuki_role_id"))
            //        guildConfig.Add(new JProperty("hazuki_role_id", ""));

            //    //aiko_role_id
            //    if (!guildConfig.ContainsKey("aiko_role_id"))
            //        guildConfig.Add(new JProperty("aiko_role_id", ""));

            //    //onpu_role_id
            //    if (!guildConfig.ContainsKey("onpu_role_id"))
            //        guildConfig.Add(new JProperty("onpu_role_id", ""));

            //    //momoko_role_id
            //    if (!guildConfig.ContainsKey("momoko_role_id"))
            //        guildConfig.Add(new JProperty("momoko_role_id", ""));

            //    //trading_card_spawn channels
            //    if (!guildConfig.ContainsKey("trading_card_spawn"))
            //        guildConfig.Add(new JProperty("trading_card_spawn", ""));

            //    //trading_card_spawn_interval
            //    if (!guildConfig.ContainsKey("trading_card_spawn_interval"))
            //        guildConfig.Add(new JProperty("trading_card_spawn_interval", "40"));

            //    //trading_card_spawn_id
            //    if (!guildConfig.ContainsKey("trading_card_spawn_id"))
            //        guildConfig.Add(new JProperty("trading_card_spawn_id", ""));

            //    //trading_card_spawn_category
            //    if (!guildConfig.ContainsKey("trading_card_spawn_category"))
            //        guildConfig.Add(new JProperty("trading_card_spawn_category", ""));

            //    //trading_card_spawn_token
            //    if (!guildConfig.ContainsKey("trading_card_spawn_token"))
            //        guildConfig.Add(new JProperty("trading_card_spawn_token", ""));

            //    //trading_card_spawn_token_hour
            //    if (!guildConfig.ContainsKey("trading_card_spawn_token_time"))
            //        guildConfig.Add(new JProperty("trading_card_spawn_token_time", ""));

            //    //trading_card_spawn_mystery
            //    if (!guildConfig.ContainsKey("trading_card_spawn_mystery"))
            //        guildConfig.Add(new JProperty("trading_card_spawn_mystery", "0"));

            //    //id_card_catcher for card catcher roles
            //    if (!guildConfig.ContainsKey("id_card_catcher"))
            //        guildConfig.Add(new JProperty("id_card_catcher", ""));

            //    if (!guildConfig.ContainsKey(TradingCardCore.BadCards.propertyBadCard))
            //        guildConfig.Add(new JProperty(TradingCardCore.BadCards.propertyBadCard, ""));

            //    if (!guildConfig.ContainsKey(TradingCardCore.BadCards.propertyBadCardNumber1))
            //        guildConfig.Add(new JProperty(TradingCardCore.BadCards.propertyBadCardNumber1, ""));

            //    if (!guildConfig.ContainsKey(TradingCardCore.BadCards.propertyBadCardEquation))
            //        guildConfig.Add(new JProperty(TradingCardCore.BadCards.propertyBadCardEquation, ""));

            //    if (!guildConfig.ContainsKey(TradingCardCore.BadCards.propertyBadCardNumber2))
            //        guildConfig.Add(new JProperty(TradingCardCore.BadCards.propertyBadCardNumber2, ""));

            //    //self assignable roles react
            //    if (!guildConfig.ContainsKey("roles_react"))
            //        guildConfig.Add(new JProperty("roles_react", new JObject()));

            //    //self assignable roles
            //    if (!guildConfig.ContainsKey("roles_list"))
            //        guildConfig.Add(new JProperty("roles_list", new JArray()));

            //    //server_shop_rating
            //    if (!guildConfig.ContainsKey("server_shop_rating"))
            //        guildConfig.Add(new JProperty("server_shop_rating", "0"));

            //    //zone card type
            //    if (!guildConfig.ContainsKey(TradingCardCore.propertyIsZone))
            //        guildConfig.Add(new JProperty(TradingCardCore.propertyIsZone, "0"));

            //    //event_token for event only
            //    if (!guildConfig.ContainsKey(TradingCardCore.CardEvent.propertyToken))
            //        guildConfig.Add(new JProperty(TradingCardCore.CardEvent.propertyToken, ""));

            //    //sweethouse_shop
            //    if (!guildConfig.ContainsKey(PatissierCore.propertyShop))
            //        guildConfig.Add(new JProperty(PatissierCore.propertyShop, new JObject()));

            //    File.WriteAllText($"{Core.headConfigGuildFolder}{id_guild}/{id_guild}.json", guildConfig.ToString());

            

            //check if minigame_data.json esists/not
            //if (File.Exists($"{Core.headConfigGuildFolder}{id_guild}/{Core.minigameDataFileName}")){
            //    JObject quizConfig = JObject.Parse(File.ReadAllText($"{Core.headConfigGuildFolder}{id_guild}/{Core.minigameDataFileName}"));
            //    if (!quizConfig.ContainsKey("score")){
            //        quizConfig.Add(new JProperty("score", new JObject()));
            //        File.WriteAllText($"{Core.headConfigGuildFolder}{id_guild}/{Core.minigameDataFileName}", quizConfig.ToString());
            //    }
            //} else {
            //    JObject guildConfig = new JObject(
            //        new JProperty("score", new JObject()));

            //    File.WriteAllText($"{Core.headConfigGuildFolder}{id_guild}/{Core.minigameDataFileName}", guildConfig.ToString());
            //}
        }

        public static string getPropertyValue(ulong id_guild, string property)
        {
            var val = JObject.Parse(File.ReadAllText($"{Core.headConfigGuildFolder}{id_guild}/{id_guild}.json"));
            JProperty optionProp = val.Property(property);
            return optionProp.Value.ToString();
        }

        public static void setPropertyValue(ulong id_guild,string property, string value)
        {
            var val = JObject.Parse(File.ReadAllText($"{Core.headConfigGuildFolder}{id_guild}/{id_guild}.json"));
            JProperty optionProp = val.Property(property);
            optionProp.Value = value.ToString();
            File.WriteAllText($"{Core.headConfigGuildFolder}{id_guild}/{id_guild}.json", val.ToString());

            //init(id_guild);//reinit the array
        }

        public static void removePropertyValue(ulong id_guild, string property, string value)
        {
            if (hasPropertyValues(id_guild.ToString(), property)){
                var val = JObject.Parse(File.ReadAllText($"{Core.headConfigGuildFolder}{id_guild}/{id_guild}.json"));
                JProperty optionProp = val.Property(property);
                optionProp.Value = "";
                File.WriteAllText($"{Core.headConfigGuildFolder}{id_guild}/{id_guild}.json", val.ToString());
            }
        }

        public static void removeGuildConfigFile(string id_guild)
        {
            //remove guild config folder
            if (Directory.Exists($"{Core.headConfigGuildFolder}{id_guild}"))
                Directory.Delete($"{Core.headConfigGuildFolder}{id_guild}");

            //remove attachments
            if (Directory.Exists($"{Core.attachmentsFolder}{id_guild}"))
                Directory.Delete($"{Core.attachmentsFolder}{id_guild}");
        }

        public static Boolean hasPropertyValues(string id_guild, string property){
            var val = JObject.Parse(File.ReadAllText($"{Core.headConfigGuildFolder}{id_guild}/{id_guild}.json"));

            if (val.ContainsKey(property))
                if (val[property].ToString()!="")
                    return true;
            
            return false;
        }

        //public static IDictionary<string, ulong> Id_notif_online = new Dictionary<string, ulong>();
        //public static IDictionary<string, ulong> Id_random_event = new Dictionary<string, ulong>();

        //public IDictionary<string, ulong> Id_notif_online { get; set; }
        //public static ulong Id_notif_online { get; set; }
        //public static ulong Id_random_event { get; set; }
        //public static ulong Id_welcome { get; set; }
    }
}
