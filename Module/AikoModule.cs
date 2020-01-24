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
    [Name("General")]
    class AikoModule : ModuleBase<SocketCommandContext>
    {
        //start
        private readonly CommandService _commands;
        private readonly IServiceProvider _map;

        public AikoModule(IServiceProvider map, CommandService commands)
        {
            _commands = commands;
            _map = map;
        }

        [Name("help"), Command("help"), Summary("Show all Aiko bot Commands.")]
        public async Task Help([Remainder]string CategoryOrCommands = "")
        {
            EmbedBuilder output = new EmbedBuilder();
            output.Color = Config.Aiko.EmbedColor;

            if (CategoryOrCommands == "")
            {
                output.WithAuthor(Config.Aiko.EmbedName, Config.Aiko.EmbedAvatarUrl);
                output.Title = $"Command List";
                output.Description = "Pretty Witchy Aiko Chi~ You can tell me what to do with " +
                    $"**{Config.Aiko.PrefixParent[2]} or {Config.Aiko.PrefixParent[0]} or {Config.Aiko.PrefixParent[1]}** as starting prefix.\n" +
                    $"Use **{Config.Aiko.PrefixParent[0]}help <category or commands>** for more help details.\n" +
                    $"Example: **{Config.Aiko.PrefixParent[0]}help general** or **{Config.Aiko.PrefixParent[0]}help hello**";

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

                    if (ctrFounded >= 1) {
                        output.Description = $"I found {ctrFounded} command(s) with **{CategoryOrCommands}** keyword:";
                        await ReplyAsync(embed: output.Build());
                        return;
                    } else {
                        await ReplyAsync($"Gomen ne, I can't find any related help that you search for. " +
                            $"See `{Config.Aiko.PrefixParent[0]}help <commands or category>` for command help.");
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
            completedText += $"**Example:** `{Config.Aiko.PrefixParent[0]}{group}{commands}";
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
            if (module.Aliases.Any())
            {
                output = string.Concat(module.Aliases.FirstOrDefault(), " ");
            }
            else
            {
                output = Config.Aiko.PrefixParent[0];
            }
            output = Config.Aiko.PrefixParent[0];
            return output;
        }

        //end

        //[Command]
        //public async Task defaultMention()
        //{
        //    string tempReply = "";
        //    List<string> listRandomRespond = new List<string>() {
        //        $"Yo {MentionUtils.MentionUser(Context.User.Id)}! ",
        //        $"Hi {MentionUtils.MentionUser(Context.User.Id)}! ",
        //    };

        //    int rndIndex = new Random().Next(0, listRandomRespond.Count);
        //    tempReply = $"{listRandomRespond[rndIndex]}I noticed that you're calling for me. Use {Config.Aiko.PrefixParent}help <commands or category> if you need help with the commands.";
        //    await ReplyAsync(tempReply);
        //}

        [Command("change"), Alias("henshin"), Summary("I will change into the ojamajo form. " +
            "Fill <form> with: **default/sharp/royal/motto** to make it spesific form.")]
        public async Task transform(string form = "motto")
        {
            IDictionary<string, string> arrImage = new Dictionary<string, string>();
            arrImage["default"] = "https://vignette.wikia.nocookie.net/ojamajowitchling/images/d/d1/Ca-aiko.gif";
            arrImage["sharp"] = "https://vignette.wikia.nocookie.net/ojamajowitchling/images/7/74/Sh-aiko.gif";
            arrImage["royal"] = "https://vignette.wikia.nocookie.net/ojamajowitchling/images/c/c6/Royalaiko.gif";
            arrImage["motto"] = "https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/ac/Mo-aiko.gif";

            if (arrImage.ContainsKey(form)) {
                await ReplyAsync("Pretty Witchy Aiko Chi~\n");
                await base.ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Aiko.EmbedColor)
                    .WithImageUrl(arrImage[form])
                    .Build());
            } else {
                await ReplyAsync("Gomen ne, I can't found that form.");
            }


        }

        [Command("fairy"), Summary("I will show you my fairy info")]
        public async Task showFairy()
        {
            await ReplyAsync("This is Mimi, my fairy.",
            embed: new EmbedBuilder()
            .WithAuthor(Config.Aiko.EmbedName, Config.Aiko.EmbedAvatarUrl)
            .WithDescription("Mimi has fair skin and sharp blue eyes and blushed cheeks. She has light blue hair that sticks up on each side of her head in tube-like shapes, with her bangs brushed to the left. " +
            "She wears a baby blue dress with a pale blue collar. In teen form, her hair stays the same and she gains a white witch hat with pale blue rim. She gains a full body and wears a pale blue dress with the shoulder cut out and a white-collar, where a blue gem rests. A baby blue top is worn under this, and she gains white booties.")
            .WithColor(Config.Aiko.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e5/No.078.jpg/revision/latest?cb=20190414080440")
            .WithFooter("[Ojamajo Witchling Wiki](https://ojamajowitchling.fandom.com/wiki/Mimi)")
            .Build());
        }

        [Command("gigantamax"), Summary("I will turn into gigantamax form")]
        public async Task gigantamaxAiko()
        {
            string[] arrRandom = {
                "This is not my final form!", "Pameruku raruku rarirori poppun! Turn me into gigantamax!",
                "Meet the gigantamax Aiko!","Aiko has been gigantamaxed!",
                "Gigantamax Aiko ready for action!", "Muahaha! I have been gigantamax-ed!"
            };

            await ReplyAsync(embed: new EmbedBuilder()
                    .WithAuthor("Gigantamax Aiko", "https://cdn.discordapp.com/attachments/569409307100315651/651127198203510824/unknown.png")
                    .WithDescription($"Level: 99\nHP: 999/999\n{arrRandom[new Random().Next(0, arrRandom.GetLength(0))]}")
                    .WithColor(Config.Aiko.EmbedColor)
                    .WithImageUrl("https://cdn.discordapp.com/attachments/569409307100315651/651127198203510824/unknown.png")
                    .WithFooter("Contributed by: Letter Three")
                    .Build());
        }

        [Command("hello"), Alias("yo"), Summary("Yo, I will greet you up")]
        public async Task aikoHello()
        {
            string tempReply = "";
            List<string> listRandomRespond = new List<string>() {
                $"Yo {MentionUtils.MentionUser(Context.User.Id)}! ",
                $"Hi {MentionUtils.MentionUser(Context.User.Id)}! ",
            };

            int rndIndex = new Random().Next(0, listRandomRespond.Count);
            tempReply = listRandomRespond[rndIndex] + Config.Aiko.arrRandomActivity[Config.Aiko.indexCurrentActivity, 1];

            await ReplyAsync(tempReply);
        }

        [Command("hugs"), Alias("hug"), Summary("I will give warm hug for you or <username>")]
        public async Task HugUser(SocketGuildUser username = null)
        {
            if (username == null)
            {
                string message = $"*hugs back*. Why, thank you for the warm hugs there {MentionUtils.MentionUser(Context.User.Id)} :hugging:";
                await Context.Channel.SendMessageAsync(message);
            }
            else
            {
                string message = $"Pameruku raruku rarirori poppun! Give a big warm hugs for {MentionUtils.MentionUser(username.Id)} :hugging:";
                await Context.Channel.SendMessageAsync(message);
            }
        }

        [Command("quotes"), Summary("I will mention random Aiko quotes")]
        public async Task quotes()
        {
            string[] arrQuotes = {
                "As a woman from Osaka, I can't lose!",
                "Let's make some delicious takoyaki"
            };

            await ReplyAsync(arrQuotes[new Random().Next(0, arrQuotes.Length)]);
        }

        [Command("random"), Alias("moments"), Summary("Show any random Aiko moments. Fill <moments> with **random/first/sharp/motto/naisho** for spesific moments.")]
        public async Task randomthing(string moments = "")
        {
            string finalUrl = ""; string footerUrl = "";
            JArray getDataObject = null; moments = moments.ToLower();
            if (moments == "")
            {
                var key = Config.Aiko.jObjRandomMoments.Properties().ToList();
                var randIndex = new Random().Next(0, key.Count);
                moments = key[randIndex].Name;
                getDataObject = (JArray)Config.Aiko.jObjRandomMoments[moments];
                finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
            }
            else
            {
                if (Config.Aiko.jObjRandomMoments.ContainsKey(moments))
                {
                    getDataObject = (JArray)Config.Aiko.jObjRandomMoments[moments];
                    finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
                }
                else
                {
                    await base.ReplyAsync($"Gomen ne, I can't found the specified moments. " +
                        $"See `{Config.Aiko.PrefixParent[0]}help random` for commands help.");
                    return;
                }
            }

            footerUrl = finalUrl;
            if (finalUrl.Contains("wikia")) footerUrl = "https://ojamajowitchling.fandom.com/";
            await base.ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Aiko.EmbedColor)
            .WithImageUrl(finalUrl)
            .WithFooter(footerUrl)
            .Build());
        }

        [Command("spooky"), Alias("exe","creepy","spook"), Summary("Please don't use this commands...")]
        public async Task showSpookyAiko()
        {
            int randAngryAiko = new Random().Next(0, 11);
            if (randAngryAiko == 5){
                string[] arrSentences = {
                    "Oy! Stop using this command. At least use another nice command for me will ya?!",
                    "I won't let you use this command again!",
                    "Oy! Stop using this command!",
                    "Oy! Stop making fun of the spooky aiko!",
                    "No!I won't let you use this command!",
                    "I don't think you'll get spooky aiko this time!",
                    "You think it was spooky Aiko? You get this one instead!",
                    "I'm preventing you from getting the spooky Aiko!",
                    "Oy! At least use another command instead using the spooky aiko one!"
                };

                await base.ReplyAsync(embed: new EmbedBuilder()
                    .WithAuthor(Config.Aiko.EmbedName, "https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b3/Linesticker14.png")
                    .WithDescription(arrSentences[new Random().Next(0, arrSentences.Length)])
                    .WithColor(Color.DarkerGrey)
                    .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b3/Linesticker14.png")
                    .Build());
            } else {
                string[] arrSentences = {
                    "Didn't I tell you already for not using this commands?!",
                    "It's midnight already, you probably should go to sleep with me",
                    "Beware of the Sp00ky Aiko",
                    "Don't look over behind...",
                    "Hello, please don't look at me...",
                    "I'm right behind you...",
                    "Why are you insisting to use this commands?",
                    "Don't worry, I'll be right behind you...",
                    "Do you wanna know how did I get these eyes?",
                    "There is someone... lurking behind you...",
                    "Aiko cannot be found...",
                    "....","Pretty...witchy..aiko...chi...",
                    "Please make me some Takoyaki...",
                    "Let's be my friend, will you?"
                };

                string[] arrRandomAuthor = {
                    "4ik00","Aik0","a.i.k.o.e.x.e","too much Ai-k0",
                    "m1ssing A1k0","seno.exe","s3n0.ex3","4k10 s3n0o",
                    "th3 A1k0","Doppel Aiko","4ik0 s3n0","A1k0","witchy.exe",
                    "4170 seno","s3n00 A1k0","a.i.k.o","a i k o","s e n o",
                    "aiko.exe","Sp00kiko","41k0","Takoyaki Girl",
                    "T4k0y4k1","Takoyaki.exe","a.i.k.0.e.x....e","aaaaaaiiiiiikkkkoooo",
                    "aaaiii1ikk00","4ikkkkkkkk0000","Aaiikk00.exe","Blue.exe",
                    "A1k0000","01100001 01101001","A1kk000","Pretty...witchy..aiko...chi..."
                };

                string[,] arrRandom = {
                {"Odd Meat","https://cdn.discordapp.com/attachments/569409307100315651/653676655294021643/daikon_9.png"},
                {"Letter Three","https://media.discordapp.net/attachments/653690054912507914/658004378732724234/unknown.png"},
                {"вештица","https://cdn.discordapp.com/attachments/569409307100315651/654463722940792855/wowspoop.gif"},
                {"Letter Three","https://media.discordapp.net/attachments/653690054912507914/658004103854817290/Spooky_Aiko.png"},
                {"Letter Three","https://cdn.discordapp.com/attachments/643722270447239169/669150355526778880/unknown.png"},
                {"Letter Three","https://cdn.discordapp.com/attachments/643722270447239169/669150430940495872/unknown.png"},
                {"Odd Meat","https://cdn.discordapp.com/attachments/643722270447239169/669225508441161758/spooks_orig.png"},
                {"Nathan","https://cdn.discordapp.com/attachments/643722270447239169/669597882114113558/20200122_124002.jpg"},
            };

                int randomedResult = new Random().Next(0, arrRandom.GetLength(0));
                await base.ReplyAsync(embed: new EmbedBuilder()
                    .WithAuthor(arrRandomAuthor[new Random().Next(0, arrRandomAuthor.Length)], arrRandom[randomedResult, 1])
                    .WithDescription(arrSentences[new Random().Next(0, arrSentences.Length)])
                    .WithColor(Color.DarkerGrey)
                    .WithImageUrl(arrRandom[randomedResult, 1])
                    .WithFooter($"Contributed by: {arrRandom[randomedResult, 0]}")
                    .Build());
            }
            

            
        }

        [Command("stats"), Alias("bio"), Summary("I will show you my biography info")]
        public async Task showStats()
        {
            await ReplyAsync("Pameruku raruku rarirori poppun! Show my biography info!",
            embed: new EmbedBuilder()
            .WithAuthor(Config.Aiko.EmbedName, Config.Aiko.EmbedAvatarUrl)
            .WithDescription("Aiko Senoo (妹尾あいこ, Senō Aiko) is one of the Main Characters and the third Ojamajo, officially joining the group with Hazuki Fujiwara after they spy on Doremi Harukaze in the Maho-do. " +
            "Aiko is from Tengachaya Osaka, who transferred to Misora with her father due to his work.Aiko is known for her distinctive Kansai - dialect and often complains when others imitate it.She has the Osaka comedy routine down and is a very smart bargainer.")
            .AddField("Full Name", "妹尾あいこ Senō Aiko", true)
            .AddField("Gender", "female", true)
            .AddField("Blood Type", "O", true)
            .AddField("Birthday", "November 14th, 1990", true)
            .AddField("Instrument", "Harmonica", true)
            .AddField("Favorite Food", "Takoyaki, Sweet Potato", true)
            .AddField("Debut", "[The Transfer Student from Naniwa! Aiko Debuts](https://ojamajowitchling.fandom.com/wiki/The_Transfer_Student_from_Naniwa!_Aiko_Debuts)", true)
            .WithColor(Config.Aiko.EmbedColor)
            .WithImageUrl("https://images-na.ssl-images-amazon.com/images/I/71gZQfA16AL._SY450_.jpg")
            .WithFooter("Source: [Ojamajo Witchling Wiki](https://ojamajowitchling.fandom.com/wiki/Aiko_Senoo)")
            .Build());
        }

        [Command("thank you"), Alias("thanks", "arigatou"), Summary("Say thank you to Aiko Bot")]
        public async Task thankYou([Remainder] string message = "")
        {
            await ReplyAsync($"Your welcome, {MentionUtils.MentionUser(Context.User.Id)}. I'm glad that you're happy with it :smile:");
        }

        [Command("turn"), Alias("transform"), Summary("Turn <username> into <wishes>")]
        public async Task spells(IUser username, [Remainder] string wishes)
        {
            //await Context.Message.DeleteAsync();
            await ReplyAsync($"Pameruku raruku rarirori poppun! Turn {username.Mention} into {wishes}",
            embed: new EmbedBuilder()
            .WithColor(Config.Aiko.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/4/46/Aiko-spell.gif")
            .Build());
        }

        [Command("wish"), Summary("I will grant you a <wishes>")]
        public async Task wish([Remainder] string wishes)
        {
            await ReplyAsync($"Pameruku raruku rarirori poppun! {wishes}");
            await base.ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Aiko.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/4/46/Aiko-spell.gif")
            .Build());
        }

    }

    [Summary("hidden")]
    public class AikoMagicalStageModule : ModuleBase
    {
        //magical stage section
        [Command("Paipai Ponpoi, Shinyaka ni!")] //magical stage from hazuki
        public async Task magicalStage()
        {
            if (Context.User.Id == Config.Hazuki.Id)
                await ReplyAsync($"{MentionUtils.MentionUser(Config.Onpu.Id)} Pameruku raruku, Takaraka ni!",
                embed: new EmbedBuilder()
                .WithColor(Config.Aiko.EmbedColor)
                .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/bb/MagicalStageMottoAiko.png")
                .Build());

        }

        [Command("Magical Stage!")]//Final magical stage: from hazuki
        public async Task magicalStagefinal([Remainder] string query)
        {
            if (Context.User.Id == Config.Hazuki.Id)
            {
                await ReplyAsync($"{MentionUtils.MentionUser(Config.Onpu.Id)} Magical Stage! {query}\n");
            }
        }
    }

    [Summary("hidden")]
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
