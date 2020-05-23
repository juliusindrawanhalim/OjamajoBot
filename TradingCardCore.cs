using Discord;
using Discord.Addons.Interactive;
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
        public static string version = "1.08";
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

        public static string imgMagicSeeds = "https://cdn.discordapp.com/attachments/706770454697738300/709013040518922260/magic_seeds.jpg";

        public static EmbedBuilder printUpdatesNote()
        {
            return new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithTitle($"Doremi Trading Card - Update {version} - 21.05.20")
                .WithDescription($"-:new: **Rank system:** For every 100 EXP, your rank increased by 1 and your catching rate increased by 10%. " +
                $"Maximum rank are available up to 5.\n" +
                $"-:wrench: Bug fix & updates on **card inventory** where paging cannot be used\n" +
                $"-:new: Updates on **card status report**\n" +
                $"-:new: Paging updates on **card trade**");
        }

        public static int getPlayerRank(int exp)
        {
            int rank = 1;
            if (exp >= 100){
                rank = (int)Math.Ceiling(Convert.ToDouble(exp)/100)+1;
            }
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
            string title = $"**Total: {arrList.Count}/{maxAmount}**\n";

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

        public static EmbedBuilder printCardDetailTemplate(Color color, string guildId, string clientId, 
            string card_id, string parent, string emojiError, string errorDescription)
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
                    catch (Exception e) { }
                }
                else if (category == "special")
                {
                    try
                    {
                        name = jObjTradingCardList["other"][category][card_id]["name"].ToString();
                        parent = "other";
                    }
                    catch (Exception e) { }
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
                    .AddField("⭐", star, true)
                    .AddField("Point", point, true)
                    .WithImageUrl(imgUrl);
                }
                else
                {
                    return new EmbedBuilder()
                    .WithColor(color)
                    .WithDescription($":x: Sorry, you don't have: **{card_id} - {name}** card yet. Try to capture it to look at this card.")
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

        public static EmbedBuilder userCompleteTheirList(Color color, string avatarEmbed, string parent, string congratulateText, string imgUrl, string guildId, string clientId)
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
            .WithFooter($"Completed at: {dateTimeNow}", avatarEmbed)
            .WithDescription(congratulateText);
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
                    .AddField("⭐", star, true)
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
            var playerData = JObject.Parse(File.ReadAllText(playerDataDirectory));

            if (!File.Exists(playerDataDirectory))
            { //not registered yet
                return new EmbedBuilder()
                .WithColor(color)
                .WithDescription($"I'm sorry, please register yourself first with **{Config.Doremi.PrefixParent[0]}card register** command.")
                .WithThumbnailUrl(emojiError);
            }
            else
            {
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
                    .WithTitle($"📇 {username} Card Status | Rank: {getPlayerRank(playerExp)}")
                    .WithColor(color)
                    .WithThumbnailUrl(thumbnailUrl)
                    .AddField("Total / EXP", $"**{totalSuccess} / {playerData["catch_attempt"].ToString()}**", false)
                    .AddField("Doremi Card Pack", doremiText, true)
                    .AddField("Hazuki Card Pack", hazukiText, true)
                    .AddField("Aiko Card Pack", aikoText, true)
                    .AddField("Onpu Card Pack", onpuText, true)
                    .AddField("Momoko Card Pack", momokoText, true)
                    .AddField("Other Card Pack", otherText, true);
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
                                        replyText = $":x: You guess the mystery card correctly but I can't capture **{spawnedCardId} - {name}** because you have it already.";
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

                                    if (catchState == 1)
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
            string category;
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
            else
                category = "doremi";
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

            public static string[] arrMysteryDescription = {
                ":birthday: July is my birthday",
                "Dodo is my fairy",
                ":birthday: February, May, March and November are not my birthday",
                ":birthday: My birthday was at 30th",
                ":sparkles: **Pirika** is one of my chanting spell",
                ":sparkles: **Pirilala** is one of my chanting spell",
                ":sparkles: **Poporina** is one of my chanting spell",
                ":sparkles: **Peperuto** is one of my chanting spell",
                ":sparkles: **Paipai Raruku Famifami Pon!** are not my spell",
                ":sparkles: **Puwapuwa Petton Pururun Rarirori!** are not my spell",
                ":sparkles: **Puu Raruku Purun Perutan!** are not my spell",
                ":sparkles: **Puu Poppun Faa Pon!** are not my spell",
                ":sparkles: **Ponpoi Pameruku Pururun Petton!** are not my spell",
                ":sparkles: **Famifami Rarirori Paipai Petton!** are not my spell"
            };

        }

        public class Hazuki {
            public static int maxNormal = 46; public static int maxPlatinum = 9; public static int maxMetal = 6;
            public static int maxOjamajos = 5;
            public static string emojiError = "https://cdn.discordapp.com/attachments/706490547191152690/706494023782629386/hazuki.png";
            public static string emojiCompleteAllCard = "https://cdn.discordapp.com/attachments/706490547191152690/707424248872042568/win1.jpg";
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
                ":sparkles: **Pirika Raruku Famifami Pon!** are not my spell",
                ":sparkles: **Purun Pirilala Pararira Rarirori!** are not my spell",
                ":sparkles: **Peperuto Poppun Faa Pon!** are not my spell",
                ":sparkles: **Peperuto Purun Rarirori Perutan!** are not my spell"
            };

        }

        public class Aiko{
            public static int maxNormal = 45; public static int maxPlatinum = 7; public static int maxMetal = 6;
            public static int maxOjamajos = 5;
            public static string emojiError = "https://cdn.discordapp.com/attachments/706490547191152690/706494032976674856/aiko.jpg";
            public static string emojiCompleteAllCard = "https://cdn.discordapp.com/attachments/706490547191152690/707424297685090344/win1.jpg";
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
                ":sparkles: **Pirika Ponpoi Famifami Pon!** are not my spell",
                ":sparkles: **Peperuto Puwapuwa Purun Perutan!** are not my spell",
                ":sparkles: **Ponpoi Purun Pirilala Petton!** are not my spell",
                ":sparkles: **Poporina Puwapuwa Famifami Pararira!** are not my spell",
                ":sparkles: **Paipai Pururun Pirika Perutan!** are not my spell",
                ":sparkles: **Puu Faa Peperuto Pon!** are not my spell"
            };

        }

        public class Onpu
        {
            public static int maxNormal = 46; public static int maxPlatinum = 13; public static int maxMetal = 6;
            public static int maxOjamajos = 6;
            public static string emojiError = "https://cdn.discordapp.com/attachments/706490547191152690/706494042631962666/onpu.jpg";
            public static string emojiCompleteAllCard = "https://cdn.discordapp.com/attachments/706490547191152690/707424375380508682/win2.jpg";
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
                ":sparkles: **Rarirori Ponpoi Pon Pirika!** are not my spell",
                ":sparkles: **Peperuto Puwapuwa Raruku Perutan!** are not my spell",
                ":sparkles: **Pirilala Ponpoi Raruku Petton!** are not my spell",
                ":sparkles: **Poporina Puwapuwa Rarirori Pararira!** are not my spell",
                ":sparkles: **Peperuto Puu Poppun Pon!** are not my spell",
                ":sparkles: **Paipai Pirika Pameruku Perutan!** are not my spell"
            };

        }

        public class Momoko
        {
            public static int maxNormal = 43; public static int maxPlatinum = 6; public static int maxMetal = 4;
            public static int maxOjamajos = 5;
            public static string emojiError = "https://cdn.discordapp.com/attachments/706490547191152690/706769235019300945/Linesticker21.png";
            public static string emojiCompleteAllCard = "https://cdn.discordapp.com/attachments/706490547191152690/707424504120344576/win5.jpg";

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
                ":sparkles: **Ponpoi Rarirori Pirika Faa!** are not my spell",
                ":sparkles: **Raruku Puwapuwa Peperuto Pururun!** are not my spell",
                ":sparkles: **Purun Ponpoi Raruku  Pirilala!** are not my spell",
                ":sparkles: **Rarirori Poporina Famifami Puwapuwa!** are not my spell",
                ":sparkles: **Faa Puu Poppun Peperuto!** are not my spell",
                ":sparkles: **Pururun Pirika Pameruku Paipai!** are not my spell"
            };

        }

    }
}
