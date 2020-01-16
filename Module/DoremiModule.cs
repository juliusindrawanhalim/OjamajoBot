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
using Victoria;
using Victoria.Enums;

namespace OjamajoBot.Module
{
    [Name("General")]
    class DoremiModule : ModuleBase<SocketCommandContext>
    {
        //start
        private readonly CommandService _commands;
        private readonly IServiceProvider _map;

        public DoremiModule(IServiceProvider map, CommandService commands)
        {
            _commands = commands;
            _map = map;
        }

        [Name("help"),Command("help"), Summary("Show all Doremi bot Commands.")]
        public async Task Help([Remainder]string CategoryOrCommands = "")
        {
            EmbedBuilder output = new EmbedBuilder();
            output.Color = Config.Doremi.EmbedColor;

            if (CategoryOrCommands == "")
            {
                output.WithAuthor(Config.Doremi.EmbedName, Config.Doremi.EmbedAvatarUrl);
                output.Title = $"Command List";
                output.Description = "Pretty Witchy Doremi Chi~ You can tell me what to do with " +
                    $"**{Config.Doremi.PrefixParent[2]} or {Config.Doremi.PrefixParent[0]} or {Config.Doremi.PrefixParent[1]}** as starting prefix.\n" +
                    $"Use **{Config.Doremi.PrefixParent[0]}help <category or commands>** for more help details.\n" +
                    $"Example: **{Config.Doremi.PrefixParent[0]}help general** or **{Config.Doremi.PrefixParent[0]}help hello**";

                foreach (var mod in _commands.Modules.Where(m => m.Parent == null))
                {
                    AddHelp(mod, ref output);
                }
                await ReplyAsync("", embed: output.Build());
                return;
            } else {
                var mod = _commands.Modules.FirstOrDefault(m => m.Name.Replace("Module", "").ToLower() == CategoryOrCommands.ToLower());
                if (mod != null) {
                    var before = mod.Name;
                    output.Title = $"{char.ToUpper(before.First()) + before.Substring(1).ToLower()} Commands";
                    output.Description = $"{mod.Summary}\n" +
                    (!string.IsNullOrEmpty(mod.Remarks) ? $"({mod.Remarks})\n" : "");
                    //(mod.Submodules.Any() ? $"Submodules: {mod.Submodules.Select(m => m.Name)}\n" : "") + " ";
                    AddCommands(mod, ref output);
                    await ReplyAsync("", embed: output.Build());
                    return;
                } else { //search for category/child
                    int ctrFounded = 0;
                    var commandsModulesToList = _commands.Modules.ToList();
                    for (var i = 0;i< commandsModulesToList.Count;i++) {
                        for(var j = 0; j < commandsModulesToList[i].Commands.Count; j++)
                        {
                            if ((commandsModulesToList[i].Commands[j].Name.ToLower()==CategoryOrCommands.ToLower()||
                                commandsModulesToList[i].Commands[j].Aliases.Contains(CategoryOrCommands.ToLower()))&&
                                commandsModulesToList[i].Summary!="hidden")
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

                    if (ctrFounded>=1){
                        output.Description = $"I found {ctrFounded} command(s) with **{CategoryOrCommands}** keyword:";
                        await ReplyAsync("", embed: output.Build());
                        return;
                    } else {
                        await ReplyAsync($"Oops, I can't find any related help that you search for. See `{Config.Doremi.PrefixParent[0]}help` for more help info. ");
                        return;
                    }
                    

                    for (var i = 0;i< commandsModulesToList.Count; i++)
                    {
                        //mod = _commands.Modules.FirstOrDefault(m => m.Name.Replace("Module", "").ToLower() ==
                        //commandsModulesToList[i].Name.Replace("Module", "").ToLower());

                        if (mod != null)
                        {
                            Console.WriteLine(mod.Name);
                            getAllCommands(mod, ref output, CategoryOrCommands);
                            await ReplyAsync("", embed: output.Build());
                        } else
                        {
                            getAllCommands(mod, ref output, CategoryOrCommands);
                            await ReplyAsync("empty commands", embed: output.Build());
                        }
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
            completedText += $"**Example:** `{Config.Doremi.PrefixParent[0]}{group}{commands}";
            if (parameters != "") completedText += parameters;
            completedText += "`\n";
            builder.AddField(commands,completedText);
        }

        public void AddHelp(ModuleInfo module, ref EmbedBuilder builder)
        {
            foreach (var sub in module.Submodules) AddHelp(sub, ref builder);

            if (module.Summary!="hidden")
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
                AddCommand(command, ref builder,commandDetails);
            }
        }

        public void AddCommand(CommandInfo command, ref EmbedBuilder builder, string commandDetails="")
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
            for(int i=1;i<alias.Count;i++)
            {
                output+=($" `{alias[i].ToString()}`,");
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
                output = Config.Doremi.PrefixParent[0];
            }
            output = Config.Doremi.PrefixParent[0];
            return output;
        }


        //end

        //[Command("Help")]
        //public async Task showHelp()
        //{
        //    await ReplyAsync(embed: new EmbedBuilder()
        //    .WithColor(Config.Doremi.EmbedColor)
        //    .WithAuthor(Config.Doremi.EmbedName, Config.Doremi.EmbedAvatarUrl)
        //    .WithTitle("Command List:")
        //    .WithDescription($"Pretty Witchy Doremi Chi~ " +
        //    $"You can tell me what to do with {MentionUtils.MentionUser(Config.Doremi.Id)} or **doremi!** or **do!** as the starting command prefix. " +
        //    $"**<opt:>** parameter means is optional")
        //    .AddField("Basic Commands",
        //    "**change** or **henshin** : Change into the ojamajo form\n" +
        //    "**dorememe** or **dorememes <opt:username>** : I will give you some random doremi memes\n" +
        //    //"**contribute <dorememe or dorememes>** <file> : Submit an image attachment to dorememe commands \n" +
        //    //"**episode** or **episodes** <season 1/2/3/4/5> <episodes> : I will give you the episodes info & summary\n" +
        //    "**fairy** : I will show you my fairy\n" +
        //    //"**feedback <feedback message>** : I will save your <feedback message> and try to improve with it\n"+
        //    "**hello** : Hello, I will greet you up\n" +
        //    "**hug <opt:username>** : I will give warm hug for you or <username>\n" +
        //    "**invite** : Generate the invitation links for related ojamajo bot\n"+
        //    "**magicalstage** or **magical stage** **<wishes>**: I will perform magical stage along with the other and make a <wishes>\n" +
        //    "**meme** or **memes** : I will give you some random memes\n" +
        //    "**quiz** : I will give you some quiz. Think you can answer them all?\n" +
        //    //"**quiz episodes** : I will give you a quiz about episodes\n" +
        //    "**quotes** : Mention any random quotes\n" +
        //    "**random** or **moments** : Show any random Doremi moments\n" +
        //    "**stats** or **bio** : I will show you my biography info\n" +
        //    "**star <message or attachment>** : I will pin this messsages on 10 stars reaction\n" +
        //    $"**steak**: Show any random steak moments {Config.Emoji.drool}{Config.Emoji.steak}\n" +
        //    "**turn** or **transform <username> <wishes>** : Turn <username> into <wishes>\n" +
        //    "**wish <wishes>** : I will grant you a <wishes>")
        //    //"**musicrepeat** or **murep** <Off/One/All> : Toggle the Music Repeat State based on the <parameter>\n" +
        //    .AddField("Musical Commands [On testing, some bugs might be founded]",
        //    "**join** : I will join to your connected voice channel (Please join any voice channel first)\n" +
        //    "**musiclist** or **mulist** : Show the doremi music list\n" +
        //    "**play <track number or title>** : Play the music with the given <track number or title> parameter\n" +
        //    "**playall** : Play all the music that's available on doremi music list\n" +
        //    "**queue** or **muq** : Show all the music that's currently on queue list\n" +
        //    "**seek <timespan>** : Seek the music into the given <timespan>[hh:mm:ss]\n" +
        //    "**skip** : Skip the music\n" +
        //    "**stop** : Stop playing the music. This will also clear the queue list\n" +
        //    "**youtube** or **yt <keyword or url>** : Play the youtube music either it's from keyword/url")
        //    .AddField("Moderator Commands [``Manage Channels`` permissions])",
        //    "**mod help** : This will display all basic moderator commands list")
        //    .Build());
        //}

        ///*quiz episodes
        //[Command("episodes", RunMode = RunMode.Async)]
        //public async Task showEpisodesInfo()
        //{
        //    string[,] arrRandomSeason1 = {
        //        {"1","I'm Doremi! Becoming a Witch","https://vignette.wikia.nocookie.net/ojamajowitchling/images/2/2e/OD-EP1-31.png/revision/latest?cb=20191013140106"},
        //        {"2","I Become Hazuki-chan","https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/ab/OD-EP2-01.png/revision/latest?cb=20191014181543"},
        //        {"3","The Transfer Student from Naniwa! Aiko Debuts","https://vignette.wikia.nocookie.net/ojamajowitchling/images/7/76/OD-EP3-01.png/revision/latest?cb=20191020221637"},
        //        {"4","It's Not Scary if We're All Witches","https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b5/OD-EP4-01.png/revision/latest?cb=20191021160947"},
        //        {"5","Grand Opening! Maho-dou","https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/56/OD-EP5-01.png/revision/latest?cb=20191028122639"},
        //        {"6","A Liar's First Friendship","https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/a2/OD-EP6-01.png/revision/latest?cb=20191103230145"},
        //        {"7","Go to the Witch World!!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/1/11/OD-EP8-01.png/revision/latest?cb=20191104092436"},
        //        {"8","Go to the Witch World!!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/1/11/OD-EP8-01.png/revision/latest?cb=20191104092436"},
        //        {"11","Early Bird Marina and a Bouquet From the Heart","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e5/OD-EP11-01.png/revision/latest?cb=20191106191355"},
        //        {"12","A Wish for a Precious Shirt","https://vignette.wikia.nocookie.net/ojamajowitchling/images/6/63/OD-EP12-01.png/revision/latest?cb=20191107002353"},
        //        {"14","Laugh and Forgive Me!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/8e/OD-EP14-01.png/revision/latest?cb=20191107104808"},
        //        {"15","Majo Rika Goes to Kindergarten","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e9/OD-EP15-01.png/revision/latest?cb=20191107231142"},
        //        {"16","Fishing for Love","https://vignette.wikia.nocookie.net/ojamajowitchling/images/0/07/OD-EP16-01.png/revision/latest?cb=20191109124447"},
        //        {"17","Yada-kun is a Delinquent!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/f/f3/OD-EP17-01.png/revision/latest?cb=20191109135953"},
        //        {"18","Don't Use That! The Forbidden Magic","https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/34/OD-EP18-01.png/revision/latest?cb=20171021190450"},
        //        {"19","Hazuki-chan is Kidnapped!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/5d/OD-EP19-01.png/revision/latest?cb=20191113144549"},
        //        {"20","The Rival Debuts! The Maho-dou is in Big Trouble!!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/39/OD-EP20-01.png/revision/latest?cb=20191113182510"},
        //        {"21","Majoruka's Goods are full of danger!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/1/12/OD-EP21-01.png/revision/latest?cb=20191113215530"},
        //        {"22","The Road to being a level 6 Witch is Hard","https://vignette.wikia.nocookie.net/ojamajowitchling/images/f/f4/OD-EP22-01.png/revision/latest?cb=20191113223335"},
        //        {"23","Big Change! The Ojamajo's Test","https://vignette.wikia.nocookie.net/ojamajowitchling/images/4/4d/OD-EP23-01.png/revision/latest?cb=20191116123630"},
        //        {"24","Majoruka versus level 6 ojamajo!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/1/13/OD-EP24-01.png/revision/latest?cb=20191116140500"},
        //        {"25","Ojamajo Poppu appears!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/d/dc/OD-EP25-01.png/revision/latest?cb=20191116143237"},
        //        {"27","Oyajide arrives?!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/ee/OD-EP27-01.png/revision/latest?cb=20191116223858"},
        //        {"28","Love is a Windy Ride over a Plateau","https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/58/OD-EP28-01.png/revision/latest?cb=20191116234149"},
        //        {"29","The Tap Disappeared at the Festival!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/6/61/OD-EP29-01.png/revision/latest?cb=20191117003019"},
        //        {"30","I want to meet the ghost!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/f/f2/OD-EP30-01.png/revision/latest?cb=20191117011838"},
        //        {"31","Present from Mongolia","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e4/OD-EP31-01.png/revision/latest?cb=20191117102136"},
        //        {"32","Overthrow Tamaki! the class election!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/6/68/OD-EP32-01.png/revision/latest?cb=20191117105859"},
        //        {"33","Panic at the Sports Festival","https://vignette.wikia.nocookie.net/ojamajowitchling/images/6/65/OD-EP33-01.png/revision/latest?cb=20191117171421"},
        //        {"34","I want to see my Mother!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/85/OD-EP34-01.png/revision/latest?cb=20191117174457"},
        //        {"35","The Transfer student is a Witch Apprentice?!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/0/0a/OD-EP35-01.png/revision/latest?cb=20191117183318"},
        //        {"36","Level four exam is Dododododo!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/59/OD-EP36-01.png/revision/latest?cb=20191117213655"},
        //        {"38","Ryota and the Midnight Monster","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e3/OD-EP38-01.png/revision/latest?cb=20191118104422"},
        //        {"41","Father and Son, the Move Towards Victory!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/4/43/OD-EP41-01.png/revision/latest?cb=20171021190911"},
        //        {"42","The Ojamajo's Fight for Justice!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/c/c2/OD-EP42-01.png/revision/latest?cb=20191118193414"},
        //        {"43","Papa, Fireworks, and Tearful Memories","https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/a1/OD-EP43-01.png/revision/latest?cb=20191118200017"},
        //        {"44","I Want to Be a Female Pro Wrestler!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/0/08/OD-EP44-01.png/revision/latest?cb=20191118205141"},
        //        {"45","Help Santa!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/96/OD-EP45-01.png/revision/latest?cb=20191118205624"},
        //        {"46","The Witches' Talent Show","https://vignette.wikia.nocookie.net/ojamajowitchling/images/6/61/OD-EP46-01.png/revision/latest?cb=20191119122536"},
        //        {"47","Fathers Arranged Marriage Meeting","https://vignette.wikia.nocookie.net/ojamajowitchling/images/0/0c/OD-EP47-01.png/revision/latest?cb=20191119122558"},
        //        {"48","Onpu's Mail is a Love Letter?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/0/07/OD-EP48-01.png/revision/latest?cb=20191120210149"},
        //        {"49","I Want to Meet Papa! The Dream Places on the Overnight Express","https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b9/OD-EP49-01.png/revision/latest?cb=20191120212755"},
        //        {"50","The Final Witch Apprentice Exam","https://vignette.wikia.nocookie.net/ojamajowitchling/images/c/c3/OD-EP50-01.png/revision/latest?cb=20191120231046"},
        //        {"51","Goodbye Maho-Dou","https://vignette.wikia.nocookie.net/ojamajowitchling/images/c/ce/OD-EP51-01.png/revision/latest?cb=20191120231059"},
        //    };
        //    string[,] arrRandomSeason2 = {
        //        {"1","Doremi Becomes a Mom!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/ae/ODS-EP1-001.png/revision/latest?cb=20191122221644"},
        //        {"2","Raising a Baby is a Lot of Trouble!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/83/ODS-EP2-001.png/revision/latest?cb=20191124140100"},
        //        {"3","Don't Fall Asleep! Pop's Witch Apprentice Exam","https://vignette.wikia.nocookie.net/ojamajowitchling/images/4/43/ODS-EP3-001.png/revision/latest?cb=20191124212633"},
        //        {"4","Doremi Fails as a Mom!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/7/7e/ODS-EP4-001.png/revision/latest?cb=20191124235204"},
        //        {"5","So Long, Oyajiide","https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/8e/ODS-EP5-001.png/revision/latest?cb=20191125102137"},
        //        {"6","Lies and Truth in Flower Language","https://vignette.wikia.nocookie.net/ojamajowitchling/images/4/43/ODS-EP6-001.png/revision/latest?cb=20191125213656"},
        //        {"7","Hana-chan's Health Examination","https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/5d/ODS-EP7-001.png/revision/latest?cb=20191125221342"},
        //        {"8","Across Time, In Search of Onpu's Moms Secret!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/1/12/ODS-EP8-001.png/revision/latest?cb=20191130001508"},
        //        {"9","The Search for the Herbs! Maho-dou's Bus Trip","https://vignette.wikia.nocookie.net/ojamajowitchling/images/d/d1/ODS-EP9-001.png/revision/latest?cb=20191201104930"},
        //        {"11","Hazuki-chan Learns how to Dance!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b4/ODS-EP11-001.png/revision/latest?cb=20191201200624"},
        //        {"12","The Health Examination's Yellow Cards!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b6/ODS-EP12-001.png/revision/latest?cb=20191202120414"},
        //        {"13","Doremi Becomes a Bride?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/80/ODS-EP13-001.png/revision/latest?cb=20191204222102"},
        //        {"14","Pop's First Love? Her Beloved Jyunichi-Sensei!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b9/ODS-EP14-001.png/revision/latest?cb=20191208134648"},
        //        {"15","Mother's Day and the Drawing of Mother","https://vignette.wikia.nocookie.net/ojamajowitchling/images/2/23/ODS-EP15-001.png/revision/latest?cb=20191209133630"},
        //        {"18","Dodo Runs Away From Home!!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/eb/ODS-EP18-001.png/revision/latest?cb=20191217185517"},
        //        {"19","Doremi and Hazuki's Big Fight","https://vignette.wikia.nocookie.net/ojamajowitchling/images/c/cc/ODS-EP19-001.png/revision/latest?cb=20191217220830"},
        //        {"21","The Misanthropist Majo Don and The Promise of The Herb","https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b9/ODS-EP21-001.png/revision/latest?cb=20191221130019"},
        //        {"22","The Wizard's Trap - Oyajide Returns","https://vignette.wikia.nocookie.net/ojamajowitchling/images/6/66/ODS-EP22-001.png/revision/latest?cb=20191222224016"},
        //        {"23","Using new powers to Rescue Hana-chan!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/0/07/ODS-EP23-001.png/revision/latest?cb=20191223102333"},
        //        {"24","Fried Bread Power is Scary!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/33/ODS-EP24-001.png/revision/latest?cb=20191229162205"},
        //        {"25","The Mysterious Pretty Boy, Akatsuki-kun Appears!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/0/0b/ODS-EP25-001.png/revision/latest?cb=20191230032553"},
        //        {"26","Kanae-chan's Diet Plan","https://vignette.wikia.nocookie.net/ojamajowitchling/images/f/fb/ODS-EP26-001.png/revision/latest?cb=20200101191420"},
        //        {"28","Health Examination Full of Hidden Dangers","https://vignette.wikia.nocookie.net/ojamajowitchling/images/d/db/ODS-EP28-001.png/revision/latest?cb=20200104231053"},
        //        {"29","Everyone Disappears During the Test of Courage!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/32/ODS-EP29-001.png/revision/latest?cb=20200105222029"},
        //        {"30","Seki-sensei's Got a Boyfriend!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/7/77/ODS-EP30-001.png/revision/latest?cb=20200105223509"},
        //        {"31","The FLAT 4 Arrive from the Wizard World!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/9b/ODS-EP31-001.png/revision/latest?cb=20200109000219"},
        //        {"32","Fly Away! Dodo and the Other Fairies' Big","https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/95/02.32.09.JPG/revision/latest?cb=20160104203250"},
        //        {"33","Say Cheese During the Class Trip!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/91/02.33.07.JPG/revision/latest?cb=20160104204330"},
        //        {"34","Takoyaki is the Taste of Making Up","https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/a4/02.33.06.JPG/revision/latest?cb=20160104203724"},
        //        {"36","Aiko and her Rival! Sports Showdown!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/31/02.36.06.JPG/revision/latest?cb=20160104204841"},
        //        {"38","Hazuki-chan's a Great Director!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/4/45/02.38.06.JPG/revision/latest?cb=20160104205546"},
        //        {"39","A Selfish Child and the Angry Monster","https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/ae/39.07.JPG/revision/latest?cb=20160104205811"},
        //        {"40","The Piano Comes to the Harukaze House!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/2/28/02.40.10.JPG/revision/latest?cb=20160104210153"},
        //        {"41","Chase after Onpu! The Path to Becoming an Idol!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/32/02.41.06.JPG/revision/latest?cb=20160104210830"},
        //        {"42","The Witch Who Does Not Cast Magic","https://vignette.wikia.nocookie.net/ojamajowitchling/images/7/7d/42.09.JPG/revision/latest?cb=20160104211048"},
        //        {"44","A Happy White Christmas","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e9/02.44.05.JPG/revision/latest?cb=20160104211626"},
        //        {"45","Ojamajo Era Drama: The Young Girls Show Their Valor!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/9b/02.45.08.JPG/revision/latest?cb=20160104211934"},
        //        {"46","The Last Examination - Hana-chan's Mom Will Protect Her!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/f/fe/02.46.09.JPG/revision/latest?cb=20160104212224"},
        //        {"47","Give Back Hana-chan! The Great Magic Battle","https://vignette.wikia.nocookie.net/ojamajowitchling/images/1/13/02.47.05.JPG/revision/latest?cb=20160104212503"},
        //        {"49","Good Bye, Hana-chan","https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/8f/49.16.JPG/revision/latest?cb=20160104213105"},
        //    };
        //    string[,] arrRandomSeason3 = {
        //        {"1","Doremi, a Stormy New Semester","https://vignette.wikia.nocookie.net/ojamajowitchling/images/4/44/Motto1-preop.png/revision/latest?cb=20171010213519"},
        //        {"2","Momoko Cried!? The Secret of the Earring","https://vignette.wikia.nocookie.net/ojamajowitchling/images/c/c7/02.15.JPG/revision/latest?cb=20151216152711"},
        //        {"3","I Hate You! But I Would Like To Be Your Friend!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/6/68/03.03.06.JPG/revision/latest?cb=20151216220704"},
        //        {"5","The SOS Trio is Disbanding!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/55/03.05.02.JPG/revision/latest?cb=20151216223354"},
        //        {"6","Challenge! The First Patissiere Exam","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/ea/06.10.JPG/revision/latest?cb=20151216231954"},
        //        {"8","What Are True Friends?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b9/03.08.07.JPG/revision/latest?cb=20151217022824"},
        //        {"9","Hazuki and Masaru's Treasure","https://vignette.wikia.nocookie.net/ojamajowitchling/images/7/7c/09.02.JPG/revision/latest?cb=20151128023340"},
        //        {"10","I Don't Want to Become an Adult!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b5/03.10.06.JPG/revision/latest?cb=20151218113031"},
        //        {"11","The Unstoppable Teacher!!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e4/03.11.06.JPG/revision/latest?cb=20151220124444"},
        //        {"12","Kotake VS Demon Coach Igarashi","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/ed/12.07.JPG/revision/latest?cb=20151220150403"},
        //        {"14","An Up and Down Happy Birthday","https://vignette.wikia.nocookie.net/ojamajowitchling/images/1/1b/14.07.JPG/revision/latest?cb=20151220152430"},
        //        {"16","Just Being Delicious is Not Enough!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/7/71/16.07.JPG/revision/latest?cb=20151220154907"},
        //        {"17","Her Destine Rival!! Harukaze and Tamaki","https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/99/03.17.09.JPG/revision/latest?cb=20151220160308"},
        //        {"18","Scoop! A Child Idol's Day","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/ec/18.09.JPG/revision/latest?cb=20151220162822"},
        //        {"19","Nothing but Fights, Like Father, Like Son","https://vignette.wikia.nocookie.net/ojamajowitchling/images/7/74/03.19.05.JPG/revision/latest?cb=20151221002953"},
        //        {"21","We're Out of Magical Ingredient","https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/a7/21.11.JPG/revision/latest?cb=20151221005022"},
        //        {"23","Clams By the Shore","https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/bd/03.23.050.JPG/revision/latest?cb=20151221010658"},
        //        {"24","Rock and Roll in the Music Club!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/7/76/03.24.06.JPG/revision/latest?cb=20151221011438"},
        //        {"25","A Lonely Summer Vacation","https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/ad/03.25.05.JPG/revision/latest?cb=20151221012136"},
        //        {"26","Deliver Her Feelings! Aiko Goes to Osaka","https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/ae/26.09.JPG/revision/latest?cb=20151221162214"},

        //    };
        //}
        //*/

        //[Command("help")]
        //public async Task showHelpDetail(string query)
        //{
        //    query = query.ToLower();
        //    switch (query)
        //    {
        //        case "A":
        //            await ReplyAsync();
        //            break;
        //    }
        //    //"**change** or **henshin** : Change into the ojamajo form\n" +
        //    //"**dorememe** or **dorememes <opt:username>** : I will give you some random doremi memes\n" +
        //    ////"**contribute <dorememe or dorememes>** <file> : Submit an image attachment to dorememe commands \n" +
        //    ////"**episode** or **episodes** <season 1/2/3/4/5> <episodes> : I will give you the episodes info & summary\n" +
        //    //"**fairy** : I will show you my fairy\n" +
        //    ////"**feedback <feedback message>** : I will save your <feedback message> and try to improve with it\n"+
        //    //"**hello** : Hello, I will greet you up\n" +
        //    //"**hug <opt:username>** : I will give warm hug for you or <username>\n" +
        //    //"**invite** : Generate the invitation links for related ojamajo bot\n" +
        //    //"**magicalstage** or **magical stage** **<wishes>**: I will perform magical stage along with the other and make a <wishes>\n" +
        //    //"**meme** or **memes** : I will give you some random memes\n" +
        //    //"**quiz** : I will give you some quiz. Think you can answer them all?\n" +
        //    ////"**quiz episodes** : I will give you a quiz about episodes\n" +
        //    //"**quotes** : Mention any random quotes\n" +
        //    //"**random** or **moments** : Show any random Doremi moments\n" +
        //    //"**stats** or **bio** : I will show you my biography info\n" +
        //    //"**star <message or attachment>** : I will pin this messsages on 10 stars reaction\n" +
        //    //$"**steak**: Show any random steak moments {Config.Emoji.drool}{Config.Emoji.steak}\n" +
        //    //"**turn** or **transform <username> <wishes>** : Turn <username> into <wishes>\n" +
        //    //"**wish <wishes>** : I will grant you a <wishes>")
        //    ////"**musicrepeat** or **murep** <Off/One/All> : Toggle the Music Repeat State based on the <parameter>\n" +
        //    //.AddField("Musical Commands [On testing, some bugs might be founded]",
        //    //"**join** : I will join to your connected voice channel (Please join any voice channel first)\n" +
        //    //"**musiclist** or **mulist** : Show the doremi music list\n" +
        //    //"**play <track number or title>** : Play the music with the given <track number or title> parameter\n" +
        //    //"**playall** : Play all the music that's available on doremi music list\n" +
        //    //"**queue** or **muq** : Show all the music that's currently on queue list\n" +
        //    //"**seek <timespan>** : Seek the music into the given <timespan>[hh:mm:ss]\n" +
        //    //"**skip** : Skip the music\n" +
        //    //"**stop** : Stop playing the music. This will also clear the queue list\n" +
        //    //"**youtube** or **yt <keyword or url>** : Play the youtube music either it's from keyword/url")
        //    //.AddField("Moderator Commands [``Manage Channels`` permissions])",
        //    //"**mod help** : This will display all basic moderator commands list"
        //}

        //[Command("help"), Summary("Gives information about a specific command")]
        //public async Task Help(string command)
        //{

        //    var result = commandService.Search(Context, command);
        //    if (!result.IsSuccess)
        //    {
        //        await ReplyAsync("Requested command invalid").ConfigureAwait(false);
        //        return;
        //    }

        //    var builder = new EmbedBuilder();
        //    foreach (var match in result.Commands)
        //    {
        //        var matchCommand = match.Command;

        //        builder
        //            .WithTitle($"Command: **{matchCommand.Name}**")
        //            .WithDescription($":white_small_square:  **Alias:**\t\t\t\t{string.Join(", ", matchCommand.Aliases)}\n"
        //                              + $":white_small_square:  **Usage:**  \t\t\t{"doremi!"}{matchCommand.Name} "
        //                              + $"{string.Join(" ", matchCommand.Parameters.Select(p => $"<{p.Name}>"))}\n"
        //                              + $":white_small_square:  **Description:**\t{matchCommand.Summary}\n")
        //            .WithColor(new Color(0xD8D45F));

        //        await ReplyAsync("", false, builder.Build()).ConfigureAwait(false);
        //    }
        //}

        //[Command("contribute"), Summary("I will save your contributed dorememes on 5 😝 reaction.")]
        //public async Task contributeDorememes([Remainder] string MessagesOrWithAttachment)
        //{
        //    try
        //    {
        //        var attachments = Context.Message.Attachments;

        //        WebClient myWebClient = new WebClient();

        //        string file = attachments.ElementAt(0).Filename;
        //        string url = attachments.ElementAt(0).Url;
        //        string extension = Path.GetExtension(attachments.ElementAt(0).Filename).ToLower();
        //        string randomedFileName = DateTime.Now.ToString("yyyyMMdd_HHmm") + new Random().Next(0, 10000) + extension;
        //        string completePath = $"attachments/{Context.Guild.Id}/{randomedFileName}";

        //        //if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif")
        //        //{
        //        // Download the resource and load the bytes into a buffer.
        //        byte[] buffer = myWebClient.DownloadData(url);
        //        Config.Core.ByteArrayToFile($"attachments/{Context.Guild.Id}/{randomedFileName}", buffer);

        //        await Context.Message.DeleteAsync();
        //        var sentMessage = await ReplyAsync($"{MentionUtils.MentionUser(Context.User.Id)} has a star messages:\n{MessagesOrWithAttachment}");
        //        var sentAttachment = await Context.Channel.SendFileAsync(completePath);
        //        await sentMessage.AddReactionAsync(new Emoji("\u2B50"));
        //        File.Delete(completePath);
        //        return;

        //    }
        //    catch (Exception e)
        //    {
        //        //Console.WriteLine(e.ToString());
        //    }

        //    await Context.Message.DeleteAsync();
        //    var sentWithoutAttached = await ReplyAsync($"{MentionUtils.MentionUser(Context.User.Id)} has a star messages:\n{MessagesOrWithAttachment}");
        //    await sentWithoutAttached.AddReactionAsync(new Emoji("\u2B50"));
        //}

        [Command("change"), Alias("henshin"), Summary("Change into the ojamajo form")]
        public async Task transform()
        {
            await ReplyAsync("Pretty Witchy Doremi Chi~\n");
            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl("https://media1.tenor.com/images/b99530648de9200b2cfaec83426f5482/tenor.gif")
                .Build());
        }

        [Command("dorememe"), Alias("dorememes"), Summary("I will give you some random doremi related memes")]
        public async Task givedorememe()
        {
            string[,] arrRandom =
            {
                {"imgflip","https://i.imgflip.com/1h9k61.jpg"},
                {"tumblr","https://66.media.tumblr.com/4b8ae988116282b0fbb86156006977a7/tumblr_ndl02pfvej1thwu0wo1_1280.png"},
                {"tumblr","https://66.media.tumblr.com/6143b1c1b6033c4cc068904909b68fbd/tumblr_n91u5yW35z1thwu0wo1_1280.png"},
                {"tumblr","https://66.media.tumblr.com/df6d13c7abe1970b4bc9726e5c264252/tumblr_n8ypyaubZl1thwu0wo1_1280.png"},
                {"tumblr","https://66.media.tumblr.com/1c00104523408517270a02f185208ff6/tumblr_n9iqy3L44d1thwu0wo1_1280.png"},
                {"tumblr","https://66.media.tumblr.com/ffad930ddacf0964646700523e80fb81/tumblr_n906n643rG1thwu0wo1_1280.png"},
                {"random","https://img1.ak.crunchyroll.com/i/spire4/1cd32824fff0e3be86cbd9f6c5b4cb2b1326942608_full.jpg"},
                {"tumblr","https://66.media.tumblr.com/9fdbbdc668507fa90c38bae8fa8d9f8a/tumblr_nvu6gqb6NE1thwu0wo1_1280.png"},
                {"random","https://images-wixmp-ed30a86b8c4ca887773594c2.wixmp.com/f/e90aeb60-6815-432d-bc4b-ad18ae885aaf/ddeph8m-bf0e2b8c-bc89-4e2f-b27c-f31d00d3c6cb.png/v1/fill/w_742,h_1077,q_70,strp/my_strawberry_shortcake_cast_meme__ojamajo_doremi__by_balloongal101_ddeph8m-pre.jpg?token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1cm46YXBwOjdlMGQxODg5ODIyNjQzNzNhNWYwZDQxNWVhMGQyNmUwIiwiaXNzIjoidXJuOmFwcDo3ZTBkMTg4OTgyMjY0MzczYTVmMGQ0MTVlYTBkMjZlMCIsIm9iaiI6W1t7ImhlaWdodCI6Ijw9MTQ4NSIsInBhdGgiOiJcL2ZcL2U5MGFlYjYwLTY4MTUtNDMyZC1iYzRiLWFkMThhZTg4NWFhZlwvZGRlcGg4bS1iZjBlMmI4Yy1iYzg5LTRlMmYtYjI3Yy1mMzFkMDBkM2M2Y2IucG5nIiwid2lkdGgiOiI8PTEwMjQifV1dLCJhdWQiOlsidXJuOnNlcnZpY2U6aW1hZ2Uub3BlcmF0aW9ucyJdfQ.YimkZYTzceBEJT3bYtwr3b0wsHrg2RGNou-a4uuLS6M"},
                {"ballmemes","https://pics.ballmemes.com/how-every-country-sees-magical-girl-anime-sailor-moon-ojamajo-44565702.png"},
                {"funnyjunk","https://2eu.funnyjunk.com/pictures/Ojamajo_a17764_528025.jpg"},
                {"Tsuneotsubasa","https://cdn.discordapp.com/attachments/644383823286763544/663616227914154014/unknown.png"},
                {"Letter Three","https://media.discordapp.net/attachments/512825478512377877/660677566599790627/DO_THE_SWAG.gif"},
                {"Odd Meat","https://cdn.discordapp.com/attachments/569409307100315651/653670970342506548/unknown.png"},
                {"Letter Three","https://media.discordapp.net/attachments/310544560164044801/398230870445785089/DSRxAB9VQAAI5Ja.png"},
                {"Letter Three","https://media.discordapp.net/attachments/314512031313035264/659229196693798912/1551058415141.png?width=396&height=469"},
                {"BreadRavager","https://cdn.discordapp.com/attachments/643722270447239169/664425825030111243/onpuflube.gif"},
                {"Ian","https://i.4pcdn.org/s4s/1537724473581.gif"},
                {"Ian","https://i.4pcdn.org/s4s/1508866828910.gif"},
                {"Ian","http://i.4pcdn.org/s4s/1500066705217.gif"},
                {"Odd Meat","https://cdn.discordapp.com/attachments/644383823286763544/655441472426082345/unknown.png"},
                {"Odd Meat","https://cdn.discordapp.com/attachments/569409307100315651/653669172873527328/unknown.png"},
                {"Odd Meat","https://cdn.discordapp.com/attachments/644383823286763544/654432214595141693/Magical_more_episode_76-.png"},
                {"Odd Meat","https://cdn.discordapp.com/attachments/644383823286763544/654812347038302220/unknown.png"},
                {"Odd Meat","https://cdn.discordapp.com/attachments/644383823286763544/655870644671741972/unknown.png"},
                {"Tsuneotsubasa","https://cdn.discordapp.com/attachments/644383823286763544/656235784038252550/scooby_guest_starring.png"},
                {"Bunty","https://cdn.discordapp.com/attachments/644383823286763544/656262643199508483/48524296-4A66-41DC-B446-2C4A8DC463C1.png"},
                {"Poob","https://i.gyazo.com/thumb/1200/e2b3d361d9ef6adeb0dfe22ee005b249-jpg.jpg"},
                {"Letter Three","https://cdn.discordapp.com/attachments/644383823286763544/658414123893391360/heck.png"},
                {"Tsuneotsubasa","https://cdn.discordapp.com/attachments/644383823286763544/658845873408704535/1575052839549.png"},
                {"Letter Three","https://cdn.discordapp.com/attachments/644383823286763544/659083573437136897/dancedance.gif"},
                {"Letter Three","https://media.discordapp.net/attachments/643721778685804544/659927439551627304/Ea04LEnzT8QAAAABJRU5ErkJggg.png"},
                {"Letter Three","https://cdn.discordapp.com/attachments/644383823286763544/662569497290473512/unknown.png"},
                {"Letter Three","https://cdn.discordapp.com/attachments/644383823286763544/663599628842958849/7fe43d8bf20a13ade7d15ca7ad29155f.png"},
                {"Tsuneotsubasa","https://cdn.discordapp.com/attachments/644383823286763544/663640113380851712/1578292210398.png"},
                {"Tsuneotsubasa","https://cdn.discordapp.com/attachments/644383823286763544/663650504257044480/hazumasameme.jpg"},
                {"Letter Three","https://cdn.discordapp.com/attachments/644383823286763544/664315986266292241/dodo_ate_that_cheese.png"},
                {"Odd Meat","https://cdn.discordapp.com/attachments/569409307100315651/654042450272190474/unknown.png"},
                {"Letter Three","https://cdn.discordapp.com/attachments/569409307100315651/654092176392847390/unknown.png"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/666231823818555392/IDS_MARIO.jpg"},
                {"Odd Meat","https://cdn.discordapp.com/attachments/569409307100315651/652209138507579422/unknown.png"},
                {"https://twitter.com/zenhuxtable | tsuneotsubasa","https://pbs.twimg.com/media/EK_4kDXX0AIXKUE?format=jpg&name=small"},
                {"Odd Meat","https://cdn.discordapp.com/attachments/569409307100315651/651493956496130068/die_monster.png"},
                {"Letter Three","https://cdn.discordapp.com/attachments/569409307100315651/651409834713022474/98a979ed-7268-42e4-87e5-c21070d1c672.png"},
                {"Odd Meat","https://cdn.discordapp.com/attachments/569409307100315651/651389510143311872/4663b1f6-55df-47c5-bb2f-8475b2d39c10.png"},
                {"Odd Meat","https://cdn.discordapp.com/attachments/569409307100315651/651388603796291589/bcecbd0e-51b9-4f5d-bb68-8a6dbf12d04b.png"},
                {"Odd Meat","https://cdn.discordapp.com/attachments/569409307100315651/651358920287191063/fe88353b-fb36-4aec-bfd2-abad90703f5b.png"},
                {"Odd Meat","https://cdn.discordapp.com/attachments/569409307100315651/651189171918209024/first_witch.png"},
                {"Odd Meat","https://cdn.discordapp.com/attachments/569409307100315651/651182811633680394/nowantaiko.png"},
                {"Tsuneotsubasa","https://cdn.discordapp.com/attachments/569409307100315651/649831768634949644/20191128_222919.jpg"},
                {"Letter Three","https://media.discordapp.net/attachments/601461955206709248/623365446392872960/unknown.png?width=654&height=468"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/649142334755307520/1504234417684.jpg"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/649142939339063296/1525921390250.gif"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/649141890725314561/NACHO_BURRITO.gif"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/649141957205164033/ebin.jpg"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/649142070325542942/1508258294538.gif"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/649141770617225226/pNmdjAu.png"},
                {"Tsuneotsubasa","https://cdn.discordapp.com/attachments/569409307100315651/648379300587896883/1500832113127_-_Copy.png"},
                {"Tsuneotsubasa","https://cdn.discordapp.com/attachments/569409307100315651/647983130640121862/saturday_is_for_ojamajo_dad.jpg"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/646817816221319178/1503179977121.jpg"},
                {"Tsuneotsubasa","https://cdn.discordapp.com/attachments/569409307100315651/644405526863544322/1567392672079_-_Copy-chip.jpg"},
                {"Logan Alex Wood","https://cdn.discordapp.com/attachments/569409307100315651/644404591676489729/6tytfgv.png"},
                {"Logan Alex Wood","https://cdn.discordapp.com/attachments/569409307100315651/644402345504931859/Doremi.Ojamajo.Doremi.07.640x480.8A67C5DB.v2.mkv_snapshot_14.46_18.02.17_21.51.41-0061.jpg"},
                {"Logan Alex Wood","https://cdn.discordapp.com/attachments/569409307100315651/644402296167596032/Doremi.Ojamajo.Doremi.07.640x480.8A67C5DB.v2.mkv_snapshot_13.14_2017.07.11_08.25.45.png"},
                {"Letter Three","https://cdn.discordapp.com/attachments/569409307100315651/637455811232268299/unknown.png"},
                {"Letter Three","https://cdn.discordapp.com/attachments/569409307100315651/634123008771883008/Wheezuki.png"},
                {"Tsuneotsubasa","https://cdn.discordapp.com/attachments/622195390682365952/633855826955599882/unknown.png"},
                {"Letter Three","https://cdn.discordapp.com/attachments/569409307100315651/632667918064418858/unknown.png"},
                {"Letter Three","https://cdn.discordapp.com/attachments/569409307100315651/630882364154970112/Doremi_and_Meatwad.png"},
                {"Letter Three","https://cdn.discordapp.com/attachments/569409307100315651/627039453998874635/Doremi_Yelling_at_Dodo.png"},
                {"Tsuneotsubasa","https://cdn.discordapp.com/attachments/569409307100315651/626865289887219723/20190926_141325.jpg"},
                {"Tsuneotsubasa","https://cdn.discordapp.com/attachments/569409307100315651/621153098257268766/EEFCeaRW4AENLCS.png"},
                {"Letter Three","https://media.discordapp.net/attachments/399954211816865792/616449423957819392/Screen_Shot_2019-08-28_at_9.31.56_PM.png?width=623&height=468"},
                {"Letter Three","https://cdn.discordapp.com/attachments/569409307100315651/617512947065028621/49c3845a262649cfc4fd9380ad0f9bf9.png"},
                {"Tsuneotsubasa","https://cdn.discordapp.com/attachments/569409307100315651/617179180139937939/1567101892481.png"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/615741385290678326/1566707190041.jpg"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/615741420736610333/1566707190042.jpg"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/615741454588969002/1566707190043.jpg"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/615741454588969002/1566707190043.jpg"},
                {"Letter Three","https://cdn.discordapp.com/attachments/569409307100315651/610687504395272195/onno.png"},
                {"Poob","https://cdn.discordapp.com/attachments/569409307100315651/610664061675241497/80ffda0ff50bba2092f7295b5597414f-png.png"},
                {"Tsuneotsubasa","https://cdn.discordapp.com/attachments/569409307100315651/609965957452136479/doremis_steaki.png"},
                {"Letter Three","http://media.tumblr.com/tumblr_m4eoeywCjm1r4lv3u.gif"},
                {"Letter Three","https://media.discordapp.net/attachments/569409307100315651/577797449691824132/1522040700266.jpg?width=403&height=468"},
                {"Letter Three","https://cdn.discordapp.com/attachments/569409307100315651/598015670009200665/moshed_2017-6-3_0.22.59.gif"},
                {"Rctgamer3","https://cdn.discordapp.com/attachments/569409307100315651/596251765448507392/unknown.png"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/577797087052300298/1537880631468.gif"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/577797344658194442/1522659324102.jpg"},
                {"Unknown user","https://cdn.discordapp.com/attachments/569409307100315651/575498217165291523/image0.jpg"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/574408691009454083/doremiboomer.png"},
                {"Letter Three","https://cdn.discordapp.com/attachments/569409307100315651/574193232838393866/latest.png"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/573123106399584277/1538920066198.jpg"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/573120294055706640/1510611686079.jpg"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/573120068137779200/1501279167144.jpg"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/573118787784802307/1520685800181.png"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/573116001445478400/1541637365326.jpg"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/573116266319970304/its_over.gif"},
                {"Segawa Onpu","https://cdn.discordapp.com/attachments/569409307100315651/569619410487345162/Onpus_in_black.jpg"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/569618915483713546/1534813397081.gif"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/569619208598454293/1534809043862.gif"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/569619273044066305/1532565451608.gif"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/569618503758249995/1549166072943.png"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/569607758903509023/check_her_out.png"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/569410491794063391/photosynthesis.png"},
                {"Gutsybird","https://cdn.discordapp.com/attachments/569409307100315651/569410391214522378/1519780154767.gif"},
                {"вештица","https://cdn.discordapp.com/attachments/644383823286763544/666315700146667521/4d9a66db2cafa940a3369afbcf5a4706.png"},
                {"Logan Alex Wood","https://cdn.discordapp.com/attachments/644383823286763544/666949577563177000/image0.png"},
                {"4chan","https://i.4pcdn.org/s4s/1524983553681.png"},
                {"4chan","https://i.4pcdn.org/s4s/1537885453707.jpg"},
                {"4chan","https://i.4pcdn.org/s4s/1537658495445.png"},
            };

            int random = new Random().Next(0, arrRandom.GetLength(0));

            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl(arrRandom[random, 1])
                .WithFooter($"Contributed by: {arrRandom[random, 0]}")
                .Build());

        }

        [Command("fairy"), Summary("I will show you my fairy")]
        public async Task showFairy()
        {
            await ReplyAsync("Meet one of my fairy, Dodo.",
            embed: new EmbedBuilder()
            .WithAuthor(Config.Doremi.EmbedName, Config.Doremi.EmbedAvatarUrl)
            .WithDescription("Dodo has fair skin and big mulberry eyes and blushed cheeks. She has pink antennae and short straightened bangs, and she has hair worn in large buns. Her dress is salmon-pink with a pale pink collar." +
            "In teen form, her hair buns grow smaller and she gains a full body.She wears a light pink dress with the shoulder cut out and a white collar.A salmon - pink top is worn under this, and at the chest is a pink gem.She also wears white booties and a white witch hat with a pale pink rim.")
            .WithColor(Config.Doremi.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e2/No.076.jpg/revision/latest?cb=20190704114558")
            .WithFooter("[Ojamajo Witchling Wiki](https://ojamajowitchling.fandom.com/wiki/Dodo)")
            .Build());
        }

        [Command("feedback"), Summary("Give feedback for Doremi Bot and other related bots.")]
        public async Task userFeedback([Remainder] string feedback_message)
        {
            using (StreamWriter w = File.AppendText($"attachments/{Context.Guild.Id}/feedback_{Context.Guild.Id}.txt"))
                w.WriteLine($"[{DateTime.Now.ToString("MM/dd/yyyy HH:mm")}]{Context.User.Username}:{feedback_message}");

            await ReplyAsync($"Thank you for your feedback. Doremi bot and her other friends will be improved soon with your feedback.",
                embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl("https://66.media.tumblr.com/c8f9c5455355f8e522d52bacb8155ab0/tumblr_mswho8nWx11r98a5go1_400.gif")
                .Build());
        }

        [Command("hello"), Summary("Hello, I will greet you up")]
        public async Task doremiHello()
        {
            string tempReply = "";
            List<string> listRandomRespond = new List<string>() {
                    $"Hii hii {MentionUtils.MentionUser(Context.User.Id)}! ", 
                    $"Hello {MentionUtils.MentionUser(Context.User.Id)}! ",
            };

            int rndIndex = new Random().Next(0, listRandomRespond.Count);
            tempReply = listRandomRespond[rndIndex]+Config.Doremi.arrRandomActivity[Config.Doremi.indexCurrentActivity,1];
            
            await ReplyAsync(tempReply);
        }

        [Command("hugs"), Alias("hug"), Summary("I will give warm hug for you or <username>")]
        public async Task HugUser(SocketGuildUser username=null)
        {
            if (username == null){
                string message = $"*hugs back*. That's very nice, thank you for the warm hugs {MentionUtils.MentionUser(Context.User.Id)} :hugging:";
                await Context.Channel.SendMessageAsync(message);
            } else {
                string message = $"Pirika pirilala poporina peperuto! Give a warm hugs for {MentionUtils.MentionUser(username.Id)} :hugging:";
                await Context.Channel.SendMessageAsync(message);
            }
        }

        [Command("invite"), Summary("Generate the invitation links for related ojamajo bot")]
        public async Task invite()
        {
            await ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Doremi.EmbedColor)
            .WithAuthor(Config.Doremi.EmbedName, Config.Doremi.EmbedAvatarUrl)
            .WithTitle("Bot Invitation Links")
            .WithDescription($"Pirika pirilala poporina peperuto! Generate the bot links!")
            .AddField("Doremi Bot", "[Click here to invite Doremi Bot](https://discordapp.com/api/oauth2/authorize?client_id=655668640502251530&permissions=2117532736&scope=bot)")
            .AddField("Hazuki Bot", "[Click here to invite Hazuki Bot](https://discordapp.com/api/oauth2/authorize?client_id=655307117128974346&permissions=238419008&scope=bot)")
            .AddField("Aiko Bot", "[Click here to invite Aiko Bot](https://discordapp.com/api/oauth2/authorize?client_id=663612449341046803&permissions=238419008&scope=bot)")
            .Build());
        }

        [Command("magical stage"), Alias("magicalstage"), Summary("I will perform magical stage along with the other and make a <wishes>")]
        public async Task magicalStage([Remainder] string query)
        {
            if (query != null)
            {
                Config.Doremi.MagicalStageWishes = query;
                await ReplyAsync($"{MentionUtils.MentionUser(Config.Hazuki.Id)} Pirika pirilala, Nobiyaka ni!",
                embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/3d/Nobiyakanis1.2.png/revision/latest?cb=20190408124752")
                .Build());
            }
            else
            {
                await ReplyAsync($"Please enter your wishes.");
            }
        }

        [Command("meme"), Alias("memes"), Summary("I will give you some random memes")]
        public async Task givememe()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://meme-api.herokuapp.com/gimme/memes/20");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string jsonResp = reader.ReadToEnd().ToString();
                JObject jobject = JObject.Parse(jsonResp);

                int randomIndex = new Random().Next(0, 21);
                var description = jobject.GetValue("memes")[randomIndex]["title"];
                var imgUrl = jobject.GetValue("memes")[randomIndex]["url"];

                await base.ReplyAsync(embed: new EmbedBuilder()
                .WithDescription(description.ToString())
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl(imgUrl.ToString())
                .Build());

            }
            catch (Exception e)
            {
                //Console.Write(e.ToString());
            }

        }

        [Command("quotes"), Summary("I will mention random Doremi quotes")]
        public async Task quotes()
        {
            string[] arrQuotes = {
                "I'm the world's most unluckiest pretty girl!",
                "Happy! Lucky! For all of you!",
                "I like Steak very much"
            };

            await ReplyAsync(arrQuotes[new Random().Next(0, arrQuotes.Length)]);
        }

        [Command("random"), Alias("moments"), Summary("Show any random Doremi moments")]
        public async Task randomthing()
        {
            string[] arrRandom =
            {"https://66.media.tumblr.com/bd4f75234f1180fa7fd99a5200ac3c8d/tumblr_nbhwuqEY6c1r98a5go1_500.gif",
            "https://66.media.tumblr.com/e62f24b9645540f4fff4e6ebe8bd213e/tumblr_pco5qx9Mim1r98a5go1_500.gif",
            "https://media1.tenor.com/images/2bedf54ca06c5a3b073f3d9349db65b4/tenor.gif",
            "https://static.zerochan.net/Harukaze.Doremi.full.2494232.gif",
            "https://i.4pcdn.org/s4s/1511988377651.gif",
            "https://66.media.tumblr.com/68b432cf50e18a72b661ba952fcf778f/tumblr_pgohlgzfvY1xqvqxzo1_400.gif",
            "https://espressocomsaudade.files.wordpress.com/2014/07/6.gif",
            "https://cdn.discordapp.com/attachments/569409307100315651/646751194441842688/unknown.png",
            "https://66.media.tumblr.com/b11f9dced4594739776b976ed66920fc/tumblr_inline_mgcb4zEbfV1r4lv3u.gif",
            "https://media1.tenor.com/images/c9d91a992a919d4c92e2d5d499f379d2/tenor.gif",
            "https://pbs.twimg.com/media/EORFh9zX0AAARER?format=png&name=small","https://pbs.twimg.com/media/EOIwxbEXsAAWcdf?format=png&name=small",
            "https://pbs.twimg.com/media/EOPBUOMXUAA6PNV?format=png&name=small","https://pbs.twimg.com/media/EOHpQh-XkAEFR1I?format=png&name=small",
            "https://pbs.twimg.com/media/EOGl7_xXUAEqihP?format=png&name=small","https://pbs.twimg.com/media/EOFV7t6WoAAmwor?format=png&name=small",
            "https://pbs.twimg.com/media/EOEfdIJX4AAHORN?format=png&name=small","https://pbs.twimg.com/media/EOBg0_pX4AQEvrR?format=png&name=small",
            "https://pbs.twimg.com/media/EOBI5p1WkAMn65j?format=png&name=small","https://pbs.twimg.com/media/EOAvJH5XUAAyso4?format=png&name=small",
            "https://pbs.twimg.com/media/EOATjeUXsAIo8Kw?format=png&name=small","https://pbs.twimg.com/media/EN_hqAQX0AA1SJf?format=png&name=small",
            "https://pbs.twimg.com/media/EN_QaD5WoAAVPlJ?format=png&name=small","https://pbs.twimg.com/media/EN-aX9OX4AErhax?format=png&name=small",
            "https://pbs.twimg.com/media/EN-G2YgX0AAk499?format=png&name=small","https://pbs.twimg.com/media/EN9klu-WsAA9h23?format=png&name=small",
            "https://pbs.twimg.com/media/EN9H27kW4AETemw?format=png&name=small","https://pbs.twimg.com/media/EN8ZpDJWsAcu-FS?format=png&name=small",
            "https://pbs.twimg.com/media/EN8R0pHWsAAUAhh?format=png&name=small","https://pbs.twimg.com/media/EN7PUhkXUAA5wh_?format=png&name=small",
            "https://pbs.twimg.com/media/EN6jLjxW4AIYf_w?format=png&name=small","https://pbs.twimg.com/media/EN5-Z4JX4AE_Kvm?format=png&name=small",
            "https://pbs.twimg.com/media/EN5AwOaWAAEwvCD?format=png&name=small","https://pbs.twimg.com/media/EN4vgHFUwAELkMA?format=png&name=small",
            "https://pbs.twimg.com/media/EN4lKurWoAABXZo?format=png&name=small","https://pbs.twimg.com/media/EN3thU4VUAAflQd?format=png&name=small",
            "https://pbs.twimg.com/media/EN3a5ykX0AAnnfD?format=png&name=small","https://pbs.twimg.com/media/EN3C-VGX0AElYvD?format=png&name=small",
            "https://pbs.twimg.com/media/EN24osAW4AYXQIr?format=png&name=small","https://pbs.twimg.com/media/EN2vN9VX0AAqz8S?format=png&name=small",
            "https://pbs.twimg.com/media/EN1teNYU8AIMRyn?format=png&name=small","https://pbs.twimg.com/media/EN1i5okXkAAMpOT?format=png&name=small",
            "https://pbs.twimg.com/media/EN0wknxX0AANgWz?format=png&name=small","https://pbs.twimg.com/media/EN0pNQvX0AAhYK1?format=png&name=small",
            "https://pbs.twimg.com/media/EN0GsE3X4AAmW5n?format=png&name=small","https://pbs.twimg.com/media/ENzolqaWsAMaqPd?format=png&name=small",
            "https://pbs.twimg.com/media/ENzf2FjWoAAa6dr?format=png&name=small","https://pbs.twimg.com/media/ENzN6R-XUAA65it?format=png&name=small",
            "https://pbs.twimg.com/media/ENywQseXkAAZORC?format=png&name=small","https://pbs.twimg.com/media/ENyCSNTWoAAupEh?format=png&name=small",
            "https://pbs.twimg.com/media/ENxdgCqX0AEWDvm?format=png&name=small","https://pbs.twimg.com/media/ENwUIldWwAAIDle?format=png&name=small",
            "https://pbs.twimg.com/media/ENvwuFNWoAAiX_X?format=png&name=small","https://pbs.twimg.com/media/ENux8aVWsAA3mp7?format=png&name=small",
            "https://pbs.twimg.com/media/ENuPOWlX0AYFHPd?format=png&name=small","https://pbs.twimg.com/media/ENsyGt6WoAAnl1O?format=png&name=small",
            "https://pbs.twimg.com/media/ENsAMTCUUAUeifE?format=png&name=small","https://pbs.twimg.com/media/ENrYcEpWwAAg3dG?format=png&name=small",
            "https://pbs.twimg.com/media/ENq-cTzXkAENaO8?format=png&name=small","https://pbs.twimg.com/media/ENqHP3lXYAINSdK?format=png&name=small",
            "https://pbs.twimg.com/media/ENpRsDsWsAABTxZ?format=png&name=small","https://pbs.twimg.com/media/ENon0itXsAEgp_B?format=png&name=small",
            "https://pbs.twimg.com/media/ENmhjgMUwAE0uY1?format=png&name=small","https://pbs.twimg.com/media/ENmQ_wQUUAAVvlR?format=png&name=small",
            "https://pbs.twimg.com/media/ENk83Q1X0AA_E3v?format=png&name=small","https://pbs.twimg.com/media/ENksiBwWsAErMhH?format=png&name=small",
            "https://pbs.twimg.com/media/ENk1RBsWsAEF_kK?format=png&name=small","https://pbs.twimg.com/media/ENjma7NVUAAq5Fp?format=png&name=small",
            "https://pbs.twimg.com/media/ENjMNHQXkAEkga6?format=png&name=small","https://pbs.twimg.com/media/ENh4wLNXUAAfKRe?format=png&name=small",
            "https://pbs.twimg.com/media/ENgoScqX0AAotuK?format=png&name=small","https://pbs.twimg.com/media/ENeVLU_U4AAttL1?format=png&name=small",
            "https://pbs.twimg.com/media/ENciclUWoAAjVER?format=png&name=small","https://pbs.twimg.com/media/ENb-kokWwAI2Pq0?format=png&name=small",
            "https://pbs.twimg.com/media/ENa44FCXYAEdfOJ?format=png&name=small","https://pbs.twimg.com/media/ENacmC-W4AAxFOm?format=png&name=small",
            "https://pbs.twimg.com/media/ENZL4vHXYAMDnS9?format=png&name=small","https://pbs.twimg.com/media/ENZCdxnXYAAO2o_?format=png&name=small",
            "https://pbs.twimg.com/media/ENY59PZXYAgK8ak?format=png&name=small","https://pbs.twimg.com/media/ENYGp8_XsAI83YN?format=png&name=small",
            "https://pbs.twimg.com/media/ENW1sxwXUAILM_i?format=png&name=small","https://pbs.twimg.com/media/ENV1UvEW4AAR0A0?format=png&name=small",
            "https://pbs.twimg.com/media/ENVTRWNXkAEV60U?format=png&name=small","https://pbs.twimg.com/media/ENVItRGWwAATIr-?format=png&name=small",
            "https://pbs.twimg.com/media/ENTeezSWoAgmvLt?format=png&name=small","https://pbs.twimg.com/media/ENTOJASWoAEGzz8?format=png&name=small",
            "https://pbs.twimg.com/media/ENSc6XVWwAAsJLL?format=png&name=small","https://pbs.twimg.com/media/ENSU3RfWoAA1eiD?format=png&name=small",
            "https://pbs.twimg.com/media/ENSBjGNXsAEHRoX?format=png&name=small","https://pbs.twimg.com/media/ENRU6kSW4AEDfSK?format=png&name=small",
            "https://pbs.twimg.com/media/ENQKro5W4AANaca?format=png&name=small","https://pbs.twimg.com/media/ENPIAq3UcAINjmv?format=png&name=small",
            "https://pbs.twimg.com/media/ENOU8n-WoAAv_aq?format=png&name=small","https://pbs.twimg.com/media/ENODBRcXYAEMmsf?format=png&name=small",
            "https://pbs.twimg.com/media/ENNT5iuWwAEvIfd?format=png&name=small","https://pbs.twimg.com/media/ENMTTAQX0AAKGBd?format=png&name=small",
            "https://pbs.twimg.com/media/ENLf_u-XYAEEMUF?format=png&name=small","https://pbs.twimg.com/media/ENKw4b1W4AEFTw0?format=png&name=small",
            "https://pbs.twimg.com/media/ENKdy17WoAAkANp?format=png&name=small","https://pbs.twimg.com/media/ENJtDOfWsAUx4VR?format=png&name=small",
            "https://pbs.twimg.com/media/ENHNwXNWsAAsQcS?format=png&name=small","https://pbs.twimg.com/media/ENF3DALWwAAHOOq?format=png&name=small",
            "https://pbs.twimg.com/media/ENFtKEZWkAAIney?format=png&name=small","https://pbs.twimg.com/media/ENAkl3vXsAAOqYX?format=png&name=small",
            "https://pbs.twimg.com/media/EM_WLxdXUAA5SQ5?format=png&name=small","https://pbs.twimg.com/media/EM-fBDYW4AAsaHJ?format=png&name=small",
            "https://pbs.twimg.com/media/EM-M2n6XkAci9q4?format=png&name=small","https://pbs.twimg.com/media/EM9jqoVWoAE0_vr?format=png&name=small",
            "https://pbs.twimg.com/media/EM9asouX0AEEg1a?format=png&name=small","https://pbs.twimg.com/media/EM9A7zwWsAITihn?format=png&name=small",
            "https://pbs.twimg.com/media/EM7-9d4X0AEAIYP?format=png&name=small","https://pbs.twimg.com/media/EM59S_nWoAEUFCr?format=png&name=small",
            "https://pbs.twimg.com/media/EM4h_h3WkAE-6n_?format=png&name=small",
            };

            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl(arrRandom[new Random().Next(0, arrRandom.Length)])
                .WithFooter("Some images contributed by: https://twitter.com/DoremiRobo, Tumblr")
                .Build());
        }

        [Command("stats"), Alias("bio"), Summary("I will show you my biography info")]
        public async Task showStats()
        {
            await ReplyAsync("Pirika pirilala poporina peperuto! Show my biography info!",
            embed: new EmbedBuilder()
            .WithAuthor(Config.Doremi.EmbedName, Config.Doremi.EmbedAvatarUrl)
            .WithDescription("Doremi Harukaze (春風どれみ, Harukaze Doremi) is the main protagonist and the titular character of Ojamajo Doremi. " +
            "She is an average, eight-year-old unlucky girl dealing with argumentative parents, a spiteful little sister, a lack of romance and terrible grades. " +
            "But after a particularly foul day, she learns the owner of Misora's Magical Shop is really a witch.")
            .AddField("Full Name", "春風どれみ Harukaze Doremi", true)
            .AddField("Gender", "female", true)
            .AddField("Blood Type", "B", true)
            .AddField("Birthday", "July 30th, 1990", true)
            .AddField("Instrument", "Piano", true)
            .AddField("Favorite Food", "Steak", true)
            .AddField("Debut", "[I'm Doremi! Becoming a Witch Apprentice!](https://ojamajowitchling.fandom.com/wiki/I%27m_Doremi!_Becoming_a_Witch_Apprentice!)", true)
            .WithColor(Config.Doremi.EmbedColor)
            .WithImageUrl("https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcRBfCfThqVYdJWQzWJOvILjx-Acf-DgRQidfN1s11-fxc0ShEe3")
            .WithFooter("Source: [Ojamajo Witchling Wiki](https://ojamajowitchling.fandom.com/wiki/Doremi_Harukaze)")
            .Build());
        }

        [Command("star"), Summary("I will pin this messsages if it has 5 stars reaction")]
        [RequireBotPermission(ChannelPermission.ManageMessages, 
            ErrorMessage = "Oops, I need ``manage channels`` permission to use this command")]
        public async Task starMessages([Remainder] string MessagesOrWithAttachment)
        {
            try
            {
                var attachments = Context.Message.Attachments;

                WebClient myWebClient = new WebClient();

                string file = attachments.ElementAt(0).Filename;
                string url = attachments.ElementAt(0).Url;
                string extension = Path.GetExtension(attachments.ElementAt(0).Filename).ToLower();
                string randomedFileName = DateTime.Now.ToString("yyyyMMdd_HHmm") + new Random().Next(0, 10000) + extension;
                string completePath = $"attachments/{Context.Guild.Id}/{randomedFileName}";

                //if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif")
                //{
                    // Download the resource and load the bytes into a buffer.
                byte[] buffer = myWebClient.DownloadData(url);
                Config.Core.ByteArrayToFile($"attachments/{Context.Guild.Id}/{randomedFileName}", buffer);

                await Context.Message.DeleteAsync();
                var sentMessage = await ReplyAsync($"{MentionUtils.MentionUser(Context.User.Id)} has a star messages:\n{MessagesOrWithAttachment}");
                var sentAttachment = await Context.Channel.SendFileAsync(completePath);
                await sentMessage.AddReactionAsync(new Emoji("\u2B50"));
                File.Delete(completePath);
                return;

                //} else {
                //    await ReplyAsync($"Oops, sorry only ``.jpg/.jpeg/.png/.gif`` format is allowed to use ``star`` commands.");
                //    return;
                //}
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
            }

            await Context.Message.DeleteAsync();
            var sentWithoutAttached = await ReplyAsync($"{MentionUtils.MentionUser(Context.User.Id)} has a star messages:\n{MessagesOrWithAttachment}");
            await sentWithoutAttached.AddReactionAsync(new Emoji("\u2B50"));
        }

        [Command("steak"), Summary("Show any random doremi and her steak moments")]
        public async Task randomsteakmoments()
        {
            string[,] arrRandom =
            { {$"Itadakimasu!{Config.Emoji.drool}" , "https://66.media.tumblr.com/5cea42347519a4f8159197ec6a064eb4/tumblr_olqtewoJDS1r809wso2_640.png"},
            {"How nice, I want to get proposed with steak too.", "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQIrd47iWprHNy350YW9GKT1E3CBWXekyF2Dk9KFzKcLWHwcltU1g&s"},
            {$"Itadakimasu~{Config.Emoji.drool}", "https://i.4pcdn.org/s4s/1507491838404.jpg"},
            {$"*Dreaming of steaks {Config.Emoji.drool}*", "https://i.4pcdn.org/s4s/1505677406037.png"},
            {$"Big Steak~{Config.Emoji.drool}", "https://images.plurk.com/vTMo-3u8rgOUOAP0RE2hrzIJHvs.jpg"},
            {$"I can't wait to taste this delicious steak~{Config.Emoji.drool}", "https://scontent-mia3-1.cdninstagram.com/vp/54447c3d0032fab0e92771612f457bc6/5E23392F/t51.2885-15/e35/68766038_126684885364883_7230690820171265153_n.jpg?_nc_ht=scontent-mia3-1.cdninstagram.com&_nc_cat=111&ig_cache_key=MjEzNDM1MTExMTk4MTM3NDA1NQ%3D%3D.2"},
            {$"A wild steak has appeared!","https://66.media.tumblr.com/85ac2417517a14300a8660a536b9e940/tumblr_oxy34aOH591tdnbbbo1_640.gif" },
            {$"Itadakimasu!{Config.Emoji.drool}" ,"https://i.4pcdn.org/s4s/1507491838404.jpg"},
            {$"Big steak{Config.Emoji.drool}","https://images.plurk.com/vTMo-3u8rgOUOAP0RE2hrzIJHvs.jpg"},
            {$"Itadakimasu!{Config.Emoji.drool}","https://66.media.tumblr.com/337aaf42d3fb0992c74f7f9e2a0bf4f6/tumblr_olqtewoJDS1r809wso1_500.png"} };

            Random rnd = new Random();
            int rndIndex = rnd.Next(0, arrRandom.GetLength(0));

            await ReplyAsync(arrRandom[rndIndex, 0]);
            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl(arrRandom[rndIndex, 1])
                .Build());
        }

        [Command("thank you"), Alias("thank you", "thanks", "arigatou"), Summary("Say thank you to Doremi Bot")]
        public async Task thankYou([Remainder] string messages = "")
        {
            await ReplyAsync($"Your welcome, {MentionUtils.MentionUser(Context.User.Id)}. I'm glad that you're happy with it :smile:");
        }

        [Command("turn"), Alias("transform"), Summary("Turn <username> into <wishes>")]
        public async Task spells(IUser username, [Remainder] string wishes)
        {
            //await Context.Message.DeleteAsync();
            await ReplyAsync($"Pirika pirilala poporina peperuto! Turn {username.Mention} into {wishes}",
            embed: new EmbedBuilder()
            .WithColor(Config.Doremi.EmbedColor)
            .WithImageUrl("https://i.makeagif.com/media/10-05-2015/rEFQz2.gif")
            .Build());
        }
 
        [Command("wish"), Summary("I will grant you a <wishes>")]
        public async Task wish([Remainder] string wishes)
        {
            await ReplyAsync($"Pirika pirilala poporina peperuto! {wishes}");
            await base.ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Doremi.EmbedColor)
            .WithImageUrl("https://i.makeagif.com/media/10-05-2015/rEFQz2.gif")
            .Build());
        }

        [Command("updates"), Summary("See what's new on Doremi & her other related bot")]
        public async Task showLatestUpdate()
        {
            await base.ReplyAsync(embed: new EmbedBuilder()
            .WithTitle("What's new?")
            .WithDescription("Pirika pirilala poporina peperuto! Show us what's new on doremi bot and her friends!")
            .AddField("Summary", 
            "-Help commands has been updated & categorized with: **@bot!help <commands or category>**\n" +
            "-**Doremi bot**: `star` commands that will let doremi bot to pin message on 5 star reactions\n" +
            "-**Aiko bot**: `spooky` commands that you better not use\n" +
            "-Doremi, Hazuki and Aiko Bot commands and functionality has been updated\n" +
            "-More random image/source for Doremi, Hazuki and Aiko Bot\n" +
            "-Specified contribution images for some commands\n" +
            "-Specified error report will be displayed more correctly\n" +
            "-Status/activity update on Doremi and her other friends")
            .AddField("Doremi Bot update/new commands:", "`star`,`meme`,`dorememes`,`feedback`" +
            ",`hug`,`random`")
            .AddField("Hazuki Bot update/new commands:", "`dabzuki`,`wheezuki`,`hug`,`thank you`,`random`")
            .AddField("Aiko Bot update/new commands:", "`spooky`,`hug`,`thank you`,`random`")
            .WithColor(Config.Doremi.EmbedColor)
            .WithFooter("Last updated on Jan 16,2019")
            .Build());
        }

        //todo/more upcoming commands: easter egg/hidden commands, set daily message announcement, gacha,
        //contribute caption for random things
        //user card maker, sing lyrics together with other ojamajo bot, birthday reminder, voting for best ojamajo bot, witch seeds to cast a spells
        //10 stars pinned message 
        //mini hangman game, virtual maho-dou shop interactive
    }

    [Name("mod"), Group("mod"), Summary("Basic moderator commands. Require `manage channels` permission")]
    [RequireUserPermission(ChannelPermission.ManageChannels,
        ErrorMessage = "Oops, You need to have the `manage channels` permission to use this command",
        NotAGuildErrorMessage = "Oops, You need to have the `manage channels` permission to use this command")]
    public class DoremiModerator : ModuleBase<SocketCommandContext>
    {
        [Name("mod channels"),Group("mod"), Summary("Channel moderator commands. Require `manage channels` permission")]
        public class DoremiModeratorChannels : ModuleBase<SocketCommandContext>
        {
            [Command("random event"), Summary("Schedule Doremi Bot to make random event message on <channel_name> for every 24 hours")]
            public async Task assignRandomEvent(IGuildChannel iguild)
            {
                Config.Guild.assignId(iguild.GuildId, "id_random_event", iguild.Id.ToString());

                if (Config.Doremi._timerRandomEvent.ContainsKey(iguild.GuildId.ToString()))
                    Config.Doremi._timerRandomEvent[iguild.GuildId.ToString()].Change(Timeout.Infinite, Timeout.Infinite);

                Config.Doremi._timerRandomEvent[$"{iguild.GuildId.ToString()}"] = new Timer(async _ =>
                {
                    Random rnd = new Random();
                    int rndIndex = rnd.Next(0, Config.Doremi.listRandomEvent.Count); //random the list value
                    Console.WriteLine("Doremi Random Event : " + Config.Doremi.listRandomEvent[rndIndex]);

                    var socketClient = Context.Client;
                    try
                    {
                        await socketClient
                        .GetGuild(iguild.GuildId)
                        .GetTextChannel(Config.Guild.Id_random_event[iguild.GuildId.ToString()])
                        .SendMessageAsync(Config.Doremi.listRandomEvent[rndIndex]);
                    }
                    catch
                    {
                        Console.WriteLine($"Doremi Random Event Exception: Send message permissions has been missing {iguild.Guild.Name} : {iguild.Name}");
                    }
                },
                    null,
                    TimeSpan.FromHours(Config.Doremi.Randomeventinterval), //time to wait before executing the timer for the first time
                    TimeSpan.FromHours(Config.Doremi.Randomeventinterval) //time to wait before executing the timer again
                );

                await ReplyAsync($"**Random Event Channels** has been assigned into: {MentionUtils.MentionChannel(iguild.Id)}");
            }

            //[Command("online")]
            //public async Task assignNotifOnline(IGuildChannel iguild)
            //{
            //    Config.Guild.assignId(iguild.GuildId, "id_notif_online", iguild.Id.ToString());
            //    await ReplyAsync($"**Bot Online Notification Channels** has been assigned into: {MentionUtils.MentionChannel(iguild.Id)}");
            //}

            [Command("remove settings"), Summary("Remove the random event settings on the assigned channels. Current available settings: `randomEvent`")]
            public async Task assignRandomEvent(string settings = "randomEvent")
            {
                string property = "";
                Boolean propertyExists = false;
                ulong channelId = 0;

                if (settings.ToLower() == "randomevent")
                {
                    property = "Random Event";

                    if (Config.Guild.Id_random_event.ContainsKey(Context.Guild.Id.ToString()))
                    {
                        channelId = Config.Guild.Id_random_event[Context.Guild.Id.ToString()];
                        propertyExists = true;
                        Config.Guild.assignId(Context.Guild.Id, "id_random_event", "");
                        if (Config.Doremi._timerRandomEvent.ContainsKey(Context.Guild.Id.ToString()))
                            Config.Doremi._timerRandomEvent[Context.Guild.Id.ToString()].Change(Timeout.Infinite, Timeout.Infinite);
                    }

                }

                if (propertyExists)
                    await ReplyAsync($"**{property} Channels** settings has been removed.");
                else
                    await ReplyAsync($"**{property} Channels** has no settings yet.");
            }
        }


        //[Command("Help")]
        //public async Task showHelpModerator()
        //{
        //    await ReplyAsync(embed: new EmbedBuilder()
        //        .WithColor(Config.Doremi.EmbedColor)
        //        .WithAuthor(Config.Doremi.EmbedName, Config.Doremi.EmbedAvatarUrl)
        //        .WithTitle("Moderator Command List:")
        //        .WithDescription($"Require ``Manage Channels`` permission.\n" +
        //        $"Basic Moderator Prefix: **{MentionUtils.MentionUser(Config.Doremi.Id)} mod** or **{Config.Doremi.PrefixParent[0]}mod** or **{Config.Doremi.PrefixParent[0]}mod** followed with the <whitespace>\n" +
        //        $"Channel Moderator Prefix: **{MentionUtils.MentionUser(Config.Doremi.Id)} mod channels** or **{Config.Doremi.PrefixParent[0]}mod channels** or **{Config.Doremi.PrefixParent[0]}mod channels** followed with the <whitespace>"
        //        )
        //        .AddField("Basic Moderator:",
        //        "**channelid <opt:channel_name>** : Give Channel Id within the optional <channel_name> parameter\n" +
        //        "**guildid** : Give the Server Id\n" +
        //        "**help** : You already execute this command")
        //        .AddField("Channel Moderator:",
        //        $"**randomevent <channel_name>** : Schedule {MentionUtils.MentionUser(Config.Doremi.Id)} to do random event message on <channel_name> every 24 hours\n" +
        //        "**remove <randomevent>** : Remove the settings for given parameter: Randomevent")
        //        .Build());
        //}

        [Command("guildid"), Summary("Give the Server Id")]
        public async Task getGuildId()
        {
            await ReplyAsync($"{Context.Guild.Id}");
        }

        [Command("channelid"),Summary("Give the channel Id on current/mentioned channels.")]
        public async Task getCurrentChannelId(IGuildChannel guildChannel=null)
        {
            if(guildChannel==null)
                await ReplyAsync($"{Context.Channel.Id}");
            else
                await ReplyAsync($"{guildChannel.Id}");
        }

        
    }

    [Summary("hidden")]
    public class DoremiMagicalStageModule : ModuleBase
    {
        //magical stage section

        [Command("Pameruku raruku, Takaraka ni!")]//from aiko
        public async Task magicalStagefinal()
        {
            if (Context.User.Id == Config.Aiko.Id)
                await ReplyAsync($"{MentionUtils.MentionUser(Config.Hazuki.Id)} Magical Stage! {Config.Doremi.MagicalStageWishes}\n");

        }

    }

    [Name("Music"),Remarks("Please join any voice channel first, then type `do!join` so the bot can stream on your voice channel.")]
    public sealed class DoremiVictoriaMusic : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public DoremiVictoriaMusic(LavaNode lavanode)
        {
            _lavaNode = lavanode;
        }

        //[Command("Join")]
        //public async Task JoinAsync()
        //{
        //    //(Context.User as IVoiceState).VoiceChannel
        //    var user = Context.User as SocketGuildUser;
        //    if (user.VoiceChannel is null)
        //    {
        //        await ReplyAsync("You need to connect to a voice channel.");
        //        return;
        //    }
        //    else
        //    {
        //        await _victoriaService.ConnectAsync(user.VoiceChannel, Context.Channel as ITextChannel);
        //        await ReplyAsync($"now connected to {user.VoiceChannel.Name}");
        //    }
        //}

        //[Command("Leave")]
        //public async Task Leave()
        //{
        //    var user = Context.User as SocketGuildUser;
        //    if (user.VoiceChannel is null)
        //    {
        //        await ReplyAsync("Please join the channel the bot is in to make it leave.");
        //    }
        //    else
        //    {
        //        await _victoriaService.LeaveAsync(user.VoiceChannel);
        //        await ReplyAsync($"Bot has now left {user.VoiceChannel.Name}");
        //    }
        //}


           // "**join** : I will join to your connected voice channel (Please join any voice channel first)\n" +
        //    "**musiclist** or **mulist** : Show the doremi music list\n" +
        //    "**play <track number or title>** : Play the music with the given <track number or title> parameter\n" +
        //    "**playall** : Play all the music that's available on doremi music list\n" +
        //    "**queue** or **muq** : Show all the music that's currently on queue list\n" +
        //    "**seek <timespan>** : Seek the music into the given <timespan>[hh:mm:ss]\n" +
        //    "**skip** : Skip the music\n" +
        //    "**stop** : Stop playing the music. This will also clear the queue list\n" +
        //    "**youtube** or **yt <keyword or url>** : Play the youtube music either it's from keyword/url")

        [Command("Join"), Summary("Join to your connected voice channel (Please join any voice channel first)")]
        public async Task JoinAsync()
        {
            //await _lavaNode.JoinAsync((Context.User as IVoiceState).VoiceChannel);
            //await ReplyAsync($"Joined {(Context.User as IVoiceState).VoiceChannel} channel!");


            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm already connected to a voice channel!");
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
                await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
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
                await ReplyAsync("I'm not connected to any voice channels.");
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
                Config.Music.queuedTrack[Context.Guild.Id.ToString()].Clear();
                await _lavaNode.LeaveAsync(voiceChannel);
                await ReplyAsync($"I've left {voiceChannel.Name}!");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Move"), Summary("Move the bots into your new voice channel")]
        public async Task MoveAsync()
        {
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            await _lavaNode.MoveAsync((Context.User as IVoiceState).VoiceChannel);
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
                await ReplyAsync("Woaaah there, I can't seek when nothing is playing.");
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
        [Command("youtube"), Alias("yt"), Summary("Play the youtube music either it's from keyword/url")]
        public async Task PlayYoutubeAsync([Remainder] string KeywordOrUrl)
        {
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            var search = await _lavaNode.SearchYouTubeAsync(KeywordOrUrl);
            var track = search.Tracks.FirstOrDefault();

            var player = _lavaNode.HasPlayer(Context.Guild)
                ? _lavaNode.GetPlayer(Context.Guild)
                : await _lavaNode.JoinAsync((Context.User as IVoiceState).VoiceChannel);
            
            Config.Music.queuedTrack[Context.Guild.Id.ToString()].Add(track.Title);

            if (player.PlayerState == PlayerState.Playing){
                player.Queue.Enqueue(track);
                //Config.Music.storedLavaTrack[Context.Guild.Id.ToString()].Add(track);
                await ReplyAsync($":arrow_down:  Added to queue: {track.Title}.");
            } else {
                await player.PlayAsync(track);
                await ReplyAsync($"🔈 Playing {track.Title}.");
            }
        }

        [Command("playall"), Summary("Play all the music that's available on doremi music list")]
        public async Task PlayAll()
        {

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            var player = _lavaNode.HasPlayer(Context.Guild)
                ? _lavaNode.GetPlayer(Context.Guild)
                : await _lavaNode.JoinAsync((Context.User as IVoiceState).VoiceChannel);

            await ReplyAsync($"I will play all music on the musiclist");

            JObject jObj = Config.Music.jobjectfile;

            for (int i = 0; i < (jObj.GetValue("musiclist") as JObject).Count; i++)
            {
                String query = jObj.GetValue("musiclist")[(i + 1).ToString()]["filename"].ToString();
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
                            Config.Music.queuedTrack[Context.Guild.Id.ToString()].Add(track.Title);
                            player.Queue.Enqueue(track);
                        }

                        //await ReplyAsync($"🔈 Enqueued {searchResponse.Tracks.Count} tracks.");
                    }
                    else
                    {
                        var track = searchResponse.Tracks[0];
                        player.Queue.Enqueue(track);
                        Config.Music.queuedTrack[Context.Guild.Id.ToString()].Add(track.Title);
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
                                Config.Music.queuedTrack[Context.Guild.Id.ToString()].Add(track.Title);
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

        [Command("play"), Summary("Play the music with the given <track number or title> parameter")]
        public async Task PlayLocal([Remainder] string TrackNumOrTitle)
        {
            if (string.IsNullOrWhiteSpace(TrackNumOrTitle))
            {
                await ReplyAsync("Please provide track numbers or title. Use do!mulist to show all doremi music list.");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            var player = _lavaNode.HasPlayer(Context.Guild)
                ? _lavaNode.GetPlayer(Context.Guild)
                : await _lavaNode.JoinAsync((Context.User as IVoiceState).VoiceChannel);

            JObject jObj = Config.Music.jobjectfile;
            if (int.TryParse(TrackNumOrTitle, out int n)) {
                
                if(n <= (jObj.GetValue("musiclist") as JObject).Count){
                    TrackNumOrTitle = jObj.GetValue("musiclist")[n.ToString()]["filename"].ToString();
                } else {
                    await ReplyAsync($"I wasn't able to find anything for track number {TrackNumOrTitle}. See the available doremi music list on ``doremi!mulist`` commands.");
                    return;
                }
                
            } else {
                for (int i = 0; i < (jObj.GetValue("musiclist") as JObject).Count; i++)
                {
                    String replacedFilename = jObj.GetValue("musiclist")[(i + 1).ToString()]["filename"].ToString().Replace(".mp3", "").Replace(".ogg", "");
                    if (replacedFilename == TrackNumOrTitle)
                    {
                        TrackNumOrTitle = jObj.GetValue("musiclist")[(i + 1).ToString()]["filename"].ToString();
                    }
                    
                }
            }

            var searchResponse = await _lavaNode.SearchAsync("music/"+TrackNumOrTitle);
            if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                searchResponse.LoadStatus == LoadStatus.NoMatches)
            {
                await ReplyAsync($"I wasn't able to find anything for `{TrackNumOrTitle}`. See the available doremi music list on ``doremi!mulist`` commands.");
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
                        Config.Music.queuedTrack[Context.Guild.Id.ToString()].Add(track.Title);
                    }

                    await ReplyAsync($":arrow_down: Enqueued {searchResponse.Tracks.Count} tracks.");
                }
                else
                {
                    var track = searchResponse.Tracks[0];
                    player.Queue.Enqueue(track);
                    //Config.Music.storedLavaTrack[Context.Guild.Id.ToString()].Add(track);
                    Config.Music.queuedTrack[Context.Guild.Id.ToString()].Add(track.Title);
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
                            //Config.Music.storedLavaTrack[Context.Guild.Id.ToString()].Add(track);
                            Config.Music.queuedTrack[Context.Guild.Id.ToString()].Add(track.Title);
                        }
                        else
                        {
                            player.Queue.Enqueue(searchResponse.Tracks[i]);
                            //Config.Music.storedLavaTrack[Context.Guild.Id.ToString()].Add(track);
                            Config.Music.queuedTrack[Context.Guild.Id.ToString()].Add(searchResponse.Tracks[i].Title);
                        }
                    }

                    await ReplyAsync($":arrow_down: Enqueued {searchResponse.Tracks.Count} tracks.");
                }
                else
                {
                    Config.Music.queuedTrack[Context.Guild.Id.ToString()].Add(track.Title);
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
                await ReplyAsync("Woaaah there, I'm not playing any tracks.");
                return;
            }

            var track = player.Track;
            var artwork = await track.FetchArtworkAsync();

            if (artwork == null)
            {
                await ReplyAsync("Music needs to be from youtube.");
                return;
            }

            var embed = new EmbedBuilder
            {
                Title = $"{track.Author} - {track.Title}",
                ThumbnailUrl = artwork,
                Url = track.Url
            }
                .AddField("Id", track.Id)
                .AddField("Duration", track.Duration)
                .AddField("Position", track.Position);

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
                await ReplyAsync("I cannot pause when I'm not playing anything!");
                return;
            }

            try
            {
                await player.PauseAsync();
                await ReplyAsync($":pause_button: Music Paused: {player.Track.Title}");
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
                await ReplyAsync("I cannot resume when I'm not playing anything!");
                return;
            }

            try
            {
                await player.ResumeAsync();
                await ReplyAsync($"Resumed: {player.Track.Title}");
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
            Config.Music.queuedTrack[Context.Guild.Id.ToString()].Clear();
            var player = _lavaNode.HasPlayer(Context.Guild)
                ? _lavaNode.GetPlayer(Context.Guild)
                : await _lavaNode.JoinAsync((Context.User as IVoiceState).VoiceChannel);
            player.Queue.Clear();
            await player.StopAsync();
            await ReplyAsync($":stop_button: Music Stopped.");
        }

        [Command("Skip"), Summary("Skip into next track")]
        public async Task SkipAsync()
        {
            var player = _lavaNode.GetPlayer(Context.Guild);
            //var player = _lavaNode.HasPlayer(Context.Guild)
            //    ? _lavaNode.GetPlayer(Context.Guild)
            //    : await _lavaNode.JoinAsync((Context.User as IVoiceState).VoiceChannel);
            //if (Config.Music.repeat == 0)
            //{
            //    Config.Music.storedLavaTrack.RemoveAt(0);
            //}

            if (!_lavaNode.TryGetPlayer(Context.Guild, out player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await ReplyAsync("Woaaah there, I can't skip when nothing is playing.");
                return;
            }

            var track = player.Track;

            player.Queue.Enqueue(player.Track);
            await player.SkipAsync();

            await ReplyAsync($"Music Skipped. Now Playing: {player.Track.Title}");

        }

        [Command("Volume"), Summary("Set the music player volume into <volume>. Max: 200")]
        public async Task SetVolume([Remainder] ushort volume)
        {
            await _lavaNode.GetPlayer(Context.Guild).UpdateVolumeAsync(volume);
            await ReplyAsync($":sound: Volume set to:{volume}");
        }

        [Command("Musiclist"), Alias("mulist"), Summary("Show all doremi music list")]
        public async Task ShowMusicList()
        {
            JObject jObj = Config.Music.jobjectfile;
            String musiclist="";
            for (int i = 0; i < (jObj.GetValue("musiclist") as JObject).Count; i++)
            {
                string replacedFilename = jObj.GetValue("musiclist")[(i + 1).ToString()]["filename"].ToString().Replace(".mp3","").Replace(".ogg","");
                string title = jObj.GetValue("musiclist")[(i + 1).ToString()]["title"].ToString();
                musiclist += $"[**{i+1}**] **{replacedFilename}** : {title}\n";
            }
            //for (int i = 0; i < Config.MusicList.arrMusicList.Count; i++)
            //{
            //    String seperatedMusicTitle = Config.MusicList.arrMusicList[i].Replace(".mp3", "").Replace(".ogg", "");//erase format
            //    String musiclist = $"[**{i + 1}**] **ojamajocarnival** : Ojamajo Carnival\n";
            //}

            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithAuthor(Config.Doremi.EmbedName, Config.Doremi.EmbedAvatarUrl)
                .WithTitle("Music List:")
                .WithDescription($"These are the music list that's available for me to play: " +
                $"You can use the **play** commands followed with the track number or title.\n" +
                $"Example: **doremi!play 1** or **doremi!play ojamajocarnival**")
                .AddField("[Num] Title",
                musiclist)
                .Build());
        }

        [Command("queue"), Alias("muq"), Summary("Show all music in queue list")]
        public async Task ShowMusicListQueue()
        {
            if (Config.Music.queuedTrack[Context.Guild.Id.ToString()].Count >= 1)
            {
                String musiclist = "";
                for (int i = 0; i < Config.Music.queuedTrack[Context.Guild.Id.ToString()].Count; i++)
                {
                    musiclist += $"[**{i + 1}**] **{Config.Music.queuedTrack[Context.Guild.Id.ToString()][i]}**\n";
                }

                await base.ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithTitle("Current music in queue:")
                    .AddField($"[Num] Title",
                    musiclist)
                    .Build());
            } else {
                await ReplyAsync($"No music on the current queue list.");
                return;
            }
            
        }

        //[Command("Musicrepeat"), Alias("murep")]
        //public async Task ToggleMusicRepeat([Remainder] string query)
        //{

        //    //if (!String.IsNullOrEmpty(query.ToString()))
        //    //{
        //    //    if (query.ToString() == "off")
        //    //    {
        //    //        Config.Music.repeat = 0;
        //    //    }
        //    //    else if (query.ToString() == "one")
        //    //    {
        //    //        Config.Music.repeat = 1;
        //    //    }
        //    //    else if(query.ToString() == "all")
        //    //    {
        //    //        Config.Music.repeat = 2;
        //    //    }

        //    //} else
        //    //{
        //    //    if (Config.Music.repeat == 0)
        //    //    {
        //    //        Config.Music.repeat = 1;
        //    //    } else if(Config.Music.repeat == 1)
        //    //    {
        //    //        Config.Music.repeat = 2;
        //    //    } else
        //    //    {
        //    //        Config.Music.repeat = 0;
        //    //    }
        //    //}

        //    query = query.ToLower();

        //    if (query == "off")
        //        Config.Music.repeat = 0;
        //    else if (query == "one")
        //        Config.Music.repeat = 1;
        //    else if (query == "all")
        //        Config.Music.repeat = 2;

        //    await ReplyAsync($"Music Repeat: {query}.");
        //    return;

        //}

        //[Command("Musicremove"), Alias("murem")]
        //public async Task RemoveMusicQueue()
        //{
        //    String musiclist = "";
        //    for (int i = 0; i < Config.Music.storedLavaTrack.Count; i++)
        //    {
        //        LavaTrack lt = Config.Music.storedLavaTrack[i];
        //        musiclist += $"[**{i + 1}**] **{lt.Title}**\n";
        //    }

        //    await base.ReplyAsync(embed: new EmbedBuilder()
        //        .WithColor(Config.Doremi.EmbedColor)
        //        .WithTitle("Current music in queue:")
        //        .AddField($"[Track No] Title",
        //        musiclist)
        //        .Build());
        //}

    }

    [Name("Interactive")]
    public class DoremiInteractive : InteractiveBase
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

        [Command("quiz", RunMode = RunMode.Async),Summary("I will give you some quiz about Doremi.")]
        public async Task Interact_Quiz()
        {
            Random rnd = new Random();
            int rndQuiz = rnd.Next(0, 4);

            String question, replyCorrect, replyWrong, replyEmbed;
            List<string> answer = new List<string>();
            String replyTimeout = "Time's up. Sorry but it seems you haven't answered yet.";

            if (rndQuiz == 1){
                question = "What is my favorite food?";
                answer.Add("steak");
                replyCorrect = "Ding Dong, correct! I love steak very much";
                replyWrong = "Sorry but that's wrong.";
                replyTimeout = "Time's up. My favorite food is steak.";
                replyEmbed = "https://66.media.tumblr.com/337aaf42d3fb0992c74f7f9e2a0bf4f6/tumblr_olqtewoJDS1r809wso1_500.png";
            } else if (rndQuiz == 2) {
                question = "Where do I attend my school?";
                answer.Add("misora elementary school"); answer.Add("misora elementary"); answer.Add("misora school");
                replyCorrect = "Ding Dong, correct!";
                replyWrong = "Sorry but that's wrong.";
                replyTimeout = "Time's up. I went to Misora Elementary School.";
                replyEmbed = "https://vignette.wikia.nocookie.net/ojamajowitchling/images/d/df/E.JPG/revision/latest?cb=20160108002304";
            } else if (rndQuiz == 3) {
                question = "What is my full name?";
                answer.Add("harukaze doremi"); answer.Add("doremi harukaze");
                replyCorrect = "Ding Dong, correct! Doremi Harukaze is my full name.";
                replyWrong = "Sorry but that's wrong.";
                replyTimeout = "Time's up. Doremi Harukaze is my full name.";
                replyEmbed = "https://i.pinimg.com/originals/e7/1c/ce/e71cce7499e4ea9f9520c6143c9672e7.jpg";
            } else {
                question = "What is my sister name?";
                answer.Add("pop"); answer.Add("harukaze pop"); answer.Add("pop harukaze");
                replyCorrect = "Ding Dong, that's correct. Pop Harukaze is my sister name.";
                replyWrong = "Sorry, wrong answer.";
                replyTimeout = "Time's up. My sister name is Pop Harukaze.";
                replyEmbed = "https://images-wixmp-ed30a86b8c4ca887773594c2.wixmp.com/f/6e3bcaa4-2e3a-4390-a51a-652dff45c0b6/d6r5yu6-bffc8dba-af11-4af3-856c-d8ce82efaba3.png/v1/fill/w_333,h_250,q_70,strp/pop_harukaze_by_xdnobody_d6r5yu6-250t.jpg?token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1cm46YXBwOjdlMGQxODg5ODIyNjQzNzNhNWYwZDQxNWVhMGQyNmUwIiwiaXNzIjoidXJuOmFwcDo3ZTBkMTg4OTgyMjY0MzczYTVmMGQ0MTVlYTBkMjZlMCIsIm9iaiI6W1t7ImhlaWdodCI6Ijw9MzAwIiwicGF0aCI6IlwvZlwvNmUzYmNhYTQtMmUzYS00MzkwLWE1MWEtNjUyZGZmNDVjMGI2XC9kNnI1eXU2LWJmZmM4ZGJhLWFmMTEtNGFmMy04NTZjLWQ4Y2U4MmVmYWJhMy5wbmciLCJ3aWR0aCI6Ijw9NDAwIn1dXSwiYXVkIjpbInVybjpzZXJ2aWNlOmltYWdlLm9wZXJhdGlvbnMiXX0.ZOzOlhlXguuSwk-EKwPjNIWywfRYeWRWKLOBQK4i5HY";
            }

            //response.Content.ToLower() to get the answer

            await ReplyAsync(question);
            //var response = await NextMessageAsync();
            //Boolean wrongLoop = false;
            Boolean correctAnswer = false;

            while (!correctAnswer)
            {
                var response = await NextMessageAsync();

                if (response == null){
                    await ReplyAsync(replyTimeout);
                    return;
                } else if (answer.Contains(response.Content.ToLower())) {
                    await ReplyAsync(replyCorrect, embed: new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithImageUrl(replyEmbed)
                    .Build());
                    correctAnswer = true;
                } else {
                    await ReplyAsync(replyWrong);
                }
            }
        }

        //[Command("quiz episodes", RunMode = RunMode.Async)]
        //public async Task Interact_Quiz_Episodes()
        //{
        //    string[,] arrRandomSeason1 = { 
        //        {"2","I Become Hazuki-chan","https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/ab/OD-EP2-01.png/revision/latest?cb=20191014181543"},
        //        {"3","The Transfer Student from Naniwa! Aiko Debuts","https://vignette.wikia.nocookie.net/ojamajowitchling/images/7/76/OD-EP3-01.png/revision/latest?cb=20191020221637"},
        //        {"4","It's Not Scary if We're All Witches","https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b5/OD-EP4-01.png/revision/latest?cb=20191021160947"},
        //        {"5","Grand Opening! Maho-dou","https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/56/OD-EP5-01.png/revision/latest?cb=20191028122639"},
        //        {"6","A Liar's First Friendship","https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/a2/OD-EP6-01.png/revision/latest?cb=20191103230145"},
        //        {"8","Go to the Witch World!!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/1/11/OD-EP8-01.png/revision/latest?cb=20191104092436"},
        //        {"11","Early Bird Marina and a Bouquet From the Heart","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e5/OD-EP11-01.png/revision/latest?cb=20191106191355"},
        //        {"12","A Wish for a Precious Shirt","https://vignette.wikia.nocookie.net/ojamajowitchling/images/6/63/OD-EP12-01.png/revision/latest?cb=20191107002353"},
        //        {"14","Laugh and Forgive Me!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/8e/OD-EP14-01.png/revision/latest?cb=20191107104808"},
        //        {"15","Majo Rika Goes to Kindergarten","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e9/OD-EP15-01.png/revision/latest?cb=20191107231142"},
        //        {"16","Fishing for Love","https://vignette.wikia.nocookie.net/ojamajowitchling/images/0/07/OD-EP16-01.png/revision/latest?cb=20191109124447"},
        //        {"17","Yada-kun is a Delinquent!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/f/f3/OD-EP17-01.png/revision/latest?cb=20191109135953"},
        //        {"18","Don't Use That! The Forbidden Magic","https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/34/OD-EP18-01.png/revision/latest?cb=20171021190450"},
        //        {"19","Hazuki-chan is Kidnapped!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/5d/OD-EP19-01.png/revision/latest?cb=20191113144549"},
        //        {"20","The Rival Debuts! The Maho-dou is in Big Trouble!!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/39/OD-EP20-01.png/revision/latest?cb=20191113182510"},
        //        {"21","Majoruka's Goods are full of danger!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/1/12/OD-EP21-01.png/revision/latest?cb=20191113215530"},
        //        {"22","The Road to being a level 6 Witch is Hard","https://vignette.wikia.nocookie.net/ojamajowitchling/images/f/f4/OD-EP22-01.png/revision/latest?cb=20191113223335"},
        //        {"23","Big Change! The Ojamajo's Test","https://vignette.wikia.nocookie.net/ojamajowitchling/images/4/4d/OD-EP23-01.png/revision/latest?cb=20191116123630"},
        //        {"24","Majoruka versus level 6 ojamajo!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/1/13/OD-EP24-01.png/revision/latest?cb=20191116140500"},
        //        {"25","Ojamajo Poppu appears!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/d/dc/OD-EP25-01.png/revision/latest?cb=20191116143237"},
        //        {"27","Oyajide arrives?!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/ee/OD-EP27-01.png/revision/latest?cb=20191116223858"},
        //        {"28","Love is a Windy Ride over a Plateau","https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/58/OD-EP28-01.png/revision/latest?cb=20191116234149"},
        //        {"29","The Tap Disappeared at the Festival!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/6/61/OD-EP29-01.png/revision/latest?cb=20191117003019"},
        //        {"30","I want to meet the ghost!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/f/f2/OD-EP30-01.png/revision/latest?cb=20191117011838"},
        //        {"31","Present from Mongolia","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e4/OD-EP31-01.png/revision/latest?cb=20191117102136"},
        //        {"32","Overthrow Tamaki! the class election!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/6/68/OD-EP32-01.png/revision/latest?cb=20191117105859"},
        //        {"33","Panic at the Sports Festival","https://vignette.wikia.nocookie.net/ojamajowitchling/images/6/65/OD-EP33-01.png/revision/latest?cb=20191117171421"},
        //        {"34","I want to see my Mother!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/85/OD-EP34-01.png/revision/latest?cb=20191117174457"},
        //        {"35","The Transfer student is a Witch Apprentice?!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/0/0a/OD-EP35-01.png/revision/latest?cb=20191117183318"},
        //        {"36","Level four exam is Dododododo!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/59/OD-EP36-01.png/revision/latest?cb=20191117213655"},
        //        {"38","Ryota and the Midnight Monster","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e3/OD-EP38-01.png/revision/latest?cb=20191118104422"},
        //        {"41","Father and Son, the Move Towards Victory!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/4/43/OD-EP41-01.png/revision/latest?cb=20171021190911"},
        //        {"42","The Ojamajo's Fight for Justice!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/c/c2/OD-EP42-01.png/revision/latest?cb=20191118193414"},
        //        {"43","Papa, Fireworks, and Tearful Memories","https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/a1/OD-EP43-01.png/revision/latest?cb=20191118200017"},
        //        {"44","I Want to Be a Female Pro Wrestler!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/0/08/OD-EP44-01.png/revision/latest?cb=20191118205141"},
        //        {"45","Help Santa!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/96/OD-EP45-01.png/revision/latest?cb=20191118205624"},
        //        {"46","The Witches' Talent Show","https://vignette.wikia.nocookie.net/ojamajowitchling/images/6/61/OD-EP46-01.png/revision/latest?cb=20191119122536"},
        //        {"47","Fathers Arranged Marriage Meeting","https://vignette.wikia.nocookie.net/ojamajowitchling/images/0/0c/OD-EP47-01.png/revision/latest?cb=20191119122558"},
        //        {"48","Onpu's Mail is a Love Letter?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/0/07/OD-EP48-01.png/revision/latest?cb=20191120210149"},
        //        {"49","I Want to Meet Papa! The Dream Places on the Overnight Express","https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b9/OD-EP49-01.png/revision/latest?cb=20191120212755"},
        //        {"50","The Final Witch Apprentice Exam","https://vignette.wikia.nocookie.net/ojamajowitchling/images/c/c3/OD-EP50-01.png/revision/latest?cb=20191120231046"},
        //        {"51","Goodbye Maho-Dou","https://vignette.wikia.nocookie.net/ojamajowitchling/images/c/ce/OD-EP51-01.png/revision/latest?cb=20191120231059"},
        //    };
        //    string[,] arrRandomSeason2 = { 
        //        {"1","Doremi Becomes a Mom!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/ae/ODS-EP1-001.png/revision/latest?cb=20191122221644"},
        //        {"2","Raising a Baby is a Lot of Trouble!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/83/ODS-EP2-001.png/revision/latest?cb=20191124140100"},   
        //        {"3","Don't Fall Asleep! Pop's Witch Apprentice Exam","https://vignette.wikia.nocookie.net/ojamajowitchling/images/4/43/ODS-EP3-001.png/revision/latest?cb=20191124212633"},   
        //        {"4","Doremi Fails as a Mom!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/7/7e/ODS-EP4-001.png/revision/latest?cb=20191124235204"},   
        //        {"5","So Long, Oyajiide","https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/8e/ODS-EP5-001.png/revision/latest?cb=20191125102137"},   
        //        {"6","Lies and Truth in Flower Language","https://vignette.wikia.nocookie.net/ojamajowitchling/images/4/43/ODS-EP6-001.png/revision/latest?cb=20191125213656"},   
        //        {"7","Hana-chan's Health Examination","https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/5d/ODS-EP7-001.png/revision/latest?cb=20191125221342"},   
        //        {"8","Across Time, In Search of Onpu's Moms Secret!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/1/12/ODS-EP8-001.png/revision/latest?cb=20191130001508"},   
        //        {"9","The Search for the Herbs! Maho-dou's Bus Trip","https://vignette.wikia.nocookie.net/ojamajowitchling/images/d/d1/ODS-EP9-001.png/revision/latest?cb=20191201104930"},   
        //        {"11","Hazuki-chan Learns how to Dance!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b4/ODS-EP11-001.png/revision/latest?cb=20191201200624"},   
        //        {"12","The Health Examination's Yellow Cards!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b6/ODS-EP12-001.png/revision/latest?cb=20191202120414"},   
        //        {"13","Doremi Becomes a Bride?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/80/ODS-EP13-001.png/revision/latest?cb=20191204222102"},   
        //        {"14","Pop's First Love? Her Beloved Jyunichi-Sensei!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b9/ODS-EP14-001.png/revision/latest?cb=20191208134648"},   
        //        {"15","Mother's Day and the Drawing of Mother","https://vignette.wikia.nocookie.net/ojamajowitchling/images/2/23/ODS-EP15-001.png/revision/latest?cb=20191209133630"},   
        //        {"18","Dodo Runs Away From Home!!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/eb/ODS-EP18-001.png/revision/latest?cb=20191217185517"},   
        //        {"19","Doremi and Hazuki's Big Fight","https://vignette.wikia.nocookie.net/ojamajowitchling/images/c/cc/ODS-EP19-001.png/revision/latest?cb=20191217220830"},   
        //        {"21","The Misanthropist Majo Don and The Promise of The Herb","https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b9/ODS-EP21-001.png/revision/latest?cb=20191221130019"},   
        //        {"22","The Wizard's Trap - Oyajide Returns","https://vignette.wikia.nocookie.net/ojamajowitchling/images/6/66/ODS-EP22-001.png/revision/latest?cb=20191222224016"},   
        //        {"23","Using new powers to Rescue Hana-chan!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/0/07/ODS-EP23-001.png/revision/latest?cb=20191223102333"},   
        //        {"24","Fried Bread Power is Scary!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/33/ODS-EP24-001.png/revision/latest?cb=20191229162205"},   
        //        {"25","The Mysterious Pretty Boy, Akatsuki-kun Appears!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/0/0b/ODS-EP25-001.png/revision/latest?cb=20191230032553"},   
        //        {"26","Kanae-chan's Diet Plan","https://vignette.wikia.nocookie.net/ojamajowitchling/images/f/fb/ODS-EP26-001.png/revision/latest?cb=20200101191420"},   
        //        {"28","Health Examination Full of Hidden Dangers","https://vignette.wikia.nocookie.net/ojamajowitchling/images/d/db/ODS-EP28-001.png/revision/latest?cb=20200104231053"},   
        //        {"29","Everyone Disappears During the Test of Courage!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/32/ODS-EP29-001.png/revision/latest?cb=20200105222029"},   
        //        {"30","Seki-sensei's Got a Boyfriend!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/7/77/ODS-EP30-001.png/revision/latest?cb=20200105223509"},   
        //        {"31","The FLAT 4 Arrive from the Wizard World!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/9b/ODS-EP31-001.png/revision/latest?cb=20200109000219"},   
        //        {"32","Fly Away! Dodo and the Other Fairies' Big","https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/95/02.32.09.JPG/revision/latest?cb=20160104203250"},   
        //        {"33","Say Cheese During the Class Trip!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/91/02.33.07.JPG/revision/latest?cb=20160104204330"},   
        //        {"34","Takoyaki is the Taste of Making Up","https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/a4/02.33.06.JPG/revision/latest?cb=20160104203724"},   
        //        {"36","Aiko and her Rival! Sports Showdown!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/31/02.36.06.JPG/revision/latest?cb=20160104204841"},   
        //        {"38","Hazuki-chan's a Great Director!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/4/45/02.38.06.JPG/revision/latest?cb=20160104205546"},   
        //        {"39","A Selfish Child and the Angry Monster","https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/ae/39.07.JPG/revision/latest?cb=20160104205811"},   
        //        {"40","The Piano Comes to the Harukaze House!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/2/28/02.40.10.JPG/revision/latest?cb=20160104210153"},   
        //        {"41","Chase after Onpu! The Path to Becoming an Idol!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/32/02.41.06.JPG/revision/latest?cb=20160104210830"},   
        //        {"42","The Witch Who Does Not Cast Magic","https://vignette.wikia.nocookie.net/ojamajowitchling/images/7/7d/42.09.JPG/revision/latest?cb=20160104211048"},   
        //        {"44","A Happy White Christmas","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e9/02.44.05.JPG/revision/latest?cb=20160104211626"},   
        //        {"45","Ojamajo Era Drama: The Young Girls Show Their Valor!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/9b/02.45.08.JPG/revision/latest?cb=20160104211934"},   
        //        {"46","The Last Examination - Hana-chan's Mom Will Protect Her!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/f/fe/02.46.09.JPG/revision/latest?cb=20160104212224"},   
        //        {"47","Give Back Hana-chan! The Great Magic Battle","https://vignette.wikia.nocookie.net/ojamajowitchling/images/1/13/02.47.05.JPG/revision/latest?cb=20160104212503"},   
        //        {"49","Good Bye, Hana-chan","https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/8f/49.16.JPG/revision/latest?cb=20160104213105"},   
        //    };
        //    string[,] arrRandomSeason3 = {
        //        {"1","Doremi, a Stormy New Semester","https://vignette.wikia.nocookie.net/ojamajowitchling/images/4/44/Motto1-preop.png/revision/latest?cb=20171010213519"},
        //        {"2","Momoko Cried!? The Secret of the Earring","https://vignette.wikia.nocookie.net/ojamajowitchling/images/c/c7/02.15.JPG/revision/latest?cb=20151216152711"},
        //        {"3","I Hate You! But I Would Like To Be Your Friend!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/6/68/03.03.06.JPG/revision/latest?cb=20151216220704"},
        //        {"5","The SOS Trio is Disbanding!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/55/03.05.02.JPG/revision/latest?cb=20151216223354"},
        //        {"6","Challenge! The First Patissiere Exam","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/ea/06.10.JPG/revision/latest?cb=20151216231954"},
        //        {"8","What Are True Friends?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b9/03.08.07.JPG/revision/latest?cb=20151217022824"},
        //        {"9","Hazuki and Masaru's Treasure","https://vignette.wikia.nocookie.net/ojamajowitchling/images/7/7c/09.02.JPG/revision/latest?cb=20151128023340"},
        //        {"10","I Don't Want to Become an Adult!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b5/03.10.06.JPG/revision/latest?cb=20151218113031"},
        //        {"11","The Unstoppable Teacher!!","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e4/03.11.06.JPG/revision/latest?cb=20151220124444"},
        //        {"12","Kotake VS Demon Coach Igarashi","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/ed/12.07.JPG/revision/latest?cb=20151220150403"},
        //        {"14","An Up and Down Happy Birthday","https://vignette.wikia.nocookie.net/ojamajowitchling/images/1/1b/14.07.JPG/revision/latest?cb=20151220152430"},
        //        {"16","Just Being Delicious is Not Enough!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/7/71/16.07.JPG/revision/latest?cb=20151220154907"},
        //        {"17","Her Destine Rival!! Harukaze and Tamaki","https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/99/03.17.09.JPG/revision/latest?cb=20151220160308"},
        //        {"18","Scoop! A Child Idol's Day","https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/ec/18.09.JPG/revision/latest?cb=20151220162822"},
        //        {"19","Nothing but Fights, Like Father, Like Son","https://vignette.wikia.nocookie.net/ojamajowitchling/images/7/74/03.19.05.JPG/revision/latest?cb=20151221002953"},
        //        {"21","We're Out of Magical Ingredient","https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/a7/21.11.JPG/revision/latest?cb=20151221005022"},
        //        {"23","Clams By the Shore","https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/bd/03.23.050.JPG/revision/latest?cb=20151221010658"},
        //        {"24","Rock and Roll in the Music Club!?","https://vignette.wikia.nocookie.net/ojamajowitchling/images/7/76/03.24.06.JPG/revision/latest?cb=20151221011438"},
        //        {"25","A Lonely Summer Vacation","https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/ad/03.25.05.JPG/revision/latest?cb=20151221012136"},
        //        {"26","Deliver Her Feelings! Aiko Goes to Osaka","https://vignette.wikia.nocookie.net/ojamajowitchling/images/a/ae/26.09.JPG/revision/latest?cb=20151221162214"},
            
        //    };
        //}


        //PagedReplyAsync will send a paginated message to the channel
        //You can customize the paginator by creating a PaginatedMessage object
        //You can customize the criteria for the paginator as well, which defaults to restricting to the source user
        // This method will not block.
        //[Command("paginator")]
        //public async Task Test_Paginator()
        //{
        //    PaginatedMessage page = new PaginatedMessage();
        //    var pages = new[] { "Page 1", "Page 2", "Page 3", "aaaaaa", "Page 5" };

        //    await PagedReplyAsync(pages);
        //}

    }

    /*backup for basic music modules:
    public class DoremiMusic : ModuleBase<SocketCommandContext>
    {
        //resource: https://gist.github.com/Joe4evr/773d3ce6cc10dbea6924d59bbfa3c62a
        //a modules stops existing when a command is done executing and services exist aslong we did not dispose them

        // Scroll down further for the AudioService.
        // Like, way down
        private readonly AudioService _service;

        // Remember to add an instance of the AudioService
        // to your IServiceCollection when you initialize your bot
        public DoremiMusic(AudioService service)
        {
            _service = service;
        }

        // You *MUST* mark these commands with 'RunMode.Async'
        // otherwise the bot will not respond until the Task times out.
        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinCmd()
        {
            await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        }

        // Remember to add preconditions to your commands,
        // this is merely the minimal amount necessary.
        // Adding more commands of your own is also encouraged.
        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveCmd()
        {
            await _service.LeaveAudio(Context.Guild);
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayCmd([Remainder] string song)
        {
            await _service.SendAudioAsync(Context.Guild, Context.Channel, "music/" + song + ".mp3");
        }

        [Command("stop", RunMode = RunMode.Async)]
        public async Task StopCmd([Remainder] string song)
        {
            await _service.SendAudioAsync(Context.Guild, Context.Channel, "music/" + song + ".mp3");
        }

    }
    */

    //backup for help modules:
    /*
    public class DoremiHelpModule : ModuleBase
    {
        private readonly CommandService _commands;
        private readonly IServiceProvider _map;

        public DoremiHelpModule(IServiceProvider map, CommandService commands)
        {
            _commands = commands;
            _map = map;
        }

        [Command("help")]
        [Summary("Lists this bot's commands.")]
        public async Task Help(string path = "")
        {
            EmbedBuilder output = new EmbedBuilder();
            if (path == "")
            {
                output.Title = $"{Config.Doremi.EmbedName} Command List";

                foreach (var mod in _commands.Modules.Where(m => m.Parent == null))
                {
                    AddHelp(mod, ref output);
                }

                output.Footer = new EmbedFooterBuilder
                {
                    Text = "Use 'help <category>' to get help with a module."
                };
            }
            else
            {
                var mod = _commands.Modules.FirstOrDefault(m => m.Name.Replace("Module", "").ToLower() == path.ToLower());
                if (mod == null) { await ReplyAsync("No module could be found with that name."); return; }

                output.Title = mod.Name;
                output.Description = $"{mod.Summary}\n" +
                (!string.IsNullOrEmpty(mod.Remarks) ? $"({mod.Remarks})\n" : "") +
                (mod.Aliases.Any() ? $"Prefix(es): {string.Join(",", mod.Aliases)}\n" : "") +
                (mod.Submodules.Any() ? $"Submodules: {mod.Submodules.Select(m => m.Name)}\n" : "") + " ";
                AddCommands(mod, ref output);
            }

            await ReplyAsync("", embed: output.Build());
        }

        public void AddHelp(ModuleInfo module, ref EmbedBuilder builder)
        {
            foreach (var sub in module.Submodules) AddHelp(sub, ref builder);
            builder.AddField(f =>
            {
                f.Name = $"**{module.Name}**";
                f.Value = $"Submodules: {string.Join(", ", module.Submodules.Select(m => m.Name))}" +
                $"\n" +
                $"Commands: {string.Join(", ", module.Commands.Select(x => $"`{x.Name}`"))}";
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

        public void AddCommand(CommandInfo command, ref EmbedBuilder builder)
        {
            builder.AddField(f =>
            {
                f.Name = $"**{command.Name}**";
                f.Value = $"{command.Summary}\n" +
                (!string.IsNullOrEmpty(command.Remarks) ? $"({command.Remarks})\n" : "") +
                (command.Aliases.Any() ? $"**Aliases:** {string.Join(", ", command.Aliases.Select(x => $"`{x}`"))}\n" : "") +
                $"**Usage:** `{GetPrefix(command)} {GetAliases(command)}`";
            });
        }

        public string GetAliases(CommandInfo command)
        {
            StringBuilder output = new StringBuilder();
            if (!command.Parameters.Any()) return output.ToString();
            foreach (var param in command.Parameters)
            {
                if (param.IsOptional)
                    output.Append($"[{param.Name} = {param.DefaultValue}] ");
                else if (param.IsMultiple)
                    output.Append($"|{param.Name}| ");
                else if (param.IsRemainder)
                    output.Append($"...{param.Name} ");
                else
                    output.Append($"<{param.Name}> ");
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
                output += string.Concat(module.Aliases.FirstOrDefault(), " ");
            return output;
        }
    }
    */

}
