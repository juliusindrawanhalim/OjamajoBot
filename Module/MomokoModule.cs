using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using OjamajoBot.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OjamajoBot.Module
{
    class MomokoModule : ModuleBase<SocketCommandContext>
    {
        [Command("Help")]
        public async Task showHelp()
        {
            await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Momoko.EmbedColor)
                .WithAuthor(Config.Momoko.EmbedName, Config.Momoko.EmbedAvatarUrl)
                .WithTitle("Command List:")
                .WithDescription("Pretty Witchy Momoko Chi~ " +
                $"You can tell me what to do with {MentionUtils.MentionUser(Config.Momoko.Id)} or **momoko!** or **mo!** as the starting command prefix.")
                .AddField("Basic Commands",
                "**cakes** or **cake** : I will give you some random cakes\n" +
                "**change** or **henshin** : Change into the ojamajo form\n" +
                "**fairy** : I will show you my fairy\n" +
                "**hello** : Hello, I will greet you up\n" +
                "**random** or **moments** : Show any random Momoko moments\n" +
                "**shocked** or **omg** : *gasp*\n" +
                "**stats** or **bio** : I will show you my biography info\n" +
                "**traditional** or **traditionify <sentences>** : It's <sentences> traditional!\n" +
                "**transform <username> <wishes>** : Transform <username> into <wishes>\n" +
                "**wish <wishes>** : I will grant you a <wishes>")
                .Build());
        }

        [Command("hello")]
        public async Task momokoHello()
        {
            string tempReply = "";
            List<string> listRandomRespond = new List<string>() {
                    $"Hii hii {MentionUtils.MentionUser(Context.User.Id)}! ",
                    $"Hello {MentionUtils.MentionUser(Context.User.Id)}! ",
            };

            int rndIndex = new Random().Next(0, listRandomRespond.Count);
            tempReply = listRandomRespond[rndIndex] + Config.Momoko.arrRandomActivity[Config.Momoko.indexCurrentActivity, 1];

            await ReplyAsync(tempReply);
        }

        [Command("thank you"), Alias("thank you,","thanks", "arigatou")]
        public async Task thankYou([Remainder] string query = "")
        {
            await ReplyAsync($"Your welcome, {MentionUtils.MentionUser(Context.User.Id)}. I'm glad that you're happy with it :smile:");
        }

        [Command("change"), Alias("henshin")]
        public async Task transform()
        {
            await ReplyAsync("Pretty Witchy Momoko Chi~\n");
            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Momoko.EmbedColor)
                .WithImageUrl("https://66.media.tumblr.com/e163487a3d9b92ec627a838499749011/tumblr_p6ksbbhwau1x776xto10_250.gif")
                .Build());
        }

        [Command("turn"), Alias("transform")]
        public async Task spells(IUser iuser, [Remainder] string query)
        {
            //await Context.Message.DeleteAsync();
            await ReplyAsync($"Peruton Peton Pararira Pon! Turn {iuser.Mention} into {query}",
            embed: new EmbedBuilder()
            .WithColor(Config.Hazuki.EmbedColor)
            .WithImageUrl("https://i.ytimg.com/vi/iOkN602s-JQ/hqdefault.jpg")
            .Build());
        }

        [Command("stats"), Alias("bio")]
        public async Task showStats()
        {
            await ReplyAsync("Peruton Peton Pararira Pon! Show my biography info!",
            embed: new EmbedBuilder()
            .WithAuthor(Config.Momoko.EmbedName, Config.Momoko.EmbedAvatarUrl)
            .WithDescription("Momoko Asuka (飛鳥ももこ, Asuka Momoko) is the sixth main character and the secondary tritagonist of Ojamajo Doremi, who became part of the group at the start of Motto. " +
            "She was called in by the Witch Queen to help the girls run their brand new Sweet Shop Maho-do.")
            .AddField("Full Name", "飛鳥ももこ Asuka Momoko", true)
            .AddField("Gender", "female", true)
            .AddField("Blood Type", "AB", true)
            .AddField("Birthday", "May 6th, 1990", true)
            .AddField("Instrument", "Guitar", true)
            .AddField("Favorite Food", "Madeleines, Strawberry Tart", true)
            .AddField("Debut", "[Doremi, a Stormy New Semester](https://ojamajowitchling.fandom.com/wiki/Doremi,_a_Stormy_New_Semester)", true)
            .WithColor(Config.Momoko.EmbedColor)
            .WithImageUrl("https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcRBfCfThqVYdJWQzWJOvILjx-Acf-DgRQidfN1s11-fxc0ShEe3")
            .WithFooter("Source: [Ojamajo Witchling Wiki](https://ojamajowitchling.fandom.com/wiki/Momoko_Asuka)")
            .Build());
        }

        [Command("fairy")]
        public async Task showFairy()
        {
            await ReplyAsync("This is my fairy, Nini.",
            embed: new EmbedBuilder()
            .WithAuthor(Config.Momoko.EmbedName, Config.Momoko.EmbedAvatarUrl)
            .WithDescription("Nini is fair skinned with peach blushed cheeks and rounded green eyes. Her light chartreuse hair resembles Momoko's, and on the corner of her head is a lilac star clip. She wears a chartreuse dress with creamy yellow collar." +
            "In teen form the only change to her hair is that her bangs are spread out.She gains a developed body and now wears a pastel gold dress with the shoulder cut out and a white collar, where a gold gem rests.A gold top is worn under this, and she gains white booties and a white witch hat with pastel yellow rim.")
            .WithColor(Config.Momoko.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e4/No.080.jpg/revision/latest?cb=20190701055501")
            .WithFooter("[Ojamajo Witchling Wiki](https://ojamajowitchling.fandom.com/wiki/Nini)")
            .Build());
        }

        [Command("cakes")]
        public async Task randomCakes()
        {
            string[] arrRandom = {
                "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcQ1E2tmL__kxM5XIx1wmizO4QbE3Zzt0_g2b4v_oaousanqUZgo",
                "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcRWF-HoRS7kFgTk6-mSod_y1nu4Tq0JNSe6Q1Q9DqrDZA-y2tVZ",
                "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcRYBmmxUecOW9tKOs3HQidVATOP6S8Vr3mprVsw8Ea-HFwowKaP",
                "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcR2EkIj1vyIbpeM-is0qxSJYBJm8veszuXhjNAuGCKkqvqVFRlA",
                "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcSNR55AU3zE_UYTEMzthq0scTWejoHTvb4LwcxFtuI5ecWPH7qm",
                "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcQxQTG69VNOaqQCv9EHOis24caAtOKh_v6CBwdSJtNQNZS7AvMX",
                "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcSBZYMYBex7AJWkPQsmuiKPZ_BgLXzIFKgs8ESGKKZw0L1E22lg",
                "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcQgAMj0JbDCKV7GP3MuhyEQAhL7PmAvmFQ72gcKta3fb9Bk7szq",
                "https://edgarsbakery.com/wp-content/uploads/2019/10/Green-Monster.jpg",
                "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcT540rH3iIfmOBnQ4NTqGXAu5ryFA4WMecYDtRK0llH-bQU5mtg",
                "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcQRIFwPr5PkYIEIRJFwRUPJ6SOPNg44AyWbJtzfZeUT_Mn4yTcX",
                "https://i.pinimg.com/originals/51/9e/a6/519ea6b5957ee995b0251ef6b0aa0832.jpg",
                "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcTyAwfheEeKAiwl8tn8LuDkOlb_5GPwf5XRNYtwILHIplk66Rha",
                "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcRerdGCQTZM_mgO50N8kAyEVOEgYxcrLZeh0BY74_WwS4wPv0QV",
                "https://i.pinimg.com/originals/f0/07/49/f00749eea0f1134be96ad105d3084461.jpg",
                "https://i.pinimg.com/originals/c9/52/41/c95241df350a1b7e2a5cec192f3f09e3.jpg",
                "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcQM02aJuuUCoxYOSYSbp0AIChfAT_hLUK94L1f5vQrhtd7zelI3",
                "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcT4vuv8DAoob_O7gWZa0CSNh0BBbdY76fUGCS8mXfVbmC98DeTy",
                "https://i.pinimg.com/originals/6a/4c/d8/6a4cd8446298f6e85b0e17007dd6cbed.jpg"
            };
        }

        [Command("traditional"), Alias("traditionify")]
        public async Task traditionify([Remainder] string query)
        {
            await ReplyAsync($"It's {query} traditional!",
            embed: new EmbedBuilder()
            .WithAuthor(Config.Momoko.EmbedName, Config.Momoko.EmbedAvatarUrl)
            .WithColor(Config.Momoko.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e4/No.080.jpg/revision/latest?cb=20190701055501")
            .Build());
        }

        [Command("random"), Summary("Show any random Momoko moments")]
        public async Task randomMoments()
        {
            string[] arrRandom =
            {"https://pbs.twimg.com/media/EOFxhI_X0AYjQNV?format=png&name=small","https://pbs.twimg.com/media/EODIh1jXUAMB6tU?format=png&name=small",
            "https://pbs.twimg.com/media/EOAcg8fW4AA6L1o?format=png&name=small","https://pbs.twimg.com/media/EN_xTTsWkAE6h2O?format=png&name=small",
            "https://pbs.twimg.com/media/EN--AkkXUAETq-T?format=png&name=small","https://pbs.twimg.com/media/EN-aX9OX4AErhax?format=png&name=small",
            "https://pbs.twimg.com/media/EN8jSnnWoAEArR1?format=png&name=small","https://pbs.twimg.com/media/EN7X0uCXsAA7QFP?format=png&name=small",
            "https://pbs.twimg.com/media/EN7PUhkXUAA5wh_?format=png&name=small","https://pbs.twimg.com/media/ENywQseXkAAZORC?format=png&name=small",
            "https://pbs.twimg.com/media/ENsyGt6WoAAnl1O?format=png&name=small","https://pbs.twimg.com/media/ENplrBWWoAALx_b?format=png&name=small",
            "https://pbs.twimg.com/media/ENk1RBsWsAEF_kK?format=png&name=small","https://pbs.twimg.com/media/ENivtN1WwAAObll?format=png&name=small",
            "https://pbs.twimg.com/media/ENdRGRvW4AUBBBP?format=png&name=small","https://pbs.twimg.com/media/ENbwEzpWwAEhzaa?format=png&name=small",
            "https://pbs.twimg.com/media/ENacmC-W4AAxFOm?format=png&name=small","https://pbs.twimg.com/media/ENZL4vHXYAMDnS9?format=png&name=small",
            "https://pbs.twimg.com/media/ENV9mlQXYAAo0Y9?format=png&name=small","https://pbs.twimg.com/media/ENVTRWNXkAEV60U?format=png&name=small",
            "https://pbs.twimg.com/media/ENPZtiIX0AIHiUs?format=png&name=small","https://pbs.twimg.com/media/ENMvVPVWsAENtds?format=png&name=small",
            "https://pbs.twimg.com/media/ENLogA5XkAAO_Go?format=png&name=small","https://pbs.twimg.com/media/ENJtDOfWsAUx4VR?format=png&name=small",
            "https://pbs.twimg.com/media/EM_EfBYXUAID071?format=png&name=small","https://pbs.twimg.com/media/EM73XiIWsAALUYU?format=png&name=small",
            "https://pbs.twimg.com/media/EM7mWhLXYAEz2bm?format=png&name=small","https://pbs.twimg.com/media/EM50Uv0WkAAtCqd?format=png&name=small",
            "https://pbs.twimg.com/media/EM5ZZ-CWoAA_E7O?format=png&name=small",
            };

            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Momoko.EmbedColor)
                .WithImageUrl(arrRandom[new Random().Next(0, arrRandom.Length)])
                .Build());
        }

        [Command("shocked"), Alias("omg")]
        public async Task shocked()
        {
            string[] arrRandom = {
                "Oh my God!","Ohhh my God!", "*shocked*", "*gasp*", "Oh my Goodness!"
            };

            string[] arrRandomImg = {
                "https://i.ibb.co/23Q0TP7/Untitled.png"
            };

            await ReplyAsync(arrRandom[new Random().Next(0,arrRandom.Length)],
            embed: new EmbedBuilder()
            .WithAuthor(Config.Momoko.EmbedName, Config.Momoko.EmbedAvatarUrl)
            .WithColor(Config.Momoko.EmbedColor)
            .WithImageUrl(arrRandomImg[new Random().Next(0,arrRandomImg.Length)])
            .Build());
        }

    }

    class MomokoRandomEventModule : ModuleBase<SocketCommandContext>
    {

    }

}
