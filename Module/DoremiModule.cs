using Config;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using OjamajoBot.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
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
        //    string tempReply = "";w
        //    List<string> listRandomRespond = new List<string>() {
        //        $"Hii hii {MentionUtils.MentionUser(Context.User.Id)}! ",
        //        $"Hello {MentionUtils.MentionUser(Context.User.Id)}! ",
        //    };

        //    int rndIndex = new Random().Next(0, listRandomRespond.Count);
        //    tempReply = $"{listRandomRespond[rndIndex]}. I noticed that you're calling for me. Use {Config.Doremi.PrefixParent}help <commands or category> if you need any help with the commands.";
        //    await ReplyAsync(tempReply);
        //}

        [Name("countdown"),Command("countdown"),Summary("Countdown to Doremi Movie")]
        public async Task countdownTimer()
        {
            DateTime endTime = new DateTime(2020, 05, 15, 2, 0, 0);//japan: time+2
            TimeSpan ts = endTime.Subtract(DateTime.Now);
            string timeLeft = ts.ToString("d' Days 'h' Hours 'm' Minutes 's' Seconds'");

            await ReplyAsync(embed: new EmbedBuilder()
                .WithAuthor("Doremi Movie Countdown",Config.Doremi.EmbedAvatarUrl)
                .WithTitle("Countdown to: Ojamajo Doremi: Majo Minarai o Sagashite")
                .WithColor(Config.Doremi.EmbedColor)
                //.WithDescription($"[Only **{timeLeft}** left until Doremi Movie!](https://www.lookingfor-magical-doremi.com/)")
                .WithDescription($"[Coming soon!](https://www.lookingfor-magical-doremi.com/news/67/)")
                .WithImageUrl("https://lookingfor-magical-doremi.com/teaser_v2/img/og-image.png")
                .Build());
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
            "Fill <form> with: **default/sharp/royal/motto/dokkan** to make it spesific form.")]
        public async Task transform(string form = "dokkan")
        {
            IDictionary<string, string> arrImage = new Dictionary<string, string>();
            arrImage["default"] = "https://cdn.discordapp.com/attachments/706812034544697404/706815418853228604/default.gif";
            arrImage["sharp"] = "https://cdn.discordapp.com/attachments/706812034544697404/706815522909716480/sharp.gif";
            arrImage["royal"] = "https://cdn.discordapp.com/attachments/706812034544697404/706815556480925787/royal.gif";
            arrImage["motto"] = "https://cdn.discordapp.com/attachments/706812034544697404/706815687322239046/motto.gif";
            arrImage["dokkan"] = "https://cdn.discordapp.com/attachments/706812034544697404/706815811406266478/dokkan.gif";

            if (arrImage.ContainsKey(form)){
                await ReplyAsync("Pretty Witchy Doremi Chi~\n", embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl(arrImage[form])
                .Build());
            } else {
                await ReplyAsync($"Sorry, I can't found that form. See `{Config.Doremi.PrefixParent[0]} help change` for help details");
            }
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
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e2/No.076.jpg")
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

        [Command("happy birthday"), Summary("Give Doremi some wonderful birthday wishes. This commands only available on her birthday.")]
        public async Task doremiBirthday([Remainder] string wishes = "")
        {
            string[] arrResponse = new string[] { $":smile: Oh, you actually remembered my birthday. Thank you, {Context.User.Mention}.",
                $":smile: Thank you {Context.User.Mention}, for the wonderful birthday wishes."};
            string[] arrResponseImg = new string[]{
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/b8/ODN-EP12-004.png",
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/30/ODN-EP2-017.png"
            };

            if (DateTime.Now.ToString("dd") == Config.Doremi.birthdayDate.ToString("dd") &&
                DateTime.Now.ToString("MM") == Config.Doremi.birthdayDate.ToString("MM") &&
                Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour){
                await ReplyAsync(arrResponse[new Random().Next(0, arrResponse.Length)],
                embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl(arrResponseImg[new Random().Next(0,arrResponseImg.Length)])
                .Build());
            } else {
                await ReplyAsync("I'm sorry, but it's not my birthday yet.",
                embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/6/6e/Hanabou2.jpg")
                .Build());
            }
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
                string message = $"Let's give a warm hugs for {MentionUtils.MentionUser(username.Id)} :hugging:";
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
            .AddField("Doremi Bot", "[Invite Doremi Bot](https://discordapp.com/api/oauth2/authorize?client_id=" + Config.Doremi.Id+"&permissions=2117532736&scope=bot)",true)
            .AddField("Hazuki Bot", "[Invite Hazuki Bot](https://discordapp.com/api/oauth2/authorize?client_id=" + Config.Hazuki.Id + "&permissions=238419008&scope=bot)", true)
            .AddField("Aiko Bot", "[Invite Aiko Bot](https://discordapp.com/api/oauth2/authorize?client_id=" + Config.Aiko.Id + "&permissions=238419008&scope=bot)", true)
            .AddField("Onpu Bot", "[Invite Onpu Bot](https://discordapp.com/api/oauth2/authorize?client_id=" + Config.Onpu.Id + "&permissions=238419008&scope=bot)", true)
            .AddField("Momoko Bot", "[Invite Momoko Bot](https://discordapp.com/api/oauth2/authorize?client_id=" + Config.Momoko.Id + "&permissions=238419008&scope=bot)", true)
            .AddField("Pop Bot", "[Invite Pop Bot](https://discordapp.com/api/oauth2/authorize?client_id=" + Config.Pop.Id + "&permissions=238419008&scope=bot)", true)
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

        [Command("dorememes"), Summary("I will give you some random doremi related memes. " +
            "You can fill <contributor> with one of the available to make it spesific contributor.\nUse `list` as parameter to list all people who have contribute the dorememes.")]
        public async Task givedorememe([Remainder]string contributor = "")
        {
            string finalUrl = ""; JArray getDataObject = null;
            contributor = contributor.ToLower();

            if (contributor == "list")
            {
                var key = Config.Doremi.jobjectdorememes.Properties().ToList();
                string listedContributor = "";
                for (int i = 0; i < key.Count; i++) listedContributor += $"{key[i].Name}\n";

                await base.ReplyAsync(embed: new EmbedBuilder()
                    .WithTitle("Dorememes listed contributor")
                    .WithDescription("Thank you to all of these peoples that have contributed the dorememes:")
                    .AddField("Contributor in List", listedContributor)
                    .WithColor(Config.Doremi.EmbedColor)
                    .Build());
                return;
            }
            else if (contributor == "")
            {
                var key = Config.Doremi.jobjectdorememes.Properties().ToList();
                var randIndex = new Random().Next(0, key.Count);
                contributor = key[randIndex].Name;
                getDataObject = (JArray)Config.Doremi.jobjectdorememes[contributor];
                finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
            }
            else
            {
                if (Config.Doremi.jobjectdorememes.ContainsKey(contributor))
                {
                    getDataObject = (JArray)Config.Doremi.jobjectdorememes[contributor];
                    finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
                }
                else
                {
                    await base.ReplyAsync($"Oops, I can't found the specified contributor. " +
                        $"See `{Config.Doremi.PrefixParent[0]}help dorememe` for commands help.");
                    return;
                }
            }

            await base.ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Doremi.EmbedColor)
            .WithImageUrl(finalUrl)
            .WithFooter("Contributed by: " + contributor)
            .Build());

        }

        [Command("meme", RunMode = RunMode.Async), Alias("memes"), Summary("I will give you some random memes")]
        public async Task givememe()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://meme-api.herokuapp.com/gimme/memes/10");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string jsonResp = reader.ReadToEnd().ToString();
                JObject jobject = JObject.Parse(jsonResp);

                int randomIndex = new Random().Next(0, 11);
                var description = jobject.GetValue("memes")[randomIndex]["title"];
                var imgUrl = jobject.GetValue("memes")[randomIndex]["url"];

                await base.ReplyAsync(embed: new EmbedBuilder()
                .WithDescription(description.ToString())
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl(imgUrl.ToString())
                .Build());

            }
            catch
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
            "Fill <moments> with **random/first/sharp/motto/naisho/dokkan** for spesific moments.")]
        public async Task randomthing(string moments = "")
        {
            string finalUrl=""; string footerUrl = "";
            JArray getDataObject = null; moments = moments.ToLower();
            if (moments == ""){
                int randomType = new Random().Next(0, 5);
                if (randomType != 3)
                {
                    int randomMix = new Random().Next(0, 2); string path;
                    if (randomMix == 0)
                        path = "config/randomMoments/doremi";
                    else
                        path = "config/randomMoments/doremi/mix";

                    string randomPathFile = GlobalFunctions.getRandomFile(path, new string[] { ".png", ".jpg", ".gif", ".webm" });
                    await Context.Channel.SendFileAsync($"{randomPathFile}");
                    return;
                } else {
                    var key = Config.Doremi.jObjRandomMoments.Properties().ToList();
                    var randIndex = new Random().Next(0, key.Count);
                    moments = key[randIndex].Name;
                    getDataObject = (JArray)Config.Doremi.jObjRandomMoments[moments];
                    finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
                }   
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
                await sentMessage.AddReactionAsync(new Discord.Emoji("\u2B50"));
                File.Delete(completePath);
                return;

                //} else {
                //    await ReplyAsync($"Oops, sorry only ``.jpg/.jpeg/.png/.gif`` format is allowed to use ``star`` commands.");
                //    return;
                //}
            }
            catch 
            {
                //Console.WriteLine(e.ToString());
            }

            await Context.Message.DeleteAsync();
            var sentWithoutAttached = await ReplyAsync($"{MentionUtils.MentionUser(Context.User.Id)} has a star messages:\n{MessagesOrWithAttachment}");
            await sentWithoutAttached.AddReactionAsync(new Discord.Emoji("\u2B50"));
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
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/98/Dore-spell.gif")
            .Build());
        }

        [Command("wish"), Summary("I will grant you a <wishes>")]
        public async Task Wish([Remainder] string wishes)
        {
            await ReplyAsync($"Pirika pirilala poporina peperuto! {wishes}",
            embed: new EmbedBuilder()
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
            $"-Added trading card command\n" +
            $"-Update on welcome members message & pictures\n"+
            $"-Update on ojamajo 'change' commands")
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

    [Name("Birthday"), Group("birthday"), Summary("This commands category related with the birthday reminder.")]
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

                File.WriteAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json", guildJsonFile.ToString());

                await ReplyAsync($"{Config.Emoji.birthdayCake} Ok! Your birthday date has been set into: **{DateMonthYear}**. I will remind everyone on your birthday date.",
                        embed: new EmbedBuilder()
                        .WithColor(Config.Doremi.EmbedColor)
                        .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/0/06/DoremiLineOK.png")
                        .Build());
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
                        try
                        {
                            var username = Context.Guild.GetUser(Convert.ToUInt64(key)).Username;
                            builder.AddField(username, val, true);
                            birthdayExisted = true;
                        }
                        catch { }
                        
                    }   
                }
            }

            if (birthdayExisted){
                builder.Title = $"{Config.Emoji.birthdayCake} {DateTime.Now.ToString("MMMM")} Birthday List";
                builder.Description = $"Here are the list of all wonderful people that will have birthday on this month:";
                builder.Color = Config.Doremi.EmbedColor;
                await ReplyAsync(embed: builder.Build());
            } else {
                await ReplyAsync("We don't have someone birthday on this month.");
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
    [RequireUserPermission(GuildPermission.ManageChannels,
        ErrorMessage = "Sorry, you need the `manage channels` permission",
        NotAGuildErrorMessage = "Sorry, you need the `manage channels` permission")]
    [RequireUserPermission(GuildPermission.ManageRoles,
        ErrorMessage = "Sorry, you need the `manage roles` permission",
        NotAGuildErrorMessage = "Sorry, you need the `manage roles` permission") ]
    public class DoremiModerator : ModuleBase<SocketCommandContext>
    {
        [Command("user leave"), Summary("Set the leaving user notifications with **off** or **on**.")]
        public async Task assignUserLeavingNotification(string settings){
            string replacedsettings = settings.Replace("off", "0").Replace("on", "1");
            Config.Guild.setPropertyValue(Context.Guild.Id, "user_leaving_notification", replacedsettings);
            await ReplyAsync($"**Leaving User Messages** has been turned **{settings}**.");
        }

        //[Command("role"), Summary("Set the role that can be set/assignable to anyone. This require the `manage role` permission.")]
        //public async Task configureAssignableRole(string role,SocketGuild guild){
            
        //}

        //leaving_message

        //set doremi role id
        [Command("doremi role id"), Summary("Set the default doremi role Id for default mentionable command prefix.")]
        public async Task setDefaultDoremiRoleId(string roleId)
        {
            Config.Guild.setPropertyValue(Context.Guild.Id, "doremi_role_id", roleId);
            await ReplyAsync($"**Doremi Role Id** has been updated successfully.");
        }

        //set hazuki role id
        [Command("hazuki role id"), Summary("Set the default hazuki role Id for default mentionable command prefix.")]
        public async Task setDefaultHazukiRoleId(string roleId)
        {
            Config.Guild.setPropertyValue(Context.Guild.Id, "hazuki_role_id", roleId);
            await ReplyAsync($"**Hazuki Role Id** has been updated successfully.");
        }

        //set aiko role id
        [Command("aiko role id"), Summary("Set the default aiko role Id for default mentionable command prefix.")]
        public async Task setDefaultAikoRoleId(string roleId)
        {
            Config.Guild.setPropertyValue(Context.Guild.Id, "aiko_role_id", roleId);
            await ReplyAsync($"**Aiko Role Id** has been updated successfully.");
        }

        //set onpu role id
        [Command("onpu role id"), Summary("Set the default onpu role Id for default mentionable command prefix.")]
        public async Task setDefaultOnpuRoleId(string roleId)
        {
            Config.Guild.setPropertyValue(Context.Guild.Id, "onpu_role_id", roleId);
            await ReplyAsync($"**Onpu Role Id** has been updated successfully.");
        }

        //set momoko role id
        [Command("momoko role id"), Summary("Set the default momoko role Id for default mentionable command prefix.")]
        public async Task setDefaultMomokoRoleId(string roleId)
        {
            Config.Guild.setPropertyValue(Context.Guild.Id, "momoko_role_id", roleId);
            await ReplyAsync($"**Momoko Role Id** has been updated successfully.");
        }

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
                var socketClient = Context.Client;

                Config.Guild.setPropertyValue(guildId, "id_birthday_announcement", channel_name.Id.ToString());
                if (Config.Doremi._timerBirthdayAnnouncement.ContainsKey(guildId.ToString()))
                    Config.Doremi._timerBirthdayAnnouncement[guildId.ToString()].Change(Timeout.Infinite, Timeout.Infinite);
                if (Config.Hazuki._timerBirthdayAnnouncement.ContainsKey(guildId.ToString()))
                    Config.Hazuki._timerBirthdayAnnouncement[guildId.ToString()].Change(Timeout.Infinite, Timeout.Infinite);
                if (Config.Aiko._timerBirthdayAnnouncement.ContainsKey(guildId.ToString()))
                    Config.Aiko._timerBirthdayAnnouncement[guildId.ToString()].Change(Timeout.Infinite, Timeout.Infinite);
                if (Config.Onpu._timerBirthdayAnnouncement.ContainsKey(guildId.ToString()))
                    Config.Onpu._timerBirthdayAnnouncement[guildId.ToString()].Change(Timeout.Infinite, Timeout.Infinite);
                if (Config.Momoko._timerBirthdayAnnouncement.ContainsKey(guildId.ToString()))
                    Config.Momoko._timerBirthdayAnnouncement[guildId.ToString()].Change(Timeout.Infinite, Timeout.Infinite);

                Config.Doremi._timerBirthdayAnnouncement[$"{guildId.ToString()}"] = new Timer(async _ =>
                {
                    try
                    {
                        DateTime date; Boolean birthdayExisted = false;
                        //announce hazuki birthday
                        if (DateTime.Now.ToString("dd") == Config.Hazuki.birthdayDate.ToString("dd") &&
                        DateTime.Now.ToString("MM") == Config.Hazuki.birthdayDate.ToString("MM") &&
                        (Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour &&
                        Int32.Parse(DateTime.Now.ToString("HH")) <= Config.Core.maxGlobalTimeHour))
                        {
                            await socketClient
                            .GetGuild(guildId)
                            .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guildId, "id_birthday_announcement")))
                            .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday to you, {MentionUtils.MentionUser(Config.Hazuki.Id)} chan. " +
                            $"She has turned into {Config.Hazuki.birthdayCalculatedYear} on this year. Let's give wonderful birthday wishes for her.");
                            birthdayExisted = true;
                        }

                        //announce aiko birthday
                        if (DateTime.Now.ToString("dd") == Config.Aiko.birthdayDate.ToString("dd") &&
                        DateTime.Now.ToString("MM") == Config.Aiko.birthdayDate.ToString("MM") &&
                        (Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour &&
                        Int32.Parse(DateTime.Now.ToString("HH")) <= Config.Core.maxGlobalTimeHour))
                        {
                            await socketClient
                            .GetGuild(guildId)
                            .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guildId, "id_birthday_announcement")))
                            .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday to our osakan friend: {MentionUtils.MentionUser(Config.Aiko.Id)} chan. " +
                            $"She has turned into {Config.Aiko.birthdayCalculatedYear} on this year. Let's give some takoyaki and wonderful birthday wishes for her.");
                            birthdayExisted = true;
                        }

                        //announce onpu birthday
                        if (DateTime.Now.ToString("dd") == Config.Onpu.birthdayDate.ToString("dd") &&
                        DateTime.Now.ToString("MM") == Config.Onpu.birthdayDate.ToString("MM") &&
                        (Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour &&
                        Int32.Parse(DateTime.Now.ToString("HH")) <= Config.Core.maxGlobalTimeHour))
                        {
                            await socketClient
                            .GetGuild(guildId)
                            .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guildId, "id_birthday_announcement")))
                            .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday to our wonderful idol friend: {MentionUtils.MentionUser(Config.Onpu.Id)} chan. " +
                            $"She has turned into {Config.Onpu.birthdayCalculatedYear} on this year. Let's give some wonderful birthday wishes for her.");
                            birthdayExisted = true;
                        }

                        //announce momoko birthday
                        if (DateTime.Now.ToString("dd") == Config.Momoko.birthdayDate.ToString("dd") &&
                        DateTime.Now.ToString("MM") == Config.Momoko.birthdayDate.ToString("MM") &&
                        (Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour &&
                        Int32.Parse(DateTime.Now.ToString("HH")) <= Config.Core.maxGlobalTimeHour))
                        {
                            await socketClient
                            .GetGuild(guildId)
                            .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guildId, "id_birthday_announcement")))
                            .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday to our wonderful friend: {MentionUtils.MentionUser(Config.Momoko.Id)} chan. " +
                            $"She has turned into {Config.Momoko.birthdayCalculatedYear} on this year. Let's give some wonderful birthday wishes for her.");
                            birthdayExisted = true;
                        }

                        EmbedBuilder builder = new EmbedBuilder();
                        builder.Color = Config.Doremi.EmbedColor;
                        builder.ImageUrl = "https://i.4pcdn.org/s4s/1508005628768.jpg";

                        var guildJsonFile = (JObject)JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json")).GetValue("user_birthday");
                        var jobjbirthday = guildJsonFile.Properties().ToList();
                        for (int i = 0; i < jobjbirthday.Count; i++)
                        {
                            string birthdayMessage = "";
                            var key = jobjbirthday[i].Name; var val = jobjbirthday[i].Value.ToString();
                            //var birthdayMonth = "";
                            try {
                                var user = channel_name.GetUser(Convert.ToUInt64(key));

                                if ((DateTime.TryParseExact(val, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date) ||
                                DateTime.TryParseExact(val, "dd/MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) &&
                                (Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour &&
                                Int32.Parse(DateTime.Now.ToString("HH")) < Config.Core.maxGlobalTimeHour))
                                {
                                    if (date.ToString("dd/MM") == DateTime.Now.ToString("dd/MM"))
                                    {
                                        string[] arrRandomedMessage = {
                                        $"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Everyone, let's give a wonderful birthday wishes for: {user.Mention} ",
                                        $"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday to our wonderful friend: {user.Mention} . " +
                                        $"Please give the wonderful birthday wishes for {user.Mention}.",
                                        $"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Everyone, we have important birthday announcement! Please give some wonderful birthday wishes for {user.Mention}."
                                    };
                                        birthdayMessage = arrRandomedMessage[new Random().Next(0, arrRandomedMessage.Length)];
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
                            .SendMessageAsync(embed: builder.Build());
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

                //Hazuki: set Doremi birthday announcement
                Config.Hazuki._timerBirthdayAnnouncement[guildId.ToString()] = new Timer(async _ =>
                {
                    //announce doremi birthday
                    if (DateTime.Now.ToString("dd") == Config.Doremi.birthdayDate.ToString("dd") &&
                    DateTime.Now.ToString("MM") == Config.Doremi.birthdayDate.ToString("MM") &&
                    (Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour &&
                    Int32.Parse(DateTime.Now.ToString("HH")) <= Config.Core.maxGlobalTimeHour))
                    {
                        await Bot.Hazuki.client
                        .GetGuild(guildId)
                        .GetTextChannel(channel_name.Id)
                        .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday, {MentionUtils.MentionUser(Config.Doremi.Id)} chan. " +
                        $"She has turned into {Config.Doremi.birthdayCalculatedYear} on this year. Let's give some big steak and wonderful birthday wishes for her.");
                    }
                },
               null,
               TimeSpan.FromSeconds(10), //time to wait before executing the timer for the first time
               TimeSpan.FromHours(24) //time to wait before executing the timer again
               );

                //Aiko: set Doremi birthday announcement
                Config.Aiko._timerBirthdayAnnouncement[guildId.ToString()] = new Timer(async _ =>
                {
                    //announce doremi birthday
                    if (DateTime.Now.ToString("dd") == Config.Doremi.birthdayDate.ToString("dd") &&
                    DateTime.Now.ToString("MM") == Config.Doremi.birthdayDate.ToString("MM") &&
                    (Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour &&
                    Int32.Parse(DateTime.Now.ToString("HH")) <= Config.Core.maxGlobalTimeHour))
                    {
                        await Bot.Aiko.client
                        .GetGuild(guildId)
                        .GetTextChannel(channel_name.Id)
                        .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday, {MentionUtils.MentionUser(Config.Doremi.Id)} chan. " +
                        $"She has turned into {Config.Doremi.birthdayCalculatedYear} on this year. Let's give some big steak and wonderful birthday wishes for her.",
                        embed: new EmbedBuilder()
                        .WithColor(Config.Aiko.EmbedColor)
                        .WithImageUrl(Config.Doremi.DoremiBirthdayCakeImgSrc)
                        .Build());
                    }
                },
               null,
               TimeSpan.FromSeconds(10), //time to wait before executing the timer for the first time
               TimeSpan.FromHours(24) //time to wait before executing the timer again
               );

                //Onpu: set Doremi birthday announcement
                Config.Onpu._timerBirthdayAnnouncement[guildId.ToString()] = new Timer(async _ =>
                {
                    //announce doremi birthday
                    if (DateTime.Now.ToString("dd") == Config.Doremi.birthdayDate.ToString("dd") &&
                    DateTime.Now.ToString("MM") == Config.Doremi.birthdayDate.ToString("MM") &&
                    (Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour &&
                    Int32.Parse(DateTime.Now.ToString("HH")) <= Config.Core.maxGlobalTimeHour))
                    {
                        await Bot.Onpu.client
                        .GetGuild(guildId)
                        .GetTextChannel(channel_name.Id)
                        .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday, {MentionUtils.MentionUser(Config.Doremi.Id)} chan. " +
                        $"She has turned into {Config.Doremi.birthdayCalculatedYear} on this year. Let's give some big steak and wonderful birthday wishes for her.");
                    }
                },
               null,
               TimeSpan.FromSeconds(10), //time to wait before executing the timer for the first time
               TimeSpan.FromHours(24) //time to wait before executing the timer again
               );

                //Momoko: set Doremi birthday announcement
                Config.Momoko._timerBirthdayAnnouncement[guildId.ToString()] = new Timer(async _ =>
                {
                    //announce doremi birthday
                    if (DateTime.Now.ToString("dd") == Config.Doremi.birthdayDate.ToString("dd") &&
                    DateTime.Now.ToString("MM") == Config.Doremi.birthdayDate.ToString("MM") &&
                    (Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour &&
                    Int32.Parse(DateTime.Now.ToString("HH")) <= Config.Core.maxGlobalTimeHour))
                    {
                        await Bot.Momoko.client
                        .GetGuild(guildId)
                        .GetTextChannel(channel_name.Id)
                        .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday, {MentionUtils.MentionUser(Config.Doremi.Id)} chan. " +
                        $"She has turned into {Config.Doremi.birthdayCalculatedYear} on this year. Let's give some big steak and wonderful birthday wishes for her.");
                    }
                },
               null,
               TimeSpan.FromSeconds(10), //time to wait before executing the timer for the first time
               TimeSpan.FromHours(24) //time to wait before executing the timer again
               );

                await ReplyAsync($"{Config.Emoji.birthdayCake} **Birthday Announcement Channels** has been assigned at: {MentionUtils.MentionChannel(channel_name.Id)}");

            }

            //trading card configuration section
            //assign trading card spawning channel
            [Command("trading card spawn"), Summary("Set Doremi Bot to make the trading card to be spawned at <channel_name>.")]
            public async Task assignTradingCardSpawnChannel(SocketGuildChannel channel_name)
            {
                var guildId = channel_name.Guild.Id;
                var socketClient = Context.Client;

                Config.Guild.setPropertyValue(guildId, "trading_card_spawn", channel_name.Id.ToString());
                await ReplyAsync($"**Trading Card Spawning Channels** has been assigned at: {MentionUtils.MentionChannel(channel_name.Id)}");

                if (Config.Doremi._timerTradingCardSpawn.ContainsKey(guildId.ToString()))
                    Config.Doremi._timerTradingCardSpawn[guildId.ToString()].Change(Timeout.Infinite, Timeout.Infinite);

                //start doremi card spawning timer
                Config.Doremi._timerTradingCardSpawn[guildId.ToString()] = new Timer(async _ =>
                {
                    int randomCategory = new Random().Next(11);
                    string chosenCategory = "";
                    if (randomCategory <= 2)//0-2
                    {//normal
                        chosenCategory = "metal";
                    }
                    else if (randomCategory <= 5)//0-5
                    {//platinum
                        chosenCategory = "platinum";
                    }
                    else if (randomCategory <= 10)//0-10
                    {//metal
                        chosenCategory = "normal";
                    }

                    int randomParent = new Random().Next(0, 5);
                    //int randomParent = 0; //don't forget to erase this, for testing purpose
                    string parent = ""; DiscordSocketClient client = socketClient;
                    Discord.Color color= Config.Doremi.EmbedColor; string author=""; string embedAvatarUrl = "";

                    if (randomParent == 0)
                    {
                        parent = "doremi"; author = $"Doremi {GlobalFunctions.UppercaseFirst(chosenCategory)} Card";
                        embedAvatarUrl = Config.Doremi.EmbedAvatarUrl;
                    }
                    else if (randomParent == 1)
                    {
                        client = Bot.Hazuki.client;
                        parent = "hazuki"; author = $"Hazuki {GlobalFunctions.UppercaseFirst(chosenCategory)} Card";
                        color = Config.Hazuki.EmbedColor; embedAvatarUrl = Config.Hazuki.EmbedAvatarUrl;
                    }
                    else if (randomParent == 2)
                    {
                        client = Bot.Aiko.client;
                        parent = "aiko"; author = $"Aiko {GlobalFunctions.UppercaseFirst(chosenCategory)} Card";
                        color = Config.Aiko.EmbedColor; embedAvatarUrl = Config.Aiko.EmbedAvatarUrl;
                    }
                    else if (randomParent == 3)
                    {
                        client = Bot.Onpu.client;
                        parent = "onpu"; author = $"Onpu {GlobalFunctions.UppercaseFirst(chosenCategory)} Card";
                        color = Config.Onpu.EmbedColor; embedAvatarUrl = Config.Onpu.EmbedAvatarUrl;
                    }
                    else if (randomParent >= 4)
                    {
                        client = Bot.Momoko.client;
                        parent = "momoko"; author = $"Momoko {GlobalFunctions.UppercaseFirst(chosenCategory)} Card";
                        color = Config.Momoko.EmbedColor; embedAvatarUrl = Config.Momoko.EmbedAvatarUrl;
                    }

                    //start read json
                    var jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));
                    var key = JObject.Parse(jObjTradingCardList[parent][chosenCategory].ToString()).Properties().ToList();
                    var randIndex = new Random().Next(0, key.Count);

                    //chosen data:
                    string chosenId = key[randIndex].Name;
                    string chosenName = jObjTradingCardList[parent][chosenCategory][key[randIndex].Name]["name"].ToString();
                    string chosenUrl = jObjTradingCardList[parent][chosenCategory][key[randIndex].Name]["url"].ToString();
                    Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyId, chosenId);
                    Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyCategory, chosenCategory);
                    Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyToken, GlobalFunctions.RandomString(8));

                    var embed = new EmbedBuilder()
                        .WithAuthor(author, embedAvatarUrl)
                        .WithColor(color)
                        .WithTitle($"{chosenName}")
                        .WithFooter($"ID: {chosenId}")
                        .WithImageUrl(chosenUrl);

                    await client
                        .GetGuild(guildId)
                        .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guildId, "trading_card_spawn")))
                        .SendMessageAsync($":exclamation:A **{chosenCategory}** {parent} card has been spawned! Capture it with **<bot>!card capture/catch**", 
                        embed:embed.Build());
                },
                null,
                TimeSpan.FromMinutes(Convert.ToInt32(Guild.getPropertyValue(guildId, "trading_card_spawn_interval")) + new Random().Next(5, 11)), //time to wait before executing the timer for the first time
                TimeSpan.FromMinutes(Convert.ToInt32(Guild.getPropertyValue(guildId, "trading_card_spawn_interval")) + new Random().Next(5, 11)) //time to wait before executing the timer again
                );
            }

            //set spawning interval
            [Command("trading card spawn interval"), Summary("Set the trading card spawn interval (in minutes).")]
            public async Task setTradingCardSpawnInterval(int interval_minutes)
            {
                if (interval_minutes <= 4 || interval_minutes >= 1441) await ReplyAsync($"Please enter interval between 5-1440 (in minutes)");
                else {
                    var guildId = Context.Guild.Id;
                    Config.Guild.setPropertyValue(guildId, "trading_card_spawn_interval", interval_minutes.ToString());
                    await ReplyAsync($"**Trading Card Spawning interval** has been set into **{interval_minutes}** minute(s)");

                    if (Config.Doremi._timerTradingCardSpawn.ContainsKey(guildId.ToString()))
                        Config.Doremi._timerTradingCardSpawn[guildId.ToString()].Change(
                            TimeSpan.FromMinutes(Convert.ToInt32(Guild.getPropertyValue(guildId, "trading_card_spawn_interval")) + new Random().Next(5, 11)), 
                            TimeSpan.FromMinutes(Convert.ToInt32(Guild.getPropertyValue(guildId, "trading_card_spawn_interval")) + new Random().Next(5, 11)));
                }
                    
            }   

            //TRADING CARD CONFIGURATION ENDS

            [Command("random event"), Summary("Schedule Doremi Bot to make random event message on <channel_name> for every 24 hours.")]
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
                "Current available settings: `birthday`/`random event`/`trading card spawn`")]
            public async Task removeChannelSettings([Remainder]string settings)
            {
                string property = ""; Boolean propertyValueExisted = false;
                var guildId = Context.Guild.Id;
                var guildJsonFile = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json"));

                if (settings.ToLower() == "birthday"){
                    property = $"{Config.Emoji.birthdayCake} Birthday Announcement";
                    if (Config.Guild.hasPropertyValues(guildId.ToString(), "id_birthday_announcement"))
                    {
                        propertyValueExisted = true;
                        Config.Guild.setPropertyValue(Context.Guild.Id, "id_birthday_announcement", "");
                        if (Config.Doremi._timerBirthdayAnnouncement.ContainsKey(Context.Guild.Id.ToString()))
                            Config.Doremi._timerBirthdayAnnouncement[Context.Guild.Id.ToString()].Change(Timeout.Infinite, Timeout.Infinite);
                        if (Config.Hazuki._timerBirthdayAnnouncement.ContainsKey(Context.Guild.Id.ToString()))
                            Config.Hazuki._timerBirthdayAnnouncement[Context.Guild.Id.ToString()].Change(Timeout.Infinite, Timeout.Infinite);
                        if (Config.Aiko._timerBirthdayAnnouncement.ContainsKey(Context.Guild.Id.ToString()))
                            Config.Aiko._timerBirthdayAnnouncement[Context.Guild.Id.ToString()].Change(Timeout.Infinite, Timeout.Infinite);
                        if (Config.Onpu._timerBirthdayAnnouncement.ContainsKey(Context.Guild.Id.ToString()))
                            Config.Onpu._timerBirthdayAnnouncement[Context.Guild.Id.ToString()].Change(Timeout.Infinite, Timeout.Infinite);
                        if (Config.Momoko._timerBirthdayAnnouncement.ContainsKey(Context.Guild.Id.ToString()))
                            Config.Momoko._timerBirthdayAnnouncement[Context.Guild.Id.ToString()].Change(Timeout.Infinite, Timeout.Infinite);
                    }
                } else if (settings.ToLower() == "random event")
                {
                    property = "Random Event";
                    if (Config.Guild.hasPropertyValues(guildId.ToString(),"id_random_event"))
                    {
                        propertyValueExisted = true;
                        Config.Guild.setPropertyValue(Context.Guild.Id, "id_random_event", "");
                        if (Config.Doremi._timerRandomEvent.ContainsKey(Context.Guild.Id.ToString()))
                            Config.Doremi._timerRandomEvent[Context.Guild.Id.ToString()].Change(Timeout.Infinite, Timeout.Infinite);
                    }
                }
                else if (settings.ToLower() == "trading card spawn")
                {
                    property = "Trading Card Spawn";
                    if (Config.Guild.hasPropertyValues(guildId.ToString(), "trading_card_spawn"))
                    {
                        propertyValueExisted = true;
                        Config.Guild.setPropertyValue(Context.Guild.Id, "trading_card_spawn", "");
                        if (Config.Doremi._timerTradingCardSpawn.ContainsKey(Context.Guild.Id.ToString()))
                            Config.Doremi._timerTradingCardSpawn[Context.Guild.Id.ToString()].Change(Timeout.Infinite, Timeout.Infinite);

                        //reset spawn settings
                        Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyId, "");
                        Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyCategory, "");
                        Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyToken, "");

                    }
                }
                else
                {
                    await ReplyAsync($"Sorry, I can't found that channel settings"); return;
                }

                if (propertyValueExisted)
                    await ReplyAsync($"**{property} Channels** settings has been removed.");
                else
                    await ReplyAsync($"**{property} Channels** has no settings yet.");
            }

        }

    }

    [Name("Card"), Group("card"), Summary("This category contains all Doremi Trading card command.")]
    public class DoremiTradingCardInteractive : InteractiveBase
    {
        [Command("register", RunMode = RunMode.Async), Summary("Register your configuration for trading cards group command.")]
        public async Task trading_card_register()
        {
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";

            if (!File.Exists(playerDataDirectory))
            {
                File.Copy($@"{Config.Core.headConfigFolder}trading_card_template_data.json", $@"{playerDataDirectory}");
                await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription($":white_check_mark: Your trading card data has been successfully registered.")
                .WithImageUrl(TradingCardCore.Doremi.emojiOk).Build());
            }
            else
            {
                await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription(":x: Sorry, your trading card data has been registered already.")
                .WithImageUrl(TradingCardCore.Doremi.emojiError).Build());
            }
        }

        [Command("capture", RunMode = RunMode.Async), Alias("catch"), Summary("Capture spawned card with Doremi.")]
        public async Task trading_card_doremi_capture()
        {
            //reference: https://www.newtonsoft.com/json/help/html/ModifyJson.htm
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
            string replyText = ""; string parent = "doremi";

            if (!File.Exists(playerDataDirectory))
            {
                replyText = "I'm sorry, please register yourself first with **do!card register** command.";
                await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription(replyText)
                .WithImageUrl(TradingCardCore.Doremi.emojiError).Build());
                return;
            }
            else
            {
                JObject arrInventory = JObject.Parse(File.ReadAllText(playerDataDirectory));
                string spawnedCardId = Config.Guild.getPropertyValue(guildId, TradingCardCore.propertyId);
                string spawnedCardCategory = Config.Guild.getPropertyValue(guildId, TradingCardCore.propertyCategory);
                if (spawnedCardId != "" && spawnedCardCategory != "")
                {
                    if (spawnedCardId.Contains("do"))//check if the card is doremi/not
                    {
                        int catchState = 0;

                        //check last capture time
                        try
                        {
                            if ((string)arrInventory["catch_token"] == "" ||
                                (string)arrInventory["catch_token"] != Config.Guild.getPropertyValue(guildId, TradingCardCore.propertyToken))
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
                                            "Congratulations,","Nice Catch!","Nice one!","Yatta!"
                                        };

                                        await ReplyAsync($":white_check_mark: {arrRandomFirstSentence[new Random().Next(0,arrRandomFirstSentence.Length)]} " +
                                            $"**{Context.User.Username}** have successfully capture **{spawnedCardCategory}** card: **{name}**",
                                            embed: TradingCardCore.printCardCaptureTemplate(Config.Doremi.EmbedColor,name,imgUrl,
                                            spawnedCardId,spawnedCardCategory,rank,star,point,Context.User.Username, Config.Doremi.EmbedAvatarUrl)
                                            .Build());

                                        //check if player have captured all card/not
                                        if (((JArray)arrInventory["doremi"]["normal"]).Count>=TradingCardCore.Doremi.maxNormal&&
                                            ((JArray)arrInventory["doremi"]["platinum"]).Count >= TradingCardCore.Doremi.maxPlatinum&&
                                            ((JArray)arrInventory["doremi"]["metal"]).Count >= TradingCardCore.Doremi.maxMetal)
                                        {
                                            await ReplyAsync(embed: TradingCardCore
                                                .userCompleteTheirList(Config.Doremi.EmbedColor,"doremi",
                                                $":clap: Congratulations, **{Context.User.Username}** have successfully capture all **Doremi Card Pack**!",
                                                TradingCardCore.Doremi.emojiCompleteAllCard,guildId.ToString(), 
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
            .WithColor(Config.Doremi.EmbedColor)
            .WithDescription(replyText)
            .WithImageUrl(TradingCardCore.Doremi.emojiError).Build());

        }

        //list all cards that have been collected
        [Command("inventory", RunMode = RunMode.Async), Summary("List all **Doremi** trading cards that you have collected.")]
        public async Task trading_card_open_inventory()
        {
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
            var jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));

            string replyText; string parent = "doremi";

            if (!File.Exists(playerDataDirectory)) //not registered yet
            {
                await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription("I'm sorry, please register yourself first with **do!card register** command.")
                .WithImageUrl(TradingCardCore.Doremi.emojiError).Build());
            }
            else
            {
                var playerData = JObject.Parse(File.ReadAllText(playerDataDirectory));

                EmbedBuilder builder = new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor);

                try
                {
                    //normal category
                    string category = "normal"; var arrList = (JArray)playerData[parent][category];

                    if (arrList.Count >= 1)
                    {
                        await PagedReplyAsync(
                            TradingCardCore.printInventoryTemplate("doremi", "doremi", category, jObjTradingCardList, arrList, TradingCardCore.Doremi.maxNormal)
                        );
                    }
                    else
                    {
                        await ReplyAsync(embed: TradingCardCore.printEmptyInventoryTemplate(
                            Config.Doremi.EmbedColor, "doremi", category, TradingCardCore.Doremi.maxNormal)
                            .Build());
                    }

                    //platinum category
                    category = "platinum"; arrList = (JArray)playerData[parent][category];
                    if (arrList.Count >= 1)
                    {
                        await PagedReplyAsync(
                            TradingCardCore.printInventoryTemplate("doremi", "doremi", category, jObjTradingCardList, arrList, TradingCardCore.Doremi.maxPlatinum)
                        );
                    }
                    else
                    {
                        await ReplyAsync(embed: TradingCardCore.printEmptyInventoryTemplate(
                            Config.Doremi.EmbedColor, "doremi", category, TradingCardCore.Doremi.maxPlatinum)
                            .Build());
                    }

                    //metal category
                    category = "metal"; arrList = (JArray)playerData[parent][category];
                    if (arrList.Count >= 1)
                    {
                        await PagedReplyAsync(
                            TradingCardCore.printInventoryTemplate("doremi", "doremi", category, jObjTradingCardList, arrList, TradingCardCore.Doremi.maxMetal)
                        );
                    }
                    else
                    {
                        await ReplyAsync(embed: TradingCardCore.printEmptyInventoryTemplate(
                            Config.Doremi.EmbedColor, "doremi", category, TradingCardCore.Doremi.maxMetal)
                            .Build());
                    }

                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }

        }

        [Command("detail", RunMode = RunMode.Async), Alias("info","look"), Summary("See the detail of Doremi card information from the <card_id>.")]
        public async Task trading_card_look(string card_id)
        {
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;
            string parent = "doremi";

            string category = "";

            if (card_id.Contains("doP"))//platinum
                category = "platinum";
            else if (card_id.Contains("doM"))//metal
                category = "metal";
            else if (card_id.Contains("do"))
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

                    await ReplyAsync(embed: TradingCardCore.printCardDetailTemplate(Config.Doremi.EmbedColor, name,
                        imgUrl, card_id, category, rank, star, point)
                        .Build());
                }
                else
                {
                    await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithDescription($"Sorry, you don't have: **{card_id} - {name}** card yet. Try capture it to look at this card.")
                    .WithImageUrl(TradingCardCore.Doremi.emojiError).Build());
                }
            }
            catch (Exception e)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription("Sorry, I can't find that card ID.")
                .WithImageUrl(TradingCardCore.Doremi.emojiError).Build());
            }

        }

        [Command("status", RunMode = RunMode.Async), Summary("Show your Trading Card Status report.")]
        public async Task trading_card_status()
        {
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
            
            if (!File.Exists(playerDataDirectory)){ //not registered yet
                await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription("I'm sorry, please register yourself first with **do!card register** command.")
                .WithImageUrl(TradingCardCore.Doremi.emojiError).Build());
            } else {
                await ReplyAsync(embed: TradingCardCore.
                    printStatusTemplate(Config.Doremi.EmbedColor,Context.User.Username,guildId.ToString(),clientId.ToString())
                    .Build());
            }
        }

        //show top 5 that capture each card pack
        [Command("leaderboard", RunMode = RunMode.Async), Summary("Show the trading card leaderboard status.")]
        public async Task trading_card_leaderboard()
        {
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;

            await ReplyAsync(embed: TradingCardCore.
                    printLeaderboardTemplate(Config.Doremi.EmbedColor, Context.User.Username, guildId.ToString(), clientId.ToString())
                    .Build());
        }

        [Command("updates"), Alias("update"), Summary("Show Trading Card Updates")]
        public async Task showCardUpdates()
        {
            await ReplyAsync(embed: TradingCardCore.
                    printUpdatesNote()
                    .Build());
        }

        //trade
        //[Command("trade", RunMode = RunMode.Async), Summary("Trade one of your doremi trading card with other user.")]
        //public async Task trading_card_trade()
        //{
        //    Boolean isTrading = false;
        //    while (isTrading)
        //    {

        //    }
        //        //you only allowed to trade for 2x each day
        //        /*json format:
        //         * "trading_queue": {
        //            "01929183481": ["do","on"]
        //        }
        //         */
        //}
    }

    [Name("memesdraw"), Group("memesdraw"), Summary("Memes Draw Category.")]
    public class DorememesModule : ModuleBase<SocketCommandContext>{

        [Command("template list", RunMode = RunMode.Async), Summary("Show all available dorememes generator template.")]
        public async Task showAllDorememesTemplate(){
            EmbedBuilder builder = new EmbedBuilder();
            builder.Color = Config.Doremi.EmbedColor;
            builder.Title = "Dorememes Generator Template List";
            builder.Description = "Here are the available dorememes generator template that can be used on `dorememes draw` commands.";

            var guildId = Context.Guild.Id;
            var guildJsonFile = (JObject)JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}ojamajo_meme_template/list.json")).GetValue("list");
            var jobjmemelist = guildJsonFile.Properties().ToList();
            string finalList = "";

            for (int i = 0; i < jobjmemelist.Count; i++){
                finalList += $"{jobjmemelist[i].Name} : {Path.GetFileNameWithoutExtension(jobjmemelist[i].Value.ToString())}\n";
            }
            builder.AddField("[Numbers] : Title",finalList);

            await ReplyAsync(embed: builder.Build());
        }

        [Command("template show",RunMode = RunMode.Async), Summary("Show the image preview of dorememes template from `dorememes template list` commands.\n" +
            "You can fill the <choices> parameter with numbers/title.")]
        public async Task showDorememesImageTemplate([Remainder]string choices){
            bool isNumeric = choices.All(char.IsDigit);

            var guildId = Context.Guild.Id;
            var guildJsonFile = (JObject)JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}ojamajo_meme_template/list.json")).GetValue("list");
            var jobjmemelist = guildJsonFile.Properties().ToList();
            bool isFounded = false;

            string selectedFile = $"{Config.Core.headConfigFolder}ojamajo_meme_template/";

            for (int i = 0; i < jobjmemelist.Count; i++){
                var checkedKey = jobjmemelist[i].Name.ToString();
                var checkedValue = jobjmemelist[i].Value.ToString();

                if ((isNumeric&&choices == checkedKey) ||
                    (!isNumeric&& choices == Path.GetFileNameWithoutExtension(checkedValue))){
                    selectedFile += checkedValue;
                    isFounded = true;
                    break;
                }
            }

            if (isFounded)
                await Context.Channel.SendFileAsync(selectedFile);
            else
                await ReplyAsync("Sorry, I can't find that choices. " +
                    $"See the available dorememes generator template with `{Config.Doremi.PrefixParent[0]}dorememes template list` command.");
        }

        [Command("draw", RunMode = RunMode.Async), Summary("Draw dorememes from available dorememes template list.\n" +
            "Commands parameters will be `<template>;<text>;<optional positions:top/bottom>`.")]
        public async Task drawFromMemeTemplate([Remainder] string parameter)
        {
            string template; string text; string[] splittedParameter;string positions="top";

            if (parameter.Contains(";")){
                splittedParameter = parameter.Split(";");
                template = splittedParameter[0];
                text = splittedParameter[1];
                if (text.Length >= 40){
                    await ReplyAsync($"Sorry, that text is too long. Please use shorter text.");
                    return;
                }
                if (2<splittedParameter.Length){
                    if (splittedParameter[2].ToLower() != "top" || splittedParameter[2].ToLower() != "bottom")
                        positions = splittedParameter[2].ToLower();
                    else {
                        await ReplyAsync($"Sorry, **positions** parameter need to be `top` or `bottom`.");
                        return;
                    }
                }
            } else {
                await ReplyAsync($"Sorry, there seems to be wrong with the parameter input.\n" +
                    $"Example: `{Config.Doremi.PrefixParent[0]}dorememes draw <template>;<text>;<optional positions:top/bottom>`");
                return;
            }

            bool isNumeric = template.All(char.IsDigit);

            var guildId = Context.Guild.Id;
            var guildJsonFile = (JObject)JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}ojamajo_meme_template/list.json")).GetValue("list");
            var jobjmemelist = guildJsonFile.Properties().ToList();
            bool isFounded = false;

            string selectedFile = $"{Config.Core.headConfigFolder}ojamajo_meme_template/";
            string selectedGetFileName = "";//return the file name only with extensions

            IMessage nowProcessing = null;

            for (int i = 0; i < jobjmemelist.Count; i++){
                var checkedKey = jobjmemelist[i].Name.ToString();
                var checkedValue = jobjmemelist[i].Value.ToString();

                if ((isNumeric && template == checkedKey) ||
                    (!isNumeric && template == Path.GetFileNameWithoutExtension(checkedValue))){
                    selectedGetFileName = checkedValue;
                    selectedFile += checkedValue;
                    isFounded = true;
                    nowProcessing = await ReplyAsync($"\u23F3 Processing the dorememes, please wait for a moment...");
                    break;
                }
            }

            if (!isFounded){
                await ReplyAsync("Sorry, I can't find that template choices. " +
                    $"See the available dorememes generator template with `{Config.Doremi.PrefixParent[0]}memesdraw template list` command.");
                return;
            }

            //end file checking

            //copy the image
            var sourceDir = selectedFile;
            var destDir = $"attachments/{guildId}/{Path.GetFileNameWithoutExtension(selectedGetFileName)}_{DateTime.Now.ToString("yyyyMMdd_HHmm")}" + 
                $"{new Random().Next(0, 10000)}{Path.GetExtension(selectedGetFileName)}";
            File.Copy(sourceDir, destDir);
            
            //process the image
            
            //every 23 words reduce the font size
            Bitmap newBitmap;
            using (var bitmap = (Bitmap)System.Drawing.Image.FromFile(destDir))//load the image file
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    //using (Font goodFont = FindFont(graphics, text, bitmap.Size, new Font("Impact", 72)))
                    using (Font goodFont = ImageEditor.GetAdjustedFont(graphics, text, new Font("Impact", 72), bitmap.Width, 72, 40, true))
                    {
                        StringFormat sf = new StringFormat();
                        sf.LineAlignment = StringAlignment.Center;
                        sf.Alignment = StringAlignment.Center;
                        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                        if (positions == "top"){
                            PointF topLocation = new PointF(bitmap.Width / 2, 100f);
                            graphics.DrawString(text, goodFont, Brushes.White, topLocation, sf);
                        } else if (positions == "bottom"){
                            PointF bottomLocation = new PointF(bitmap.Width / 2, bitmap.Height - 150f);
                            graphics.DrawString(text, goodFont, Brushes.White, bottomLocation, sf);
                        }
                        
                    }
                }
                newBitmap = new Bitmap(bitmap);
            }

            newBitmap.Save(destDir);//save the image file
            newBitmap.Dispose();

            await Context.Channel.SendFileAsync(destDir);
            await Context.Channel.DeleteMessageAsync(Convert.ToUInt64(nowProcessing.Id));

            File.Delete(destDir);

        }

        [Command("jojo", RunMode = RunMode.Async), Summary("Add Jojo image filter to the image.")]
        public async Task drawJojoficationToBeContinue(string attachment=""){

            try
            {
                var attachments = Context.Message.Attachments;
                WebClient myWebClient = new WebClient();

                string file = attachments.ElementAt(0).Filename;
                string url = attachments.ElementAt(0).Url;
                string extension = Path.GetExtension(attachments.ElementAt(0).Filename).ToLower();
                string randomedFileName = "jojofication_"+DateTime.Now.ToString("yyyyMMdd_HHmm") + new Random().Next(0, 10000) + extension;
                string completePath = $"attachments/{Context.Guild.Id}/{randomedFileName}";
                string toBeContinueImagePath = $"{Config.Core.headConfigFolder}ojamajo_meme_template/to be continue.png";
                string resizedToBeContinueImagePath = $"attachments/{Context.Guild.Id}/to be continue.png";

                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif")
                {
                    IMessage nowProcessing = await ReplyAsync($"\u23F3 Processing the dorememes jojofication, please wait for a moment...");

                    //Download the resource and load the bytes into a buffer.
                    byte[] buffer = myWebClient.DownloadData(url);
                    Config.Core.ByteArrayToFile($"attachments/{Context.Guild.Id}/{randomedFileName}", buffer);

                    //await Context.Message.DeleteAsync();

                    //File.Delete(completePath);

                    //convert to sepia
                    Bitmap newBitmap;
                    using (var bitmap = (Bitmap)System.Drawing.Image.FromFile(completePath))  
                       newBitmap = new Bitmap(ImageEditor.convertSepia(bitmap));

                    //copy & resize to be continue image
                    Bitmap toBeContinueImage = (Bitmap)System.Drawing.Image.FromFile(toBeContinueImagePath);

                    int resizedHeight = Convert.ToInt32(newBitmap.Height / 3.5);

                    toBeContinueImage = ImageEditor.ResizeImage(toBeContinueImage, Convert.ToInt32(newBitmap.Width / 2.5), resizedHeight);
                    toBeContinueImage.Save(resizedToBeContinueImagePath);
                    
                    //merge the image
                    newBitmap = ImageEditor.MergeBitmaps(toBeContinueImage, newBitmap);

                    toBeContinueImage.Dispose();

                    newBitmap.Save(completePath);//save the image file
                    newBitmap.Dispose();

                    var sentAttachment = await Context.Channel.SendFileAsync(completePath);
                    await Context.Channel.DeleteMessageAsync(Convert.ToUInt64(nowProcessing.Id));

                    File.Delete(resizedToBeContinueImagePath);
                    File.Delete(completePath);

                } else {
                    await ReplyAsync($"Oops, sorry I can only process `.jpg/.jpeg/.png/.gif` image format.");
                    return;
                }

            }
            catch(Exception e) { Console.WriteLine(e.ToString()); }
            
        }

        // This function checks the room size and your text and appropriate font
        //  for your text to fit in room
        // PreferedFont is the Font that you wish to apply
        // Room is your space in which your text should be in.
        // LongString is the string which it's bounds is more than room bounds.

    }

    [Summary("hidden")]
    public class DoremiMagicalStageModule : ModuleBase
    {
        //magical stage section
        [Command("Peruton Peton, Sawayaka ni!")]//from momoko
        public async Task magicalStagefinal()
        {
            if (Context.User.Id == Config.Momoko.Id){
                await ReplyAsync($"{MentionUtils.MentionUser(Config.Hazuki.Id)} Magical Stage! {Config.Doremi.MagicalStageWishes}\n");
                Config.Doremi.MagicalStageWishes = "";
            }
        }

    }

    [Name("minigame"), Group("minigame"), Summary("This category contains all Doremi minigame interactive commands.")]
    public class DoremiMinigameInteractive : InteractiveBase
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
        [Command("score"), Summary("Show your current minigame score points.")]
        public async Task Show_Quiz_Score(){//show the player score
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
        public async Task Show_Minigame_Leaderboard(){//show top 10 player score
            var guildId = Context.Guild.Id;
            var userId = Context.User.Id;

            var quizJsonFile = (JObject)JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.minigameDataFileName}")).GetValue("score");

            string finalText = "";
            EmbedBuilder builder = new EmbedBuilder();
            builder.Title = "\uD83C\uDFC6 Minigame Leaderboard";
            
            builder.Color = Config.Doremi.EmbedColor;

            if (quizJsonFile.Count >= 1){
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

        [Command("rockpaperscissor", RunMode = RunMode.Async), Alias("rps"), Summary("Play the Rock Paper Scissor minigame with Doremi. 20 score points reward.")]
        public async Task RockPaperScissor(string guess = "")
        {
            if (guess == ""){
                await ReplyAsync($"Please enter the valid parameter: **rock** or **paper** or **scissor**");
                return;
            } else if (guess.ToLower() != "rock" && guess.ToLower() != "paper" && guess.ToLower() != "scissor") {
                await ReplyAsync($"Sorry **{Context.User.Username}**. " +
                    $"Please enter the valid parameter: **rock** or **paper** or **scissor**");
                return;
            }

            guess = guess.ToLower();//lower the text
            int randomGuess = new Random().Next(0, 3);//generate random

            string[] arrWinReaction = { "Looks like I win the game this time.", 
                $"Sorry {Context.User.Username}, better luck next time.",
                $"No way! I guess you will have to pay me a {Config.Emoji.steak}"};//bot win
            string[] arrLoseReaction = { "I'm the world unluckiest pretty girl! :sob:", 
                "Oh no, looks like I lose the game."};//bot lose
            string[] arrDrawReaction = { "Ehh, it's a draw!","We got a draw this time." };//bot draw

            string textTemplate = $"emojicontext Doremi landed her **{MinigameCore.rockPaperScissor(randomGuess,guess)["randomResult"]}** against your **{guess}**. ";
            
            string picReactionFolderDir = "config/rps_reaction/doremi/";

            if (MinigameCore.rockPaperScissor(randomGuess,guess)["gameState"] == "win"){ // player win
                int rndIndex = new Random().Next(0, arrLoseReaction.Length);

                picReactionFolderDir += "lose";
                textTemplate = textTemplate.Replace("emojicontext", ":clap:");
                textTemplate += $"{Context.User.Username} **win** the game! You got **20** score points.\n" +
                    $"\"{arrLoseReaction[rndIndex]}\"";

                var guildId = Context.Guild.Id;
                var userId = Context.User.Id;

                //save the data
                MinigameCore.updateScore(guildId.ToString(), userId.ToString(), 10);

            } else if (MinigameCore.rockPaperScissor(randomGuess,guess)["gameState"] == "draw"){ // player draw
                int rndIndex = new Random().Next(0, arrDrawReaction.Length);
                picReactionFolderDir += "draw";
                textTemplate = textTemplate.Replace("emojicontext", ":x:");
                textTemplate += $"**The game is draw!**\n" +
                    $"\"{arrDrawReaction[rndIndex]}\"";
            } else  { //player lose
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
        
        [Command("hangman", RunMode = RunMode.Async), Summary("Play the hangman game with the available category.\n**Available category:** `random`/`characters`/`color`/`fruit`/`animal`\n" +
            "**Available difficulty:**\n" +
            "**easy:** 30 seconds, 10 lives, score+50\n" +
            "**medium:** 20 seconds, 7 lives, score+100\n" +
            "**hard:** 15 seconds, 5 lives, score+200\n")]
        public async Task Interact_Quiz_Hangman(string category="random", string difficulty = "easy")
        {
            //check first if category available on quiz.json/not
            if (category.ToLower()!="random" && !Config.Core.jobjectQuiz.ContainsKey(category.ToLower())){
                await ReplyAsync($"Sorry, I can't find that category. Available category options: **random**/**characters**/**color**/**fruit**/**animal**");
                return;
            }
            
            if(difficulty.ToLower()!="easy"&&difficulty.ToLower() != "medium" && difficulty.ToLower() != "hard"){
                await ReplyAsync($"Sorry, I can't find that difficulty. Available difficulty options: **easy**/**medium**/**hard**");
                return;
            }

            if (!Config.Doremi.isRunningMinigame.ContainsKey(Context.User.Id.ToString()))
                Config.Doremi.isRunningMinigame.Add(Context.User.Id.ToString(), false);
            
            if (!Config.Doremi.isRunningMinigame[Context.User.Id.ToString()])
            {
                Config.Doremi.isRunningMinigame[Context.User.Id.ToString()] = true;
                //default difficulty: easy
                int lives = 10; var timeoutDuration = 30;//in seconds
                int scoreValue = 50;//default score

                if (difficulty.ToLower() == "medium"){
                    lives = 7; timeoutDuration = 20; scoreValue = 100;
                } else if (difficulty.ToLower() == "hard"){
                    lives = 5; timeoutDuration = 15; scoreValue = 200;
                }
                
                string key = category;//default:random

                if (category.ToLower()=="random"){
                    //default: random
                    var jobjquiz = Config.Core.jobjectQuiz.Properties().ToList();
                    key = jobjquiz[new Random().Next(0, jobjquiz.Count)].Name;
                }

                var arrRandomed = (JArray)Config.Core.jobjectQuiz.GetValue(key);
                string randomedAnswer = arrRandomed[new Random().Next(0, arrRandomed.Count)].ToString();
                string replacedAnswer = ""; string[] containedAnswer = { }; List<string> guessedWord = new List<string>();
                for (int i = 0; i < randomedAnswer.Length; i++)
                {
                    if (randomedAnswer.Substring(i, 1) != " "){
                        replacedAnswer += randomedAnswer.Substring(i, 1).Replace(randomedAnswer.Substring(i, 1), "_ ");
                    } else if(randomedAnswer.Substring(i, 1) == " ")
                    {
                        replacedAnswer += "  ";
                    }
                    
                }
                
                string tempRandomedAnswer = string.Join(" ", randomedAnswer.ToCharArray()) + " "; //with space

                string questionsFormat = $"Can you guess what **{key}** is this?```{replacedAnswer}```";

                if (category.ToLower() == "characters"){
                    questionsFormat = $"Guess one of the ojamajo doremi characters name:```{replacedAnswer}```";
                }

                await ReplyAsync($"{Context.User.Username}, \u23F1 You have **{timeoutDuration}** seconds each turn, with **{lives}** \u2764. " +
                    $"Type **exit** to exit from the games.\n" +
                    questionsFormat);

                var response = await NextMessageAsync(timeout:TimeSpan.FromSeconds(timeoutDuration));

                while (lives > 0 && response!=null){
                    Boolean isGuessed = false;
                    string loweredResponse = response.Content.ToLower();

                    if (loweredResponse == "exit"){
                        Config.Doremi.isRunningMinigame.Remove(Context.User.Id.ToString());
                        await ReplyAsync($"**{Context.User.Username}** has left the hangman minigame.");
                        return;
                    }
                    else if (loweredResponse.Length > 1)
                        await ReplyAsync($"Sorry **{Context.User.Username}**, but you can only guess a word each turn.");
                    else if (loweredResponse == " ")
                        await ReplyAsync($"Sorry **{Context.User.Username}**, but you can't enter a whitespace character.");
                    else if (loweredResponse.Length <= 1){
                        foreach (string x in guessedWord){
                            if (loweredResponse.Contains(x)){
                                await ReplyAsync($"**{Context.User.Username}**, you already guessed **{x}**");
                                isGuessed = true;
                                break;
                            }
                        }

                        guessedWord.Add(loweredResponse);

                        if (!tempRandomedAnswer.Contains(loweredResponse) && !isGuessed)
                        {
                            lives -= 1;
                            if (lives > 0){
                                Config.Doremi.isRunningMinigame.Remove(Context.User.Id.ToString());
                                await ReplyAsync($"\u274C Sorry **{Context.User.Username}**, you guess it wrong. \u2764: **{lives}** . Category:**{key}**```{replacedAnswer}```");
                            } else {
                                lives = 0;
                                Config.Doremi.isRunningMinigame.Remove(Context.User.Id.ToString());
                                await ReplyAsync($"\u274C Sorry **{Context.User.Username}**, you're running out of guessing attempt. The correct answer is : **{randomedAnswer}**");
                                return;
                            }
                                
                        } else if (!isGuessed) {
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
                            if(replacedAnswer.Contains("_"))
                                await ReplyAsync($":white_check_mark: **{Context.User.Username}**. Category:**{key}**\n```{replacedAnswer}```");
                            else {
                                Config.Doremi.isRunningMinigame.Remove(Context.User.Id.ToString());

                                await ReplyAsync($"\uD83D\uDC4F Congratulations **{Context.User.Username}**, you guess the correct answer: **{randomedAnswer}**. Your **score+{scoreValue}**");

                                var guildId = Context.Guild.Id;
                                var userId = Context.User.Id;

                                //save the data
                                MinigameCore.updateScore(guildId.ToString(), userId.ToString(), scoreValue);
                                return;
                            }
                        }

                    }

                response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(timeoutDuration));

            }

                lives = 0;
                Config.Doremi.isRunningMinigame.Remove(Context.User.Id.ToString());
                await ReplyAsync($"\u23F1 Time's up **{Context.User.Username}**. The correct answer is : **{randomedAnswer}**.");
                return;

            }
            else
                await ReplyAsync($"Sorry **{Context.User.Username}**, you're still running the **minigame** interactive commands, please finish it first.");

        }

        [Command("dorequiz", RunMode = RunMode.Async), Summary("I will give you some quiz about Doremi.")]
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
                    int scoreValue = 20;
                    var guildId = Context.Guild.Id;
                    var userId = Context.User.Id;

                    //save the data
                    MinigameCore.updateScore(guildId.ToString(), userId.ToString(), scoreValue);

                    replyCorrect += $". Your **score+{scoreValue}**";
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

        [Command("numbers", RunMode = RunMode.Async), Alias("number","dice"), Summary("Guess if the number is lower/higher than the one I give.")]
        public async Task Interact_Minigame_Guess_Numbers()
        {
            int scoreValue = 50;
            int timeoutDuration = 15;
            int randomNumbers = new Random().Next(6, 11);
            if (!Config.Doremi.isRunningMinigame.ContainsKey(Context.User.Id.ToString()))
                Config.Doremi.isRunningMinigame.Add(Context.User.Id.ToString(), false);

            if (!Config.Doremi.isRunningMinigame[Context.User.Id.ToString()]){
                Boolean isPlaying = true;
                await ReplyAsync($"{Context.User.Username}, \uD83C\uDFB2 Number **{randomNumbers}** out of **12** has been selected.\n" +
                    $"\u23F1 You have **{timeoutDuration}** seconds to guess if the next number will be **lower** or **higher** or **same**. " +
                    $"Type **exit** to exit from the minigame.");

                var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(timeoutDuration));

                while (isPlaying&&response!=null){
                    string loweredResponse = response.Content.ToLower();

                    if (loweredResponse == "exit"){
                        Config.Doremi.isRunningMinigame.Remove(Context.User.Id.ToString());
                        await ReplyAsync($"**{Context.User.Username}** has left the numbers minigame.");
                        return;
                    } else if (loweredResponse == "same" || loweredResponse == "lower"|| loweredResponse == "higher"){
                        int nextRandomNumbers = new Random().Next(1, 13);
                        string responseResult = "";
                        Boolean isCorrect = true;//default

                        if (randomNumbers == nextRandomNumbers && loweredResponse == "same"){
                            responseResult = $"\uD83D\uDC4F Congratulations, your guess was **correct**. You got **{scoreValue}** score points.";
                        } else if (nextRandomNumbers < randomNumbers && loweredResponse == "lower") {
                            responseResult = $"\uD83D\uDC4F Congratulations, your guess was **correct**. You got **{scoreValue}** score points.";
                        } else if (nextRandomNumbers > randomNumbers && loweredResponse == "higher"){
                            responseResult = $"\uD83D\uDC4F Congratulations, your guess was **correct**. You got **{scoreValue}** score points.";
                        } else {
                            responseResult = "\u274C Sorry, your guess was **wrong**.";
                            isCorrect = false;
                        }
                        await ReplyAsync($"\uD83C\uDFB2 First number was:**{randomNumbers}**, the next selected number was: **{nextRandomNumbers}** and you guess it: **{loweredResponse}**.\n{responseResult}");

                        if (isCorrect){
                            var guildId = Context.Guild.Id;
                            var userId = Context.User.Id;
                            //save the data
                            MinigameCore.updateScore(guildId.ToString(), userId.ToString(), scoreValue);
                        }
                        return;
                    } else if (loweredResponse != "same" || loweredResponse != "lower" || loweredResponse != "higher"){
                        await ReplyAsync("Sorry, please answer it with **lower** or **higher** or **same**.");
                        response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(timeoutDuration));
                    }
                }

                Config.Doremi.isRunningMinigame.Remove(Context.User.Id.ToString());
                await ReplyAsync($"\u23F1 Time's up, **{Context.User.Username}**.");
                return;
            } else
                await ReplyAsync($"Sorry **{Context.User.Username}**, you're still running the **minigame** interactive commands, please finish it first.");
        }

    }

    [Name("pureleine"), Group("pureleine"), Summary("This category contains all pureleine interactive commands minigame.")]
    public class DoremiPureleineInteractive : InteractiveBase{
        //register, capture, spawn, leaderboard

    }

    //calculate how many days has been on the server
    //

}
