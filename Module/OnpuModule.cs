using Config;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Lavalink.NET;
using Newtonsoft.Json.Linq;
using OjamajoBot.Database.Model;
using OjamajoBot.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace OjamajoBot.Module
{
    [Name("General")]
    class OnpuModule : ModuleBase<SocketCommandContext>
    {
        //start
        private readonly CommandService _commands;
        private readonly IServiceProvider _map;

        public OnpuModule(IServiceProvider map, CommandService commands)
        {
            _commands = commands;
            _map = map;
        }

        [Name("help"), Command("help"), Summary("Show all Onpu bot Commands.")]
        public async Task Help([Remainder]string CategoryOrCommands = "")
        {
            EmbedBuilder output = new EmbedBuilder();
            output.Color = Config.Onpu.EmbedColor;

            if (CategoryOrCommands == "")
            {
                output.WithAuthor(Config.Onpu.EmbedName, Config.Onpu.EmbedAvatarUrl);
                output.Title = $"Command List";
                output.Description = "Pretty Witchy Onpu Chi~ You can tell me what to do with " +
                    $"**{Config.Onpu.PrefixParent[2]} or {Config.Onpu.PrefixParent[0]} or {Config.Onpu.PrefixParent[1]}** as starting prefix.\n" +
                    $"Use **{Config.Onpu.PrefixParent[0]}help <category or commands>** for more help details.\n" +
                    $"Example: **{Config.Onpu.PrefixParent[0]}help general** or **{Config.Onpu.PrefixParent[0]}help hello**";

                foreach (var mod in _commands.Modules.Where(m => m.Parent == null))
                {
                    AddHelp(mod, ref output);
                }
                await ReplyAsync("", embed: output.Build());
                return;
            }
            else
            {
                var mod = _commands.Modules.FirstOrDefault(m => m.Name.Replace("Module", "").ToLower() == CategoryOrCommands.ToLower());
                if (mod != null)
                {
                    var before = mod.Name;
                    output.Title = $"{char.ToUpper(before.First()) + before.Substring(1).ToLower()} Commands";
                    output.Description = $"{mod.Summary}\n" +
                    (!string.IsNullOrEmpty(mod.Remarks) ? $"({mod.Remarks})\n" : "");
                    //(mod.Submodules.Any() ? $"Submodules: {mod.Submodules.Select(m => m.Name)}\n" : "") + " ";
                    AddCommands(mod, ref output);
                    await ReplyAsync("", embed: output.Build());
                    return;
                }
                else
                { //search for category/child
                    int ctrFounded = 0;
                    var commandsModulesToList = _commands.Modules.ToList();
                    for (var i = 0; i < commandsModulesToList.Count; i++)
                    {
                        for (var j = 0; j < commandsModulesToList[i].Commands.Count; j++)
                        {
                            if ((commandsModulesToList[i].Commands[j].Name.ToLower() == CategoryOrCommands.ToLower() ||
                                commandsModulesToList[i].Commands[j].Aliases.Contains(CategoryOrCommands.ToLower())) &&
                                commandsModulesToList[i].Summary != "hidden")
                            {
                                HelpDetails(ref output,
                                commandsModulesToList[i].Name,
                                commandsModulesToList[i].Commands[j].Summary,
                                GetAliases(commandsModulesToList[i].Commands[j].Aliases),
                                commandsModulesToList[i].Group,
                                commandsModulesToList[i].Commands[j].Name,
                                getParameters(commandsModulesToList[i].Commands[j].Parameters));
                                ctrFounded++;
                            }
                        }
                    }

                    if (ctrFounded >= 1){
                        output.Description = $"I found {ctrFounded} command(s) with **{CategoryOrCommands}** keyword:";
                        await ReplyAsync(embed: output.Build());
                        return;
                    } else {
                        await ReplyAsync($"Sorry, I can't find any related help that you search for. " +
                            $"See `{Config.Onpu.PrefixParent[0]}help <commands or category>` for command help.");
                        return;
                    }

                }
            }

        }

        public void HelpDetails(ref EmbedBuilder builder, string category, string summary,
            string alias, string group, string commands, string parameters)
        {

            var completedText = ""; commands = commands.ToLower(); category = category.ToLower();
            if (summary != "") completedText += $"{summary}\n";
            completedText += $"**Category:** {category}\n";
            if (alias != "") completedText += $"**Alias:** {alias}\n";

            if (!object.ReferenceEquals(group, null))
            {
                group = category + " ";
            }
            completedText += $"**Example:** `{Config.Onpu.PrefixParent[0]}{group}{commands}";
            if (parameters != "") completedText += " " + parameters;
            completedText += "`\n";
            builder.AddField(commands, completedText);
        }

        public void AddHelp(ModuleInfo module, ref EmbedBuilder builder)
        {
            foreach (var sub in module.Submodules) AddHelp(sub, ref builder);

            if (module.Summary != "hidden")
                builder.AddField(f =>
                {
                    var joinedString = string.Join(", ", module.Submodules.Select(m => m.Name));
                    if (joinedString == "") { joinedString = "-"; }
                    f.Name = $"**{char.ToUpper(module.Name.First()) + module.Name.Substring(1).ToLower()}**";
                    var selectedModuleCommands = module.Commands.Select(x => $"`{x.Name.ToLower()}`");
                    f.Value = string.Join(", ", selectedModuleCommands);
                });

        }

        public void AddCommands(ModuleInfo module, ref EmbedBuilder builder)
        {
            foreach (var command in module.Commands)
            {
                command.CheckPreconditionsAsync(Context, _map).GetAwaiter().GetResult();
                AddCommand(command, ref builder);
            }
        }

        public void getAllCommands(ModuleInfo module, ref EmbedBuilder builder, string commandDetails)
        {
            foreach (var command in module.Commands)
            {
                command.CheckPreconditionsAsync(Context, _map).GetAwaiter().GetResult();
                AddCommand(command, ref builder, commandDetails);
            }
        }

        public void AddCommand(CommandInfo command, ref EmbedBuilder builder, string commandDetails = "")
        {
            if (commandDetails == "" ||
                (commandDetails != "" &&
                command.Name == commandDetails))
            {
                builder.AddField(f =>
                {
                    string alt = "";
                    if (command.Aliases.Count >= 2)
                    {
                        for (var i = 1; i < command.Aliases.Count; i++)
                        {
                            alt += $"/{command.Aliases[i].ToLower()}";
                        }
                    }

                    f.Name = $"**{command.Name.ToLower()}{alt}**";
                    f.Value = $"{command.Summary}" +
                    (!string.IsNullOrEmpty(command.Remarks) ? $"({command.Remarks})" : "") +
                    "\n" +
                    $"**Example: **`{GetPrefix(command)}{GetAliases(command)}`";

                });
            }
        }
        public string GetAliases(CommandInfo command)
        {
            StringBuilder output = new StringBuilder();
            if (!command.Parameters.Any()) return output.ToString();
            foreach (var param in command.Parameters)
            {
                if (param.IsOptional)
                {
                    if (param.DefaultValue != null)
                        output.Append($"[default {param.Name}:{param.DefaultValue}]");
                    else
                        output.Append($"[optional:{param.Name}]");
                }
                else if (param.IsMultiple)
                    output.Append($"|{param.Name}|");
                else if (param.IsRemainder)
                    output.Append($"<{param.Name}...>");
                else
                    output.Append($"<{param.Name}>");
            }
            return output.ToString();
        }

        public string GetAliases(IReadOnlyList<string> alias)
        {
            var output = "";
            if (alias == null) return output.ToString();
            for (int i = 1; i < alias.Count; i++)
            {
                output += ($" `{alias[i].ToString()}`,");
            }
            return output.TrimEnd(',').ToString();
        }

        public string getParameters(IReadOnlyList<ParameterInfo> parameters)
        {
            StringBuilder output = new StringBuilder();
            if (!parameters.Any()) return output.ToString();
            foreach (var param in parameters)
            {
                if (param.IsOptional)
                {
                    if (param.DefaultValue != null)
                        output.Append($"[default {param.Name}:{param.DefaultValue}]");
                    else
                        output.Append($"[optional:{param.Name}]");
                }
                else if (param.IsMultiple)
                    output.Append($"|{param.Name}|");
                else if (param.IsRemainder)
                    output.Append($"<{param.Name}...>");
                else
                    output.Append($"<{param.Name}>");
            }
            return output.ToString();
        }

        public string GetPrefix(CommandInfo command)
        {
            var output = GetPrefix(command.Module);
            output += $"{command.Aliases.FirstOrDefault()} ";
            return output;
        }

        public string GetPrefix(ModuleInfo module)
        {
            string output = "";
            if (module.Parent != null) output = $"{GetPrefix(module.Parent)}{output}";
            if (module.Aliases.Any()) output = string.Concat(module.Aliases.FirstOrDefault(), " ");
            else output = Config.Onpu.PrefixParent[0];
            output = Config.Onpu.PrefixParent[0];
            return output;
        }

        //end

        //[Command]
        //public async Task defaultMention()
        //{
        //    string tempReply = "";
        //    List<string> listRandomRespond = new List<string>() {
        //        $"Hello {MentionUtils.MentionUser(Context.User.Id)}. ",
        //    };

        //    int rndIndex = new Random().Next(0, listRandomRespond.Count);
        //    tempReply = $"{listRandomRespond[rndIndex]}I noticed that you're calling for me. Use {Config.Onpu.PrefixParent}help <commands or category> if you need help with the commands.";
        //    await ReplyAsync(tempReply);
        //}

        [Command("change"), Alias("henshin"), Summary("I will change into the ojamajo form. " +
            "Fill <form> with: **default/sharp/royal/motto/dokkan** to make it spesific form.")]
        public async Task transform(string form = "dokkan")
        {
            IDictionary<string, string> arrImage = new Dictionary<string, string>();
            arrImage["default"] = "https://cdn.discordapp.com/attachments/706812100789403659/706819480176689163/sharp.gif";
            arrImage["sharp"] = "https://cdn.discordapp.com/attachments/706812100789403659/706819480176689163/sharp.gif";
            arrImage["royal"] = "https://cdn.discordapp.com/attachments/706812100789403659/706819551119015946/royal.gif";
            arrImage["motto"] = "https://cdn.discordapp.com/attachments/706812100789403659/706819681075331172/motto.gif";
            arrImage["dokkan"] = "https://cdn.discordapp.com/attachments/706812100789403659/706819976409120788/dokkan.gif";

            if (arrImage.ContainsKey(form)){
                await base.ReplyAsync("Pretty Witchy Onpu Chi~",
                    embed: new EmbedBuilder()
                    .WithColor(Config.Onpu.EmbedColor)
                    .WithImageUrl(arrImage[form])
                    .Build());
            } else {
                await ReplyAsync("I'm sorry, I can't found that form.");
            }

        }

        [Command("fairy"), Summary("I will show you my fairy info")]
        public async Task showFairy()
        {
            await ReplyAsync("This is my elegant fairy, Roro.",
            embed: new EmbedBuilder()
            .WithAuthor("Roro")
            .WithDescription("Roro has fair skin with pink blushed cheeks and Onpu's eyes. Her light purple hair frames her face and she has a thick strand sticking up on the left to resemble Onpu's side-tail. She wears a light purple dress with a lilac collar.\n" +
            "In teen form, the only change to her hair is that her side - tail now sticks down, rather than curling up.She gains a developed body and now wears a lilac dress with the shoulder cut out and a white - collar, where a purple gem rests.A lilac top is worn under this, and she gains white booties and a white witch hat with a lilac rim.")
            .WithColor(Config.Onpu.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/84/No.079.jpg")
            .WithFooter("[Ojamajo Witchling Wiki](https://ojamajowitchling.fandom.com/wiki/Roro)")
            .Build());
        }

        [Command("mushroom"),Alias("grow","giant","powerup"), Summary("Make Onpu to take the mushroom power up.")]
        public async Task onpuGrow()
        {
            string completePath = $"config/mushroom_onpu/";
            await ReplyAsync($"**Onpu has grown bigger and bigger from the mushroom power up.**");
            await Context.Channel.SendFileAsync($"{completePath}mario_mushroom.png");
            await Context.Channel.SendFileAsync($"{completePath}onpusmol.png");
            await Context.Channel.SendFileAsync($"{completePath}onpumedium.png");
            await Context.Channel.SendFileAsync($"{completePath}onpubig.png");
            await Context.Channel.SendFileAsync($"{completePath}onpuxtrabig.png");
            
        }

        [Command("happy birthday"), Summary("Give Onpu some wonderful birthday wishes. This commands only available on her birthday.")]
        public async Task onpuBirthday([Remainder]string wishes = "")
        {
            string[] arrResponse = new string[] { $":smile: Thank you {Context.User.Mention} for the wonderful birthday wishes.",
                $":smile: Thank you {Context.User.Mention}, for giving me the wonderful birthday wishes."};
            string[] arrResponseImg = new string[]{
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/2/2f/ODN-EP3-039.png",
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/7/7e/04.04.11.JPG"
            };

            if (DateTime.Now.ToString("dd") == Config.Onpu.birthdayDate.ToString("dd") &&
                DateTime.Now.ToString("MM") == Config.Onpu.birthdayDate.ToString("MM") &&
                Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour)
            {
                await ReplyAsync(arrResponse[new Random().Next(0, arrResponse.Length)],
                embed: new EmbedBuilder()
                .WithColor(Config.Onpu.EmbedColor)
                .WithImageUrl(arrResponseImg[new Random().Next(0, arrResponseImg.Length)])
                .Build());
            } else {
                await ReplyAsync("I'm sorry, but it's not my birthday yet.",
                embed: new EmbedBuilder()
                .WithColor(Config.Onpu.EmbedColor)
                .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/0/05/ODN-EP8-100.png")
                .Build());
            }
        }

        [Command("hello"), Summary("Hello, I will greet you up")]
        public async Task onpuHello()
        {
            List<string> listRandomRespond = new List<string>() {
                $"Hello {MentionUtils.MentionUser(Context.User.Id)}. ",
            };

            int rndIndex = new Random().Next(0, listRandomRespond.Count);
            string tempReply = listRandomRespond[rndIndex] + Config.Onpu.Status.currentActivityReply;

            await ReplyAsync(tempReply);
        }

        [Command("hugs"), Alias("hug"), Summary("I will give warm hug for you or <username>")]
        public async Task HugUser(SocketGuildUser username = null)
        {
            if (username == null)
            {
                string message = $"*hugs back*. Thank you for the friendly hugs, {MentionUtils.MentionUser(Context.User.Id)} :hugging:";
                await Context.Channel.SendMessageAsync(message);
            }
            else
            {
                string message = $"Let's give a warm hugs for {MentionUtils.MentionUser(username.Id)} :hugging:";
                await Context.Channel.SendMessageAsync(message);
            }
        }

        [Command("random"), Alias("moments"), Summary("Show any random Onpu moments. " +
            "Fill <moments> with **random/sharp/motto/naisho/dokkan** for spesific moments.")]
        public async Task randomthing(string moments = "")
        {
            string finalUrl = ""; string footerUrl = "";
            JArray getDataObject = null; moments = moments.ToLower();
            if (moments == ""){

                int randomType = new Random().Next(0, 5);
                if (randomType != 3){
                    int randomMix = new Random().Next(0, 2); string path;
                    if (randomMix == 0)
                        path = "config/randomMoments/onpu";
                    else
                        path = "config/randomMoments/onpu/mix";

                    string randomPathFile = GlobalFunctions.getRandomFile(path, new string[] { ".png", ".jpg", ".gif", ".webm" });
                    await Context.Channel.SendFileAsync($"{randomPathFile}");
                    return;
                } else {
                    var key = Config.Onpu.jObjRandomMoments.Properties().ToList();
                    var randIndex = new Random().Next(0, key.Count);
                    moments = key[randIndex].Name;
                    getDataObject = (JArray)Config.Onpu.jObjRandomMoments[moments];
                    finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
                }
                
            } else {
                if (Config.Onpu.jObjRandomMoments.ContainsKey(moments)){
                    getDataObject = (JArray)Config.Onpu.jObjRandomMoments[moments];
                    finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
                } else {
                    await base.ReplyAsync($"Oops, I can't found the specified moments. " +
                        $"See `{Config.Onpu.PrefixParent[0]}help random` for commands help.");
                    return;
                }
            }

            footerUrl = finalUrl;
            if (finalUrl.Contains("wikia")) footerUrl = "https://ojamajowitchling.fandom.com/";
            await base.ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Onpu.EmbedColor)
            .WithImageUrl(finalUrl)
            .WithFooter(footerUrl)
            .Build());
        }

        //[Command("sing"), Summary("I will sing a random song")]
        //public async Task sing()
        //{
            
        //}

        [Command("sign"), Summary("I will sign and give you my autograph card signatures.")]
        public async Task sign()
        {
            string[] arrRandom = {
                $"**The idol {MentionUtils.MentionUser(Config.Onpu.Id)} has give you a big smiles and her autograph**",
                "\uD83D\uDE09 This is my autograph sign. I hope you're happy with it~",
                "Oh, you want my autograph signatures? Here you go \uD83D\uDE09",
                "\uD83D\uDE09 Here you go, I hope you're happy with it~",
                $"**You have been given the autograph signatures by the idol {MentionUtils.MentionUser(Config.Onpu.Id)}**"
            };

            string[] arrFieldDescription = {
                "I can speak English and Chinese Mandarin",
                "I don't like peppers."
            };

            string[] arrRandomImg = {
                "https://i.4pcdn.org/s4s/1553625652304.jpg",
                "https://i.4pcdn.org/s4s/1502212001406.jpg",
                "https://i.4pcdn.org/s4s/1512681025683.jpg",
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/3d/Onpu_CD.png",
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/1/12/Onpu_6.png",
                "https://i.pinimg.com/236x/d8/c7/24/d8c7243267fd2df3c4bf11172e8885e6.jpg",
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/ef/18.03.JPG",
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/be/18.04.JPG",
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/95/18.05.JPG",
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/ad/Onpu2card.jpg",
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/0/0f/Onpuhappycard.jpg",
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/97/Onpu4card.jpg",
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/39/Onpucdcard.jpg",
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/0/0b/Onpulettercard.jpg",
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/1/1c/Onpusingcard.jpg",
            };

            await ReplyAsync(arrRandom[new Random().Next(0, arrRandom.Length)],
            embed: new EmbedBuilder()
            .WithAuthor("Onpu Segawa", Config.Onpu.EmbedAvatarUrl)
            .WithDescription($"Idol Card#{new Random().Next(1000,2001)}")
            .AddField("Full Name", "瀬川 おんぷ Segawa Onpu", true)
            .AddField("Gender", "female", true)
            .AddField("Blood Type", "B", true)
            .AddField("Birthday", "March 3rd, 1991", true)
            .AddField("Favorite Food", "Waffles, Crepes, Fat-free Candies", true)
            .WithColor(Config.Onpu.EmbedColor)
            .WithThumbnailUrl(arrRandomImg[new Random().Next(0,arrRandomImg.Length)])
            .WithFooter($"Signed by: Onpu Segawa [{DateTime.Now.ToString("yyyy-MM-dd")}]")
            .Build());
        }

        [Command("stats"), Alias("bio"), Summary("I will show you my biography info")]
        public async Task showStats()
        {
            await ReplyAsync($"Pururun purun famifami faa! Give {MentionUtils.MentionUser(Context.User.Id)} my biography info!",
            embed: new EmbedBuilder()
            .WithAuthor("Onpu Segawa", Config.Onpu.EmbedAvatarUrl)
            .WithDescription("Onpu Segawa (瀬川おんぷ, Segawa Onpu) is one of the Main Characters and the fifth Ojamajo, initially starting off as an antagonistic Apprentice beneath Majoruka. She began attending Misora Elementary School and quickly befriended Doremi, Hazuki, and Aiko with the intention of revealing her true goals to them.\n" +
            "At the start of Sharp, Onpu officially joined the group as a tritagonist after revealing that she became a real friend of theirs after losing their Apprentice status.She joined them under Majorika when they were given the job of watching Hana.")
            .AddField("Full Name", "瀬川 おんぷ Segawa Onpu", true)
            .AddField("Gender", "female", true)
            .AddField("Blood Type", "B", true)
            .AddField("Birthday", "March 3rd, 1991", true)
            .AddField("Instrument", "Flute", true)
            .AddField("Favorite Food", "Waffles, Crepes, Fat-free Candies", true)
            .AddField("Debut", "[The Transfer student is a Witch Apprentice?!](https://ojamajowitchling.fandom.com/wiki/The_Transfer_student_is_a_Witch_Apprentice%3F!)", true)
            .WithColor(Config.Onpu.EmbedColor)
            .WithImageUrl("https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcRSfjMF-ijylKYP4f7-Lvdf9Vx_HDrmCWc1DGkoSVWu-CPrHfJl")
            .WithFooter("Source: [Ojamajo Witchling Wiki](https://ojamajowitchling.fandom.com/wiki/Onpu_Segawa)")
            .Build());
        }

        [Command("thank you"), Alias("thanks", "arigatou"), Summary("Say thank you to Onpu Bot")]
        public async Task thankYou([Remainder] string messages = null)
        {
            await ReplyAsync($"Your welcome, {MentionUtils.MentionUser(Context.User.Id)}. I'm glad that you're happy with it :smile:");
        }

        [Command("turn"), Alias("transform"), Summary("Transform <username> into <wishes>")]
        public async Task spells(IUser username, [Remainder] string wishes)
        {
            await ReplyAsync($"Pururun purun famifami faa! Turn {username.Mention} into {wishes}",
            embed: new EmbedBuilder()
            .WithColor(Config.Onpu.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/86/Onpu-spell.gif")
            .Build());
        }

        [Command("wish"), Summary("I will grant you a <wishes>")]
        public async Task wish([Remainder] string wishes)
        {
            await ReplyAsync($"Pururun purun famifami faa! {wishes}");
            await base.ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Onpu.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/86/Onpu-spell.gif")
            .Build());
        }

        //todo: smug commands

    }

    [Name("minigame"), Group("minigame"), Summary("This category contains all Onpu minigame interactive commands.")]
    public class OnpuMinigameInteractive : InteractiveBase
    {
        [Command("score"), Summary("Show your minigame score points.")]
        public async Task Show_Minigame_Score()
        {//show the player score
            await ReplyAsync(embed: MinigameCore.printScore(Context, Config.Doremi.EmbedColor).Build());
        }

        [Command("leaderboard"), Summary("Show the top 10 player with the highest score points.")]
        public async Task Show_Minigame_Leaderboard()
        {//show top 10 player score
            await ReplyAsync(embed: MinigameCore.printLeaderboard(Context, Config.Onpu.EmbedColor).Build());
        }

        [Command("rockpaperscissor", RunMode = RunMode.Async), Alias("rps"), Summary("Play the Rock Paper Scissor minigame with Hazuki. 20 score points reward.")]
        public async Task RockPaperScissor(string guess = "")
        {
            if (guess == "")
            {
                await ReplyAsync($"Please enter the valid parameter: **rock** or **paper** or **scissor**");
                return;
            }
            else if (guess.ToLower() != "rock" && guess.ToLower() != "paper" && guess.ToLower() != "scissor")
            {
                await ReplyAsync($"Sorry **{Context.User.Username}**. " +
                    $"Please enter the valid parameter: **rock** or **paper** or **scissor**");
                return;
            }

            guess = guess.ToLower();//lower the text
            int randomGuess = new Random().Next(0, 3);//generate random

            string[] arrWinReaction = { $"Better luck next time, {Context.User.Username}.", "I win the game this round!" };//bot win
            string[] arrLoseReaction = { "I lose from the game." };//bot lose
            string[] arrDrawReaction = { "Well, it's a draw." };//bot draw

            Tuple<string, EmbedBuilder, Boolean> result = MinigameCore.rockPaperScissor.rpsResults(Config.Onpu.EmbedColor, Config.Onpu.EmbedAvatarUrl, randomGuess, guess, "onpu", Context.User.Username,
                arrWinReaction, arrLoseReaction, arrDrawReaction,
                Context.Guild.Id, Context.User.Id);

            await Context.Channel.SendFileAsync(result.Item1, embed: result.Item2.Build());
        }

    }

    [Name("Card"), Group("card"), Summary("This category contains all Onpu Trading card command.")]
    public class OnpuTradingCardInteractive : InteractiveBase
    {
        [Command("capture", RunMode = RunMode.Async), Alias("catch"), Summary("Capture spawned card with Onpu.")]
        public async Task<RuntimeResult> trading_card_onpu_capture(string boost = "")
        {
            //reference: https://www.newtonsoft.com/json/help/html/ModifyJson.htm
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;

            var guildSpawnData = TradingCardGuildCore.getGuildData(guildId);
            if (Convert.ToInt32(guildSpawnData[DBM_Trading_Card_Guild.Columns.spawn_is_zone]) == 1)
            {
                var userTradingCardData = UserTradingCardDataCore.getUserData(clientId);
                string userCardZone = userTradingCardData[DBM_User_Trading_Card_Data.Columns.card_zone].ToString();
                if (!userCardZone.Contains("onpu"))
                {
                    await ReplyAndDeleteAsync(":x: Sorry, you are not on the correct card zone. " +
                        $"Please assign yourself on the correct card zone with **{Config.Onpu.PrefixParent[0]}card zone set <category>** command.", timeout: TimeSpan.FromSeconds(20));
                    return Ok();
                }
            }

            //var cardCaptureReturn = TradingCardCore.cardCapture(Config.Onpu.EmbedColor, Context.Client.CurrentUser.GetAvatarUrl(), guildId, clientId, Context.User.Username,
            //TradingCardCore.Onpu.emojiError, "onpu", boost, Config.Onpu.PrefixParent[0], "on",
            //TradingCardCore.Onpu.maxNormal, TradingCardCore.Onpu.maxPlatinum, TradingCardCore.Onpu.maxMetal, TradingCardCore.Onpu.maxOjamajos);

            var cardCaptureReturn = TradingCardCore.cardCapture(Context, Config.Onpu.EmbedColor,
                TradingCardCore.Onpu.emojiError, "onpu", boost, "on");

            if (cardCaptureReturn.Item1 == "")
            {
                await ReplyAndDeleteAsync(null, embed: cardCaptureReturn.Item2.Build(), timeout: TimeSpan.FromSeconds(15));
            }
            else
                await ReplyAsync(cardCaptureReturn.Item1,
                    embed: cardCaptureReturn.Item2.Build());

            //check if player is ranked up
            if (cardCaptureReturn.Item3 != "")
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Onpu.EmbedColor)
                    .WithTitle("Rank Up!")
                    .WithDescription(cardCaptureReturn.Item3)
                    .WithThumbnailUrl(Context.User.GetAvatarUrl())
                    .Build());

            //check if player have captured all doremi card/not
            if (cardCaptureReturn.Item4["doremi"])
            {
                if (Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Doremi.roleCompletionist).ToList().Count >= 1)
                {
                    await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(
                        Context.Guild.Roles.First(x => x.Name == TradingCardCore.Doremi.roleCompletionist)
                    );

                    await Bot.Doremi.client
                    .GetGuild(Context.Guild.Id)
                    .GetTextChannel(Context.Channel.Id)
                    .SendFileAsync(TradingCardCore.Doremi.imgCompleteAllCard, null, embed: TradingCardCore
                    .userCompleteTheirList(Context, Config.Doremi.EmbedColor, Config.Doremi.EmbedAvatarUrl, "doremi",
                    TradingCardCore.Doremi.imgCompleteAllCard, TradingCardCore.Doremi.roleCompletionist)
                    .Build());

                }
            }

            //check if player have captured all hazuki card/not
            if (cardCaptureReturn.Item4["hazuki"])
            {
                if (Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Hazuki.roleCompletionist).ToList().Count >= 1)
                {
                    await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(
                        Context.Guild.Roles.First(x => x.Name == TradingCardCore.Hazuki.roleCompletionist)
                        );

                    await Bot.Hazuki.client
                    .GetGuild(Context.Guild.Id)
                    .GetTextChannel(Context.Channel.Id)
                    .SendFileAsync(TradingCardCore.Hazuki.imgCompleteAllCard, null, embed: TradingCardCore
                    .userCompleteTheirList(Context, Config.Hazuki.EmbedColor, Config.Hazuki.EmbedAvatarUrl, "hazuki",
                    TradingCardCore.Hazuki.imgCompleteAllCard, TradingCardCore.Hazuki.roleCompletionist)
                    .Build());
                }
            }

            //check if player have captured all aiko card/not
            if (cardCaptureReturn.Item4["aiko"])
            {
                if (Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Aiko.roleCompletionist).ToList().Count >= 1)
                {
                    await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(
                        Context.Guild.Roles.First(x => x.Name == TradingCardCore.Aiko.roleCompletionist)
                        );

                    await Bot.Aiko.client
                    .GetGuild(Context.Guild.Id)
                    .GetTextChannel(Context.Channel.Id)
                    .SendFileAsync(TradingCardCore.Aiko.imgCompleteAllCard, null, embed: TradingCardCore
                    .userCompleteTheirList(Context, Config.Aiko.EmbedColor, Config.Aiko.EmbedAvatarUrl, "aiko",
                    TradingCardCore.Aiko.imgCompleteAllCard, TradingCardCore.Aiko.roleCompletionist)
                    .Build());
                }
            }

            //check if player have captured all onpu card/not
            if (cardCaptureReturn.Item4["onpu"])
            {
                if (Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Onpu.roleCompletionist).ToList().Count >= 1)
                {
                    await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(
                        Context.Guild.Roles.First(x => x.Name == TradingCardCore.Onpu.roleCompletionist)
                        );

                    await Bot.Onpu.client
                    .GetGuild(Context.Guild.Id)
                    .GetTextChannel(Context.Channel.Id)
                    .SendFileAsync(TradingCardCore.Aiko.imgCompleteAllCard, null, embed: TradingCardCore
                    .userCompleteTheirList(Context, Config.Onpu.EmbedColor, Config.Onpu.EmbedAvatarUrl, "onpu",
                    TradingCardCore.Onpu.imgCompleteAllCard, TradingCardCore.Onpu.roleCompletionist)
                    .Build());
                }
            }

            //check if player have captured all momoko card/not
            if (cardCaptureReturn.Item4["momoko"])
            {
                if (Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Momoko.roleCompletionist).ToList().Count >= 1)
                {
                    await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(
                        Context.Guild.Roles.First(x => x.Name == TradingCardCore.Momoko.roleCompletionist)
                        );

                    await Bot.Momoko.client
                    .GetGuild(Context.Guild.Id)
                    .GetTextChannel(Context.Channel.Id)
                    .SendFileAsync(TradingCardCore.Aiko.imgCompleteAllCard, null, embed: TradingCardCore
                    .userCompleteTheirList(Context, Config.Momoko.EmbedColor, Config.Momoko.EmbedAvatarUrl, "momoko",
                    TradingCardCore.Momoko.imgCompleteAllCard, TradingCardCore.Momoko.roleCompletionist)
                    .Build());
                }
            }

            //check if player have captured all other special card/not
            if (cardCaptureReturn.Item4["special"])
            {
                if (Context.Guild.Roles.Where(x => x.Name == TradingCardCore.roleCompletionistSpecial).ToList().Count >= 1)
                {
                    await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(
                        Context.Guild.Roles.First(x => x.Name == TradingCardCore.roleCompletionistSpecial)
                        );

                    await Bot.Onpu.client
                    .GetGuild(Context.Guild.Id)
                    .GetTextChannel(Context.Channel.Id)
                    .SendFileAsync(TradingCardCore.Onpu.imgCompleteAllCard, null, embed: TradingCardCore
                    .userCompleteTheirList(Context, Config.Onpu.EmbedColor, Config.Onpu.EmbedAvatarUrl, "other",
                    TradingCardCore.imgCompleteAllCardSpecial, TradingCardCore.roleCompletionistSpecial)
                    .Build());
                }
            }

            return Ok();

        }

        [Command("pureleine", RunMode = RunMode.Async), Alias("pureline"), Summary("Detect the bad card with the help from oyajide & pureleine computer. " +
            "Insert the answer as parameter to remove the bad cards if it's existed. Example: ha!card pureleine 10")]
        public async Task trading_card_pureleine(string answer = "")
        {
            await ReplyAsync(embed: TradingCardCore.activatePureleine(Context, answer).Build());   
        }

        [Command("inventory", RunMode = RunMode.Async), Summary("Show the inventory of **Onpu** card category. " +
            "You can put optional parameter with this format: <bot>!card inventory <category> <username>.")]
        public async Task trading_card_inventory_self(string category = "")
        {
            Boolean showAllInventory = true;
            if (category.ToLower() != "normal" && category.ToLower() != "platinum" && category.ToLower() != "metal" &&
                category.ToLower() != "ojamajos" && category.ToLower() != "special" && category.ToLower() != "other" &&
                category.ToLower() != "")
            {
                await ReplyAsync($":x: Sorry, that is not the valid pack/category. " +
                $"Valid category: **normal**/**platinum**/**metal**/**ojamajos**/**special**/**other**");
                return;
            }
            else if (category.ToLower() == "other")
            {
                category = "special";
                showAllInventory = false;
            }
            else if (category.ToLower() != "")
                showAllInventory = false;

            try
            {
                //normal category
                if (showAllInventory || category.ToLower() == "normal")
                {
                    category = "normal";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Onpu.EmbedColor, "onpu", category, TradingCardCore.Onpu.maxNormal));
                }

                //platinum category
                if (showAllInventory || category.ToLower() == "platinum")
                {
                    category = "platinum";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Onpu.EmbedColor, "onpu", category, TradingCardCore.Onpu.maxPlatinum));
                }

                //metal category
                if (showAllInventory || category.ToLower() == "metal")
                {
                    category = "metal";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Onpu.EmbedColor, "onpu", category, TradingCardCore.Onpu.maxMetal));
                }

                //ojamajos category
                if (showAllInventory || category.ToLower() == "ojamajos")
                {
                    category = "ojamajos";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Onpu.EmbedColor, "onpu", category, TradingCardCore.Onpu.maxOjamajos));
                }

                //special category
                if (showAllInventory || category.ToLower() == "special")
                {
                    category = "special";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Onpu.EmbedColor, "other", category, TradingCardCore.maxSpecial));
                }

            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        [Command("inventory", RunMode = RunMode.Async), Summary("Show the inventory of **Onpu** card category. " +
            "You can put optional parameter with this format: <bot>!card inventory <category> <username>.")]
        public async Task trading_card_inventory_other([Remainder]SocketGuildUser username = null)
        {
            if (username != null)
            {
                try
                {
                    var clientId = username.Id;
                    var userUsername = username.Username;
                    var userAvatar = username.GetAvatarUrl();
                }
                catch
                {
                    await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Onpu.EmbedColor)
                    .WithDescription($"Sorry, I can't find that username. Please mention the correct username.")
                    .WithThumbnailUrl(TradingCardCore.Onpu.emojiError).Build());
                    return;
                }
            }

            Boolean showAllInventory = true;

            try
            {
                string category = "";
                //normal category
                if (showAllInventory || category.ToLower() == "normal")
                {
                    category = "normal";

                    PaginatedAppearanceOptions pao = new PaginatedAppearanceOptions();
                    pao.JumpDisplayOptions = JumpDisplayOptions.Never;
                    pao.DisplayInformationIcon = false;

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Onpu.EmbedColor, "onpu", category, TradingCardCore.Onpu.maxNormal, username));
                }

                //platinum category
                if (showAllInventory || category.ToLower() == "platinum")
                {
                    category = "platinum";
                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Onpu.EmbedColor, "onpu", category, TradingCardCore.Onpu.maxPlatinum, username));
                }

                //metal category
                if (showAllInventory || category.ToLower() == "metal")
                {
                    category = "metal";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Onpu.EmbedColor, "onpu", category, TradingCardCore.Onpu.maxMetal, username));
                }

                //ojamajos category
                if (showAllInventory || category.ToLower() == "ojamajos")
                {
                    category = "ojamajos";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Onpu.EmbedColor, "onpu", category, TradingCardCore.Onpu.maxOjamajos, username));
                }

                //special category
                if (showAllInventory || category.ToLower() == "special")
                {
                    category = "special";
                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Onpu.EmbedColor, "other", category, TradingCardCore.maxSpecial, username));
                }

            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        [Command("inventory", RunMode = RunMode.Async), Summary("Show the inventory of **Onpu** card category. " +
            "You can put optional parameter with this format: <bot>!card inventory <category> <username>.")]
        public async Task trading_card_inventory_category_other(string category = "", [Remainder]SocketGuildUser username = null)
        {
            if (username != null)
            {
                try
                {
                    var clientId = username.Id;
                    var userUsername = username.Username;
                    var userAvatar = username.GetAvatarUrl();
                }
                catch
                {
                    await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Onpu.EmbedColor)
                    .WithDescription($"Sorry, I can't find that username. Please mention the correct username.")
                    .WithThumbnailUrl(TradingCardCore.Onpu.emojiError).Build());
                    return;
                }
            }

            Boolean showAllInventory = true;
            if (category.ToLower() != "normal" && category.ToLower() != "platinum" && category.ToLower() != "metal" &&
                category.ToLower() != "ojamajos" && category.ToLower() != "special" && category.ToLower() != "other" &&
                category.ToLower() != "")
            {
                await ReplyAsync($":x: Sorry, that is not the valid pack/category. " +
                $"Valid category: **normal**/**platinum**/**metal**/**ojamajos**/**special**/**other**");
                return;
            }
            else if (category.ToLower() == "other")
            {
                category = "special";
                showAllInventory = false;
            }
            else if (category.ToLower() != "")
                showAllInventory = false;

            try
            {
                //normal category
                if (showAllInventory || category.ToLower() == "normal")
                {
                    category = "normal";

                    PaginatedAppearanceOptions pao = new PaginatedAppearanceOptions();
                    pao.JumpDisplayOptions = JumpDisplayOptions.Never;
                    pao.DisplayInformationIcon = false;

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Onpu.EmbedColor, "onpu", category, TradingCardCore.Onpu.maxNormal, username));
                }

                //platinum category
                if (showAllInventory || category.ToLower() == "platinum")
                {
                    category = "platinum";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Onpu.EmbedColor, "onpu", category, TradingCardCore.Onpu.maxPlatinum, username));
                }

                //metal category
                if (showAllInventory || category.ToLower() == "metal")
                {
                    category = "metal";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Onpu.EmbedColor, "onpu", category, TradingCardCore.Onpu.maxMetal, username));
                }

                //ojamajos category
                if (showAllInventory || category.ToLower() == "ojamajos")
                {
                    category = "ojamajos";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Onpu.EmbedColor, "onpu", category, TradingCardCore.Onpu.maxOjamajos, username));
                }

                //special category
                if (showAllInventory || category.ToLower() == "special")
                {
                    category = "special";
                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Aiko.EmbedColor, "other", category, TradingCardCore.maxSpecial, username));
                }

            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        [Command("verify", RunMode = RunMode.Async), Summary("Verify the onpu card pack to get the card completion role & badge on this server " +
            " if you have completed it.")]
        public async Task verify_card_completion()
        {
            var guildId = Context.Guild.Id;
            var userId = Context.User.Id;
            string userAvatarUrl = Context.User.GetAvatarUrl();
            string username = Context.User.Username;

            string cardPack = "onpu";

            if (UserTradingCardDataCore.checkCardCompletion(userId, cardPack))
            {
                if (Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Onpu.roleCompletionist).ToList().Count >= 1)
                {
                    await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(
                        Context.Guild.Roles.First(x => x.Name == TradingCardCore.Onpu.roleCompletionist)
                    );

                    await Bot.Onpu.client
                    .GetGuild(Context.Guild.Id)
                    .GetTextChannel(Context.Channel.Id)
                    .SendFileAsync(TradingCardCore.Onpu.imgCompleteAllCard, null, embed: TradingCardCore
                    .userCompleteTheirList(Context, Config.Onpu.EmbedColor, Config.Onpu.EmbedAvatarUrl, cardPack,
                    TradingCardCore.Onpu.imgCompleteAllCard, TradingCardCore.Onpu.roleCompletionist)
                    .Build());
                }
            }
        }

        [Command("status", RunMode = RunMode.Async), Summary("Show your Trading Card Progression/Status. " +
            "You can put the mentioned username to see the card status of that user.")]
        public async Task trading_card_status([Remainder]SocketGuildUser otherUser = null)
        {
            if (otherUser != null)
            {
                try
                {
                    ulong clientId = otherUser.Id;
                    string userUsername = otherUser.Username;
                    string userAvatar = otherUser.GetAvatarUrl();
                }
                catch
                {
                    await ReplyAsync(":x: Sorry, I can't find that username. Please enter the correct username.");
                    return;
                }
            }

            await ReplyAsync(embed: TradingCardCore.
                        printStatusTemplate(Context, Config.Onpu.EmbedColor, otherUser)
                        .Build());
        }

        [Command("status complete", RunMode = RunMode.Async), Summary("Show your trading card completion date status. " +
            "You can add the optional username parameter to see the completion card status of that user.")]
        public async Task trading_card_status_complete([Remainder] SocketGuildUser otherUser = null)
        {
            if (otherUser != null)
            {
                try
                {
                    ulong clientId = otherUser.Id;
                    string userUsername = otherUser.Username;
                    string userAvatar = otherUser.GetAvatarUrl();
                }
                catch
                {
                    await ReplyAsync(":x: Sorry, I can't find that username. Please enter the correct username.");
                    return;
                }
            }

            await ReplyAsync(embed: TradingCardCore.
                        printStatusComplete(Context, Config.Onpu.EmbedColor, otherUser)
                        .Build());
        }

        [Command("detail", RunMode = RunMode.Async), Alias("info","look"), Summary("See the detail of Onpu card information from the <card_id>.")]
        public async Task trading_card_look(string card_id)
        {
            await ReplyAsync(null, embed: TradingCardCore.printCardDetailTemplate(Context, Config.Onpu.EmbedColor, card_id, TradingCardCore.Onpu.emojiError)
                    .Build());

        }

        [Command("boost", RunMode = RunMode.Async), Summary("Show card boost status.")]
        public async Task showCardBoostStatus()
        {
            await ReplyAsync(embed: TradingCardCore
                    .printCardBoostStatus(Context, Config.Onpu.EmbedColor)
                    .Build());
        }

        [Command("zone set"), Alias("region set"), Summary("Set your card zone at **onpu** and the entered category. " +
            "Example: **on!card zone platinum**.")]
        public async Task setCardZone(string category = "")
        {
            await ReplyAsync(embed: TradingCardCore.assignZone(Context, "onpu", category, Config.Onpu.EmbedColor)
                .Build());
        }

        [Command("zone where"), Alias("region where"), Summary("Get your assigned card zone.")]
        public async Task lookCardZone()
        {
            await ReplyAsync(embed: TradingCardCore.lookZone(Context, Config.Onpu.EmbedColor)
                .Build());
        }

        //show top 5 that capture each card pack
        [Command("leaderboard", RunMode = RunMode.Async), Summary("Show top 5 onpu trading card leaderboard status.")]
        public async Task trading_card_leaderboard()
        {
            await ReplyAsync(embed: TradingCardCore.
                    printLeaderboardTemplate(Context, Config.Onpu.EmbedColor, "onpu")
                    .Build());
        }
    }

    [Name("Music"), Remarks("Please join any voice channel first, then type `on!join` so the bot can stream on your voice channel.")]
    public sealed class OnpuVictoriaMusic : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public OnpuVictoriaMusic(LavaNode lavanode)
        {
            _lavaNode = lavanode;
        }

        [Command("Join"), Summary("Join to your connected voice channel (Please join any voice channel first)")]
        public async Task JoinAsync()
        {
            //await _lavaNode.JoinAsync((Context.User as IVoiceState).VoiceChannel);
            //await ReplyAsync($"Joined {(Context.User as IVoiceState).VoiceChannel} channel!");


            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm already connected to a voice channel.");
                return;
            }

            var voiceState = Context.User as IVoiceState;

            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("Please join any voice channel first.");
                return;
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($":microphone: Onpu has joined {voiceState.VoiceChannel.Name}. Thank you for inviting me~");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Leave"), Summary("Leave from connected voice channel")]
        public async Task LeaveAsync()
        {
            //await _lavaNode.LeaveAsync((Context.User as IVoiceState).VoiceChannel);
            //await ReplyAsync($"Left {(Context.User as IVoiceState).VoiceChannel} channel!");


            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to any voice channels yet.");
                return;
            }

            var voiceChannel = (Context.User as IVoiceState).VoiceChannel ?? player.VoiceChannel;
            if (voiceChannel == null)
            {
                await ReplyAsync("Not sure which voice channel to disconnect from.");
                return;
            }

            try
            {
                await _lavaNode.LeaveAsync(voiceChannel);
                await ReplyAsync(embed:new EmbedBuilder()
                    .WithColor(Config.Onpu.EmbedColor)
                    .WithDescription($"Onpu is now leaving {MentionUtils.MentionChannel(voiceChannel.Id)}. Thank you for inviting me up~")
                    .WithThumbnailUrl("https://pm1.narvii.com/6537/3e62e3ff05fc640b28943026d4c5efd28f43adc1_00.jpg")
                    .WithFooter("Signed by: Onpu Segawa",Config.Onpu.EmbedAvatarUrl)
                    .Build());
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Move"), Summary("Move onpu bot into your new connected voice channel")]
        public async Task MoveAsync()
        {
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            await _lavaNode.MoveChannelAsync((Context.User as IVoiceState).VoiceChannel);
            var player = _lavaNode.GetPlayer(Context.Guild);
            await ReplyAsync($"Moved from {player.VoiceChannel} to {(Context.User as IVoiceState).VoiceChannel}!");
        }

        [Command("Seek"), Summary("Seek the music into the given <timespan>[hh:mm:ss]")]
        public async Task SeekAsync([Remainder] string timespan)
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await ReplyAsync("Sorry, I can't seek when nothing is playing.");
                return;
            }

            try
            {
                await player.SeekAsync(TimeSpan.Parse(timespan));
                await ReplyAsync($"I've seeked `{player.Track.Title}` to {TimeSpan.Parse(timespan)}.");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        //https://www.youtube.com/watch?v=dQw4w9WgXcQ
        [Command("play"), Alias("yt"), Summary("Play the youtube music. `<KeywordOrUrl>` parameter can be a search keyword or youtube url.")]
        public async Task PlayYoutubeAsync([Remainder] string KeywordOrUrl)
        {
            if (string.IsNullOrWhiteSpace(KeywordOrUrl))
            {
                await ReplyAsync("Please provide search terms.");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync($"Please invite me with **{Config.Onpu.PrefixParent[0]}join** on your connected voice channel.");
                return;
            }

            var search = await _lavaNode.SearchYouTubeAsync(KeywordOrUrl);
            var track = search.Tracks.FirstOrDefault();

            Boolean isUrl = Uri.IsWellFormedUriString(KeywordOrUrl, UriKind.Absolute);

            //check maximum video/music must be under 5 minutes
            if (track.Duration.TotalMinutes >= 8)
            {
                await ReplyAsync($"Sorry, that music is above/within 8 minutes. Please use the shorter one.");
                return;
            }

            var player = _lavaNode.HasPlayer(Context.Guild)
                ? _lavaNode.GetPlayer(Context.Guild)
                : await _lavaNode.JoinAsync((Context.User as IVoiceState).VoiceChannel);

            if (player.PlayerState == PlayerState.Playing)
            {
                player.Queue.Enqueue(track);
                //Config.Music.storedLavaTrack[Context.Guild.Id.ToString()].Add(track);
                if (!isUrl)
                    await ReplyAsync(embed: new EmbedBuilder()
                    .WithAuthor("⬇️ Added to queue")
                    .WithTitle(track.Title)
                    .WithColor(Config.Onpu.EmbedColor)
                    .WithUrl(track.Url)
                    .AddField("Duration", track.Duration, true)
                    .AddField("Author", track.Author, true)
                    .WithThumbnailUrl($"https://i.ytimg.com/vi/{track.Id}/hqdefault.jpg")
                    .WithFooter("Onpu Musicbox", Config.Onpu.EmbedAvatarUrl)
                    .Build());
                 else 
                    await ReplyAsync($"⬇️ Added to queue: {track.Title}");
                
            }
            else
            {
                await player.PlayAsync(track);
                if (!isUrl)
                    await ReplyAsync(embed: new EmbedBuilder()
                    .WithTitle(track.Title)
                    .WithColor(Config.Onpu.EmbedColor)
                    .WithUrl(track.Url)
                    .WithAuthor("Now Playing")
                    .AddField("Duration", track.Duration, true)
                    .AddField("Author", track.Author, true)
                    .WithThumbnailUrl($"https://i.ytimg.com/vi/{track.Id}/hqdefault.jpg")
                    .WithFooter("Onpu Musicbox", Config.Onpu.EmbedAvatarUrl)
                    .Build());
                else
                    await ReplyAsync($"Now playing: {track.Title}");
            }
        }

        [Command("radio playall"), Alias("radio all"), Summary("Play all the music that's available on onpu music list")]
        public async Task PlayAll()
        {

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync($"Please invite me with **{Config.Onpu.PrefixParent[0]}join** on your connected voice channel.");
                return;
            }

            var player = _lavaNode.HasPlayer(Context.Guild)
                ? _lavaNode.GetPlayer(Context.Guild)
                : await _lavaNode.JoinAsync((Context.User as IVoiceState).VoiceChannel);

            await ReplyAsync($"I will play all music on the musiclist");

            JObject jObj = Config.Music.jobjectfile;

            for (int i = 0; i < (jObj.GetValue("musiclist") as JObject).Count; i++)
            {
                string query = jObj.GetValue("musiclist")[(i + 1).ToString()]["filename"].ToString();
                var searchResponse = await _lavaNode.SearchAsync("music/" + query);

                if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                searchResponse.LoadStatus == LoadStatus.NoMatches)
                {
                    await ReplyAsync($"I can't find anything for `{query}`.");
                    return;
                }

                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                {
                    if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                    {
                        foreach (var track in searchResponse.Tracks)
                        {
                            //Config.Music.storedLavaTrack[Context.Guild.Id.ToString()].Add(track);
                            player.Queue.Enqueue(track);
                        }

                        //await ReplyAsync($"🔈 Enqueued {searchResponse.Tracks.Count} tracks.");
                    }
                    else
                    {
                        var track = searchResponse.Tracks[0];
                        player.Queue.Enqueue(track);
                        //await ReplyAsync($"🔈 Enqueued: {track.Title}");
                    }
                }
                else
                {
                    var track = searchResponse.Tracks[0];
                    //Config.Music.storedLavaTrack[Context.Guild.Id.ToString()].Add(track);

                    if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                    {
                        for (var j = 0; j < searchResponse.Tracks.Count; j++)
                        {
                            if (j == 0)
                            {
                                await player.PlayAsync(track);
                                await ReplyAsync($"🔈 Now Playing: {track.Title}");
                            }
                            else
                            {
                                player.Queue.Enqueue(searchResponse.Tracks[j]);
                            }
                        }

                        //await ReplyAsync($"🔈 Enqueued {searchResponse.Tracks.Count} tracks.");
                    }
                    else
                    {
                        await player.PlayAsync(track);
                        await ReplyAsync($"🔈 Now Playing: {track.Title}");
                    }
                }

            }

        }

        [Command("radio"), Summary("Play the music with the given <track number or title> parameter")]
        public async Task PlayLocal([Remainder] string TrackNumbersOrTitle)
        {
            if (string.IsNullOrWhiteSpace(TrackNumbersOrTitle))
            {
                await ReplyAsync($"Please provide track numbers or title. Use {Config.Onpu.PrefixParent[0]}mulist to show all onpu music list.");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync($"Please invite me with **{Config.Onpu.PrefixParent[0]}join** on your connected voice channel.");
                return;
            }

            var player = _lavaNode.HasPlayer(Context.Guild)
                ? _lavaNode.GetPlayer(Context.Guild)
                : await _lavaNode.JoinAsync((Context.User as IVoiceState).VoiceChannel);

            JObject jObj = Config.Music.jobjectfile;
            if (int.TryParse(TrackNumbersOrTitle, out int n))
            {

                if (n <= (jObj.GetValue("musiclist") as JObject).Count)
                {
                    TrackNumbersOrTitle = jObj.GetValue("musiclist")[n.ToString()]["filename"].ToString();
                }
                else
                {
                    await ReplyAsync($"Sorry, I can't find anything for track number {TrackNumbersOrTitle}. See the available onpu music list on `{Config.Onpu.PrefixParent[0]}mulist`.");
                    return;
                }

            }
            else
            {
                for (int i = 0; i < (jObj.GetValue("musiclist") as JObject).Count; i++)
                {
                    string replacedFilename = jObj.GetValue("musiclist")[(i + 1).ToString()]["filename"].ToString().Replace(".mp3", "").Replace(".ogg", "");
                    if (replacedFilename == TrackNumbersOrTitle)
                    {
                        TrackNumbersOrTitle = jObj.GetValue("musiclist")[(i + 1).ToString()]["filename"].ToString();
                    }

                }
            }

            var searchResponse = await _lavaNode.SearchAsync("music/" + TrackNumbersOrTitle);
            if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                searchResponse.LoadStatus == LoadStatus.NoMatches)
            {
                await ReplyAsync($"I wasn't able to find anything for `{TrackNumbersOrTitle}`. See the available onpu music list on `onpu!mulist` commands.");
                return;
            }

            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                {
                    foreach (var track in searchResponse.Tracks)
                    {
                        player.Queue.Enqueue(track);
                        Console.WriteLine("play queue:" + track.Title);
                        //Config.Music.storedLavaTrack[Context.Guild.Id.ToString()].Add(track);
                    }

                    await ReplyAsync($":arrow_down: Enqueued {searchResponse.Tracks.Count} tracks.");
                }
                else
                {
                    var track = searchResponse.Tracks[0];
                    player.Queue.Enqueue(track);
                    //Config.Music.storedLavaTrack[Context.Guild.Id.ToString()].Add(track);
                    await ReplyAsync($":arrow_down: Enqueued: {track.Title}");
                }
            }
            else
            {
                var track = searchResponse.Tracks[0];
                //Config.Music.storedLavaTrack[Context.Guild.Id.ToString()].Add(track);

                if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                {
                    for (var i = 0; i < searchResponse.Tracks.Count; i++)
                    {
                        if (i == 0)
                        {
                            await player.PlayAsync(track);
                            await ReplyAsync($"🔈 Now Playing: {track.Title}");
                        }
                        else
                        {
                            player.Queue.Enqueue(searchResponse.Tracks[i]);
                        }
                    }

                    await ReplyAsync($":arrow_down: Enqueued {searchResponse.Tracks.Count} tracks.");
                }
                else
                {
                    await player.PlayAsync(track);
                    await ReplyAsync($"🔈 Now Playing: {track.Title}");
                }
            }
        }

        [Command("NowPlaying"), Alias("Np"), Summary("Show the currently played music (Needs to be from Youtube)")]
        public async Task NowPlayingAsync()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await ReplyAsync("I'm not playing any tracks yet.");
                return;
            }

            var track = player.Track;

            var embed = new EmbedBuilder
            {
                Title = $"{track.Title}",
                ThumbnailUrl = $"https://i.ytimg.com/vi/{track.Id}/hqdefault.jpg",
                Url = track.Url,
                Color = Config.Onpu.EmbedColor
            }

            .WithAuthor("Now Playing")
            .AddField("Author", track.Author,true)
            .AddField("Duration", track.Duration, true)
            .AddField("Position", track.Position.ToString(@"hh\:mm\:ss"), true)
            .WithFooter("Onpu Musicbox", Config.Onpu.EmbedAvatarUrl);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("Pause"), Summary("Pause the music player")]
        public async Task PauseAsync()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await ReplyAsync("I cannot pause when I'm not playing anything.");
                return;
            }

            try
            {
                await player.PauseAsync();
                await ReplyAsync($":pause_button: Music Paused: **{player.Track.Title}**");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Resume"), Summary("Resume the music player")]
        public async Task ResumeAsync()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Paused)
            {
                await ReplyAsync("I cannot resume when I'm not playing anything.");
                return;
            }

            try
            {
                await player.ResumeAsync();
                await ReplyAsync($":arrow_forward: Music Resumed: **{player.Track.Title}**");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Stop"), Summary("Stop the music player")]
        public async Task StopAsync()
        {
            //Config.Music.storedLavaTrack.Clear();
            var player = _lavaNode.HasPlayer(Context.Guild)
                ? _lavaNode.GetPlayer(Context.Guild)
                : await _lavaNode.JoinAsync((Context.User as IVoiceState).VoiceChannel);
            player.Queue.Clear();
            await player.StopAsync();
            await ReplyAsync($":stop_button: Okay, I have stop the music.");
        }

        [Command("Skip"), Summary("Skip into next track")]
        public async Task SkipAsync()
        {
            var player = _lavaNode.GetPlayer(Context.Guild);

            if (!_lavaNode.TryGetPlayer(Context.Guild, out player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await ReplyAsync("Sorry, I can't skip when nothing is playing.");
                return;
            } else if (player.Queue.Count <= 0) {
                await ReplyAsync("Sorry, there's no next queue on the list.");
                return;
            }

            //player.Queue.Enqueue(player.Track);
            string oldTrack = player.Track.Title;
            await player.SkipAsync();

            var track = player.Track;

            await ReplyAsync($"⏭️ Ok, I've skipped: **{oldTrack}**.", 
                embed: new EmbedBuilder()
                .WithAuthor("Now Playing")
                .WithTitle(track.Title)
                .WithColor(Config.Onpu.EmbedColor)
                .WithUrl(track.Url)
                .AddField("Duration", track.Duration, true)
                .AddField("Author", track.Author, true)
                .WithThumbnailUrl($"https://i.ytimg.com/vi/{track.Id}/hqdefault.jpg")
                .WithFooter("Onpu Musicbox", Config.Onpu.EmbedAvatarUrl)
                .Build());

        }

        [Command("Volume"), Summary("Set the music player volume into given <volume>. Max: 200")]
        public async Task SetVolume([Remainder] ushort volume)
        {
            await _lavaNode.GetPlayer(Context.Guild).UpdateVolumeAsync(volume);
            await ReplyAsync($":sound: Volume set to:{volume}");
        }

        [Command("Musiclist"), Alias("mulist"), Summary("Show all available onpu music list")]
        public async Task ShowMusicList()
        {
            JObject jObj = Config.Music.jobjectfile;
            String musiclist = "";
            for (int i = 0; i < (jObj.GetValue("musiclist") as JObject).Count; i++)
            {
                string replacedFilename = jObj.GetValue("musiclist")[(i + 1).ToString()]["filename"].ToString().Replace(".mp3", "").Replace(".ogg", "");
                string title = jObj.GetValue("musiclist")[(i + 1).ToString()]["title"].ToString();
                musiclist += $"[**{i + 1}**] **{replacedFilename}** : {title}\n";
            }
            //for (int i = 0; i < Config.MusicList.arrMusicList.Count; i++)
            //{
            //    String seperatedMusicTitle = Config.MusicList.arrMusicList[i].Replace(".mp3", "").Replace(".ogg", "");//erase format
            //    String musiclist = $"[**{i + 1}**] **ojamajocarnival** : Ojamajo Carnival\n";
            //}

            await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Onpu.EmbedColor)
                .WithTitle("Onpu Music List:")
                .WithDescription($"These are the music list that's available for me to play: " +
                $"You can use the **radio** commands followed with the track number or title.\n" +
                $"Example: **{Config.Onpu.PrefixParent[0]}radio 1** or **{Config.Onpu.PrefixParent[0]}radio ojamajocarnival**")
                .AddField("[Num] Title",
                musiclist)
                .Build());
        }

        [Command("queue"), Alias("muq"), Summary("Show all music in queue list")]
        public async Task ShowMusicListQueue()
        {
            try
            {
                if (_lavaNode.HasPlayer(Context.Guild))
                {
                    var player = _lavaNode.GetPlayer(Context.Guild);
                    //var itemsQueue = player.Queue.Items.Cast<LavaTrack>().ToList();

                    var allTracks = player.Queue.Cast<LavaTrack>().ToList();
                    String musiclist = "";
                    musiclist += $"**1**. **[{player.Track.Title}]({player.Track.Url})** [Now Playing]\n";

                    if (player.Queue.Count >= 1)
                    {
                        for (int i = 0; i < player.Queue.Count; i++)
                            musiclist += $"**{i + 2}**. **[{allTracks[i].Title}]({allTracks[i].Url})**\n";
                    }

                    await ReplyAsync(embed: new EmbedBuilder()
                            .WithColor(Config.Onpu.EmbedColor)
                            .WithThumbnailUrl($"https://i.ytimg.com/vi/{player.Track.Id}/hqdefault.jpg")
                            .WithTitle("⬇️ Current music in queue:")
                            .AddField($"Title", musiclist)
                            .WithFooter($"Total music in queue: {player.Queue.Count + 1}", Config.Onpu.EmbedAvatarUrl)
                            .Build());
                }
            } catch(Exception e) {
                Console.WriteLine(e.ToString());
                await ReplyAsync($"Sorry, there are no music on the current queue list.");
                return;
            }
            


        }

    }

    [Summary("hidden")]
    class OnpuMagicalStageModule : ModuleBase<SocketCommandContext>
    {
        //magical stage section
        [Command("Pameruku raruku, Takaraka ni!")] //magical stage from doremi
        public async Task magicalStage()
        {
            if (Context.User.Id == Config.Aiko.Id)
                await ReplyAsync($"{MentionUtils.MentionUser(Config.Momoko.Id)} Pururun purun, Suzuyaka ni!",
                embed: new EmbedBuilder()
                .WithColor(Config.Onpu.EmbedColor)
                .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/1/1d/MagicalStageMottoOnpu.png")
                .Build());
        }

        [Command("Magical Stage!")]//Final magical stage: from doremi
        public async Task magicalStagefinal([Remainder] string query)
        {
            if (Context.User.Id == Config.Aiko.Id)
                await ReplyAsync($"{MentionUtils.MentionUser(Config.Momoko.Id)} Magical Stage! {query}\n");
        }
    }

    [Summary("hidden")]
    class OnpuRandomEventModule : ModuleBase<SocketCommandContext>
    {
        
    }

}
