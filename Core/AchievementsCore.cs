using Discord;
using Discord.Addons.Interactive;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace OjamajoBot
{
    class AchievementsCore
    {
        public static IDictionary<string, string[]> achievementList = new Dictionary<string, string[]>();
        public static IDictionary<string, string[]> achievementListHidden = new Dictionary<string, string[]>();

        public const string TYPE_NORMAL = "normal";
        public const string TYPE_HIDDEN = "hidden";

        /*
         * key types that will display: 
         * _min = will show on the progress
         * _bool = not showing the progress
         */

        public static Boolean achievementDataExists(ulong guildId, ulong clientId)
        {
            bool ret = false;
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headAchievementsConfigFolder}/{clientId}.json";
            if (File.Exists(playerDataDirectory))
            {
                ret = true;
            }

            return ret;
        }

        public static EmbedBuilder printTemplate(Color color,string title,string description)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithColor(color)
                .WithTitle(title)
                .WithDescription(description);
            return eb;
        }

        public static PaginatedMessage printAchievementsStatus(Color color, ulong guildId, ulong clientId,
            string username, string thumbnailUrl)
        {
            Boolean achievementDataExists = false;

            PaginatedAppearanceOptions pao = new PaginatedAppearanceOptions();
            pao.JumpDisplayOptions = JumpDisplayOptions.Never;
            pao.DisplayInformationIcon = false;

            List<string> pageContent = new List<string>();

            string title = "";
            string tempVal = title;
            int currentIndex = 0;

            for (int i = 0; i < achievementList.Count; i++)
            {
                string achievementKey = achievementList.ElementAt(i).Key;
                string[] arrMaster = achievementList[achievementKey];//format: title, description
                string mTitle = arrMaster[0];
                string mDesc = arrMaster[1];

                if (!achievementDataExists)
                    tempVal += ":white_check_mark: ";
                else
                    tempVal += ":x: ";

                tempVal += $"**{mTitle}**\n{mDesc}\n";

                if (i == achievementList.Count - 1)
                {
                    pageContent.Add(tempVal);
                }
                else
                {
                    if (currentIndex < 9) currentIndex++;
                    else
                    {
                        pageContent.Add(tempVal);
                        currentIndex = 0;
                        tempVal = title;
                    }
                }
            }

            var pager = new PaginatedMessage
            {
                Title = $"**{username}'s Achievements**\n",
                Pages = pageContent,
                Color = color,
                Author = new EmbedAuthorBuilder()
                {
                    Name = GlobalFunctions.UppercaseFirst(username),
                    IconUrl = thumbnailUrl
                },
                Options = pao
            };

            return pager;
        }

        public static string getParentProperty(string key)
        {
            string ret = "minigame";
            if (key.ToLower().StartsWith("minigame"))
                ret = "minigame";
            else if (key.ToLower().StartsWith("status"))
                ret = "status";
            else if (key.ToLower().StartsWith("card"))
                ret = "card";
            else if (key.ToLower().StartsWith("shop"))
                ret = "shop";
            else if (key.ToLower().StartsWith("garden"))
                ret = "garden";
            return ret;
        }

        //trigger the achievements
        public static void triggerAchievements(ulong guildId, ulong clientId, string achievementsId)
        {
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headAchievementsConfigFolder}/{clientId}.json";
            JObject arrAchievements = JObject.Parse(File.ReadAllText(playerDataDirectory));
            string parent = getParentProperty(achievementsId);
        }

        public static void triggerHiddenAchievements(ulong guildId, ulong clientId, string achievementsId)
        {
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headAchievementsConfigFolder}/{clientId}.json";

        }

        public static class Minigame
        {
            static Minigame()
            {
                //init achievement list
                achievementList["minigame1_min"] = new string[] { "Gamezone I", "Get 1000 scores from minigame."};
                achievementList["minigame2_min"] = new string[] { "Gamezone II", "Get 5000 scores from minigame."};
                achievementListHidden["minigameh1_min"] = new string[] { "Gamezone III", "Get 10000 scores from minigame."};
            }

            public class Check
            {
                public static Tuple<string,string,string> scoreGroup(int score)
                {
                    string key;
                    //scoring group
                    if (score >= 10000)
                    {
                        key = "minigameh1_min";
                    }
                    else if (score >= 5000)
                    {
                        key = "minigame2_min";
                    }
                    else if (score >= 1000)
                    {
                        key = "minigame1_min";
                    } else
                    {
                        key = "";
                    }

                    if (key != "")
                    {
                        return Tuple.Create(key, achievementList[key][0], TYPE_NORMAL);
                    } else
                    {
                        return Tuple.Create("","","");
                    }
                    
                }
            }
        }

        public static class Status
        {
            static Status()
            {
                //init achievement list
                achievementList["status1_min"] = new string[] { "Rookie", "Reach level 2."};
                achievementList["status2_min"] = new string[] { "Veteran", "Reach level 5."};

                achievementListHidden["status_hidden1_min"] = new string[] { "EXPerience is the best teacher", "Get 500 EXP."};
                achievementListHidden["status_hidden2_min"] = new string[] { "Status Peeker", "Use the card status command 50 times."};
                achievementListHidden["status_hidden3_min"] = new string[] { "The inventorist", "Use the card inventory command 50 times."};
            }

            public class Check
            {
                public static Tuple<string, string, string> levelGroup(int level)
                {
                    string key;
                    //scoring group
                    if (level == 5)
                    {
                        key = "status2_min";
                    }
                    else if (level == 2)
                    {
                        key = "status1_min";
                    }
                    else
                    {
                        key = "";
                    }

                    if (key != "")
                    {
                        return Tuple.Create(key, achievementList[key][0], TYPE_NORMAL);
                    }
                    else
                    {
                        return null;
                    }
                }

                public static Tuple<string, string, string> expGroup(int exp)
                {
                    string key;
                    //scoring group
                    if (exp >= 500)
                    {
                        key = "statush1_min";
                    }
                    else
                    {
                        key = "";
                    }

                    if (key != "")
                    {
                        return Tuple.Create(key, achievementListHidden[key][0], TYPE_HIDDEN);
                    }
                    else
                    {
                        return null;
                    }
                }

                public static Tuple<string, string, string> statusGroup(int val)
                {
                    string key;
                    //scoring group
                    if (val >= 50)
                    {
                        key = "statush2_min";
                    }
                    else
                    {
                        key = "";
                    }

                    if (key != "")
                    {
                        return Tuple.Create(key, achievementListHidden[key][0], TYPE_HIDDEN);
                    }
                    else
                    {
                        return null;
                    }
                }

                public static Tuple<string, string, string> inventoryGroup(int val)
                {
                    string key;
                    //scoring group
                    if (val >= 50)
                    {
                        key = "status_hidden3_min";
                    }
                    else
                    {
                        key = "";
                    }

                    if (key != "")
                    {
                        return Tuple.Create(key, achievementListHidden[key][0], TYPE_HIDDEN);
                    }
                    else
                    {
                        return null;
                    }
                }
            }

        }

        public static class Card
        {

            static Card()
            {
                //init achievement list
                achievementList["card1_bool"] = new string[] { "The First Card", "Capture your first card." };
                achievementList["card2_min"] = new string[] { "Cardtastic", "Collect 300 cards." };
                achievementList["card3_bool"] = new string[] { "Steak for you", "Complete all Doremi Card Pack." };
                achievementList["card4_bool"] = new string[] { "The violinist", "Complete all Hazuki Card Pack." };
                achievementList["card5_bool"] = new string[] { "The Bravest Pack", "Complete all Aiko Card Pack." };
                achievementList["card6_bool"] = new string[] { "Onpu Biggest Fan", "Complete all Onpu Card Pack." };
                achievementList["card7_bool"] = new string[] { "It's American Traditional!", "Complete all Momoko Card Pack." };
                achievementList["card8_bool"] = new string[] { "Spectialist", "Complete all Other/Special Card Pack." };

                achievementListHidden["card_hidden1_bool"] = new string[] { "We are the pureleine", "Remove bad card for first time." };
                achievementListHidden["card_hidden2_bool"] = new string[] { "Seedtastic", "Remove bad card and get your first magic seed reward." };
            }
        }

        //name,image url, type, minimum trigger
        public static class Shop
        {
            
            static Shop()
            {
                //init achievement list
                achievementList["shop1_bool"] = new string[]{"Shopper","Buy item from the shop for first time."};
                achievementList["shop2_min"] = new string[]{"Shoptastic", "Buy item from the shop 10 times." };

                achievementListHidden["shop_hidden1_min"] = new string[] { "Window Shopping", "Visit the Doremi Trading Card Shop 50 times." };
            }

        }

        public static class Garden
        {
            static Garden()
            {
                achievementList["garden1_bool"] = new string[] { "Sprout", "Water the plant for first time." };
                achievementList["garden2_min"] = new string[] { "Gardener", "Collect 100 magic seeds." };

                achievementListHidden["gardenh1_bool"] = new string[] { "The royalist", "Get your first royal seeds." };
                achievementListHidden["gardenh2_min"] = new string[] { "I love magic seeds", "Collect 500 magic seeds." };
            }
        }
    }
}
