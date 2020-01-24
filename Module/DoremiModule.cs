using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using OjamajoBot.Service;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        //[Command]
        //public async Task defaultMention()
        //{
        //    string tempReply = "";
        //    List<string> listRandomRespond = new List<string>() {
        //        $"Hii hii {MentionUtils.MentionUser(Context.User.Id)}! ",
        //        $"Hello {MentionUtils.MentionUser(Context.User.Id)}! ",
        //    };

        //    int rndIndex = new Random().Next(0, listRandomRespond.Count);
        //    tempReply = $"{listRandomRespond[rndIndex]}. I noticed that you're calling for me. Use {Config.Doremi.PrefixParent}help <commands or category> if you need any help with the commands.";
        //    await ReplyAsync(tempReply);
        //}

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
                                commandsModulesToList[i].Summary!="hidden"){
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
                        await ReplyAsync(embed: output.Build());
                        return;
                    } else {
                        await ReplyAsync($"Oops, I can't find any related help that you search for. " +
                            $"See `{Config.Doremi.PrefixParent[0]}help <commands or category>` for command help.");
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

            if (!object.ReferenceEquals(group, null)){
                group = category+" ";
            }
            completedText += $"**Example:** `{Config.Doremi.PrefixParent[0]}{group}{commands}";
            if (parameters != "") completedText += " "+parameters;
            completedText += "`\n";
            builder.AddField(commands, completedText);
        }

        public void AddHelp(ModuleInfo module, ref EmbedBuilder builder)
        {
            foreach (var sub in module.Submodules) AddHelp(sub, ref builder);

            if (module.Summary!="hidden")
                builder.AddField(f => {
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

        [Command("change"), Alias("henshin"), Summary("I will change into the ojamajo form. " +
            "Fill <form> with: **default/sharp/royal/motto** to make it spesific form.")]
        public async Task transform(string form = "motto")
        {
            IDictionary<string, string> arrImage = new Dictionary<string, string>();
            arrImage["default"] = "https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/bf/Ca-doremi.gif";
            arrImage["sharp"] = "https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/3c/Sh-doremi.gif";
            arrImage["royal"] = "https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/5d/Royaldoremi.gif";
            arrImage["motto"] = "https://vignette.wikia.nocookie.net/ojamajowitchling/images/7/7d/Mo-doremi.gif";

            if (arrImage.ContainsKey(form)){
                await ReplyAsync("Pretty Witchy Doremi Chi~\n");
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithImageUrl(arrImage[form])
                    .Build());
            } else {
                await ReplyAsync($"Sorry, I can't found that form. See `{Config.Doremi.PrefixParent[0]} help change` for help details");
            }
        }

        [Command("dorememe"), Alias("dorememes"), Summary("I will give you some random doremi related memes. " +
            "You can fill <contributor> with one of the available to make it spesific contributor.\nFill it with `list` to list all contributor.")]
        public async Task givedorememe([Remainder]string contributor="")
        {
            string finalUrl = ""; JArray getDataObject = null;
            contributor = contributor.ToLower();

            if (contributor == "list"){
                var key = Config.Doremi.jobjectdorememes.Properties().ToList();
                string listedContributor = "";
                for(int i = 0; i < key.Count; i++) listedContributor += $"{key[i].Name}\n";
                
                await base.ReplyAsync(embed: new EmbedBuilder()
                    .WithTitle("Dorememes listed contributor")
                    .WithDescription("Thank you to all of peoples that contributing dorememes. Here are all listed dorememes contributor:")
                    .AddField("Contributor in List", listedContributor)
                    .WithColor(Config.Doremi.EmbedColor)
                    .Build());
                return;
            } else if (contributor == "") {
                var key = Config.Doremi.jobjectdorememes.Properties().ToList();
                var randIndex = new Random().Next(0, key.Count);
                contributor = key[randIndex].Name;
                getDataObject = (JArray)Config.Doremi.jobjectdorememes[contributor];
                finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
            } else {
                if (Config.Doremi.jobjectdorememes.ContainsKey(contributor)){
                    getDataObject = (JArray)Config.Doremi.jobjectdorememes[contributor];
                    finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
                } else {
                    await base.ReplyAsync($"Oops, I can't found the specified contributor. " +
                        $"See `{Config.Doremi.PrefixParent[0]}help dorememe` for commands help.");
                    return;
                }
            }

            await base.ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Doremi.EmbedColor)
            .WithImageUrl(finalUrl)
            .WithFooter("Contributed by: "+contributor)
            .Build());

        }

        [Command("fairy"), Summary("I will show you my fairy info")]
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
            using (StreamWriter sw = (File.Exists($"attachments/{Context.Guild.Id}/feedback_{Context.Guild.Id}.txt")) ? File.AppendText($"attachments/{Context.Guild.Id}/feedback_{Context.Guild.Id}.txt") :
                    File.CreateText($"attachments/{Context.Guild.Id}/feedback_{Context.Guild.Id}.txt"))
                sw.WriteLine($"[{DateTime.Now.ToString("MM/dd/yyyy HH:mm")}]{Context.User.Mention}{Context.User.Username}:{feedback_message}");

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
            .AddField("Doremi Bot", "[Click here to invite Doremi Bot](https://discordapp.com/api/oauth2/authorize?client_id="+Config.Doremi.Id+"&permissions=2117532736&scope=bot)")
            .AddField("Hazuki Bot", "[Click here to invite Hazuki Bot](https://discordapp.com/api/oauth2/authorize?client_id=" + Config.Hazuki.Id + "&permissions=238419008&scope=bot)")
            .AddField("Aiko Bot", "[Click here to invite Aiko Bot](https://discordapp.com/api/oauth2/authorize?client_id=" + Config.Aiko.Id + "&permissions=238419008&scope=bot)")
            .AddField("Onpu Bot", "[Click here to invite Onpu Bot](https://discordapp.com/api/oauth2/authorize?client_id=" + Config.Onpu.Id + "&permissions=238419008&scope=bot)")
            .AddField("Momoko Bot", "[Click here to invite Momoko Bot](https://discordapp.com/api/oauth2/authorize?client_id=" + Config.Momoko.Id + "&permissions=238419008&scope=bot)")
            .Build());
        }

        [Command("magical stage"), Alias("magicalstage"), Summary("I will perform magical stage along with the other and make a <wishes>")]
        public async Task magicalStage([Remainder] string wishes)
        {
            if (wishes != null)
            {
                Config.Doremi.MagicalStageWishes = wishes;
                await ReplyAsync($"{MentionUtils.MentionUser(Config.Hazuki.Id)} Pirika pirilala, Nobiyaka ni!",
                embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/38/MagicalStageMottoDoremi.png")
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

        [Command("random"), Alias("moments"), Summary("Show any random Doremi moments. " +
            "Fill <moments> with **random/first/sharp/motto/naisho** for spesific moments.")]
        public async Task randomthing(string moments = "")
        {
            string finalUrl=""; string footerUrl = "";
            JArray getDataObject = null; moments = moments.ToLower();
            if (moments == ""){
                var key = Config.Doremi.jObjRandomMoments.Properties().ToList();
                var randIndex = new Random().Next(0, key.Count);
                moments = key[randIndex].Name;
                getDataObject = (JArray)Config.Doremi.jObjRandomMoments[moments];
                finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
            } else {
                if (Config.Doremi.jObjRandomMoments.ContainsKey(moments)){
                    getDataObject = (JArray)Config.Doremi.jObjRandomMoments[moments];
                    finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
                } else {
                    await base.ReplyAsync($"Oops, I can't found the specified moments. " +
                        $"See `{Config.Doremi.PrefixParent[0]}help random` for commands help.");
                    return;
                }
            }

            footerUrl = finalUrl;
            if (finalUrl.Contains("wikia")) footerUrl = "https://ojamajowitchling.fandom.com/";
            await base.ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Doremi.EmbedColor)
            .WithImageUrl(finalUrl)
            .WithFooter(footerUrl)
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
            ErrorMessage = "Oops, I need `manage channels` permission to use this command")]
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

        [Command("thank you"), Alias("thanks", "arigatou"), Summary("Say thank you to Doremi Bot")]
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
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/98/Dore-spell.gif/revision/latest?cb=20170814182746")
            .Build());
        }

        [Command("wish"), Summary("I will grant you a <wishes>")]
        public async Task wish([Remainder] string wishes)
        {
            await ReplyAsync($"Pirika pirilala poporina peperuto! {wishes}");
            await base.ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Doremi.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/98/Dore-spell.gif")
            .Build());
        }

        [Command("updates"), Summary("See what's new on Doremi & her other related bot")]
        public async Task showLatestUpdate()
        {
            await base.ReplyAsync(embed: new EmbedBuilder()
            .WithTitle("What's new?")
            .WithDescription("Pirika pirilala poporina peperuto! Show us what's new on doremi bot and her other friends!")
            .AddField("Summary",
            $"-Onpu & Momoko bot has arrived. You can invite them with `{Config.Doremi.PrefixParent[0]}invite` commands.\n" +
            $"-Doremi and her other friends has updated into **motto** version.\n" +
            $"-Doremi bot can now assign your birthday with `{Config.Doremi.PrefixParent[0]}birthday set commands`.\n" +
            "-Doremi bot now has wiki category that let you get the [wiki information](https://ojamajowitchling.fandom.com)\n" +
            "-Added more random moments image source for Doremi and other related bots.\n" +
            "-Doremi and her other friends now has individual greeting message.\n"+
            "-Aiko bot: `spooky` commands now has higher chance for you to execute it.\n")
            .AddField("Doremi bot updated commands", "**wiki category**,**birthday category**,**moderator category**")
            .AddField("Aiko bot updated commands", "`spooky`")
            .WithColor(Config.Doremi.EmbedColor)
            .WithFooter($"Last updated on {Config.Core.lastUpdate}")
            .Build());
        }

        //event schedule/reminder
        //vote for best characters
        //change into pet form for doremi & other bot
        //present: give a random present on reaction unwrapped
        //present to someone: give a random present on reaction unwrapped
        //todo/more upcoming commands: easter egg/hidden commands, set daily message announcement, gacha,
        //contribute caption for random things
        //user card maker, sing lyrics together with other ojamajo bot, birthday reminder, voting for best ojamajo bot, witch seeds to cast a spells
    }

    [Name("Birthday"), Group("birthday"), Summary("This commands category will give the birthday reminder into the group.")]
    public class DoremiBirthdayModule : InteractiveBase
    {
        [Command("set"), Summary("I will set your birthday date reminder. Format must be: **dd/mm/yyyy** or **dd/mm**. " +
            "Example: `do!birthday set 31/01/1993`")]
        public async Task setBirthdayDate(string DateMonthYear){
            var guildId = Context.Guild.Id;
            var userId = Context.User.Id;

            DateTime date;
            if (DateTime.TryParseExact(DateMonthYear, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date) ||
                DateTime.TryParseExact(DateMonthYear, "dd/MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                var guildJsonFile = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json"));
                var jobjbirthday = (JObject)guildJsonFile.GetValue("user_birthday");
                if (!jobjbirthday.ContainsKey(userId.ToString()))
                    jobjbirthday.Add(new JProperty(userId.ToString(), DateMonthYear));
                else
                    jobjbirthday[userId.ToString()] = DateMonthYear;

                await ReplyAsync($"{Config.Emoji.birthdayCake} Ok! Your birthday date has been set into: **{DateMonthYear}**. I will remind everyone on your birthday date.",
                        embed: new EmbedBuilder()
                        .WithColor(Config.Doremi.EmbedColor)
                        .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/0/06/DoremiLineOK.png")
                        .Build());

                File.WriteAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json", guildJsonFile.ToString());
            }
            else
            {
                await ReplyAsync("Sorry, you need to give me the correct birthday date format: `dd/mm/yyyy` or `dd/mm`.");
            }
        }

        [Command("search"), Summary("Search the birthday date for mentioned <username>. Returned birthday date format will be: `dd/mm`")]
        public async Task searchBirthdayDate(IUser username)
        {
            var guildId = Context.Guild.Id;
            var userId = username.Id; EmbedBuilder builder = new EmbedBuilder();
            var guildJsonFile = (JObject)JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json")).GetValue("user_birthday");
            var jobjbirthday = guildJsonFile.Properties().ToList();
            for (int i = 0; i < jobjbirthday.Count; i++)
            {
                var key = jobjbirthday[i].Name; var val = jobjbirthday[i].Value.ToString();
                if (userId.ToString() == key){
                    builder.ThumbnailUrl = username.GetAvatarUrl();
                    builder.Color = Config.Doremi.EmbedColor;
                    builder.Description = $"{username.Username} birthday will be celebrated on {val}";
                    await ReplyAsync(embed: builder.Build());
                    return;
                }
            }

            await ReplyAsync("Sorry, I can't found the birthday date for that user."); return;
        }

        [Command("show"), Summary("Show all wonderful people that will have birthday on this month.")]
        public async Task showAllBirthdayDate(){
            DateTime date; Boolean birthdayExisted = false;
            var thisMonth = DateTime.Now.ToString("MMMM");
            EmbedBuilder builder = new EmbedBuilder();
            var guildId = Context.Guild.Id;
            
            var guildJsonFile = (JObject)JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json")).GetValue("user_birthday");
            var jobjbirthday = guildJsonFile.Properties().ToList();
            
            for (int i = 0; i < jobjbirthday.Count; i++){
                var key = jobjbirthday[i].Name; var val = jobjbirthday[i].Value.ToString();
                //var birthdayMonth = "";
                if (DateTime.TryParseExact(val, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date)||
                    DateTime.TryParseExact(val, "dd/MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out date)){
                    if (date.ToString("MM") == DateTime.Now.ToString("MM")){
                        var username = Context.Guild.GetUser(Convert.ToUInt64(key)).Username;
                        builder.AddField(username, val, true);
                        birthdayExisted = true;
                    }   
                }
            }

            if (birthdayExisted){
                builder.Title = $"{Config.Emoji.birthdayCake} {DateTime.Now.ToString("MMMM")} Birthday List";
                builder.Description = $"Here are the list of all wonderful people that will have birthday on this month:";
                builder.Color = Config.Doremi.EmbedColor;
                await ReplyAsync(embed: builder.Build());
            } else {
                await ReplyAsync("We don't have people that will have birthday on this month.");
            }

        }

        [Command("remove"), Summary("Remove the birthday date reminder settings.")]
        public async Task removeBirthdayDate(){
            var guildId = Context.Guild.Id;
            var userId = Context.User.Id;
            var guildJsonFile = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json"));
            var jobjbirthday = (JObject)guildJsonFile.GetValue("user_birthday");
            if (jobjbirthday.ContainsKey(userId.ToString())){
                jobjbirthday.Remove(userId.ToString());
                File.WriteAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json", guildJsonFile.ToString());
                await ReplyAsync("Ok, your birthday date settings has been removed.",
                    embed: new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/0/06/DoremiLineOK.png")
                    .Build());
            } else {
                await ReplyAsync("Sorry, it seems you haven't set your birthday date yet.");
            }

        }

    }

    [Name("Wiki"), Group("wiki"), Summary("This commands category will get the information from [ojamajo witchling wiki](https://ojamajowitchling.fandom.com)")]
    public class DoremiWiki : InteractiveBase
    {
        [Command("episodes", RunMode = RunMode.Async), Alias("episode"), Summary("I will give all episodes list based on the season. " +
            "Fill the optional <season> parameter with `first`/`sharp`/`motto`/`naisho`/`dokkan` for spesific list.")]
        public async Task showEpisodesList(string season = "first")
        {
            var jParentObject = (JObject)Config.Core.jObjWiki["episodes"];
            if (jParentObject.ContainsKey(season))
            {
                List<string> pageContent = new List<string>();
                EmbedBuilder builder = new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor);

                try
                {
                    string title = $"**{season.First().ToString().ToUpper() + season.Substring(1)} Season Episodes List**\n";
                    var arrList = (JArray)Config.Core.jObjWiki.GetValue("episodes")[season];
                    string tempVal = title;
                    int currentIndex = 0;
                    for (int i = 0; i < arrList.Count; i++)
                    {
                        string replacedUrl = arrList[i].ToString().Replace(" ", "_");
                        replacedUrl = Config.Core.wikiParentUrl + replacedUrl.ToString().Replace("?", "%3F");
                        tempVal += $"Ep {i + 1}: [{arrList[i]}]({replacedUrl})\n";

                        if (currentIndex < 14) currentIndex++;
                        else {
                            pageContent.Add(tempVal);
                            currentIndex = 0;
                            tempVal = title;
                        }

                        if (i == arrList.Count - 1) pageContent.Add(tempVal);

                    }

                    await PagedReplyAsync(pageContent);
                }
                catch (Exception e) { Console.WriteLine("Doremi wiki episodes error:" + e.ToString()); }

            } else {
                await ReplyAsync($"I'm sorry, but I can't find that season. See `{Config.Doremi.PrefixParent[0]}help wiki episodes` for commands help.");
            }

        }

        [Command("witches", RunMode = RunMode.Async), Alias("witch"), Summary("I will give all witches characters list. " +
            "Fill the optional <characters> parameter with the available witches characters name.")]
        public async Task showCharactersWitches([Remainder]string characters = ""){
            EmbedBuilder builder = new EmbedBuilder();
            builder.Color = Config.Doremi.EmbedColor;

            if (characters==""){
                try {
                    builder.Title = "Witches Characters List";
                    var arrList = ((JObject)Config.Core.jObjWiki.GetValue("witches")).Properties().ToList();
                    for (int i = 0; i < arrList.Count; i++){
                        builder.AddField(arrList[i].Name, $"[wiki link]({Config.Core.wikiParentUrl + arrList[i].Value["url"]})", true);
                    }
                    builder.WithFooter($"I found {arrList.Count} witches characters from ojamajo witchling wiki");
                    await ReplyAsync(embed: builder.Build());
                    return;
                }
                catch (Exception e){ Console.WriteLine("Doremi wiki witches characters error:" + e.ToString()); }
            } else {
                    if (((JObject)Config.Core.jObjWiki.GetValue("witches")).ContainsKey(characters)){
                        var arrList = Config.Core.jObjWiki.GetValue("witches")[characters];
                        var arrListDetails = ((JObject)arrList).Properties().ToList();
                        builder.Title = characters.First().ToString().ToUpper()+ characters.Substring(1) + " Characters Info";
                        builder.Description = arrList["description"].ToString();
                        for (int i = 0; i < arrListDetails.Count; i++){
                            if(arrListDetails[i].Name.ToLower()!="url"&&
                               arrListDetails[i].Name.ToLower() != "img"&&
                               arrListDetails[i].Name.ToLower() != "name"&&
                               arrListDetails[i].Name.ToLower() != "description")
                            builder.AddField(arrListDetails[i].Name.ToString().First().ToString().ToUpper()+ arrListDetails[i].Name.ToString().Substring(1),
                                arrListDetails[i].Value.ToString().First().ToString().ToUpper()+ arrListDetails[i].Value.ToString().Substring(1), true);
                        }
                        builder.AddField("More info",
                                $"[Click here]({Config.Core.wikiParentUrl+arrList["url"].ToString()})", true);

                        builder.WithImageUrl(arrList["img"].ToString());
                        await ReplyAsync(embed: builder.Build());
                        return;
                    } else await ReplyAsync("I'm sorry, but I can't find that witches characters. " +
                        $"See `{Config.Doremi.PrefixParent[0]}wiki witches` to display all witches characters list.");
            }
        }

        [Command("wizards", RunMode = RunMode.Async), Alias("wizard"), Summary("I will give all wizards characters list. " +
            "Fill the optional <characters> parameter with the available wizards characters name.")]
        public async Task showCharactersWizards([Remainder]string characters = ""){
            EmbedBuilder builder = new EmbedBuilder();
            builder.Color = Config.Doremi.EmbedColor;

            if (characters == ""){
                try{
                    builder.Title = "Wizards Characters List";
                    var arrList = ((JObject)Config.Core.jObjWiki.GetValue("wizards")).Properties().ToList();
                    for (int i = 0; i < arrList.Count; i++)
                    {
                        builder.AddField(arrList[i].Name, $"[wiki link]({Config.Core.wikiParentUrl + arrList[i].Value["url"]})", true);
                    }
                    builder.WithFooter($"I found {arrList.Count} wizards characters from ojamajo witchling wiki");
                    await ReplyAsync(embed: builder.Build());
                    return;
                }
                catch (Exception e) { Console.WriteLine("Doremi wiki wizards characters error:" + e.ToString()); }
            } else {
                if (((JObject)Config.Core.jObjWiki.GetValue("wizards")).ContainsKey(characters)){
                    var arrList = Config.Core.jObjWiki.GetValue("wizards")[characters];
                    var arrListDetails = ((JObject)arrList).Properties().ToList();
                    builder.Title = characters.First().ToString().ToUpper() + characters.Substring(1) + " Characters Info";
                    builder.Description = arrList["description"].ToString();
                    for (int i = 0; i < arrListDetails.Count; i++)
                    {
                        if (arrListDetails[i].Name.ToLower() != "url" &&
                           arrListDetails[i].Name.ToLower() != "img" &&
                           arrListDetails[i].Name.ToLower() != "name" &&
                           arrListDetails[i].Name.ToLower() != "description")
                            builder.AddField(arrListDetails[i].Name.ToString().First().ToString().ToUpper() + arrListDetails[i].Name.ToString().Substring(1),
                                arrListDetails[i].Value.ToString().First().ToString().ToUpper() + arrListDetails[i].Value.ToString().Substring(1), true);
                    }
                    builder.AddField("More info",
                            $"[Click here]({Config.Core.wikiParentUrl + arrList["url"].ToString()})", true);

                    builder.WithImageUrl(arrList["img"].ToString());
                    await ReplyAsync(embed: builder.Build());
                    return;
                } else await ReplyAsync("I'm sorry, but I can't find that wizards characters. " +
                  $"See `{Config.Doremi.PrefixParent[0]}wiki wizards` to display all wizards characters list.");
            }
        }

    }

    [Name("mod"), Group("mod"), Summary("Basic moderator commands. Require `manage channels` permission")]
    [RequireUserPermission(ChannelPermission.ManageChannels,
        ErrorMessage = "Oops, You need to have the `manage channels` permission to use this command",
        NotAGuildErrorMessage = "Oops, You need to have the `manage channels` permission to use this command")]
    public class DoremiModerator : ModuleBase<SocketCommandContext>
    {
        [Command("user leave"), Summary("Set the leaving user notifications with **off** or **on**.")]
        public async Task assignUserLeavingNotification(string settings="off")
        {
            string replacedsettings = settings.Replace("off", "0").Replace("on", "1");
            Config.Guild.setPropertyValue(Context.Guild.Id, "user_leaving_notification", replacedsettings);
            await ReplyAsync($"**Leaving User Messages** has been turned **{settings}**.");
        }
        //leaving_message

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

        [Name("mod channels"), Group("channels"), Summary("These commands require `manage channels` permissions.")]
        public class DoremiModeratorChannels : ModuleBase<SocketCommandContext>
        {
            [Command("birthday"), Summary("Set Doremi Bot to make birthday announcement on <channel_name>.")]
            public async Task assignBirthdayChannel(SocketGuildChannel channel_name)
            {
                var guildId = channel_name.Guild.Id;
                Config.Guild.setPropertyValue(guildId, "id_birthday_announcement", channel_name.Id.ToString());
                if (Config.Doremi._timerBirthdayAnnouncement.ContainsKey(guildId.ToString()))
                    Config.Doremi._timerBirthdayAnnouncement[guildId.ToString()].Change(Timeout.Infinite, Timeout.Infinite);

                Config.Doremi._timerBirthdayAnnouncement[$"{guildId.ToString()}"] = new Timer(async _ =>
                {
                    var socketClient = Context.Client;
                    try
                    {
                        DateTime date; Boolean birthdayExisted = false;
                        //announce hazuki birthday
                        if (DateTime.Now.ToString("dd") == Config.Hazuki.birthdayDate.ToString("dd") &&
                        DateTime.Now.ToString("MM") == Config.Hazuki.birthdayDate.ToString("MM"))
                        {
                            var calculatedYear = Convert.ToInt32(DateTime.Now.ToString("yyyy")) - Convert.ToInt32(Config.Hazuki.birthdayDate.ToString("yyyy"));
                            await socketClient
                            .GetGuild(guildId)
                            .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guildId, "id_birthday_announcement")))
                            .SendMessageAsync($"{Config.Emoji.birthdayCake} Happy birthday to you, {MentionUtils.MentionUser(Config.Hazuki.Id)} chan. " +
                            $"She has turned into {calculatedYear} on this year. Let's give wonderful birthday wishes for her.");
                            birthdayExisted = true;
                        }

                        //announce aiko birthday
                        if (DateTime.Now.ToString("dd") == Config.Aiko.birthdayDate.ToString("dd") &&
                        DateTime.Now.ToString("MM") == Config.Aiko.birthdayDate.ToString("MM"))
                        {
                            var calculatedYear = Convert.ToInt32(DateTime.Now.ToString("yyyy")) - Convert.ToInt32(Config.Aiko.birthdayDate.ToString("yyyy"));
                            await socketClient
                            .GetGuild(guildId)
                            .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guildId, "id_birthday_announcement")))
                            .SendMessageAsync($"{Config.Emoji.birthdayCake} Happy birthday to our osakan friend: {MentionUtils.MentionUser(Config.Aiko.Id)} chan. " +
                            $"She has turned into {calculatedYear} on this year. Let's give some takoyaki and wonderful birthday wishes for her.");
                            birthdayExisted = true;
                        }

                        //announce onpu birthday
                        if (DateTime.Now.ToString("dd") == Config.Onpu.birthdayDate.ToString("dd") &&
                        DateTime.Now.ToString("MM") == Config.Onpu.birthdayDate.ToString("MM"))
                        {
                            var calculatedYear = Convert.ToInt32(DateTime.Now.ToString("yyyy")) - Convert.ToInt32(Config.Onpu.birthdayDate.ToString("yyyy"));
                            await socketClient
                            .GetGuild(guildId)
                            .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guildId, "id_birthday_announcement")))
                            .SendMessageAsync($"{Config.Emoji.birthdayCake} Happy birthday to our wonderful idol friend: {MentionUtils.MentionUser(Config.Onpu.Id)} chan. " +
                            $"She has turned into {calculatedYear} on this year. Let's give some wonderful birthday wishes for her.");
                            birthdayExisted = true;
                        }

                        //announce momoko birthday
                        if (DateTime.Now.ToString("dd") == Config.Momoko.birthdayDate.ToString("dd") &&
                        DateTime.Now.ToString("MM") == Config.Momoko.birthdayDate.ToString("MM"))
                        {
                            var calculatedYear = Convert.ToInt32(DateTime.Now.ToString("yyyy")) - Convert.ToInt32(Config.Momoko.birthdayDate.ToString("yyyy"));
                            await socketClient
                            .GetGuild(guildId)
                            .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guildId, "id_birthday_announcement")))
                            .SendMessageAsync($"{Config.Emoji.birthdayCake} Happy birthday to our wonderful friend: {MentionUtils.MentionUser(Config.Momoko.Id)} chan. " +
                            $"She has turned into {calculatedYear} on this year. Let's give some wonderful birthday wishes for her.");
                            birthdayExisted = true;
                        }

                        EmbedBuilder builder = new EmbedBuilder();
                        var guildJsonFile = (JObject)JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json")).GetValue("user_birthday");
                        var jobjbirthday = guildJsonFile.Properties().ToList();
                        for (int i = 0; i < jobjbirthday.Count; i++)
                        {
                            string birthdayMessage = "";
                            var key = jobjbirthday[i].Name; var val = jobjbirthday[i].Value.ToString();
                            //var birthdayMonth = "";
                            try{
                                var user = channel_name.GetUser(Convert.ToUInt64(key));
                                
                                if (DateTime.TryParseExact(val, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date) ||
                                DateTime.TryParseExact(val, "dd/MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                                {
                                    if (date.ToString("dd/MM") == DateTime.Now.ToString("dd/MM"))
                                    {
                                        string[] arrRandomedMessage = {
                                        $"{Config.Emoji.birthdayCake} Everyone, let's give a wonderful birthday wishes for: {user.Mention} ",
                                        $"{Config.Emoji.birthdayCake} Happy birthday to our wonderful friend: {user.Mention} . " +
                                        $"Please give some wonderful birthday wishes for {user.Mention}.",
                                        $"{Config.Emoji.birthdayCake} Everyone, we have important birthday announcement! Please give some wonderful birthday wishes for {user.Mention}."
                                    };
                                        birthdayMessage = arrRandomedMessage[new Random().Next(0, arrRandomedMessage.Length)];
                                        builder.ImageUrl = "https://i.4pcdn.org/s4s/1508005628768.jpg";
                                        birthdayExisted = true;

                                        await socketClient
                                        .GetGuild(guildId)
                                        .GetTextChannel(channel_name.Id)
                                        .SendMessageAsync(birthdayMessage);
                                    }
                                }
                            }
                            catch {
                                //remove the unknown user properties
                                //guildJsonFile.Property(key).Remove();
                                //File.WriteAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json", guildJsonFile.ToString());
                            }


                        }

                        if (birthdayExisted)
                            await socketClient
                            .GetGuild(guildId)
                            .GetTextChannel(channel_name.Id)
                            .SendMessageAsync(embed:builder.Build());
                    }
                    catch
                    {
                        Console.WriteLine($"Doremi Birthday Announcement Exception: Send message permissions has been missing {channel_name.Guild.Name} : {channel_name.Name}");
                    }
                },
                    null,
                    TimeSpan.FromSeconds(5), //time to wait before executing the timer for the first time
                    TimeSpan.FromHours(24) //time to wait before executing the timer again
                );

                await ReplyAsync($"{Config.Emoji.birthdayCake} **Birthday Announcement Channels** has been assigned into: {MentionUtils.MentionChannel(channel_name.Id)}");

            }

            [Command("random event"), Summary("Schedule Doremi Bot to make random event message on <channel_name> for every 24 hours")]
            public async Task assignRandomEventChannel(IGuildChannel channel_name)
            {
                Config.Guild.setPropertyValue(channel_name.GuildId, "id_random_event", channel_name.Id.ToString());

                if (Config.Doremi._timerRandomEvent.ContainsKey(channel_name.GuildId.ToString()))
                    Config.Doremi._timerRandomEvent[channel_name.GuildId.ToString()].Change(Timeout.Infinite, Timeout.Infinite);

                Config.Doremi._timerRandomEvent[$"{channel_name.GuildId.ToString()}"] = new Timer(async _ =>
                {
                    int rndIndex = new Random().Next(0, Config.Doremi.listRandomEvent.Count); //random the list value
                    Console.WriteLine("Doremi Random Event : " + Config.Doremi.listRandomEvent[rndIndex]);

                    var socketClient = Context.Client;
                    try
                    {
                        await socketClient
                        .GetGuild(channel_name.GuildId)
                        .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(channel_name.GuildId, "id_random_event")))
                        .SendMessageAsync(Config.Doremi.listRandomEvent[rndIndex]);
                    }
                    catch
                    {
                        Console.WriteLine($"Doremi Random Event Exception: Send message permissions has been missing {channel_name.Guild.Name} : {channel_name.Name}");
                    }
                },
                    null,
                    TimeSpan.FromHours(Config.Doremi.Randomeventinterval), //time to wait before executing the timer for the first time
                    TimeSpan.FromHours(Config.Doremi.Randomeventinterval) //time to wait before executing the timer again
                );

                await ReplyAsync($"**Random Event Channels** has been assigned into: {MentionUtils.MentionChannel(channel_name.Id)}");
            }

            //[Command("online")]
            //public async Task assignNotifOnline(IGuildChannel iguild)
            //{
            //    Config.Guild.assignId(iguild.GuildId, "id_notif_online", iguild.Id.ToString());
            //    await ReplyAsync($"**Bot Online Notification Channels** has been assigned into: {MentionUtils.MentionChannel(iguild.Id)}");
            //}

            [Command("remove settings"), Summary("Remove the settings on the assigned channels. " +
                "Current available settings: `birthday`/`random event`")]
            public async Task removeChannelSettings([Remainder]string settings)
            {
                string property = ""; Boolean propertyValueExisted = false;
                var guildId = Context.Guild.Id;
                var guildJsonFile = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json"));

                if (settings.ToLower() == "birthday"){
                    property = "birthday announcement";
                    if (Config.Guild.hasPropertyValues(guildId.ToString(), "id_birthday_announcement"))
                    {
                        propertyValueExisted = true;
                        Config.Guild.setPropertyValue(Context.Guild.Id, "id_birthday_announcement", "");
                        if (Config.Doremi._timerBirthdayAnnouncement.ContainsKey(Context.Guild.Id.ToString()))
                            Config.Doremi._timerBirthdayAnnouncement[Context.Guild.Id.ToString()].Change(Timeout.Infinite, Timeout.Infinite);
                    }
                } else if (settings.ToLower() == "random event")
                {
                    property = "random event";
                    if (Config.Guild.hasPropertyValues(guildId.ToString(),"id_random_event"))
                    {
                        propertyValueExisted = true;
                        Config.Guild.setPropertyValue(Context.Guild.Id, "id_random_event", "");
                        if (Config.Doremi._timerRandomEvent.ContainsKey(Context.Guild.Id.ToString()))
                            Config.Doremi._timerRandomEvent[Context.Guild.Id.ToString()].Change(Timeout.Infinite, Timeout.Infinite);
                    }
                }
                else
                {
                    await ReplyAsync($"Sorry, I can't found that channel settings"); return;
                }

                if (propertyValueExisted)
                    await ReplyAsync($"**{property} channels** settings has been removed.");
                else
                    await ReplyAsync($"**{property} channels** has no settings yet.");
            }
        }

    }

    [Summary("hidden")]
    public class DoremiMagicalStageModule : ModuleBase
    {
        //magical stage section
        [Command("Peruton Peton, Sawayaka ni!")]//from aiko
        public async Task magicalStagefinal()
        {
            if (Context.User.Id == Config.Momoko.Id){
                await ReplyAsync($"{MentionUtils.MentionUser(Config.Hazuki.Id)} Magical Stage! {Config.Doremi.MagicalStageWishes}\n");
                Config.Doremi.MagicalStageWishes = "";
            }
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
                Config.Music.queuedTrack[Context.Guild.Id.ToString()].Clear();
                await _lavaNode.LeaveAsync(voiceChannel);
                await ReplyAsync($"I've left {voiceChannel.Name}!");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Move"), Summary("Move doremi bot into your new connected voice channel")]
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
        [Command("youtube"), Alias("yt"), Summary("Play the youtube music. `<KeywordOrUrl>` parameter can be a search keyword or youtube url.")]
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
        public async Task PlayLocal([Remainder] string TrackNumbersOrTitle)
        {
            if (string.IsNullOrWhiteSpace(TrackNumbersOrTitle))
            {
                await ReplyAsync($"Please provide track numbers or title. Use {Config.Doremi.PrefixParent[0]}mulist to show all doremi music list.");
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
            if (int.TryParse(TrackNumbersOrTitle, out int n)) {
                
                if(n <= (jObj.GetValue("musiclist") as JObject).Count){
                    TrackNumbersOrTitle = jObj.GetValue("musiclist")[n.ToString()]["filename"].ToString();
                } else {
                    await ReplyAsync($"I can't find anything for track number {TrackNumbersOrTitle}. See the available doremi music list on `{Config.Doremi.PrefixParent[0]}mulist`.");
                    return;
                }
                
            } else {
                for (int i = 0; i < (jObj.GetValue("musiclist") as JObject).Count; i++)
                {
                    String replacedFilename = jObj.GetValue("musiclist")[(i + 1).ToString()]["filename"].ToString().Replace(".mp3", "").Replace(".ogg", "");
                    if (replacedFilename == TrackNumbersOrTitle)
                    {
                        TrackNumbersOrTitle = jObj.GetValue("musiclist")[(i + 1).ToString()]["filename"].ToString();
                    }
                    
                }
            }

            var searchResponse = await _lavaNode.SearchAsync("music/"+TrackNumbersOrTitle);
            if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                searchResponse.LoadStatus == LoadStatus.NoMatches)
            {
                await ReplyAsync($"I wasn't able to find anything for `{TrackNumbersOrTitle}`. See the available doremi music list on ``doremi!mulist`` commands.");
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
                await ReplyAsync("I'm not playing any tracks yet.");
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
                await ReplyAsync("Oops, I can't skip when nothing is playing.");
                return;
            }

            var track = player.Track;

            player.Queue.Enqueue(player.Track);
            await player.SkipAsync();

            await ReplyAsync($"Ok, music has been skipped. Now Playing: {player.Track.Title}");

        }

        [Command("Volume"), Summary("Set the music player volume into given <volume>. Max: 200")]
        public async Task SetVolume([Remainder] ushort volume)
        {
            await _lavaNode.GetPlayer(Context.Guild).UpdateVolumeAsync(volume);
            await ReplyAsync($":sound: Volume set to:{volume}");
        }

        [Command("Musiclist"), Alias("mulist"), Summary("Show all available doremi music list")]
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

            await ReplyAsync(embed: new EmbedBuilder()
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

    [Name("quiz"), Group("quiz"), Summary("This category contains all quiz interactive commands minigame.")]
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

        [Command("color", RunMode = RunMode.Async), Alias("colors"), Summary("Play the color terminology guessing games.")]
        public async Task Interact_Quiz_Colors()
        {
            //Config.Doremi.isRunningQuiz[Context.User.Id.ToString()] = true;
            if (!Config.Doremi.isRunningQuiz.ContainsKey(Context.User.Id.ToString()))
                Config.Doremi.isRunningQuiz.Add(Context.User.Id.ToString(), false);

            if (!Config.Doremi.isRunningQuiz[Context.User.Id.ToString()])
            {
                Config.Doremi.isRunningQuiz[Context.User.Id.ToString()] = true;
                int attempt = 5;
                var arrRandomed = (JArray)Config.Core.jobjectQuiz.GetValue("color");
                string randomedAnswer = arrRandomed[new Random().Next(0, arrRandomed.Count)].ToString();
                string replacedAnswer = ""; string[] containedAnswer = { }; List<string> guessedWord = new List<string>();
                for (int i = 0; i < randomedAnswer.Length; i++)
                    replacedAnswer += randomedAnswer.Substring(i, 1).Replace(randomedAnswer.Substring(i, 1), "_ ");

                string tempRandomedAnswer = string.Join(" ", randomedAnswer.ToCharArray()) + " "; //with space
                                                                                                  //Console.WriteLine(randomedAnswer);
                await ReplyAsync($"Can you guess what color is this?```{replacedAnswer}```");

                while (attempt > 0 && replacedAnswer.Contains("_"))
                {
                    Boolean isGuessed = false;
                    var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
                    string loweredResponse = response.Content.ToLower();

                    if (response == null)
                    {
                        Config.Doremi.isRunningQuiz[Context.User.Id.ToString()] = false;
                        await ReplyAsync("Time's up, sorry you're not guessing any words yet.");
                        return;
                    }
                    else if (loweredResponse.Length > 1)
                        await ReplyAsync($"Sorry, but you can only guess a word each turn.");
                    else if (loweredResponse == " ")
                        await ReplyAsync($"Sorry, but you can't enter a whitespace character.");
                    else if (loweredResponse.Length <= 1)
                    {
                        foreach (string x in guessedWord)
                        {
                            if (loweredResponse.Contains(x))
                            {
                                await ReplyAsync($"You already guessed **{x}**");
                                isGuessed = true;
                                return;
                            }
                        }

                        guessedWord.Add(loweredResponse);

                        if (!tempRandomedAnswer.Contains(loweredResponse) && !isGuessed)
                        {
                            await ReplyAsync($"Sorry, you guess it wrong.");
                            attempt -= 1;
                        }
                        else if (!isGuessed)
                        {
                            try
                            {
                                StringBuilder sb = new StringBuilder(replacedAnswer);
                                for (int i = 0; i < replacedAnswer.Length; i++)
                                {
                                    if (loweredResponse == tempRandomedAnswer[i].ToString())
                                    {
                                        sb[i] = loweredResponse.ToCharArray()[0];
                                    }
                                }
                                replacedAnswer = sb.ToString();
                            }
                            catch (Exception e) { Console.WriteLine(e.ToString()); }

                        }

                    }

                    await ReplyAsync($"Guessing attempt(s) left: **{attempt}**```{replacedAnswer}```");
                }

                if (replacedAnswer.Contains("_"))
                    await ReplyAsync($"Sorry, you're running out of guessing attempt. The correct answer is : **{randomedAnswer}**");
                else
                    await ReplyAsync($"Congratulations, you guess the correct answer: **{randomedAnswer}**");

                Config.Doremi.isRunningQuiz[Context.User.Id.ToString()] = false;
                return;
            }
            else
                await ReplyAsync($"Sorry, but you still have a running quiz interactive commands, please finish it first.");
            
        }

        [Command("doremi", RunMode = RunMode.Async), Summary("I will give you some quiz about Doremi.")]
        public async Task Interact_Quiz()
        {
            Random rnd = new Random();
            int rndQuiz = rnd.Next(0, 4);

            string question, replyCorrect, replyWrong, replyEmbed;
            List<string> answer = new List<string>();
            string replyTimeout = "Time's up. Sorry but it seems you haven't answered yet.";

            if (rndQuiz == 1)
            {
                question = "What is my favorite food?";
                answer.Add("steak");
                replyCorrect = "Ding Dong, correct! I love steak very much";
                replyWrong = "Sorry but that's wrong. Please retype the correct answer.";
                replyTimeout = "Time's up. My favorite food is steak.";
                replyEmbed = "https://66.media.tumblr.com/337aaf42d3fb0992c74f7f9e2a0bf4f6/tumblr_olqtewoJDS1r809wso1_500.png";
            }
            else if (rndQuiz == 2)
            {
                question = "Where do I attend my school?";
                answer.Add("misora elementary school"); answer.Add("misora elementary"); answer.Add("misora school");
                replyCorrect = "Ding Dong, correct!";
                replyWrong = "Sorry but that's wrong. Please retype the correct answer.";
                replyTimeout = "Time's up. I went to Misora Elementary School.";
                replyEmbed = "https://vignette.wikia.nocookie.net/ojamajowitchling/images/d/df/E.JPG";
            }
            else if (rndQuiz == 3)
            {
                question = "What is my full name?";
                answer.Add("doremi harukaze"); answer.Add("harukaze doremi");
                replyCorrect = "Ding Dong, correct! Doremi Harukaze is my full name.";
                replyWrong = "Sorry but that's wrong. Please retype the correct answer.";
                replyTimeout = "Time's up. Doremi Harukaze is my full name.";
                replyEmbed = "https://i.pinimg.com/originals/e7/1c/ce/e71cce7499e4ea9f9520c6143c9672e7.jpg";
            }
            else
            {
                question = "What is my sister name?";
                answer.Add("pop"); answer.Add("harukaze pop"); answer.Add("pop harukaze");
                replyCorrect = "Ding Dong, that's correct. Pop Harukaze is my sister name.";
                replyWrong = "Sorry, wrong answer. Please retype the correct answer.";
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

                if (response == null)
                {
                    await ReplyAsync(replyTimeout);
                    return;
                }
                else if (answer.Contains(response.Content.ToLower()))
                {
                    await ReplyAsync(replyCorrect, embed: new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithImageUrl(replyEmbed)
                    .Build());
                    correctAnswer = true;
                }
                else
                {
                    await ReplyAsync(replyWrong);
                }
            }
        }

    }
}
