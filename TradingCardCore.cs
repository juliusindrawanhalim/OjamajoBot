using Discord;
using Discord.Addons.Interactive;
using Discord.WebSocket;
using Lavalink4NET.Statistics;
using Newtonsoft.Json.Linq;
using Spectacles.NET.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OjamajoBot
{
    public class TradingCardCore
    {
        public static string version = "1.19";
        public static string propertyId = "trading_card_spawn_id";
        public static string propertyCategory = "trading_card_spawn_category";
        public static string propertyToken = "trading_card_spawn_token";
        public static string propertyMystery = "trading_card_spawn_mystery";

        public static int captureRateNormal = 9;
        public static int captureRatePlatinum = 5;
        public static int captureRateMetal = 3;
        public static int captureRateOjamajos = 2;
        public static int captureRateSpecial = 4;

        public static int spawnRateNormal = 10;
        public static int spawnRatePlatinum = 5;
        public static int spawnRateMetal = 2;
        public static int spawnRateOjamajos = 1;

        public static int maxSpecial = 37;

        public static string roleCompletionistSpecial = "Ojamajo Card Special Badge";
        public static Color roleCompletionistColor = new Discord.Color(4, 173, 18);
        public static string imgCompleteAllCardSpecial = $"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/badge/badge_special.png";

        public static EmbedBuilder printUpdatesNote()
        {

            //return new EmbedBuilder()
            //    .WithColor(Config.Doremi.EmbedColor)
            //    .WithTitle($"Ojamajo Trading Card - Update {version} - 30.05.20")
            //    .WithDescription($"-:new: Added bad cards. This card are attached upon card spawn and extremely **dangerous**. It need to be removed first before using card capture commands!")
            //    .AddField("How many type of bad cards?","There are 3 type of bad cards:\n" +
            //    "-curse: Steal one of your card after catch attempt. A bonus card will be rewarded upon removed.\n" +
            //    "-failure: Dropped your card catch rate into 0%. A bonus card will be rewarded upon removed.\n" +
            //    "-seeds: Steal your magic seeds after catch attempt. A magic seeds will be rewarded upon removed.")
            //    .AddField("How to remove bad cards?", "You can remove the bad cards with **do!card pureleine**. After seeing the question you need to answer with **do!card pureleine <answer>** commands.")
            //    .AddField("How to notice bad cards?", " Bad cards are marked on the bottom part where there'll be a logo marked and id written and followed by the bad card type. An example of bad card marker will be shown below:")
            //    .WithImageUrl("https://cdn.discordapp.com/attachments/706770454697738300/716045344135315566/unknown.png");

            //return new EmbedBuilder()
            //    .WithColor(Config.Doremi.EmbedColor)
            //    .WithTitle($"Ojamajo Trading Card - Update {version} - 31.05.20")
            //    .WithDescription("-Bad card spawn rate has been lowered down\n" +
            //    "-Entering new month: June [New card spawn will appear]\n" +
            //    "-Pureleine command update: question difficulty has been increased\n" +
            //    "-Pureleine command update: seeds reward are now randomized between 1 - 3\n" +
            //    "-Pureleine command update: answering the wrong question will losing up a capture chance on that spawn turn\n" +
            //    "-Magic seeds reward image has been resized");

            //return new EmbedBuilder()
            //.WithColor(Config.Doremi.EmbedColor)
            //.WithTitle($"Ojamajo Trading Card - Update {version} - 12.06.20")
            //.WithDescription("-:tools: Card status & inventory command can now display their information with given mentioned username parameter.\n" +
            //"Example: **do!card inventory <username>**, **do!card status <username>**");

            //return new EmbedBuilder()
            //.WithColor(Config.Doremi.EmbedColor)
            //.WithTitle($"Ojamajo Trading Card - Update {version} - 16.06.20")
            //.WithDescription("-Card capture command that is fail/error now have auto deletion message within 10 seconds after message has been shown.\n" +
            //"-Card status command now have total percentage displayed on each card pack\n" +
            //"-Card save command with **do!card save**: Now you can make your trading card save file that can be used as a backup & continued on with card register command on another server.\n" +
            //"-Card trade command now have auto deletion message system to keep the channel clean.");

            //return new EmbedBuilder()
            //.WithColor(Config.Doremi.EmbedColor)
            //.WithTitle($"Ojamajo Trading Card - Update {version} - 17.06.20")
            //.AddField(":tools: **Updates:**",
            //"-More cleaner card category command: **capture**/**shop**/**trade**/**trade process** command: failed/error/timeout message will automatically deleted within 10-15 seconds.\n" +
            //"-**card register** command that has been loaded now have new rules applied to keep the progress balanced: you can't catch any card on the current card spawn turn.\n" +
            //"-**card inventory** command will now sort all the card in order\n" +
            //"-Mystery card image has been updated")
            //.AddField(":beetle: **Bug Fix:**",
            //"-**card trade process** command fix: the user list can now be displayed.")
            //.AddField(":new: **New Features**:",
            //"-Card data delete command with **do!card delete**: Want to start over from beginning? Now you can delete your card data progress on current server and start over again. " +
            //"Please read some note & rules that applied before executing this command!");

            //return new EmbedBuilder()
            //.WithColor(Config.Doremi.EmbedColor)
            //.WithTitle($"Ojamajo Trading Card - Update {version} - 22.06.20")
            //.AddField(":tools: **Updates:**",
            //"-**card inventory** is now displaying the percentage progression status and fixed the unsorted order on each page.\n" +
            //"-**card trade** now only displaying the available user on the server.")
            //.AddField(":beetle: Bug fix",
            //"-**card trade** and **card trade process** : can only be executed one time per instance at trade interactive.")
            //.AddField("New Features",
            //"-**card timer**: now you can check the approximate of next card spawn timer.");

            //return new EmbedBuilder()
            //.WithColor(Config.Doremi.EmbedColor)
            //.WithTitle($"Ojamajo Trading Card - Update {version} - 25.06.20")
            //.AddField(":tools: **Updates:**",
            //"**card trade** and **card trade process** : you can now trade **ojamajos** category card.")
            //.AddField(":beetle: Bug fix",
            //"-**card trade** : issues resolved where you can't choose the user/exit.");

            //return new EmbedBuilder()
            //.WithColor(Config.Doremi.EmbedColor)
            //.WithTitle($"Ojamajo Trading Card - Update {version} - 27.06.20")
            //.AddField(":tools: **Command updates:**",
            //"**card inventory**: can now be called with category command upon with other username. " +
            //"Example: **do!card inventory normal @someone**")
            //.AddField(":new: New Command",
            //"-**card checklist** or **card list**: now you can check the card checklist.")
            //.AddField(":new: Garden maho-dou",
            //"-**do!daily** command now track your plant growth progression.\n" +
            //"A 100% growing plant progression will reward you 1 royal seeds and reset its progress into 0%.\n" +
            //$"-new command: **do!garden progress** to check your plant growth progress.\n"+
            //"-new command: **do!garden weather** to check the current weather.\n" +
            //"-4 weather available: **sunny**/**cloudy**/**rainy**/**thunder storm**. " +
            //"Each weather will affect your plant growing progression. The weather will change for every 2 hours.\n" +
            //"*More usage & information about royal seeds will be added on upcoming updates soon.");

            return new EmbedBuilder()
            .WithColor(Config.Doremi.EmbedColor)
            .WithTitle($"Ojamajo Trading Card - Update {version} - 05.07.20")
            .AddField(":tools: **Updates:**",
            "**daily commands**: Fixed the growth rate progress for each weather.");
        }

        public static int getPlayerRank(int exp)
        {
            //0 = 1
            //100 = 2
            //200 = 3
            //300 = 4
            int rank = 1;
            if (exp >= 100) rank = (int)Math.Ceiling(Convert.ToDouble(exp)/100);
            
            if (exp == 100) rank = 2;
            if (rank >= 5) rank = 5;
            return rank;
        }

        public static PaginatedMessage printInventoryTemplate(Color color, string pack, string parent, string category,
            JObject jObjTradingCardList, JArray arrData, int maxAmount,string username, string thumbnailUrl)
        {
            PaginatedAppearanceOptions pao = new PaginatedAppearanceOptions();
            pao.JumpDisplayOptions = JumpDisplayOptions.Never;
            pao.DisplayInformationIcon = false;

            List<string> pageContent = new List<string>();
            var arrList = (JArray)arrData;
            JArray sorted = new JArray(arrList.OrderBy(e=>e));
            arrList = sorted;

            double calculated = (double)arrList.Count / maxAmount * 100;
            string percentageCompleted = $"({Math.Round(calculated)}%)";

            string title = $"**Progress: {arrList.Count}/{maxAmount} {percentageCompleted}**\n";

            string tempVal = title;
            int currentIndex = 0;
            for (int i = 0; i < arrList.Count; i++)
            {
                string cardId = arrList[i].ToString();
                string name = jObjTradingCardList[parent][category][cardId]["name"].ToString();
                string url = jObjTradingCardList[parent][category][cardId]["url"].ToString();
                tempVal += $"[{arrList[i]} - {name}]({url})\n";

                if (i == arrList.Count - 1) {
                    String output = string.Join("\n", tempVal.Split("\n").OrderBy(s => s));

                    //pageContent.Add(tempVal); //original
                    pageContent.Add(output);
                } else
                {
                    if (currentIndex < 14) currentIndex++;
                    else
                    {
                        String output = string.Join("\n", tempVal.Split("\n").OrderBy(s => s));

                        //pageContent.Add(tempVal); //original
                        pageContent.Add(output);
                        currentIndex = 0;
                        tempVal = title;
                    }
                }
            }

            var pager = new PaginatedMessage
            {
                Title = $"**{GlobalFunctions.UppercaseFirst(parent)} {GlobalFunctions.UppercaseFirst(category)} Card Inventory**\n",
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

        public static PaginatedMessage printChecklistTemplate(Color color, string pack, string parent, string category,
            JObject jObjTradingCardList, JArray arrData, int maxAmount, string username, string thumbnailUrl)
        {
            PaginatedAppearanceOptions pao = new PaginatedAppearanceOptions();
            pao.JumpDisplayOptions = JumpDisplayOptions.Never;
            pao.DisplayInformationIcon = false;

            List<string> pageContent = new List<string>();
            var arrList = (JArray)arrData;

            try
            {
                var arrListMaster = ((JObject)jObjTradingCardList[parent][category]).Properties().ToList();
                JObject sorted = new JObject(arrListMaster.OrderBy(e => e.Name));
                var arrListMasterSorted = sorted.Properties().ToList();

                double calculated = (double)arrList.Count / maxAmount * 100;
                string percentageCompleted = $"({Math.Round(calculated)}%)";

                string title = $"**Progress: {arrList.Count}/{maxAmount} {percentageCompleted}**\n";

                string tempVal = title;
                int currentIndex = 0;
                for (int i = 0; i < arrListMasterSorted.Count; i++)
                {
                    string cardId = arrListMasterSorted[i].Name;
                    string name = arrListMasterSorted[i].Value["name"].ToString();
                    string url = arrListMasterSorted[i].Value["url"].ToString();

                    var owned = arrList.ToString().Contains(cardId);
                    if (owned)
                        tempVal += ":white_check_mark: ";
                     else
                        tempVal += ":x: ";
                    
                    tempVal+= $"[{cardId} - {name}]({url})\n";

                    if (i == arrListMasterSorted.Count - 1)
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            var pager = new PaginatedMessage
            {
                Title = $"**{GlobalFunctions.UppercaseFirst(parent)} {GlobalFunctions.UppercaseFirst(category)} Card Checklist**\n",
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

        public static EmbedBuilder printCardBoostStatus(Color color, string guildId, ulong userId, string username)
        {
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{userId}.json";
            JObject arrInventory = JObject.Parse(File.ReadAllText(playerDataDirectory));

            string doremiBoost = ""; string hazukiBoost = ""; string aikoBoost = "";
            string onpuBoost = ""; string momokoBoost = ""; string specialBoost = "";

            //doremi boost
            if (Convert.ToInt32(arrInventory["boost"]["doremi"]["normal"].ToString()) > 0)
                doremiBoost = $"**normal: {Convert.ToInt32(arrInventory["boost"]["doremi"]["normal"].ToString())*10}%**\n";
            if (Convert.ToInt32(arrInventory["boost"]["doremi"]["normal"].ToString()) > 0)
                doremiBoost += $"**platinum: {Convert.ToInt32(arrInventory["boost"]["doremi"]["platinum"].ToString()) * 10}%**\n";
            if (Convert.ToInt32(arrInventory["boost"]["doremi"]["normal"].ToString()) > 0)
                doremiBoost += $"**metal: {Convert.ToInt32(arrInventory["boost"]["doremi"]["metal"].ToString()) * 10}%**\n";
            if (Convert.ToInt32(arrInventory["boost"]["doremi"]["ojamajos"].ToString()) > 0)
                doremiBoost += $"**ojamajos: {Convert.ToInt32(arrInventory["boost"]["doremi"]["ojamajos"].ToString()) * 10}%**";

            //hazuki boost
            if (Convert.ToInt32(arrInventory["boost"]["hazuki"]["normal"].ToString()) > 0)
                hazukiBoost = $"**normal: {Convert.ToInt32(arrInventory["boost"]["hazuki"]["normal"].ToString()) * 10}%**\n";
            if (Convert.ToInt32(arrInventory["boost"]["hazuki"]["normal"].ToString()) > 0)
                hazukiBoost += $"**platinum: {Convert.ToInt32(arrInventory["boost"]["hazuki"]["platinum"].ToString()) * 10}%**\n";
            if (Convert.ToInt32(arrInventory["boost"]["hazuki"]["normal"].ToString()) > 0)
                hazukiBoost += $"**metal: {Convert.ToInt32(arrInventory["boost"]["hazuki"]["metal"].ToString()) * 10}%**\n";
            if (Convert.ToInt32(arrInventory["boost"]["hazuki"]["ojamajos"].ToString()) > 0)
                hazukiBoost += $"**ojamajos: {Convert.ToInt32(arrInventory["boost"]["hazuki"]["ojamajos"].ToString()) * 10}%**";

            //aiko boost
            if (Convert.ToInt32(arrInventory["boost"]["aiko"]["normal"].ToString()) > 0)
                aikoBoost = $"**normal: {Convert.ToInt32(arrInventory["boost"]["aiko"]["normal"].ToString()) * 10}%**\n";
            if (Convert.ToInt32(arrInventory["boost"]["aiko"]["normal"].ToString()) > 0)
                aikoBoost += $"**platinum: {Convert.ToInt32(arrInventory["boost"]["aiko"]["platinum"].ToString()) * 10}%**\n";
            if (Convert.ToInt32(arrInventory["boost"]["aiko"]["normal"].ToString()) > 0)
                aikoBoost += $"**metal: {Convert.ToInt32(arrInventory["boost"]["aiko"]["metal"].ToString()) * 10}%**\n";
            if (Convert.ToInt32(arrInventory["boost"]["aiko"]["ojamajos"].ToString()) > 0)
                aikoBoost += $"**ojamajos: {Convert.ToInt32(arrInventory["boost"]["aiko"]["ojamajos"].ToString()) * 10}%**";

            //onpu boost
            if (Convert.ToInt32(arrInventory["boost"]["onpu"]["normal"].ToString()) > 0)
                onpuBoost = $"**normal: {Convert.ToInt32(arrInventory["boost"]["onpu"]["normal"].ToString()) * 10}%**\n";
            if (Convert.ToInt32(arrInventory["boost"]["onpu"]["normal"].ToString()) > 0)
                onpuBoost += $"**platinum: {Convert.ToInt32(arrInventory["boost"]["onpu"]["platinum"].ToString()) * 10}%**\n";
            if (Convert.ToInt32(arrInventory["boost"]["onpu"]["normal"].ToString()) > 0)
                onpuBoost += $"**metal: {Convert.ToInt32(arrInventory["boost"]["onpu"]["metal"].ToString()) * 10}%**\n";
            if (Convert.ToInt32(arrInventory["boost"]["onpu"]["ojamajos"].ToString()) > 0)
                onpuBoost += $"**ojamajos: {Convert.ToInt32(arrInventory["boost"]["onpu"]["ojamajos"].ToString()) * 10}%**";

            //momoko boost
            if (Convert.ToInt32(arrInventory["boost"]["momoko"]["normal"].ToString()) > 0)
                momokoBoost = $"**normal: {Convert.ToInt32(arrInventory["boost"]["momoko"]["normal"].ToString()) * 10}%**\n";
            if (Convert.ToInt32(arrInventory["boost"]["momoko"]["normal"].ToString()) > 0)
                momokoBoost += $"**platinum: {Convert.ToInt32(arrInventory["boost"]["momoko"]["platinum"].ToString()) * 10}%**\n";
            if (Convert.ToInt32(arrInventory["boost"]["momoko"]["normal"].ToString()) > 0)
                momokoBoost += $"**metal: {Convert.ToInt32(arrInventory["boost"]["momoko"]["metal"].ToString()) * 10}%**\n";
            if (Convert.ToInt32(arrInventory["boost"]["momoko"]["ojamajos"].ToString()) > 0)
                momokoBoost += $"**ojamajos: {Convert.ToInt32(arrInventory["boost"]["momoko"]["ojamajos"].ToString()) * 10}%**";

            //other boost
            if (Convert.ToInt32(arrInventory["boost"]["other"]["special"].ToString()) > 0)
                specialBoost = $"**special:{Convert.ToInt32(arrInventory["boost"]["other"]["special"].ToString()) * 10}%**";

            if (doremiBoost == "") doremiBoost = "No available boost for this card pack.";
            if (hazukiBoost == "") hazukiBoost = "No available boost for this card pack.";
            if (aikoBoost == "") aikoBoost = "No available boost for this card pack.";
            if (onpuBoost == "") onpuBoost = "No available boost for this card pack.";
            if (momokoBoost == "") momokoBoost = "No available boost for this card pack.";
            if (specialBoost == "") specialBoost = "No available boost for this card pack.";

            return new EmbedBuilder()
                .WithColor(color)
                .WithTitle($":arrow_double_up: **{username} Card Status Boost**")
                .WithDescription("Using a boost for **any category** on a card pack will remove all that boost status. " +
                "You can use the capture boost with **<bot prefix>!card capture boost**")
                .AddField("Doremi Boost", doremiBoost, true)
                .AddField("Hazuki Boost", hazukiBoost, true)
                .AddField("Aiko Boost", aikoBoost, true)
                .AddField("Onpu Boost", onpuBoost, true)
                .AddField("Momoko Boost", momokoBoost, true)
                .AddField("Other Boost", specialBoost, true);
        }


        public static EmbedBuilder printEmptyInventoryTemplate(Color color, string pack, string category, int maxAmount, string username)
        {
            return new EmbedBuilder()
                .WithColor(color)
                .WithTitle($"**{username}'s {GlobalFunctions.UppercaseFirst(pack)} {GlobalFunctions.UppercaseFirst(category)} Card (0/{maxAmount})**")
                .WithDescription($":x: There are no {pack} - {category} cards that you have captured yet.");
        }

        public static EmbedBuilder printCardDetailTemplate(Color color, string guildId, string clientId, 
            string username, string card_id, string parent, string emojiError, string errorDescription)
        {
            string category = getCardCategory(card_id);

            //start read json
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
            JObject arrInventory = JObject.Parse(File.ReadAllText(playerDataDirectory));
            var jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));

            if (!File.Exists(playerDataDirectory)) //not registered yet
            {
                return new EmbedBuilder()
                .WithColor(color)
                .WithDescription($":x: I'm sorry, please register yourself first with **{Config.Doremi.PrefixParent[0]}card register** command.")
                .WithThumbnailUrl(emojiError);
            } else {
                string name = "";
                if (category != "special")
                {
                    try
                    {
                        name = jObjTradingCardList[parent][category][card_id]["name"].ToString();
                    }
                    catch { }
                }
                else if (category == "special")
                {
                    try
                    {
                        name = jObjTradingCardList["other"][category][card_id]["name"].ToString();
                        parent = "other";
                    }
                    catch { }
                }

                if (name == "")
                {
                    return new EmbedBuilder()
                    .WithColor(color)
                    .WithDescription(errorDescription)
                    .WithThumbnailUrl(emojiError);
                }
                else if (arrInventory[parent][category].ToString().Contains(card_id))
                {
                    string imgUrl = jObjTradingCardList[parent][category][card_id]["url"].ToString();
                    string rank = jObjTradingCardList[parent][category][card_id]["0"].ToString();
                    string star = jObjTradingCardList[parent][category][card_id]["1"].ToString();
                    string point = jObjTradingCardList[parent][category][card_id]["2"].ToString();

                    return new EmbedBuilder()
                    .WithAuthor(name)
                    .WithColor(color)
                    .AddField("ID", card_id, true)
                    .AddField("Category", category, true)
                    .AddField("Rank", rank, true)
                    .AddField("Star", star, true)
                    .AddField("Point", point, true)
                    .WithImageUrl(imgUrl);
                }
                else
                {
                    return new EmbedBuilder()
                    .WithColor(color)
                    .WithDescription($":x: Sorry **{username}**, you don't have: **{card_id} - {name}** card yet. Try to capture it to look at this card.")
                    .WithThumbnailUrl(emojiError);
                }
            }
        }

        public static EmbedBuilder printLeaderboardTemplate(Color color, string username, string guildId, string clientId){
            var jObjLeaderboard = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/trading_card_leaderboard_data.json"));
            
            var arrListDoremi = ((JObject)jObjLeaderboard["doremi"]).Properties().ToList();
            var arrListHazuki = ((JObject)jObjLeaderboard["hazuki"]).Properties().ToList();
            var arrListAiko = ((JObject)jObjLeaderboard["aiko"]).Properties().ToList();
            var arrListOnpu = ((JObject)jObjLeaderboard["onpu"]).Properties().ToList();
            var arrListMomoko = ((JObject)jObjLeaderboard["momoko"]).Properties().ToList();
            var arrListOther = ((JObject)jObjLeaderboard["other"]).Properties().ToList();

            string doremiText=""; string hazukiText = ""; string aikoText = ""; string onpuText = ""; string momokoText = ""; string otherText = "";
            for (int i = 0; i < arrListDoremi.Count; i++)
            {
                if (i <= 4)
                    doremiText += $"**{i + 1}. {MentionUtils.MentionUser(Convert.ToUInt64(arrListDoremi[i].Name))} : {arrListDoremi[i].Value}**";
                 else
                    break;
            }
                
            for (int i = 0; i < arrListHazuki.Count; i++)
            {
                if (i <= 4)
                    hazukiText += $"**{i + 1}. {MentionUtils.MentionUser(Convert.ToUInt64(arrListHazuki[i].Name))} : {arrListHazuki[i].Value}**";
                else
                    break;
            }
                
            for (int i = 0; i < arrListAiko.Count; i++)
            {
                if (i <= 4)
                    aikoText += $"**{i + 1}. {MentionUtils.MentionUser(Convert.ToUInt64(arrListAiko[i].Name))} : {arrListAiko[i].Value}**";
                else
                    break;
            }
                
            for (int i = 0; i < arrListOnpu.Count; i++)
            {
                if (i <= 4)
                    onpuText += $"**{i + 1}. {MentionUtils.MentionUser(Convert.ToUInt64(arrListOnpu[i].Name))} : {arrListOnpu[i].Value}**";
                else
                    break;
            }
                
            for (int i = 0; i < arrListMomoko.Count; i++)
            {
                if (i <= 4)
                    momokoText += $"**{i + 1}. {MentionUtils.MentionUser(Convert.ToUInt64(arrListMomoko[i].Name))} : {arrListMomoko[i].Value}**";
                else
                    break;
            }

            for (int i = 0; i < arrListOther.Count; i++)
            {
                if (i <= 4)
                    otherText += $"**{i + 1}. {MentionUtils.MentionUser(Convert.ToUInt64(arrListOther[i].Name))} : {arrListOther[i].Value}**";
                else
                    break;
            }

            if (doremiText == "") doremiText = "No one has complete Doremi card pack yet.";
            if (hazukiText == "") hazukiText = "No one has complete Hazuki card pack yet.";
            if (aikoText == "") aikoText = "No one has complete Aiko card pack yet.";
            if (onpuText == "") onpuText = "No one has complete Onpu card pack yet.";
            if (momokoText == "") momokoText = "No one has complete Momoko card pack yet.";
            if (otherText == "") otherText = "No one has complete Other card pack yet.";
            
            return new EmbedBuilder()
                .WithTitle($"\uD83C\uDFC6 Top 5 Trading Card Leaderboard")
                .WithColor(color)
                .AddField("Doremi Card Pack", doremiText)
                .AddField("Hazuki Card Pack", hazukiText)
                .AddField("Aiko Card Pack", aikoText)
                .AddField("Onpu Card Pack", onpuText)
                .AddField("Momoko Card Pack", momokoText)
                .AddField("Other Card Pack", otherText);
        }

        public static EmbedBuilder userCompleteTheirList(Color color, string avatarEmbed, string parent, string imgUrl, string guildId, string clientId,
            string unlockText, string username, string userAvatarUrl)
        {
            //update & save leaderboard data
            string dateTimeNow = DateTime.Now.ToString("MM/dd/yyyy");
            string leaderboardDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/trading_card_leaderboard_data.json";
            var jObjLeaderboard = JObject.Parse(File.ReadAllText(leaderboardDataDirectory));
            ((JObject)jObjLeaderboard[parent]).Add(clientId, dateTimeNow);
            File.WriteAllText(leaderboardDataDirectory, jObjLeaderboard.ToString());

            EmbedBuilder eb = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder
                {
                    Name = username,
                    IconUrl = userAvatarUrl
                })
                .WithTitle($"{GlobalFunctions.UppercaseFirst(parent)} Card Pack Completed!")
                .WithDescription($"Congratulations, **{username}** have completed all **{GlobalFunctions.UppercaseFirst(parent)} Card Pack**!")
                .WithColor(color)
                .WithImageUrl($"attachment://{Path.GetFileName(imgUrl)}")
                .WithFooter($"Completed at: {dateTimeNow}", avatarEmbed);
                eb.AddField("Role & Badge Reward:", unlockText);

            //return congratulate embed
            return eb;
        }

        public static EmbedBuilder printCardCaptureTemplate(Color color, string name, string imgUrl, string card_id,
            string category, string rank, string star, string point, string username, string botIconUrl,
            int totalCaptured, int max)
        {
            return new EmbedBuilder()
                    .WithAuthor(name)
                    .WithColor(color)
                    .AddField("ID", card_id, true)
                    .AddField("Category", category, true)
                    .AddField("Rank", rank, true)
                    .AddField("Star", star, true)
                    .AddField("Point", point, true)
                    .WithImageUrl(imgUrl)
                    .WithFooter($"Captured by: {username} ({totalCaptured}/{max})",botIconUrl);
        }

        public static void resetSpawnInstance(ulong guildId)
        {
            Config.Guild.setPropertyValue(guildId, propertyId, "");
            Config.Guild.setPropertyValue(guildId, propertyCategory, "");
            Config.Guild.setPropertyValue(guildId, propertyToken, "");
            Config.Guild.setPropertyValue(guildId, propertyMystery, "0");
        }

        public static EmbedBuilder printStatusTemplate(Color color, string username, string guildId, string clientId, string emojiError,
            string thumbnailUrl)
        {
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
            
            if (!File.Exists(playerDataDirectory))
            { //not registered yet
                return new EmbedBuilder()
                .WithColor(color)
                .WithDescription($"I'm sorry, {MentionUtils.MentionUser(Convert.ToUInt64(clientId))} need to " +
                $"register first with **{Config.Doremi.PrefixParent[0]}card register** command.")
                .WithThumbnailUrl(emojiError);
            }
            else
            {
                DateTime creation = File.GetCreationTime($"{playerDataDirectory}");
                var playerData = JObject.Parse(File.ReadAllText(playerDataDirectory));
                var arrListDoremi = playerData["doremi"];
                var arrListHazuki = playerData["hazuki"];
                var arrListAiko = playerData["aiko"];
                var arrListOnpu = playerData["onpu"];
                var arrListMomoko = playerData["momoko"];
                var arrListOther = playerData["other"];
                var playerExp = Convert.ToInt32(playerData["catch_attempt"].ToString());

                int totalSuccess = ((JArray)arrListDoremi["normal"]).Count + ((JArray)arrListDoremi["platinum"]).Count + ((JArray)arrListDoremi["metal"]).Count + ((JArray)arrListDoremi["ojamajos"]).Count +
                    ((JArray)arrListHazuki["normal"]).Count + ((JArray)arrListHazuki["platinum"]).Count + ((JArray)arrListHazuki["metal"]).Count + ((JArray)arrListHazuki["ojamajos"]).Count +
                    ((JArray)arrListAiko["normal"]).Count + ((JArray)arrListAiko["platinum"]).Count + ((JArray)arrListAiko["metal"]).Count + ((JArray)arrListAiko["ojamajos"]).Count +
                    ((JArray)arrListOnpu["normal"]).Count + ((JArray)arrListOnpu["platinum"]).Count + ((JArray)arrListOnpu["metal"]).Count + ((JArray)arrListOnpu["ojamajos"]).Count +
                    ((JArray)arrListMomoko["normal"]).Count + ((JArray)arrListMomoko["platinum"]).Count + ((JArray)arrListMomoko["metal"]).Count + ((JArray)arrListMomoko["ojamajos"]).Count +
                    ((JArray)arrListOther["special"]).Count;

                string doremiText = $"**Normal: {((JArray)arrListDoremi["normal"]).Count}/{Doremi.maxNormal}**\n" +
                    $"**Platinum: {((JArray)arrListDoremi["platinum"]).Count}/{Doremi.maxPlatinum}**\n" +
                    $"**Metal: {((JArray)arrListDoremi["metal"]).Count}/{Doremi.maxMetal}**\n" +
                    $"**Ojamajos: {((JArray)arrListDoremi["ojamajos"]).Count}/{Doremi.maxOjamajos}**";
                int totalSuccessPack = ((JArray)arrListDoremi["normal"]).Count + ((JArray)arrListDoremi["platinum"]).Count
                    + ((JArray)arrListDoremi["metal"]).Count + ((JArray)arrListDoremi["ojamajos"]).Count;
                int totalMax = Doremi.maxNormal+ Doremi.maxPlatinum + Doremi.maxMetal + Doremi.maxOjamajos;
                double calculated = (double)totalSuccessPack / totalMax * 100;
                string doremiPercentage = $"({Math.Round(calculated)}%)";

                string hazukiText = $"**Normal: {((JArray)arrListHazuki["normal"]).Count}/{Hazuki.maxNormal}**\n" +
                    $"**Platinum: {((JArray)arrListHazuki["platinum"]).Count}/{Hazuki.maxPlatinum}**\n" +
                    $"**Metal: {((JArray)arrListHazuki["metal"]).Count}/{Hazuki.maxMetal}**\n" +
                    $"**Ojamajos: {((JArray)arrListHazuki["ojamajos"]).Count}/{Hazuki.maxOjamajos}**";
                totalSuccessPack = ((JArray)arrListHazuki["normal"]).Count + ((JArray)arrListHazuki["platinum"]).Count
                    + ((JArray)arrListHazuki["metal"]).Count + ((JArray)arrListHazuki["ojamajos"]).Count;
                totalMax = Hazuki.maxNormal + Hazuki.maxPlatinum + Hazuki.maxMetal + Hazuki.maxOjamajos;
                calculated = (double)totalSuccessPack / totalMax * 100;
                string hazukiPercentage = $"({Math.Round(calculated)}%)";

                string aikoText = $"**Normal: {((JArray)arrListAiko["normal"]).Count}/{Aiko.maxNormal}**\n" +
                    $"**Platinum: {((JArray)arrListAiko["platinum"]).Count}/{Aiko.maxPlatinum}**\n" +
                    $"**Metal: {((JArray)arrListAiko["metal"]).Count}/{Aiko.maxMetal}**\n" +
                    $"**Ojamajos: {((JArray)arrListAiko["ojamajos"]).Count}/{Aiko.maxOjamajos}**";
                totalSuccessPack = ((JArray)arrListAiko["normal"]).Count + ((JArray)arrListAiko["platinum"]).Count
                    + ((JArray)arrListAiko["metal"]).Count + ((JArray)arrListAiko["ojamajos"]).Count;
                totalMax = Aiko.maxNormal + Aiko.maxPlatinum + Aiko.maxMetal + Aiko.maxOjamajos;
                calculated = (double)totalSuccessPack / totalMax * 100;
                string aikoPercentage = $"({Math.Round(calculated)}%)";

                string onpuText = $"**Normal: {((JArray)arrListOnpu["normal"]).Count}/{Onpu.maxNormal}**\n" +
                    $"**Platinum: {((JArray)arrListOnpu["platinum"]).Count}/{Onpu.maxPlatinum}**\n" +
                    $"**Metal: {((JArray)arrListOnpu["metal"]).Count}/{Onpu.maxMetal}**\n" +
                    $"**Ojamajos: {((JArray)arrListOnpu["ojamajos"]).Count}/{Onpu.maxOjamajos}**";
                totalSuccessPack = ((JArray)arrListOnpu["normal"]).Count + ((JArray)arrListOnpu["platinum"]).Count
                    + ((JArray)arrListOnpu["metal"]).Count + ((JArray)arrListOnpu["ojamajos"]).Count;
                totalMax = Onpu.maxNormal + Onpu.maxPlatinum + Onpu.maxMetal + Onpu.maxOjamajos;
                calculated = (double)totalSuccessPack / totalMax * 100;
                string onpuPercentage = $"({Math.Round(calculated)}%)";

                string momokoText = $"**Normal: {((JArray)arrListMomoko["normal"]).Count}/{Momoko.maxNormal}**\n" +
                    $"**Platinum: {((JArray)arrListMomoko["platinum"]).Count}/{Momoko.maxPlatinum}**\n" +
                    $"**Metal: {((JArray)arrListMomoko["metal"]).Count}/{Momoko.maxMetal}**\n" +
                    $"**Ojamajos: {((JArray)arrListMomoko["ojamajos"]).Count}/{Momoko.maxOjamajos}**";
                totalSuccessPack = ((JArray)arrListMomoko["normal"]).Count + ((JArray)arrListMomoko["platinum"]).Count
                    + ((JArray)arrListMomoko["metal"]).Count + ((JArray)arrListMomoko["ojamajos"]).Count;
                totalMax = Momoko.maxNormal + Momoko.maxPlatinum + Momoko.maxMetal + Momoko.maxOjamajos;
                calculated = (double)totalSuccessPack / totalMax * 100;
                string momokoPercentage = $"({Math.Round(calculated)}%)";

                string otherText = $"**Special: {((JArray)arrListOther["special"]).Count}/{maxSpecial}**";
                totalSuccessPack = ((JArray)arrListOther["special"]).Count;
                totalMax = maxSpecial;
                calculated = (double)totalSuccessPack / totalMax * 100;
                string otherPercentage = $"({Math.Round(calculated)}%)";

                return new EmbedBuilder()
                    .WithTitle($"📇 {username} Card Status | Rank: {getPlayerRank(playerExp)}")
                    .WithColor(color)
                    .WithThumbnailUrl(thumbnailUrl)
                    .AddField("Collected / EXP", $"**{totalSuccess} / {playerData["catch_attempt"].ToString()}**", false)
                    .AddField($"Doremi Pack {doremiPercentage}", doremiText, true)
                    .AddField($"Hazuki Pack {hazukiPercentage}", hazukiText, true)
                    .AddField($"Aiko Pack {aikoPercentage}", aikoText, true)
                    .AddField($"Onpu Pack {onpuPercentage}", onpuText, true)
                    .AddField($"Momoko Pack {momokoPercentage}", momokoText, true)
                    .AddField($"Other Pack {otherPercentage}", otherText, true)
                    .WithFooter($"Magic seeds: {playerData["magic_seeds"]}");
            }
        }

        public static List<string> tradeListAllowed(JObject playerData)
        {
            List<string> listAllowed = new List<string>();
            var arrListDoremi = playerData["doremi"];
            var arrListHazuki = playerData["hazuki"];
            var arrListAiko = playerData["aiko"];
            var arrListOnpu = playerData["onpu"];
            var arrListMomoko = playerData["momoko"];
            //doremi
            if (((JArray)(arrListDoremi["normal"])).Count >= 1)
                listAllowed.Add("doremi normal");
            if (((JArray)(arrListDoremi["platinum"])).Count >= 1)
                listAllowed.Add("doremi platinum");
            if (((JArray)(arrListDoremi["metal"])).Count >= 1)
                listAllowed.Add("doremi metal");
            if (((JArray)(arrListDoremi["ojamajos"])).Count >= 1)
                listAllowed.Add("doremi ojamajos");
            //hazuki
            if (((JArray)(arrListHazuki["normal"])).Count >= 1)
                listAllowed.Add("hazuki normal");
            if (((JArray)(arrListHazuki["platinum"])).Count >= 1)
                listAllowed.Add("hazuki platinum");
            if (((JArray)(arrListHazuki["metal"])).Count >= 1)
                listAllowed.Add("hazuki metal");
            if (((JArray)(arrListHazuki["ojamajos"])).Count >= 1)
                listAllowed.Add("hazuki ojamajos");
            //aiko
            if (((JArray)(arrListAiko["normal"])).Count >= 1)
                listAllowed.Add("aiko normal");
            if (((JArray)(arrListAiko["platinum"])).Count >= 1)
                listAllowed.Add("aiko platinum");
            if (((JArray)(arrListAiko["metal"])).Count >= 1)
                listAllowed.Add("aiko metal");
            if (((JArray)(arrListHazuki["ojamajos"])).Count >= 1)
                listAllowed.Add("aiko ojamajos");
            //onpu
            if (((JArray)(arrListOnpu["normal"])).Count >= 1)
                listAllowed.Add("onpu normal");
            if (((JArray)(arrListOnpu["platinum"])).Count >= 1)
                listAllowed.Add("onpu platinum");
            if (((JArray)(arrListOnpu["metal"])).Count >= 1)
                listAllowed.Add("onpu metal");
            if (((JArray)(arrListHazuki["ojamajos"])).Count >= 1)
                listAllowed.Add("onpu ojamajos");
            //momoko
            if (((JArray)(arrListMomoko["normal"])).Count >= 1)
                listAllowed.Add("momoko normal");
            if (((JArray)(arrListMomoko["platinum"])).Count >= 1)
                listAllowed.Add("momoko platinum");
            if (((JArray)(arrListMomoko["metal"])).Count >= 1)
                listAllowed.Add("momoko metal");
            if (((JArray)(arrListHazuki["ojamajos"])).Count >= 1)
                listAllowed.Add("momoko ojamajos");

            return listAllowed;
        }

        public static EmbedBuilder activatePureleine(ulong guildId, string clientId, string username, string answer)
        {
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";

            EmbedBuilder embed;
            if (Config.Guild.getPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCard) != "")
            {
                string badCardType = TradingCardCore.BadCards.getType(
                    Convert.ToInt32(Config.Guild.getPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCard))
                    );
                int number1 = Convert.ToInt32(Config.Guild.getPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardNumber1));
                int number2 = Convert.ToInt32(Config.Guild.getPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardNumber2));
                string equation = Config.Guild.getPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardEquation);
                string correctAnswer = "";
                if (equation == "+") correctAnswer = (number1 + number2).ToString();
                else if (equation == "-") correctAnswer = (number1 - number2).ToString();

                if (answer == "")
                {
                    embed = new EmbedBuilder()
                    .WithAuthor(TradingCardCore.BadCards.embedPureleineName, TradingCardCore.BadCards.embedPureleineAvatar)
                    .WithColor(TradingCardCore.BadCards.embedPureleineColor)
                    .WithTitle("Bad card detected!")
                    .WithDescription($"I'm detecting a great amount of bad card energy! You need to remove it with **<bot>!card pureleine <answer>** commands from the question below:")
                    .WithThumbnailUrl(TradingCardCore.BadCards.imgPureleineFound);
                    embed.AddField("Question:", $"{number1}{equation}{number2} = ?");
                }
                else if (answer != correctAnswer)
                {
                    embed = new EmbedBuilder()
                    .WithAuthor(TradingCardCore.BadCards.embedPureleineName, TradingCardCore.BadCards.embedPureleineAvatar)
                    .WithColor(TradingCardCore.BadCards.embedPureleineColor)
                    .WithTitle("Wrong answer!")
                    .WithDescription($":x: That answer is wrong! **{username}** also lost a chance to capture card for this turn...")
                    .WithThumbnailUrl(TradingCardCore.BadCards.imgAnswerWrong);
                    JObject arrInventory = JObject.Parse(File.ReadAllText(playerDataDirectory));
                    arrInventory["catch_token"] = Config.Guild.getPropertyValue(guildId, TradingCardCore.propertyToken);
                    File.WriteAllText(playerDataDirectory, arrInventory.ToString());
                }
                else
                {
                    JObject arrInventory = JObject.Parse(File.ReadAllText(playerDataDirectory));

                    embed = new EmbedBuilder()
                    .WithAuthor(TradingCardCore.BadCards.embedPureleineName, TradingCardCore.BadCards.embedPureleineAvatar)
                    .WithColor(TradingCardCore.BadCards.embedPureleineColor)
                    .WithTitle("Bad cards effect has been removed!")
                    .WithDescription($":white_check_mark: You may now safely capture the spawned card again.\n")
                    .WithFooter($"Removed by: {username}")
                    .WithThumbnailUrl(TradingCardCore.BadCards.imgAnswerCorrect);
                    Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCard, "");
                    Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardEquation, "");
                    Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardNumber1, "");
                    Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardNumber2, "");
                    //isRemoved = true;

                    if (badCardType == "curse" || badCardType == "failure")
                    {
                        //generate random card
                        int randomParent = new Random().Next(0, 6);
                        int randomCategory = new Random().Next(100);
                        string chosenCategory = "normal";

                        if (randomCategory <= 5)//0-1 //platinum
                            chosenCategory = "platinum";
                        
                        string parent = "doremi";
                        if (randomParent == 1)
                            parent = "hazuki";
                        else if (randomParent == 2)
                            parent = "aiko";
                        else if (randomParent == 3)
                            parent = "onpu";
                        else if (randomParent == 4)
                            parent = "momoko";

                        //start read json
                        var jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));
                        var key = JObject.Parse(jObjTradingCardList[parent][chosenCategory].ToString()).Properties().ToList();
                        int randIndex = new Random().Next(0, key.Count);
                        string chosenId = key[randIndex].Name;


                        //chosen data:
                        //chosenId = "do004"; chosenCategory = "normal"; parent = "doremi"; //for debug only
                        string chosenName = jObjTradingCardList[parent][chosenCategory][chosenId]["name"].ToString();

                        try
                        {
                            if (arrInventory[parent][chosenCategory].ToString().Contains(chosenId))
                                embed.Description += $"Sorry, I can't give **{username}** bonus card: **{chosenId} - {chosenName}** because you have it already...";
                            else
                            {
                                string chosenUrl = jObjTradingCardList[parent][chosenCategory][chosenId]["url"].ToString();
                                embed.Description += $"**{username}** have been rewarded with a bonus card!";
                                embed.AddField($"{chosenCategory} {parent} Bonus Card Reward:", $"**{chosenId} - {chosenName}**");
                                embed.WithImageUrl(chosenUrl);

                                arrInventory["catch_attempt"] = (Convert.ToInt32(arrInventory["catch_attempt"]) + 1).ToString();
                                JArray item = (JArray)arrInventory[parent][chosenCategory];
                                item.Add(chosenId);
                                File.WriteAllText(playerDataDirectory, arrInventory.ToString());

                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    }
                    else if (badCardType == "seeds")
                    {
                        int randomedMagicSeeds = new Random().Next(1, 4);
                        embed.Description += $"**{username}** have been rewarded with some magic seeds!";
                        embed.AddField("Rewards:", $"{randomedMagicSeeds} magic seeds.");
                        embed.WithImageUrl(GardenCore.imgMagicSeeds);
                        arrInventory["magic_seeds"] = (Convert.ToInt32(arrInventory["magic_seeds"]) + randomedMagicSeeds).ToString();
                        File.WriteAllText(playerDataDirectory, arrInventory.ToString());
                    }
                }
            }
            else
                embed = new EmbedBuilder()
                .WithAuthor(TradingCardCore.BadCards.embedPureleineName, TradingCardCore.BadCards.embedPureleineAvatar)
                .WithColor(TradingCardCore.BadCards.embedPureleineColor)
                .WithThumbnailUrl(null)
                .WithDescription(":x: I didn't sense any bad cards energy right now...")
                .WithThumbnailUrl(TradingCardCore.BadCards.imgPureleineNotFound);

            return embed;

            //if (isRemoved) await TradingCardCore.printCardSpawned(guildId);
            
        }

        public static Tuple<string, EmbedBuilder, string, IDictionary<string,Boolean>> cardCapture(Color color, string embedAvatarUrl, 
            ulong guildId, string clientId, string username, 
            string emojiError, string parent, string boost, string errorPrefix, string containCardId,
            int maxNormal, int maxPlatinum, int maxMetal, int maxOjamajos)
        {
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId.ToString()}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
            var jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));

            EmbedBuilder returnEmbedBuilder;
            string replyText = "";
            IDictionary<string, Boolean> returnCompleted = new Dictionary<string,Boolean>();//state of completedCard
            returnCompleted.Add("doremi", false); returnCompleted.Add("hazuki", false); returnCompleted.Add("aiko", false);
            returnCompleted.Add("onpu", false); returnCompleted.Add("momoko", false); returnCompleted.Add("special", false);

            if (!File.Exists(playerDataDirectory))
            {
                returnEmbedBuilder = new EmbedBuilder()
                .WithColor(color)
                .WithDescription($"I'm sorry, please register yourself first with **{Config.Doremi.PrefixParent[0]}card register** command.")
                .WithThumbnailUrl(emojiError);
                return Tuple.Create("", returnEmbedBuilder, "", returnCompleted);
                //return;
            }
            else
            {
                JObject arrInventory = JObject.Parse(File.ReadAllText(playerDataDirectory));
                string spawnedCardId = Config.Guild.getPropertyValue(guildId, TradingCardCore.propertyId);
                string spawnedCardCategory = Config.Guild.getPropertyValue(guildId, TradingCardCore.propertyCategory);
                string spawnedMystery = Config.Guild.getPropertyValue(guildId, TradingCardCore.propertyMystery);
                string spawnedBadCards = Config.Guild.getPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCard);

                int boostNormal = 0; int boostPlatinum = 0; int boostMetal = 0;
                int boostOjamajos = 0; int boostSpecial = 0; 
                int playerRank = getPlayerRank(Convert.ToInt32(arrInventory["catch_attempt"].ToString()));
                if (spawnedCardCategory.ToLower() == "special")
                {
                    parent = "other";
                    boostSpecial = Convert.ToInt32(arrInventory["boost"]["other"]["special"].ToString());
                }
                else
                {
                    boostNormal = Convert.ToInt32(arrInventory["boost"][parent]["normal"].ToString());
                    boostPlatinum = Convert.ToInt32(arrInventory["boost"][parent]["platinum"].ToString());
                    boostMetal = Convert.ToInt32(arrInventory["boost"][parent]["metal"].ToString());
                    boostOjamajos = Convert.ToInt32(arrInventory["boost"][parent]["ojamajos"].ToString());
                }

                //process booster
                Boolean useBoost = false;
                if (boost.ToLower() != "" && boost.ToLower() != "boost")
                {
                    returnEmbedBuilder = new EmbedBuilder()
                    .WithColor(color)
                    .WithDescription($":x: Sorry, that is not the valid card capture boost command. " +
                    $"Use: **{errorPrefix}card capture boost** to activate card boost.")
                    .WithThumbnailUrl(emojiError);
                    return Tuple.Create("", returnEmbedBuilder, "", returnCompleted);
                }
                else if ((boost.ToLower() == "boost" && spawnedCardCategory == "normal" && boostNormal <= 0) ||
                    (boost.ToLower() == "boost" && spawnedCardCategory == "platinum" && boostPlatinum <= 0) ||
                    (boost.ToLower() == "boost" && spawnedCardCategory == "metal" && boostMetal <= 0) || 
                    (boost.ToLower() == "boost" && spawnedCardCategory == "ojamajos" && boostOjamajos <= 0) ||
                    (boost.ToLower() == "boost" && spawnedCardCategory == "special" && boostSpecial <= 0))
                {
                    returnEmbedBuilder = new EmbedBuilder()
                    .WithColor(color)
                    .WithDescription($":x: Sorry, you have no **{parent} {spawnedCardCategory}** card capture boost that you can use.")
                    .WithThumbnailUrl(emojiError);
                    return Tuple.Create("", returnEmbedBuilder, "", returnCompleted);
                }
                else if (boost.ToLower() == "boost") useBoost = true;

                Boolean indexExists = false;
                try
                {
                    var cardExists = jObjTradingCardList[parent][spawnedCardCategory][spawnedCardId]["name"];
                    indexExists = true;
                } catch {

                }
                
                if (spawnedCardId != "" && spawnedCardCategory != "")
                {
                    if (spawnedCardId.Contains(containCardId) ||
                        (spawnedCardId.Contains("oj") && indexExists) ||
                        spawnedCardCategory.ToLower() == "special" ||
                        spawnedMystery == "1")//check if the card is doremi/ojamajos/other
                    {
                        int catchState = 0;

                        if ((string)arrInventory["catch_token"] == Config.Guild.getPropertyValue(guildId, TradingCardCore.propertyToken))
                        {
                            returnEmbedBuilder = new EmbedBuilder()
                            .WithColor(color)
                            .WithDescription($":x: Sorry, please wait for the next card spawn.")
                            .WithThumbnailUrl(emojiError);
                            return Tuple.Create("", returnEmbedBuilder, "", returnCompleted);
                        }
                        else if (spawnedMystery == "1" && !indexExists)
                        {
                            arrInventory["catch_attempt"] = (Convert.ToInt32(arrInventory["catch_attempt"]) + 1).ToString();
                            arrInventory["catch_token"] = Config.Guild.getPropertyValue(guildId, TradingCardCore.propertyToken);
                            File.WriteAllText(playerDataDirectory, arrInventory.ToString());

                            returnEmbedBuilder = new EmbedBuilder()
                            .WithColor(color)
                            .WithDescription($":x: Sorry, you guessed the wrong mystery card.")
                            .WithThumbnailUrl(emojiError);
                            return Tuple.Create("", returnEmbedBuilder, "", returnCompleted);
                        }

                        //check last capture time
                        try
                        {
                            if ((string)arrInventory["catch_token"] == "" ||
                                (string)arrInventory["catch_token"] != Config.Guild.getPropertyValue(guildId, TradingCardCore.propertyToken))
                            {
                                int boostPlayerRank = 0;
                                if (playerRank >= 2) boostPlayerRank = playerRank;
                                int catchRate = new Random().Next(10 - boostPlayerRank);
                                string name = jObjTradingCardList[parent][spawnedCardCategory][spawnedCardId]["name"].ToString();
                                string imgUrl = jObjTradingCardList[parent][spawnedCardCategory][spawnedCardId]["url"].ToString();
                                string rank = jObjTradingCardList[parent][spawnedCardCategory][spawnedCardId]["0"].ToString();
                                string star = jObjTradingCardList[parent][spawnedCardCategory][spawnedCardId]["1"].ToString();
                                string point = jObjTradingCardList[parent][spawnedCardCategory][spawnedCardId]["2"].ToString();

                                //check inventory
                                if (arrInventory[parent][spawnedCardCategory].ToString().Contains(spawnedCardId))
                                {//card already exist on inventory
                                    if (spawnedMystery != "1")
                                        replyText = $":x: Sorry, I can't capture **{spawnedCardId} - {name}** because you have it already.";
                                    else
                                        replyText = $":x: You guessed the mystery card correctly but I can't capture **{spawnedCardId} - {name}** because you have it already.";
                                }
                                else
                                {
                                    int maxCard = 0; string boostRate = "";
                                    //init RNG catch rate
                                    //if boost: change the TradingCardCore.captureRate
                                    if (spawnedCardCategory.ToLower() == "normal")
                                    {
                                        maxCard = maxNormal;
                                        if (!useBoost)
                                        {
                                            if ((catchRate < TradingCardCore.captureRateNormal && spawnedMystery != "1") ||
                                                (catchRate < TradingCardCore.captureRateNormal + 1 && spawnedMystery == "1")) catchState = 1;
                                        }
                                        else
                                        {
                                            boostRate = $"{boostNormal*10}%"; 
                                            if ((catchRate < boostNormal && spawnedMystery != "1") ||
                                                (catchRate < boostNormal + 1 && spawnedMystery == "1")) catchState = 1;
                                        }
                                    }
                                    else if (spawnedCardCategory.ToLower() == "platinum")
                                    {
                                        maxCard = maxPlatinum;
                                        if (!useBoost)
                                        {
                                            if ((catchRate < TradingCardCore.captureRatePlatinum && spawnedMystery != "1") ||
                                                (catchRate < TradingCardCore.captureRatePlatinum + 1 && spawnedMystery == "1")) catchState = 1;
                                        }
                                        else
                                        {
                                            boostRate = $"{boostPlatinum * 10}%";
                                            if ((catchRate < boostPlatinum && spawnedMystery != "1") ||
                                                (catchRate < boostPlatinum + 1 && spawnedMystery == "1")) catchState = 1;
                                        }
                                    }
                                    else if (spawnedCardCategory.ToLower() == "metal")
                                    {
                                        maxCard = maxMetal;
                                        if (!useBoost)
                                        {
                                            if ((catchRate < TradingCardCore.captureRateMetal && spawnedMystery != "1") ||
                                                (catchRate < TradingCardCore.captureRateMetal + 2 && spawnedMystery == "1")) catchState = 1;
                                        }
                                        else
                                        {
                                            boostRate = $"{boostMetal * 10}%";
                                            if ((catchRate < boostMetal && spawnedMystery != "1") ||
                                                (catchRate < boostMetal + 2 && spawnedMystery == "1")) catchState = 1;
                                        }
                                    }
                                    else if (spawnedCardCategory.ToLower() == "ojamajos")
                                    {
                                        maxCard = maxOjamajos;
                                        if (!useBoost && catchRate < TradingCardCore.captureRateOjamajos)
                                            catchState = 1;
                                        else if (useBoost && catchRate < boostOjamajos)
                                            catchState = 1;
                                        if(useBoost) boostRate = $"{boostOjamajos * 10}%";
                                    }
                                    else if (spawnedCardCategory.ToLower() == "special")
                                    {
                                        maxCard = TradingCardCore.maxSpecial;
                                        if (!useBoost && catchRate < TradingCardCore.captureRateSpecial)
                                            catchState = 1;
                                        else if (useBoost && catchRate < boostSpecial)
                                            catchState = 1;
                                        if (useBoost) boostRate = $"{boostSpecial * 10}%";
                                    }

                                    if (spawnedBadCards != "")
                                    {
                                        //bad card trigger
                                        if (spawnedBadCards != "")
                                        {
                                            spawnedBadCards = BadCards.getType(Convert.ToInt32(spawnedBadCards));
                                            emojiError = BadCards.imgBadCardActivated;

                                            if (spawnedBadCards == "seeds")
                                            {
                                                int randomLost = new Random().Next(1, 11);
                                                replyText = $":skull: Oh no, **{spawnedBadCards}** bad card effect has activated! **{username}** just lost {randomLost} magic seeds!";
                                                int currentMagicSeeds = Convert.ToInt32(arrInventory["magic_seeds"]);
                                                if (currentMagicSeeds >= 10)
                                                {
                                                    arrInventory["magic_seeds"] = Convert.ToInt32(arrInventory["magic_seeds"]) - randomLost;
                                                    File.WriteAllText(playerDataDirectory, arrInventory.ToString());
                                                } else
                                                {
                                                    arrInventory["magic_seeds"] = 0;
                                                    File.WriteAllText(playerDataDirectory, arrInventory.ToString());
                                                }
                                            }
                                            else if (spawnedBadCards == "curse")
                                            {
                                                string parentLost = parent;
                                                if (parent != "doremi" && parent != "hazuki" && parent != "aiko" && parent != "onpu" && parent != "momoko")
                                                    parentLost = "doremi";
                                                
                                                if (arrInventory[parentLost]["normal"].Count() >= 1)
                                                {
                                                    JArray item = (JArray)arrInventory[parentLost]["normal"];
                                                    string stolenCard = item[0].ToString();//[a,b]
                                                    string stolenName = jObjTradingCardList[parentLost]["normal"][stolenCard]["name"].ToString();
                                                    item.Remove(item[0]);
                                                    File.WriteAllText(playerDataDirectory, arrInventory.ToString());
                                                    replyText = $":skull: Oh no, **{spawnedBadCards}** bad card effect has activated! **{username}** just lost **{parentLost} normal** card: **{stolenCard} - {stolenName}**!";
                                                }
                                                else
                                                {
                                                    replyText = $":skull: Oh no, **{spawnedBadCards}** bad card effect has activated! But **{username}** don't have any card to lose!";
                                                }
                                            } else if(spawnedBadCards=="failure")
                                            {//failure
                                                replyText = $":skull: Oh no, **{spawnedBadCards}** bad card effect has activated! **{username}** just lost a chance to catch a card on this spawn turn!";
                                                arrInventory["catch_token"] = Config.Guild.getPropertyValue(guildId, TradingCardCore.propertyToken);
                                                File.WriteAllText(playerDataDirectory, arrInventory.ToString());
                                            }
                                        }
                                        //bad card ends
                                    }
                                    else if (catchState == 1)
                                    {
                                        //card not exist yet
                                        if (useBoost)
                                        {//reset boost
                                            replyText = $":arrow_double_up: **{GlobalFunctions.UppercaseFirst(parent)} {GlobalFunctions.UppercaseFirst(spawnedCardCategory)}** Card Capture boost has been used and boosted into " +
                                                $"**{boostRate}**!\n";
                                            if (spawnedCardCategory == "special")
                                                arrInventory["boost"]["other"]["special"] = "0";
                                            else
                                            {
                                                arrInventory["boost"][parent]["normal"] = "0";
                                                arrInventory["boost"][parent]["platinum"] = "0";
                                                arrInventory["boost"][parent]["metal"] = "0";
                                                arrInventory["boost"][parent]["ojamajos"] = "0";
                                            }
                                        }

                                        //save data:
                                        arrInventory["catch_attempt"] = (Convert.ToInt32(arrInventory["catch_attempt"]) + 1).ToString();
                                        arrInventory["catch_token"] = Config.Guild.getPropertyValue(guildId, TradingCardCore.propertyToken);
                                        JArray item = (JArray)arrInventory[parent][spawnedCardCategory];
                                        item.Add(spawnedCardId);

                                        if (spawnedCardCategory == "ojamajos")
                                        {
                                            var related = (JArray)jObjTradingCardList[parent][spawnedCardCategory][spawnedCardId]["related"];
                                            for (int i = 0; i < related.Count; i++)
                                            {
                                                if (!arrInventory[related[i].ToString()][spawnedCardCategory].ToString().Contains(spawnedCardId))
                                                {//check for duplicate on other card
                                                    JArray itemRelated = (JArray)arrInventory[related[i].ToString()][spawnedCardCategory];
                                                    itemRelated.Add(spawnedCardId);
                                                }
                                            }
                                        }

                                        File.WriteAllText(playerDataDirectory, arrInventory.ToString());

                                        string returnLevelUp = "";
                                        if ((int)arrInventory["catch_attempt"] == 100 || (int)arrInventory["catch_attempt"] == 200 ||
                                            (int)arrInventory["catch_attempt"] == 300 || (int)arrInventory["catch_attempt"] == 400 ||
                                            (int)arrInventory["catch_attempt"] == 500)
                                        {
                                            returnLevelUp = $":up: Congratulations! **{username}** is now rank {getPlayerRank((int)arrInventory["catch_attempt"])}!";
                                        }

                                        string[] arrRandomFirstSentence = {
                                            "Congratulations,","Nice Catch!","Nice one!","Yatta!"
                                        };

                                        if (spawnedMystery == "1")
                                            replyText += $":white_check_mark: {arrRandomFirstSentence[new Random().Next(0, arrRandomFirstSentence.Length)]} " +
                                            $"**{username}** have successfully revealed & captured **{spawnedCardCategory}** mystery card: **{name}**";
                                        else
                                            replyText += $":white_check_mark: {arrRandomFirstSentence[new Random().Next(0, arrRandomFirstSentence.Length)]} " +
                                            $"**{username}** have successfully captured **{spawnedCardCategory}** card: **{name}**";

                                        returnEmbedBuilder = TradingCardCore.printCardCaptureTemplate(color, name, imgUrl,
                                        spawnedCardId, spawnedCardCategory, rank, star, point, username, embedAvatarUrl, 
                                        item.Count, maxCard);

                                        //check if player have captured all doremi card/not
                                        if (((JArray)arrInventory["doremi"]["normal"]).Count >= TradingCardCore.Doremi.maxNormal &&
                                            ((JArray)arrInventory["doremi"]["platinum"]).Count >= TradingCardCore.Doremi.maxPlatinum &&
                                            ((JArray)arrInventory["doremi"]["metal"]).Count >= TradingCardCore.Doremi.maxMetal &&
                                            ((JArray)arrInventory["doremi"]["ojamajos"]).Count >= TradingCardCore.Doremi.maxOjamajos)
                                            returnCompleted["doremi"] = true;

                                        //check if player have captured all hazuki card/not
                                        if (((JArray)arrInventory["hazuki"]["normal"]).Count >= TradingCardCore.Hazuki.maxNormal &&
                                            ((JArray)arrInventory["hazuki"]["platinum"]).Count >= TradingCardCore.Hazuki.maxPlatinum &&
                                            ((JArray)arrInventory["hazuki"]["metal"]).Count >= TradingCardCore.Hazuki.maxMetal &&
                                            ((JArray)arrInventory["hazuki"]["ojamajos"]).Count >= TradingCardCore.Hazuki.maxOjamajos)
                                            returnCompleted["hazuki"] = true;

                                        //check if player have captured all aiko card/not
                                        if (((JArray)arrInventory["aiko"]["normal"]).Count >= TradingCardCore.Aiko.maxNormal &&
                                            ((JArray)arrInventory["aiko"]["platinum"]).Count >= TradingCardCore.Aiko.maxPlatinum &&
                                            ((JArray)arrInventory["aiko"]["metal"]).Count >= TradingCardCore.Aiko.maxMetal &&
                                            ((JArray)arrInventory["aiko"]["ojamajos"]).Count >= TradingCardCore.Aiko.maxOjamajos)
                                            returnCompleted["aiko"] = true;

                                        //check if player have captured all onpu card/not
                                        if (((JArray)arrInventory["onpu"]["normal"]).Count >= TradingCardCore.Onpu.maxNormal &&
                                            ((JArray)arrInventory["onpu"]["platinum"]).Count >= TradingCardCore.Onpu.maxPlatinum &&
                                            ((JArray)arrInventory["onpu"]["metal"]).Count >= TradingCardCore.Onpu.maxMetal &&
                                            ((JArray)arrInventory["onpu"]["ojamajos"]).Count >= TradingCardCore.Onpu.maxOjamajos)
                                            returnCompleted["onpu"] = true;

                                        //check if player have captured all momoko card/not
                                        if (((JArray)arrInventory["momoko"]["normal"]).Count >= TradingCardCore.Momoko.maxNormal &&
                                            ((JArray)arrInventory["momoko"]["platinum"]).Count >= TradingCardCore.Momoko.maxPlatinum &&
                                            ((JArray)arrInventory["momoko"]["metal"]).Count >= TradingCardCore.Momoko.maxMetal &&
                                            ((JArray)arrInventory["momoko"]["ojamajos"]).Count >= TradingCardCore.Momoko.maxOjamajos)
                                            returnCompleted["momoko"] = true;

                                        //check if player have captured all other special card/not
                                        if (((JArray)arrInventory["other"]["special"]).Count >= TradingCardCore.maxSpecial)
                                            returnCompleted["special"] = true;

                                        //erase spawned instance
                                        TradingCardCore.resetSpawnInstance(guildId);
                                        return Tuple.Create(replyText, returnEmbedBuilder, returnLevelUp, returnCompleted);
                                    }
                                    else
                                    {
                                        //save data:
                                        arrInventory["catch_attempt"] = (Convert.ToInt32(arrInventory["catch_attempt"]) + 1).ToString();
                                        arrInventory["catch_token"] = Config.Guild.getPropertyValue(guildId, TradingCardCore.propertyToken);
                                        if (useBoost)
                                        {   //reset boost
                                            replyText = $":arrow_double_up: **{GlobalFunctions.UppercaseFirst(parent)} {GlobalFunctions.UppercaseFirst(spawnedCardCategory)}** Card Capture Boost has been used!\n";
                                            if (spawnedCardCategory.ToLower() == "special")
                                                arrInventory["boost"]["other"]["special"] = "0";
                                            else
                                            {
                                                arrInventory["boost"][parent]["normal"] = "0";
                                                arrInventory["boost"][parent]["platinum"] = "0";
                                                arrInventory["boost"][parent]["metal"] = "0";
                                                arrInventory["boost"][parent]["ojamajos"] = "0";
                                            }
                                        }

                                        File.WriteAllText(playerDataDirectory, arrInventory.ToString());
                                        if (spawnedMystery == "1")
                                            replyText += $":x: Card revealed correctly! But sorry {username}, you **fail** to catch the mystery card. Better luck next time.";
                                        else
                                            replyText += $":x: I'm sorry {username}, you **fail** to catch the card. Better luck next time.";
                                    }
                                }

                            }
                            else
                            {
                                replyText = ":x: Sorry, please wait for the next card spawn.";
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }

                    }
                    else
                    {
                        replyText = ":x: Sorry, I can't capture that card. Try to use the other ojamajo bot to capture this card.";
                    }
                }
                else
                {
                    replyText = ":x: Sorry, either this card has been captured by someone or not spawned anymore. Please wait for the card to spawn again.";
                }
            }

            //fail
            returnEmbedBuilder = new EmbedBuilder()
            .WithColor(color)
            .WithDescription(replyText)
            .WithThumbnailUrl(emojiError);
            return Tuple.Create("", returnEmbedBuilder, "", returnCompleted);

        }

        public static Tuple<int,int> getCardCaptureState(string spawnedCardCategory,Boolean useBoost,
            int catchRate,string spawnedMystery,
            int maxNormal, int maxPlatinum, int maxMetal, int maxOjamajos,
            int boostNormal,int boostPlatinum,int boostMetal, int boostOjamajos,
            int boostSpecial)
        {
            int maxCard = 0; int catchState = 0;
            if (spawnedCardCategory.ToLower() == "normal")
            {
                maxCard = maxNormal;
                if (!useBoost)
                {
                    if ((catchRate < TradingCardCore.captureRateNormal && spawnedMystery != "1") ||
                        (catchRate < TradingCardCore.captureRateNormal + 1 && spawnedMystery == "1")) catchState = 1;
                }
                else
                {
                    if ((catchRate < boostNormal && spawnedMystery != "1") ||
                        (catchRate < boostNormal + 1 && spawnedMystery == "1")) catchState = 1;
                }
            }
            else if (spawnedCardCategory.ToLower() == "platinum")
            {
                maxCard = maxPlatinum;
                if (!useBoost)
                {
                    if ((catchRate < TradingCardCore.captureRatePlatinum && spawnedMystery != "1") ||
                        (catchRate < TradingCardCore.captureRatePlatinum + 1 && spawnedMystery == "1")) catchState = 1;
                }
                else
                {
                    if ((catchRate < boostPlatinum && spawnedMystery != "1") ||
                        (catchRate < boostPlatinum + 1 && spawnedMystery == "1")) catchState = 1;
                }
            }
            else if (spawnedCardCategory.ToLower() == "metal")
            {
                maxCard = maxMetal;
                if (!useBoost)
                {
                    if ((catchRate < TradingCardCore.captureRateMetal && spawnedMystery != "1") ||
                        (catchRate < TradingCardCore.captureRateMetal + 2 && spawnedMystery == "1")) catchState = 1;
                }
                else
                {
                    if ((catchRate < boostMetal && spawnedMystery != "1") ||
                        (catchRate < boostMetal + 2 && spawnedMystery == "1")) catchState = 1;
                }
            }
            else if (spawnedCardCategory.ToLower() == "ojamajos")
            {
                maxCard = maxOjamajos;
                if (!useBoost && catchRate < TradingCardCore.captureRateOjamajos)
                    catchState = 1;
                else if (useBoost && catchRate < boostOjamajos)
                    catchState = 1;

            }
            else if (spawnedCardCategory.ToLower() == "special")
            {
                maxCard = TradingCardCore.maxSpecial;
                if (!useBoost && catchRate < TradingCardCore.captureRateSpecial)
                    catchState = 1;
                else if (useBoost && catchRate < boostSpecial)
                    catchState = 1;
            }
            return Tuple.Create(maxCard, catchState);
        }

        //parameter pass: 
        public static List<string> printTradeCardListTemplate(string parent, string category,
            JObject jObjTradingCardList, JArray arrData)
        {
            List<string> pageContent = new List<string>();
            var arrList = (JArray)arrData;
            string title = $"**{GlobalFunctions.UppercaseFirst(parent)} {GlobalFunctions.UppercaseFirst(category)} Card List**\n";

            string tempVal = title;
            int currentIndex = 0;
            for (int i = 0; i < arrList.Count; i++)
            {
                string cardId = arrList[i].ToString();
                string name = jObjTradingCardList[parent][category][cardId]["name"].ToString();
                string url = jObjTradingCardList[parent][category][cardId]["url"].ToString();
                tempVal += $"[{arrList[i]} - {name}]({url})\n";


                if (currentIndex < 14) currentIndex++;
                else
                {
                    pageContent.Add(tempVal);
                    currentIndex = 0;
                    tempVal = title;
                }

                if (i == arrList.Count - 1) pageContent.Add(tempVal);

            }
            return pageContent;

        }

        //parameter pass: list
        public static List<string> printTradeCardListTemplate(string parent, string category,
            JObject jObjTradingCardList, List<string> arrList)
        {
            List<string> pageContent = new List<string>();
            string title = $"**{GlobalFunctions.UppercaseFirst(parent)} {GlobalFunctions.UppercaseFirst(category)} Card List**\n";

            string tempVal = title;
            int currentIndex = 0;
            for (int i = 0; i < arrList.Count; i++)
            {
                string cardId = arrList[i].ToString();
                string name = jObjTradingCardList[parent][category][cardId]["name"].ToString();
                string url = jObjTradingCardList[parent][category][cardId]["url"].ToString();
                tempVal += $"[{arrList[i]} - {name}]({url})\n";


                if (currentIndex < 14) currentIndex++;
                else
                {
                    pageContent.Add(tempVal);
                    currentIndex = 0;
                    tempVal = title;
                }

                if (i == arrList.Count - 1) pageContent.Add(tempVal);

            }
            return pageContent;

        }

        //public static Tuple<string,string> getCardParentOjamajos(string cardId)
        //{
        //    //return: original parent, related
        //    //Console.WriteLine(parent + " " +category + " "+ cardId + " " + property);
        //    var jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));
        //    string parent = "doremi"; string related = "";
        //    try
        //    {
        //        JToken arrListRelated = jObjTradingCardList["doremi"]["ojamajos"][cardId]["related"][0];
        //        parent = "doremi";
        //        related = arrListRelated.ToString();
        //    }
        //    catch { }

        //    try
        //    {
        //        JToken arrListRelated = jObjTradingCardList["hazuki"]["ojamajos"][cardId]["related"][0];
        //        parent = "hazuki";
        //        related = arrListRelated.ToString();
        //    }
        //    catch { }

        //    try
        //    {
        //        JToken arrListRelated = jObjTradingCardList["aiko"]["ojamajos"][cardId]["related"][0];
        //        parent = "aiko";
        //        related = arrListRelated.ToString();
        //    }
        //    catch { }

        //    try
        //    {
        //        JToken arrListRelated = jObjTradingCardList["onpu"]["ojamajos"][cardId]["related"][0];
        //        parent = "onpu";
        //        related = arrListRelated.ToString();
        //    }
        //    catch { }

        //    try
        //    {
        //        JToken arrListRelated = jObjTradingCardList["momoko"]["ojamajos"][cardId]["related"][0];
        //        parent = "momoko";
        //        related = arrListRelated.ToString();
        //    }
        //    catch { }

        //    return Tuple.Create(parent,related);


        //}

        public static string getCardProperty(string parent, string category,string cardId,string property){
            //Console.WriteLine(parent + " " +category + " "+ cardId + " " + property);
            var jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));
            var arrList = jObjTradingCardList[parent][category][cardId];
            return arrList[property].ToString();
        }

        public static string getCardCategory(string cardId)
        {
            string category;
            if (cardId.ToLower().Contains("dop"))
                category = "platinum";
            else if (cardId.ToLower().Contains("dom"))
                category = "metal";
            else if (cardId.ToLower().Contains("hap"))
                category = "platinum";
            else if (cardId.ToLower().Contains("ham"))
                category = "metal";
            else if (cardId.ToLower().Contains("aip"))
                category = "platinum";
            else if (cardId.ToLower().Contains("aim"))
                category = "metal";
            else if (cardId.ToLower().Contains("onp"))
                category = "platinum";
            else if (cardId.ToLower().Contains("onm"))
                category = "metal";
            else if (cardId.ToLower().Contains("mop"))
                category = "platinum";
            else if (cardId.ToLower().Contains("mom"))
                category = "metal";
            else if (cardId.ToLower().Contains("oj"))
                category = "ojamajos";
            else if (cardId.ToLower().Contains("ot"))
                category = "special";
            else
                category = "normal";
            return category;
        }

        public static string getCardParent(string cardId)
        {
            string parent;
            if (cardId.ToLower().Contains("do"))
                parent = "doremi";
            else if (cardId.ToLower().Contains("ha"))
                parent = "hazuki";
            else if (cardId.ToLower().Contains("ai"))
                parent = "aiko";
            else if (cardId.ToLower().Contains("on"))
                parent = "onpu";
            else if (cardId.ToLower().Contains("mo"))
                parent = "momoko";
            else if (cardId.ToLower().Contains("ot"))
                parent = "special";
            else
                parent = "doremi";
            return parent;
        }

        public static async Task generateCardSpawn(ulong guildId)
        {
            int randomParent = new Random().Next(0, 6);
            int randomCategory = new Random().Next(11);
            int randomMystery = new Random().Next(0, 2);
            int randomBadCard = new Random().Next(0, 12);

            string chosenCategory = ""; string catchRate = ""; string badCardIcon = null;
            Boolean isMystery = false; if (randomMystery <= 0) isMystery = true;

            if (randomCategory <= TradingCardCore.spawnRateOjamajos)//0-1
            {//ojamajos
                chosenCategory = "ojamajos"; catchRate = (TradingCardCore.captureRateOjamajos * 10).ToString() + "%";
            }
            else if (randomCategory <= TradingCardCore.spawnRateMetal)//0-2
            {//metal

                chosenCategory = "metal";
                if (isMystery)
                    catchRate = ((TradingCardCore.captureRateMetal + 2) * 10).ToString() + "%";
                else
                    catchRate = (TradingCardCore.captureRateMetal * 10).ToString() + "%";
            }
            else if (randomCategory <= TradingCardCore.spawnRatePlatinum)//0-5
            {//platinum
                chosenCategory = "platinum";
                if (isMystery)
                    catchRate = ((TradingCardCore.captureRatePlatinum + 1) * 10).ToString() + "%";
                else
                    catchRate = (TradingCardCore.captureRatePlatinum * 10).ToString() + "%";
            }
            else if (randomCategory <= TradingCardCore.spawnRateNormal)//0-10
            {//normal
                chosenCategory = "normal";
                if (isMystery)
                    catchRate = ((TradingCardCore.captureRateNormal + 1) * 10).ToString() + "%";
                else
                    catchRate = (TradingCardCore.captureRateNormal * 10).ToString() + "%";
            }

            string parent = "doremi"; DiscordSocketClient client = Bot.Doremi.client;
            string descriptionMystery = "";
            Discord.Color color = Config.Doremi.EmbedColor; 
            string embedAvatarUrl = "";
            //randomParent = 0; //don't forget to erase this, for testing purpose
            //chosenCategory = "ojamajos";//for testing purpose
            if (randomParent == 0)
            {
                embedAvatarUrl = Config.Doremi.EmbedAvatarUrl;
                descriptionMystery = TradingCardCore.Doremi.arrMysteryDescription[new Random().Next(TradingCardCore.Doremi.arrMysteryDescription.Length)];
            }
            else if (randomParent == 1)
            {
                if (!isMystery) client = Bot.Hazuki.client;
                parent = "hazuki";
                color = Config.Hazuki.EmbedColor; embedAvatarUrl = Config.Hazuki.EmbedAvatarUrl;
                descriptionMystery = TradingCardCore.Hazuki.arrMysteryDescription[new Random().Next(TradingCardCore.Hazuki.arrMysteryDescription.Length)];
            }
            else if (randomParent == 2)
            {
                if (!isMystery) client = Bot.Aiko.client;
                parent = "aiko";
                color = Config.Aiko.EmbedColor; embedAvatarUrl = Config.Aiko.EmbedAvatarUrl;
                descriptionMystery = TradingCardCore.Aiko.arrMysteryDescription[new Random().Next(TradingCardCore.Aiko.arrMysteryDescription.Length)];
            }
            else if (randomParent == 3)
            {
                if (!isMystery) client = Bot.Onpu.client;
                parent = "onpu";
                color = Config.Onpu.EmbedColor; embedAvatarUrl = Config.Onpu.EmbedAvatarUrl;
                descriptionMystery = TradingCardCore.Onpu.arrMysteryDescription[new Random().Next(TradingCardCore.Onpu.arrMysteryDescription.Length)];
            }
            else if (randomParent == 4)
            {
                if (!isMystery) client = Bot.Momoko.client;
                parent = "momoko";
                color = Config.Momoko.EmbedColor; embedAvatarUrl = Config.Momoko.EmbedAvatarUrl;
                descriptionMystery = TradingCardCore.Momoko.arrMysteryDescription[new Random().Next(TradingCardCore.Momoko.arrMysteryDescription.Length)];
            }
            else if (randomParent >= 5)
            {
                chosenCategory = "special"; parent = "other";
                color = Config.Doremi.EmbedColor; embedAvatarUrl = Config.Doremi.EmbedAvatarUrl;
                catchRate = (TradingCardCore.captureRateSpecial * 10).ToString() + "%";
            }


            string author = $"{GlobalFunctions.UppercaseFirst(parent)} {GlobalFunctions.UppercaseFirst(chosenCategory)} Card";
            if (chosenCategory == "ojamajos")
                author = $"{GlobalFunctions.UppercaseFirst(chosenCategory)} Card";
            else if (chosenCategory == "special")
                author = $"Other {GlobalFunctions.UppercaseFirst(chosenCategory)} Card";
            

            string footerBadCard = "";
            //start read json
            JObject jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));
            
            //using (var stream = File.Open($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json", FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
            //{
            //    using var sr = new StreamReader(stream);
            //    jObjTradingCardList = JObject.Parse(sr.ReadToEnd());
            //}

            var key = JObject.Parse(jObjTradingCardList[parent][chosenCategory].ToString()).Properties().ToList();
            int randIndex = 0;
            int timedLoop = Convert.ToInt32(DateTime.Now.ToString("dd"));
            for(int i = 0; i <= timedLoop; i++)
            {
                randIndex = new Random().Next(0, key.Count);
            }

            //chosen data:
            string chosenId = key[randIndex].Name;
            string chosenName = jObjTradingCardList[parent][chosenCategory][key[randIndex].Name]["name"].ToString();
            string chosenUrl = jObjTradingCardList[parent][chosenCategory][key[randIndex].Name]["url"].ToString();

            //reset default & assign all
            Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyId, chosenId);
            Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyCategory, chosenCategory);
            Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyToken, GlobalFunctions.RandomString(8));
            Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyMystery, "0");
            Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCard, "");
            Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardEquation, "");
            Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardNumber1, "");
            Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardNumber2, "");

            //bad card trigger
            if (randomBadCard <= 1)
            {
                int randomBadCardType = new Random().Next(0, 3);
                badCardIcon = TradingCardCore.BadCards.embedFooterUrl;
                Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCard, $"{randomBadCardType}");
                int randomEquation = new Random().Next(0, 2);
                int randomNumber1 = new Random().Next(50, 201);
                int randomNumber2 = new Random().Next(50, 201);

                if (randomNumber1 < randomNumber2)
                {
                    int tempNumbers = randomNumber1;
                    randomNumber1 = randomNumber2;
                    randomNumber2 = tempNumbers;
                }

                if (randomEquation == 0)
                    Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardEquation, "+");
                else
                    Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardEquation, "-");

                Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardNumber1, randomNumber1.ToString());
                Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardNumber2, randomNumber2.ToString());
                footerBadCard += $"-{TradingCardCore.BadCards.getType(randomBadCardType)}";
            }

            if (!isMystery || chosenCategory == "ojamajos" || chosenCategory == "special")
            {//not mystery
                EmbedBuilder embed;
                if (chosenCategory == "ojamajos" || chosenCategory == "special")
                    embed = new EmbedBuilder()
                    .WithAuthor(author)
                    .WithColor(Discord.Color.Gold)
                    .WithTitle($"{chosenName}")
                    .WithFooter($"ID: {chosenId}{footerBadCard} | Catch Rate: {catchRate}")
                    .WithThumbnailUrl(badCardIcon)
                    .WithImageUrl(chosenUrl);
                else
                    embed = new EmbedBuilder()
                    .WithAuthor(author, embedAvatarUrl)
                    .WithColor(color)
                    .WithTitle($"{chosenName}")
                    .WithFooter($"ID: {chosenId}{footerBadCard} | Catch Rate: {catchRate}")
                    .WithThumbnailUrl(badCardIcon)
                    .WithImageUrl(chosenUrl);

                if (chosenCategory == "ojamajos") parent = "";

                await client
                .GetGuild(guildId)
                .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guildId, "trading_card_spawn")))
                .SendMessageAsync($":exclamation:A **{chosenCategory}** {parent} card has appeared! " +
                $"Capture it with **<bot>!card capture** or **<bot>!card capture boost**",
                embed: embed.Build());
            }
            else
            {//mystery card
                var embed = new EmbedBuilder()
                .WithAuthor("Mystery Card")
                .WithColor(Discord.Color.DarkerGrey)
                .WithTitle($"🔍 Revealed Hint:")
                .WithDescription(descriptionMystery)
                //.WithImageUrl("https://cdn.discordapp.com/attachments/709293222387777626/710869697972797440/mystery.jpg")
                .WithImageUrl("https://cdn.discordapp.com/attachments/709293222387777626/722780058904821760/mystery.gif")
                .WithThumbnailUrl(badCardIcon)
                .WithFooter($"ID: ???{footerBadCard} | Catch Rate: {catchRate}");

                Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyMystery, "1");
                await client
                .GetGuild(guildId)
                .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guildId, "trading_card_spawn")))
                .SendMessageAsync($":question:A **mystery** card has appeared! Can you guess whose card is this belongs to?\n" +
                $"Reveal & capture it with **<bot>!card capture** or **<bot>!card capture boost**",
                embed: embed.Build());
            }
        }

        public static async Task printCardSpawned(ulong guildId)
        {
            string chosenId = Config.Guild.getPropertyValue(guildId, propertyId);
            string chosenCategory = getCardCategory(Config.Guild.getPropertyValue(guildId, propertyCategory)); 
            string catchRate = ""; string footerIconUrl = null;
            Boolean isMystery = false;
            if(Config.Guild.getPropertyValue(guildId,propertyMystery) == "1") isMystery = true;

            if (chosenCategory == "ojamajos")//0-1
            {//ojamajos
                catchRate = (TradingCardCore.captureRateOjamajos * 10).ToString() + "%";
            }
            else if (chosenCategory == "metal")//0-2
            {//metal
                if (isMystery)
                    catchRate = ((TradingCardCore.captureRateMetal + 2) * 10).ToString() + "%";
                else
                    catchRate = (TradingCardCore.captureRateMetal * 10).ToString() + "%";
            }
            else if (chosenCategory == "platinum")//0-5
            {//platinum
                if (isMystery)
                    catchRate = ((TradingCardCore.captureRatePlatinum + 1) * 10).ToString() + "%";
                else
                    catchRate = (TradingCardCore.captureRatePlatinum * 10).ToString() + "%";
            }
            else if (chosenCategory == "normal")//0-10
            {//normal
                if (isMystery)
                    catchRate = ((TradingCardCore.captureRateNormal + 1) * 10).ToString() + "%";
                else
                    catchRate = (TradingCardCore.captureRateNormal * 10).ToString() + "%";
            }

            string parent = getCardParent(Config.Guild.getPropertyValue(guildId, propertyId)); 
            DiscordSocketClient client = Bot.Doremi.client;
            string descriptionMystery = "";
            Discord.Color color = Config.Doremi.EmbedColor;
            string author = $"{GlobalFunctions.UppercaseFirst(parent)} {GlobalFunctions.UppercaseFirst(chosenCategory)} Card";
            string embedAvatarUrl = "";
            if (parent == "doremi")
            {
                embedAvatarUrl = Config.Doremi.EmbedAvatarUrl;
                descriptionMystery = TradingCardCore.Doremi.arrMysteryDescription[new Random().Next(TradingCardCore.Doremi.arrMysteryDescription.Length)];
            }
            else if (parent == "hazuki")
            {
                if (!isMystery) client = Bot.Hazuki.client;
                color = Config.Hazuki.EmbedColor; embedAvatarUrl = Config.Hazuki.EmbedAvatarUrl;
                descriptionMystery = TradingCardCore.Hazuki.arrMysteryDescription[new Random().Next(TradingCardCore.Hazuki.arrMysteryDescription.Length)];
            }
            else if (parent == "aiko")
            {
                if (!isMystery) client = Bot.Aiko.client;
                color = Config.Aiko.EmbedColor; embedAvatarUrl = Config.Aiko.EmbedAvatarUrl;
                descriptionMystery = TradingCardCore.Aiko.arrMysteryDescription[new Random().Next(TradingCardCore.Aiko.arrMysteryDescription.Length)];
            }
            else if (parent == "onpu")
            {
                if (!isMystery) client = Bot.Onpu.client;
                color = Config.Onpu.EmbedColor; embedAvatarUrl = Config.Onpu.EmbedAvatarUrl;
                descriptionMystery = TradingCardCore.Onpu.arrMysteryDescription[new Random().Next(TradingCardCore.Onpu.arrMysteryDescription.Length)];
            }
            else if (parent == "momoko")
            {
                if (!isMystery) client = Bot.Momoko.client;
                color = Config.Momoko.EmbedColor; embedAvatarUrl = Config.Momoko.EmbedAvatarUrl;
                descriptionMystery = TradingCardCore.Momoko.arrMysteryDescription[new Random().Next(TradingCardCore.Momoko.arrMysteryDescription.Length)];
            }
            else if (parent == "other")
            {
                chosenCategory = "special"; parent = "other";
                color = Config.Doremi.EmbedColor; embedAvatarUrl = Config.Doremi.EmbedAvatarUrl;
                catchRate = (TradingCardCore.captureRateSpecial * 10).ToString() + "%";
            }

            if (chosenCategory == "ojamajos")
                author = $"{GlobalFunctions.UppercaseFirst(chosenCategory)} Card";
            else if (chosenCategory == "special")            
                author = $"Other {GlobalFunctions.UppercaseFirst(chosenCategory)} Card";

            string footerBadCard = "-";
            //start read json
            var jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));

            //chosen data:
            string chosenName = ""; string chosenUrl = "";
            try
            {
                chosenName = jObjTradingCardList[parent][chosenCategory][chosenId]["name"].ToString();
                chosenUrl = jObjTradingCardList[parent][chosenCategory][chosenId]["url"].ToString();
            } catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
              
            int badCard = Convert.ToInt32(Config.Guild.getPropertyValue(guildId, BadCards.propertyBadCard));

            if (badCard <= 2)
            {
                footerIconUrl = BadCards.embedFooterUrl;
                footerBadCard += $"{BadCards.getType(badCard)}";
            }

            if (!isMystery || chosenCategory == "ojamajos" || chosenCategory == "special")
            {//not mystery
                EmbedBuilder embed;
                if (chosenCategory == "ojamajos" || chosenCategory == "special")
                    embed = new EmbedBuilder()
                    .WithAuthor(author)
                    .WithColor(Discord.Color.Gold)
                    .WithTitle($"{chosenName}")
                    .WithFooter($"ID: {chosenId}{footerBadCard} | Catch Rate: {catchRate}", footerIconUrl)
                    .WithImageUrl(chosenUrl);
                else
                    embed = new EmbedBuilder()
                    .WithAuthor(author, embedAvatarUrl)
                    .WithColor(color)
                    .WithTitle($"{chosenName}")
                    .WithFooter($"ID: {chosenId}{footerBadCard} | Catch Rate: {catchRate}", footerIconUrl)
                    .WithImageUrl(chosenUrl);

                if (chosenCategory == "ojamajos") parent = "";

                await client
                .GetGuild(guildId)
                .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guildId, "trading_card_spawn")))
                .SendMessageAsync($":exclamation:A **{chosenCategory}** {parent} card has appeared! " +
                $"Capture it with **<bot>!card capture** or **<bot>!card capture boost**",
                embed: embed.Build());
            }
            else
            {//mystery card
                var embed = new EmbedBuilder()
                .WithAuthor("Mystery Card")
                .WithColor(Discord.Color.DarkerGrey)
                .WithTitle($"🔍 Revealed Hint:")
                .WithDescription(descriptionMystery)
                .WithImageUrl("https://cdn.discordapp.com/attachments/709293222387777626/710869697972797440/mystery.jpg")
                .WithFooter($"ID: ???{footerBadCard} | Catch Rate: {catchRate}", footerIconUrl);

                Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyMystery, "1");
                await client
                .GetGuild(guildId)
                .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guildId, "trading_card_spawn")))
                .SendMessageAsync($":question:A **mystery** card has appeared! Can you guess whose card is this belongs to?\n" +
                $"Reveal & capture it with **<bot>!card capture** or **<bot>!card capture boost**",
                embed: embed.Build());
            }
        }

        //json format:
        // "trading_queue": {
        //  "01929183481": ["do","on"]
        //}

        public class Doremi {
            public static int maxNormal = 48; public static int maxPlatinum = 8; public static int maxMetal = 6;
            public static int maxOjamajos = 5;

            public static string parent = "doremi";
            public static string emojiOk = "https://cdn.discordapp.com/attachments/706490547191152690/706511135788105728/143751262x.png";
            public static string emojiError = "https://cdn.discordapp.com/attachments/706490547191152690/706494009991757864/doremi.png";
            public static string imgCompleteAllCard = $"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/badge/badge_doremi.png";

            public static string embedName = "Doremi Trading Card Hub";
            public static string roleCompletionist = "Doremi Card Badge";

            public static string getCardCategory(string cardId)
            {
                string category = "";
                if (cardId.ToLower().Contains("dop"))
                    category = "platinum";
                else if (cardId.ToLower().Contains("dom"))
                    category = "metal";
                else
                    category = "normal";
                return category;
            }

            public static string[] arrMysteryDescription = {
                ":birthday: July is my birthday",
                "Dodo is my fairy",
                ":birthday: February, May, March and November are not my birthday",
                ":birthday: My birthday was at 30th",
                ":sparkles: **Pirika** is one of my chanting spell",
                ":sparkles: **Pirilala** is one of my chanting spell",
                ":sparkles: **Poporina** is one of my chanting spell",
                ":sparkles: **Peperuto** is one of my chanting spell",
                ":sparkles: **Paipai Poppun Famifami Pon!** are not my spell",
                ":sparkles: **Faa Puwapuwa Pon Rarirori!** are not my spell",
                ":sparkles: **Puu Raruku Purun Perutan!** are not my spell",
                ":sparkles: **Puu Poppun Faa Pon!** are not my spell",
                ":sparkles: **Petton Puu Pameruku Faa!** are not my spell",
                ":sparkles: **Famifami Rarirori Paipai Petton!** are not my spell"
            };

        }

        public class Hazuki {
            public static int maxNormal = 46; public static int maxPlatinum = 9; public static int maxMetal = 6;
            public static int maxOjamajos = 5;
            public static string emojiError = "https://cdn.discordapp.com/attachments/706490547191152690/706494023782629386/hazuki.png";
            public static string imgCompleteAllCard = $"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/badge/badge_hazuki.png";

            public static string roleCompletionist = "Hazuki Card Badge";

            public static string getCardCategory(string cardId)
            {
                string category;
                if (cardId.ToLower().Contains("hap"))
                    category = "platinum";
                else if (cardId.ToLower().Contains("ham"))
                    category = "metal";
                else if (cardId.ToLower().Contains("oj"))
                    category = "ojamajos";
                else
                    category = "normal";
                return category;
            }

            public static string[] arrMysteryDescription = {
                ":birthday: February is my birthday",
                "Rere is my fairy",
                ":birthday: May, July, March and November are not my birthday month",
                ":birthday: My birthday is the same day of the month as Aiko but I was born a few months earlier",
                ":drop_of_blood: My blood type was A",
                ":fork_and_knife: One of my favorite food ends with **e**",
                ":fork_and_knife: One of my favorite food start with **ch**",
                ":sparkles: **Paipai** is one of my chanting spell",
                ":sparkles: **Ponpoi** is one of my chanting spell",
                ":sparkles: **Puwapuwa** is one of my chanting spell",
                ":sparkles: **Puu** is one of my chanting spell",
                ":sparkles: **Raruku Famifami Pirika Pon!** are not my spell",
                ":sparkles: **Pararira Faa Rarirori Poporina!** are not my spell",
                ":sparkles: **Poppun Pirika Faa Perutan!** are not my spell",
                ":sparkles: **Rarirori Peperuto Perutan Purun!** are not my spell"
            };

        }

        public class Aiko{
            public static int maxNormal = 45; public static int maxPlatinum = 7; public static int maxMetal = 6;
            public static int maxOjamajos = 5;
            public static string emojiError = "https://cdn.discordapp.com/attachments/706490547191152690/706494032976674856/aiko.jpg";
            public static string imgCompleteAllCard = $"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/badge/badge_aiko.png";

            public static string roleCompletionist = "Aiko Card Badge";

            public static string getCardCategory(string cardId)
            {
                string category;
                if (cardId.ToLower().Contains("aip"))
                    category = "platinum";
                else if (cardId.ToLower().Contains("aim"))
                    category = "metal";
                else if (cardId.ToLower().Contains("oj"))
                    category = "ojamajos";
                else
                    category = "normal";
                return category;
            }

            public static string[] arrMysteryDescription = {
                ":birthday: November is my birthday",
                "Mimi is my fairy",
                ":birthday: July, February, March and May are not my birthday",
                ":birthday: My birthday is the same day of the month as Hazuki but I was born a few months older",
                ":drop_of_blood: My blood type was O",
                ":fork_and_knife: One of my favorite food ends with **i**",
                ":fork_and_knife: One of my favorite food start with **t**",
                ":sparkles: **Pameruku** is one of my chanting spell",
                ":sparkles: **Raruku** is one of my chanting spell",
                ":sparkles: **Rarirori** is one of my chanting spell",
                ":sparkles: **Poppun** is one of my chanting spell",
                ":sparkles: **Famifami Pon Ponpoi Pirika!** are not my spell",
                ":sparkles: **Peperuto Puwapuwa Purun Perutan!** are not my spell",
                ":sparkles: **Ponpoi Purun Pirilala Petton!** are not my spell",
                ":sparkles: **Famifami Pararira Puwapuwa Poporina!** are not my spell",
                ":sparkles: **Pururun Paipai Perutan Pirika!** are not my spell",
                ":sparkles: **Puu Pon Faa Peperuto!** are not my spell"
            };

        }

        public class Onpu
        {
            public static int maxNormal = 46; public static int maxPlatinum = 13; public static int maxMetal = 6;
            public static int maxOjamajos = 6;
            public static string emojiError = "https://cdn.discordapp.com/attachments/706490547191152690/706494042631962666/onpu.jpg";
            public static string imgCompleteAllCard = $"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/badge/badge_onpu.png";

            public static string roleCompletionist = "Onpu Card Badge";

            public static string getCardCategory(string cardId)
            {
                string category;
                if (cardId.ToLower().Contains("onp"))
                    category = "platinum";
                else if (cardId.ToLower().Contains("onm"))
                    category = "metal";
                else if (cardId.ToLower().Contains("oj"))
                    category = "ojamajos";
                else
                    category = "normal";
                return category;
            }

            public static string[] arrMysteryDescription = {
                ":birthday: March is my birthday",
                "Roro is my fairy",
                ":birthday: July, February, November and May are not my birthday",
                ":birthday: My birthday was at 3rd",
                ":fork_and_knife: One of my favorite food ends with **s**",
                ":fork_and_knife: One of my favorite food start with **cr**",
                ":sparkles: **Pururun** is one of my chanting spell",
                ":sparkles: **Purun** is one of my chanting spell",
                ":sparkles: **Famifami** is one of my chanting spell",
                ":sparkles: **Faa** is one of my chanting spell",
                ":sparkles: **Rarirori Pirika Ponpoi Pon!** are not my spell",
                ":sparkles: **Puwapuwa Peperuto Raruku Perutan!** are not my spell",
                ":sparkles: **Ponpoi Raruku Petton Pirilala!** are not my spell",
                ":sparkles: **Poporina Puwapuwa Rarirori Pararira!** are not my spell",
                ":sparkles: **Peperuto Pon Poppun Puu!** are not my spell",
                ":sparkles: **Paipai Pirika Pameruku Perutan!** are not my spell"
            };

        }

        public class Momoko
        {
            public static int maxNormal = 43; public static int maxPlatinum = 6; public static int maxMetal = 4;
            public static int maxOjamajos = 5;
            public static string emojiError = "https://cdn.discordapp.com/attachments/706490547191152690/706769235019300945/Linesticker21.png";
            public static string imgCompleteAllCard = $"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/badge/badge_momoko.png";

            public static string roleCompletionist = "Momoko Card Badge";

            public static string getCardCategory(string cardId)
            {
                string category;
                if (cardId.ToLower().Contains("mop"))
                    category = "platinum";
                else if (cardId.ToLower().Contains("mom"))
                    category = "metal";
                else if (cardId.ToLower().Contains("oj"))
                    category = "ojamajos";
                else
                    category = "normal";
                return category;
            }

            public static string[] arrMysteryDescription = {
                ":birthday: May is my birthday",
                "Nini is my fairy",
                ":drop_of_blood: My blood type was AB",
                ":birthday: July, February, November and March are not my birthday",
                ":birthday: My birthday was at 6th",
                ":fork_and_knife: One of my favorite food ends with **t**",
                ":fork_and_knife: One of my favorite food start with **s**",
                ":sparkles: **Perutan** is one of my chanting spell",
                ":sparkles: **Petton** is one of my chanting spell",
                ":sparkles: **Pararira** is one of my chanting spell",
                ":sparkles: **Pon** is one of my chanting spell",
                ":sparkles: **Ponpoi Pirika Faa Rarirori!** are not my spell",
                ":sparkles: **Raruku Puwapuwa Peperuto Pururun!** are not my spell",
                ":sparkles: **Purun Ponpoi Pirilala Raruku!** are not my spell",
                ":sparkles: **Rarirori Poporina Famifami Puwapuwa!** are not my spell",
                ":sparkles: **Faa Poppun Puu Peperuto!** are not my spell",
                ":sparkles: **Pururun Pameruku Pirika Paipai!** are not my spell"
            };

        }

        public class BadCards
        {
            public static string embedFooterUrl = "https://cdn.discordapp.com/attachments/706770454697738300/715945298370887722/latest.png";
            public static string embedPureleineAvatar = "https://cdn.discordapp.com/attachments/706770454697738300/715959473889476698/oyajide.jpg";
            public static string imgPureleineFound = "https://cdn.discordapp.com/attachments/706770454697738300/715951866768261180/found.jpg";
            public static string imgPureleineNotFound = "https://cdn.discordapp.com/attachments/706770454697738300/715951867561246820/not-found.jpg";
            public static string imgAnswerWrong = "https://cdn.discordapp.com/attachments/706770454697738300/715951862964158464/error.jpg";
            public static string imgAnswerCorrect = "https://cdn.discordapp.com/attachments/706770454697738300/715951861781233785/correct.jpg";
            public static string imgBadCardActivated = "https://cdn.discordapp.com/attachments/706770454697738300/716029107938525285/latest.png";

            public static string propertyBadCard = "trading_card_spawn_badcard";
            public static string propertyBadCardNumber1 = "trading_card_spawn_badcard_number1";
            public static string propertyBadCardEquation = "trading_card_spawn_badcard_equation";
            public static string propertyBadCardNumber2 = "trading_card_spawn_badcard_number2";

            public static string embedPureleineName = "Oyajide";
            public static Color embedPureleineColor = new Discord.Color(196, 156, 9);

            public static string getType(int type)
            {
                string returnType = "seeds";
                if (type == 0)
                    returnType = "curse";
                else if (type == 1)
                    returnType = "failure";
                return returnType;
            }

        }

    }
}
