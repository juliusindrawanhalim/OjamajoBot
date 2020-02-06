using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
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
            "Fill <form> with: **default/motto** to make it spesific form.")]
        public async Task transform(string form = "motto")
        {
            IDictionary<string, string> arrImage = new Dictionary<string, string>();
            arrImage["default"] = "https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/57/Mo-momo.gif";
            arrImage["motto"] = "https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/57/Mo-momo.gif";

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
            .WithAuthor(Config.Momoko.EmbedName, Config.Momoko.EmbedAvatarUrl)
            .WithDescription("Nini is fair skinned with peach blushed cheeks and rounded green eyes. Her light chartreuse hair resembles Momoko's, and on the corner of her head is a lilac star clip. She wears a chartreuse dress with creamy yellow collar." +
            "In teen form the only change to her hair is that her bangs are spread out.She gains a developed body and now wears a pastel gold dress with the shoulder cut out and a white collar, where a gold gem rests.A gold top is worn under this, and she gains white booties and a white witch hat with pastel yellow rim.")
            .WithColor(Config.Momoko.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e4/No.080.jpg/revision/latest?cb=20190701055501")
            .WithFooter("[Ojamajo Witchling Wiki](https://ojamajowitchling.fandom.com/wiki/Nini)")
            .Build());
        }

        [Command("hello"), Summary("Hello, I will greet you up")]
        public async Task momokoHello()
        {
            string tempReply = "";
            List<string> listRandomRespond = new List<string>() {
                $"Hello there, {MentionUtils.MentionUser(Context.User.Id)}. ",
                $"Hey, {MentionUtils.MentionUser(Context.User.Id)}. ",
            };

            int rndIndex = new Random().Next(0, listRandomRespond.Count);
            tempReply = listRandomRespond[rndIndex] + Config.Momoko.arrRandomActivity[Config.Momoko.indexCurrentActivity, 1];

            await ReplyAsync(tempReply);
        }

        [Command("random"), Alias("moments"), Summary("Show any random Momoko moments. " +
            "Fill <moments> with **pre-motto/motto/naisho** for spesific moments.")]
        public async Task randomthing(string moments = "")
        {
            string finalUrl = ""; string footerUrl = "";
            JArray getDataObject = null; moments = moments.ToLower();
            if (moments == "")
            {
                var key = Config.Momoko.jObjRandomMoments.Properties().ToList();
                var randIndex = new Random().Next(0, key.Count);
                moments = key[randIndex].Name;
                getDataObject = (JArray)Config.Momoko.jObjRandomMoments[moments];
                finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
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

        [Command("shocked"), Alias("omg", "shock"), Summary("Oh my God!")]
        public async Task shocked()
        {
            string[] arrRandom = {
                "Oh my God!","Ohhh my God!", "*shocked*", "*gasping*", "Oh my Goodness!",
                "Ohh my Goooneesss!","Oh no!","Oh my GAH!","*le gasp*",":O"
            };

            string[] arrRandomImg = {
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/d/d3/ODN-EP5-068.png",
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/1/1a/Motto-02-momo4.png",
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/6/69/OjamajoLINE2.26.png"
            };

            await ReplyAsync(arrRandom[new Random().Next(0, arrRandom.Length)],
            embed: new EmbedBuilder()
            .WithColor(Config.Momoko.EmbedColor)
            .WithImageUrl(arrRandomImg[new Random().Next(0, arrRandomImg.Length)])
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

        [Command("turn"), Alias("transform"), Summary("Turn <username> into <wishes>")]
        public async Task spells(IUser username, [Remainder] string wishes)
        {
            //await Context.Message.DeleteAsync();
            await ReplyAsync($"Peruton Peton Pararira Pon! Turn {username.Mention} into {wishes}",
            embed: new EmbedBuilder()
            .WithColor(Config.Momoko.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/99/Momo-spell.gif")
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

    [Summary("hidden")]
    class MomokoMagicalStageModule : ModuleBase<SocketCommandContext>
    {
        //magical stage section
        [Command("Pururun purun, Suzuyaka ni!")] //magical stage from doremi
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
                    .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/ad/Motto1-MagicalStage.png")
                    .Build());

        }
    }

    [Summary("hidden")]
    class MomokoRandomEventModule : ModuleBase<SocketCommandContext>
    {

    }

    [Name("Bakery"), Group("bakery"), Summary("Virtual sweet house maho-dou, you can try to order some sweet speciality food on our shop :smile: ")]
    public class MomokoBakery : InteractiveBase
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

        [Command("order", RunMode = RunMode.Async), Summary("I will give you the available listed menu and you can try to order it up.")]
        public async Task Interact_Bakery()
        {
            if (!Config.Momoko.isRunningBakery.ContainsKey(Context.User.Id.ToString()))
                Config.Momoko.isRunningBakery.Add(Context.User.Id.ToString(), false);

            if (!Config.Momoko.isRunningBakery[Context.User.Id.ToString()]){
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
                    .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/c/cc/ODN-EP11-027.png")
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
                            .WithDescription("Oh, it seems you don't want to order anything for now, no worries. " +
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

            try{
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
            } catch(Exception e) {
                Console.WriteLine(e.ToString());
                return e.ToString();
            }
        }

    }

}
