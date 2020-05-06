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
using System.Threading;
using System.Net;
using System.Drawing;

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
            "Fill <form> with: **default/sharp/royal/motto/dokkan** to make it spesific form.")]
        public async Task transform(string form = "dokkan")
        {
            IDictionary<string, string> arrImage = new Dictionary<string, string>();
            arrImage["default"] = "https://cdn.discordapp.com/attachments/706812082368282646/706817977626001469/default.gif";
            arrImage["sharp"] = "https://cdn.discordapp.com/attachments/706812082368282646/706818340265656350/sharp.gif";
            arrImage["royal"] = "https://cdn.discordapp.com/attachments/706812082368282646/706818543647457310/royal.gif";
            arrImage["motto"] = "https://cdn.discordapp.com/attachments/706812082368282646/706818722400043008/motto.gif";
            arrImage["dokkan"] = "https://cdn.discordapp.com/attachments/706812082368282646/706818945197539438/dokkan.gif";

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
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e5/No.078.jpg")
            .WithFooter("[Ojamajo Witchling Wiki](https://ojamajowitchling.fandom.com/wiki/Mimi)")
            .Build());
        }

        [Command("gigantamax"), Summary("I will turn into gigantamax form")]
        public async Task gigantamaxAiko()
        {
            string[] arrRandom = {
                "This is not my final form!", "Pameruku raruku rarirori poppun! Turn me make me bigger!",
                "Meet the gigantamax Aiko!","Aiko has been gigantamax-ed!",
                "Gigantamax Aiko ready for action!", "Muahaha! I have been gigantamax-ed!",
                "A wishing star has shined upon Aiko's forehead.\nAiko has been gigantamax-ed",
                "Shiny Forehead Aiko has appeared!"
            };

            await ReplyAsync(embed: new EmbedBuilder()
                    .WithAuthor("Gigantamax Aiko", "https://cdn.discordapp.com/attachments/569409307100315651/651127198203510824/unknown.png")
                    .WithDescription($"Level: 99\nHP: 999/999\n{arrRandom[new Random().Next(0, arrRandom.GetLength(0))]}")
                    .WithColor(Config.Aiko.EmbedColor)
                    .WithImageUrl("https://cdn.discordapp.com/attachments/569409307100315651/651127198203510824/unknown.png")
                    .WithFooter("Contributed by: Letter Three")
                    .Build());
        }

        [Command("happy birthday"), Summary("Give Aiko some wonderful birthday wishes. This commands only available on her birthday.")]
        public async Task aikoBirthday([Remainder] string wishes = "")
        {
            string[] arrResponse = new string[] { $":smile: Thank you for your wonderful birthday wishes, {Context.User.Mention}.",
                $":smile: Thank you {Context.User.Mention}, for the wonderful birthday wishes."};
            string[] arrResponseImg = new string[]{
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/94/ODN-EP10-097.png",
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e9/10.04.JPG"
            };

            if (DateTime.Now.ToString("dd") == Config.Aiko.birthdayDate.ToString("dd") &&
                DateTime.Now.ToString("MM") == Config.Aiko.birthdayDate.ToString("MM") &&
                Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour){
                await ReplyAsync(arrResponse[new Random().Next(0, arrResponse.Length)],
                embed: new EmbedBuilder()
                .WithColor(Config.Aiko.EmbedColor)
                .WithImageUrl(arrResponseImg[new Random().Next(0, arrResponseImg.Length)])
                .Build());
            } else {
                await ReplyAsync("Gomen ne, but it's not my birthday yet.",
                embed: new EmbedBuilder()
                .WithColor(Config.Aiko.EmbedColor)
                .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/1/1d/ODN-EP3-005.png")
                .Build());
            }
        }

        [Command("hello"), Summary("Yo, I will greet you up")]
        public async Task aikoHello()
        {
            List<string> listRandomRespond = new List<string>() {
                $"Yo {MentionUtils.MentionUser(Context.User.Id)}! ",
                $"Hi {MentionUtils.MentionUser(Context.User.Id)}! ",
            };

            int rndIndex = new Random().Next(0, listRandomRespond.Count);
            string tempReply = listRandomRespond[rndIndex] + Config.Aiko.arrRandomActivity[Config.Aiko.indexCurrentActivity, 1];

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

        [Command("random"), Alias("moments"), Summary("Show any random Aiko moments. " +
            "Fill <moments> with **random/first/sharp/motto/naisho/dokkan** for spesific moments.")]
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
                        path = "config/randomMoments/aiko";
                    else
                        path = "config/randomMoments/aiko/mix";

                    string randomPathFile = GlobalFunctions.getRandomFile(path, new string[] { ".png", ".jpg", ".gif", ".webm" });
                    await Context.Channel.SendFileAsync($"{randomPathFile}");
                    return;
                } else {
                    var key = Config.Aiko.jObjRandomMoments.Properties().ToList();
                    var randIndex = new Random().Next(0, key.Count);
                    moments = key[randIndex].Name;
                    getDataObject = (JArray)Config.Aiko.jObjRandomMoments[moments];
                    finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
                }    
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

        [Command("spooky", RunMode = RunMode.Async), Alias("exe","creepy"), Summary("Please don't use this commands...")]
        public async Task showSpookyAiko(){
            string mentionedUsername = MentionUtils.MentionUser(Context.User.Id);
            
            int randAngryAiko = new Random().Next(0, 11);
            if (randAngryAiko == 5) {
                string[] arrSentences = {
                    "Oy! Stop using this command. At least use another nice command for me will ya?!",
                    "I'm not letting you get the spooky Aiko this time!",
                    "Oy! Stop using this command!",
                    "Oy! Stop making fun of the spooky aiko!",
                    "No!I won't let you use this command!",
                    "I don't think you'll get spooky aiko this time!",
                    "You think it was spooky Aiko? You get this one instead!",
                    "You think it was spooky Aiko? It was me, the real Aiko!",
                    "I'm preventing you from getting the spooky Aiko!",
                    "Oy! At least use another nice commands for me, instead using the spooky aiko!"
                };

                await base.ReplyAsync(embed: new EmbedBuilder()
                    .WithAuthor(Config.Aiko.EmbedName, "https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b3/Linesticker14.png")
                    .WithDescription(arrSentences[new Random().Next(0, arrSentences.Length)])
                    .WithColor(Discord.Color.DarkerGrey)
                    .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b3/Linesticker14.png")
                    .Build());
                return;
            } else {
                string[] arrSentences = {
                    "Didn't I tell you already for not using this commands?!","spuɐɯɯoɔ sᴉɥʇ ƃuᴉsn ʇou ɹoɟ ʎpɐǝɹlɐ noʎ llǝʇ I ʇ,upᴉp",
                    "It's midnight already, you probably should go to sleep with me","ǝɯ ɥʇᴉʍ dǝǝls oʇ oƃ plnoɥs ʎlqɐqoɹd noʎ 'ʎpɐǝɹlɐ ʇɥƃᴉupᴉɯ s,ʇI",
                    "Beware of the Spooky Aiko","I’m not Aiko, I’m the Spooky aiko.","oʞᴉ∀ ʎʞoodS ǝɥʇ ɟo ǝɹɐʍǝq","˙oʞᴉɐ ʎʞoodS ǝɥʇ ɯ’I 'oʞᴉ∀ ʇou ɯ’I",
                    "Did you just steal my takoyaki?","¿ᴉʞɐʎoʞɐʇ ʎɯ lɐǝʇs ʇsnɾ noʎ pᴉp",
                    "Don't look over behind...","˙˙˙puᴉɥǝq ɹǝʌo ʞool ʇ,uop",
                    "Hello, please don't look at my face...","˙˙˙ǝɔɐɟ ʎɯ ʇɐ ʞool ʇ,uop ǝsɐǝld 'ollǝH",
                    "Pretty witchy exe chi","ᴉɥɔ ǝxǝ ʎɥɔʇᴉʍ ʎʇʇǝɹԀ",
                    "I'm right behind you...","˙˙˙noʎ puᴉɥǝq ʇɥƃᴉɹ ɯ,I",
                    "Why are you keep using this commands?","¿spuɐɯɯoɔ sᴉɥʇ ƃuᴉsn dǝǝʞ noʎ ǝɹɐ ʎɥM",
                    $"Don't worry {mentionedUsername}, I'll be right behind you...",$"˙˙˙noʎ puᴉɥǝq ʇɥƃᴉɹ ǝq ll,I ' {mentionedUsername} ʎɹɹoʍ ʇ,uop",
                    "Do you wanna know how did I get these eyes?","¿sǝʎǝ ǝsǝɥʇ ʇǝƃ I pᴉp ʍoɥ ʍouʞ ɐuuɐʍ noʎ op",
                    "There is someone... lurking behind you...","˙˙˙noʎ puᴉɥǝq ƃuᴉʞɹnl ˙˙˙ǝuoǝɯos sᴉ ǝɹǝɥ┴",
                    "I'm sorry, but Aiko cannot be found...","˙˙˙punoɟ ǝq ʇouuɐɔ oʞᴉ∀ ʇnq 'ʎɹɹos ɯ,I",
                    "....","Pretty...witchy..aiko...chi...","˙˙˙ᴉɥɔ˙˙˙oʞᴉɐ˙˙ʎɥɔʇᴉʍ˙˙˙ʎʇʇǝɹԀ",
                    $"Please make me some Takoyaki, {mentionedUsername}...",$"˙˙˙{mentionedUsername} 'ᴉʞɐʎoʞɐ┴ ǝɯos ǝɯ ǝʞɐɯ ǝsɐǝlԀ",
                    "Let's be my friend, will you?","¿noʎ llᴉʍ 'puǝᴉɹɟ ʎɯ ǝq s,ʇǝ˥",
                    $"Let's play together with me, {mentionedUsername}",$"{mentionedUsername} 'ǝɯ ɥʇᴉʍ ɹǝɥʇǝƃoʇ ʎɐld s,ʇǝ˥",
                    "Do You Like Spooky Aiko? Well here I am...","˙˙˙ɯɐ I ǝɹǝɥ llǝM ¿oʞᴉ∀ ʎʞoodS ǝʞᴉ˥ no⅄ op",
                    "Do You Want To Play A Game?","¿ǝɯɐפ ∀ ʎɐlԀ o┴ ʇuɐM no⅄ op",
                    "Be Afraid, Be Very Afraid.","˙pᴉɐɹɟ∀ ʎɹǝΛ ǝq 'pᴉɐɹɟ∀ ǝq",
                    "Whatever You Do, Don’t Fall Asleep.","˙dǝǝls∀ llɐℲ ʇ’uop 'op no⅄ ɹǝʌǝʇɐɥM",
                    "Tasty, tasty, beautiful fear.","˙ɹɐǝɟ lnɟᴉʇnɐǝq 'ʎʇsɐʇ 'ʎʇsɐ┴",
                    $"I am your number one fan, {mentionedUsername}.",$"{mentionedUsername} 'uɐɟ ǝuo ɹǝqɯnu ɹnoʎ ɯɐ I",
                    "Hi, I’m Spooky Aiko. Wanna play?", "Here's Spooky Aiko!","¿ʎɐld ɐuuɐM ˙oʞᴉ∀ ʎʞoodS ɯ’I 'ᴉH","¡oʞᴉ∀ ʎʞoodS s,ǝɹǝH",
                    $"I’m your friend now, {mentionedUsername}.",$"˙ {mentionedUsername} 'ʍou puǝᴉɹɟ ɹnoʎ ɯ’I"
                };

                string[] arrRandomAuthor = {
                    "4ik00","Aik0","a.i.k.o.e.x.e","aiko.exe",
                    "senoo.exe","s3n00.ex3","4k10 s3n0o","the spooky Aiko",
                    "th3 A1k0","A1k0","witchy.exe","spooky.exe",
                    "41k0.seno","s3n00 A1k0","a.i.k.o","a i k o s e n o o",
                    "aiko.exe","Sp00kiyaki","41k0","Takoyaki Girl.exe",
                    "T4k0y4k1","Takoyaki.exe","a.i.k.0.e.x..e","aaaiiikkkoo",
                    "aaaiii1ikk00","4ikk0000","Aaiikk00.exe","Blue.exe",
                    "A1k0000","01000001 01101001 01101011 01101111","A1kk000","Pretty...witchy.exe"
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
                    {"Letter Three","https://cdn.discordapp.com/attachments/643722270447239169/687814823014039795/unknown.png" }
                };

                int randomedResult = new Random().Next(0, arrRandom.GetLength(0));
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithAuthor(arrRandomAuthor[new Random().Next(0, arrRandomAuthor.Length)], arrRandom[randomedResult, 1])
                    .WithDescription(arrSentences[new Random().Next(0, arrSentences.Length)])
                    .WithColor(Discord.Color.DarkerGrey)
                    .WithImageUrl(arrRandom[randomedResult, 1])
                    .WithFooter($"Contributed by: {arrRandom[randomedResult, 0]}")
                    .Build());

                string[,] arrRandomCameo = {
                    {"Odd Meat","https://cdn.discordapp.com/attachments/643722270447239169/669581419701338132/002.png"},
                    {"Letter Three","https://cdn.discordapp.com/attachments/643722270447239169/669598054776569856/SPOILER_unknown.png"},
                    {"Letter Three","https://cdn.discordapp.com/attachments/643722270447239169/669603942090670080/Halloween_Hazuki.png"},
                    {"Letter Three","https://cdn.discordapp.com/attachments/643722270447239169/669606154946740224/Chop_Harukaze.png"},
                    {"Odd Meat","https://cdn.discordapp.com/attachments/643722270447239169/669618799762210846/unknown.png"},
                    {"Odd Meat","https://cdn.discordapp.com/attachments/421584908130189312/622528776747876362/cursed_majo_rika-export.gif"},
                    {"вештица","https://cdn.discordapp.com/attachments/643722270447239169/674417584325787676/creepyrika3.gif"},
                    {"Odd Meat","https://cdn.discordapp.com/attachments/644383823286763544/683436748960694308/gronpu.png"},
                    {"Odd Meat","https://cdn.discordapp.com/attachments/644383823286763544/683462806687055894/bluberraiko.png"},
                    {"Odd Meat","https://cdn.discordapp.com/attachments/644383823286763544/680524562357944403/cursed_80cgt3.gif"},
                    {"Nathan","https://cdn.discordapp.com/attachments/643722270447239169/679391488005767188/20200218_131725.jpg" },
                    {"Letter Three","https://cdn.discordapp.com/attachments/643722270447239169/687159873682669722/unknown.png"},
                    {"Letter Three","https://cdn.discordapp.com/attachments/644383823286763544/694792672023674910/SPOILER_unknown.png" },
                    {"Letter Three","https://cdn.discordapp.com/attachments/653690054912507914/702682994632163349/SPOILER_unknown.png" }

                };

                string[] arrRandomTextCameo = {
                    "Also, meet one of my finest creation...",
                    "Spooky aiko companion has arrived",
                    "Hello from spooky aiko??? companion"
                };

                int randomedResultCameo = new Random().Next(0, arrRandomCameo.GetLength(0));
                await ReplyAsync("Also, meet one of my finest creation...",
                    embed: new EmbedBuilder()
                    .WithAuthor("Spooky.exe???companion", arrRandomCameo[randomedResultCameo, 1])
                    .WithColor(Discord.Color.DarkerGrey)
                    .WithImageUrl(arrRandomCameo[randomedResultCameo, 1])
                    .WithFooter($"Contributed by: {arrRandomCameo[randomedResultCameo, 0]}")
                    .Build());

                if (new Random().Next(0, 2) == 1){
                    //trigger self executing commands
                    if (!Config.Aiko.hasSpookyAikoInvader.ContainsKey(Context.User.Id.ToString()))
                        Config.Aiko.hasSpookyAikoInvader.Add(Context.User.Id.ToString(), false);

                    if (!Config.Aiko.hasSpookyAikoInvader[Context.User.Id.ToString()])
                    {
                        Config.Aiko.hasSpookyAikoInvader[Context.User.Id.ToString()] = true;
                        string avatarUrl = Context.User.GetAvatarUrl().Replace("?size=128", "?size=512");
                        WebClient myWebClient = new WebClient();

                        //string file = attachments.ElementAt(0).Filename;
                        string extension = Path.GetExtension(avatarUrl).ToLower().Replace("?size=512", "");
                        string randomedFileName = DateTime.Now.ToString("yyyyMMdd_HHmm") + extension;
                        string completePath = $"attachments/{Context.Guild.Id}/{randomedFileName}";
                        byte[] buffer = myWebClient.DownloadData(avatarUrl);
                        Config.Core.ByteArrayToFile(completePath, buffer);

                        //process
                        Bitmap newBitmap;
                        using (var bitmap = (Bitmap)System.Drawing.Image.FromFile(completePath))
                            newBitmap = new Bitmap(ImageEditor.convertNegative(bitmap));

                        newBitmap.Save(completePath);//save the edited image file
                        newBitmap.Dispose();

                        string[] arrSentencesSelf = {
                            $"You just call me before, {Context.User.Username}. Now enjoy the {Context.User.Mention}!spooky commands",
                            $"spuɐɯɯoɔ ʎʞoods¡{Context.User.Mention} ǝɥʇ ʎoɾuǝ ʍoN ˙{Context.User.Username} 'ǝɹoɟǝq ǝɯ llɐɔ ʇsnɾ no⅄",
                            $"We meet again, {Context.User.Username}. But this time I'm executing the {Context.User.Mention}!spooky commands",
                            $"spuɐɯɯoɔ ʎʞoods¡{Context.User.Mention} ǝɥʇ ƃuᴉʇnɔǝxǝ ɯ,I ǝɯᴉʇ sᴉɥʇ ʇnq ˙{Context.User.Username} 'uᴉɐƃɐ ʇǝǝɯ ǝM",
                            $"{Context.User.Mention}!spooky. The forbidden command has been self executed for you, {Context.User.Username}",
                            $"{Context.User.Username} 'noʎ ɹoɟ pǝʇnɔǝxǝ ɟlǝs uǝǝq sɐɥ puɐɯɯoɔ uǝppᴉqɹoɟ ǝɥ┴ ˙ʎʞoods¡{Context.User.Mention}",
                            $"Now executing the self {Context.User.Mention}!spooky commands. I hope you enjoy it...",
                            $"˙˙˙ʇᴉ ʎoɾuǝ noʎ ǝdoɥ I ˙spuɐɯɯoɔ ʎʞoods¡{Context.User.Mention} ɟlǝs ǝɥʇ ƃuᴉʇnɔǝxǝ ʍoN",
                            $"{Context.User.Mention}!spooky",
                            $"ʎʞoods¡{Context.User.Mention}",
                            $"{Context.User.Mention}!exe",
                            $"ǝxǝ¡{Context.User.Mention}",
                            $"{Context.User.Mention}!creepy",
                            $"ʎdǝǝɹɔ¡{Context.User.Mention}"
                        };
                        string[] arrDescriptionSelf ={
                            $"{Context.User.Mention} has been locked under Aiko.exe commands.",
                            $"{Context.User.Mention} has been captured by Aiko.exe",
                            $"Thank you for releasing me earlier. Now I'm going to lock you up, {Context.User.Mention}.",
                            $"Now capturing {Context.User.Mention}....\nCapture completed.",
                            $"Now cloning dark {Context.User.Mention}....\nCloning process complete",
                        };
                        Config.Aiko._timerSpookyInvader[Context.User.Id.ToString()] = new Timer(async _ =>
                            await ReplyAsync(arrSentencesSelf[new Random().Next(0, arrSentencesSelf.Length)],
                                embed: new EmbedBuilder()
                                .WithAuthor($"{Context.User.Username}.exe")
                                .WithDescription(arrDescriptionSelf[new Random().Next(0, arrDescriptionSelf.Length)])
                                .WithColor(Discord.Color.DarkerGrey)
                                .Build()),
                                null, 60000, Timeout.Infinite);

                        Config.Aiko._timerSpookyInvader[Context.User.Id.ToString()] = new Timer(async _ =>
                            await Context.Channel.SendFileAsync(completePath),
                            null, 62000, Timeout.Infinite);

                        Config.Aiko._timerSpookyInvader[Context.User.Id.ToString()] = new Timer(async _ =>
                            File.Delete(completePath),
                         null, 75000, Timeout.Infinite);

                        Config.Aiko._timerSpookyInvader[Context.User.Id.ToString()] = new Timer(async _ =>
                            Config.Aiko.hasSpookyAikoInvader[Context.User.Id.ToString()] = false,
                         null, 70000, Timeout.Infinite);

                        //end process
                        //=============
                    }
                }
                

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

        //spooky aiko invader on the server
    }

    [Name("minigame"), Group("minigame"), Summary("This category contains all Hazuki minigame interactive commands.")]
    public class AikoMinigameInteractive : InteractiveBase
    {
        [Command("score"), Summary("Show your current minigame score points.")]
        public async Task Show_Quiz_Score()
        {//show the player score
            var guildId = Context.Guild.Id;
            var userId = Context.User.Id;
            var quizJsonFile = (JObject)JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.minigameDataFileName}")).GetValue("score");
            int score = 0;
            if (quizJsonFile.ContainsKey(userId.ToString()))
                score = (int)quizJsonFile.GetValue(userId.ToString());
            await ReplyAsync($"\uD83C\uDFC6 Your minigame score points are: **{score}**");
            return;
        }

        [Command("leaderboard"), Summary("Show the top 10 player score points for minigame leaderboard.")]
        public async Task Show_Minigame_Leaderboard()
        {//show top 10 player score
            var guildId = Context.Guild.Id;
            var userId = Context.User.Id;

            var quizJsonFile = (JObject)JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.minigameDataFileName}")).GetValue("score");

            string finalText = "";
            EmbedBuilder builder = new EmbedBuilder();
            builder.Title = "\uD83C\uDFC6 Minigame Leaderboard";

            builder.Color = Config.Aiko.EmbedColor;

            if (quizJsonFile.Count >= 1)
            {
                builder.Description = "Here are the top 10 player score points for minigame leaderboard:";

                var convertedToList = quizJsonFile.Properties().OrderByDescending(p => (int)p.Value).ToList();
                int ctrExists = 0;
                for (int i = 0; i < quizJsonFile.Count; i++)
                {
                    SocketGuildUser userExists = Context.Guild.GetUser(Convert.ToUInt64(convertedToList[i].Name));
                    if (userExists != null)
                    {
                        finalText += $"{i + 1}. {MentionUtils.MentionUser(Convert.ToUInt64(convertedToList[i].Name))} : {convertedToList[i].Value} \n";
                        ctrExists++;
                    }
                    if (ctrExists >= 9) break;
                }
                builder.AddField("[Rank]. Name & Score", finalText);
            }
            else
            {
                builder.Description = "Currently there's no minigame leaderboard yet.";
            }

            await ReplyAsync(embed: builder.Build());

        }

        [Command("rockpaperscissor", RunMode = RunMode.Async), Alias("rps"), Summary("Play the Rock Paper Scissor minigame with Aiko. 20 score points reward.")]
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

            string[] arrWinReaction = { $"Better luck next time, {Context.User.Username}.","I win the game this round!" };//bot win
            string[] arrLoseReaction = { "I'm losing the game." };//bot lose
            string[] arrDrawReaction = { "Well, it's a draw." };//bot draw

            string textTemplate = $"emojicontext Aiko landed her **{MinigameCore.rockPaperScissor(randomGuess, guess)["randomResult"]}** against your **{guess}**. ";

            string picReactionFolderDir = "config/rps_reaction/aiko/";

            if (MinigameCore.rockPaperScissor(randomGuess, guess)["gameState"] == "win")
            { // player win
                int rndIndex = new Random().Next(0, arrLoseReaction.Length);

                picReactionFolderDir += "lose";
                textTemplate = textTemplate.Replace("emojicontext", ":clap:");
                textTemplate += $"{Context.User.Username} **win** the game! You got **20** score points.\n" +
                    $"\"{arrLoseReaction[rndIndex]}\"";

                var guildId = Context.Guild.Id;
                var userId = Context.User.Id;

                //save the data
                MinigameCore.updateScore(guildId.ToString(), userId.ToString(), 10);

            }
            else if (MinigameCore.rockPaperScissor(randomGuess, guess)["gameState"] == "draw")
            { // player draw
                int rndIndex = new Random().Next(0, arrDrawReaction.Length);
                picReactionFolderDir += "draw";
                textTemplate = textTemplate.Replace("emojicontext", ":x:");
                textTemplate += $"**The game is draw!**\n" +
                    $"\"{arrDrawReaction[rndIndex]}\"";
            }
            else
            { //player lose
                int rndIndex = new Random().Next(0, arrWinReaction.Length);
                picReactionFolderDir += "win";
                textTemplate = textTemplate.Replace("emojicontext", ":x:");
                textTemplate += $"{Context.User.Username} **lose** the game!\n" +
                    $"\"{arrWinReaction[rndIndex]}\"";
            }

            string randomPathFile = GlobalFunctions.getRandomFile(picReactionFolderDir, new string[] { ".png", ".jpg", ".gif", ".webm" });
            await ReplyAsync(textTemplate);
            await Context.Channel.SendFileAsync($"{randomPathFile}");
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

    [Name("Card"), Group("card"), Summary("This category contains all Aiko Trading card command.")]
    public class AikoTradingCardInteractive : InteractiveBase
    {

        [Command("capture", RunMode = RunMode.Async), Alias("catch"), Summary("Capture spawned card with Aiko.")]
        public async Task trading_card_aiko_capture()
        {
            //reference: https://www.newtonsoft.com/json/help/html/ModifyJson.htm
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
            string replyText = ""; string parent = "aiko";

            if (!File.Exists(playerDataDirectory))
            {
                replyText = "Gomen ne, please register yourself first with **do!card register** command.";
                await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Aiko.EmbedColor)
                .WithDescription(replyText)
                .WithImageUrl(TradingCardCore.Aiko.emojiError).Build());
                return;
            }
            else
            {
                JObject arrInventory = JObject.Parse(File.ReadAllText(playerDataDirectory));
                string spawnedCardId = Config.Guild.getPropertyValue(guildId,TradingCardCore.propertyId);
                string spawnedCardCategory = Config.Guild.getPropertyValue(guildId, TradingCardCore.propertyCategory);
                if (spawnedCardId != "" && spawnedCardCategory != "")
                {
                    if (spawnedCardId.Contains("ai"))//check if the card is aiko/not
                    {
                        
                        int catchState = 0;

                        //check last capture time
                        try
                        {
                            if ((string)arrInventory["catch_token"] == "" ||
                                (string)arrInventory["catch_token"] != Config.Guild.getPropertyValue(guildId,TradingCardCore.propertyToken))
                            {
                                int catchRate;

                                //init RNG catch rate
                                if (spawnedCardCategory.ToLower() == "normal")
                                {
                                    catchRate = new Random().Next(11);
                                    if (catchRate <= 9) catchState = 1;
                                }
                                else if (spawnedCardCategory.ToLower() == "platinum")
                                {
                                    catchRate = new Random().Next(11);
                                    if (catchRate <= 5) catchState = 1;
                                }
                                else if (spawnedCardCategory.ToLower() == "metal")
                                {
                                    catchRate = new Random().Next(11);
                                    if (catchRate <= 2) catchState = 1;
                                }

                                if (catchState == 1)
                                {
                                    //start read json
                                    var jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));

                                    string name = jObjTradingCardList[parent][spawnedCardCategory][spawnedCardId]["name"].ToString();
                                    string imgUrl = jObjTradingCardList[parent][spawnedCardCategory][spawnedCardId]["url"].ToString();
                                    string rank = jObjTradingCardList[parent][spawnedCardCategory][spawnedCardId]["0"].ToString();
                                    string star = jObjTradingCardList[parent][spawnedCardCategory][spawnedCardId]["1"].ToString();
                                    string point = jObjTradingCardList[parent][spawnedCardCategory][spawnedCardId]["2"].ToString();

                                    //check inventory
                                    if (arrInventory[parent][spawnedCardCategory].ToString().Contains(spawnedCardId))
                                    {//card already exist on inventory
                                        replyText = $":x: Sorry, I can't capture **{spawnedCardId} - {name}** because you have it already.";
                                    }
                                    else
                                    {//card not exist yet
                                        //save data:
                                        arrInventory["catch_attempt"] = (Convert.ToInt32(arrInventory["catch_attempt"]) + 1).ToString();
                                        arrInventory["catch_token"] = Config.Guild.getPropertyValue(guildId,TradingCardCore.propertyToken);
                                        JArray item = (JArray)arrInventory[parent][spawnedCardCategory];
                                        item.Add(spawnedCardId);
                                        File.WriteAllText(playerDataDirectory, arrInventory.ToString());

                                        string[] arrRandomFirstSentence = {
                                            "Congratulations,","Awesome!","Great one!"
                                        };

                                        await ReplyAsync($":white_check_mark: {arrRandomFirstSentence[new Random().Next(0, arrRandomFirstSentence.Length)]} " +
                                            $"**{Context.User.Username}** have successfully capture **{spawnedCardCategory}** card: **{name}**",
                                            embed: TradingCardCore.printCardCaptureTemplate(Config.Aiko.EmbedColor, name, imgUrl,
                                            spawnedCardId, spawnedCardCategory, rank, star, point, Context.User.Username, Config.Aiko.EmbedAvatarUrl)
                                            .Build());

                                        //check if player have captured all card/not
                                        if (((JArray)arrInventory["aiko"]["normal"]).Count >= TradingCardCore.Aiko.maxNormal &&
                                            ((JArray)arrInventory["aiko"]["platinum"]).Count >= TradingCardCore.Aiko.maxPlatinum &&
                                            ((JArray)arrInventory["aiko"]["metal"]).Count >= TradingCardCore.Aiko.maxMetal)
                                        {
                                            await ReplyAsync(embed: TradingCardCore
                                                .userCompleteTheirList(Config.Aiko.EmbedColor, "aiko",
                                                $":clap: Congratulations, **{Context.User.Username}** have successfully capture all **Aiko Card Pack**!",
                                                TradingCardCore.Aiko.emojiCompleteAllCard, guildId.ToString(),
                                                Context.User.Id.ToString())
                                                .Build());
                                        }

                                        //erase spawned instance
                                        TradingCardCore.resetSpawnInstance(guildId);
                                        return;
                                    }
                                }
                                else
                                {
                                    //save data:
                                    arrInventory["catch_attempt"] = (Convert.ToInt32(arrInventory["catch_attempt"]) + 1).ToString();
                                    arrInventory["catch_token"] = Config.Guild.getPropertyValue(guildId,TradingCardCore.propertyToken);
                                    File.WriteAllText(playerDataDirectory, arrInventory.ToString());
                                    replyText = $":x: I'm sorry {Context.User.Username}, but you **fail** to catch the card. Better luck next time.";
                                }
                            }
                            else
                            {
                                replyText = ":x: Sorry, please wait for the next card spawn.";
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }

                        
                    }
                    else
                    {
                        replyText = ":x: Sorry, I can't capture that card. Try to use the other ojamajo bot to capture this card.";
                    }

                }
                else
                {
                    replyText = ":x: Sorry, either this card has been captured by someone or not spawned anymore. Please wait for the card to spawn again.";
                }

            }

            //fail
            await ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Aiko.EmbedColor)
            .WithDescription(replyText)
            .WithImageUrl(TradingCardCore.Aiko.emojiError).Build());

        }

        //list all cards that have been collected
        [Command("inventory", RunMode = RunMode.Async), Summary("List all **Aiko** trading cards that you have collected.")]
        public async Task trading_card_open_inventory()
        {
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
            var jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));

            string replyText; string parent = "aiko";

            if (!File.Exists(playerDataDirectory)) //not registered yet
            {
                await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Aiko.EmbedColor)
                .WithDescription("Gomen ne, please register yourself first with **do!card register** command.")
                .WithImageUrl(TradingCardCore.Aiko.emojiError).Build());
            }
            else
            {
                var playerData = JObject.Parse(File.ReadAllText(playerDataDirectory));
                //var jParentObject = (JObject)Config.Core.jObjWiki["episodes"];

                EmbedBuilder builder = new EmbedBuilder()
                .WithColor(Config.Aiko.EmbedColor);

                try
                {
                    //normal category
                    string category = "normal"; var arrList = (JArray)playerData[parent][category];

                    if (arrList.Count >= 1)
                    {
                        await PagedReplyAsync(
                            TradingCardCore.printInventoryTemplate("aiko", "aiko", category, jObjTradingCardList, arrList, TradingCardCore.Aiko.maxNormal)
                        ) ;
                    }
                    else
                    {
                        await ReplyAsync(embed: TradingCardCore.printEmptyInventoryTemplate(
                            Config.Aiko.EmbedColor, "aiko", category, TradingCardCore.Aiko.maxNormal)
                            .Build());
                    }

                    //platinum category
                    category = "platinum"; arrList = (JArray)playerData[parent][category];
                    if (arrList.Count >= 1)
                    {
                        await PagedReplyAsync(
                            TradingCardCore.printInventoryTemplate("aiko", "aiko", category, jObjTradingCardList, arrList, TradingCardCore.Aiko.maxPlatinum)
                        );
                    }
                    else
                    {
                        await ReplyAsync(embed: TradingCardCore.printEmptyInventoryTemplate(
                            Config.Aiko.EmbedColor, "aiko", category, TradingCardCore.Aiko.maxPlatinum)
                            .Build());
                    }

                    //metal category
                    category = "metal"; arrList = (JArray)playerData[parent][category];
                    if (arrList.Count >= 1)
                    {
                        await PagedReplyAsync(
                            TradingCardCore.printInventoryTemplate("aiko", "aiko", category, jObjTradingCardList, arrList, TradingCardCore.Aiko.maxMetal)
                        );
                    }
                    else
                    {
                        await ReplyAsync(embed: TradingCardCore.printEmptyInventoryTemplate(
                            Config.Aiko.EmbedColor, "aiko", category, TradingCardCore.Aiko.maxMetal)
                            .Build());
                    }

                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }


            }

        }

        [Command("status", RunMode = RunMode.Async), Summary("Show your Trading Card Status report.")]
        public async Task trading_card_status()
        {
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";

            if (!File.Exists(playerDataDirectory))
            { //not registered yet
                await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Aiko.EmbedColor)
                .WithDescription("Gomen ne, please register yourself first with **do!card register** command.")
                .WithImageUrl(TradingCardCore.Aiko.emojiError).Build());
            }
            else
            {
                await ReplyAsync(embed: TradingCardCore.
                    printStatusTemplate(Config.Aiko.EmbedColor, Context.User.Username, guildId.ToString(), clientId.ToString())
                    .Build());
            }
        }

        [Command("detail", RunMode = RunMode.Async), Alias("info","look"), Summary("See the detail of Aiko card information from the <card_id>.")]
        public async Task trading_card_look(string card_id)
        {
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;
            string parent = "aiko";

            string category = "";

            if (card_id.Contains("aiP"))//platinum
                category = "platinum";
            else if (card_id.Contains("aiM"))//metal
                category = "metal";
            else if (card_id.Contains("ai"))
                category = "normal";

            try
            {
                //start read json
                string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
                JObject arrInventory = JObject.Parse(File.ReadAllText(playerDataDirectory));
                var jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));
                string name = jObjTradingCardList[parent][category][card_id]["name"].ToString();

                if (arrInventory[parent][category].ToString().Contains(card_id))
                {

                    string imgUrl = jObjTradingCardList[parent][category][card_id]["url"].ToString();
                    string rank = jObjTradingCardList[parent][category][card_id]["0"].ToString();
                    string star = jObjTradingCardList[parent][category][card_id]["1"].ToString();
                    string point = jObjTradingCardList[parent][category][card_id]["2"].ToString();

                    await ReplyAsync(embed: TradingCardCore.printCardDetailTemplate(Config.Aiko.EmbedColor, name,
                        imgUrl, card_id, category, rank, star, point)
                        .Build());
                }
                else
                {
                    await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Aiko.EmbedColor)
                    .WithDescription($"Sorry, you don't have: **{card_id} - {name}** card yet. Try capture it to look at this card.")
                    .WithImageUrl(TradingCardCore.Aiko.emojiError).Build());
                }

            }
            catch (Exception e)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Aiko.EmbedColor)
                .WithDescription("Sorry, I can't find that card ID.")
                .WithImageUrl(TradingCardCore.Aiko.emojiError).Build());
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
