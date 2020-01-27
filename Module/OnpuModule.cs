using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using OjamajoBot.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            "Fill <form> with: **default/sharp/royal/motto** to make it spesific form.")]
        public async Task transform(string form = "motto")
        {
            IDictionary<string, string> arrImage = new Dictionary<string, string>();
            arrImage["default"] = "https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/3e/Sh-onpu.gif";
            arrImage["sharp"] = "https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/3e/Sh-onpu.gif";
            arrImage["royal"] = "https://vignette.wikia.nocookie.net/ojamajowitchling/images/f/f0/Royalonpu.gif";
            arrImage["motto"] = "https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e1/Mo-onpu.gif";

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
            .WithAuthor(Config.Onpu.EmbedName, Config.Onpu.EmbedAvatarUrl)
            .WithDescription("Roro has fair skin with pink blushed cheeks and Onpu's eyes. Her light purple hair frames her face and she has a thick strand sticking up on the left to resemble Onpu's side-tail. She wears a light purple dress with a lilac collar.\n" +
            "In teen form, the only change to her hair is that her side - tail now sticks down, rather than curling up.She gains a developed body and now wears a lilac dress with the shoulder cut out and a white - collar, where a purple gem rests.A lilac top is worn under this, and she gains white booties and a white witch hat with a lilac rim.")
            .WithColor(Config.Onpu.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/84/No.079.jpg/revision/latest?cb=20190429130114")
            .WithFooter("[Ojamajo Witchling Wiki](https://ojamajowitchling.fandom.com/wiki/Roro)")
            .Build());
        }

        [Command("hello"), Summary("Hello, I will greet you up")]
        public async Task onpuHello()
        {
            string tempReply = "";
            List<string> listRandomRespond = new List<string>() {
                $"Hello {MentionUtils.MentionUser(Context.User.Id)}. ",
            };

            int rndIndex = new Random().Next(0, listRandomRespond.Count);
            tempReply = listRandomRespond[rndIndex] + Config.Onpu.arrRandomActivity[Config.Onpu.indexCurrentActivity, 1];

            await ReplyAsync(tempReply);
        }

        [Command("random"), Alias("moments"), Summary("Show any random Onpu moments. " +
            "Fill <moments> with **random/sharp/motto/naisho** for spesific moments.")]
        public async Task randomthing(string moments = "")
        {
            string finalUrl = ""; string footerUrl = "";
            JArray getDataObject = null; moments = moments.ToLower();
            if (moments == ""){
                var key = Config.Onpu.jObjRandomMoments.Properties().ToList();
                var randIndex = new Random().Next(0, key.Count);
                moments = key[randIndex].Name;
                getDataObject = (JArray)Config.Onpu.jObjRandomMoments[moments];
                finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
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

        [Command("autograph"), Summary("I will sign and give you my autograph card signatures.")]
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
            .WithDescription($"Idol Card#{new Random().Next(1000,2000)}")
            .AddField("Full Name", "瀬川 おんぷ Segawa Onpu", true)
            .AddField("Gender", "female", true)
            .AddField("Blood Type", "B", true)
            .AddField("Birthday", "March 3rd, 1991", true)
            .AddField("Favorite Food", "Waffles, Crepes, Fat-free Candies", true)
            .WithColor(Config.Onpu.EmbedColor)
            .WithImageUrl(arrRandomImg[new Random().Next(0,arrRandomImg.Length)])
            .WithFooter($"Signed by: Onpu Segawa [{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}]")
            .Build());

        }

        [Command("stats"), Alias("bio"), Summary("I will show you my biography info")]
        public async Task showStats()
        {
            await ReplyAsync($"Pururun purun famifami faa! Give {MentionUtils.MentionUser(Context.User.Id)} my biography info!",
            embed: new EmbedBuilder()
            .WithAuthor(Config.Onpu.EmbedName, Config.Onpu.EmbedAvatarUrl)
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
            //await Context.Message.DeleteAsync();
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
