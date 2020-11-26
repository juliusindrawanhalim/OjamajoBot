using Discord;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OjamajoBot {
    public static class PatissierCore
    {
        public static string headCoreConfigFolder = "core/patissier/";
        public static string headUserConfigFolder = "patissier";

        public static string version = "1.00";
        public static string propertyShop = "pattisier_shop";

        public static Color embedShopColor = new Color(2,19,43);

        //parameter:ingredients name,price_min,price_max
        public static string[,] shopList = {
            { "flour","50","100" },
            { "sugar","10","100" },
            { "butter","80","150" },
            { "cream","50","100" },
            { "egg","20","100" },
            { "milk","50","150" }
        };

        public static EmbedBuilder printStatusTemplate(Color color, string username, string guildId, string clientId, string emojiError,
            string thumbnailUrl)
        {
            string playerGardenDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{GardenCore.headUserConfigFolder}/{clientId}.json";
            if (!File.Exists(playerGardenDataDirectory)) //not registered yet
                GardenCore.createGardenUserData(playerGardenDataDirectory);

            if (!File.Exists(playerGardenDataDirectory))
            { //not registered yet
                return new EmbedBuilder()
                .WithColor(color)
                .WithDescription($"")
                .WithThumbnailUrl(emojiError);
            }

            var playerData = JObject.Parse(File.ReadAllText(playerGardenDataDirectory));
            var arrListIngredients = playerData["ingredients"];
            var playerExp = Convert.ToInt32(playerData["exp"].ToString());

            //int totalSuccess = ((JArray)arrListData["normal"]).Count;

            //string doremiText = $"**Scones: {((JArray)arrListDoremi["normal"]).Count}/{Doremi.maxNormal}**\n";
            //int totalSuccessPack = ((JArray)arrListDoremi["normal"]).Count;
            //int totalMax = Doremi.maxNormal;
            //double calculated = (double)totalSuccessPack / totalMax * 100;
            //string doremiPercentage = $"({Math.Round(calculated)}%)";

            string recipeListText = "";

            return new EmbedBuilder()
                .WithTitle($"{username} Patissier Level: ")
                .WithColor(color)
                .WithThumbnailUrl(thumbnailUrl)
                .AddField("EXP:", $"**{playerData["exp"].ToString()}**", false)
                .AddField($"Recipe Level:", recipeListText, true)
                .WithFooter($"Seeds: Magic: {playerData["magic_seeds"]} / Royal: {playerData["royal_seeds"]}");
        }

        public static async Task generateShopSpawn(ulong guildId)
        {
            //generate first item
            string textItemList = "";
            int randomQuality = new Random().Next(1, 6);
            textItemList += $"{shopList[0,0]}: {shopList[0, 2]} seeds";

            string thumbnailUrl = "https://cdn.discordapp.com/attachments/777809375624167424/777809414131286026/4e0bd81cd1ea8649d2817e79c74dfc83.png";

            for (int i = 1; i < shopList.Length; i++)
            {
                int randomAppear = new Random().Next(0, 2);
                randomQuality = new Random().Next(1, 6);
                //.AddField("a","",true)
                if (randomAppear <= 0)
                {
                    textItemList += $"{shopList[i, 0]}: {shopList[i, 2]} seeds";
                    //int randomPrice = new Random().Next(shopList[0, 2], shopList[0, 2]);
                }
            }

            await Bot.Momoko.client
                .GetGuild(guildId)
                .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guildId, "trading_card_spawn")))
                .SendMessageAsync(embed: new EmbedBuilder()
                .WithAuthor("Dela Patissiere Shop", "https://cdn.myanimelist.net/images/characters/15/274587.jpg")
                .WithColor(embedShopColor)
                .WithDescription("Dela's here to sell some finest ingredients that you need!")
                .WithThumbnailUrl(thumbnailUrl)
                .AddField($"Ingredients in sell:", textItemList, true)
                .Build());
        }

        public class RecipeDiary
        {
            public class Ingredients
            {

            }
        }
    }
}
