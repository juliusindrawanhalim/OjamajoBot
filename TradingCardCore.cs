using Discord;
using Lavalink4NET.Statistics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OjamajoBot
{
    public class TradingCardCore
    {
        public static string version = "1.04";
        public static string propertyId = "trading_card_spawn_id";
        public static string propertyCategory = "trading_card_spawn_category";
        public static string propertyToken = "trading_card_spawn_token";

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

        public static string imgMagicSeeds = "https://cdn.discordapp.com/attachments/706770454697738300/709013040518922260/magic_seeds.jpg";

        public static EmbedBuilder printUpdatesNote()
        {
            return new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithTitle($"Doremi Trading Card - Update {version} - 13.05.20")
                .WithDescription($"-You can now select inventory category for each bot with additional inventory parameter. " +
                $"Example: **{Config.Doremi.PrefixParent[0]}card inventory platinum**.\n" +
                $"-Updated card display image layout\n" +
                $"-Card catch &spawn rate can be displayed with **{Config.Doremi.PrefixParent[0]}card rate**");
        }

        public static List<string> printInventoryTemplate(string pack, string parent, string category,
            JObject jObjTradingCardList, JArray arrData, int maxAmount,ulong userId)
        {
            List<string> pageContent = new List<string>();
            var arrList = (JArray)arrData;
            string title = $"**{MentionUtils.MentionUser(Convert.ToUInt64(userId))} {GlobalFunctions.UppercaseFirst(pack)} {GlobalFunctions.UppercaseFirst(category)} Card ({arrList.Count}/{maxAmount})**\n";

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
                .WithTitle($"**{username} Card Status Boost**\n")
                .WithDescription("Note: Using a boost for any category on a card pack will remove all that boost status.")
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

        public static EmbedBuilder printCardDetailTemplate(Color color, string name, string imgUrl, string card_id,
            string category, string rank, string star, string point)
        {
            return new EmbedBuilder()
                .WithAuthor(name)
                .WithColor(color)
                .AddField("ID", card_id, true)
                .AddField("Category", category, true)
                .AddField("Rank", rank, true)
                .AddField("⭐", star, true)
                .AddField("Point", point, true)
                .WithImageUrl(imgUrl);
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

            if (doremiText == "") doremiText = "No one has complete their Doremi card pack yet.";
            if (hazukiText == "") hazukiText = "No one has complete their Hazuki card pack yet.";
            if (aikoText == "") aikoText = "No one has complete their Aiko card pack yet.";
            if (onpuText == "") onpuText = "No one has complete their Onpu card pack yet.";
            if (momokoText == "") momokoText = "No one has complete their Momoko card pack yet.";
            if (otherText == "") otherText = "No one has complete their Other card pack yet.";
            
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

        public static EmbedBuilder userCompleteTheirList(Color color, string parent, string congratulateText, string imgUrl, string guildId, string clientId)
        {
            //update & save leaderboard data
            string dateTimeNow = DateTime.Now.ToString("MM/dd/yyyy");
            string leaderboardDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/trading_card_leaderboard_data.json";
            var jObjLeaderboard = JObject.Parse(File.ReadAllText(leaderboardDataDirectory));
            ((JObject)jObjLeaderboard[parent]).Add(clientId, dateTimeNow);
            File.WriteAllText(leaderboardDataDirectory, jObjLeaderboard.ToString());

            //return congratulate embed
            return new EmbedBuilder()
            .WithColor(color)
            .WithImageUrl(imgUrl)
            .WithFooter($"Completed at: {dateTimeNow}")
            .WithDescription(congratulateText);
        }

        public static EmbedBuilder printCardCaptureTemplate(Color color, string name, string imgUrl, string card_id,
            string category, string rank, string star, string point, string username, string botIconUrl)
        {
            return new EmbedBuilder()
                    .WithAuthor(name)
                    .WithColor(Config.Doremi.EmbedColor)
                    .AddField("ID", card_id, true)
                    .AddField("Category", category, true)
                    .AddField("Rank", rank, true)
                    .AddField("⭐", star, true)
                    .AddField("Point", point, true)
                    .WithImageUrl(imgUrl)
                    .WithFooter($"Captured by: {username}",botIconUrl);
        }

        public static void resetSpawnInstance(ulong guildId)
        {
            Config.Guild.setPropertyValue(guildId, propertyId, "");
            Config.Guild.setPropertyValue(guildId, propertyCategory, "");
            Config.Guild.setPropertyValue(guildId, propertyToken, "");
        }

        public static EmbedBuilder printStatusTemplate(Color color, string username, string guildId, string clientId)
        {
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
            var jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));
            var playerData = JObject.Parse(File.ReadAllText(playerDataDirectory));
            var arrListDoremi = playerData["doremi"];
            var arrListHazuki = playerData["hazuki"];
            var arrListAiko = playerData["aiko"];
            var arrListOnpu = playerData["onpu"];
            var arrListMomoko = playerData["momoko"];
            var arrListOther = playerData["other"];

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

            string hazukiText = $"**Normal: {((JArray)arrListHazuki["normal"]).Count}/{Hazuki.maxNormal}**\n" +
                $"**Platinum: {((JArray)arrListHazuki["platinum"]).Count}/{Hazuki.maxPlatinum}**\n" +
                $"**Metal: {((JArray)arrListHazuki["metal"]).Count}/{Hazuki.maxMetal}**\n" +
                $"**Ojamajos: {((JArray)arrListHazuki["ojamajos"]).Count}/{Hazuki.maxOjamajos}**";

            string aikoText = $"**Normal: {((JArray)arrListAiko["normal"]).Count}/{Aiko.maxNormal}**\n" +
                $"**Platinum: {((JArray)arrListAiko["platinum"]).Count}/{Aiko.maxPlatinum}**\n" +
                $"**Metal: {((JArray)arrListAiko["metal"]).Count}/{Aiko.maxMetal}**\n" +
                $"**Ojamajos: {((JArray)arrListAiko["ojamajos"]).Count}/{Aiko.maxOjamajos}**";

            string onpuText = $"**Normal: {((JArray)arrListOnpu["normal"]).Count}/{Onpu.maxNormal}**\n" +
                $"**Platinum: {((JArray)arrListOnpu["platinum"]).Count}/{Onpu.maxPlatinum}**\n" +
                $"**Metal: {((JArray)arrListOnpu["metal"]).Count}/{Onpu.maxMetal}**\n" +
                $"**Ojamajos: {((JArray)arrListOnpu["ojamajos"]).Count}/{Onpu.maxOjamajos}**";

            string momokoText = $"**Normal: {((JArray)arrListMomoko["normal"]).Count}/{Momoko.maxNormal}**\n" +
                $"**Platinum: {((JArray)arrListMomoko["platinum"]).Count}/{Momoko.maxPlatinum}**\n" +
                $"**Metal: {((JArray)arrListMomoko["metal"]).Count}/{Momoko.maxMetal}**\n" +
                $"**Ojamajos: {((JArray)arrListMomoko["ojamajos"]).Count}/{Momoko.maxOjamajos}**";

            string otherText = $"**Special: {((JArray)arrListOther["special"]).Count}/{maxSpecial}**";

            return new EmbedBuilder()
                .WithTitle($"{username} Card Status Report")
                .WithColor(color)
                .AddField("Catch Attempt", $"**{totalSuccess} / {playerData["catch_attempt"].ToString()}**", false)
                .AddField("Doremi Card Pack", doremiText, true)
                .AddField("Hazuki Card Pack", hazukiText, true)
                .AddField("Aiko Card Pack", aikoText, true)
                .AddField("Onpu Card Pack", onpuText, true)
                .AddField("Momoko Card Pack", momokoText, true)
                .AddField("Other Card Pack", otherText, true);

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
            //hazuki
            if (((JArray)(arrListHazuki["normal"])).Count >= 1)
                listAllowed.Add("hazuki normal");
            if (((JArray)(arrListHazuki["platinum"])).Count >= 1)
                listAllowed.Add("hazuki platinum");
            if (((JArray)(arrListHazuki["metal"])).Count >= 1)
                listAllowed.Add("hazuki metal");
            //aiko
            if (((JArray)(arrListAiko["normal"])).Count >= 1)
                listAllowed.Add("aiko normal");
            if (((JArray)(arrListAiko["platinum"])).Count >= 1)
                listAllowed.Add("aiko platinum");
            if (((JArray)(arrListAiko["metal"])).Count >= 1)
                listAllowed.Add("aiko metal");
            //onpu
            if (((JArray)(arrListOnpu["normal"])).Count >= 1)
                listAllowed.Add("onpu normal");
            if (((JArray)(arrListOnpu["platinum"])).Count >= 1)
                listAllowed.Add("onpu platinum");
            if (((JArray)(arrListOnpu["metal"])).Count >= 1)
                listAllowed.Add("onpu metal");
            //momoko
            if (((JArray)(arrListMomoko["normal"])).Count >= 1)
                listAllowed.Add("momoko normal");
            if (((JArray)(arrListMomoko["platinum"])).Count >= 1)
                listAllowed.Add("momoko platinum");
            if (((JArray)(arrListMomoko["metal"])).Count >= 1)
                listAllowed.Add("momoko metal");

            return listAllowed;
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

        public static string getCardProperty(string parent, string category,string cardId,string property){
            //Console.WriteLine(parent + " " +category + " "+ cardId + " " + property);
            var jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));
            var arrList = jObjTradingCardList[parent][category][cardId];
            return arrList[property].ToString();
        }

        public static string getCardCategory(string cardId)
        {
            string category = "";
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
            else
                category = "normal";
            return category;
        }

        public static string getCardParent(string cardId)
        {
            string category = "";
            if (cardId.ToLower().Contains("do"))
                category = "doremi";
            else if (cardId.ToLower().Contains("ha"))
                category = "hazuki";
            else if (cardId.ToLower().Contains("ai"))
                category = "aiko";
            else if (cardId.ToLower().Contains("on"))
                category = "onpu";
            else if (cardId.ToLower().Contains("mo"))
                category = "momoko";
            else if (cardId.ToLower().Contains("ot"))
                category = "special";
            else if (cardId.ToLower().Contains("oj"))
                category = "ojamajos";
            else
                category = "normal";
            return category;
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
            public static string emojiCompleteAllCard = "https://cdn.discordapp.com/attachments/706490547191152690/707424151723311154/win1.jpg";

            public static string embedName = "Doremi Trading Card Hub";

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
        
        }

        public class Hazuki {
            public static int maxNormal = 46; public static int maxPlatinum = 9; public static int maxMetal = 6;
            public static int maxOjamajos = 5;
            public static string emojiError = "https://cdn.discordapp.com/attachments/706490547191152690/706494023782629386/hazuki.png";
            public static string emojiCompleteAllCard = "https://cdn.discordapp.com/attachments/706490547191152690/707424248872042568/win1.jpg";
            public static string getCardCategory(string cardId)
            {
                string category = "";
                if (cardId.ToLower().Contains("hap"))
                    category = "platinum";
                else if (cardId.ToLower().Contains("ham"))
                    category = "metal";
                else
                    category = "normal";
                return category;
            }

        }

        public class Aiko{
            public static int maxNormal = 45; public static int maxPlatinum = 7; public static int maxMetal = 6;
            public static int maxOjamajos = 5;
            public static string emojiError = "https://cdn.discordapp.com/attachments/706490547191152690/706494032976674856/aiko.jpg";
            public static string emojiCompleteAllCard = "https://cdn.discordapp.com/attachments/706490547191152690/707424297685090344/win1.jpg";
            public static string getCardCategory(string cardId)
            {
                string category = "";
                if (cardId.ToLower().Contains("aip"))
                    category = "platinum";
                else if (cardId.ToLower().Contains("aim"))
                    category = "metal";
                else
                    category = "normal";
                return category;
            }
        }

        public class Onpu
        {
            public static int maxNormal = 46; public static int maxPlatinum = 13; public static int maxMetal = 6;
            public static int maxOjamajos = 6;
            public static string emojiError = "https://cdn.discordapp.com/attachments/706490547191152690/706494042631962666/onpu.jpg";
            public static string emojiCompleteAllCard = "https://cdn.discordapp.com/attachments/706490547191152690/707424375380508682/win2.jpg";
            public static string getCardCategory(string cardId)
            {
                string category = "";
                if (cardId.ToLower().Contains("onp"))
                    category = "platinum";
                else if (cardId.ToLower().Contains("onm"))
                    category = "metal";
                else
                    category = "normal";
                return category;
            }

        }

        public class Momoko
        {
            public static int maxNormal = 43; public static int maxPlatinum = 6; public static int maxMetal = 4;
            public static int maxOjamajos = 5;
            public static string emojiError = "https://cdn.discordapp.com/attachments/706490547191152690/706769235019300945/Linesticker21.png";
            public static string emojiCompleteAllCard = "https://cdn.discordapp.com/attachments/706490547191152690/707424504120344576/win5.jpg";

            public static string getCardCategory(string cardId)
            {
                string category = "";
                if (cardId.ToLower().Contains("mop"))
                    category = "platinum";
                else if (cardId.ToLower().Contains("mom"))
                    category = "metal";
                else
                    category = "normal";
                return category;
            }

        }

    }
}
