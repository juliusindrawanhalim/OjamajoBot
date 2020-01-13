using System;

using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;
using Victoria;
using Discord;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;

namespace Config
{
    public class Core
    {
        public JObject jobjectfile;
        public static string headConfigFolder = "config/";

        public Core()
        {
            try {
                jobjectfile = JObject.Parse(File.ReadAllText("config/config.json"));

                //init doremi config
                try
                {
                    string _parent = "doremi";
                    Doremi.Token = jobjectfile.GetValue(_parent)["token"].ToString();
                    Doremi.Randomeventinterval = (double)jobjectfile.GetValue(_parent)["randomeventinterval"];
                }
                catch { Console.WriteLine("Error: Doremi configuration array is not properly formatted"); Console.ReadLine(); }

                //init hazuki config
                try
                {
                    string _parent = "hazuki";
                    Hazuki.Token = jobjectfile.GetValue(_parent)["token"].ToString();
                    Hazuki.Randomeventinterval = (double)jobjectfile.GetValue(_parent)["randomeventinterval"];
                }
                catch { Console.WriteLine("Error: Hazuki configuration array is not properly formatted"); Console.ReadLine(); }

                //init aiko config
                try
                {
                    string _parent = "aiko";
                    Aiko.Token = jobjectfile.GetValue(_parent)["token"].ToString();
                    Aiko.Randomeventinterval = (double)jobjectfile.GetValue(_parent)["randomeventinterval"];
                }
                catch { Console.WriteLine("Error: Aiko configuration array is not properly formatted"); Console.ReadLine(); }

                //init music config
                Music.jobjectfile = jobjectfile;
                Console.WriteLine("Log file initialized");
            } catch {
                Console.WriteLine("Config folder cannot be found");
            }
        }

    }

    public class Doremi
    {
        public static ulong Id = 655668640502251530;
        public static string Token { get; set; }
        public static double Randomeventinterval { get; set; }
        public static Color EmbedColor = Color.Red;
        public static string MagicalStageWishes { get; set; }

        public static List<string> listRandomEvent = new List<string>{
            $"{MentionUtils.MentionUser(Hazuki.Id)} let's go to maho dou",
            $"{MentionUtils.MentionUser(Aiko.Id)} let's go to maho dou",
            $"{MentionUtils.MentionUser(Hazuki.Id)} let's go to my house today",
            $"{MentionUtils.MentionUser(Aiko.Id)} let's go to my house today",
            $"Hii everyone, hope you all have a nice day and always be happy. Happy! Lucky! For all of you! :smile:",
            $"Hii everyone, don't forget to visit our shop: maho dou :smile:",
            $"Someone, please give me a big steak right now {Emoji.drool}{Emoji.steak}"
        };

        public static IDictionary<string, Timer> _timerRandomEvent = new Dictionary<string, Timer>();
        public static string[,] arrRandomActivity = {
            {"at misora elementary school" , "I'm still at school right now."},
            {"piano","I'm currently playing piano now. Do you want to hear me playing some music?"},
            {"at maho dou","I'm working on maho-dou right now. Please visit the shop any time"},
            {"with steak",$"I'm eating my favorite food right now, the big steak {Emoji.drool}"},
            {"with friends","I'm playing with my classmates. I love everyone on the class."},
            {"with Hazuki",$"I'm playing with {MentionUtils.MentionUser(Hazuki.Id)} right now. She's one of my best friend."},
            {"with Aiko",$"I'm playing with {MentionUtils.MentionUser(Aiko.Id)} right now. " +
                    $"We're going to make Takoyaki together with {MentionUtils.MentionUser(Hazuki.Id)}. " +
                    $"Please come and join us to make takoyaki, will you?"},
            {"with Kotake","Psst, I'm trying to disturb kotake right now :smirk:"},
            {"with Pop","I'm playing with Pop now. She needs my help with some piano lesson."}
        };
        public static int indexCurrentActivity { get; set; }
    }

    public class Hazuki
    {
        public static ulong Id = 655307117128974346;
        public static string Token { get; set; }
        public static double Randomeventinterval { get; set; }
        public static Color EmbedColor = Color.Orange;
        public static string[,] arrRandomActivity = {
            {"at misora elementary school" , "I'm still at school right now."},
            {"at maho dou","I'm working on maho-dou right now. Please come visit the shop any time."},
            {"with Doremi",$"I'm playing with {MentionUtils.MentionUser(Doremi.Id)} right now. She's one of my best friend."},
            {"with Aiko",$"I'm playing with {MentionUtils.MentionUser(Aiko.Id)} right now. We're going to make some Takoyaki with {MentionUtils.MentionUser(Doremi.Id)}."},
            {"violin","I'm playing with my violin instrument now. Wanna hear me to play some music?"},
            {"with Masaru","I'm playing with my Masaru right now. We're usually playing music together on the afternoon. :blush:"}
        };
        public static int indexCurrentActivity { get; set; }
    }

    public class Aiko
    {
        public static ulong Id = 663612449341046803;
        public static string Token { get; set; }
        public static double Randomeventinterval { get; set; }
        public static Color EmbedColor = Color.Blue;

        public static string[,] arrRandomActivity = {
            {"at misora elementary school" , "I'm still at school right now."},
            {"at maho dou","I'm working on maho-dou right now. Please come to the shop any time"},
            {"with takoyaki",$"I'm making some delicious takoyaki right now with {MentionUtils.MentionUser(Doremi.Id)} and {MentionUtils.MentionUser(Hazuki.Id)}. I will give you some when it's ready."},
            {"with friends","I'm playing with my classmates. We're gonna play some tennis table."},
            {"with Hazuki",$"I'm playing with {MentionUtils.MentionUser(Hazuki.Id)} right now. She's a really nice and kind friend for you to meet."},
            {"with Doremi",$"I'm playing with {MentionUtils.MentionUser(Doremi.Id)} right now. We're probably gonna make steak for her."},
            {"with Nobuko","I'm playing with Nobuko right now. Currently the latest Detective Boy Tatekawa series was pretty cool."},
            {"harmonica","I'm playing with my harmonica instrument now. Wanna hear me to play some music?"}
        };
        public static int indexCurrentActivity { get; set; }
    }

    public static class Emoji
    {
        public static string doremi = "<:Doremi:651062436866293760>";
        public static string hazuki = "<:Hazuki:651062978854125589>";
        public static string onpu = "<:Onpu:651063415514857492>";
        public static string drool = ":drooling_face:";
        public static string steak = ":cut_of_meat:";
        public static string dabzuki = "<:Dabzuki:658926367286755331>";
    }

    public static class Music
    {
        //public static List<string> arrMusicList = new List<string>();
        public static JObject jobjectfile { get; set; }
        //public static List<LavaTrack> storedLavaTrack = new List<LavaTrack>();
        //public static IDictionary<string, List<LavaTrack>> storedLavaTrack = new Dictionary<string, List<LavaTrack>>();
        public static IDictionary<string,List<string>> queuedTrack = new Dictionary<string,List<string>>();
        public static Byte repeat = 2;//0:repeat off;1:repeat one;2:repeat all

    }

}
