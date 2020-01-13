using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Audio;
using Discord.Commands;
using OjamajoBot.Service;

using Victoria;
using Victoria.Enums;
using Newtonsoft.Json.Linq;
using Discord.WebSocket;

namespace OjamajoBot.Module
{
    class AikoModule : ModuleBase<SocketCommandContext>
    {
        [Command("Help")]
        public async Task showhelp()
        {
            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Aiko.EmbedColor)
                .WithAuthor("Aiko Bot", "https://cdn.discordapp.com/emojis/651063151948726273.png?v=1")
                .WithTitle("Command List:")
                .WithDescription($"Pretty Witchy Aiko Chi~ " +
                $"You can either tell me what to do by mentioning me **<@{Config.Aiko.Id}>** or **aiko!** or **ai!** as the starting command prefix.")
                .AddField("Basic Commands",
                "**hello** : I will greet you up\n" +
                "**change** or **henshin** : I will change into the ojamajo form\n" +
                "**transform <username> <wishes>** : Transform mentioned <username> into <wishes>\n" +
                "**wish <wishes>** : Give the user some <wishes>\n" +
                "**quotes** : I will mention any random quotes.\n" +
                "**random** : I will do anything random." 
                /*"**foreheadjokes** : Forehead Jokes"*/)
                .Build());
        }

        [Command("hello")]
        public async Task aikoHello()
        {
            string tempReply = "";
            List<string> listRandomRespond = new List<string>() {
                    $"Yo {MentionUtils.MentionUser(Context.User.Id)}! ",
                    $"Hello there {MentionUtils.MentionUser(Context.User.Id)}! ",
            };

            int rndIndex = new Random().Next(0, listRandomRespond.Count);
            tempReply = listRandomRespond[rndIndex] + Config.Aiko.arrRandomActivity[Config.Aiko.indexCurrentActivity, 1];

            await ReplyAsync(tempReply);
        }

        [Command("change"), Alias("henshin")]
        public async Task transform()
        {
            await ReplyAsync("Pretty Witchy Aiko Chi~\n");
            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Aiko.EmbedColor)
                .WithImageUrl("https://66.media.tumblr.com/13cf3226a3c5b77a2100cd121de61eb7/tumblr_nnf0ckQlnh1usz98wo1_250.gif")
                .Build());
        }

        [Command("transform")]
        public async Task spells(IUser iuser, [Remainder] string query)
        {
            //await Context.Message.DeleteAsync();
            await ReplyAsync("Pameruku raruku rarirori poppun! Transform " + iuser.Mention + " into " + query);
            await base.ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Aiko.EmbedColor)
            .WithImageUrl("https://i.pinimg.com/originals/79/14/40/7914406b1876370c3058d8b8f14de96e.jpg")
            .Build());
        }

        [Command("wish")]
        public async Task wish([Remainder] string query)
        {
            await ReplyAsync($"Pameruku raruku rarirori poppun! {query}");
            await base.ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Aiko.EmbedColor)
            .WithImageUrl("https://i.pinimg.com/originals/79/14/40/7914406b1876370c3058d8b8f14de96e.jpg")
            .Build());
        }

        [Command("quotes")]
        public async Task quotes()
        {
            String[] arrQuotes = {
                "As a woman from Osaka, I can't lose!",
                "Let's make some delicious takoyaki"
            };

            await ReplyAsync(arrQuotes[new Random().Next(0, arrQuotes.Length)]);
        }

        //[Command("meme")]
        //public async Task giveMeme()
        //{
        //    String [] arrRandom = {
        //        "https://media.discordapp.net/attachments/314512031313035264/659229196693798912/1551058415141.png?width=396&height=469"
        //    };

        //    await ReplyAsync(arrRandom[new Random().Next(0, arrRandom.Length)]);
        //}

        [Command("random")]
        public async Task randomthing()
        {
            String[,] arrRandom =
            { {"Aiko has given you a big smile" , "https://38.media.tumblr.com/224f6ca12018eca4ff34895cce9b7649/tumblr_nds3eyKFLH1r98a5go1_500.gif"},
             {":v", "https://yt3.ggpht.com/a/AGF-l7-JcB38-lOhu0HzFN5NsWre0wgnl50IeIZq8Q=s900-c-k-c0xffffffff-no-rj-mo"},
            {"Swimming lessons will be canceled!","https://thumbs.gfycat.com/EntireGrizzledFlyinglemur-poster.jpg"},
            {":ok_hand:","https://media.discordapp.net/attachments/569409307100315651/651188503991812107/1564810493598.jpg?width=829&height=622"},
            {"Thank you, thank you :wink:","https://66.media.tumblr.com/f6b42eb806ae7b64fc34e6e8b1a18c3f/tumblr_inline_mgcb5odip41r4lv3u.gif" } };

            Random rnd = new Random();
            int rndIndex = rnd.Next(0, arrRandom.GetLength(0));

            await ReplyAsync(arrRandom[rndIndex, 0]);
            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Aiko.EmbedColor)
                .WithImageUrl(arrRandom[rndIndex, 1])
                .Build());
        }

        //todo/upcoming command:
        //[Command("foreheadjokes")]
        //public async Task sendForeheadJokes()
        //{
        //    String[] arrRandom = {
        //        "https://cdn.discordapp.com/attachments/569409307100315651/651127198203510824/unknown.png",
        //    };

        //    await ReplyAsync(arrRandom[new Random().Next(0, arrRandom.GetLength(0))],
        //            embed: new EmbedBuilder()
        //            .WithColor(Config.Hazuki.EmbedColor)
        //            .WithImageUrl("https://cdn.discordapp.com/attachments/663232256676069386/663603236099457035/Dabzuki.png")
        //            .Build());
        
        //}

        //magical stage section
        [Command("Paipai Ponpoi, Shinyaka ni!")] //magical stage from hazuki
        public async Task magicalStage()
        {
            if (Context.User.Id == Config.Hazuki.Id)
            {
                await ReplyAsync($"<@{Config.Doremi.Id}> Pameruku raruku, Takaraka ni! \n",
                    embed: new EmbedBuilder()
                    .WithColor(Config.Aiko.EmbedColor)
                    .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e9/Takarakanis1.2.png/revision/latest?cb=20190408124952")
                    .Build());
            }
        }

        [Command("Magical Stage!")]//Final magical stage: from hazuki
        public async Task magicalStagefinal([Remainder] string query)
        {
            if (Context.User.Id == Config.Hazuki.Id){
                await ReplyAsync($"Magical Stage! {query}\n",
                embed: new EmbedBuilder()
                .WithColor(Config.Aiko.EmbedColor)
                .WithImageUrl("https://i.ytimg.com/vi/HyizF7XWfU8/maxresdefault.jpg")
                .Build());
            }
        }
    }

    class AikoRandomEventModule : ModuleBase<SocketCommandContext>
    {
        List<string> listRespondDefault = new List<string>() {$":sweat_smile: Gommen ne, {MentionUtils.MentionUser(Config.Doremi.Id)} chan, I'm having a plan with my dad later on."};

        [Remarks("go to the shop event")]
        [Command("let's go to maho dou")]
        public async Task eventmahoudou()
        {
            if (Context.User.Id == Config.Doremi.Id)
            {
                List<string> listRespond = new List<string>() {":smile: Yosh! let's go to maho dou." };
                for (int i = 0; i < listRespondDefault.Count - 1; i++)
                    listRespond.Add(listRespondDefault[i]);

                Random rnd = new Random();
                int rndIndex = rnd.Next(0, listRespond.Count); //random the list value
                await ReplyAsync($"{listRespond[rndIndex]}");
            }
        }

        [Remarks("go to doremi house")]
        [Command("let's go to my house today")]
        public async Task eventdoremihouse()
        {
            if (Context.User.Id == Config.Doremi.Id)
            {
                List<string> listRespond = new List<string>() { ":smile: Sure thing, I hope we can also make takoyaki on your house later on." };
                for (int i = 0; i < listRespondDefault.Count - 1; i++)
                    listRespond.Add(listRespondDefault[i]);

                Random rnd = new Random();
                int rndIndex = rnd.Next(0, listRespond.Count); //random the list value
                await ReplyAsync($"{listRespond[rndIndex]}");
            }
        }
    }

    public class AikoInteractive : InteractiveBase
    {
        //[Command("quiz", RunMode = RunMode.Async)]
        //public async Task Interact_Quiz()
        //{
        //    Random rnd = new Random();
        //    int rndQuiz = rnd.Next(0, 2);

        //    String question, replyCorrect, replyWrong;
        //    List<string> answer = new List<string>();
        //    String replyTimeout = "Time's up. Sorry but it seems you haven't answered yet.";

        //    if (rndQuiz == 0)
        //    {
        //        question = "What is my favorite food?";
        //        answer.Add("takoyaki");
        //        replyCorrect = "Yes, that's corret! Takoyaki was one of my favorite food";
        //        replyWrong = "Sorry but that's wrong.";
        //        replyTimeout = "Time's up. My favorite food is takoyaki.";
        //    } else
        //    {
        //        question = "What is my full name?";
        //        answer.Add("aiko senoo"); answer.Add("senoo aiko");
        //        replyCorrect = "Yes, that's corret! Aiko Senoo is my full name.";
        //        replyWrong = "Sorry but that's wrong.";
        //        replyTimeout = "Time's up. Aiko Senoo is my full name.";
        //    }

        //    //response.Content.ToLower() to get the answer

        //    await ReplyAsync(question);
        //    //var response = await NextMessageAsync();
        //    //Boolean wrongLoop = false;
        //    Boolean correctAnswer = false;

        //    while (!correctAnswer)
        //    {
        //        var response = await NextMessageAsync();

        //        if (response == null)
        //        {
        //            await ReplyAsync(replyTimeout);
        //            return;
        //        }
        //        else if (answer.Contains(response.Content.ToLower()))
        //        {
        //            await ReplyAsync(replyCorrect);
        //            correctAnswer = true;
        //        }
        //        else
        //        {
        //            await ReplyAsync(replyWrong);
        //        }
        //    }
        //}

    }
}
