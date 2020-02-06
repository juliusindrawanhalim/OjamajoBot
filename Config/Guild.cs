using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Config
{
    public class Guild
    {
        public static void init(ulong id_guild)
        {
            //check if directory exists
            if (!Directory.Exists($"attachments/{id_guild.ToString()}"))
                Directory.CreateDirectory($"attachments/{id_guild.ToString()}");
            if (!Directory.Exists($"attachments/{id_guild.ToString()}/contribute"))
                Directory.CreateDirectory($"attachments/{id_guild.ToString()}/contribute");//init contribute folder for submitted meme pictures
            //check if feedback.txt exists/not
            if (!File.Exists($"attachments/{id_guild.ToString()}/feedback_{id_guild.ToString()}.txt"))
                File.CreateText($"attachments/{id_guild.ToString()}/feedback_{id_guild.ToString()}.txt");
            //check if logs directory exists/not
            if (!Directory.Exists($"logs/{id_guild.ToString()}"))
                Directory.CreateDirectory($"logs/{id_guild.ToString()}");
            //check if config guild directory exists/not
            if (!Directory.Exists($"{Core.headConfigGuildFolder}{id_guild.ToString()}"))
                Directory.CreateDirectory($"{Core.headConfigGuildFolder}{id_guild.ToString()}");

            if (File.Exists($"{Core.headConfigGuildFolder}{id_guild}/{id_guild}.json")){
                JObject guildConfig = JObject.Parse(File.ReadAllText($"{Core.headConfigGuildFolder}{id_guild}/{id_guild}.json"));

                //id_random_event
                if (!guildConfig.ContainsKey("id_random_event"))
                    guildConfig.Add(new JProperty("id_random_event", ""));

                //id_birthday_announcement
                if (!guildConfig.ContainsKey("id_birthday_announcement"))
                    guildConfig.Add(new JProperty("id_birthday_announcement", ""));

                //leaving user message
                if (!guildConfig.ContainsKey("user_leaving_notification"))
                    guildConfig.Add(new JProperty("user_leaving_notification", "1"));

                //user birthday
                if (!guildConfig.ContainsKey("user_birthday"))
                    guildConfig.Add(new JProperty("user_birthday", new JObject()));

                File.WriteAllText($"{Core.headConfigGuildFolder}{id_guild}/{id_guild}.json", guildConfig.ToString());

            } else { //create json file if it's not existed

                JObject guildConfig = new JObject(
                    new JProperty("id_random_event", ""),
                    new JProperty("id_birthday_announcement", ""),
                    new JProperty("user_leaving_notification", "0"),
                    new JProperty("user_birthday", new JObject()));

                File.WriteAllText($"{Core.headConfigGuildFolder}{id_guild}/{id_guild}.json", guildConfig.ToString());
            }

            //check if minigame_data.json esists/not
            if (File.Exists($"{Core.headConfigGuildFolder}{id_guild}/{Core.minigameDataFileName}")){
                JObject quizConfig = JObject.Parse(File.ReadAllText($"{Core.headConfigGuildFolder}{id_guild}/{Core.minigameDataFileName}"));
                if (!quizConfig.ContainsKey("score")){
                    quizConfig.Add(new JProperty("score", new JObject()));
                    File.WriteAllText($"{Core.headConfigGuildFolder}{id_guild}/{Core.minigameDataFileName}", quizConfig.ToString());
                }
            } else {
                JObject guildConfig = new JObject(
                    new JProperty("score", new JObject()));

                File.WriteAllText($"{Core.headConfigGuildFolder}{id_guild}/{Core.minigameDataFileName}", guildConfig.ToString());
            }
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
