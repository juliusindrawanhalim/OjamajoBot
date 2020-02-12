using System;

using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;
using Victoria;
using Discord;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using System.Globalization;

namespace Config
{
    public class Core
    {
        public static JObject jobjectconfig; public static JObject jobjectQuiz;
        public static JObject jobjectRandomMoments;
        public static string headConfigFolder = "config/";
        public static string attachmentsFolder = "attachments/";
        public static string headConfigGuildFolder = $"{headConfigFolder}guild/";
        public static string headLogsFolder = "logs/";
        public static string minigameDataFileName = "minigame_data.json";
        public static string lastUpdate = "Feb 12,2020";
        public static JObject jObjWiki;
        public static string wikiParentUrl = "https://ojamajowitchling.fandom.com/wiki/";
        public static int minGlobalTimeHour = 12;
        public static int maxGlobalTimeHour = 15;

        public Core()
        {
            try {
                jobjectconfig = JObject.Parse(File.ReadAllText($"{headConfigFolder}config.json"));
                jobjectRandomMoments = JObject.Parse(File.ReadAllText($"{headConfigFolder}randomMoments.json"));
                jobjectQuiz = JObject.Parse(File.ReadAllText($"{headConfigFolder}quiz.json"));
                
                //init wiki json
                try
                {
                    jObjWiki = JObject.Parse(File.ReadAllText($"{headConfigFolder}wiki.json"));
                }
                catch { Console.WriteLine("Error: Wiki configuration array is not properly formatted"); Console.ReadLine(); }

                //init doremi config
                try
                {
                    string _parent = "doremi";
                    Doremi.Token = jobjectconfig.GetValue(_parent)["token"].ToString();
                    Doremi.Randomeventinterval = (double)jobjectconfig.GetValue(_parent)["randomeventinterval"];
                    Doremi.jObjRandomMoments = (JObject)jobjectRandomMoments.GetValue(_parent);
                    Doremi.jobjectdorememes = JObject.Parse(File.ReadAllText($"{headConfigFolder}dorememes.json"));
                }
                catch { Console.WriteLine("Error: Doremi configuration array is not properly formatted"); Console.ReadLine(); }

                //init hazuki config
                try
                {
                    string _parent = "hazuki";
                    Hazuki.Token = jobjectconfig.GetValue(_parent)["token"].ToString();
                    Hazuki.Randomeventinterval = (double)jobjectconfig.GetValue(_parent)["randomeventinterval"];
                    Hazuki.jObjRandomMoments = (JObject)jobjectRandomMoments.GetValue(_parent);
                }
                catch { Console.WriteLine("Error: Hazuki configuration array is not properly formatted"); Console.ReadLine(); }

                //init aiko config
                try
                {
                    string _parent = "aiko";
                    Aiko.Token = jobjectconfig.GetValue(_parent)["token"].ToString();
                    Aiko.Randomeventinterval = (double)jobjectconfig.GetValue(_parent)["randomeventinterval"];
                    Aiko.jObjRandomMoments = (JObject)jobjectRandomMoments.GetValue(_parent);
                }
                catch { Console.WriteLine("Error: Aiko configuration array is not properly formatted"); Console.ReadLine(); }

                //init onpu config
                try
                {
                    string _parent = "onpu";
                    Onpu.Token = jobjectconfig.GetValue(_parent)["token"].ToString();
                    Onpu.Randomeventinterval = (double)jobjectconfig.GetValue(_parent)["randomeventinterval"];
                    Onpu.jObjRandomMoments = (JObject)jobjectRandomMoments.GetValue(_parent);
                }
                catch { Console.WriteLine("Error: Onpu configuration array is not properly formatted"); Console.ReadLine(); }

                //init momoko config
                try
                {
                    string _parent = "momoko";
                    Momoko.Token = jobjectconfig.GetValue(_parent)["token"].ToString();
                    Momoko.Randomeventinterval = (double)jobjectconfig.GetValue(_parent)["randomeventinterval"];
                    Momoko.jObjRandomMoments = (JObject)jobjectRandomMoments.GetValue(_parent);
                }
                catch { Console.WriteLine("Error: Momoko configuration array is not properly formatted"); Console.ReadLine(); }

                //init music config
                Music.jobjectfile = jobjectconfig;
                Console.WriteLine("Log file initialized");
            } catch {
                Console.WriteLine("Config folder cannot be found");
            }
        }

        public static bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
                return false;
            }
        }

    }

    public class Doremi
    {
        //public static ulong Id = 673223105237352488;//beta
        public static ulong Id = 655668640502251530;//original
        public static string Token { get; set; }
        public static double Randomeventinterval { get; set; }
        public static Color EmbedColor = Color.Red;
        public static DateTime birthdayDate = DateTime.ParseExact("30/07/1990", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
        
        public static string MagicalStageWishes { get; set; }
        public static string[] PrefixParent = {"do!","doremi!",MentionUtils.MentionUser(Id)};
        public static string DoremiBirthdayCakeImgSrc = "https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/a5/-Doremi-.Motto.Ojamajo.Doremi.25.-7C457374-.avi_snapshot_19.25_-2020.02.12_10.34.20-.jpg";

        public static IDictionary<string, Boolean> isRunningMinigame = new Dictionary<string, Boolean>();

        public static JObject jObjRandomMoments;
        public static JObject jobjectdorememes;

        public static List<string> listRandomEvent = new List<string>{
            $"{MentionUtils.MentionUser(Hazuki.Id)} let's go to maho dou",
            $"{MentionUtils.MentionUser(Aiko.Id)} let's go to maho dou",
            $"{MentionUtils.MentionUser(Hazuki.Id)} let's go to my house today",
            $"{MentionUtils.MentionUser(Aiko.Id)} let's go to my house today",
            $"Hii everyone, hope you all have a nice day and always be happy. Happy! Lucky! For all of you! :smile:",
            $"Hii everyone, don't forget to visit our shop: maho dou :smile:",
            $"Someone, please give me a big steak right now {Emoji.drool}{Emoji.steak}",
            $"Pirika pirilala poporina peperuto! Please makes everyone happy today!"
        };

        public static IDictionary<string, Timer> _timerRandomEvent = new Dictionary<string, Timer>();
        public static IDictionary<string, Timer> _timerBirthdayAnnouncement = new Dictionary<string, Timer>();//birthday reminder timer

        public static string[,] arrRandomActivity = {
            {$"at misora elementary school {Emoji.school}" , "I'm still at school right now."},
            {$"piano {Emoji.piano}","I'm playing piano right now. Do you want to hear me playing the piano?"},
            {"at sweet house maho dou","I'm working on maho-dou right now. Please visit the shop any time"},
            {$"with steak {Emoji.steak}",$"I'm eating my favorite food right now, the big steak {Emoji.drool}"},
            {"with friends","I'm playing with my classmates. I love everyone on the class."},
            {"with Hazuki",$"I'm playing with {MentionUtils.MentionUser(Hazuki.Id)} right now. She's one of my best friend."},
            {"with Aiko",$"I'm playing with {MentionUtils.MentionUser(Aiko.Id)} right now. " +
                $"We're going to make Takoyaki together with {MentionUtils.MentionUser(Hazuki.Id)}, {MentionUtils.MentionUser(Onpu.Id)} and {MentionUtils.MentionUser(Momoko.Id)}. " +
                $"Please come and join us to make takoyaki, will you?"},
            {"with Kotake","Psst, I'm trying to disturb kotake right now :smirk:"},
            {"with Pop","I'm playing with Pop now. She needs my help with some piano lesson."},
            {"at House",$"I'm at my house right now. I hope my mom will make a steak for dinner {Emoji.steak}"},
            {"at witch's world","I'm at the witch's world right now."},
            {"with homework \uD83D\uDCDA","I'm doing my homework right now"},
            {"with Momoko",$"I'm playing with {MentionUtils.MentionUser(Momoko.Id)} right now."},
            {"with Onpu",$"I'm playing with {MentionUtils.MentionUser(Onpu.Id)} right now."},
            {"with Dodo",$"I'm playing with fairy: Dodo right now."}
        };
        public static int indexCurrentActivity { get; set; }

        public static string EmbedName = "Doremi Bot";
        public static string EmbedAvatarUrl = "https://cdn.discordapp.com/emojis/651062436866293760.png?v=1";
    }

    public class Hazuki
    {
        //public static ulong Id = 666572726592471040;
        public static ulong Id = 655307117128974346; //original
        public static string Token { get; set; }
        public static double Randomeventinterval { get; set; }
        public static Color EmbedColor = Color.Orange;
        public static DateTime birthdayDate = DateTime.ParseExact("14/02/1991", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
        public static string[] PrefixParent = { "ha!", "hazuki!", MentionUtils.MentionUser(Id) };
        public static JObject jObjRandomMoments;

        public static IDictionary<string, Timer> _timerRandomEvent = new Dictionary<string, Timer>();
        public static IDictionary<string, Timer> _timerBirthdayAnnouncement = new Dictionary<string, Timer>();//birthday reminder timer

        public static string[,] arrRandomActivity = {
            {$"at misora elementary school {Emoji.school}" , "I'm still at school right now."},
            {"at sweet house maho dou","I'm working on maho-dou right now. Please come visit the shop any time."},
            {"with Doremi",$"I'm playing with {MentionUtils.MentionUser(Doremi.Id)} right now. She's one of my best friend."},
            {"with Aiko",$"I'm playing with {MentionUtils.MentionUser(Aiko.Id)} right now. " +
                    $"We're going to make some Takoyaki with {MentionUtils.MentionUser(Doremi.Id)}, {MentionUtils.MentionUser(Onpu.Id)} and {MentionUtils.MentionUser(Momoko.Id)}."},
            {$"violin {Emoji.violin}","I'm playing with my violin instrument now. Wanna hear me to play some music?"},
            {"with Masaru","I'm playing with my Masaru right now. We're usually playing music together on the afternoon \uD83D\uDE0A"},
            {"at House",$"I'm at my house right now. I have violin lesson to attend after this."},
            {"at witch's world","I'm at the witch's world right now."},
            {"with homework \uD83D\uDCDA","I'm doing my homework right now."},
            {"with Momoko",$"I'm playing with {MentionUtils.MentionUser(Momoko.Id)} right now."},
            {"with Onpu",$"I'm playing with {MentionUtils.MentionUser(Onpu.Id)} right now."},
            {"with Rere",$"I'm playing with my fairy: Rere right now."}
        };
        public static int indexCurrentActivity { get; set; }

        public static string EmbedName = "Hazuki Bot";
        public static string EmbedAvatarUrl = "https://cdn.discordapp.com/emojis/651062978854125589.png?v=1";
    }

    public class Aiko
    {
        //public static ulong Id = 666574244725129216;
        public static ulong Id = 663612449341046803;//original
        public static string Token { get; set; }
        public static double Randomeventinterval { get; set; }
        public static Color EmbedColor = Color.Blue;
        public static DateTime birthdayDate = DateTime.ParseExact("14/11/1990", "dd/MM/yyyy" ,CultureInfo.InvariantCulture, DateTimeStyles.None);
        public static string[] PrefixParent = { "ai!", "aiko!", MentionUtils.MentionUser(Id) };
        public static JObject jObjRandomMoments;

        public static IDictionary<string, Timer> _timerRandomEvent = new Dictionary<string, Timer>();
        public static IDictionary<string, Timer> _timerBirthdayAnnouncement = new Dictionary<string, Timer>();//birthday reminder timer

        public static string[,] arrRandomActivity = {
            {$"at misora elementary school {Emoji.school}","I'm still at school right now."},
            {"at sweet house maho dou","I'm working on maho-dou right now. Please come to the shop any time"},
            {"with takoyaki",$"I'm making some delicious takoyaki right now with {MentionUtils.MentionUser(Doremi.Id)}, {MentionUtils.MentionUser(Hazuki.Id)}, {MentionUtils.MentionUser(Onpu.Id)} and {MentionUtils.MentionUser(Momoko.Id)}. " +
                    $"I will give you some when it's ready."},
            {"with friends","I'm playing with my classmates. We're gonna play some tennis table."},
            {"with Hazuki",$"I'm playing with {MentionUtils.MentionUser(Hazuki.Id)} right now. She's a really nice and kind friend for you to meet."},
            {"with Doremi",$"I'm playing with {MentionUtils.MentionUser(Doremi.Id)} right now. We're probably gonna make steak for her."},
            {"with Nobuko","I'm playing with Nobuko right now. Currently the latest Detective Boy Tatekawa series was pretty cool."},
            {"harmonica","I'm playing with my harmonica instrument now. Wanna hear me to play some music?"},
            {"at witch's world","I'm at the witch's world right now."},
            {"with sweet potatoes","Sweet potatoes is one of my favorite foods, I just love to eat it so much."},
            {"with homework \uD83D\uDCDA","I'm doing my homework right now."},
            {"with Momoko",$"I'm playing with {MentionUtils.MentionUser(Momoko.Id)} right now."},
            {"with Onpu",$"I'm playing with {MentionUtils.MentionUser(Onpu.Id)} right now."},
            {"with Mimi",$"I'm playing with my fairy: Mimi right now."}
        };
        public static int indexCurrentActivity { get; set; }

        public static string EmbedName = "Aiko Bot";
        public static string EmbedAvatarUrl = "https://cdn.discordapp.com/emojis/651063151948726273.png?v=1";
    }

    public class Onpu
    {
        //public static ulong Id = 666575360191627265;
        public static ulong Id = 663615334150045706;//original
        public static string Token { get; set; }
        public static double Randomeventinterval { get; set; }
        public static Color EmbedColor = Color.Purple;
        public static DateTime birthdayDate = DateTime.ParseExact("03/03/1991", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
        public static string[] PrefixParent = { "on!", "onpu!", MentionUtils.MentionUser(Id) };
        public static JObject jObjRandomMoments;

        public static IDictionary<string, Timer> _timerRandomEvent = new Dictionary<string, Timer>();
        public static IDictionary<string, Timer> _timerBirthdayAnnouncement = new Dictionary<string, Timer>();//birthday reminder timer

        public static string[,] arrRandomActivity = {
            {$"at misora elementary school {Emoji.school}" , "I'm still at school right now."},
            {"at sweet house maho dou","I'm working on maho-dou right now. Please come to the shop any time"},
            {"with Hazuki",$"I'm playing with {MentionUtils.MentionUser(Hazuki.Id)} right now. She's a really nice and kind friend for you to meet."},
            {"with Doremi",$"I'm playing with {MentionUtils.MentionUser(Doremi.Id)} right now. We're probably gonna make steak for her."},
            {"with Aiko",$"I'm playing with {MentionUtils.MentionUser(Aiko.Id)} right now. We're probably gonna make steak for her."},
            {"with Momoko",$"I'm playing with {MentionUtils.MentionUser(Momoko.Id)} right now."},
            {"at tv studio","I'm working on the studio right now. Feel free to come and watch me on some drama performances."},
            {"at radio station","I'm currently broadcasting at radio station right now. Stay tune for more daily info."},
            {"flute","I'm playing with my flute instrument now. Wanna hear me to play some music?"},
            {"with homework \uD83D\uDCDA","I'm doing my homework right now."},
            {"with Roro",$"I'm playing with my fairy: Roro right now."}
        };
        public static int indexCurrentActivity { get; set; }

        public static string EmbedName = "Onpu Bot";
        public static string EmbedAvatarUrl = "https://cdn.discordapp.com/emojis/651063415514857492.png?v=1";
    }

    public class Momoko
    {
        //public static ulong Id = 666576136045592586;
        public static ulong Id = 663615454140432414;//original
        public static string Token { get; set; }
        public static double Randomeventinterval { get; set; }
        public static Color EmbedColor = new Color(234, 211, 57);
        public static DateTime birthdayDate = DateTime.ParseExact("06/05/1990", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
        
        public static string[] PrefixParent = { "mo!", "momoko!", MentionUtils.MentionUser(Id) };
        public static JObject jObjRandomMoments;

        public static IDictionary<string, Timer> _timerRandomEvent = new Dictionary<string, Timer>();
        public static IDictionary<string, Timer> _timerBirthdayAnnouncement = new Dictionary<string, Timer>();//birthday reminder timer

        public static string[,] arrRandomActivity = {
            {$"at misora elementary school{Emoji.school}" , "I'm still at school right now."},
            {"at sweet house maho dou","I'm working on maho-dou right now. Please come to the shop any time"},
            {"with Hazuki",$"I'm playing with {MentionUtils.MentionUser(Hazuki.Id)} right now. She's a really nice and kind friend for you to meet."},
            {"with Doremi",$"I'm playing with {MentionUtils.MentionUser(Doremi.Id)} right now. We're probably gonna make steak for her."},
            {"with Aiko",$"I'm playing with {MentionUtils.MentionUser(Aiko.Id)} right now. She's a nice girl that loves to eat takoyaki very much."},
            {"with Onpu",$"I'm playing with {MentionUtils.MentionUser(Onpu.Id)} right now."},
            {$"guitar{Emoji.guitar}","I'm playing with my guitar instrument now. Wanna hear me to play some music?"},
            {"with homework \uD83D\uDCDA","I'm doing my homework right now."},
            {"with Nini",$"I'm playing with my fairy: Nini right now."}
        };
        public static int indexCurrentActivity { get; set; }

        public static string EmbedName = "Momoko Bot";
        public static string EmbedAvatarUrl = "https://cdn.discordapp.com/emojis/651063629403127808.png?v=1";
        public static IDictionary<string, Timer> timerProcessBakery = new Dictionary<string, Timer>();
        public static IDictionary<string, Boolean> isRunningBakery = new Dictionary<string, Boolean>();
    }

    public static class Emoji
    {
        public static string doremi = "<:Doremi:651062436866293760>";
        public static string hazuki = "<:Hazuki:651062978854125589>";
        public static string onpu = "<:Onpu:651063415514857492>";
        public static string dabzuki = "<:Dabzuki:658926367286755331>";
        public static string drool = "\uD83E\uDD24";
        public static string steak = "\uD83E\uDD69";
        public static string school = "\uD83C\uDFEB";
        public static string piano = "\uD83C\uDFB9";
        public static string violin = "\uD83C\uDFBB";
        public static string guitar = "\uD83C\uDFB8";
        public static string clap = "\uD83D\uDC4F";
        public static string birthdayCake = "\uD83C\uDF82";
        public static string partyPopper = "\uD83C\uDF89";
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
