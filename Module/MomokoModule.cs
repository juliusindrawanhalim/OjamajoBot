using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using OjamajoBot.Database.Model;
using OjamajoBot.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OjamajoBot.Module
{
    [Name("General")]
    class MomokoModule : ModuleBase<SocketCommandContext>
    {
        //start
        private readonly CommandService _commands;
        private readonly IServiceProvider _map;

        public MomokoModule(IServiceProvider map, CommandService commands)
        {
            _commands = commands;
            _map = map;
        }

        [Name("help"), Command("help"), Summary("Show all Momoko bot Commands.")]
        public async Task Help([Remainder]string CategoryOrCommands = "")
        {
            EmbedBuilder output = new EmbedBuilder();
            output.Color = Config.Momoko.EmbedColor;

            if (CategoryOrCommands == "")
            {
                output.WithAuthor(Config.Momoko.EmbedName, Config.Momoko.EmbedAvatarUrl);
                output.Title = $"Command List";
                output.Description = "Pretty Witchy Momoko Chi~ You can tell me what to do with " +
                    $"**{Config.Momoko.PrefixParent[2]} or {Config.Momoko.PrefixParent[0]} or {Config.Momoko.PrefixParent[1]}** as starting prefix.\n" +
                    $"Use **{Config.Momoko.PrefixParent[0]}help <commands or category>** for more help details.\n" +
                    $"Example: **{Config.Momoko.PrefixParent[0]}help general** or **{Config.Momoko.PrefixParent[0]}help hello**";

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
                            $"See `{Config.Momoko.PrefixParent[0]}help <commands or category>` for command help.");
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
            completedText += $"**Example:** `{Config.Momoko.PrefixParent[0]}{group}{commands}";
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
            else output = Config.Momoko.PrefixParent[0];
            output = Config.Momoko.PrefixParent[0];
            return output;
        }

        //end

        //[Command]
        //public async Task defaultMention()
        //{
        //    string tempReply = "";
        //    List<string> listRandomRespond = new List<string>() {
        //        $"Hello there, {MentionUtils.MentionUser(Context.User.Id)}. ",
        //    };

        //    int rndIndex = new Random().Next(0, listRandomRespond.Count);
        //    tempReply = $"{listRandomRespond[rndIndex]}I noticed that you're calling for me. Use {Config.Momoko.PrefixParent}help <commands or category> if you need help with the commands.";
        //    await ReplyAsync(tempReply);
        //}

        [Command("change"), Alias("henshin"), Summary("I will change into the ojamajo form. " +
            "Fill <form> with: **default/motto/dokkan** to make it spesific form.")]
        public async Task transform(string form = "dokkan")
        {
            IDictionary<string, string> arrImage = new Dictionary<string, string>();
            arrImage["default"] = "https://cdn.discordapp.com/attachments/706812115482312754/706820194944942141/motto.gif";
            arrImage["motto"] = "https://cdn.discordapp.com/attachments/706812115482312754/706820194944942141/motto.gif";
            arrImage["dokkan"] = "https://cdn.discordapp.com/attachments/706812115482312754/706820390017564692/dokkan.gif";

            if (arrImage.ContainsKey(form)){
                await ReplyAsync("Pretty Witchy Momoko Chi~\n");
                await base.ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Momoko.EmbedColor)
                    .WithImageUrl(arrImage[form])
                    .Build());
            } else {
                await ReplyAsync("Sorry, I can't found that form.");
            }
        }

        [Command("fairy"), Summary("I will show you my fairy info")]
        public async Task showFairy()
        {
            await ReplyAsync("This is my fairy, Nini.",
            embed: new EmbedBuilder()
            .WithAuthor("Nini")
            .WithDescription("Nini is fair skinned with peach blushed cheeks and rounded green eyes. Her light chartreuse hair resembles Momoko's, and on the corner of her head is a lilac star clip. She wears a chartreuse dress with creamy yellow collar." +
            "In teen form the only change to her hair is that her bangs are spread out.She gains a developed body and now wears a pastel gold dress with the shoulder cut out and a white collar, where a gold gem rests.A gold top is worn under this, and she gains white booties and a white witch hat with pastel yellow rim.")
            .WithColor(Config.Momoko.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e4/No.080.jpg")
            .WithFooter("[Ojamajo Witchling Wiki](https://ojamajowitchling.fandom.com/wiki/Nini)")
            .Build());
        }

        [Command("happy birthday"), Summary("Give Momoko some wonderful birthday wishes. This commands only available on her birthday.")]
        public async Task onpuBirthday([Remainder]string wishes = "")
        {
            string[] arrResponse = new string[] { $":smile: Thank you {Context.User.Mention} for the wonderful birthday wishes.",
                $":smile: Thank you {Context.User.Mention}, for giving me the wonderful birthday wishes."};
            string[] arrResponseImg = new string[]{
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/2/29/14.04.JPG",
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/a2/ODN-EP2-092.png"
            };

            if (DateTime.Now.ToString("dd") == Config.Momoko.birthdayDate.ToString("dd") &&
                DateTime.Now.ToString("MM") == Config.Momoko.birthdayDate.ToString("MM") &&
                Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour)
            {
                await ReplyAsync(arrResponse[new Random().Next(0, arrResponse.Length)],
                embed: new EmbedBuilder()
                .WithColor(Config.Momoko.EmbedColor)
                .WithImageUrl(arrResponseImg[new Random().Next(0, arrResponseImg.Length)])
                .Build());
            }
            else
            {
                await ReplyAsync("I'm sorry, but it's not my birthday yet.",
                embed: new EmbedBuilder()
                .WithColor(Config.Momoko.EmbedColor)
                .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/52/ODN-EP9-025.png")
                .Build());
            }
        }

        [Command("hello"), Summary("Hello, I will greet you up")]
        public async Task momokoHello()
        {
            List<string> listRandomRespond = new List<string>() {
                $"Hello, {MentionUtils.MentionUser(Context.User.Id)}. ",
                $"Hey, {MentionUtils.MentionUser(Context.User.Id)}. ",
                $"Hi, {MentionUtils.MentionUser(Context.User.Id)}. ",
            };

            int rndIndex = new Random().Next(0, listRandomRespond.Count);
            string tempReply = listRandomRespond[rndIndex] + Config.Momoko.Status.currentActivityReply;

            await ReplyAsync(embed: new EmbedBuilder()
                .WithDescription(tempReply)
                .WithColor(Config.Momoko.EmbedColor)
                .WithImageUrl("https://cdn.discordapp.com/attachments/706770454697738300/790614647904403506/016406434152f0d59b71d66c44b1b4f2d3bbbb93_00.png")
                .Build());
        }

        [Command("random"), Alias("moments"), Summary("Show any random Momoko moments. " +
            "Fill <moments> with **pre-motto/motto/naisho/dokkan** for spesific moments.")]
        public async Task randomthing(string moments = "")
        {
            string finalUrl = ""; string footerUrl = "";
            JArray getDataObject = null; moments = moments.ToLower();
            if (moments == "")
            {
                int randomType = new Random().Next(0, 5);
                if (randomType != 3){
                    int randomMix = new Random().Next(0, 2); string path;
                    if (randomMix == 0)
                        path = "config/randomMoments/momoko";
                    else
                        path = "config/randomMoments/momoko/mix";

                    string randomPathFile = GlobalFunctions.getRandomFile(path, new string[] { ".png", ".jpg", ".gif", ".webm" });
                    await Context.Channel.SendFileAsync($"{randomPathFile}");
                    return;
                } else {
                    var key = Config.Momoko.jObjRandomMoments.Properties().ToList();
                    var randIndex = new Random().Next(0, key.Count);
                    moments = key[randIndex].Name;
                    getDataObject = (JArray)Config.Momoko.jObjRandomMoments[moments];
                    finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
                }
            }
            else
            {
                if (Config.Momoko.jObjRandomMoments.ContainsKey(moments))
                {
                    getDataObject = (JArray)Config.Momoko.jObjRandomMoments[moments];
                    finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
                }
                else
                {
                    await base.ReplyAsync($"Sorry, I can't found the specified moments. " +
                        $"See `{Config.Momoko.PrefixParent[0]}help random` for commands help.");
                    return;
                }
            }

            footerUrl = finalUrl;
            if (finalUrl.Contains("wikia")) footerUrl = "https://ojamajowitchling.fandom.com/";
            await base.ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Momoko.EmbedColor)
            .WithImageUrl(finalUrl)
            .WithFooter(footerUrl)
            .Build());
        }

        [Command("stats"), Alias("bio"), Summary("I will show you my biography info")]
        public async Task showStats()
        {
            await ReplyAsync("Peruton Peton Pararira Pon! Show my biography info!",
            embed: new EmbedBuilder()
            .WithAuthor("Momoko Asuka", Config.Momoko.EmbedAvatarUrl)
            .WithColor(Config.Momoko.EmbedColor)
            .WithDescription("Momoko Asuka (飛鳥ももこ, Asuka Momoko) is the sixth main character of Ojamajo Doremi, who became part of the group at the start of Motto. " +
            "She was called in by the Witch Queen to help the girls run their brand new Sweet Shop Maho-do.")
            .AddField("Full Name", "飛鳥ももこ Asuka Momoko", true)
            .AddField("Gender", "female", true)
            .AddField("Blood Type", "AB", true)
            .AddField("Birthday", "May 6th, 1990", true)
            .AddField("Instrument", "Guitar", true)
            .AddField("Favorite Food", "Madeleines, Strawberry Tart", true)
            .AddField("Debut", "[Doremi, a Stormy New Semester](https://ojamajowitchling.fandom.com/wiki/Doremi,_a_Stormy_New_Semester)", true)
            .WithImageUrl("https://static.wikia.nocookie.net/ojamajowitchling/images/c/c2/O.D_LFMD%27_Momoko_Asuka.png")
            .WithFooter("Source: [Ojamajo Doremi Wiki](https://ojamajowitchling.fandom.com/wiki/Momoko_Asuka)")
            .Build());
        }

        [Command("thank you"), Alias("thanks", "arigatou"), Summary("Say thank you to Momoko Bot")]
        public async Task thankYou([Remainder] string messages = "")
        {
            await ReplyAsync($"Your welcome, {MentionUtils.MentionUser(Context.User.Id)}. I'm glad that you're happy with it :smile:");
        }

        [Command("traditional"), Alias("traditionify"), Summary("It's <sentences> traditional!")]
        public async Task traditionify([Remainder] string sentences="japanese")
        {
            string[] arrRandomImages = {
            "https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/55/ODN-EP13-018.png",
            "https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/99/ODN-EP13-017.png",
            "https://vignette.wikia.nocookie.net/ojamajowitchling/images/c/ca/Linesticker20.png"};

            await ReplyAsync($"It's {sentences} traditional!",
            embed: new EmbedBuilder()
            .WithColor(Config.Momoko.EmbedColor)
            .WithImageUrl(arrRandomImages[new Random().Next(0,arrRandomImages.Length)])
            .Build());
        }

        [Command("wish"), Summary("I will grant you a <wishes>")]
        public async Task wish([Remainder] string wishes)
        {
            await ReplyAsync($"Peruton Peton Pararira Pon! {wishes}");
            await base.ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Momoko.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/99/Momo-spell.gif")
            .Build());
        }
        //upcoming commands: wow

    }

    [Name("minigame"), Group("minigame"), Summary("This category contains all Momoko minigame interactive commands.")]
    public class MomokoMinigameInteractive : InteractiveBase
    {
        [Command("score"), Summary("Show your minigame score points.")]
        public async Task Show_Quiz_Score()
        {//show the player score
            await ReplyAsync(embed: MinigameCore.printScore(Context, Config.Momoko.EmbedColor).Build());
        }

        [Command("leaderboard"), Summary("Show the top 10 player with the highest score points.")]
        public async Task Show_Minigame_Leaderboard()
        {
            await ReplyAsync(embed: MinigameCore.printLeaderboard(Context, Config.Momoko.EmbedColor).Build());
        }

        [Command("jankenpon", RunMode = RunMode.Async), Alias("rps", "rockpaperscissor"), Summary("Play the Rock Paper Scissors minigame with Momoko. " +
            "Reward: 20 minigame score points & 1 magic seeds.")]
        public async Task RockPaperScissor(string guess = "")
        {
            if (guess == "")
            {
                await ReplyAsync($"Please enter the valid parameter: **rock** or **paper** or **scissors**");
                return;
            }
            else if (guess.ToLower() != "rock" && guess.ToLower() != "paper" && guess.ToLower() != "scissors")
            {
                await ReplyAsync($"Sorry **{Context.User.Username}**. " +
                    $"Please enter the valid parameter: **rock** or **paper** or **scissors**");
                return;
            }

            guess = guess.ToLower();//lower the text
            int randomGuess = new Random().Next(0, 3);//generate random

            string[] arrWinReaction = { $"Better luck next time, {Context.User.Username}.", "I win the game this round!" };//bot win
            string[] arrLoseReaction = { "I loss." };//bot lose
            string[] arrDrawReaction = { "Well, it's a draw." };//bot draw

            Tuple<string, EmbedBuilder, Boolean> result = MinigameCore.rockPaperScissors.rpsResults(Config.Momoko.EmbedColor, Config.Momoko.EmbedAvatarUrl, randomGuess, guess, "momoko", Context.User.Username,
                arrWinReaction, arrLoseReaction, arrDrawReaction,
                Context.Guild.Id, Context.User.Id);

            await Context.Channel.SendFileAsync(result.Item1, embed: result.Item2.Build());
        }

    }

    [Summary("hidden")]
    class MomokoMagicalStageModule : ModuleBase<SocketCommandContext>
    {
        //magical stage section
        [Command("Pururun purun, Suzuyaka ni!")] //magical stage from onpu
        public async Task magicalStage()
        {
            if (Context.User.Id == Config.Onpu.Id)
                await ReplyAsync($"{MentionUtils.MentionUser(Config.Doremi.Id)} Peruton Peton, Sawayaka ni!",
                embed: new EmbedBuilder()
                .WithColor(Config.Momoko.EmbedColor)
                .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/4/4b/MagicalStageMottoMomoko.png")
                .Build());
        }

        [Command("Magical Stage!")]//Final magical stage: from doremi
        public async Task magicalStagefinal([Remainder] string query)
        {
            if (Context.User.Id == Config.Onpu.Id)
                await ReplyAsync($"Magical Stage! {query}\n",
                    embed: new EmbedBuilder()
                    .WithColor(Config.Momoko.EmbedColor)
                    .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/5b/MOD-EP1-078.png")
                    .Build());

        }
    }

    [Summary("hidden")]
    class MomokoRandomEventModule : ModuleBase<SocketCommandContext>
    {

    }

    [Name("patissiere"), Group("patissiere"), Summary("This category contains all Momoko Sweeethouse command.")]
    public class MomokoPatissiere : InteractiveBase
    {
        // NextMessageAsync will wait for the next message to come in over the gateway, given certain criteria
        // By default, this will be limited to messages from the source user in the source channel
        // This method will block the gateway, so it should be ran in async mode.
        //[Command("interact", RunMode = RunMode.Async)]
        //public async Task Test_NextMessageAsync()
        //{
        //    await ReplyAsync("What is 2+2?");
        //    var response = await NextMessageAsync();
        //    if (response != null)
        //        await ReplyAsync($"You replied: {response.Content}");
        //    else
        //        await ReplyAsync("You did not reply before the timeout");
        //}
        //reference: https://github.com/PassiveModding/Discord.Addons.Interactive/blob/master/SocketSampleBot/Module.cs

        //memorization minigame
        [Command("schedule"), Summary("Show the witch exam schedule.")]
        public async Task Show_Exam_Schedule()
        {

        }

        //exam: reset weekly, you can participate exam 1 time for each week. There are 3 witches examiners 
        //Participating a witch exam will grant you patissiere point
        //other user can help to contribute the exam. 
        //User that contribute for the exam will receive the exp multiplied by the amount of people that are joining.

        //1: scones
        //2: choux pastry
        //3: ice cream sandwich
        //4: tourbillon cake

        [Command("recipe"), Summary("Open the recipe diary.")]
        public async Task Open_Recipe()
        {
            string query = $"SELECT * " +
                $" FROM  ";
        }

        [Command("status"), Summary("Show your patissiere Status progress. " +
            "You can add the optional username parameter to see the patissiere status of that user.")]
        public async Task sweethouse_status(SocketGuildUser username = null)
        {

        }

        [Command("buy"), Summary("Buy the available ingredients from Dela.")]
        public async Task Buy_Ingredients([Remainder] string ingredients = "")
        {
            //strawberry,cream,milk,flour,sugar,eggs,butter,oil,baking soda,baking powder,eggs

        }

        [Command("order", RunMode = RunMode.Async), Summary("I will give you the available listed menu and you can try to order it up.")]
        public async Task Interact_Bakery()
        {
            if (!Config.Momoko.isRunningBakery.ContainsKey(Context.User.Id.ToString()))
                Config.Momoko.isRunningBakery.Add(Context.User.Id.ToString(), false);

            if (!Config.Momoko.isRunningBakery[Context.User.Id.ToString()])
            {
                Config.Momoko.isRunningBakery[Context.User.Id.ToString()] = true;

                string[] menu = {
                "apple pie","cake","cookies","chocolate","croissant","cupcakes","donut",
                "eclair","gingerbread","pancake","pudding","waffle","scones" };

                string concatMenu = ""; foreach (string item in menu) concatMenu += $"**-{item}**\n";
                concatMenu += $"Please reply with one of the menu choices, for example: **donut**.\nTo leave or cancel your order, type `cancel`.";

                string replyTimeout = "I'm sorry, I can't process your order.";

                await ReplyAsync(embed: new EmbedBuilder()
                    .WithAuthor("Sweet house Maho-dou", "https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/9f/Sweet2.jpg")
                    .WithDescription("Hello, welcome to the sweet house Maho-dou. " +
                    "Your order will be placed within 20 seconds, please wait shortly right after confirming your order. " +
                    "Please order something up from the menu listed below:")
                    .AddField("Menu list", concatMenu)
                    .WithColor(Config.Momoko.EmbedColor)
                    .WithThumbnailUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/c/cc/ODN-EP11-027.png")
                    .Build());

                Boolean procedureFinish = false;

                while (!procedureFinish)
                {
                    var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
                    string ordered = response.Content.ToLower().ToString();

                    if (response == null)
                    {
                        Config.Momoko.isRunningBakery[Context.User.Id.ToString()] = false;
                        await ReplyAsync(replyTimeout);
                        return;
                    }
                    else if (ordered == "cancel")
                    {
                        Config.Momoko.isRunningBakery[Context.User.Id.ToString()] = false;
                        await ReplyAsync(embed: new EmbedBuilder()
                            .WithAuthor("Sweet house Maho-dou", "https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/9f/Sweet2.jpg")
                            .WithDescription("It seems you don't want to order anything for now. " +
                            "Thank you for stopping by and please come back again soon.")
                            .WithColor(Config.Momoko.EmbedColor)
                            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/c/cc/ODN-EP11-027.png")
                            .Build());
                        return;
                    }
                    else if (!menu.Any(ordered.Contains))
                    {
                        await ReplyAsync("Sorry, I can't find that menu. Please retype the correct order menu choice.");
                    }
                    else if (menu.Any(ordered.Contains))
                    {
                        await ReplyAsync($"Your orders: **{ordered}** will be arrived soon. " +
                            $"Please wait within 20 seconds while we're going to process it.");
                        procedureFinish = true;

                        Config.Momoko.isRunningBakery[Context.User.Id.ToString()] = false;
                        Config.Momoko.timerProcessBakery[Context.User.Id.ToString()] = new Timer(async _ => await ReplyAsync($"Hello {MentionUtils.MentionUser(Context.User.Id)}, your order: **{ordered}** has arrived. " +
                            $"Thank you for ordering from our sweet house maho-dou. Please come back next time :smile:",
                            embed: new EmbedBuilder()
                            .WithAuthor("Sweet house Maho-dou", "https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/9f/Sweet2.jpg")
                            .WithColor(Config.Momoko.EmbedColor)
                            .WithImageUrl(getHtmlResult(ordered))
                            .Build()),
                            null, 20000, Timeout.Infinite);
                        //send thank you image
                        Config.Momoko.timerProcessBakery[Context.User.Id.ToString() + "ty"] = new Timer(async _ => await ReplyAsync(
                              embed: new EmbedBuilder()
                              .WithColor(Config.Momoko.EmbedColor)
                              .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/9d/Linesticker23.png")
                              .Build()),
                            null, 25000, Timeout.Infinite);

                        return;
                    }
                }

            }
            else
                await ReplyAsync($"Sorry, but you still have a running the bakery commands, please finish it first.");

        }

        public string getHtmlResult(string order)
        {
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://source.unsplash.com/random/?"+order);

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://pixabay.com/api/?key=14962595-31091f9e5ebfbdd912a540e0f&q=" + order +
                "&per_page=50&category=food");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string jsonresp = reader.ReadToEnd().ToString();
                JObject jobject = JObject.Parse(jsonresp);
                JArray items = (JArray)jobject.GetValue("hits");
                int totalItems = items.Count;
                int randomedIndex = new Random().Next(0, totalItems);
                return items[randomedIndex]["webformatURL"].ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return e.ToString();
            }
        }

    }

    [Name("Card"), Group("card"), Summary("This category contains all Momoko Trading card command.")]
    public class MomokoTradingCardInteractive : InteractiveBase
    {
        [Command("capture", RunMode = RunMode.Async), Alias("catch"), Summary("Capture spawned card with Momoko.")]
        public async Task<RuntimeResult> trading_card_momoko_capture(string boost="")
        {
            //reference: https://www.newtonsoft.com/json/help/html/ModifyJson.htm
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;

            var guildSpawnData = TradingCardGuildCore.getGuildData(guildId);
            if (Convert.ToInt32(guildSpawnData[DBM_Trading_Card_Guild.Columns.spawn_is_zone]) == 1)
            {
                var userTradingCardData = UserTradingCardDataCore.getUserData(clientId);
                string userCardZone = userTradingCardData[DBM_User_Trading_Card_Data.Columns.card_zone].ToString();
                if (!userCardZone.Contains("momoko"))
                {
                    await ReplyAsync(":x: Sorry, you are not on the correct card zone. " +
                        $"Please assign yourself on the correct card zone with **{Config.Momoko.PrefixParent[0]}card zone set <category>** command.");
                    return Ok();
                }
            }

            //var cardCaptureReturn = TradingCardCore.cardCapture(Config.Momoko.EmbedColor, Context.Client.CurrentUser.GetAvatarUrl(), guildId, clientId, Context.User.Username,
            //TradingCardCore.Momoko.emojiError, "momoko", boost, Config.Momoko.PrefixParent[0], "mo",
            //TradingCardCore.Momoko.maxNormal, TradingCardCore.Momoko.maxPlatinum, TradingCardCore.Momoko.maxMetal, TradingCardCore.Momoko.maxOjamajos);

            var cardCaptureReturn = TradingCardCore.cardCapture(Context, Config.Momoko.EmbedColor,
                TradingCardCore.Momoko.emojiError, "momoko", boost, "mo");

            if (cardCaptureReturn.Item1 == "")
            {
                await ReplyAsync(null, embed: cardCaptureReturn.Item2.Build());
            }
            else
                await ReplyAsync(cardCaptureReturn.Item1,
                    embed: cardCaptureReturn.Item2.Build());

            //check if player is ranked up
            if (cardCaptureReturn.Item3 != "")
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Momoko.EmbedColor)
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
                }

                await Bot.Doremi.client
                    .GetGuild(Context.Guild.Id)
                    .GetTextChannel(Context.Channel.Id)
                    .SendFileAsync(TradingCardCore.Doremi.imgCompleteAllCard, null, embed: TradingCardCore
                    .userCompleteTheirList(Context, Config.Doremi.EmbedColor, Config.Doremi.EmbedAvatarUrl, "doremi",
                    TradingCardCore.Doremi.imgCompleteAllCard, TradingCardCore.Doremi.roleCompletionist)
                    .Build());

            }

            //check if player have captured all hazuki card/not
            if (cardCaptureReturn.Item4["hazuki"])
            {
                if (Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Hazuki.roleCompletionist).ToList().Count >= 1)
                {
                    await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(
                        Context.Guild.Roles.First(x => x.Name == TradingCardCore.Hazuki.roleCompletionist)
                        );
                }

                await Bot.Hazuki.client
                    .GetGuild(Context.Guild.Id)
                    .GetTextChannel(Context.Channel.Id)
                    .SendFileAsync(TradingCardCore.Hazuki.imgCompleteAllCard, null, embed: TradingCardCore
                    .userCompleteTheirList(Context, Config.Hazuki.EmbedColor, Config.Hazuki.EmbedAvatarUrl, "hazuki",
                    TradingCardCore.Hazuki.imgCompleteAllCard, TradingCardCore.Hazuki.roleCompletionist)
                    .Build());

            }

            //check if player have captured all aiko card/not
            if (cardCaptureReturn.Item4["aiko"])
            {
                if (Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Aiko.roleCompletionist).ToList().Count >= 1)
                {
                    await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(
                        Context.Guild.Roles.First(x => x.Name == TradingCardCore.Aiko.roleCompletionist)
                        );
                }

                await Bot.Aiko.client
                    .GetGuild(Context.Guild.Id)
                    .GetTextChannel(Context.Channel.Id)
                    .SendFileAsync(TradingCardCore.Aiko.imgCompleteAllCard, null, embed: TradingCardCore
                    .userCompleteTheirList(Context, Config.Aiko.EmbedColor, Config.Aiko.EmbedAvatarUrl, "aiko",
                    TradingCardCore.Aiko.imgCompleteAllCard, TradingCardCore.Aiko.roleCompletionist)
                    .Build());

            }

            //check if player have captured all onpu card/not
            if (cardCaptureReturn.Item4["onpu"])
            {
                if (Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Onpu.roleCompletionist).ToList().Count >= 1)
                {
                    await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(
                        Context.Guild.Roles.First(x => x.Name == TradingCardCore.Onpu.roleCompletionist)
                        );
                }

                await Bot.Onpu.client
                    .GetGuild(Context.Guild.Id)
                    .GetTextChannel(Context.Channel.Id)
                    .SendFileAsync(TradingCardCore.Onpu.imgCompleteAllCard, null, embed: TradingCardCore
                    .userCompleteTheirList(Context, Config.Onpu.EmbedColor, Config.Onpu.EmbedAvatarUrl, "onpu",
                    TradingCardCore.Onpu.imgCompleteAllCard, TradingCardCore.Onpu.roleCompletionist)
                    .Build());

            }

            //check if player have captured all momoko card/not
            if (cardCaptureReturn.Item4["momoko"])
            {
                if (Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Momoko.roleCompletionist).ToList().Count >= 1)
                {
                    await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(
                        Context.Guild.Roles.First(x => x.Name == TradingCardCore.Momoko.roleCompletionist)
                        );
                }

                await Bot.Momoko.client
                    .GetGuild(Context.Guild.Id)
                    .GetTextChannel(Context.Channel.Id)
                    .SendFileAsync(TradingCardCore.Momoko.imgCompleteAllCard, null, embed: TradingCardCore
                    .userCompleteTheirList(Context, Config.Momoko.EmbedColor, Config.Momoko.EmbedAvatarUrl, "momoko",
                    TradingCardCore.Momoko.imgCompleteAllCard, TradingCardCore.Momoko.roleCompletionist)
                    .Build());

            }

            //check if player have captured all other special card/not
            if (cardCaptureReturn.Item4["special"])
            {
                if (Context.Guild.Roles.Where(x => x.Name == TradingCardCore.roleCompletionistSpecial).ToList().Count >= 1)
                {
                    await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(
                        Context.Guild.Roles.First(x => x.Name == TradingCardCore.roleCompletionistSpecial)
                        );
                }

                await Bot.Momoko.client
                    .GetGuild(Context.Guild.Id)
                    .GetTextChannel(Context.Channel.Id)
                    .SendFileAsync(TradingCardCore.imgCompleteAllCardSpecial, null, embed: TradingCardCore
                    .userCompleteTheirList(Context, Config.Momoko.EmbedColor, Config.Momoko.EmbedAvatarUrl, "other",
                    TradingCardCore.imgCompleteAllCardSpecial, TradingCardCore.roleCompletionistSpecial)
                    .Build());

            }

            return Ok();

        }

        [Command("pureleine", RunMode = RunMode.Async), Alias("oyajide"), Summary("Detect the bad card with the help from oyajide & pureleine computer. " +
            "Insert the answer as parameter to remove the bad cards if it's existed. Example: ha!card pureleine 10")]
        public async Task trading_card_pureleine(string answer = "")
        {
            await ReplyAsync(embed: TradingCardCore.activatePureleine(Context, answer).Build());
            
        }

        [Command("inventory", RunMode = RunMode.Async), Summary("Show the inventory of **Momoko** card category. " +
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
                        TradingCardCore.printInventory(Context, Config.Momoko.EmbedColor, "momoko", category, TradingCardCore.Momoko.maxNormal));
                }

                //platinum category
                if (showAllInventory || category.ToLower() == "platinum")
                {
                    category = "platinum";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Momoko.EmbedColor, "momoko", category, TradingCardCore.Momoko.maxPlatinum));
                }

                //metal category
                if (showAllInventory || category.ToLower() == "metal")
                {
                    category = "metal";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Momoko.EmbedColor, "momoko", category, TradingCardCore.Momoko.maxMetal));
                }

                //ojamajos category
                if (showAllInventory || category.ToLower() == "ojamajos")
                {
                    category = "ojamajos";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Momoko.EmbedColor, "momoko", category, TradingCardCore.Momoko.maxOjamajos));
                }

                //special category
                if (showAllInventory || category.ToLower() == "special")
                {
                    category = "special";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Momoko.EmbedColor, "other", category, TradingCardCore.maxSpecial));
                }

            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        [Command("inventory", RunMode = RunMode.Async), Summary("Show the inventory of **Momoko** card category. " +
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
                    .WithColor(Config.Momoko.EmbedColor)
                    .WithDescription($"Sorry, I can't find that username. Please mention the correct username.")
                    .WithThumbnailUrl(TradingCardCore.Momoko.emojiError).Build());
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
                        TradingCardCore.printInventory(Context, Config.Momoko.EmbedColor, "momoko", category, TradingCardCore.Momoko.maxNormal, username));
                }

                //platinum category
                if (showAllInventory || category.ToLower() == "platinum")
                {
                    category = "platinum";
                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Momoko.EmbedColor, "momoko", category, TradingCardCore.Momoko.maxPlatinum, username));
                }

                //metal category
                if (showAllInventory || category.ToLower() == "metal")
                {
                    category = "metal";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Momoko.EmbedColor, "momoko", category, TradingCardCore.Momoko.maxMetal, username));
                }

                //ojamajos category
                if (showAllInventory || category.ToLower() == "ojamajos")
                {
                    category = "ojamajos";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Momoko.EmbedColor, "momoko", category, TradingCardCore.Momoko.maxOjamajos, username));
                }

                //special category
                if (showAllInventory || category.ToLower() == "special")
                {
                    category = "special";
                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Momoko.EmbedColor, "other", category, TradingCardCore.maxSpecial, username));
                }

            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        [Command("inventory", RunMode = RunMode.Async), Summary("Show the inventory of **Momoko** card category. " +
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
                    .WithColor(Config.Momoko.EmbedColor)
                    .WithDescription($"Sorry, I can't find that username. Please mention the correct username.")
                    .WithThumbnailUrl(TradingCardCore.Momoko.emojiError).Build());
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
                        TradingCardCore.printInventory(Context, Config.Momoko.EmbedColor, "momoko", category, TradingCardCore.Momoko.maxNormal, username));
                }

                //platinum category
                if (showAllInventory || category.ToLower() == "platinum")
                {
                    category = "platinum";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Momoko.EmbedColor, "momoko", category, TradingCardCore.Momoko.maxPlatinum, username));
                }

                //metal category
                if (showAllInventory || category.ToLower() == "metal")
                {
                    category = "metal";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Momoko.EmbedColor, "momoko", category, TradingCardCore.Momoko.maxMetal, username));
                }

                //ojamajos category
                if (showAllInventory || category.ToLower() == "ojamajos")
                {
                    category = "ojamajos";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Momoko.EmbedColor, "momoko", category, TradingCardCore.Momoko.maxOjamajos, username));
                }

                //special category
                if (showAllInventory || category.ToLower() == "special")
                {
                    category = "special";
                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Momoko.EmbedColor, "other", category, TradingCardCore.maxSpecial, username));
                }

            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        [Command("verify", RunMode = RunMode.Async), Summary("Verify the momoko card pack to get the card completion role & badge on this server " +
            " if you have completed it.")]
        public async Task verify_card_completion()
        {
            var guildId = Context.Guild.Id;
            var userId = Context.User.Id;
            string userAvatarUrl = Context.User.GetAvatarUrl();
            string username = Context.User.Username;

            string cardPack = "momoko";

            if (UserTradingCardDataCore.checkCardCompletion(userId, cardPack))
            {
                try
                {
                    if (Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Momoko.roleCompletionist).ToList().Count >= 1)
                    {
                        await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(
                            Context.Guild.Roles.First(x => x.Name == TradingCardCore.Momoko.roleCompletionist)
                        );
                    }
                } catch(Exception e) { }
                
                EmbedBuilder embedReturn = TradingCardCore
                .userCompleteTheirList(Context, Config.Momoko.EmbedColor, Config.Momoko.EmbedAvatarUrl, cardPack,
                TradingCardCore.Momoko.imgCompleteAllCard, TradingCardCore.Momoko.roleCompletionist);

                if (embedReturn != null)
                {
                    await Bot.Momoko.client
                    .GetGuild(Context.Guild.Id)
                    .GetTextChannel(Context.Channel.Id)
                    .SendFileAsync(TradingCardCore.Momoko.imgCompleteAllCard, null, embed: embedReturn
                    .Build());
                } else
                {
                    await ReplyAsync(":white_check_mark: Your **momoko** card completion status has been verified");
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
                        printStatusTemplate(Context, Config.Momoko.EmbedColor, otherUser)
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
                        printStatusComplete(Context, Config.Momoko.EmbedColor, otherUser)
                        .Build());
        }

        [Command("detail", RunMode = RunMode.Async), Alias("info", "look"), Summary("See the detail of Momoko card information from the <card_id>.")]
        public async Task trading_card_look(string card_id)
        {
            await ReplyAsync(null, embed: TradingCardCore.printCardDetailTemplate(Context, Config.Momoko.EmbedColor, card_id, TradingCardCore.Momoko.emojiError)
                    .Build());

        }

        [Command("boost", RunMode = RunMode.Async), Summary("Show card boost status.")]
        public async Task showCardBoostStatus()
        {
            await ReplyAsync(embed: TradingCardCore
                    .printCardBoostStatus(Context, Config.Momoko.EmbedColor)
                    .Build());
        }

        [Command("zone set"), Alias("region set"), Summary("Set your card zone at **momoko** and the entered category. " +
            "Example: **mo!card zone platinum**.")]
        public async Task setCardZone(string category = "")
        {
            await ReplyAsync(embed: TradingCardCore.assignZone(Context, "momoko", category, Config.Momoko.EmbedColor)
                .Build());
        }

        [Command("zone where"), Alias("region where"), Summary("Get your assigned card zone.")]
        public async Task lookCardZone()
        {
            await ReplyAsync(embed: TradingCardCore.lookZone(Context, Config.Momoko.EmbedColor)
                .Build());
        }

        //show top 5 that capture each card pack
        [Command("leaderboard", RunMode = RunMode.Async), Summary("Show top 5 momoko trading card leaderboard status.")]
        public async Task trading_card_leaderboard()
        {
            await ReplyAsync(embed: TradingCardCore.
                    printLeaderboardTemplate(Context, Config.Momoko.EmbedColor, "momoko")
                    .Build());
        }

    }



}
