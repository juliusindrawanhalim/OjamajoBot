﻿using System;

using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;
using Victoria;
using Discord;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using System.Globalization;
using System.Diagnostics;
using System.Collections;
using Discord.WebSocket;
using System.Threading.Tasks;
using Spectacles.NET.Types;
using System.Security.Cryptography.X509Certificates;
using OjamajoBot.Bot;

namespace Config
{

    public class Core
    {
        public static Dictionary<string, string> customPrefix = new Dictionary<string, string>();
        public static JObject jobjectconfig; public static JObject jobjectQuiz;
        public static JObject jobjectRandomMoments;
        public static string headTradingCardConfigFolder = "trading_card";
        public static string headAchievementsConfigFolder = "achievements";
        public static string headTradingCardSaveConfigFolder = "trading_card_save/";
        public static string attachmentsFolder = "attachments/";
        public static string headConfigFolder = "config/";
        public static string headConfigGuildFolder = $"{headConfigFolder}guild/";
        public static string headLogsFolder = "logs/";
        public static string minigameDataFileName = "minigame_data.json";
        public static string tradingCardDataFileName = "trading_card_list.json";
        public static string configFileName = "config.json";


        public static string sweethouseRecipeDataFileName = "sweethouse_recipe_data.json";

        public static string version = "1.9";
        public static string lastUpdate = "Jan 7,2021";
        public static JObject jObjWiki;
        public static string wikiParentUrl = "https://ojamajowitchling.fandom.com/wiki/";
        public static int minGlobalTimeHour = 7;
        public static int maxGlobalTimeHour = 15;

        public Core()
        {
            try {
                jobjectconfig = JObject.Parse(File.ReadAllText($"{headConfigFolder}{configFileName}"));
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

                //init database
                

                //init pop config
                //try
                //{
                //    string _parent = "pop";
                //    Pop.Token = jobjectconfig.GetValue(_parent)["token"].ToString();
                //    Pop.Randomeventinterval = (double)jobjectconfig.GetValue(_parent)["randomeventinterval"];
                //    Pop.jObjRandomMoments = (JObject)jobjectRandomMoments.GetValue(_parent);
                //}
                //catch { Console.WriteLine("Error: Pop configuration array is not properly formatted"); Console.ReadLine(); }

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
        
        public enum BotClass {
            Doremi,
            Hazuki,
            Aiko,
            Onpu,
            Momoko
        }

        public class BotStatus
        {
            public static object[] statusSleeping = { "bed time", $"Oyasumi...", UserStatus.DoNotDisturb };
            public static object[] statusSchool = { $"at misora elementary school {Emoji.school}", "I'm still at the school right now.", UserStatus.DoNotDisturb };
            public static object[] statusGlobalMahoDou = {"at maho dou","I'm working on maho-dou right now. Please come and visit the shop any time~", UserStatus.Idle};
            public static List<List<object>> arrPlayingWith = new List<List<object>>();

            public BotStatus()
            {
                arrPlayingWith.Add(new List<object>{ "with Doremi", $"I'm playing with {MentionUtils.MentionUser(Doremi.Id)} right now.", UserStatus.Online});
                arrPlayingWith.Add(new List<object>{ "with Hazuki", $"I'm playing with {MentionUtils.MentionUser(Hazuki.Id)} right now.", UserStatus.Online });
                arrPlayingWith.Add(new List<object> {"with Aiko",$"I'm playing with {MentionUtils.MentionUser(Aiko.Id)} right now. " +
                "Please come and join us to make takoyaki together, will you?", UserStatus.Online});
                arrPlayingWith.Add(new List<object> { "with Onpu", $"I'm playing with {MentionUtils.MentionUser(Onpu.Id)} right now.", UserStatus.Online });
                arrPlayingWith.Add(new List<object> { "with Momoko", $"I'm playing with {MentionUtils.MentionUser(Momoko.Id)} right now.", UserStatus.Online });

            }

            //public static List<object>[,] arrPlayingWith = 
            //{
            //    { "with Doremi", $"I'm playing with {MentionUtils.MentionUser(Doremi.Id)} right now.", UserStatus.Online},
            //    { "with Hazuki", $"I'm playing with {MentionUtils.MentionUser(Hazuki.Id)} right now." , UserStatus.Online},
            //    {"with Aiko",$"I'm playing with {MentionUtils.MentionUser(Aiko.Id)} right now. " +
            //    "Please come and join us to make takoyaki together, will you?", UserStatus.Online},
            //    { "with Onpu", $"I'm playing with {MentionUtils.MentionUser(Onpu.Id)} right now.", UserStatus.Online },
            //    { "with Momoko", $"I'm playing with {MentionUtils.MentionUser(Momoko.Id)} right now.", UserStatus.Online }
            //};
            public static string myBirthdayActivity = $"at my birthday party {Config.Emoji.birthdayCake}";
            public static string myBirthdayActivityReply = "it's my birthday party today!";
            public static UserStatus myBirthdayActivityUserStatus = UserStatus.Online;

            public static Tuple<object[]> checkStatusActivity(BotClass caller, object[,] arrRandomActivity)
            {
                //return parameter: force status change,the status array
                Boolean isWeekday = false;
                if (DateTime.Now.ToString("dddd").ToLower() == "monday" || DateTime.Now.ToString("dddd").ToLower() == "tuesday" ||
                DateTime.Now.ToString("dddd").ToLower() == "wednesday" || DateTime.Now.ToString("dddd").ToLower() == "thursday" ||
                DateTime.Now.ToString("dddd").ToLower() == "friday"){
                    isWeekday = true;
                }

                //start
                //if (randomizePlayingWith)
                //{
                //    //doremi->hazuki,hazuki->doremi

                //    //random doremi:
                //    int randomPlayingWithDoremi = new Random().Next(0, 5);
                //    if(!Config.Doremi.Status.isPlayingWith && 
                //        randomPlayingWithDoremi == 0)
                //    {
                //        arrPlayingWith.RemoveAt(0);
                //        Config.Doremi.Status.currentActivity = arrPlayingWith[0][0];
                //        Config.Doremi.Status.userStatus = (UserStatus)arrPlayingWith[0, 1];

                //    } else
                //    {

                //    }

                //    //random hazuki:
                //    //random aiko:
                //    //random onpu:
                //    //random momoko:
                //}
                //end

                object[] returnStatus = { "init","init","init" };

                Boolean forceChange = false;
                
                //if (Config.Doremi.Status.isBirthday())
                //{
                //    if(caller == BotClass.Doremi)
                //    {
                //        returnStatus[0] = myBirthdayActivity;
                //        returnStatus[1] = myBirthdayActivityReply;
                //        returnStatus[2] = myBirthdayActivityUserStatus;
                //    } else
                //    {
                //        returnStatus[0] = Config.Doremi.Status.birthdayActivity;
                //        returnStatus[1] = "We're celebrating Doremi birthday today.";
                //        returnStatus[2] = myBirthdayActivityUserStatus;
                //    }
                //    forceChange = true;
                //}

                //if (Config.Hazuki.Status.isBirthday())
                //{
                //    if (caller == BotClass.Hazuki)
                //    {
                //        returnStatus[0] = myBirthdayActivity;
                //        returnStatus[1] = myBirthdayActivityReply;
                //        returnStatus[2] = myBirthdayActivityUserStatus;
                //    }
                //    else
                //    {
                //        returnStatus[0] = Config.Hazuki.Status.birthdayActivity;
                //        returnStatus[1] = "We're celebrating Hazuki birthday today.";
                //        returnStatus[2] = myBirthdayActivityUserStatus;
                //    }
                //    forceChange = true;
                //}

                //if (Config.Aiko.Status.isBirthday())
                //{
                //    if (caller == BotClass.Aiko)
                //    {
                //        returnStatus[0] = myBirthdayActivity;
                //        returnStatus[1] = myBirthdayActivityReply;
                //        returnStatus[2] = myBirthdayActivityUserStatus;
                //    }
                //    else
                //    {
                //        returnStatus[0] = Config.Aiko.Status.birthdayActivity;
                //        returnStatus[1] = "We're celebrating Aiko birthday today.";
                //        returnStatus[2] = myBirthdayActivityUserStatus;
                //    }
                //    forceChange = true;
                //}

                //if (Config.Onpu.Status.isBirthday())
                //{
                //    if (caller == BotClass.Onpu)
                //    {
                //        returnStatus[0] = myBirthdayActivity;
                //        returnStatus[1] = myBirthdayActivityReply;
                //        returnStatus[2] = myBirthdayActivityUserStatus;
                //    }
                //    else
                //    {
                //        returnStatus[0] = Config.Onpu.Status.birthdayActivity;
                //        returnStatus[1] = "We're celebrating Onpu birthday today.";
                //        returnStatus[2] = myBirthdayActivityUserStatus;
                //    }
                //    forceChange = true;
                //}

                //if (Config.Momoko.Status.isBirthday())
                //{
                //    if (caller == BotClass.Momoko)
                //    {
                //        returnStatus[0] = myBirthdayActivity;
                //        returnStatus[1] = myBirthdayActivityReply;
                //        returnStatus[2] = myBirthdayActivityUserStatus;
                //    }
                //    else
                //    {
                //        returnStatus[0] = Config.Momoko.Status.birthdayActivity;
                //        returnStatus[1] = "We're celebrating Momoko birthday today.";
                //        returnStatus[2] = myBirthdayActivityUserStatus;
                //    }
                //    forceChange = true;
                //}


                if (!forceChange)
                {
                    int hourNow = Convert.ToInt32(DateTime.Now.ToString("%H"));
                    if ( hourNow >= 22 || hourNow < 4)
                    {
                        returnStatus = statusSleeping;
                    }
                    //else if (isWeekday && hourNow >= 4 && hourNow <= 10)
                    //{
                    //    returnStatus = statusSchool;
                    //}
                    else if (isWeekday && hourNow >= 11 && hourNow <= 12)
                    {
                        returnStatus = statusGlobalMahoDou;
                    }
                    else
                    {
                        //random activity:
                        //,reply,user status

                        Random rnd = new Random();
                        int rndIndex = rnd.Next(0, arrRandomActivity.GetLength(0));

                        //original:
                        returnStatus = new object[]{ arrRandomActivity[rndIndex, 0],
                            arrRandomActivity[rndIndex, 1],
                            arrRandomActivity[rndIndex, 2] };

                    }
                }
                
                 return Tuple.Create(returnStatus);
            }


        }

    }

    public class Doremi
    {
        //public static ulong Id = 673223105237352488;//beta
        public static ulong Id = 655668640502251530;//original
        public static string Token { get; set; }
        public static double Randomeventinterval { get; set; }
        public static Discord.Color EmbedColor = new Color(247, 140, 193);
        public static DateTime birthdayDate = DateTime.ParseExact("30/07/1990", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
        public static int birthdayCalculatedYear = Convert.ToInt32(DateTime.Now.ToString("yyyy")) - Convert.ToInt32(birthdayDate.ToString("yyyy"));

        public static string MagicalStageWishes { get; set; }
        public static string[] PrefixParent = {"do!","doremi!",MentionUtils.MentionUser(Id)};
        public static string DoremiBirthdayCakeImgSrc = "https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/a5/-Doremi-.Motto.Ojamajo.Doremi.25.-7C457374-.avi_snapshot_19.25_-2020.02.12_10.34.20-.jpg";

        public static IDictionary<string, Boolean> isRunningMinigame = new Dictionary<string, Boolean>();

        public static JObject jObjRandomMoments;
        public static JObject jobjectdorememes;

        //public static IDictionary<string, Boolean> isRunningTradeCard = new Dictionary<string, Boolean>();
        //public static IDictionary<string, Boolean> isRunningTradeCardProcess = new Dictionary<string, Boolean>();
        public static IDictionary<string,Boolean> isRunningInteractive = new Dictionary<string, Boolean>();

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
        //trading card
        public static IDictionary<string, Timer> _timerTradingCardSpawn = new Dictionary<string, Timer>();//trading card spawn timer
        public static IDictionary<string, Stopwatch> _stopwatchCardSpawn = new Dictionary<string, Stopwatch>();//trading card stopwatch spawn timer
        public static IDictionary<string, List<IMessage>> _imReactionRole = new Dictionary<string, List<IMessage>>();
        
        //public static IDictionary<string, string> _tradingCardSpawnedId = new Dictionary<string, string>();//the spawned trading card id
        //public static IDictionary<string, string> _tradingCardSpawnedCategory = new Dictionary<string, string>();//category
        //public static IDictionary<string, string> _tradingCardCatchToken = new Dictionary<string, string>();//catch token

        public class Status
        {
            public static object[,] arrRandomActivity = {
                {$"piano {Emoji.piano}","I'm playing piano right now. Do you want to hear me playing the piano?", UserStatus.Online},
                {$"with big steak {Emoji.steak}",$"I'm eating my favorite food right now, the big steak {Emoji.drool}", UserStatus.Online},
                {"with Kotake","Psst, I'm trying to disturb kotake right now :smirk:", UserStatus.Online},
                {"with Pop","I'm playing with Pop now. She needs my help with some piano lesson.", UserStatus.Online},
                {"with Hana","I'm playing with Hana now.", UserStatus.Online},
                {"at Home",$"I'm at my home right now. I hope my mom will make a steak for dinner {Emoji.steak}", UserStatus.Online},
                {$"at witch's world {Emoji.broom}","I'm at the witch's world right now.", UserStatus.Online},
                //{"with homework \uD83D\uDCDA","I'm doing my homework right now", UserStatus.DoNotDisturb},
                {"with Dodo the fairy",$"I'm playing with fairy: Dodo right now.", UserStatus.Online},
                {"with Tamaki",$"I'm playing with Tamaki right now.", UserStatus.Online},
                {"with Ojamajo Trading Card",$"I'm playing Ojamajo Trading Card right now.", UserStatus.Online}
            };

            public static object[,] arrPlayingWith = {
                {"with Hazuki", $"I'm playing with {MentionUtils.MentionUser(Hazuki.Id)} right now." , UserStatus.Online},
                {"with Aiko",$"I'm playing with {MentionUtils.MentionUser(Aiko.Id)} right now. " +
                "Please come and join us to make takoyaki together, will you?", UserStatus.Online},
                { "with Onpu", $"I'm playing with {MentionUtils.MentionUser(Onpu.Id)} right now.", UserStatus.Online },
                { "with Momoko", $"I'm playing with {MentionUtils.MentionUser(Momoko.Id)} right now.", UserStatus.Online }
            };

            public static string birthdayActivity = $"at Doremi birthday {Config.Emoji.birthdayCake}";

            public static string currentActivity = "";
            public static string currentActivityReply = "";
            public static UserStatus userStatus = UserStatus.Online;
            public static Boolean isPlayingWith = false;

            public static Boolean isBirthday()
            {
                Boolean isBirthday = false;
                if (DateTime.Now.ToString("dd") == Config.Doremi.birthdayDate.ToString("dd") &&
                DateTime.Now.ToString("MM") == Config.Doremi.birthdayDate.ToString("MM") &&
                Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour)
                {
                    isBirthday = true;
                }
                return isBirthday;
            }
        }

        public static string EmbedName = "Doremi Bot";
        public static string EmbedNameError = "Doremi 404";
        public static string EmbedAvatarUrl = "https://cdn.discordapp.com/avatars/655668640502251530/96739e8725d5607cf9bb592d4a52f920.png";
    }

    public class Hazuki
    {
        //public static ulong Id = 666572726592471040;
        public static ulong Id = 655307117128974346; //original
        public static string Token { get; set; }
        public static double Randomeventinterval { get; set; }
        public static Color EmbedColor = Color.Orange;
        public static DateTime birthdayDate = DateTime.ParseExact("14/02/1991", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
        
        public static int birthdayCalculatedYear = Convert.ToInt32(DateTime.Now.ToString("yyyy")) - Convert.ToInt32(birthdayDate.ToString("yyyy"));
        public static string[] PrefixParent = { "ha!", "hazuki!", MentionUtils.MentionUser(Id) };
        public static JObject jObjRandomMoments;

        public static IDictionary<string, Timer> _timerRandomEvent = new Dictionary<string, Timer>();
        public static IDictionary<string, Timer> _timerBirthdayAnnouncement = new Dictionary<string, Timer>();//birthday reminder timer

        public class Status{
            public static object[,] arrRandomActivity = {
                {$"violin {Emoji.violin}","I'm playing with my violin instrument now. Wanna hear me to play some music?", UserStatus.Online},
                {"with Masaru","I'm playing with my Masaru right now. We're usually playing music together on the afternoon \uD83D\uDE0A", UserStatus.Online},
                {"at Home",$"I'm at my home right now. I have violin lesson to attend after this.", UserStatus.Online},
                {$"at witch's world {Emoji.broom}","I'm at the witch's world right now.", UserStatus.Online},
                //{"with homework \uD83D\uDCDA","I'm doing my homework right now.", UserStatus.DoNotDisturb},
                {"with Rere the fairy",$"I'm playing with my fairy: Rere right now.", UserStatus.Online},
                {"with Hana","I'm playing with Hana now.", UserStatus.Online},
                {"with Marina","I'm playing with Marina now. We're planning to plant some flower together.", UserStatus.Online},
                {"with Ojamajo Trading Card",$"I'm playing Ojamajo Trading Card right now.", UserStatus.Online}
            };

            public static object[,] arrPlayingWith = {
                { "with Doremi", $"I'm playing with {MentionUtils.MentionUser(Doremi.Id)} right now.", UserStatus.Online},
                {"with Aiko",$"I'm playing with {MentionUtils.MentionUser(Aiko.Id)} right now. " +
                "Please come and join us to make takoyaki together, will you?", UserStatus.Online},
                { "with Onpu", $"I'm playing with {MentionUtils.MentionUser(Onpu.Id)} right now.", UserStatus.Online },
                { "with Momoko", $"I'm playing with {MentionUtils.MentionUser(Momoko.Id)} right now.", UserStatus.Online }
            };

            public static string birthdayActivity = $"at Hazuki birthday {Config.Emoji.birthdayCake}";

            public static string currentActivity = "";
            public static string currentActivityReply = "";
            public static UserStatus userStatus = UserStatus.Online;
            public static Boolean isPlayingWith = false;

            public static Boolean isBirthday()
            {
                Boolean isBirthday = false;
                if (DateTime.Now.ToString("dd") == Config.Hazuki.birthdayDate.ToString("dd") &&
                DateTime.Now.ToString("MM") == Config.Hazuki.birthdayDate.ToString("MM") &&
                Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour)
                {
                    isBirthday = true;
                }
                return isBirthday;
            }

        }

        public static string EmbedName = "Hazuki Bot";
        public static string EmbedNameError = "Hazuki 404";
        public static string EmbedAvatarUrl = "https://cdn.discordapp.com/avatars/655307117128974346/5411e9a959f73069151b94862f8efa56.png";
    }

    public class Aiko
    {
        //public static ulong Id = 666574244725129216;
        public static ulong Id = 663612449341046803;//original
        public static string Token { get; set; }
        public static double Randomeventinterval { get; set; }
        public static Color EmbedColor = Color.Blue;
        public static DateTime birthdayDate = DateTime.ParseExact("14/11/1990", "dd/MM/yyyy" ,CultureInfo.InvariantCulture, DateTimeStyles.None);
        public static int birthdayCalculatedYear = Convert.ToInt32(DateTime.Now.ToString("yyyy")) - Convert.ToInt32(birthdayDate.ToString("yyyy"));
        public static string[] PrefixParent = { "ai!", "aiko!", MentionUtils.MentionUser(Id) };
        public static JObject jObjRandomMoments;

        public static IDictionary<string, Timer> _timerRandomEvent = new Dictionary<string, Timer>();
        public static IDictionary<string, Timer> _timerBirthdayAnnouncement = new Dictionary<string, Timer>();//birthday reminder timer

        public static IDictionary<string, Boolean> hasSpookyAikoInvader = new Dictionary<string, Boolean>();
        public static IDictionary<string, Timer> _timerSpookyInvader = new Dictionary<string, Timer>();//spooky aiko self executing timer

        public class Status
        {
            public static object[,] arrRandomActivity = {
                {"with takoyaki",$"I'm making some delicious takoyaki right now with {MentionUtils.MentionUser(Doremi.Id)}, {MentionUtils.MentionUser(Hazuki.Id)}, {MentionUtils.MentionUser(Onpu.Id)} and {MentionUtils.MentionUser(Momoko.Id)}. " +
                        $"I will give you some when it's ready.", UserStatus.Online},
                {"with friends","I'm playing with my classmates. We're gonna play some tennis table.", UserStatus.Online},
                {"with Nobuko","I'm playing with Nobuko right now. Currently the latest Detective Boy Tatekawa series was pretty cool.", UserStatus.Online},
                {"harmonica","I'm playing with my harmonica instrument now. Wanna hear me to play some music?", UserStatus.Online},
                {$"at witch's world {Emoji.broom}","I'm at the witch's world right now.", UserStatus.Online},
                {"with sweet potatoes","Sweet potatoes is one of my favorite foods, I just love to eat it so much.", UserStatus.Online},
                //{"with homework \uD83D\uDCDA","I'm doing my homework right now.", UserStatus.DoNotDisturb},
                {"with Mimi the fairy",$"I'm playing with my fairy: Mimi right now.", UserStatus.Online},
                {"with Hana","I'm playing with Hana now.", UserStatus.Online},
                {"with Ojamajo Trading Card",$"I'm playing Ojamajo Trading Card right now.", UserStatus.Online}
            };

            public static object[,] arrPlayingWith = {
                { "with Doremi", $"I'm playing with {MentionUtils.MentionUser(Doremi.Id)} right now.", UserStatus.Online},
                { "with Hazuki", $"I'm playing with {MentionUtils.MentionUser(Hazuki.Id)} right now." , UserStatus.Online},
                { "with Onpu", $"I'm playing with {MentionUtils.MentionUser(Onpu.Id)} right now.", UserStatus.Online },
                { "with Momoko", $"I'm playing with {MentionUtils.MentionUser(Momoko.Id)} right now.", UserStatus.Online }
            };

            public static string birthdayActivity = $"at Aiko birthday {Config.Emoji.birthdayCake}";

            public static string currentActivity = "";
            public static string currentActivityReply = "";
            public static UserStatus userStatus = UserStatus.Online;
            public static Boolean isPlayingWith = false;

            public static Boolean isBirthday()
            {
                Boolean isBirthday = false;
                if (DateTime.Now.ToString("dd") == Config.Aiko.birthdayDate.ToString("dd") &&
                DateTime.Now.ToString("MM") == Config.Aiko.birthdayDate.ToString("MM") &&
                Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour)
                {
                    isBirthday = true;
                }
                return isBirthday;
            }
        }

        public static string EmbedName = "Aiko Bot";
        public static string EmbedNameError = "Aiko 404";
        public static string EmbedAvatarUrl = "https://cdn.discordapp.com/avatars/663612449341046803/ccb59d60b2206d6a3749a2836b8e6e80.png";
    }

    public class Onpu
    {
        //public static ulong Id = 666575360191627265;
        public static ulong Id = 663615334150045706;//original
        public static string Token { get; set; }
        public static double Randomeventinterval { get; set; }
        public static Color EmbedColor = Color.Purple;
        public static DateTime birthdayDate = DateTime.ParseExact("03/03/1991", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
        
        public static int birthdayCalculatedYear = Convert.ToInt32(DateTime.Now.ToString("yyyy")) - Convert.ToInt32(birthdayDate.ToString("yyyy"));
        public static string[] PrefixParent = { "on!", "onpu!", MentionUtils.MentionUser(Id) };
        public static JObject jObjRandomMoments;

        public static IDictionary<string, Timer> _timerRandomEvent = new Dictionary<string, Timer>();
        public static IDictionary<string, Timer> _timerBirthdayAnnouncement = new Dictionary<string, Timer>();//birthday reminder timer

        public class Status
        {
            public static object[,] arrRandomActivity = {
                {"at misora tv studio","I'm working on the studio right now. Feel free to come and watch me on some drama performances.", UserStatus.DoNotDisturb},
                {"at misora radio station","I'm currently broadcasting at radio station right now. Stay tune for more daily info.", UserStatus.DoNotDisturb},
                {$"at witch's world {Emoji.broom}","I'm at the witch's world right now.", UserStatus.Online},
                {"flute","I'm playing with my flute instrument now. Wanna hear me to play some music?", UserStatus.Online},
                //{"with homework \uD83D\uDCDA","I'm doing my homework right now.", UserStatus.DoNotDisturb},
                {"with Roro the fairy",$"I'm playing with my fairy: Roro right now.", UserStatus.Online},
                {"with Hana","I'm playing with Hana now.", UserStatus.Online},
                {"with Ojamajo Trading Card",$"I'm playing Ojamajo Trading Card right now.", UserStatus.Online}
            };

            public static object[,] arrPlayingWith = {
                { "with Doremi", $"I'm playing with {MentionUtils.MentionUser(Doremi.Id)} right now.", UserStatus.Online},
                { "with Hazuki", $"I'm playing with {MentionUtils.MentionUser(Hazuki.Id)} right now." , UserStatus.Online},
                {"with Aiko",$"I'm playing with {MentionUtils.MentionUser(Aiko.Id)} right now. " +
                "Please come and join us to make takoyaki together, will you?", UserStatus.Online},
                { "with Momoko", $"I'm playing with {MentionUtils.MentionUser(Momoko.Id)} right now.", UserStatus.Online }
            };

            public static string birthdayActivity = $"at Onpu birthday {Config.Emoji.birthdayCake}";

            public static string currentActivity = "";
            public static string currentActivityReply = "";
            public static UserStatus userStatus = UserStatus.Online;
            public static Boolean isPlayingWith = false;

            public static Boolean isBirthday()
            {
                Boolean isBirthday = false;
                if (DateTime.Now.ToString("dd") == Config.Onpu.birthdayDate.ToString("dd") &&
                DateTime.Now.ToString("MM") == Config.Onpu.birthdayDate.ToString("MM") &&
                Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour)
                {
                    isBirthday = true;
                }
                return isBirthday;
            }
        }

        public static string EmbedName = "Onpu Bot";
        public static string EmbedNameError = "Onpu 404";
        public static string EmbedAvatarUrl = "https://cdn.discordapp.com/avatars/663615334150045706/57116d111b518b8a29de8efb9438fa4b.png";
    }

    public class Momoko
    {
        //public static ulong Id = 666576136045592586;
        public static ulong Id = 663615454140432414;//original
        public static string Token { get; set; }
        public static double Randomeventinterval { get; set; }
        public static Color EmbedColor = new Color(234, 211, 57);
        public static DateTime birthdayDate = DateTime.ParseExact("06/05/1990", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
        //public static DateTime birthdayDate = DateTime.ParseExact("24/12/1990", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);

        public static int birthdayCalculatedYear = Convert.ToInt32(DateTime.Now.ToString("yyyy")) - Convert.ToInt32(birthdayDate.ToString("yyyy"));

        public static string[] PrefixParent = { "mo!", "momoko!", MentionUtils.MentionUser(Id) };
        public static JObject jObjRandomMoments;

        public static IDictionary<string, Timer> _timerRandomEvent = new Dictionary<string, Timer>();
        public static IDictionary<string, Timer> _timerBirthdayAnnouncement = new Dictionary<string, Timer>();//birthday reminder timer

        public class Status
        {
            public static object[,] arrRandomActivity = {
                {$"at witch's world {Emoji.broom}","I'm at the witch's world right now.",UserStatus.Online},
                {$"guitar {Emoji.guitar}","I'm playing with my guitar instrument now. Wanna hear me to play some music?",UserStatus.Online},
                //{"with homework \uD83D\uDCDA","I'm doing my homework right now.",UserStatus.DoNotDisturb},
                {"with Nini",$"I'm playing with my fairy: Nini right now.",UserStatus.Idle},
                {"with Hana","I'm playing with Hana now.", UserStatus.Online},
                {"baseball \u26BE",$"I'm playing baseball right now.",UserStatus.Idle},
                {"with Hana","I'm playing with Hana now.",UserStatus.Idle},
                {"with Tamaki",$"I'm playing with Tamaki right now.",UserStatus.Idle},
                {"with Ojamajo Trading Card",$"I'm playing Ojamajo Trading Card right now.",UserStatus.Idle}
            };

            public static object[,] arrPlayingWith = {
                { "with Doremi", $"I'm playing with {MentionUtils.MentionUser(Doremi.Id)} right now.", UserStatus.Online},
                { "with Hazuki", $"I'm playing with {MentionUtils.MentionUser(Hazuki.Id)} right now." , UserStatus.Online},
                {"with Aiko",$"I'm playing with {MentionUtils.MentionUser(Aiko.Id)} right now. " +
                "Please come and join us to make takoyaki together, will you?", UserStatus.Online},
                { "with Onpu", $"I'm playing with {MentionUtils.MentionUser(Onpu.Id)} right now.", UserStatus.Online }
            };

            public static string birthdayActivity = $"at Momoko birthday {Config.Emoji.birthdayCake}";


            public static string currentActivity = "";
            public static string currentActivityReply = "";
            public static UserStatus userStatus = UserStatus.Online;
            public static Boolean isPlayingWith = false;

            public static Boolean isBirthday()
            {
                Boolean isBirthday = false;
                if (DateTime.Now.ToString("dd") == Config.Momoko.birthdayDate.ToString("dd") &&
                DateTime.Now.ToString("MM") == Config.Momoko.birthdayDate.ToString("MM") &&
                Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour)
                {
                    isBirthday = true;
                }
                return isBirthday;
            }
        }

        public static string EmbedName = "Momoko Bot";
        public static string EmbedNameError = "Momoko 404";
        public static string EmbedAvatarUrl = "https://cdn.discordapp.com/avatars/663615454140432414/37f52c6178d6ca94c796db50c7511c74.png";
        public static IDictionary<string, Timer> timerProcessBakery = new Dictionary<string, Timer>();
        public static IDictionary<string, Boolean> isRunningBakery = new Dictionary<string, Boolean>();
    }

    public class Pop
    {
        public static ulong Id = 677042851426861056;//original
        public static string Token { get; set; }
        public static double Randomeventinterval { get; set; }
        public static Color EmbedColor = new Color(185, 70, 75);
        public static DateTime birthdayDate = DateTime.ParseExact("09/09/1994", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
        public static int birthdayCalculatedYear = Convert.ToInt32(DateTime.Now.ToString("yyyy")) - Convert.ToInt32(birthdayDate.ToString("yyyy"));

        public static string[] PrefixParent = { "pop!", MentionUtils.MentionUser(Id) };
        public static JObject jObjRandomMoments;

        public static string[,] arrRandomActivity = {
            {$"at misora elementary school {Emoji.school}" , "I'm still at school right now."},
            {"at maho dou","I'm working on maho-dou right now. Please come to the shop any time"},
            {$"piano {Emoji.piano}","I'm playing piano right now. Do you want to hear me playing the piano?"},
            {"with homework \uD83D\uDCDA","I'm doing my homework right now."},
            {"with Fafa the fairy",$"I'm playing with my fairy: Fafa right now."}
        };
        public static int indexCurrentActivity { get; set; }

        public static string EmbedName = "Pop Bot";
        public static string EmbedNameError = "Pop 404";
        public static string EmbedAvatarUrl = "https://cdn.discordapp.com/emojis/651063629403127808.png?v=1";
    }

    public static class Emoji
    {
        public static string doremi = "<:Doremi:651062436866293760>";
        public static string hazuki = "<:Hazuki:651062978854125589>";
        public static string onpu = "<:Onpu:651063415514857492>";
        public static string dabzuki = "<:hazuki_dab:695162857222045737>";
        public static string drool = "\uD83E\uDD24";
        public static string steak = "\uD83E\uDD69";
        public static string school = "\uD83C\uDFEB";
        public static string piano = "\uD83C\uDFB9";
        public static string violin = "\uD83C\uDFBB";
        public static string guitar = "\uD83C\uDFB8";
        public static string clap = "\uD83D\uDC4F";
        public static string birthdayCake = "\uD83C\uDF82";
        public static string partyPopper = "\uD83C\uDF89";
        public static string broom = "\uD83E\uDDF9";
    }

    public static class Music
    {
        //public static List<string> arrMusicList = new List<string>();
        public static JObject jobjectfile { get; set; }
        //public static List<LavaTrack> storedLavaTrack = new List<LavaTrack>();
        //public static IDictionary<string, List<LavaTrack>> storedLavaTrack = new Dictionary<string, List<LavaTrack>>();
        //public static IDictionary<string,List<string>> queuedTrack = new Dictionary<string,List<string>>();
        public static Byte repeat = 2;//0:repeat off;1:repeat one;2:repeat all
    }

}
