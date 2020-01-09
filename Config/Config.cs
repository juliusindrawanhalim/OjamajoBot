using System;

using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;
using Victoria;
using Discord;

namespace Config
{
    public class Core
    {
        public JObject jobjectfile = JObject.Parse(File.ReadAllText("config.json"));

        public Core()
        {
            try
            {
                //init guild config
                try
                {
                    String _parent = "guild";
                    Guild.Id = (ulong)jobjectfile.GetValue(_parent)["id"];
                    Guild.Id_notif_online = (ulong)jobjectfile.GetValue(_parent)["id_notif_online"];
                    Guild.Id_random_event = (ulong)jobjectfile.GetValue(_parent)["id_random_event"];
                    Guild.Id_welcome = (ulong)jobjectfile.GetValue(_parent)["id_welcome"];
                }
                catch { Console.WriteLine("Error: Guild configuration array is not properly formatted"); Console.ReadLine(); }
                //init doremi config
                try
                {
                    String _parent = "doremi";
                    Doremi.Token = jobjectfile.GetValue(_parent)["token"].ToString();
                    Doremi.Id = (ulong)jobjectfile.GetValue(_parent)["id"];
                    Doremi.Randomeventinterval = (double)jobjectfile.GetValue(_parent)["randomeventinterval"];
                }
                catch { Console.WriteLine("Error: Doremi configuration array is not properly formatted"); Console.ReadLine(); }
                //init hazuki config
                try
                {
                    String _parent = "hazuki";
                    Hazuki.Token = jobjectfile.GetValue(_parent)["token"].ToString();
                    Hazuki.Id = (ulong)jobjectfile.GetValue(_parent)["id"];
                    Hazuki.Randomeventinterval = (double)jobjectfile.GetValue(_parent)["randomeventinterval"];
                }
                catch { Console.WriteLine("Error: Hazuki configuration array is not properly formatted"); Console.ReadLine(); }
                //init aiko config
                try
                {
                    String _parent = "aiko";
                    Aiko.Token = jobjectfile.GetValue(_parent)["token"].ToString();
                    Aiko.Id = (ulong)jobjectfile.GetValue(_parent)["id"];
                    Aiko.Randomeventinterval = (double)jobjectfile.GetValue(_parent)["randomeventinterval"];
                }
                catch { Console.WriteLine("Error: Aiko configuration array is not properly formatted"); Console.ReadLine(); }
                //init music list
                try
                {
                    Music.jobjectfile = jobjectfile;
                    //for (int i = 0; i < (jobjectfile.GetValue("musiclist") as JObject).Count; i++)
                    //{
                    //    Console.WriteLine(jobjectfile.GetValue("musiclist")[(i + 1).ToString()]["title"].ToString());
                    //    MusicList.arrMusicList.Add(jobjectfile.GetValue("musiclist")[(i + 1).ToString()].ToString());
                    //}
                }
                catch (InvalidCastException e)
                {
                    Console.WriteLine("Error:" + e.ToString());
                }
            }
            catch { Console.WriteLine("Error: Config.json file not exist"); Console.ReadLine(); }
        }
    }

    public static class Guild
    {
        public static ulong Id { get; set; }
        public static ulong Id_notif_online { get; set; }
        public static ulong Id_random_event { get; set; }
        public static ulong Id_welcome { get; set; }
    }

    public static class Doremi
    {
        public static ulong Id { get; set; }
        public static String Token { get; set; }
        public static double Randomeventinterval { get; set; }
        public static Color EmbedColor = Color.Red;
        public static String MagicalStageWishes { get; set; }
    }

    public static class Hazuki
    {
        public static ulong Id { get; set; }
        public static String Token { get; set; }
        public static double Randomeventinterval { get; set; }
        public static Color EmbedColor = Color.Orange;
    }

    public static class Aiko
    {
        public static ulong Id { get; set; }
        public static String Token { get; set; }
        public static double Randomeventinterval { get; set; }
        public static Color EmbedColor = Color.Blue;
    }

    public static class Emoji
    {
        public static String doremi = "<:Doremi:651062436866293760>";
        public static String Hazuki = "<:Hazuki:651062978854125589>";
        public static String onpu = "<:Onpu:651063415514857492>";
        public static String drool = ":drooling_face:";
        public static String steak = ":cut_of_meat:";
        public static String dabzuki = "<:Dabzuki:658926367286755331>";

    }

    public static class Music
    {
        //public static List<string> arrMusicList = new List<string>();
        public static JObject jobjectfile { get; set; }
        public static List<LavaTrack> storedLavaTrack = new List<LavaTrack>();
        public static Byte repeat = 2;//0:repeat off;1:repeat one;2:repeat all

    }


}
