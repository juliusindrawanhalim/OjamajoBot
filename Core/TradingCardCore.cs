using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Lavalink4NET.Statistics;
using Newtonsoft.Json.Linq;
using OjamajoBot.Database;
using OjamajoBot.Database.Model;
using Spectacles.NET.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace OjamajoBot
{
    public enum PACK
    {
        DOREMI,
        HAZUKI,
        AIKO,
        ONPU,
        MOMOKO,
        POP,
        HANA
    }

    public class TradingCardCore
    {
        public static string version = "1.30";
        public static string propertyId = "trading_card_spawn_id";
        public static string propertyCategory = "trading_card_spawn_category";
        public static string propertyToken = "trading_card_spawn_token";
        public static string propertyMystery = "trading_card_spawn_mystery";
        public static string propertyTokenTime = "trading_card_spawn_token_time";
        public static string propertyIsZone = "trading_card_spawn_is_zone";

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
        public static string roleCardCatcher = "Card Catcher";
        public static Color roleCompletionistColor = new Discord.Color(4, 173, 18);
        public static string imgCompleteAllCardSpecial = $"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/badge/badge_special.png";

        public static EmbedBuilder printUpdatesNote()
        {
            //1.30
            return new EmbedBuilder()
            .WithColor(Config.Doremi.EmbedColor)
            .WithTitle($"Ojamajo Trading Card - Update {version} - 25.11.20")
            .WithDescription("Major data migration & changes " +
            "for most all trading card & garden related command are being made for this version. " +
            "As a bonus compensation, all user will receive free 100 magic seeds. " +
            $"Please report if you notice any of the related command that is not working properly.")
            .AddField("**Updates**:",
            $"-**register** command has been removed, user can now immediately capture card\n" +
            $"-**spawn,save,delete** command has been removed\n" +
            $"-**trade** command has been removed and will be change upon upcoming updates\n" +
            $"-**card guide** has been updated\n" +
            $"-Purchasing the **card boost** now doesn't reset the boost that you have from the listed boost effect\n" +
            $"-Using the **card boost** command now doesn't reset all card boost for that pack\n" +
            $"-The amount of **magic seeds & royal seeds** that can be store now have limited cap.\n" +
            $"-Magic seeds are limited up to 3000 and royal seeds are limited up to 10.\n" +
            $"-**leaderboard** command has been updated and can be called with each ojamajo bot for each pack")
            .AddField("**New command**:",
            "-**status complete <username>**: show the completion date status. You can put the <username> parameter with other user.\n"+
            "-**card verify**: let you verify the card completion status.");
        }

        public static int getUserRank(int exp)
        {
            //0 = 1
            //100 = 2
            //200 = 3
            //300 = 4
            int rank = 1;
            if (exp >= 100) rank = (int)Math.Ceiling(Convert.ToDouble(exp) / 100);

            if (exp == 100) rank = 2;
            if (rank >= 5) rank = 5;
            return rank;
        }

        public static bool checkUserHaveCard(ulong userId, string cardPack, string cardId)
        {
            //for non ojamajos category
            Dictionary<string, object> columns = new Dictionary<string, object>();
            bool ret = false;
            DBC db = new DBC();
            string query = "";

            string category = getCardCategory(cardId);
            if (category == "ojamajos")
            {
                query = $"SELECT {DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_card} " +
                $" FROM {DBM_User_Trading_Card_Inventory_Ojamajos.tableName} " +
                $" WHERE {DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_user}=@{DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_user} AND " +
                $" {DBM_User_Trading_Card_Inventory_Ojamajos.Columns.pack} like @{DBM_User_Trading_Card_Inventory_Ojamajos.Columns.pack} AND " +
                $" {DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_card}=@{DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_card} ";
                columns[DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_user] = userId.ToString();
                columns[DBM_User_Trading_Card_Inventory_Ojamajos.Columns.pack] = $"%{cardPack}%";
                columns[DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_card] = cardId;
            } else
            {
                query = $"SELECT {DBM_User_Trading_Card_Inventory.Columns.id_card} " +
                $" FROM {DBM_User_Trading_Card_Inventory.tableName} " +
                $" WHERE {DBM_User_Trading_Card_Inventory.Columns.id_user}=@{DBM_User_Trading_Card_Inventory.Columns.id_user} AND  " +
                $" {DBM_User_Trading_Card_Inventory.Columns.id_card}=@{DBM_User_Trading_Card_Inventory.Columns.id_card} ";
                columns[DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_user] = userId.ToString();
                columns[DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_card] = cardId;
            }
            Console.WriteLine($"ID CARD:{cardId}");

            var results = db.selectAll(query,columns);
            if (results.Rows.Count >= 1)
                ret = true;
            
            return ret;
        }

        public static void insertUserData(ulong userId)
        {
            DBC db = new DBC();

            Dictionary<string, object> columns = new Dictionary<string, object>();
            columns[DBM_User_Trading_Card_Data.Columns.id_user] = userId.ToString();
            db.insert(DBM_User_Trading_Card_Data.tableName, columns);
        }


        public static PaginatedMessage printInventory(SocketCommandContext context, Color color, string pack, string category,
            int maxAmount, SocketGuildUser otherUser = null)
        {
            ulong userId = context.User.Id;
            string username = context.User.Username;
            string thumbnailUrl = context.User.GetAvatarUrl();

            if (otherUser != null)
            {
                userId = otherUser.Id;
                username = otherUser.Username;
                thumbnailUrl = otherUser.GetAvatarUrl();
            }

            PaginatedAppearanceOptions pao = new PaginatedAppearanceOptions();
            pao.JumpDisplayOptions = JumpDisplayOptions.Never;
            pao.DisplayInformationIcon = false;

            DBC db = new DBC();
            string query = @$"select tc.id_card,tc.name,tc.pack,tc.url,tc.attr_0,tc.attr_1,tc.attr_2,inv.id_user as owned 
		    from {DBM_Trading_Card_Data.tableName} tc 
            left join {DBM_User_Trading_Card_Inventory.tableName} inv 
            on inv.{DBM_User_Trading_Card_Inventory.Columns.id_user}=@{DBM_User_Trading_Card_Inventory.Columns.id_user} and 
            inv.{DBM_User_Trading_Card_Inventory.Columns.id_card} = tc.{DBM_Trading_Card_Data.Columns.id_card} 
            where tc.{DBM_Trading_Card_Data.Columns.pack}=@{DBM_Trading_Card_Data.Columns.pack} and 
            tc.{DBM_Trading_Card_Data.Columns.category}=@{DBM_Trading_Card_Data.Columns.category} 
            order by tc.{DBM_Trading_Card_Data.Columns.id_card} asc";

            Dictionary<string, object> colFilter = new Dictionary<string, object>();
            colFilter[DBM_Trading_Card_Data.Columns.pack] = pack;
            colFilter[DBM_User_Trading_Card_Inventory.Columns.id_user] = userId;
            
            if (category == "ojamajos")
            {
                query = @$"select tc.id_card,tc.name,tc.pack,tc.url,tc.attr_0,tc.attr_1,tc.attr_2,inv.id_user as owned 
		        from {DBM_Trading_Card_Data_Ojamajos.tableName} tc
                left join {DBM_User_Trading_Card_Inventory_Ojamajos.tableName} inv 
                on inv.{DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_user}=@{DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_user} and 
                inv.{DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_card} = tc.{DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_card} 
                where tc.{DBM_Trading_Card_Data_Ojamajos.Columns.pack} like @{DBM_Trading_Card_Data_Ojamajos.Columns.pack} 
                order by tc.{DBM_Trading_Card_Data_Ojamajos.Columns.id_card} asc";
                colFilter[DBM_Trading_Card_Data.Columns.pack] = $"%{pack}%";
            } else
            {
                colFilter[DBM_Trading_Card_Data.Columns.category] = category;
            }

            DataTable dt = db.selectAll(query, colFilter);

            List<string> pageContent = new List<string>();

            int totalOwned = 0; 
            foreach (DataRow row in dt.Rows) //calculate owned
            {
                if (row["owned"].ToString() != "")
                    totalOwned++;
            }

            double calculated = (double)totalOwned / maxAmount * 100;
            string percentageCompleted = $"({Math.Round(calculated)}%)";

            string title = $"**Progress: {totalOwned}/{maxAmount} {percentageCompleted}**\n";

            string tempVal = title;

            var i = 0;
            int currentIndex = 0;

            foreach (DataRow row in dt.Rows)
            {
                if (row["owned"].ToString() != "")
                    tempVal += ":white_check_mark: ";
                 else
                    tempVal += ":x: ";
                
                string cardId = row["id_card"].ToString();
                string name = row["name"].ToString();
                string url = row["url"].ToString();

                tempVal += $"[{cardId} - {name}]({url})\n";

                if (i == dt.Rows.Count - 1)
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
                i++;
            }

            var pager = new PaginatedMessage
            {
                Title = $"**{GlobalFunctions.UppercaseFirst(pack)} {GlobalFunctions.UppercaseFirst(category)} Card Inventory**\n",
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

        public static EmbedBuilder printCardBoostStatus(SocketCommandContext context, Color color)
        {
            string username = context.User.Username;
            ulong userId = context.User.Id;

            var userData = UserTradingCardDataCore.getUserData(userId);

            int boostDoremiNormal = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_doremi_normal])*10;
            int boostDoremiPlatinum = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum]) * 10;
            int boostDoremiMetal = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_doremi_metal]) * 10; 
            int boostDoremiOjamajos = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos]) * 10;

            int boostHazukiNormal = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal]) * 10;
            int boostHazukiPlatinum = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum]) * 10;
            int boostHazukiMetal = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal]) * 10;
            int boostHazukiOjamajos = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos]) * 10;

            int boostAikoNormal = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_aiko_normal]) * 10;
            int boostAikoPlatinum = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum]) * 10;
            int boostAikoMetal = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_aiko_metal]) * 10;
            int boostAikoOjamajos = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos]) * 10;

            int boostOnpuNormal = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_onpu_normal]) * 10;
            int boostOnpuPlatinum = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum]) * 10;
            int boostOnpuMetal = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_onpu_metal]) * 10;
            int boostOnpuOjamajos = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos]) * 10;

            int boostMomokoNormal = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_momoko_normal]) * 10;
            int boostMomokoPlatinum = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_momoko_platinum]) * 10;
            int boostMomokoMetal = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_momoko_metal]) * 10;
            int boostMomokoOjamajos = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_momoko_ojamajos]) * 10;

            int boostOtherSpecial = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.boost_other_special]) * 10;

            string doremiBoost = $"Normal: {boostDoremiNormal}%\n" +
                $"Platinum: {boostDoremiPlatinum}%\n" +
                $"Metal: {boostDoremiMetal}%\n" +
                $"Ojamajos: {boostDoremiOjamajos}%".Replace(" 0%","-");

            string hazukiBoost = $"Normal: {boostHazukiNormal}%\n" +
                $"Platinum: {boostHazukiPlatinum}%\n" +
                $"Metal: {boostHazukiMetal}%\n" +
                $"Ojamajos: {boostHazukiOjamajos}%".Replace(" 0%", "-");

            string aikoBoost = $"Normal: {boostAikoNormal}%\n" +
                $"Platinum: {boostAikoPlatinum}%\n" +
                $"Metal: {boostAikoMetal}%\n" +
                $"Ojamajos: {boostAikoOjamajos}%".Replace(" 0%", "-");

            string onpuBoost = $"Normal: {boostOnpuNormal}%\n" +
                $"Platinum: {boostOnpuPlatinum}%\n" +
                $"Metal: {boostOnpuMetal}%\n" +
                $"Ojamajos: {boostOnpuOjamajos}%".Replace(" 0%", "-");

            string momokoBoost = $"Normal: {boostMomokoNormal}%\n" +
                $"Platinum: {boostMomokoPlatinum}%\n" +
                $"Metal: {boostMomokoMetal}%\n" +
                $"Ojamajos: {boostMomokoOjamajos}%".Replace(" 0%", "-");

            string otherBoost = $"**Special: {boostOtherSpecial * 10}%**".Replace(" 0%", "-");

            return new EmbedBuilder()
                .WithColor(color)
                .WithTitle($":arrow_double_up: **{username} Card Status Boost**")
                .AddField("Doremi Boost", doremiBoost, true)
                .AddField("Hazuki Boost", hazukiBoost, true)
                .AddField("Aiko Boost", aikoBoost, true)
                .AddField("Onpu Boost", onpuBoost, true)
                .AddField("Momoko Boost", momokoBoost, true)
                .AddField("Other Boost", otherBoost, true);
        }


        public static EmbedBuilder printEmptyInventoryTemplate(Color color, string pack, string category, int maxAmount, string username)
        {
            return new EmbedBuilder()
                .WithColor(color)
                .WithTitle($"**{username}'s {GlobalFunctions.UppercaseFirst(pack)} {GlobalFunctions.UppercaseFirst(category)} Card (0/{maxAmount})**")
                .WithDescription($":x: There are no {pack} - {category} cards that you have captured yet.");
        }

        public static EmbedBuilder printCardDetailTemplate(SocketCommandContext context, Color color, 
            string card_id, string emojiError)
        {
            var userId = context.User.Id;
            var username = context.User.Username;

            if (card_id == "")
            {
                return new EmbedBuilder()
                .WithColor(color)
                .WithDescription($":x: Please enter the card id.")
                .WithThumbnailUrl(emojiError);
            } else
            {
                //check if card exists on db
                DBC db = new DBC();
                string query = @$"select id_card,name,pack,category,url,attr_0,attr_1,attr_2 
                from {DBM_Trading_Card_Data.tableName} 
                where id_card = '{card_id}' 
                union  
                select id_card,name,pack,'ojamajos' as category,url,attr_0,attr_1,attr_2 
                from {DBM_Trading_Card_Data_Ojamajos.tableName}  
                where id_card = '{card_id}'";

                Dictionary<string, object> colFilter = new Dictionary<string, object>();
                colFilter[DBM_Trading_Card_Data.Columns.id_card] = card_id;

                DataTable dt = db.selectAll(query, colFilter);
                
                if (dt.Rows.Count <= 0)
                {
                    return new EmbedBuilder()
                    .WithColor(color)
                    .WithDescription($":x: Sorry, I can't find that card ID.")
                    .WithThumbnailUrl(emojiError);

                }
                else
                {
                    string imgUrl = ""; string rank = ""; string name = "";
                    string star = ""; string point = ""; string category = "";

                    foreach (DataRow row in dt.Rows)
                    {
                        name = row["name"].ToString(); category = row["category"].ToString();
                        imgUrl = row["url"].ToString(); rank = row["attr_0"].ToString();
                        star = row["attr_1"].ToString(); point = row["attr_2"].ToString();

                        switch (row["category"].ToString())
                        {
                            case "ojamajos":
                                query = @$"select {DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_card} 
                                from {DBM_User_Trading_Card_Inventory_Ojamajos.tableName} 
                                where {DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_user}=@{DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_user} and 
                                {DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_card}=@{DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_card}";
                                break;
                            default:
                                query = @$"select {DBM_User_Trading_Card_Inventory.Columns.id_card} 
                                from {DBM_User_Trading_Card_Inventory.tableName} 
                                where {DBM_User_Trading_Card_Inventory.Columns.id_user}=@{DBM_User_Trading_Card_Inventory.Columns.id_user} and 
                                {DBM_User_Trading_Card_Inventory.Columns.id_card}=@{DBM_User_Trading_Card_Inventory.Columns.id_card}";
                                break;
                        }
                    }

                    Dictionary<string, object> colFilterUserHave = new Dictionary<string, object>();
                    colFilterUserHave[DBM_User_Trading_Card_Inventory.Columns.id_user] = userId;
                    colFilterUserHave[DBM_Trading_Card_Data.Columns.id_card] = card_id;

                    //check if user have card
                    DataTable dtUserHave = db.selectAll(query, colFilterUserHave);
                    foreach (DataRow row in dtUserHave.Rows)
                    {
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

                    return new EmbedBuilder()
                        .WithDescription($":x: Sorry **{username}**, you don't have this card: **{card_id} - {name}**. " +
                        $"Capture it first to see this card.")
                        .WithThumbnailUrl(emojiError);

                    //if (dtUserHave.Rows.Count <= 0)
                    //{
                        
                    //} else
                    //{
                        
                    //}
                }
            }
        }

        public static EmbedBuilder printLeaderboardTemplate(SocketCommandContext context, Color color, 
            string cardPack)
        {
            var guildId = context.Guild.Id;

            DBC db = new DBC();
            Dictionary<string, object> columnFilter = new Dictionary<string, object>();

            //base cardPack
            string query = "SELECT * " +
                $" FROM {DBM_Trading_Card_Leaderboard.tableName} " +
                $" WHERE {DBM_Trading_Card_Leaderboard.Columns.id_guild}=@{DBM_Trading_Card_Leaderboard.Columns.id_guild} AND " +
                $" {DBM_Trading_Card_Leaderboard.Columns.card_pack}=@{DBM_Trading_Card_Leaderboard.Columns.card_pack} " +
                $" ORDER BY {DBM_Trading_Card_Leaderboard.Columns.complete_date} asc " +
                $" LIMIT 5";
            
            columnFilter[DBM_Trading_Card_Leaderboard.Columns.id_guild] = guildId.ToString();
            columnFilter[DBM_Trading_Card_Leaderboard.Columns.card_pack] = cardPack;
            var result = db.selectAll(query, columnFilter);

            string contentText = ""; string otherText = ""; string popText = ""; string hanaText = "";

            var ctr = 1;
            foreach (DataRow rows in result.Rows)
            {
                ulong userId = Convert.ToUInt64(rows[DBM_Trading_Card_Leaderboard.Columns.id_user].ToString());
                string completionDate = DateTime.Parse(rows[DBM_Trading_Card_Leaderboard.Columns.complete_date].ToString())
                    .ToString("MM/dd/yyyy");

                contentText += $"{ctr}. {MentionUtils.MentionUser(userId)} : {completionDate}\n";
                ctr++;
            }

            //other
            columnFilter = new Dictionary<string, object>();
            columnFilter[DBM_Trading_Card_Leaderboard.Columns.id_guild] = guildId.ToString();
            columnFilter[DBM_Trading_Card_Leaderboard.Columns.card_pack] = "other";
            result = db.selectAll(query, columnFilter);

            ctr = 1;
            foreach (DataRow rows in result.Rows)
            {
                ulong userId = Convert.ToUInt64(rows[DBM_Trading_Card_Leaderboard.Columns.id_user].ToString());
                string completionDate = DateTime.Parse(rows[DBM_Trading_Card_Leaderboard.Columns.complete_date].ToString())
                    .ToString("MM/dd/yyyy");

                otherText += $"{ctr}. {MentionUtils.MentionUser(userId)} : {completionDate}\n";
                ctr++;
            }

            //pop
            columnFilter = new Dictionary<string, object>();
            columnFilter[DBM_Trading_Card_Leaderboard.Columns.id_guild] = guildId.ToString();
            columnFilter[DBM_Trading_Card_Leaderboard.Columns.card_pack] = "pop";
            result = db.selectAll(query, columnFilter);

            ctr = 1;
            foreach (DataRow rows in result.Rows)
            {
                ulong userId = Convert.ToUInt64(rows[DBM_Trading_Card_Leaderboard.Columns.id_user].ToString());
                string completionDate = DateTime.Parse(rows[DBM_Trading_Card_Leaderboard.Columns.complete_date].ToString())
                    .ToString("MM/dd/yyyy");

                popText += $"{ctr}. {MentionUtils.MentionUser(userId)} : {completionDate}\n";
                ctr++;
            }

            //hana
            columnFilter = new Dictionary<string, object>();
            columnFilter[DBM_Trading_Card_Leaderboard.Columns.id_guild] = guildId.ToString();
            columnFilter[DBM_Trading_Card_Leaderboard.Columns.card_pack] = "hana";
            result = db.selectAll(query, columnFilter);

            ctr = 1;
            foreach (DataRow rows in result.Rows)
            {
                ulong userId = Convert.ToUInt64(rows[DBM_Trading_Card_Leaderboard.Columns.id_user].ToString());
                string completionDate = DateTime.Parse(rows[DBM_Trading_Card_Leaderboard.Columns.complete_date].ToString())
                    .ToString("MM/dd/yyyy");

                hanaText += $"{ctr}. {MentionUtils.MentionUser(userId)} : {completionDate}\n";
                ctr++;
            }

            if (contentText == "") contentText = $"No one has complete **{cardPack}** card pack yet.";
            if (otherText == "") contentText = $"No one has complete **Other** card pack yet.";
            if (popText == "") contentText = $"No one has complete **Pop** card pack yet.";
            if (hanaText == "") contentText = $"No one has complete **Hana** card pack yet.";

            return new EmbedBuilder()
                .WithTitle($"\uD83C\uDFC6 Top 5 Trading Card Leaderboard")
                .WithColor(color)
                .AddField($"{cardPack} Card Pack", contentText)
                .AddField("Other Card Pack", otherText)
                .AddField("Pop Card Pack", popText)
                .AddField("Hana Card Pack", hanaText);
        }

        public static EmbedBuilder userCompleteTheirList(SocketCommandContext context, Color color, 
            string avatarEmbed, string cardPack, string imgUrl, string unlockText)
        {
            DBC db = new DBC();
            var guildId = context.Guild.Id;
            var userId = context.User.Id;
            string userAvatarUrl = context.User.GetAvatarUrl();
            string username = context.User.Username;

            string completedAt = DateTime.Now.ToString("MM/dd/yyyy");

            //check if leaderboard exists/not
            string query = @$"SELECT * 
            FROM {DBM_Trading_Card_Leaderboard.tableName} 
            WHERE {DBM_Trading_Card_Leaderboard.Columns.id_guild}=@{DBM_Trading_Card_Leaderboard.Columns.id_guild} AND 
            {DBM_Trading_Card_Leaderboard.Columns.id_user}=@{DBM_Trading_Card_Leaderboard.Columns.id_user} AND 
            {DBM_Trading_Card_Leaderboard.Columns.card_pack}=@{DBM_Trading_Card_Leaderboard.Columns.card_pack}";
            Dictionary<string, object> columnFilter = new Dictionary<string, object>();
            columnFilter[DBM_Trading_Card_Leaderboard.Columns.id_guild] = guildId;
            columnFilter[DBM_Trading_Card_Leaderboard.Columns.id_user] = userId;
            columnFilter[DBM_Trading_Card_Leaderboard.Columns.card_pack] = cardPack;
            var resultLeaderBoardExist = db.selectAll(query,columnFilter);

            if (resultLeaderBoardExist.Rows.Count<=0 &&
                UserTradingCardDataCore.checkCardCompletion(userId, cardPack))
            {
                //return congratulate embed
                Dictionary<string, object> columnInsert = new Dictionary<string, object>();
                columnInsert[DBM_Trading_Card_Leaderboard.Columns.id_guild] = guildId;
                columnInsert[DBM_Trading_Card_Leaderboard.Columns.id_user] = userId;
                columnInsert[DBM_Trading_Card_Leaderboard.Columns.card_pack] = cardPack;

                db.insert(DBM_Trading_Card_Leaderboard.tableName, columnInsert);

                EmbedBuilder eb = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder
                {
                    Name = username,
                    IconUrl = userAvatarUrl
                })
                .WithTitle($"{GlobalFunctions.UppercaseFirst(cardPack)} Card Pack Completed!")
                .WithDescription($"Congratulations, **{username}** have completed all **{GlobalFunctions.UppercaseFirst(cardPack)} Card Pack**!")
                .WithColor(color)
                .WithImageUrl($"attachment://{Path.GetFileName(imgUrl)}")
                .AddField("Role & Badge Reward:", unlockText)
                .WithFooter($"Completed at: {completedAt}", avatarEmbed);
                return eb;
            } else
            {
                return null;
            }
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
                    .WithFooter($"Captured by: {username} ({totalCaptured}/{max})", botIconUrl);
        }

        public static void resetSpawnInstance(ulong guildId)
        {
            string query = $"UPDATE {DBM_Trading_Card_Guild.tableName} " +
                $" SET {DBM_Trading_Card_Guild.Columns.spawn_id}=@{DBM_Trading_Card_Guild.Columns.spawn_id}," +
                $" {DBM_Trading_Card_Guild.Columns.spawn_parent}=@{DBM_Trading_Card_Guild.Columns.spawn_parent}," +
                $" {DBM_Trading_Card_Guild.Columns.spawn_token}=@{DBM_Trading_Card_Guild.Columns.spawn_token}," +
                $" {DBM_Trading_Card_Guild.Columns.spawn_category}=@{DBM_Trading_Card_Guild.Columns.spawn_category}, " +
                $" {DBM_Trading_Card_Guild.Columns.spawn_is_mystery}=@{DBM_Trading_Card_Guild.Columns.spawn_is_mystery}, " +
                $" {DBM_Trading_Card_Guild.Columns.spawn_is_badcard}=@{DBM_Trading_Card_Guild.Columns.spawn_is_badcard} " +
                $" WHERE {DBM_Trading_Card_Guild.Columns.id_guild} = {DBM_Trading_Card_Guild.Columns.id_guild}";
            Dictionary<string, object> columnsFilter = new Dictionary<string, object>();
            columnsFilter[DBM_Trading_Card_Guild.Columns.spawn_id] = "";
            columnsFilter[DBM_Trading_Card_Guild.Columns.spawn_parent] = "";
            columnsFilter[DBM_Trading_Card_Guild.Columns.spawn_token] = "";
            columnsFilter[DBM_Trading_Card_Guild.Columns.spawn_category] = "";
            columnsFilter[DBM_Trading_Card_Guild.Columns.spawn_is_mystery] = 0;
            columnsFilter[DBM_Trading_Card_Guild.Columns.spawn_is_badcard] = 0;
            columnsFilter[DBM_Trading_Card_Guild.Columns.id_guild] = guildId.ToString();

            new DBC().update(query, columnsFilter);

            //Config.Guild.setPropertyValue(guildId, propertyId, "");
            //Config.Guild.setPropertyValue(guildId, propertyCategory, "");
            //Config.Guild.setPropertyValue(guildId, propertyToken, "");
            //Config.Guild.setPropertyValue(guildId, propertyMystery, "0");
        }

        public static EmbedBuilder printStatusTemplate(SocketCommandContext context, Color color, SocketGuildUser otherUser = null)
        {
            //Dictionary<string, object> dataUser = new Dictionary<string, object>();
            //Dictionary<string, object> dataInventory = new Dictionary<string, object>();

            var userId = context.User.Id;
            var username = context.User.Username;
            var thumbnailUrl = context.User.GetAvatarUrl();

            if (otherUser != null)
            {
                userId = otherUser.Id;
                username = otherUser.Username;
                thumbnailUrl = otherUser.GetAvatarUrl();
            }

            //get status
            DBC db = new DBC();

            //only for readd purpose
            UserTradingCardDataCore.getUserData(userId);

            string query = @$"select (select count(distinct(inv.id_card))
            from user_trading_card_inventory inv, trading_card_data tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack = 'doremi' and tc.category = 'normal') as doremi_normal,
            (select count(distinct(inv.id_card))
            from user_trading_card_inventory inv, trading_card_data tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack = 'doremi' and tc.category = 'platinum') as doremi_platinum,
            (select count(distinct(inv.id_card))
            from user_trading_card_inventory inv, trading_card_data tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack = 'doremi' and tc.category = 'metal') as doremi_metal,
            (select count(distinct(inv.id_card))
            from user_trading_card_inventory_ojamajos inv, trading_card_data_ojamajos tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack like '%doremi%') as doremi_ojamajos,


            (select count(distinct(inv.id_card))
            from user_trading_card_inventory inv, trading_card_data tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack = 'hazuki' and tc.category = 'normal') as hazuki_normal,
            (select count(distinct(inv.id_card))
            from user_trading_card_inventory inv, trading_card_data tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack = 'hazuki' and tc.category = 'platinum') as hazuki_platinum,
            (select count(distinct(inv.id_card))
            from user_trading_card_inventory inv, trading_card_data tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack = 'hazuki' and tc.category = 'metal') as hazuki_metal,
            (select count(distinct(inv.id_card))
            from user_trading_card_inventory_ojamajos inv, trading_card_data_ojamajos tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack like '%hazuki%') as hazuki_ojamajos,


            (select count(distinct(inv.id_card))
            from user_trading_card_inventory inv, trading_card_data tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack = 'aiko' and tc.category = 'normal') as aiko_normal,
            (select count(distinct(inv.id_card))
            from user_trading_card_inventory inv, trading_card_data tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack = 'aiko' and tc.category = 'platinum') as aiko_platinum,
            (select count(distinct(inv.id_card))
            from user_trading_card_inventory inv, trading_card_data tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack = 'aiko' and tc.category = 'metal') as aiko_metal,
            (select count(distinct(inv.id_card))
            from user_trading_card_inventory_ojamajos inv, trading_card_data_ojamajos tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack like '%aiko%') as aiko_ojamajos,


            (select count(distinct(inv.id_card))
            from user_trading_card_inventory inv, trading_card_data tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack = 'onpu' and tc.category = 'normal') as onpu_normal,
            (select count(distinct(inv.id_card))
            from user_trading_card_inventory inv, trading_card_data tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack = 'onpu' and tc.category = 'platinum') as onpu_platinum,
            (select count(distinct(inv.id_card))
            from user_trading_card_inventory inv, trading_card_data tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack = 'onpu' and tc.category = 'metal') as onpu_metal,
            (select count(distinct(inv.id_card))
            from user_trading_card_inventory_ojamajos inv, trading_card_data_ojamajos tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack like '%onpu%') as onpu_ojamajos,

            (select count(distinct(inv.id_card))
            from user_trading_card_inventory inv, trading_card_data tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack = 'momoko' and tc.category = 'normal') as momoko_normal,
            (select count(distinct(inv.id_card))
            from user_trading_card_inventory inv, trading_card_data tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack = 'momoko' and tc.category = 'platinum') as momoko_platinum,
            (select count(distinct(inv.id_card))
            from user_trading_card_inventory inv, trading_card_data tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack = 'momoko' and tc.category = 'metal') as momoko_metal,
            (select count(distinct(inv.id_card))
            from user_trading_card_inventory_ojamajos inv, trading_card_data_ojamajos tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack like '%momoko%') as momoko_ojamajos,

            (select count(distinct(inv.id_card))
            from user_trading_card_inventory inv, trading_card_data tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack = 'other' and tc.category = 'special') as other_special,

            (select count(distinct(inv.id_card))
            from user_trading_card_inventory inv, trading_card_data tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack = 'pop' and tc.category = 'normal') as pop_normal,

            (select count(distinct(inv.id_card))
            from user_trading_card_inventory inv, trading_card_data tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack = 'hana' and tc.category = 'normal') as hana_normal,
            (select count(distinct(inv.id_card))
            from user_trading_card_inventory inv, trading_card_data tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack = 'hana' and tc.category = 'platinum') as hana_platinum,
            (select count(distinct(inv.id_card))
            from user_trading_card_inventory inv, trading_card_data tc
            where inv.id_user = '{userId}' and inv.id_card = tc.id_card
            and tc.pack = 'hana' and tc.category = 'metal') as hana_metal";

            DataTable dt = db.selectAll(query);

            int playerExp = 0; string catchAttempt = "0";
            //end read database


            int doremiNormal = 0; int doremiMetal = 0; int doremiPlatinum = 0; int doremiOjamajos = 0;
            int hazukiNormal = 0; int hazukiMetal = 0; int hazukiPlatinum = 0; int hazukiOjamajos = 0;
            int aikoNormal = 0; int aikoMetal = 0; int aikoPlatinum = 0; int aikoOjamajos = 0;
            int onpuNormal = 0; int onpuMetal = 0; int onpuPlatinum = 0; int onpuOjamajos = 0;
            int momokoNormal = 0; int momokoMetal = 0; int momokoPlatinum = 0; int momokoOjamajos = 0;
            int otherSpecial = 0;
            int popNormal = 0; int hanaNormal = 0; int hanaPlatinum = 0; int hanaMetal = 0;

            Dictionary<string, object> userData = UserTradingCardDataCore.getUserData(userId);
            if (userData.Count >= 1)
            {
                catchAttempt = userData[DBM_User_Trading_Card_Data.Columns.catch_attempt].ToString();
            }

            foreach (DataRow row in dt.Rows)
            {
                doremiNormal = Convert.ToInt32(row["doremi_normal"]);
                doremiPlatinum = Convert.ToInt32(row["doremi_platinum"]);
                doremiMetal = Convert.ToInt32(row["doremi_metal"]);
                doremiOjamajos = Convert.ToInt32(row["doremi_ojamajos"]);

                hazukiNormal = Convert.ToInt32(row["hazuki_normal"]);
                hazukiPlatinum = Convert.ToInt32(row["hazuki_platinum"]);
                hazukiMetal = Convert.ToInt32(row["hazuki_metal"]);
                hazukiOjamajos = Convert.ToInt32(row["hazuki_ojamajos"]);

                aikoNormal = Convert.ToInt32(row["aiko_normal"]);
                aikoPlatinum = Convert.ToInt32(row["aiko_platinum"]);
                aikoMetal = Convert.ToInt32(row["aiko_metal"]);
                aikoOjamajos = Convert.ToInt32(row["aiko_ojamajos"]);

                onpuNormal = Convert.ToInt32(row["onpu_normal"]);
                onpuPlatinum = Convert.ToInt32(row["onpu_platinum"]);
                onpuMetal = Convert.ToInt32(row["onpu_metal"]);
                onpuOjamajos = Convert.ToInt32(row["onpu_ojamajos"]);

                momokoNormal = Convert.ToInt32(row["momoko_normal"]);
                momokoPlatinum = Convert.ToInt32(row["momoko_platinum"]);
                momokoMetal = Convert.ToInt32(row["momoko_metal"]);
                momokoOjamajos = Convert.ToInt32(row["momoko_ojamajos"]);

                otherSpecial = Convert.ToInt32(row["other_special"]);

                popNormal = Convert.ToInt32(row["pop_normal"]);
                hanaNormal = Convert.ToInt32(row["hana_normal"]);
                hanaPlatinum = Convert.ToInt32(row["hana_platinum"]);
                hanaMetal = Convert.ToInt32(row["hana_metal"]);

            }

            int totalSuccess = doremiNormal + doremiPlatinum + doremiMetal + doremiOjamajos  + 
               hazukiNormal + hazukiPlatinum + hazukiMetal + hazukiOjamajos +
               aikoNormal + aikoPlatinum + aikoMetal + aikoOjamajos + 
               onpuNormal + onpuPlatinum + onpuMetal + onpuOjamajos +
               momokoNormal + momokoPlatinum + momokoMetal + momokoOjamajos + otherSpecial;

            string doremiText = $"**Normal: {doremiNormal}/{Doremi.maxNormal}**\n" +
                $"**Platinum: {doremiPlatinum}/{Doremi.maxPlatinum}**\n" +
                $"**Metal: {doremiMetal}/{Doremi.maxMetal}**\n" +
                $"**Ojamajos: {doremiOjamajos}/{Doremi.maxOjamajos}**";
            int totalSuccessPack = doremiNormal + doremiPlatinum + doremiMetal + doremiOjamajos;
            int totalMax = Doremi.maxNormal+ Doremi.maxPlatinum + Doremi.maxMetal + Doremi.maxOjamajos;
                double calculated = (double)totalSuccessPack / totalMax * 100;
            if (Math.Round(calculated) >= 100)
            {
                //:white_check_mark:
            }
            string doremiPercentage = $"({Math.Round(calculated)}%)";

            string hazukiText = $"**Normal: {hazukiNormal}/{Hazuki.maxNormal}**\n" +
                $"**Platinum: {hazukiPlatinum}/{Hazuki.maxPlatinum}**\n" +
                $"**Metal: {hazukiMetal}/{Hazuki.maxMetal}**\n" +
                $"**Ojamajos: {hazukiOjamajos}/{Hazuki.maxOjamajos}**";
                totalSuccessPack = hazukiNormal + hazukiPlatinum + hazukiMetal + hazukiOjamajos;
                totalMax = Hazuki.maxNormal + Hazuki.maxPlatinum + Hazuki.maxMetal + Hazuki.maxOjamajos;
                calculated = (double)totalSuccessPack / totalMax * 100;
                string hazukiPercentage = $"({Math.Round(calculated)}%)";

            string aikoText = $"**Normal: {aikoNormal}/{Aiko.maxNormal}**\n" +
                $"**Platinum: {aikoPlatinum}/{Aiko.maxPlatinum}**\n" +
                $"**Metal: {aikoMetal}/{Aiko.maxMetal}**\n" +
                $"**Ojamajos: {aikoOjamajos}/{Aiko.maxOjamajos}**";
            totalSuccessPack = aikoNormal + aikoPlatinum + aikoMetal + aikoOjamajos;
            totalMax = Aiko.maxNormal + Aiko.maxPlatinum + Aiko.maxMetal + Aiko.maxOjamajos;
            calculated = (double)totalSuccessPack / totalMax * 100;
            string aikoPercentage = $"({Math.Round(calculated)}%)";

            string onpuText = $"**Normal: {onpuNormal}/{Onpu.maxNormal}**\n" +
                $"**Platinum: {onpuPlatinum}/{Onpu.maxPlatinum}**\n" +
                $"**Metal: {onpuMetal}/{Onpu.maxMetal}**\n" +
                $"**Ojamajos: {onpuOjamajos}/{Onpu.maxOjamajos}**";
            totalSuccessPack = onpuNormal + onpuPlatinum + onpuMetal + onpuOjamajos;
            totalMax = Onpu.maxNormal + Onpu.maxPlatinum + Onpu.maxMetal + Onpu.maxOjamajos;
            calculated = (double)totalSuccessPack / totalMax * 100;
            string onpuPercentage = $"({Math.Round(calculated)}%)";

            string momokoText = $"**Normal: {momokoNormal}/{Momoko.maxNormal}**\n" +
                $"**Platinum: {momokoPlatinum}/{Momoko.maxPlatinum}**\n" +
                $"**Metal: {momokoMetal}/{Momoko.maxMetal}**\n" +
                $"**Ojamajos: {momokoOjamajos}/{Momoko.maxOjamajos}**";
            totalSuccessPack = momokoNormal + momokoPlatinum + momokoMetal + momokoOjamajos;
            totalMax = Momoko.maxNormal + Momoko.maxPlatinum + Momoko.maxMetal + Momoko.maxOjamajos;
            calculated = (double)totalSuccessPack / totalMax * 100;
            string momokoPercentage = $"({Math.Round(calculated)}%)";

            string popText = $"**Normal: {popNormal}/{Pop.maxNormal}**";
            totalSuccessPack = popNormal;
            totalMax = Pop.maxNormal;
            calculated = (double)totalSuccessPack / totalMax * 100;
            string popPercentage = $"({Math.Round(calculated)}%)";

            string hanaText = $"**Normal: {hanaNormal}/{Hana.maxNormal}**\n" +
                $"**Platinum: {hanaPlatinum}/{Hana.maxPlatinum}**\n" +
                $"**Metal: {hanaMetal}/{Hana.maxMetal}**";
            totalSuccessPack = hanaNormal + hanaPlatinum + hanaMetal;
            totalMax = Hana.maxNormal + Hana.maxPlatinum + Hana.maxMetal;
            calculated = (double)totalSuccessPack / totalMax * 100;
            string hanaPercentage = $"({Math.Round(calculated)}%)";

            string otherText = $"**Special: {otherSpecial}/{maxSpecial}**";
            totalSuccessPack = otherSpecial;
            totalMax = maxSpecial;
            calculated = (double)totalSuccessPack / totalMax * 100;
            string otherPercentage = $"({Math.Round(calculated)}%)";

            return new EmbedBuilder()
                .WithTitle($"📇 {username} Card Status | Rank: {getUserRank(Convert.ToInt32(catchAttempt))}")
                .WithColor(color)
                .WithThumbnailUrl(thumbnailUrl)
                .AddField("Collected / EXP", $"**{totalSuccess} / {catchAttempt}**", false)
                .AddField($"Doremi Pack {doremiPercentage}", doremiText, true)
                .AddField($"Hazuki Pack {hazukiPercentage}", hazukiText, true)
                .AddField($"Aiko Pack {aikoPercentage}", aikoText, true)
                .AddField($"Onpu Pack {onpuPercentage}", onpuText, true)
                .AddField($"Momoko Pack {momokoPercentage}", momokoText, true)
                .AddField($"Other Pack {otherPercentage}", otherText, true)
                .AddField($"Hana Pack {hanaPercentage}", hanaText, true)
                .AddField($"Pop Pack {popPercentage}", popText, true);
        }

        public static EmbedBuilder printStatusComplete(SocketCommandContext context, Color color, SocketGuildUser otherUser = null)
        {
            DBC db = new DBC();
            var guildId = context.Guild.Id;
            var userId = context.User.Id;
            var username = context.User.Username;
            var thumbnailUrl = context.User.GetAvatarUrl();

            if (otherUser != null)
            {
                userId = otherUser.Id;
                username = otherUser.Username;
                thumbnailUrl = otherUser.GetAvatarUrl();
            }

            //for readd purpose if not exists
            UserTradingCardDataCore.getUserData(userId);

            string doremiIcon = ":x:"; string hazukiIcon = ":x:"; string aikoIcon = ":x:";
            string onpuIcon = ":x:"; string momokoIcon = ":x:"; string otherIcon = ":x:";
            string popIcon = ":x:"; string hanaIcon = ":x:";

            string doremiText = "Not completed yet..."; string hazukiText = "Not completed yet...";
            string aikoText = "Not completed yet..."; string onpuText = "Not completed yet...";
            string momokoText = "Not completed yet..."; string otherText = "Not completed yet...";
            string popText = "Not completed yet..."; string hanaText = "Not completed yet...";

            string query = $"SELECT * " +
                $" FROM {DBM_Trading_Card_Leaderboard.tableName} " +
                $" WHERE {DBM_Trading_Card_Leaderboard.Columns.id_guild}=@{DBM_Trading_Card_Leaderboard.Columns.id_guild} AND " +
                $" {DBM_Trading_Card_Leaderboard.Columns.id_user}=@{DBM_Trading_Card_Leaderboard.Columns.id_user} ";
            Dictionary<string, object> columnFilter = new Dictionary<string, object>();
            columnFilter[DBM_Trading_Card_Leaderboard.Columns.id_guild] = guildId;
            columnFilter[DBM_Trading_Card_Leaderboard.Columns.id_user] = userId;
            var result = db.selectAll(query, columnFilter);
            foreach(DataRow rows in result.Rows)
            {
                string completionDate = 
                    DateTime.Parse(rows[DBM_Trading_Card_Leaderboard.Columns.complete_date].ToString())
                    .ToString("MM/dd/yyyy");

                switch (rows[DBM_Trading_Card_Leaderboard.Columns.card_pack])
                {
                    case "doremi":
                        doremiIcon = ":white_check_mark:";
                        doremiText = completionDate;
                        break;
                    case "hazuki":
                        hazukiIcon = ":white_check_mark:";
                        hazukiText = completionDate;
                        break;
                    case "aiko":
                        aikoIcon = ":white_check_mark:";
                        aikoText = completionDate;
                        break;
                    case "onpu":
                        onpuIcon = ":white_check_mark:";
                        onpuText = completionDate;
                        break;
                    case "momoko":
                        momokoIcon = ":white_check_mark:";
                        momokoText = completionDate;
                        break;
                    case "other":
                        otherIcon = ":white_check_mark:";
                        otherText = completionDate;
                        break;
                    case "pop":
                        popIcon = ":white_check_mark:";
                        popText = completionDate;
                        break;
                    case "hana":
                        hanaIcon = ":white_check_mark:";
                        hanaText = completionDate;
                        break;
                }
            }

            return new EmbedBuilder()
                .WithTitle($"📇 {username} Card Completion Date Status")
                .WithColor(color)
                .WithThumbnailUrl(thumbnailUrl)
                .AddField($"{doremiIcon} Doremi Pack", doremiText, true)
                .AddField($"{hazukiIcon} Hazuki Pack", hazukiText, true)
                .AddField($"{aikoIcon} Aiko Pack", aikoText, true)
                .AddField($"{onpuIcon} Onpu Pack", onpuText, true)
                .AddField($"{momokoIcon} Momoko Pack", momokoText, true)
                .AddField($"{otherIcon} Other Pack", otherText, true)
                .AddField($"{hanaIcon} Hana Pack (Majorika Event 2020)", hanaText, true)
                .AddField($"{popIcon} Pop Pack (Majorika Event 2020)", popText, true);

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

        public static EmbedBuilder activatePureleine(SocketCommandContext context,string answer="")
        {
            var guildId = context.Guild.Id;
            var userId = context.User.Id;
            var userData = UserTradingCardDataCore.getUserData(userId);

            EmbedBuilder embed = new EmbedBuilder()
                .WithAuthor(TradingCardCore.BadCards.embedPureleineName, TradingCardCore.BadCards.embedPureleineAvatar)
                .WithColor(TradingCardCore.BadCards.embedPureleineColor);
            Dictionary<string, object> columnFilter = new Dictionary<string, object>();
            columnFilter[DBM_Trading_Card_Guild.Columns.id_guild] = guildId.ToString();
            columnFilter[DBM_Trading_Card_Guild.Columns.spawn_is_badcard] = 1;

            string query = @$"select * 
                            from {DBM_Trading_Card_Guild.tableName} guild, {DBM_Trading_Card_Data.tableName} tc  
                            where guild.{DBM_Trading_Card_Guild.Columns.id_guild}=@{DBM_Trading_Card_Guild.Columns.id_guild} and 
                            guild.{DBM_Trading_Card_Guild.Columns.spawn_is_badcard} =@{DBM_Trading_Card_Guild.Columns.spawn_is_badcard} and 
                            guild.{DBM_Trading_Card_Guild.Columns.spawn_id}=tc.{DBM_Trading_Card_Data.Columns.id_card}";
            var resultTradingCardGroup = new DBC().selectAll(query,columnFilter);
            if (resultTradingCardGroup.Rows.Count > 0)
            {
                foreach (DataRow rows in resultTradingCardGroup.Rows)
                {
                    string badCardType = TradingCardCore.BadCards.getType(
                    Convert.ToInt32(rows[DBM_Trading_Card_Guild.Columns.spawn_badcard_type]));

                    Console.WriteLine(Convert.ToInt32(rows[DBM_Trading_Card_Guild.Columns.spawn_badcard_type]));

                    string question = rows[DBM_Trading_Card_Guild.Columns.spawn_badcard_question].ToString();
                    string correctAnswer = rows[DBM_Trading_Card_Guild.Columns.spawn_badcard_answer].ToString();

                    if (userData[DBM_User_Trading_Card_Data.Columns.catch_token].ToString() == 
                        rows[DBM_Trading_Card_Guild.Columns.spawn_token].ToString() )
                    {
                        embed = new EmbedBuilder()
                        .WithAuthor(TradingCardCore.BadCards.embedPureleineName, TradingCardCore.BadCards.embedPureleineAvatar)
                        .WithColor(TradingCardCore.BadCards.embedPureleineColor)
                        .WithTitle("No No No!")
                        .WithDescription($"Sorry, you cannot use the pureleine command again. Please wait until the next card spawn.")
                        .WithThumbnailUrl(TradingCardCore.BadCards.imgAnswerWrong);
                    }
                    else if (answer == "")
                    {
                        embed = new EmbedBuilder()
                        .WithAuthor(TradingCardCore.BadCards.embedPureleineName, TradingCardCore.BadCards.embedPureleineAvatar)
                        .WithColor(TradingCardCore.BadCards.embedPureleineColor)
                        .WithTitle("Bad card detected!")
                        .WithDescription($"I'm detecting a great amount of bad card energy! You need to remove it with **<bot>!card pureleine <answer>** commands from the question below:")
                        .WithThumbnailUrl(TradingCardCore.BadCards.imgPureleineFound)
                        .AddField("Question:", $"{question} = ?");
                    }
                    else if (answer != correctAnswer)
                    {
                        embed = new EmbedBuilder()
                        .WithAuthor(TradingCardCore.BadCards.embedPureleineName, TradingCardCore.BadCards.embedPureleineAvatar)
                        .WithColor(TradingCardCore.BadCards.embedPureleineColor)
                        .WithTitle("Wrong answer!")
                        .WithDescription($":x: That answer is wrong! **{context.User.Username}** also lost a chance to capture card for this turn...")
                        .WithThumbnailUrl(TradingCardCore.BadCards.imgAnswerWrong);

                        query = $"UPDATE {DBM_User_Trading_Card_Data.tableName} " +
                            $" SET {DBM_User_Trading_Card_Data.Columns.catch_token}=@{DBM_User_Trading_Card_Data.Columns.catch_token} " +
                            $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user} ";
                        columnFilter = new Dictionary<string, object>();
                        columnFilter[DBM_User_Trading_Card_Data.Columns.catch_token] = rows[DBM_Trading_Card_Guild.Columns.spawn_token];
                        columnFilter[DBM_User_Trading_Card_Data.Columns.id_user] = userId.ToString();
                        new DBC().update(query, columnFilter);
                    } else
                    {
                        embed = new EmbedBuilder()
                        .WithAuthor(TradingCardCore.BadCards.embedPureleineName, TradingCardCore.BadCards.embedPureleineAvatar)
                        .WithColor(TradingCardCore.BadCards.embedPureleineColor)
                        .WithTitle("Bad cards effect has been removed!")
                        .WithDescription($":white_check_mark: You may now safely capture the spawned card again.\n")
                        .WithFooter($"Removed by: {context.User.Username}")
                        .WithThumbnailUrl(TradingCardCore.BadCards.imgAnswerCorrect);

                        //reset bad card
                        query = $"UPDATE {DBM_Trading_Card_Guild.tableName} " +
                            $" SET {DBM_Trading_Card_Guild.Columns.spawn_badcard_question}=@{DBM_Trading_Card_Guild.Columns.spawn_badcard_question}, " +
                            $" {DBM_Trading_Card_Guild.Columns.spawn_badcard_answer}=@{DBM_Trading_Card_Guild.Columns.spawn_badcard_answer}, " +
                            $" {DBM_Trading_Card_Guild.Columns.spawn_is_badcard}=@{DBM_Trading_Card_Guild.Columns.spawn_is_badcard} " +
                            $" WHERE {DBM_Trading_Card_Guild.Columns.id_guild}=@{DBM_Trading_Card_Guild.Columns.id_guild} ";
                        columnFilter = new Dictionary<string, object>();
                        columnFilter[DBM_Trading_Card_Guild.Columns.spawn_badcard_question] = "";
                        columnFilter[DBM_Trading_Card_Guild.Columns.spawn_badcard_answer] = "";
                        columnFilter[DBM_Trading_Card_Guild.Columns.spawn_is_badcard] = 0;
                        columnFilter[DBM_Trading_Card_Guild.Columns.id_guild] = context.Guild.Id;
                        new DBC().update(query, columnFilter);

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
                            //var jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));
                            //var key = JObject.Parse(jObjTradingCardList[parent][chosenCategory].ToString()).Properties().ToList();
                            //int randIndex = new Random().Next(0, key.Count);
                            //string chosenId = key[randIndex].Name;

                            //chosen data:
                            //chosenId = "do004"; chosenCategory = "normal"; parent = "doremi"; //for debug only
                            //string chosenName = jObjTradingCardList[parent][chosenCategory][chosenId]["name"].ToString();
                            query = $"SELECT * " +
                                $" FROM {DBM_Trading_Card_Data.tableName}  " +
                                $" ORDER BY rand() " +
                                $" LIMIT 1 ";
                            var resultRandomCard = new DBC().selectAll(query);
                            string chosenId = ""; string chosenName = ""; string chosenUrl = "";
                            foreach (DataRow rowsRandomCard in resultRandomCard.Rows)
                            {
                                chosenId = rowsRandomCard[DBM_Trading_Card_Data.Columns.id_card].ToString();
                                chosenName = rowsRandomCard[DBM_Trading_Card_Data.Columns.name].ToString();
                                chosenUrl = rowsRandomCard[DBM_Trading_Card_Data.Columns.url].ToString();
                            }

                            if (checkUserHaveCard(context.User.Id, parent, chosenId))
                            {
                                embed.Description += $"Sorry, I can't give **{context.User.Username}** a bonus card: **{chosenId} - {chosenName}** because you have it already...";
                            } else
                            {
                                embed.Description += $"**{context.User.Username}** have been rewarded with a bonus card!";
                                embed.AddField($"{chosenCategory} {parent} Bonus Card Reward:", $"**{chosenId} - {chosenName}**");
                                embed.WithImageUrl(chosenUrl);
                                //add bonus card & update catch attempt
                                columnFilter = new Dictionary<string, object>();
                                columnFilter[DBM_User_Trading_Card_Inventory.Columns.id_user] = userId.ToString();
                                columnFilter[DBM_User_Trading_Card_Inventory.Columns.id_card] = chosenId;

                                new DBC().insert(DBM_User_Trading_Card_Inventory.tableName, columnFilter);
                                UserTradingCardDataCore.addCatchAttempt(context.User.Id);
                            }
                        }
                        else if (badCardType == "seeds")
                        {
                            int randomedMagicSeeds = new Random().Next(10, 25);
                            embed.Description += $"**{context.User.Username}** have been rewarded with {randomedMagicSeeds} magic seeds!";
                            embed.WithImageUrl(GardenCore.imgMagicSeeds);
                            UserDataCore.updateMagicSeeds(context.User.Id, randomedMagicSeeds);
                        }

                    }

                }
            } 
            else
            {
                embed = new EmbedBuilder()
                .WithAuthor(TradingCardCore.BadCards.embedPureleineName, TradingCardCore.BadCards.embedPureleineAvatar)
                .WithColor(TradingCardCore.BadCards.embedPureleineColor)
                .WithDescription(":x: I didn't sense any bad cards energy right now...")
                .WithThumbnailUrl(TradingCardCore.BadCards.imgPureleineNotFound);
            }

            return embed;

        }

        public static Tuple<string, EmbedBuilder, string, IDictionary<string, Boolean>> cardCapture(SocketCommandContext context, 
            Color color, string emojiError, string parent, string boost, string errorPrefix)
        {
            ulong guildId = context.Guild.Id;
            ulong clientId = context.User.Id;
            string username = context.User.Username;
            string embedAvatarUrl = context.User.GetAvatarUrl();

            //select spawned card
            var userData = UserTradingCardDataCore.getUserData(Convert.ToUInt64(clientId));

            Dictionary<string, object> columns = new Dictionary<string, object>();
            DBC db = new DBC();

            EmbedBuilder returnEmbedBuilder;
            string replyText = "";
            IDictionary<string, Boolean> returnCompleted = new Dictionary<string, Boolean>();//state of completedCard
            returnCompleted.Add("doremi", false); returnCompleted.Add("hazuki", false); returnCompleted.Add("aiko", false);
            returnCompleted.Add("onpu", false); returnCompleted.Add("momoko", false); returnCompleted.Add("special", false);

            string spawnedCardId = ""; string spawnedCardPack = ""; string spawnedCardCategory = "";
            int spawnedIsMystery = 0; string spawnedBadCardType = "";
            int spawnedIsBadCard = 0; int spawnedIsZone = 0; string spawnedToken = "";

            //spawned card information
            string name = ""; string imgUrl = ""; string rank = ""; string star = ""; string point = "";

            //get guild spawned card information
            var guildSpawnData = TradingCardGuildCore.getGuildData(guildId);

            //get spawn info
            if (Convert.ToBoolean(guildSpawnData[DBM_Trading_Card_Guild.Columns.spawn_is_zone]) || 
                (guildSpawnData[DBM_Trading_Card_Guild.Columns.spawn_id].ToString()!=""&&
                !Convert.ToBoolean(guildSpawnData[DBM_Trading_Card_Guild.Columns.spawn_is_zone])))
            {
                spawnedCardId = guildSpawnData[DBM_Trading_Card_Guild.Columns.spawn_id].ToString();
                spawnedCardPack = guildSpawnData[DBM_Trading_Card_Guild.Columns.spawn_parent].ToString();
                spawnedCardCategory = guildSpawnData[DBM_Trading_Card_Guild.Columns.spawn_category].ToString();

                spawnedIsMystery = Convert.ToInt32(guildSpawnData[DBM_Trading_Card_Guild.Columns.spawn_is_mystery]);
                spawnedIsBadCard = Convert.ToInt32(guildSpawnData[DBM_Trading_Card_Guild.Columns.spawn_is_badcard]);
                if (spawnedIsBadCard == 1)
                    spawnedBadCardType = BadCards.getType(Convert.ToInt32(guildSpawnData[DBM_Trading_Card_Guild.Columns.spawn_badcard_type]));
                    
                spawnedIsZone = Convert.ToInt32(guildSpawnData[DBM_Trading_Card_Guild.Columns.spawn_is_zone]);
                spawnedToken = guildSpawnData[DBM_Trading_Card_Guild.Columns.spawn_token].ToString();
            } 
            else 
            {
                returnEmbedBuilder = new EmbedBuilder()
                .WithColor(color)
                .WithDescription(":x: Sorry, either this card has been captured by someone or not spawned anymore. " +
                "Please wait for the card to spawn again.")
                .WithThumbnailUrl(emojiError);
                return Tuple.Create("", returnEmbedBuilder, "", returnCompleted);
            }

            //check if spawned token same with user
            if (userData[DBM_User_Trading_Card_Data.Columns.catch_token].ToString() == spawnedToken)
            {
                //check if spawned token is same like guild
                returnEmbedBuilder = new EmbedBuilder()
                .WithColor(color)
                .WithDescription($":x: Sorry, please wait for the next card spawn.")
                .WithThumbnailUrl(emojiError);
                return Tuple.Create("", returnEmbedBuilder, "", returnCompleted);
            }

            //end get guild spawned card information

            //get card information
            if (spawnedIsZone == 1)
            {
                if (!userData[DBM_User_Trading_Card_Data.Columns.card_zone].ToString().Contains("ojamajos"))
                {//default card zone
                    //override if card type is zone
                    string query = $"SELECT * " +
                    $" FROM {DBM_Trading_Card_Data.tableName} " +
                    $" WHERE {DBM_Trading_Card_Data.Columns.pack}=@{DBM_Trading_Card_Data.Columns.pack} AND " +
                    $" {DBM_Trading_Card_Data.Columns.category}=@{DBM_Trading_Card_Data.Columns.category} " +
                    $" ORDER BY rand() " +
                    $" LIMIT 1 ";
                    string[] splitted = userData[DBM_User_Trading_Card_Data.Columns.card_zone].ToString()
                        .Split(" ");
                    columns = new Dictionary<string, object>();
                    columns[DBM_Trading_Card_Data.Columns.pack] = parent;
                    columns[DBM_Trading_Card_Data.Columns.category] = splitted[1];
                    var dbZone = db.selectAll(query,columns);

                    foreach (DataRow rows in dbZone.Rows)
                    {
                        spawnedCardId = rows[DBM_Trading_Card_Data.Columns.id_card].ToString();
                        spawnedCardPack = rows[DBM_Trading_Card_Data.Columns.pack].ToString();
                        spawnedCardCategory = rows[DBM_Trading_Card_Data.Columns.category].ToString();

                        name = rows[DBM_Trading_Card_Data.Columns.name].ToString();
                        imgUrl = rows[DBM_Trading_Card_Data.Columns.url].ToString();
                        rank = rows[DBM_Trading_Card_Data.Columns.attr_0].ToString();
                        star = rows[DBM_Trading_Card_Data.Columns.attr_1].ToString();
                        point = rows[DBM_Trading_Card_Data.Columns.attr_2].ToString();
                    }
                } else {//ojamajo card zone
                    //override if card type is zone
                    string query = $"SELECT * " +
                    $" FROM {DBM_Trading_Card_Data_Ojamajos.tableName}" +
                    $" WHERE {DBM_Trading_Card_Data_Ojamajos.Columns.pack} like @{DBM_Trading_Card_Data_Ojamajos.Columns.pack} " +
                    $" ORDER BY rand() " +
                    $" LIMIT 1 ";
                    columns = new Dictionary<string, object>();
                    columns[DBM_Trading_Card_Data_Ojamajos.Columns.pack] = $"%{parent}%";
                    var dbZone = db.selectAll(query,columns);

                    foreach (DataRow rows in dbZone.Rows)
                    {
                        spawnedCardId = rows[DBM_Trading_Card_Data_Ojamajos.Columns.id_card].ToString();
                        spawnedCardPack = rows[DBM_Trading_Card_Data_Ojamajos.Columns.pack].ToString();
                        spawnedCardCategory = "ojamajos";

                        name = rows[DBM_Trading_Card_Data_Ojamajos.Columns.name].ToString();
                        imgUrl = rows[DBM_Trading_Card_Data_Ojamajos.Columns.url].ToString();
                        rank = rows[DBM_Trading_Card_Data_Ojamajos.Columns.attr_0].ToString();
                        star = rows[DBM_Trading_Card_Data_Ojamajos.Columns.attr_1].ToString();
                        point = rows[DBM_Trading_Card_Data_Ojamajos.Columns.attr_2].ToString();
                    }
                }
                
            } 
            else
            {
                spawnedCardPack = guildSpawnData[DBM_Trading_Card_Guild.Columns.spawn_parent].ToString();
                spawnedCardCategory = guildSpawnData[DBM_Trading_Card_Guild.Columns.spawn_category].ToString();
                spawnedCardId = guildSpawnData[DBM_Trading_Card_Guild.Columns.spawn_id].ToString();
                if(spawnedCardCategory == "ojamajos"){
                    //ojamajo card category
                    string query = $"SELECT * " +
                    $" FROM {DBM_Trading_Card_Data_Ojamajos.tableName} " +
                    $" WHERE {DBM_Trading_Card_Data_Ojamajos.Columns.id_card}=@{DBM_Trading_Card_Data_Ojamajos.Columns.id_card}";
                    Dictionary<string, object> columnFilter = new Dictionary<string, object>();
                    columnFilter[DBM_Trading_Card_Data_Ojamajos.Columns.id_card] = spawnedCardId;
                    var selectedCard = db.selectAll(query,columnFilter);
                    foreach(DataRow rows in selectedCard.Rows)
                    {
                        name = rows[DBM_Trading_Card_Data.Columns.name].ToString();
                        imgUrl = rows[DBM_Trading_Card_Data.Columns.url].ToString();
                        rank = rows[DBM_Trading_Card_Data.Columns.attr_0].ToString();
                        star = rows[DBM_Trading_Card_Data.Columns.attr_1].ToString();
                        point = rows[DBM_Trading_Card_Data.Columns.attr_2].ToString();
                    }
                } else
                {
                    string query = $"SELECT * " +
                    $" FROM {DBM_Trading_Card_Data.tableName} " +
                    $" WHERE {DBM_Trading_Card_Data.Columns.id_card}=@{DBM_Trading_Card_Data.Columns.id_card}";
                    Dictionary<string, object> columnFilter = new Dictionary<string, object>();
                    columnFilter[DBM_Trading_Card_Data.Columns.id_card] = spawnedCardId;
                    var selectedCard = db.selectAll(query, columnFilter);
                    foreach (DataRow rows in selectedCard.Rows)
                    {
                        name = rows[DBM_Trading_Card_Data.Columns.name].ToString();
                        imgUrl = rows[DBM_Trading_Card_Data.Columns.url].ToString();
                        rank = rows[DBM_Trading_Card_Data.Columns.attr_0].ToString();
                        star = rows[DBM_Trading_Card_Data.Columns.attr_1].ToString();
                        point = rows[DBM_Trading_Card_Data.Columns.attr_2].ToString();
                    }
                }
            }
            //get card information end

            //get card boost information
            int boostNormal = 0; int boostPlatinum = 0; int boostMetal = 0;
            int boostOjamajos = 0; int boostSpecial = 0;
            int playerRank = getUserRank(Convert.ToInt32(userData["catch_attempt"]));
            if (spawnedCardCategory.ToLower() == "special")
            {
                parent = "other";
                boostSpecial = Convert.ToInt32(userData["boost_other_special"]);
            }
            else
            {
                boostNormal = Convert.ToInt32(userData[$"boost_{parent}_normal"]);
                boostPlatinum = Convert.ToInt32(userData[$"boost_{parent}_platinum"]);
                boostMetal = Convert.ToInt32(userData[$"boost_{parent}_metal"]);
                boostOjamajos = Convert.ToInt32(userData[$"boost_{parent}_ojamajos"]);
            }
            //get card boost information end

            //check process booster
            Boolean useBoost = false;
            if (boost.ToLower() != "" && boost.ToLower() != "boost")
            {
                returnEmbedBuilder = new EmbedBuilder()
                .WithColor(color)
                .WithDescription($":x: Sorry, that is not the valid card capture boost command. " +
                $"Use: **{errorPrefix}card capture boost** to activate the card boost.")
                .WithThumbnailUrl(emojiError);
                return Tuple.Create("", returnEmbedBuilder, "", returnCompleted);
            } 
            else if (boost.ToLower() == "boost")
            {
                if ((spawnedCardCategory == "normal" && boostNormal <= 0) ||
                    (spawnedCardCategory == "platinum" && boostPlatinum <= 0 )||
                    (spawnedCardCategory == "metal" && boostMetal <= 0)||
                    (spawnedCardCategory == "ojamajos" && boostOjamajos <= 0)||
                    (spawnedCardCategory == "special" && boostSpecial <= 0))
                {
                    returnEmbedBuilder = new EmbedBuilder()
                    .WithColor(color)
                    .WithDescription($":x: Sorry, you have no **{parent} {spawnedCardCategory}** card capture boost that you can use.")
                    .WithThumbnailUrl(emojiError);
                    return Tuple.Create("", returnEmbedBuilder, "", returnCompleted);
                }
                else
                    useBoost = true;
            }

            if (spawnedIsMystery == 1 && parent != spawnedCardPack)
            {
                //update token
                UserTradingCardDataCore.addCatchAttempt(clientId, spawnedToken);
                replyText = ":x: Sorry, you guessed the wrong mystery card.";

                //returnEmbedBuilder = new EmbedBuilder()
                //.WithColor(color)
                //.WithDescription($":x: Sorry, you guessed the wrong mystery card.")
                //.WithThumbnailUrl(emojiError);
                //return Tuple.Create("", returnEmbedBuilder, "", returnCompleted);
            }
            else if (spawnedIsZone==1||
            (spawnedCardId != "" && spawnedCardCategory != ""))
            {
                if (spawnedIsZone == 1 || 
                    parent == spawnedCardPack)//check if the caller bot pack is same like the guild spawn
                {
                    int catchState = 0;

                    //check last capture time
                    try
                    {
                        int boostPlayerRank = 0;
                        if (playerRank >= 2) boostPlayerRank = playerRank;
                        int catchRate = new Random().Next(10 - boostPlayerRank);
                        
                        //check if user have the card/not
                        if (checkUserHaveCard(clientId,parent,spawnedCardId))
                        {//card already exist on inventory
                            if (spawnedIsMystery != 1)
                                replyText = $":x: Sorry, I can't capture **{spawnedCardId} - {name}** because you already have it. " +
                                    $"";
                            else
                                replyText = $":x: You guessed the mystery card correctly but I can't capture **{spawnedCardId} - {name}** because you aleady have it. " +
                                    $"";
                            //UserDataCore.updateMagicSeeds(clientId, 1);
                            //UserTradingCardDataCore.updateFragmentPoints(clientId, 1);
                            UserTradingCardDataCore.addCatchAttempt(clientId, spawnedToken);
                        }
                        else
                        {
                            //user don't have card yet
                            int maxCard = 0; string boostRate = "";
                            //init RNG catch rate
                            //if boost: change the TradingCardCore.captureRate

                            if (spawnedCardCategory.ToLower() == "normal")
                            {
                                if (!useBoost)
                                {
                                    if ((catchRate < TradingCardCore.captureRateNormal && spawnedIsMystery != 1) ||
                                        (catchRate < TradingCardCore.captureRateNormal + 1 && spawnedIsMystery != 1)) catchState = 1;
                                }
                                else
                                {
                                    boostRate = $"{boostNormal * 10}%";
                                    if ((catchRate < boostNormal && spawnedIsMystery != 1) ||
                                        (catchRate < boostNormal + 1 && spawnedIsMystery == 1)) catchState = 1;
                                }
                            }
                            else if (spawnedCardCategory.ToLower() == "platinum")
                            {
                                if (!useBoost)
                                {
                                    if ((catchRate < TradingCardCore.captureRatePlatinum && spawnedIsMystery != 1) ||
                                        (catchRate < TradingCardCore.captureRatePlatinum + 1 && spawnedIsMystery == 1)) catchState = 1;
                                }
                                else
                                {
                                    boostRate = $"{boostPlatinum * 10}%";
                                    if ((catchRate < boostPlatinum && spawnedIsMystery != 1) ||
                                        (catchRate < boostPlatinum + 1 && spawnedIsMystery == 1)) catchState = 1;
                                }
                            }
                            else if (spawnedCardCategory.ToLower() == "metal")
                            {
                                if (!useBoost)
                                {
                                    if ((catchRate < TradingCardCore.captureRateMetal && spawnedIsMystery != 1) ||
                                        (catchRate < TradingCardCore.captureRateMetal + 2 && spawnedIsMystery == 1)) catchState = 1;
                                }
                                else
                                {
                                    boostRate = $"{boostMetal * 10}%";
                                    if ((catchRate < boostMetal && spawnedIsMystery != 1) ||
                                        (catchRate < boostMetal + 2 && spawnedIsMystery == 1)) catchState = 1;
                                }
                            }
                            else if (spawnedCardCategory.ToLower() == "ojamajos")
                            {
                                if (!useBoost && catchRate < TradingCardCore.captureRateOjamajos)
                                    catchState = 1;
                                else if (useBoost && catchRate < boostOjamajos)
                                    catchState = 1;
                                if (useBoost) boostRate = $"{boostOjamajos * 10}%";
                            }
                            else if (spawnedCardCategory.ToLower() == "special")
                            {
                                if (!useBoost && catchRate < TradingCardCore.captureRateSpecial)
                                    catchState = 1;
                                else if (useBoost && catchRate < boostSpecial)
                                    catchState = 1;
                                if (useBoost) boostRate = $"{boostSpecial * 10}%";
                            }

                            if (spawnedIsBadCard == 1)
                            {
                                //bad card trigger
                                emojiError = BadCards.imgBadCardActivated;

                                switch (spawnedBadCardType)
                                {
                                    case "seeds":
                                        int randomLost = new Random().Next(1, 11);
                                        replyText = $":skull: Oh no, **{spawnedBadCardType}** bad card effect has activated! " +
                                            $"**{username}** just lost {randomLost} magic seeds!";

                                        UserDataCore.updateMagicSeeds(Convert.ToUInt64(clientId), -randomLost);
                                        break;
                                    case "curse":
                                        string parentLost = parent;
                                        if (parent != "doremi" && parent != "hazuki" && parent != "aiko" && parent != "onpu" && parent != "momoko")
                                            parentLost = "doremi";

                                        //get the random lost card information from user
                                        string query = @$"select * 
                                            from {DBM_User_Trading_Card_Inventory.tableName} inv, {DBM_Trading_Card_Data.tableName} tc 
                                            where inv.{DBM_User_Trading_Card_Inventory.Columns.id_user}=@{DBM_User_Trading_Card_Inventory.Columns.id_user} and                                       inv.{DBM_User_Trading_Card_Inventory.Columns.id_card} = tc.{DBM_User_Trading_Card_Inventory.Columns.id_card} and 
                                            tc.{DBM_Trading_Card_Data.Columns.pack}=@{DBM_Trading_Card_Data.Columns.pack} and 
                                            tc.{DBM_Trading_Card_Data.Columns.category}=@{DBM_Trading_Card_Data.Columns.category} 
                                            order by rand() 
                                            limit 1";
                                        columns = new Dictionary<string, object>();
                                        columns[DBM_User_Trading_Card_Inventory.Columns.id_user] = clientId.ToString();
                                        columns[DBM_Trading_Card_Data.Columns.pack] = parentLost;
                                        columns[DBM_Trading_Card_Data.Columns.category] = "normal";
                                        var resultLost = new DBC().selectAll(query, columns);

                                        if (resultLost.Rows.Count >= 1)
                                        {
                                            string stolenCardId = ""; string stolenName = "";

                                            foreach (DataRow rows in resultLost.Rows)
                                            {
                                                stolenCardId = rows[DBM_User_Trading_Card_Inventory.Columns.id_card].ToString();
                                            }

                                            //delete
                                            query = $"DELETE FROM {DBM_User_Trading_Card_Inventory.tableName} " +
                                                $" WHERE {DBM_User_Trading_Card_Inventory.Columns.id_card}=@{DBM_User_Trading_Card_Inventory.Columns.id_card} AND " +
                                                $" {DBM_User_Trading_Card_Inventory.Columns.id_user}=@{DBM_User_Trading_Card_Inventory.Columns.id_user} ";
                                            columns = new Dictionary<string, object>();
                                            columns[DBM_User_Trading_Card_Inventory.Columns.id_card] = stolenCardId;
                                            columns[DBM_User_Trading_Card_Inventory.Columns.id_user] = clientId.ToString();
                                            new DBC().delete(query, columns);

                                            replyText = $":skull: Oh no, **{spawnedBadCardType}** bad card effect has activated! " +
                                                $"**{username}** just lost **{parentLost} normal** card: **{stolenCardId} - {stolenName}**!";
                                        }
                                        else
                                        {
                                            replyText = $":skull: Oh no, **{spawnedBadCardType}** bad card effect has activated! But **{username}** don't have any card to lose...";
                                        }
                                        break;
                                    case "failure":
                                        replyText = $":skull: Oh no, **{spawnedBadCardType}** bad card effect has activated! " +
                                            $"**{username}** just lost a chance to catch a card on this spawn turn!";

                                        //update
                                        query = $"UPDATE {DBM_User_Trading_Card_Data.tableName} " +
                                            $" SET {DBM_User_Trading_Card_Data.Columns.catch_token}=@{DBM_User_Trading_Card_Data.Columns.catch_token} " +
                                            $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user} ";
                                        columns = new Dictionary<string, object>();
                                        columns[DBM_User_Trading_Card_Data.Columns.catch_token] = spawnedToken;
                                        columns[DBM_User_Trading_Card_Data.Columns.id_user] = clientId;
                                        new DBC().update(query, columns);
                                        break;
                                }

                                //bad card ends
                            }
                            else if (catchState == 1)
                            {
                                columns = new Dictionary<string, object>();
                                columns["boost"] = 0;
                                //card not exist yet
                                if (useBoost)
                                {//reset boost
                                    string query = "";
                                    replyText = $":arrow_double_up: **{GlobalFunctions.UppercaseFirst(spawnedCardPack)} {GlobalFunctions.UppercaseFirst(spawnedCardCategory)}** " +
                                        $"Card capture boost has been used and boosted into **{boostRate}**!\n";
                                    switch (spawnedCardCategory)
                                    {
                                        case "special":
                                            query = $"UPDATE {DBM_User_Trading_Card_Data.tableName} " +
                                            $" SET {DBM_User_Trading_Card_Data.Columns.boost_other_special}=@boost " +
                                            $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user} ";

                                            //arrInventory["boost"]["other"]["special"] = "0";
                                            break;
                                        default:
                                            query = $"UPDATE {DBM_User_Trading_Card_Data.tableName} " +
                                            $" SET boost_{spawnedCardPack.ToLower()}_{spawnedCardCategory.ToLower()}=@boost " +
                                            $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user} ";

                                            //arrInventory["boost"][parent][spawnedCardCategory] = "0";
                                            break;
                                    }
                                    new DBC().update(query, columns);
                                }

                                //item.Add(spawnedCardId,);

                                //add card to inventory
                                if (spawnedCardCategory == "ojamajos")
                                {   //card type is ojamajos
                                    //check related card pack
                                    string query = $"SELECT * " +
                                        $" FROM {DBM_User_Trading_Card_Inventory_Ojamajos.tableName} " +
                                        $" WHERE {DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_card}=@{DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_card}";
                                    columns = new Dictionary<string, object>();
                                    columns[DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_card] = spawnedCardId;
                                    var relatedResult = new DBC().selectAll(query, columns);

                                    foreach(DataRow rows in relatedResult.Rows)
                                    {
                                        string[] related = rows["pack"].ToString().Split(",");
                                        for(int i = 0;i<related.Length;i++)
                                        {
                                            try
                                            {
                                                columns = new Dictionary<string, object>();
                                                columns[DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_user] = clientId;
                                                columns[DBM_User_Trading_Card_Inventory_Ojamajos.Columns.pack] = related[i];
                                                columns[DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_card] = spawnedCardId;
                                                db.insert(DBM_User_Trading_Card_Inventory_Ojamajos.tableName, columns);
                                            }
                                            catch { }
                                        }
                                    }
                                } else
                                {   //card type is normal/non ojamajos 
                                    columns = new Dictionary<string, object>();
                                    columns[DBM_User_Trading_Card_Inventory.Columns.id_user] = clientId;
                                    columns[DBM_User_Trading_Card_Inventory.Columns.id_card] = spawnedCardId;
                                    db.insert(DBM_User_Trading_Card_Inventory.tableName, columns);
                                }

                                //level up notification
                                string returnLevelUp = "";
                                if ((int)userData["catch_attempt"] == 100 || (int)userData["catch_attempt"] == 200 ||
                                    (int)userData["catch_attempt"] == 300 || (int)userData["catch_attempt"] == 400 ||
                                    (int)userData["catch_attempt"] == 500)
                                {
                                    returnLevelUp = $":up: Congratulations! **{username}** is now rank {getUserRank((int)userData["catch_attempt"])}!";
                                }

                                string[] arrRandomFirstSentence = {
                                    "Congratulations,","Nice Catch!","Nice one!","Yatta!"
                                };

                                if (spawnedIsMystery == 1)
                                    replyText += $":white_check_mark: {arrRandomFirstSentence[new Random().Next(0, arrRandomFirstSentence.Length)]} " +
                                    $"**{username}** have successfully revealed & captured **{spawnedCardCategory}** mystery card: **{name}**";
                                else
                                {
                                    replyText += $":white_check_mark: {arrRandomFirstSentence[new Random().Next(0, arrRandomFirstSentence.Length)]} " +
                                    $"**{username}** have successfully captured **{spawnedCardCategory}** card: **{name}**";

                                    if (spawnedIsZone == 1)
                                    {
                                        replyText += " from the zone card.";
                                    }
                                }

                                //get latest total data
                                int currentTotal = 0;
                                if (parent == "ojamajos")
                                {
                                    //get max current
                                    string query = @$"select count(distinct(inv.{DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_card})) as total 
                                            from {DBM_User_Trading_Card_Inventory_Ojamajos.tableName} inv, {DBM_Trading_Card_Data_Ojamajos.tableName} tc 
                                            where inv.{DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_user}=@{DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_user} and 
                                            inv.{DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_card}=tc.{DBM_Trading_Card_Data_Ojamajos.Columns.id_card} and 
                                            tc.{DBM_Trading_Card_Data_Ojamajos.Columns.pack} like @{DBM_Trading_Card_Data_Ojamajos.Columns.pack}";
                                    columns = new Dictionary<string, object>();
                                    columns[DBM_User_Trading_Card_Inventory_Ojamajos.Columns.id_user] = clientId.ToString();
                                    columns[DBM_Trading_Card_Data_Ojamajos.Columns.pack] = $"%{parent}%";
                                    var result = new DBC().selectAll(query, columns);
                                    foreach(DataRow row in result.Rows)
                                    {
                                        currentTotal = Convert.ToInt32(row["total"]);
                                    }

                                    //get max total
                                    query = @$"select count(distinct(inv.{DBM_Trading_Card_Data_Ojamajos.Columns.id_card})) as total 
                                            from {DBM_Trading_Card_Data_Ojamajos.tableName} tc 
                                            tc.{DBM_Trading_Card_Data_Ojamajos.Columns.pack} like @{DBM_Trading_Card_Data_Ojamajos.Columns.pack}";
                                    columns = new Dictionary<string, object>();
                                    columns[DBM_Trading_Card_Data_Ojamajos.Columns.pack] = $"%{parent}%";
                                    result = new DBC().selectAll(query, columns);
                                    foreach (DataRow row in result.Rows)
                                    {
                                        maxCard = Convert.ToInt32(row["total"]);
                                    }
                                } else
                                {
                                    //get max current
                                    string query = @$"select count(distinct(inv.{DBM_User_Trading_Card_Inventory.Columns.id_card})) as total  
                                            from {DBM_User_Trading_Card_Inventory.tableName} inv, {DBM_Trading_Card_Data.tableName} tc 
                                            where inv.{DBM_User_Trading_Card_Inventory.Columns.id_user}=@{DBM_User_Trading_Card_Inventory.Columns.id_user} and 
                                            tc.{DBM_Trading_Card_Data.Columns.id_card}=inv.{DBM_User_Trading_Card_Inventory.Columns.id_card} and 
                                            tc.{DBM_Trading_Card_Data.Columns.pack}=@{DBM_Trading_Card_Data.Columns.pack} and 
                                            tc.{DBM_Trading_Card_Data.Columns.category}=@{DBM_Trading_Card_Data.Columns.category}";
                                    columns = new Dictionary<string, object>();
                                    columns[DBM_User_Trading_Card_Inventory.Columns.id_user] = clientId.ToString();
                                    columns[DBM_Trading_Card_Data.Columns.pack] = spawnedCardPack;
                                    columns[DBM_Trading_Card_Data.Columns.category] = spawnedCardCategory;
                                        
                                    var result = new DBC().selectAll(query, columns);
                                    foreach (DataRow row in result.Rows)
                                    {
                                        currentTotal = Convert.ToInt32(row["total"]);
                                    }

                                    //get max total
                                    query = @$"select count(distinct(tc.{DBM_Trading_Card_Data.Columns.id_card})) as total  
                                        from {DBM_Trading_Card_Data.tableName} tc 
                                        where tc.{DBM_Trading_Card_Data.Columns.pack}=@{DBM_Trading_Card_Data.Columns.pack} and 
                                        tc.{DBM_Trading_Card_Data.Columns.category}=@{DBM_Trading_Card_Data.Columns.category}";
                                    columns = new Dictionary<string, object>();
                                    columns[DBM_Trading_Card_Data.Columns.pack] = spawnedCardPack;
                                    columns[DBM_Trading_Card_Data.Columns.category] = spawnedCardCategory;
                                    result = new DBC().selectAll(query, columns);
                                    foreach (DataRow row in result.Rows)
                                    {
                                        maxCard = Convert.ToInt32(row["total"]);
                                    }
                                }

                                returnEmbedBuilder = TradingCardCore.printCardCaptureTemplate(color, name, imgUrl,
                                spawnedCardId, spawnedCardCategory, rank, star, point, username, embedAvatarUrl,
                                currentTotal, maxCard);

                                //check card completion
                                if (UserTradingCardDataCore.checkCardCompletion(clientId, parent))
                                    returnCompleted[parent] = true;

                                //if (UserTradingCardDataCore.checkCardCompletion(clientId,"doremi"))
                                //    returnCompleted["doremi"] = true;

                                //if (UserTradingCardDataCore.checkCardCompletion(clientId, "hazuki"))
                                //    returnCompleted["hazuki"] = true;

                                //if (UserTradingCardDataCore.checkCardCompletion(clientId, "aiko"))
                                //    returnCompleted["aiko"] = true;

                                //if (UserTradingCardDataCore.checkCardCompletion(clientId, "onpu"))
                                //    returnCompleted["onpu"] = true;

                                //if (UserTradingCardDataCore.checkCardCompletion(clientId, "momoko"))
                                //    returnCompleted["momoko"] = true;

                                //if (UserTradingCardDataCore.checkCardCompletion(clientId, "other"))
                                //    returnCompleted["other"] = true;

                                //erase spawned instance if not zone card
                                if (spawnedIsZone == 0)
                                    TradingCardCore.resetSpawnInstance(guildId);

                                UserTradingCardDataCore.addCatchAttempt(clientId, spawnedToken);
                                return Tuple.Create(replyText, returnEmbedBuilder, returnLevelUp, returnCompleted);
                            }
                            else
                            {
                                //fail to catch & update catch attempt & token
                                UserTradingCardDataCore.addCatchAttempt(clientId, spawnedToken);

                                if (useBoost)
                                {   //reset boost
                                    columns = new Dictionary<string, object>();
                                    columns["boost"] = 0;
                                    string query = "";
                                    replyText = $":arrow_double_up: **{GlobalFunctions.UppercaseFirst(parent)} {GlobalFunctions.UppercaseFirst(spawnedCardCategory)}** " +
                                        $"Card Capture Boost has been used!\n";
                                    if (spawnedCardCategory.ToLower() == "special")
                                    {
                                        query = $"UPDATE {DBM_User_Trading_Card_Data.tableName} " +
                                            $" SET {DBM_User_Trading_Card_Data.Columns.boost_other_special}=@boost " +
                                            $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user} ";
                                    }   
                                    else
                                    {
                                        query = $"UPDATE {DBM_User_Trading_Card_Data.tableName} " +
                                            $" SET boost_{spawnedCardPack.ToLower()}_{spawnedCardCategory.ToLower()}=@boost " +
                                            $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user} ";
                                    }

                                    new DBC().update(query, columns);
                                }

                                if (spawnedIsMystery == 1)
                                    replyText += $":x: Card revealed correctly! " +
                                        $"But sorry, {username} **fail** to catch the mystery card. Better luck next time.";
                                else
                                    replyText += $":x: Sorry {username}, **fail** to catch the card. Better luck next time.";
                            }
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


            //fail
            returnEmbedBuilder = new EmbedBuilder()
            .WithColor(color)
            .WithDescription(replyText)
            .WithThumbnailUrl(emojiError);
            return Tuple.Create("", returnEmbedBuilder, "", returnCompleted);

        }

        public static Tuple<int, int> getCardCaptureState(string spawnedCardCategory, Boolean useBoost,
            int catchRate, string spawnedMystery,
            int maxNormal, int maxPlatinum, int maxMetal, int maxOjamajos,
            int boostNormal, int boostPlatinum, int boostMetal, int boostOjamajos,
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

            List<string> SortedList = arrList.OrderBy(o => o).ToList();

            for (int i = 0; i < SortedList.Count; i++)
            {
                string cardId = SortedList[i].ToString();
                string name = jObjTradingCardList[parent][category][cardId]["name"].ToString();
                string url = jObjTradingCardList[parent][category][cardId]["url"].ToString();
                tempVal += $"[{SortedList[i]} - {name}]({url})\n";


                if (currentIndex < 14) currentIndex++;
                else
                {
                    pageContent.Add(tempVal);
                    currentIndex = 0;
                    tempVal = title;
                }

                if (i == SortedList.Count - 1) pageContent.Add(tempVal);

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

        public static string getCardProperty(string parent, string category, string cardId, string property)
        {
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
            else if (cardId.ToLower().Contains("hanp"))
                category = "platinum";
            else if (cardId.ToLower().Contains("hanm"))
                category = "metal";
            else
                category = "normal";
            return category;
        }

        public static string getCardParent(string cardId)
        {
            string parent;
            if (cardId.ToLower().Contains("do"))
                parent = "doremi";
            else if (cardId.ToLower().Contains("han"))
                parent = "hana";
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
            else if (cardId.ToLower().Contains("po"))
                parent = "pop";
            else
                parent = "doremi";
            return parent;
        }

        public static EmbedBuilder lookZone(SocketCommandContext context, Discord.Color color)
        {
            var userId = context.User.Id;
            var userData = UserTradingCardDataCore.getUserData(userId);

            return new EmbedBuilder()
            .WithColor(color)
            .WithDescription($"{MentionUtils.MentionUser(context.User.Id)} was assigned at " +
            $"**{userData[DBM_User_Trading_Card_Data.Columns.card_zone].ToString()}** zone.");
        }

        public static EmbedBuilder assignZone(SocketCommandContext context,
        string cardPack, string category, Discord.Color color)
        {
            var userId = context.User.Id;
            
            if (category != "normal" && category != "platinum"
              && category != "metal" && category != "ojamajos")
            {
                return new EmbedBuilder()
                .WithColor(color)
                .WithDescription($":x: Sorry, That that is not the valid category. " +
                $"The available zone category are: **normal/platinum/metal/ojamajos**.");
            }

            var userData = UserDataCore.getUserData(userId);

            int selectedPrice = 40;//normal/default
            switch (category)
            {
                case "platinum":
                    selectedPrice = 60;
                    break;
                case "metal":
                    selectedPrice = 80;
                    break;
                case "ojamajos":
                    selectedPrice = 40;
                    break;
            }

            if (Convert.ToInt32(userData[DBM_User_Data.Columns.magic_seeds].ToString()) < selectedPrice)
            {
                return new EmbedBuilder()
                .WithColor(color)
                .WithDescription($":x: Sorry, you don't have: **{selectedPrice}** magic seeds to change your zone into **{cardPack} {category}**.");
            } else
            {
                DBC db = new DBC();
                string queryUpdate = @$"UPDATE {DBM_User_Trading_Card_Data.tableName}
                                        SET {DBM_User_Trading_Card_Data.Columns.card_zone}=@{DBM_User_Trading_Card_Data.Columns.card_zone}
                                        WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user}";
                Dictionary<string, object> columnFilter = new Dictionary<string, object>();
                columnFilter[DBM_User_Trading_Card_Data.Columns.card_zone] = $"{cardPack} {category}";
                columnFilter[DBM_User_Trading_Card_Data.Columns.id_user] = userId.ToString();
                db.update(queryUpdate,columnFilter);

                //update seeds data
                UserDataCore.updateMagicSeeds(userId,-selectedPrice);

                return new EmbedBuilder()
                .WithColor(color)
                .WithDescription($":white_check_mark: {MentionUtils.MentionUser(userId)} is now assigned at **{cardPack} {category}** zone and" +
                $" use {selectedPrice} magic seeds.");
            }
        }

        public static async Task generateCardSpawn(ulong guildId)
        {
            DBC dbUpdate = new DBC();
            int randomParent = new Random().Next(0, 6);
            int randomCategory = new Random().Next(11);
            int randomMystery = new Random().Next(0, 2);
            int randomBadCard = new Random().Next(0, 12);

            var tokenTime = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";

            //get card catcher notifier
            var guildTradingCardData = TradingCardGuildCore.getGuildData(guildId);
            string mentionedCardCatcherRoles = "";
            if (guildTradingCardData[DBM_Trading_Card_Guild.Columns.id_card_catcher].ToString()!="")
            {
                mentionedCardCatcherRoles = MentionUtils.MentionRole(Convert.ToUInt64(
                    guildTradingCardData[DBM_Trading_Card_Guild.Columns.id_card_catcher].ToString()));
            }

            int randomZone = new Random().Next(0, 20);
            if (randomZone <= 8)
            {
                string queryZone = $"UPDATE {DBM_Trading_Card_Guild.tableName} " +
                $" SET {DBM_Trading_Card_Guild.Columns.spawn_is_badcard}=@{DBM_Trading_Card_Guild.Columns.spawn_is_badcard}," +
                $" {DBM_Trading_Card_Guild.Columns.spawn_id}=@{DBM_Trading_Card_Guild.Columns.spawn_id}, " +
                $" {DBM_Trading_Card_Guild.Columns.spawn_parent}=@{DBM_Trading_Card_Guild.Columns.spawn_parent}, " +
                $" {DBM_Trading_Card_Guild.Columns.spawn_category}=@{DBM_Trading_Card_Guild.Columns.spawn_category}, " +
                $" {DBM_Trading_Card_Guild.Columns.spawn_token}=@{DBM_Trading_Card_Guild.Columns.spawn_token}," +
                $" {DBM_Trading_Card_Guild.Columns.spawn_badcard_question}=@{DBM_Trading_Card_Guild.Columns.spawn_badcard_question}, " +
                $" {DBM_Trading_Card_Guild.Columns.spawn_badcard_answer}=@{DBM_Trading_Card_Guild.Columns.spawn_badcard_answer}, " +
                $" {DBM_Trading_Card_Guild.Columns.spawn_is_mystery}=@{DBM_Trading_Card_Guild.Columns.spawn_is_mystery}, " +
                $" {DBM_Trading_Card_Guild.Columns.spawn_is_zone}=@{DBM_Trading_Card_Guild.Columns.spawn_is_zone}, " +
                $" {DBM_Trading_Card_Guild.Columns.spawn_time}=@{DBM_Trading_Card_Guild.Columns.spawn_time} " +
                $" WHERE {DBM_Trading_Card_Guild.Columns.id_guild}=@{DBM_Trading_Card_Guild.Columns.id_guild}";

                Dictionary<string, object> columnsZone = new Dictionary<string, object>();
                columnsZone[DBM_Trading_Card_Guild.Columns.spawn_is_badcard] = 0;
                columnsZone[DBM_Trading_Card_Guild.Columns.spawn_id] = "";
                columnsZone[DBM_Trading_Card_Guild.Columns.spawn_parent] = "";
                columnsZone[DBM_Trading_Card_Guild.Columns.spawn_category] = "";
                columnsZone[DBM_Trading_Card_Guild.Columns.spawn_token] = GlobalFunctions.RandomString(8);
                columnsZone[DBM_Trading_Card_Guild.Columns.spawn_badcard_question] = "";
                columnsZone[DBM_Trading_Card_Guild.Columns.spawn_badcard_answer] = "";
                columnsZone[DBM_Trading_Card_Guild.Columns.spawn_is_mystery] = 0;
                columnsZone[DBM_Trading_Card_Guild.Columns.spawn_is_zone] = 1;
                columnsZone[DBM_Trading_Card_Guild.Columns.spawn_time] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                columnsZone[DBM_Trading_Card_Guild.Columns.id_guild] = guildId.ToString();
                dbUpdate.update(queryZone, columnsZone);

                var embed = new EmbedBuilder()
                .WithAuthor("Zone Card")
                .WithColor(Discord.Color.DarkerGrey)
                .WithDescription($":exclamation: A **Zone** card has appeared! " +
                "Capture it with **<bot> card capture** based from your assigned zone.")
                .WithImageUrl("https://cdn.discordapp.com/attachments/709293222387777626/722780058904821760/mystery.gif")
                .WithFooter($"Capture Rate: ???%");

                await Bot.Doremi.client
                .GetGuild(guildId)
                .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guildId, "trading_card_spawn")))
                .SendMessageAsync(mentionedCardCatcherRoles,
                embed: embed.Build());

                return;
            }


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

            string queryRandom = $"SELECT * " +
                    $" FROM {DBM_Trading_Card_Data.tableName} " +
                    $" WHERE {DBM_Trading_Card_Data.Columns.pack}=@{DBM_Trading_Card_Data.Columns.pack} AND " +
                    $" {DBM_Trading_Card_Data.Columns.category}=@{DBM_Trading_Card_Data.Columns.category} " +
                    $" ORDER BY rand() " +
                    $" LIMIT 1 "; 
            string chosenId = "";
            string chosenName = ""; string chosenUrl = "";

            Dictionary<string, object> columnsRandom = new Dictionary<string, object>();

            string author = $"{GlobalFunctions.UppercaseFirst(parent)} {GlobalFunctions.UppercaseFirst(chosenCategory)} Card";
            switch (chosenCategory)
            {
                case "ojamajos":
                    queryRandom = $"SELECT * " +
                    $" FROM {DBM_Trading_Card_Data_Ojamajos.tableName} " +
                    $" WHERE {DBM_Trading_Card_Data_Ojamajos.Columns.pack} like @{DBM_Trading_Card_Data_Ojamajos.Columns.pack}" +
                    $" ORDER BY rand() " +
                    $" LIMIT 1 ";
                    columnsRandom[DBM_Trading_Card_Data_Ojamajos.Columns.pack] = $"%{parent}%";
                    break;
                case "special":
                    author = $"Other {GlobalFunctions.UppercaseFirst(chosenCategory)} Card";
                    columnsRandom[DBM_Trading_Card_Data.Columns.pack] = parent;
                    columnsRandom[DBM_Trading_Card_Data.Columns.category] = chosenCategory;
                    break;
                default:
                    columnsRandom[DBM_Trading_Card_Data.Columns.pack] = parent;
                    columnsRandom[DBM_Trading_Card_Data.Columns.category] = chosenCategory;
                    break;
            }

            string footerBadCard = "";
            //start read json
            
            //var key = JObject.Parse(jObjTradingCardList[parent][chosenCategory].ToString()).Properties().ToList();

            //chosen data:
            //chosenId = key[randIndex].Name;
            //chosenName = jObjTradingCardList[parent][chosenCategory][key[randIndex].Name]["name"].ToString();
            //chosenUrl = jObjTradingCardList[parent][chosenCategory][key[randIndex].Name]["url"].ToString();

            var result = new DBC().selectAll(queryRandom, columnsRandom);
            foreach (DataRow rows in result.Rows)
            {
                chosenId = rows[DBM_Trading_Card_Data_Ojamajos.Columns.id_card].ToString();
                chosenName = rows[DBM_Trading_Card_Data_Ojamajos.Columns.name].ToString();
                chosenUrl = rows[DBM_Trading_Card_Data_Ojamajos.Columns.url].ToString();
            }
            

            //update data
            string query = $"UPDATE {DBM_Trading_Card_Guild.tableName} " +
                $" SET {DBM_Trading_Card_Guild.Columns.spawn_is_badcard}=@{DBM_Trading_Card_Guild.Columns.spawn_is_badcard}," +
                $" {DBM_Trading_Card_Guild.Columns.spawn_id}=@{DBM_Trading_Card_Guild.Columns.spawn_id}, " +
                $" {DBM_Trading_Card_Guild.Columns.spawn_parent}=@{DBM_Trading_Card_Guild.Columns.spawn_parent}, " +
                $" {DBM_Trading_Card_Guild.Columns.spawn_category}=@{DBM_Trading_Card_Guild.Columns.spawn_category}, " +
                $" {DBM_Trading_Card_Guild.Columns.spawn_token}=@{DBM_Trading_Card_Guild.Columns.spawn_token}," +
                $" {DBM_Trading_Card_Guild.Columns.spawn_badcard_question}=@{DBM_Trading_Card_Guild.Columns.spawn_badcard_question}, " +
                $" {DBM_Trading_Card_Guild.Columns.spawn_badcard_answer}=@{DBM_Trading_Card_Guild.Columns.spawn_badcard_answer}, " +
                $" {DBM_Trading_Card_Guild.Columns.spawn_is_mystery}=@{DBM_Trading_Card_Guild.Columns.spawn_is_mystery}, " +
                $" {DBM_Trading_Card_Guild.Columns.spawn_is_zone}=@{DBM_Trading_Card_Guild.Columns.spawn_is_zone}, " +
                $" {DBM_Trading_Card_Guild.Columns.spawn_time}=@{DBM_Trading_Card_Guild.Columns.spawn_time} " +
                $" WHERE {DBM_Trading_Card_Guild.Columns.id_guild}=@{DBM_Trading_Card_Guild.Columns.id_guild}";

            Dictionary<string, object> columns = new Dictionary<string, object>();
            columns[DBM_Trading_Card_Guild.Columns.spawn_is_badcard] = 0;
            columns[DBM_Trading_Card_Guild.Columns.spawn_id] = chosenId;
            columns[DBM_Trading_Card_Guild.Columns.spawn_parent] = parent;
            columns[DBM_Trading_Card_Guild.Columns.spawn_category] = chosenCategory;
            columns[DBM_Trading_Card_Guild.Columns.spawn_token] = GlobalFunctions.RandomString(8);
            columns[DBM_Trading_Card_Guild.Columns.spawn_badcard_question] = "";
            columns[DBM_Trading_Card_Guild.Columns.spawn_badcard_answer] = "";
            columns[DBM_Trading_Card_Guild.Columns.spawn_is_mystery] = 0;
            columns[DBM_Trading_Card_Guild.Columns.spawn_is_zone] = 0;
            columns[DBM_Trading_Card_Guild.Columns.spawn_time] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            columns[DBM_Trading_Card_Guild.Columns.id_guild] = guildId.ToString();

            dbUpdate.update(query, columns);

            //reset default & assign all
            Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyId, chosenId);
            Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyCategory, chosenCategory);
            Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyToken, GlobalFunctions.RandomString(8));
            Config.Guild.setPropertyValue(guildId, TradingCardCore.CardEvent.propertyToken, GlobalFunctions.RandomString(8));//for event only
            Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyMystery, "0");
            Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCard, "");
            Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardEquation, "");
            Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardNumber1, "");
            Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardNumber2, "");
            Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyTokenTime, tokenTime);
            Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyIsZone, "0");

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

                string question = ""; string answer = "";

                Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardNumber1, randomNumber1.ToString());
                Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardNumber2, randomNumber2.ToString());

                if (randomEquation == 0)
                {
                    Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardEquation, "+");
                    question = $"{randomNumber1}+{randomNumber2}";
                    answer = (randomNumber1 + randomNumber2).ToString();
                }
                else
                {
                    Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardEquation, "-");
                    question = $"{randomNumber1}-{randomNumber2}";
                    answer = (randomNumber1 - randomNumber2).ToString();
                }
                
                footerBadCard += $"-{TradingCardCore.BadCards.getType(randomBadCardType)}";

                //update
                query = $"UPDATE {DBM_Trading_Card_Guild.tableName} " +
                $" SET {DBM_Trading_Card_Guild.Columns.spawn_is_badcard}=@{DBM_Trading_Card_Guild.Columns.spawn_is_badcard}," + 
                $" {DBM_Trading_Card_Guild.Columns.spawn_token}=@{DBM_Trading_Card_Guild.Columns.spawn_token}," +
                $" {DBM_Trading_Card_Guild.Columns.spawn_badcard_question}=@{DBM_Trading_Card_Guild.Columns.spawn_badcard_question}, " +
                $" {DBM_Trading_Card_Guild.Columns.spawn_badcard_type}=@{DBM_Trading_Card_Guild.Columns.spawn_badcard_type}, " +
                $" {DBM_Trading_Card_Guild.Columns.spawn_badcard_answer}=@{DBM_Trading_Card_Guild.Columns.spawn_badcard_answer} " +
                $" WHERE {DBM_Trading_Card_Guild.Columns.id_guild}=@{DBM_Trading_Card_Guild.Columns.id_guild}";

                columns = new Dictionary<string, object>();
                columns[DBM_Trading_Card_Guild.Columns.spawn_is_badcard] =1;
                columns[DBM_Trading_Card_Guild.Columns.spawn_token] = GlobalFunctions.RandomString(8);
                columns[DBM_Trading_Card_Guild.Columns.spawn_badcard_question] = question;
                columns[DBM_Trading_Card_Guild.Columns.spawn_badcard_answer] = answer;
                columns[DBM_Trading_Card_Guild.Columns.spawn_badcard_type] = randomBadCardType;
                columns[DBM_Trading_Card_Guild.Columns.id_guild] = guildId.ToString();

                dbUpdate.update(query, columns);

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
                .SendMessageAsync($":exclamation:{mentionedCardCatcherRoles} A **{chosenCategory}** {parent} card has appeared! " +
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
                .WithImageUrl("https://cdn.discordapp.com/attachments/709293222387777626/722780058904821760/mystery.gif")
                .WithThumbnailUrl(badCardIcon)
                .WithFooter($"ID: ???{footerBadCard} | Catch Rate: {catchRate}");

                Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyMystery, "1");
                await client
                .GetGuild(guildId)
                .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guildId, "trading_card_spawn")))
                .SendMessageAsync($":question: {mentionedCardCatcherRoles} A ||**mystery**|| card has appeared! Can you guess whose card is this belongs to?\n" +
                $"Reveal & capture it with **<bot>!card capture** or **<bot>!card capture boost**",
                embed: embed.Build());

                //start updating
                query = $"UPDATE {DBM_Trading_Card_Guild.tableName} " +
                $" SET {DBM_Trading_Card_Guild.Columns.spawn_is_mystery}=@{DBM_Trading_Card_Guild.Columns.spawn_is_mystery} " +
                $" WHERE {DBM_Trading_Card_Guild.Columns.id_guild}=@{DBM_Trading_Card_Guild.Columns.id_guild}";

                columns = new Dictionary<string, object>();
                columns[DBM_Trading_Card_Guild.Columns.spawn_is_mystery] = 1;
                columns[DBM_Trading_Card_Guild.Columns.id_guild] = guildId;
                dbUpdate.update(query, columns);

            }
        }

        public static Tuple<string, EmbedBuilder> generateCardSpawnIndividual(ulong guildId, ulong clientId)
        {
            int randomParent = new Random().Next(0, 6);
            int randomCategory = new Random().Next(11);
            int randomMystery = new Random().Next(0, 2);
            int randomBadCard = new Random().Next(0, 12);
            int randomizedSeedsAmount = new Random().Next(10,30);

            var tokenTime = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
            var lastCapturedTime = Config.Guild.getPropertyValue(guildId, TradingCardCore.propertyTokenTime);
            var minCaptureTime = Convert.ToInt32(Config.Guild.getPropertyValue(guildId, "trading_card_spawn_interval"));

            var CardCatcherRoles = Config.Guild.getPropertyValue(guildId, "id_card_catcher");
            string mentionedCardCatcherRoles = "";
            if (CardCatcherRoles != "")
            {
                mentionedCardCatcherRoles = MentionUtils.MentionRole(Convert.ToUInt64(CardCatcherRoles));
            }

            if (lastCapturedTime != "")
            {
                TimeSpan diff = DateTime.Now - DateTime.Parse(lastCapturedTime);
                var minutes = Math.Round(diff.TotalMinutes);
                //10:22 - 10:20
                if (minutes < minCaptureTime)
                {
                    var finalTotal = minCaptureTime - minutes;
                    string finalSpawnText = $"{finalTotal} more minute(s)";
                    if (finalTotal<=0)
                    {
                        finalSpawnText = "less than 1 minute(s)";
                    }

                    return new Tuple<string, EmbedBuilder>($":x: Sorry, the card spawner command available " +
                    $"approximately at **{finalSpawnText}.**", null);

                }

            }

            int randomZone = new Random().Next(0, 20);
            if (randomZone <= 8)
            {
                //reset default & assign all
                Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyId, "");
                Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyCategory, "");
                Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyToken, GlobalFunctions.RandomString(8));
                Config.Guild.setPropertyValue(guildId, TradingCardCore.CardEvent.propertyToken, GlobalFunctions.RandomString(8));//for event only
                Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyMystery, "0");
                Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCard, "");
                Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardEquation, "");
                Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardNumber1, "");
                Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardNumber2, "");
                Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyTokenTime, tokenTime);
                Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyIsZone, "1");

                var embed = new EmbedBuilder()
                .WithAuthor("Zone Card")
                .WithColor(Discord.Color.DarkerGrey)
                .WithDescription($":exclamation: A **zone** card has appeared! " +
                "Capture it with **<bot> card capture** or **<bot> card capture boost** based from your assigned zone.")
                .WithImageUrl("https://cdn.discordapp.com/attachments/709293222387777626/722780058904821760/mystery.gif")
                .WithFooter($"Capture Rate: ???%");

                return new Tuple<string, EmbedBuilder>($":exclamation:{MentionUtils.MentionUser(clientId)} received {randomizedSeedsAmount} magic seeds for spawning the card.\n" +
                    $"{mentionedCardCatcherRoles}",
                embed);
            }

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
            for (int i = 0; i <= timedLoop; i++)
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
            Config.Guild.setPropertyValue(guildId, TradingCardCore.CardEvent.propertyToken, GlobalFunctions.RandomString(8));//for event only
            Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyMystery, "0");
            Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCard, "");
            Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardEquation, "");
            Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardNumber1, "");
            Config.Guild.setPropertyValue(guildId, TradingCardCore.BadCards.propertyBadCardNumber2, "");
            Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyTokenTime, tokenTime);
            Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyIsZone, "0");

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

            //process magic seeds bonus for user
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
            JObject arrInventory = JObject.Parse(File.ReadAllText(playerDataDirectory));
            arrInventory["magic_seeds"] = Convert.ToInt32(arrInventory["magic_seeds"]) + randomizedSeedsAmount;
            File.WriteAllText(playerDataDirectory, arrInventory.ToString());

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

                return new Tuple<string, EmbedBuilder>($":exclamation:{MentionUtils.MentionUser(clientId)} received {randomizedSeedsAmount} magic seeds for spawning the card.\n" +
                    $"{mentionedCardCatcherRoles}" +
                    $"A **{chosenCategory}** {parent} card has appeared! " +
                $"Capture it with **<bot>!card capture** or **<bot>!card capture boost**",
                embed);
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
                
                return new Tuple<string, EmbedBuilder>($":question:{MentionUtils.MentionUser(clientId)} received {randomizedSeedsAmount} magic seeds for spawning the card.\n" +
                    $"{mentionedCardCatcherRoles}" +
                $"A ||**mystery**|| card has appeared! Can you guess whose card is this belongs to?\n" +
                $"Reveal & capture it with **<bot>!card capture** or **<bot>!card capture boost**",
                embed);
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

            public static string[] cardCategoryList = { 
                "doremi normal",
                "doremi platinum",
                "doremi metal",
                "doremi ojamajos"
            };

            public static string[] arrMysteryDescription = {
                //19,20,5,1,11
                //a=1, b=4, c=7
                //d/h/a/o/m
                //doremi/hazuki/aiko/onpu/momoko
                //":sparkles: **Pirika** is one of my chanting spell",
                //":sparkles: **Pirilala** is one of my chanting spell",
                //":sparkles: **Poporina** is one of my chanting spell",
                //":sparkles: **Peperuto** is one of my chanting spell",

                ":birthday: My birthday was on July",
                ":woman_fairy: Translate these numbers into words: 4-15-4-15",//dodo
                ":woman_fairy: Translate these numbers into words and rearrange the result: 15-4-15-4",
                ":birthday: February, May, March and November are not my birthday",
                ":birthday: My birthday date was on 30",
                
                ":sparkles: **Paipai Poppun Famifami Pon!** are not my spell",
                ":sparkles: **Faa Puwapuwa Pon Rarirori!** are not my spell",
                ":sparkles: **Puu Raruku Purun Perutan!** are not my spell",
                ":sparkles: **Puu Poppun Faa Pon!** are not my spell",
                ":sparkles: **Petton Puu Pameruku Faa!** are not my spell",
                ":sparkles: **Famifami Rarirori Paipai Petton!** are not my spell",
                //new:
                ":girl: Translate these numbers into words: 4-15-18-5-13-9",//doremi
                ":girl: Translate these numbers into words and rearrange the result: 5-18-15-4-9-13",//doremi
                ":girl: Translate these numbers into words and rearrange the result: 18-5-9-13-15-4",//doremi
                ":girl: Translate these numbers into words and rearrange the result: 15-4-13-9-18-5",//doremi
                ":girl: Translate these numbers into words and rearrange the result: 9-13-4-15-18-5",//doremi
                ":girl: Translate these numbers into words and rearrange the result: 9-18-5-15-13-4",//doremi

                ":girl: Translate these numbers into words: 8-1-18-21-11-1-26-5",//harukaze
                ":girl: Translate these numbers into words and rearrange the result: 1-1-26-5-11-8-18-21",//harukaze
                ":girl: Translate these numbers into words and rearrange the result: 5-1-21-1-8-11-18-26",//harukaze

                ":fork_and_knife: Translate these numbers into words: 19-20-5-1-11",//steak
                ":fork_and_knife: Translate these numbers into words and rearrange the result: 20-19-11-5-1",//steak
                ":fork_and_knife: Translate these numbers into words and rearrange the result: 1-11-19-5-20"//steak
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

            public static string[] cardCategoryList = {
                "hazuki normal",
                "hazuki platinum",
                "hazuki metal",
                "hazuki ojamajos"
            };

            public static string[] arrMysteryDescription = {
                //
                //":fork_and_knife: One of my favorite food ends with **e**",
                //":fork_and_knife: One of my favorite food start with **ch**",
                //":sparkles: **Paipai** is one of my chanting spell",
                //":sparkles: **Ponpoi** is one of my chanting spell",
                //":sparkles: **Puwapuwa** is one of my chanting spell",
                //":sparkles: **Puu** is one of my chanting spell",

                ":drop_of_blood: My blood type is A",
                ":birthday: My birthday was on February",
                ":woman_fairy: Translate these numbers into words: 18-5-18-5",//rere
                ":woman_fairy: Translate these numbers into words and rearrange the result: 5-18-18-5",//rere
                ":woman_fairy: Translate these numbers into words and rearrange the result: 5-5-18-18",//rere
                ":birthday: May, July, March and November are not my birthday month",
                ":birthday: My birthday is the same day of the month as Aiko but I was born a few months earlier",
                ":sparkles: **Raruku Famifami Pirika Pon!** are not my spell",
                ":sparkles: **Pararira Faa Rarirori Poporina!** are not my spell",
                ":sparkles: **Poppun Pirika Faa Perutan!** are not my spell",
                ":sparkles: **Rarirori Peperuto Perutan Purun!** are not my spell",
                ":girl: Translate these numbers into words: 8-1-26-21-11-9",//hazuki
                ":girl: Translate these numbers into words and rearrange the result: 26-9-8-21-1-11",//hazuki
                ":girl: Translate these numbers into words and rearrange the result: 11-21-8-1-26-9",//hazuki
                ":girl: Translate these numbers into words and rearrange the result: 9-21-8-1-26-11",//hazuki
                
                ":girl: Translate these numbers into words: 6-21-10-9-23-1-18-1", //fujiwara
                ":girl: Translate these numbers into words and rearrange the result: 1-23-6-18-21-1-9-10", //fujiwara
                ":girl: Translate these numbers into words and rearrange the result: 1-10-9-21-19-1-6-23", //fujiwara
                ":girl: Translate these numbers into words and rearrange the result: 10-9-21-1-6-18-23-1", //fujiwara
                ":girl: Translate these numbers into words and rearrange the result: 23-1-18-1-10-21-6-9", //fujiwara
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

            public static string[] cardCategoryList = {
                "aiko normal",
                "aiko platinum",
                "aiko metal",
                "aiko ojamajos"
            };

            public static string[] arrMysteryDescription = {
                //":sparkles: **Pameruku** is one of my chanting spell",
                //":sparkles: **Raruku** is one of my chanting spell",
                //":sparkles: **Rarirori** is one of my chanting spell",
                //":sparkles: **Poppun** is one of my chanting spell",

                ":birthday: My birthday was on November",
                ":woman_fairy: Translate these numbers into words: 13-9-13-9",//mimi
                ":woman_fairy: Translate these numbers into words: 13-13-9-9",//mimi
                ":fork_and_knife: Translate these numbers into words: 20-1-11-15-25-1-11-9",//takoyaki
                ":fork_and_knife: Translate these numbers into words and rearrange the result: 1-11-11-9-20-1-25-15",//takoyaki
                ":fork_and_knife: Translate these numbers into words and rearrange the result: 15-11-9-20-11-1-25-1",//takoyaki
                ":fork_and_knife: Translate these numbers into words and rearrange the result: 9-1-15-25-11-1-20-11",//takoyaki
                ":birthday: July, February, March and May are not my birthday",
                ":birthday: My birthday is the same day of the month as Hazuki but I was born a few months older",
                ":drop_of_blood: My blood type is O",
                ":fork_and_knife: One of my favorite food ends with **i**",
                ":fork_and_knife: One of my favorite food start with **t**",
                ":sparkles: **Famifami Pon Ponpoi Pirika!** are not my spell",
                ":sparkles: **Peperuto Puwapuwa Purun Perutan!** are not my spell",
                ":sparkles: **Ponpoi Purun Pirilala Petton!** are not my spell",
                ":sparkles: **Famifami Pararira Puwapuwa Poporina!** are not my spell",
                ":sparkles: **Pururun Paipai Perutan Pirika!** are not my spell",
                ":sparkles: **Puu Pon Faa Peperuto!** are not my spell",
                ":girl: Translate these numbers into words: 1-9-11-15",//aiko
                ":girl: Translate these numbers into words and rearrange the result: 15-11-9-1",//aiko
                ":girl: Translate these numbers into words and rearrange the result: 11-15-1-9",//aiko
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

            public static string[] cardCategoryList = {
                "onpu normal",
                "onpu platinum",
                "onpu metal",
                "onpu ojamajos"
            };

            public static string[] arrMysteryDescription = {
                //":sparkles: **Pururun** is one of my chanting spell",
                //":sparkles: **Purun** is one of my chanting spell",
                //":sparkles: **Famifami** is one of my chanting spell",
                //":sparkles: **Faa** is one of my chanting spell",
                //":fork_and_knife: One of my favorite food ends with **s**",
                //":fork_and_knife: One of my favorite food start with **cr**",

                ":birthday: My birthday was on March",
                ":woman_fairy: Translate these numbers into words: 18-15-18-15",//roro
                ":woman_fairy: Translate these numbers into words: 18-18-15-15",//roro
                ":birthday: July, February, November and May are not my birthday",
                ":birthday: My birthday date was on 3",
                ":sparkles: **Rarirori Pirika Ponpoi Pon!** are not my spell",
                ":sparkles: **Puwapuwa Peperuto Raruku Perutan!** are not my spell",
                ":sparkles: **Ponpoi Raruku Petton Pirilala!** are not my spell",
                ":sparkles: **Poporina Puwapuwa Rarirori Pararira!** are not my spell",
                ":sparkles: **Peperuto Pon Poppun Puu!** are not my spell",
                ":sparkles: **Paipai Pirika Pameruku Perutan!** are not my spell",
                ":girl: Translate these numbers into words: 15-14-16-21", //onpu
                ":girl: Translate these numbers into words and rearrange the result: 16-21-15-14", //onpu
                ":girl: Translate these numbers into words and rearrange the result: 21-15-14-16", //onpu
                
                ":girl: Translate these numbers into words: 19-5-7-1-23-1", //segawa
                ":girl: Translate these numbers into words and rearrange the result: 1-7-23-5-1-19", //segawa
                ":girl: Translate these numbers into words and rearrange the result: 1-1-5-23-19-7", //segawa
                ":girl: Translate these numbers into words and rearrange the result: 5-1-1-23-19-7", //segawa
            };

        }

        public class Momoko
        {
            public static int maxNormal = 42; public static int maxPlatinum = 6; public static int maxMetal = 4;
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

            public static string[] cardCategoryList = {
                "momoko normal",
                "momoko platinum",
                "momoko metal",
                "momoko ojamajos"
            };

            public static string[] arrMysteryDescription = {
                //":fork_and_knife: One of my favorite food ends with **t**",
                //":sparkles: **Perutan** is one of my chanting spell",
                //":sparkles: **Petton** is one of my chanting spell",
                //":sparkles: **Pararira** is one of my chanting spell",
                //":sparkles: **Pon** is one of my chanting spell",

                ":birthday: My birthday was on May",
                ":woman_fairy: Translate these numbers into words: 14-9-14-9",//nini
                ":drop_of_blood: My blood type is AB",
                ":birthday: July, February, November and March are not my birthday",
                ":birthday: My birthday date was on 6",
                ":sparkles: **Ponpoi Pirika Faa Rarirori!** are not my spell",
                ":sparkles: **Raruku Puwapuwa Peperuto Pururun!** are not my spell",
                ":sparkles: **Purun Ponpoi Pirilala Raruku!** are not my spell",
                ":sparkles: **Rarirori Poporina Famifami Puwapuwa!** are not my spell",
                ":sparkles: **Faa Poppun Puu Peperuto!** are not my spell",
                ":sparkles: **Pururun Pameruku Pirika Paipai!** are not my spell",

                ":girl: Translate these numbers into words: 13-15-13-15-11-15",//momoko
                ":girl: Translate these numbers into words and rearrange the result: 15-15-13-13-11-15",//momoko
                ":girl: Translate these numbers into words and rearrange the result: 11-13-15-13-15-15"//momoko
            };

        }

        public class Pop
        {
            public static int maxNormal = 8;
            public static Discord.Color embedColor = new Discord.Color(234, 104, 140);
            public static string roleCompletionist = "Pop Card Badge";
            public static string imgCompleteAllCard = $"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/badge/badge_pop.png";

        }

        public class Hana
        {
            public static int maxNormal = 21; public static int maxPlatinum = 8; public static int maxMetal = 6;
            public static Discord.Color embedColor = new Discord.Color(253, 254, 255);
            public static string roleCompletionist = "Hana Card Badge";
            public static string imgCompleteAllCard = $"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/badge/badge_hana.png";

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

        public class CardEvent
        {
            public static string embedShopName = "Majo Rika";
            public static string embedShopImg = "https://media.discordapp.net/attachments/706770454697738300/746766700363645038/PuyoQueRika_2.png";
            public static Discord.Color embedColor = new Discord.Color(131,173,96);

            public static string propertyToken = "event_token";

            public static string[] partList = {
                "pink part A","pink part I","pink part U",
                "pink part E","pink part O","white part A",
                "white part I","white part U","white part E",
                "white part O"
            };

            public static string[] royalPart = {
                "royal pink part","royal white part"
            };

            //pop
            public static string[] partListPop = {
                "pink part A","pink part I","pink part U",
                "pink part E","pink part O"
            };

            public static string royalPartPop = "royal pink part";

            //hana
            public static string[] partListHana = {
                "white part A","white part I","white part U",
                "white part E","white part O"
            };
            public static string royalPartHana = "royal white part";



            public static PaginatedMessage printInventoryTemplate(Color color, JObject arrInventory, string username,
                string thumbnailUrl)
            {
                PaginatedAppearanceOptions pao = new PaginatedAppearanceOptions();
                pao.JumpDisplayOptions = JumpDisplayOptions.Never;
                pao.DisplayInformationIcon = false;

                List<string> pageContent = new List<string>();

                try
                {
                    JArray array = JArray.Parse(arrInventory["event_inventory"].ToString());
                    //JArray sorted = new JArray(array.OrderBy(obj => (string)obj["event_inventory"]));
                    JArray sorted = new JArray(array.OrderBy(e=>(string)e.ToString()));

                    string title = $"";

                    string tempVal = title;
                    int currentIndex = 0;
                    for (int i = 0; i < sorted.Count; i++)
                    {

                        tempVal += $"-{sorted[i]}\n";

                        if (i == sorted.Count - 1)
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
                    //Console.WriteLine(e.ToString());
                }

                var pager = new PaginatedMessage
                {
                    Title = $"**Card Part Inventory**:\n",
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

            public static EmbedBuilder printEmptyInventoryTemplate(Color color, string username)
            {
                return new EmbedBuilder()
                    .WithColor(color)
                    .WithTitle($"**{username}'s Card Part Inventory**")
                    .WithDescription($":x: There are no card part that you have collected.");
            }

        }

        public class CardShop
        {

        }

    }
}
