﻿using Discord.WebSocket;
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
            if (File.Exists($"config/{id_guild}.json"))
            {
                JObject guildConfig = JObject.Parse(File.ReadAllText($"config/{id_guild}.json"));
                
                //id_notif_online
                if (!guildConfig.ContainsKey("id_notif_online")) 
                    guildConfig.Add(new JProperty("id_notif_online", ""));
                
                if (ulong.TryParse(guildConfig.GetValue("id_notif_online").ToString(),out var result))
                    Guild.Id_notif_online[$"{id_guild}"] = (ulong)guildConfig.GetValue("id_notif_online");
                
                //id_random_event
                if (!guildConfig.ContainsKey("id_random_event"))
                    guildConfig.Add( new JProperty("id_random_event", ""));
                
                if (ulong.TryParse(guildConfig.GetValue("id_random_event").ToString(), out var resultNotifOnline))
                    Guild.Id_random_event[$"{id_guild}"] = (ulong)guildConfig.GetValue("id_random_event");

                File.WriteAllText($"config/{id_guild}.json", guildConfig.ToString());

            } else { //create json file if it's not existed
                
                JObject guildConfig = new JObject(
                    new JProperty("id_notif_online", ""),
                    new JProperty("id_random_event", ""));

                File.WriteAllText($"config/{id_guild}.json", guildConfig.ToString());
            }
            
        }

        public static void assignId(ulong id_guild,string property, string value)
        {
            var val = JObject.Parse(File.ReadAllText($"config/{id_guild.ToString()}.json"));
            JProperty optionProp = val.Property(property);
            //string option = optionProp.Value.Value<string>();

            optionProp.Value = value.ToString();
            File.WriteAllText($"config/{id_guild.ToString()}.json", val.ToString());

            init(id_guild);//reinit the array
        }

        public static void remove(String id_guild)
        {
            if (File.Exists($"config/{id_guild}.json"))
                File.Delete($"config/{id_guild}.json");
            
        }

        public static IDictionary<string, ulong> Id_notif_online = new Dictionary<string, ulong>();
        public static IDictionary<string, ulong> Id_random_event = new Dictionary<string, ulong>();

        //public IDictionary<string, ulong> Id_notif_online { get; set; }
        //public static ulong Id_notif_online { get; set; }
        //public static ulong Id_random_event { get; set; }
        //public static ulong Id_welcome { get; set; }
    }
}
