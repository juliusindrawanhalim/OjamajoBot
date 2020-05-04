using Discord;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace OjamajoBot
{
    public class TradingCardCore
    {
        public static List<string> printInventoryTemplate(string pack,string parent, string category,
            JObject jObjTradingCardList, JArray arrData,int maxAmount)
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

        public static EmbedBuilder printCardDetailTemplate(Color color, string name, string imgUrl,string card_id,
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



    }
}
