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

                    if (ctrFounded >= 1)
                    {
                        output.Description = $"I found {ctrFounded} command(s) with **{CategoryOrCommands}** keyword:";
                        await ReplyAsync("", embed: output.Build());
                        return;
                    }
                    else
                    {
                        await ReplyAsync($"Oops, I can't find any related help that you search for. " +
                            $"See `{Config.Aiko.PrefixParent[0]}help <optional category>`for command help.");
                        return;
                    }

                }
            }

        }

        public void HelpDetails(ref EmbedBuilder builder, string category, string summary,
            string alias, string group, string commands, string parameters)
        {
            var completedText = ""; commands = commands.ToLower();
            if (summary != "") completedText += $"{summary}\n";
            completedText += $"**Category:** {category.ToLower()}\n";
            if (alias != "") completedText += $"**Alias:** {alias}\n";
            if (group != "") commands += " ";
            completedText += $"**Example:** `{Config.Aiko.PrefixParent[0]}{group}{commands}";
            if (parameters != "") completedText += parameters;
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
                        output.Append($"[opt {param.Name}:{param.DefaultValue}]");
                    else
                        output.Append($"[opt:{param.Name}]");
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
                        output.Append($"[opt {param.Name}:{param.DefaultValue}]");
                    else
                        output.Append($"[opt:{param.Name}]");
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

        [Command("change"), Alias("henshin"), Summary("Change into the ojamajo form")]
        public async Task transform()
        {
            await ReplyAsync("Pretty Witchy Aiko Chi~\n");
            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Aiko.EmbedColor)
                .WithImageUrl("https://66.media.tumblr.com/13cf3226a3c5b77a2100cd121de61eb7/tumblr_nnf0ckQlnh1usz98wo1_250.gif")
                .Build());
        }

        [Command("fairy"), Summary("I will show you my fairy")]
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
            String[] arrRandom = {
                "This is not my final form!",
                "Pameruku raruku rarirori poppun! Turn me into gigantamax!",
                "Meet the gigantamax Aiko!",
                "Aiko has been gigantamaxed!",
                "Gigantamax Aiko ready for action!",
                "Muahaha! I have been gigantamax-ed!"
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
                    $"Hello there {MentionUtils.MentionUser(Context.User.Id)}! ",
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

        [Command("random"), Alias("moments"), Summary("Show any random Aiko moments")]
        public async Task randommoments()
        {
            string[] arrRandom =
            {"https://38.media.tumblr.com/224f6ca12018eca4ff34895cce9b7649/tumblr_nds3eyKFLH1r98a5go1_500.gif",
            "https://yt3.ggpht.com/a/AGF-l7-JcB38-lOhu0HzFN5NsWre0wgnl50IeIZq8Q=s900-c-k-c0xffffffff-no-rj-mo",
            "https://thumbs.gfycat.com/EntireGrizzledFlyinglemur-poster.jpg",
            "https://media.discordapp.net/attachments/569409307100315651/651188503991812107/1564810493598.jpg?width=829&height=622",
            "https://66.media.tumblr.com/f6b42eb806ae7b64fc34e6e8b1a18c3f/tumblr_inline_mgcb5odip41r4lv3u.gif",
            "https://pbs.twimg.com/media/EOWSqgPX4AAF7g5?format=png&name=small","https://pbs.twimg.com/media/EOL7w1GXUAUXwmE?format=png&name=small",
            "https://pbs.twimg.com/media/EOSGlULW4AAUUGB?format=png&name=small","https://pbs.twimg.com/media/EOAvJH5XUAAyso4?format=png&name=small",
            "https://pbs.twimg.com/media/EONImyFXUAIwmKw?format=png&name=small","https://pbs.twimg.com/media/EN_hqAQX0AA1SJf?format=png&name=small",
            "https://pbs.twimg.com/media/EOI6pfmX0AA4BPN?format=png&name=small","https://pbs.twimg.com/media/EN-aX9OX4AErhax?format=png&name=small",
            "https://pbs.twimg.com/media/EN-G2YgX0AAk499?format=png&name=small","https://pbs.twimg.com/media/EN9so25WoAAhMxA?format=png&name=small",
            "https://pbs.twimg.com/media/EN9klu-WsAA9h23?format=png&name=small","https://pbs.twimg.com/media/EN9H27kW4AETemw?format=png&name=small",
            "https://pbs.twimg.com/media/EN8jSnnWoAEArR1?format=png&name=small","https://pbs.twimg.com/media/EN8R0pHWsAAUAhh?format=png&name=small",
            "https://pbs.twimg.com/media/EN7PUhkXUAA5wh_?format=png&name=small","https://pbs.twimg.com/media/EN5-Z4JX4AE_Kvm?format=png&name=small",
            "https://pbs.twimg.com/media/EN24osAW4AYXQIr?format=png&name=small","https://pbs.twimg.com/media/EN1FeXCX4AAARzI?format=png&name=small",
            "https://pbs.twimg.com/media/EN0pNQvX0AAhYK1?format=png&name=small","https://pbs.twimg.com/media/ENzf2FjWoAAa6dr?format=png&name=small",
            "https://pbs.twimg.com/media/ENywQseXkAAZORC?format=png&name=small","https://pbs.twimg.com/media/ENyCSNTWoAAupEh?format=png&name=small",
            "https://pbs.twimg.com/media/ENx5UcsWwAA1lcX?format=png&name=small","https://pbs.twimg.com/media/ENxdgCqX0AEWDvm?format=png&name=small",
            "https://pbs.twimg.com/media/ENxCWETX0AETqVT?format=png&name=small","https://pbs.twimg.com/media/ENvwuFNWoAAiX_X?format=png&name=small",
            "https://pbs.twimg.com/media/ENup42BWoAAALrL?format=png&name=small","https://pbs.twimg.com/media/ENt_lzDX0AAbJQY?format=png&name=small",
            "https://pbs.twimg.com/media/ENtLLZxXsAAIhsn?format=png&name=small",
            "https://pbs.twimg.com/media/ENsyGt6WoAAnl1O?format=png&name=small","https://pbs.twimg.com/media/ENsAMTCUUAUeifE?format=png&name=small",
            "https://pbs.twimg.com/media/ENq0jyBWoAE-LOl?format=png&name=small","https://pbs.twimg.com/media/ENqqrQ4XYAIoJ3U?format=png&name=small",
            "https://pbs.twimg.com/media/ENp9XiNX0AAZ8t9?format=png&name=small","https://pbs.twimg.com/media/ENpRsDsWsAABTxZ?format=png&name=small",
            "https://pbs.twimg.com/media/ENon0itXsAEgp_B?format=png&name=small","https://pbs.twimg.com/media/ENmQ_wQUUAAVvlR?format=png&name=small",
            "https://pbs.twimg.com/media/ENk1RBsWsAEF_kK?format=png&name=small","https://pbs.twimg.com/media/ENkPW8HXYAE4Jpa?format=png&name=small",
            "https://pbs.twimg.com/media/ENjMNHQXkAEkga6?format=png&name=small","https://pbs.twimg.com/media/ENhTvnPVAAAxkl0?format=png&name=small",
            "https://pbs.twimg.com/media/ENg-1VGXkAAHXIx?format=png&name=small","https://pbs.twimg.com/media/ENgoScqX0AAotuK?format=png&name=small",
            "https://pbs.twimg.com/media/ENgfFwnWsAABofj?format=png&name=small","https://pbs.twimg.com/media/ENcYzFOWoAAyxvG?format=png&name=small",
            "https://pbs.twimg.com/media/ENbwEzpWwAEhzaa?format=png&name=small","https://pbs.twimg.com/media/ENbKWP2WwAE2hUy?format=png&name=small",
            "https://pbs.twimg.com/media/ENa44FCXYAEdfOJ?format=png&name=small","https://pbs.twimg.com/media/ENacmC-W4AAxFOm?format=png&name=small",
            "https://pbs.twimg.com/media/ENZeRyjW4AIe-_1?format=png&name=small","https://pbs.twimg.com/media/ENZL4vHXYAMDnS9?format=png&name=small",
            "https://pbs.twimg.com/media/ENZCdxnXYAAO2o_?format=png&name=small","https://pbs.twimg.com/media/ENX1ZQFXkAARxKE?format=png&name=small",
            "https://pbs.twimg.com/media/ENW1sxwXUAILM_i?format=png&name=small","https://pbs.twimg.com/media/ENWH8D7WwAESuoe?format=png&name=small",
            "https://pbs.twimg.com/media/ENV1UvEW4AAR0A0?format=png&name=small","https://pbs.twimg.com/media/ENVTRWNXkAEV60U?format=png&name=small",
            "https://pbs.twimg.com/media/ENVItRGWwAATIr-?format=png&name=small","https://pbs.twimg.com/media/ENTOJASWoAEGzz8?format=png&name=small",
            "https://pbs.twimg.com/media/ENSBjGNXsAEHRoX?format=png&name=small","https://pbs.twimg.com/media/ENPZtiIX0AIHiUs?format=png&name=small",
            "https://pbs.twimg.com/media/ENPIAq3UcAINjmv?format=png&name=small","https://pbs.twimg.com/media/ENO3rh8WoAEugIk?format=png&name=small",
            "https://pbs.twimg.com/media/ENODBRcXYAEMmsf?format=png&name=small","https://pbs.twimg.com/media/ENKdy17WoAAkANp?format=png&name=small",
            "https://pbs.twimg.com/media/ENJtDOfWsAUx4VR?format=png&name=small","https://pbs.twimg.com/media/ENGBY_bXkAAHehk?format=png&name=small",
            "https://pbs.twimg.com/media/ENF3DALWwAAHOOq?format=png&name=small","https://pbs.twimg.com/media/ENFtKEZWkAAIney?format=png&name=small",
            "https://pbs.twimg.com/media/ENC0SvGWkAUiF5y?format=png&name=small","https://pbs.twimg.com/media/ENB_pKEXsAAFg9P?format=png&name=small",
            "https://pbs.twimg.com/media/EM_WLxdXUAA5SQ5?format=png&name=small","https://pbs.twimg.com/media/EM-x3viXkAIN29a?format=png&name=small",
            "https://pbs.twimg.com/media/EM9A7zwWsAITihn?format=png&name=small","https://pbs.twimg.com/media/EM7vjOGXUAEejP_?format=png&name=small",
            "https://pbs.twimg.com/media/EM6qHQiWkAE_FyF?format=png&name=small","https://pbs.twimg.com/media/EM59S_nWoAEUFCr?format=png&name=small",
            "https://pbs.twimg.com/media/EM50Uv0WkAAtCqd?format=png&name=small","https://pbs.twimg.com/media/EM4zsomWkAAT7CZ?format=png&name=small",
            "https://pbs.twimg.com/media/EM4h_h3WkAE-6n_?format=png&name=small",
            };


            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Aiko.EmbedColor)
                .WithImageUrl(arrRandom[new Random().Next(0, arrRandom.Length)])
                .WithFooter("Some images contributed by: https://twitter.com/DoremiRobo, Tumblr")
                .Build());
        }
        
        [Command("spooky"), Alias("creepy", "spook"), Summary("Please don't use this commands...")]
        public async Task showSpookyAiko()
        {
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
                "....",
                "Please make me some Takoyaki...",
                "Get me some Takoyaki..."
            };

            string[] arrRandomAuthor =
            {
                "4ik00","Aik0","a.i.k.o.e.x.e","too much Ai-k0",
                "m1ssing A1k0","seno.exe","s3n0.ex3","a=k1 s3n0o",
                "th3 A1k0","The twinAiko","4ik0 s3n0","A1k0",
                "4170 seno","s3n00 A1k0","a.i.k.o","a i k o","s e n o",
                "the l0st girl","M1ssing Author","The lost girl","G1rl.3xe",
                "aiko.exe","Sp00kiko","The cult of Aiko","41k0","Takoyaki Girl",
                "T4k0y4k1","Takoyaki","ai ko .e   .e .x ....e","aaaaaaiiiiiikkkkoooo",
                "aaaiiiiiiiiiikkk0000","4ikkkkkkkk0000","Aaaaaiiiik 000.exe","Blue.exe",
                "A1k0000","010101010101 0000000","A1kk000"
            };

            string[,] arrRandom = {
                {"Odd Meat","https://cdn.discordapp.com/attachments/569409307100315651/653676655294021643/daikon_9.png"},
                {"вештица","https://cdn.discordapp.com/attachments/569409307100315651/654463722940792855/wowspoop.gif"}
            };

            int randomedResult = new Random().Next(0, arrRandom.GetLength(0));
            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithAuthor(arrRandomAuthor[new Random().Next(0, arrRandomAuthor.Length)], arrRandom[randomedResult, 1])
                .WithDescription(arrSentences[new Random().Next(0,arrSentences.Length)])
                .WithColor(Color.DarkerGrey)
                .WithImageUrl(arrRandom[randomedResult, 1])
                .WithFooter($"Contributed by: {arrRandom[randomedResult, 0]}")
                .Build());
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
            .WithImageUrl("https://i.pinimg.com/originals/79/14/40/7914406b1876370c3058d8b8f14de96e.jpg")
            .Build());
        }

        [Command("wish"), Summary("I will grant you a <wishes>")]
        public async Task wish([Remainder] string wishes)
        {
            await ReplyAsync($"Pameruku raruku rarirori poppun! {wishes}");
            await base.ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Aiko.EmbedColor)
            .WithImageUrl("https://i.pinimg.com/originals/79/14/40/7914406b1876370c3058d8b8f14de96e.jpg")
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
                await ReplyAsync($"{MentionUtils.MentionUser(Config.Doremi.Id)} Pameruku raruku, Takaraka ni! \n",
                    embed: new EmbedBuilder()
                    .WithColor(Config.Aiko.EmbedColor)
                    .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e9/Takarakanis1.2.png/revision/latest?cb=20190408124952")
                    .Build());

        }

        [Command("Magical Stage!")]//Final magical stage: from hazuki
        public async Task magicalStagefinal([Remainder] string query)
        {
            if (Context.User.Id == Config.Hazuki.Id)
            {
                await ReplyAsync($"Magical Stage! {query}\n",
                embed: new EmbedBuilder()
                .WithColor(Config.Aiko.EmbedColor)
                .WithImageUrl("https://i.ytimg.com/vi/HyizF7XWfU8/maxresdefault.jpg")
                .Build());
                Config.Doremi.MagicalStageWishes = "";//erase the last magical stage wishes
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
