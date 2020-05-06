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
        public static string version = "1.01";
        public static string propertyId = "trading_card_spawn_id";
        public static string propertyCategory = "trading_card_spawn_category";
        public static string propertyToken = "trading_card_spawn_token";

        public static EmbedBuilder printUpdatesNote()
        {
            return new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithTitle($"Doremi Trading Card - Update {version} - 06.06.20")
                .WithDescription("-New optional capture command:`card info/card look`\n" +
                "-New optional capture command: `card catch`\n" +
                "-New command: `card status` [let you see all the cards & total that you own for all card pack]\n" +
                "-`card status` command allow you to track down your catching attempt\n" +
                "-New command: `card leaderboard` [let you see top 5 user that capture all the card for each pack]\n" +
                "-Updates on card spawn display\n" +
                "-Updates on spawn rate & catch rate\n" +
                "-Dialogue updates on card capture\n" +
                "-Additional randomized timer for spawn interval between 5-10 minutes");
        }

        public static List<string> printInventoryTemplate(string pack, string parent, string category,
            JObject jObjTradingCardList, JArray arrData, int maxAmount)
        {
            List<string> pageContent = new List<string>();
            var arrList = (JArray)arrData;
            string title = $"**{GlobalFunctions.UppercaseFirst(pack)} Card - {GlobalFunctions.UppercaseFirst(category)} Category ({arrList.Count}/{maxAmount})**\n";

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

        public static EmbedBuilder printEmptyInventoryTemplate(Color color, string pack, string category, int maxAmount)
        {
            return new EmbedBuilder()
                .WithColor(color)
                .WithTitle($"**{GlobalFunctions.UppercaseFirst(pack)} Card - {GlobalFunctions.UppercaseFirst(category)} category (0/{maxAmount})**")
                .WithDescription($"There are no {pack} - {category} cards that you have captured yet.");
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

            string doremiText=""; string hazukiText = ""; string aikoText = ""; string onpuText = ""; string momokoText = "";
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

            if (doremiText == "") doremiText = "No one has complete their Doremi card pack yet.";
            if (hazukiText == "") hazukiText = "No one has complete their Hazuki card pack yet.";
            if (aikoText == "") aikoText = "No one has complete their Aiko card pack yet.";
            if (onpuText == "") onpuText = "No one has complete their Onpu card pack yet.";
            if (momokoText == "") momokoText = "No one has complete their Momoko card pack yet.";
            
            return new EmbedBuilder()
                .WithTitle($"\uD83C\uDFC6 Top 5 Trading Card Leaderboard")
                .WithColor(color)
                .AddField("Doremi Card Pack", doremiText)
                .AddField("Hazuki Card Pack", hazukiText)
                .AddField("Aiko Card Pack", aikoText)
                .AddField("Onpu Card Pack", onpuText)
                .AddField("Momoko Card Pack", momokoText);
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

            int totalSuccess = ((JArray)arrListDoremi["normal"]).Count + ((JArray)arrListDoremi["platinum"]).Count + ((JArray)arrListDoremi["metal"]).Count +
                ((JArray)arrListHazuki["normal"]).Count + ((JArray)arrListHazuki["platinum"]).Count + ((JArray)arrListHazuki["metal"]).Count +
                ((JArray)arrListAiko["normal"]).Count + ((JArray)arrListAiko["platinum"]).Count + ((JArray)arrListAiko["metal"]).Count +
                ((JArray)arrListOnpu["normal"]).Count + ((JArray)arrListOnpu["platinum"]).Count + ((JArray)arrListOnpu["metal"]).Count +
                ((JArray)arrListMomoko["normal"]).Count + ((JArray)arrListMomoko["platinum"]).Count + ((JArray)arrListMomoko["metal"]).Count;

            string doremiText = $"**Normal: {((JArray)arrListDoremi["normal"]).Count}/{Doremi.maxNormal}**\n" +
                $"**Platinum: {((JArray)arrListDoremi["platinum"]).Count}/{Doremi.maxPlatinum}**\n" +
                $"**Metal: {((JArray)arrListDoremi["metal"]).Count}/{Doremi.maxMetal}**";

            string hazukiText = $"**Normal: {((JArray)arrListHazuki["normal"]).Count}/{Hazuki.maxNormal}**\n" +
                $"**Platinum: {((JArray)arrListHazuki["platinum"]).Count}/{Hazuki.maxPlatinum}**\n" +
                $"**Metal: {((JArray)arrListHazuki["metal"]).Count}/{Hazuki.maxMetal}**";

            string aikoText = $"**Normal: {((JArray)arrListAiko["normal"]).Count}/{Aiko.maxNormal}**\n" +
                $"**Platinum: {((JArray)arrListAiko["platinum"]).Count}/{Aiko.maxPlatinum}**\n" +
                $"**Metal: {((JArray)arrListAiko["metal"]).Count}/{Aiko.maxMetal}**";

            string onpuText = $"**Normal: {((JArray)arrListOnpu["normal"]).Count}/{Onpu.maxNormal}**\n" +
                $"**Platinum: {((JArray)arrListOnpu["platinum"]).Count}/{Onpu.maxPlatinum}**\n" +
                $"**Metal: {((JArray)arrListOnpu["metal"]).Count}/{Onpu.maxMetal}**";

            string momokoText = $"**Normal: {((JArray)arrListMomoko["normal"]).Count}/{Momoko.maxNormal}**\n" +
                $"**Platinum: {((JArray)arrListMomoko["platinum"]).Count}/{Momoko.maxPlatinum}**\n" +
                $"**Metal: {((JArray)arrListMomoko["metal"]).Count}/{Momoko.maxMetal}**";

            return new EmbedBuilder()
                .WithTitle($"{username} Card Status Report")
                .WithColor(color)
                .AddField("Catch Attempt", $"**{totalSuccess} / {playerData["catch_attempt"].ToString()}**", false)
                .AddField("Doremi Card Pack", doremiText, true)
                .AddField("Hazuki Card Pack", hazukiText, true)
                .AddField("Aiko Card Pack", aikoText, true)
                .AddField("Onpu Card Pack", onpuText, true)
                .AddField("Momoko Card Pack", momokoText, true);

        }

        public class Doremi {
            public static int maxNormal = 48; public static int maxPlatinum = 8; public static int maxMetal = 6;
            public static string parent = "doremi";
            public static string emojiOk = "https://cdn.discordapp.com/attachments/706490547191152690/706511135788105728/143751262x.png";
            public static string emojiError = "https://cdn.discordapp.com/attachments/706490547191152690/706494009991757864/doremi.png";
            public static string emojiCompleteAllCard = "https://cdn.discordapp.com/attachments/706490547191152690/707424151723311154/win1.jpg";
        }

        public class Hazuki {
            public static int maxNormal = 46; public static int maxPlatinum = 9; public static int maxMetal = 6;
            public static string emojiError = "https://cdn.discordapp.com/attachments/706490547191152690/706494023782629386/hazuki.png";
            public static string emojiCompleteAllCard = "https://cdn.discordapp.com/attachments/706490547191152690/707424248872042568/win1.jpg";
        }

        public class Aiko{
            public static int maxNormal = 45; public static int maxPlatinum = 7; public static int maxMetal = 6;
            public static string emojiError = "https://cdn.discordapp.com/attachments/706490547191152690/706494032976674856/aiko.jpg";
            public static string emojiCompleteAllCard = "https://cdn.discordapp.com/attachments/706490547191152690/707424297685090344/win1.jpg";
        }

        public class Onpu
        {
            public static int maxNormal = 46; public static int maxPlatinum = 13; public static int maxMetal = 6;
            public static string emojiError = "https://cdn.discordapp.com/attachments/706490547191152690/706494042631962666/onpu.jpg";
            public static string emojiCompleteAllCard = "https://cdn.discordapp.com/attachments/706490547191152690/707424375380508682/win2.jpg";
        }

        public class Momoko
        {
            public static int maxNormal = 43; public static int maxPlatinum = 6; public static int maxMetal = 4;
            public static string emojiError = "https://cdn.discordapp.com/attachments/706490547191152690/706769235019300945/Linesticker21.png";
            public static string emojiCompleteAllCard = "https://cdn.discordapp.com/attachments/706490547191152690/707424504120344576/win5.jpg";
        }

    }
}
