using Config;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Lavalink.NET;
using Newtonsoft.Json.Linq;
using OjamajoBot.Service;
using Spectacles.NET.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
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
                .WithTitle(":alarm_clock: Countdown to: Ojamajo Doremi: Majo Minarai o Sagashite")
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
            .WithAuthor("Dodo")
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
            //.AddField("Pop Bot", "[Invite Pop Bot](https://discordapp.com/api/oauth2/authorize?client_id=" + Config.Pop.Id + "&permissions=238419008&scope=bot)", true)
            .Build());
        }

        [Command("magical stage"), Alias("magicalstage"), Summary("I will perform magical stage along with the other and make a <wishes>")]
        public async Task magicalStage([Remainder] string wishes="")
        {
            if (wishes != "")
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
                await ReplyAsync($"Please type your wishes.");
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

        [Command("ping")]
        public async Task printPing()
        {
            await ReplyAsync($"Hello! I'm running at **{Context.Client.Latency} ms**");
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
            .WithAuthor("Doremi Harukaze", Config.Doremi.EmbedAvatarUrl)
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
        public async Task starMessages([Remainder] string MessagesOrWithAttachment="")
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
                if (sentAttachment != null)
                {
                    await sentAttachment.AddReactionAsync(new Discord.Emoji("\u2B50"));
                } else
                {
                    await sentMessage.AddReactionAsync(new Discord.Emoji("\u2B50"));
                }
                
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

            if (MessagesOrWithAttachment == "")
            {
                await ReplyAsync("Please write some text to be starred on.");
                return;
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

            await ReplyAsync(arrRandom[rndIndex, 0],
                embed: new EmbedBuilder()
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

        [Command("daily"), Alias("claim"), Summary("Water the plant and receive daily magic seeds.")]
        public async Task dailyClaimMagicalSeeds()
        {
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
            
            if (!File.Exists(playerDataDirectory)) //not registered yet
            {
                await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription($":x: I'm sorry, please register yourself first with **{Config.Doremi.PrefixParent[0]}card register** command.")
                .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build());
            } else {
                JObject arrInventory = JObject.Parse(File.ReadAllText(playerDataDirectory));
                if((string)arrInventory["magic_seeds_last_claim"] == ""||
                    (string)arrInventory["magic_seeds_last_claim"]!= DateTime.Now.ToString("dd"))
                {
                    int randomedReceive = new Random().Next(1, 6);
                    int totalMagicSeeds = Convert.ToInt32(arrInventory["magic_seeds"]) + randomedReceive;
                    await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithDescription($":seedling: {MentionUtils.MentionUser(clientId)} have watered the plant and received {randomedReceive} magic seed that makes total of: **{totalMagicSeeds.ToString()}** magic seeds." +
                    $"Thank you for watering it~")
                    .WithThumbnailUrl(TradingCardCore.imgMagicSeeds).Build());
                    
                    arrInventory["magic_seeds"] = totalMagicSeeds.ToString();
                    arrInventory["magic_seeds_last_claim"] = DateTime.Now.ToString("dd");
                    File.WriteAllText(playerDataDirectory, arrInventory.ToString());
                } else
                {
                    var now = DateTime.Now;
                    var tomorrow = now.AddDays(1).Date;
                    double totalHours = (tomorrow - now).TotalHours;

                    await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithDescription($":x: Sorry, you have received your daily magic seeds.\nPlease wait for **{Math.Floor(totalHours)}** hour(s) " +
                    $"**{Math.Ceiling(60*(totalHours - Math.Floor(totalHours)))}** more minute(s) until the next growing time.")
                    .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build());
                    
                }
            }
        }

        [Command("seeds"), Alias("magicseeds"), Summary("See the total of magic seeds that you have.")]
        public async Task showTotalSeeds()
        {
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";

            if (!File.Exists(playerDataDirectory)) //not registered yet
            {
                await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription($":x: I'm sorry, please register yourself first with **{Config.Doremi.PrefixParent[0]}card register** command.")
                .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build());
            } else {
                JObject arrInventory = JObject.Parse(File.ReadAllText(playerDataDirectory));
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithDescription($":seedling: {MentionUtils.MentionUser(clientId)} have **{arrInventory["magic_seeds"]}** magic seeds.")
                    .Build());
            }
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
                    string title = $"**{GlobalFunctions.UppercaseFirst(season)} Season Episodes List**\n";
                    var arrList = (JArray)Config.Core.jObjWiki.GetValue("episodes")[season];
                    string tempVal = "";

                    int currentIndex = 0; int indexPage = 0;
                    for (int i = 0; i < arrList.Count; i++)
                    {
                        string replacedUrl = arrList[i].ToString().Replace(" ", "_");
                        replacedUrl = Config.Core.wikiParentUrl + replacedUrl.ToString().Replace("?", "%3F");
                        tempVal += $"Ep {i + 1}: [{arrList[i]}]({replacedUrl})\n";

                        if (currentIndex < 14)
                        {
                            currentIndex++;
                        }
                        else
                        {
                            pageContent.Add(tempVal);
                            currentIndex = 0;
                            tempVal = "";

                            if (i == arrList.Count - 1)
                            {
                                pageContent.Add(tempVal);
                            };
                            indexPage += 1;
                        }

                    }

                    PaginatedAppearanceOptions pao = new PaginatedAppearanceOptions();
                    pao.JumpDisplayOptions = JumpDisplayOptions.Never;
                    pao.DisplayInformationIcon = false;

                    var pager = new PaginatedMessage
                    {
                        Title = title,
                        Pages = pageContent,
                        Color = Config.Doremi.EmbedColor,
                        Options = pao
                    };

                    await PagedReplyAsync(pager);

                }
                catch (Exception e) { Console.WriteLine("Doremi wiki episodes error:" + e.ToString()); }

            } else {
                await ReplyAsync($"I'm sorry, but I can't find that season. See `{Config.Doremi.PrefixParent[0]}help wiki episodes` for commands help.");
            }

        }

        [Command("witches", RunMode = RunMode.Async), Alias("witch"), Summary("I will give all witches characters list. " +
            "Fill the optional <characters> parameter with the available witches characters name.")]
        public async Task showCharactersWitches([Remainder]string characters = "")
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.Color = Config.Doremi.EmbedColor;

            if (characters == "")
            {
                try
                {
                    builder.Title = "Witches Characters List";
                    var arrList = ((JObject)Config.Core.jObjWiki.GetValue("witches")).Properties().ToList();
                    for (int i = 0; i < arrList.Count; i++)
                    {
                        builder.AddField(arrList[i].Name, $"[wiki link]({Config.Core.wikiParentUrl + arrList[i].Value["url"]})", true);
                    }
                    builder.WithFooter($"I found {arrList.Count} witches characters from ojamajo witchling wiki");
                    await ReplyAsync(embed: builder.Build());
                    return;
                }
                catch (Exception e) { Console.WriteLine("Doremi wiki witches characters error:" + e.ToString()); }
            }
            else
            {
                if (((JObject)Config.Core.jObjWiki.GetValue("witches")).ContainsKey(characters))
                {
                    var arrList = Config.Core.jObjWiki.GetValue("witches")[characters];
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
                }
                else await ReplyAsync("I'm sorry, but I can't find that witches characters. " +
                  $"See `{Config.Doremi.PrefixParent[0]}wiki witches` to display all witches characters list.");
            }
        }

        [Command("wizards", RunMode = RunMode.Async), Alias("wizard"), Summary("I will give all wizards characters list. " +
            "Fill the optional <characters> parameter with the available wizards characters name.")]
        public async Task showCharactersWizards([Remainder]string characters = "")
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.Color = Config.Doremi.EmbedColor;

            if (characters == "")
            {
                try
                {
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
            }
            else
            {
                if (((JObject)Config.Core.jObjWiki.GetValue("wizards")).ContainsKey(characters))
                {
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
                }
                else await ReplyAsync("I'm sorry, but I can't find that wizards characters. " +
                $"See `{Config.Doremi.PrefixParent[0]}wiki wizards` to display all wizards characters list.");
            }
        }

    }

    [Name("role"), Group("role"), Summary("These contains role commands category.")]
    public class DoremiRoles : InteractiveBase
    {
        [Command("list"), Summary("Show all the available self assignable role list.")]
        public async Task showRoleList()
        {
            var guildId = Context.Guild.Id;
            var guildJsonFile = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json"));

            PaginatedAppearanceOptions pao = new PaginatedAppearanceOptions();
            pao.JumpDisplayOptions = JumpDisplayOptions.Never;
            pao.DisplayInformationIcon = false;

            List<string> pageContent = new List<string>();
            string title = $"";
            JArray arrList = (JArray)guildJsonFile["roles_list"];

            string tempVal = title;
            int currentIndex = 0;
            for (int i = 0; i < arrList.ToList().Count(); i++)
            {
                var roleSearch = Context.Guild.Roles.FirstOrDefault(x => x.Id == Convert.ToUInt64(arrList[i]));
                if (roleSearch != null)
                {
                    tempVal += $"**{roleSearch.Name}**\n";
                }

                if (currentIndex <= 10) currentIndex++;
                else
                {
                    pageContent.Add(tempVal);
                    currentIndex = 0;
                    tempVal = title;
                }

                if (i == arrList.ToList().Count - 1) pageContent.Add(tempVal);
            }

            if (arrList.ToList().Count == 0)
            {
                tempVal = "There are no self assignable role list yet.";
                pageContent.Add(tempVal);
            }

            var pager = new PaginatedMessage
            {
                Title = $"**Self Assignable Role List**\n",
                Pages = pageContent,
                Color = Config.Doremi.EmbedColor,
                Options = pao
            };

            await PagedReplyAsync(pager);

        }

        [Command("set"), Summary("Set your roles with given role parameter. Use `do!role list` to display all self assignable roles list.")]
        public async Task setRole(string role)
        {
            var guildId = Context.Guild.Id;
            var guildJsonFile = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json"));
            var embed = new EmbedBuilder().WithColor(Config.Doremi.EmbedColor);

            var roleSearch = Context.Guild.Roles.FirstOrDefault(x => x.Name == role);
            var userRoles = Context.Guild.GetUser(Context.User.Id).Roles.FirstOrDefault(x => x.Name == role);

            if (roleSearch == null)
                await ReplyAsync($"Sorry, I can't find that role.");
            else
            {
                JArray item = (JArray)guildJsonFile["roles_list"];
                if (userRoles != null)
                {
                    await ReplyAsync("You already have that roles.");
                } else
                {
                    if (item.ToString().Contains(roleSearch.Id.ToString()))
                    {
                        await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(roleSearch);
                        await ReplyAsync(embed: embed
                            .WithTitle("Role updated!")
                            .WithDescription($":white_check_mark: **{Context.User.Username}** have new role: {MentionUtils.MentionRole(roleSearch.Id)}")
                            .Build());
                    }
                    else
                    {
                        await ReplyAsync(embed: embed
                            .WithDescription($"Sorry, you can't assign into that role.")
                            .Build());
                    }
                }   
            }
        }

        [Command("remove"), Summary("Remove your roles from given role parameter.")]
        public async Task removeRole(string role)
        {
            var guildId = Context.Guild.Id;
            var embed = new EmbedBuilder().WithColor(Config.Doremi.EmbedColor);

            var roleSearch = Context.Guild.Roles.FirstOrDefault(x => x.Name == role);
            var userRoles = Context.Guild.GetUser(Context.User.Id).Roles.FirstOrDefault(x => x.Name == role);

            if (roleSearch == null)
            {
                await ReplyAsync(embed: embed
                        .WithDescription($"Sorry, I can't find that role.")
                        .Build());
            }
            else if (userRoles == null)
            {
                await ReplyAsync(embed: embed
                    .WithDescription($"You already have that roles.")
                    .Build());
            }
            else
            {   
                await Context.Guild.GetUser(Context.User.Id).RemoveRoleAsync(roleSearch);
                await ReplyAsync(embed: embed
                    .WithTitle("Role removed!")
                    .WithDescription($":white_check_mark: **{Context.User.Username}** role has been removed from: {MentionUtils.MentionRole(roleSearch.Id)}")
                    .Build());
                
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
    public class DoremiModerator : InteractiveBase
    {
        //leaving_message
        [Command("user leave"), Summary("Set the leaving user notifications with **off** or **on**.")]
        public async Task assignUserLeavingNotification(string settings){
            string replacedsettings = settings.Replace("off", "0").Replace("on", "1");
            Config.Guild.setPropertyValue(Context.Guild.Id, "user_leaving_notification", replacedsettings);
            await ReplyAsync($"**Leaving User Messages** has been turned **{settings}**.");
        }

        [Name("mod role"), Group("role"), Summary("These commands contains all self assignable role list command. " +
            "Requires `manage roles permission`.")]
        public class DoremiModeratorRoles : InteractiveBase
        {
            [Command("add"), Summary("Add role to self assignable role list.")]
            public async Task addSelfAssignableRoles(string roleId)
            {
                var embed = new EmbedBuilder().WithColor(Config.Doremi.EmbedColor);

                var roleSearch = Context.Guild.Roles.FirstOrDefault(x => x.Id == Convert.ToUInt64(roleId));
                if (roleSearch == null)
                    await ReplyAsync($"Sorry, I can't find that role id. See the role list with " +
                        $"**{Config.Doremi.PrefixParent[0]}mod role list**");
                 else
                {
                    var guildId = Context.Guild.Id;
                    var fileDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json";
                    var guildJsonFile = JObject.Parse(File.ReadAllText(fileDirectory));

                    JArray item = (JArray)guildJsonFile["roles_list"];
                    if (item.ToString().Contains(roleId))
                    {
                        await ReplyAsync(embed: embed
                         .WithTitle("Error!")
                         .WithDescription("That roles already existed on self assignable roles list.")
                         .Build());
                    } else {
                        item.Add(roleId);
                        File.WriteAllText(fileDirectory, guildJsonFile.ToString());

                        await ReplyAsync(embed: embed
                            .WithTitle("Success adding new roles")
                            .WithDescription($"{MentionUtils.MentionRole(Convert.ToUInt64(roleId))} has been added into role list.")
                            .Build());
                    }
                    
                }

                
            }

            [Command("remove"), Summary("Remove roles from self assignable role list.")]
            public async Task removeSelfAssignableRoles(string roleId)
            {
                var embed = new EmbedBuilder().WithColor(Config.Doremi.EmbedColor);

                var roleSearch = Context.Guild.Roles.FirstOrDefault(x => x.Id == Convert.ToUInt64(roleId));
                if (roleSearch == null)
                    await ReplyAsync($"Sorry, I can't find that role id. See the role list with " +
                        $"**{Config.Doremi.PrefixParent[0]}mod role list**");
                else
                {
                    var guildId = Context.Guild.Id;
                    var fileDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json";
                    var guildJsonFile = JObject.Parse(File.ReadAllText(fileDirectory));

                    JArray item = (JArray)guildJsonFile["roles_list"];
                    if (item.ToString().Contains(roleId))
                    {
                        var founded = item.FirstOrDefault(x => roleId != null);
                        if (founded != null)
                            item.Remove(founded);
                        
                        File.WriteAllText(fileDirectory, guildJsonFile.ToString());
                        await ReplyAsync(embed: embed
                            .WithTitle("Role has been removed from the list!")
                            .WithDescription($"Role has been removed from self assignable roles list.")
                            .Build());
                    }
                    else
                    {
                        await ReplyAsync(embed: embed
                            .WithTitle("Error!")
                            .WithDescription("Can't remove that role because it's not on the self assignable roles list.")
                            .Build());
                    }

                }
            }

            [Command("list"), Summary("Show all role list along with the assignable status.")]
            public async Task listSelfAssignableRolesList()
            {
                var guildId = Context.Guild.Id;
                var guildJsonFile = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json"));

                PaginatedAppearanceOptions pao = new PaginatedAppearanceOptions();
                pao.JumpDisplayOptions = JumpDisplayOptions.Never;
                pao.DisplayInformationIcon = false;

                List<string> pageContent = new List<string>();
                string title = $"**Role Name - ID - Self Assignable?**\n";
                JArray arrList = (JArray)guildJsonFile["roles_list"];
                var allRoles = Context.Guild.Roles.Where(x => x.Id!= Context.Guild.EveryoneRole.Id); 

                string tempVal = title;
                int currentIndex = 0;
                for (int i = 0; i < allRoles.ToList().Count(); i++)
                {
                    //aa : 12334 - yes
                    tempVal += $"**{allRoles.ElementAt(i).Name}** - {allRoles.ElementAt(i).Id} - ";
                    if (arrList.ToString().Contains(allRoles.ElementAt(i).Id.ToString()))
                    {
                        tempVal += "yes";
                    } else
                    {
                        tempVal += "no";
                    }
                    //if (arrList.Contains(allRoles.ElementAt(i).Id))
                    //    tempVal += "yes";
                    //else
                    //    tempVal += "no";
                    tempVal += "\n";

                    if (currentIndex <=10) currentIndex++;
                    else
                    {
                        pageContent.Add(tempVal);
                        currentIndex = 0;
                        tempVal = title;
                    }

                    if (i == allRoles.ToList().Count - 1) pageContent.Add(tempVal);
                }

                var pager = new PaginatedMessage
                {
                    Title = $"**Self Assignable Role List**\n",
                    Pages = pageContent,
                    Color = Config.Doremi.EmbedColor,
                    Options = pao
                };

                await PagedReplyAsync(pager);

            }
        }

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

        [Name("mod channels"), Group("channels"), Summary("These commands require `Manage Channels` permissions.")]
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
                        builder.ThumbnailUrl = "https://i.4pcdn.org/s4s/1508005628768.jpg";

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
                string property; Boolean propertyValueExisted = false;
                var guildId = Context.Guild.Id;
                
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

        [Name("mod trading card"), Group("trading card"), Summary("These commands require `Manage Card`|`Manage Roles` permissions.")]
        public class DoremiModeratorTradingCards : ModuleBase<SocketCommandContext>
        {
            //trading card configuration section
            //assign trading card spawning channel
            [Command("spawn"), Summary("Set Doremi Bot and the others to make the trading card spawned at <channel_name>.")]
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
                    await TradingCardCore.generateCardSpawn(guildId);
                },
                null,
                TimeSpan.FromMinutes(Convert.ToInt32(Config.Guild.getPropertyValue(guildId, "trading_card_spawn_interval"))), //time to wait before executing the timer for the first time
                TimeSpan.FromMinutes(Convert.ToInt32(Config.Guild.getPropertyValue(guildId, "trading_card_spawn_interval"))) //time to wait before executing the timer again
                );
            }

            //set spawning interval
            [Command("interval"), Summary("Set the trading card spawn interval (in minutes).")]
            public async Task setTradingCardSpawnInterval(int interval_minutes)
            {
                if (interval_minutes <= 4 || interval_minutes >= 1441) await ReplyAsync($"Please enter the interval between 5-1440 (in minutes)");
                else
                {
                    var guildId = Context.Guild.Id;
                    Config.Guild.setPropertyValue(guildId, "trading_card_spawn_interval", interval_minutes.ToString());
                    await ReplyAsync($"**Trading Card Spawn interval** has been set into **{interval_minutes}** minute(s)");

                    if (Config.Doremi._timerTradingCardSpawn.ContainsKey(guildId.ToString()))
                        Config.Doremi._timerTradingCardSpawn[guildId.ToString()].Change(
                            TimeSpan.FromMinutes(Convert.ToInt32(Config.Guild.getPropertyValue(guildId, "trading_card_spawn_interval"))),
                            TimeSpan.FromMinutes(Convert.ToInt32(Config.Guild.getPropertyValue(guildId, "trading_card_spawn_interval"))));
                }
            }

            [Command("badge role create"), Summary("Register the trading card role completionist.")]
            public async Task initTradingCardRoleCompletionist()
            {
                var roleDoremi = Context.Guild.Roles.Where(x=>x.Name==TradingCardCore.Doremi.roleCompletionist).ToList();
                var roleHazuki = Context.Guild.Roles.Where(x=>x.Name==TradingCardCore.Hazuki.roleCompletionist).ToList();
                var roleAiko = Context.Guild.Roles.Where(x=>x.Name==TradingCardCore.Aiko.roleCompletionist).ToList();
                var roleOnpu = Context.Guild.Roles.Where(x=>x.Name==TradingCardCore.Onpu.roleCompletionist).ToList();
                var roleMomoko = Context.Guild.Roles.Where(x=>x.Name==TradingCardCore.Momoko.roleCompletionist).ToList();
                var roleSpecial = Context.Guild.Roles.Where(x=>x.Name==TradingCardCore.roleCompletionistSpecial).ToList();

                if (roleDoremi.Count <= 0)
                    await Context.Guild.CreateRoleAsync(
                        TradingCardCore.Doremi.roleCompletionist, null, color: Config.Doremi.EmbedColor,false,false
                    );
                
                
                if (roleHazuki.Count <= 0)
                    await Context.Guild.CreateRoleAsync(
                        TradingCardCore.Hazuki.roleCompletionist, null, color: Config.Hazuki.EmbedColor, false, false
                    );

                if (roleAiko.Count <= 0)
                    await Context.Guild.CreateRoleAsync(
                        TradingCardCore.Aiko.roleCompletionist, null, color: Config.Aiko.EmbedColor, false, false
                    );

                if (roleOnpu.Count <= 0)
                    await Context.Guild.CreateRoleAsync(
                        TradingCardCore.Onpu.roleCompletionist, null, color: Config.Onpu.EmbedColor, false, false
                    );

                if (roleMomoko.Count <= 0)
                    await Context.Guild.CreateRoleAsync(
                        TradingCardCore.Momoko.roleCompletionist, null, color: Config.Momoko.EmbedColor, false, false
                    );

                if (roleSpecial.Count <= 0)
                    await Context.Guild.CreateRoleAsync(
                        TradingCardCore.roleCompletionistSpecial, null, color: TradingCardCore.roleCompletionistColor, false, false
                    );

                await ReplyAsync($":white_check_mark: **Trading Card Badge Role** has been created!");

            }

            [Command("remove"), Summary("Remove the trading card spawn settings.")]
            public async Task removeTradingCardSpawn()
            {
                var guildId = Context.Guild.Id;
                if (Config.Guild.hasPropertyValues(guildId.ToString(), "trading_card_spawn"))
                {
                    Config.Guild.setPropertyValue(Context.Guild.Id, "trading_card_spawn", "");
                    if (Config.Doremi._timerTradingCardSpawn.ContainsKey(Context.Guild.Id.ToString()))
                        Config.Doremi._timerTradingCardSpawn[Context.Guild.Id.ToString()].Change(Timeout.Infinite, Timeout.Infinite);

                    //reset spawn settings
                    Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyId, "");
                    Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyCategory, "");
                    Config.Guild.setPropertyValue(guildId, TradingCardCore.propertyToken, "");
                    await ReplyAsync($"**Trading Card Spawn Channels** settings has been removed.");
                } else
                    await ReplyAsync($"**Trading Card Spawn Channels** has no settings yet.");
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

        //[Command("debug badcards", RunMode = RunMode.Async), Alias("catch"), Summary("Bad card debug")]
        //public async Task badCardsDebug()
        //{
        //    var guildId = Context.Guild.Id;
        //    await TradingCardCore.generateCardSpawn(guildId);
        //}

        //[Command("debug showspawn", RunMode = RunMode.Async), Alias("catch"), Summary("Bad card debug")]
        //public async Task showSpawnDebug()
        //{
        //    var guildId = Context.Guild.Id;
        //    await TradingCardCore.printCardSpawned(guildId);
        //}

        [Command("pureleine", RunMode = RunMode.Async), Alias("pureline"), Summary("Detect the bad card with the help from oyajide & pureleine computer. " +
            "Insert the answer as parameter to remove the bad cards if it's existed. Example: do!card pureleine 10")]
        public async Task trading_card_pureleine(string answer = "")
        {
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
            
            if (!File.Exists(playerDataDirectory)) //not registered yet
            {
                await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription($":x: I'm sorry, please register yourself first with **{Config.Doremi.PrefixParent[0]}card register** command.")
                .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build());
            }
            else
            {
                await ReplyAsync(embed: TradingCardCore.activatePureleine(guildId,clientId.ToString(),answer).Build());
            }
        }

        [Command("capture", RunMode = RunMode.Async), Alias("catch"), Summary("Capture spawned card with Doremi.")]
        public async Task trading_card_doremi_capture(string boost = "")
        {
            //reference: https://www.newtonsoft.com/json/help/html/ModifyJson.htm
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;

            var cardCaptureReturn = TradingCardCore.cardCapture(Config.Doremi.EmbedColor, Context.Client.CurrentUser.GetAvatarUrl(), guildId, clientId.ToString(), Context.User.Username,
            TradingCardCore.Doremi.emojiError, "doremi", boost, Config.Doremi.PrefixParent[0], "do",
            TradingCardCore.Doremi.maxNormal, TradingCardCore.Doremi.maxPlatinum, TradingCardCore.Doremi.maxMetal, TradingCardCore.Doremi.maxOjamajos);

            if (cardCaptureReturn.Item1 == "")
                await ReplyAsync(embed: cardCaptureReturn.Item2.Build());
            else
                await ReplyAsync(cardCaptureReturn.Item1,
                    embed: cardCaptureReturn.Item2.Build());

            //check if player is ranked up
            if (cardCaptureReturn.Item3!="")
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
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
                    .userCompleteTheirList(Config.Doremi.EmbedColor, Context.Client.CurrentUser.GetAvatarUrl(), "doremi",
                    TradingCardCore.Doremi.imgCompleteAllCard, Context.Guild.Id.ToString(),
                    Context.User.Id.ToString(), TradingCardCore.Doremi.roleCompletionist, Context.User.Username, Context.User.GetAvatarUrl())
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
                    .userCompleteTheirList(Config.Hazuki.EmbedColor, Context.Client.CurrentUser.GetAvatarUrl(), "hazuki",
                    TradingCardCore.Hazuki.imgCompleteAllCard, Context.Guild.Id.ToString(),
                    Context.User.Id.ToString(), TradingCardCore.Hazuki.roleCompletionist, Context.User.Username, Context.User.GetAvatarUrl())
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
                    .userCompleteTheirList(Config.Aiko.EmbedColor, Context.Client.CurrentUser.GetAvatarUrl(), "aiko",
                    TradingCardCore.Aiko.imgCompleteAllCard, Context.Guild.Id.ToString(),
                    Context.User.Id.ToString(), TradingCardCore.Aiko.roleCompletionist, Context.User.Username, Context.User.GetAvatarUrl())
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
                    .SendFileAsync(TradingCardCore.Onpu.imgCompleteAllCard, null, embed: TradingCardCore
                    .userCompleteTheirList(Config.Onpu.EmbedColor, Context.Client.CurrentUser.GetAvatarUrl(), "onpu",
                    TradingCardCore.Onpu.imgCompleteAllCard, Context.Guild.Id.ToString(),
                    Context.User.Id.ToString(), TradingCardCore.Onpu.roleCompletionist, Context.User.Username, Context.User.GetAvatarUrl())
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
                    .SendFileAsync(TradingCardCore.Momoko.imgCompleteAllCard, null, embed: TradingCardCore
                    .userCompleteTheirList(Config.Momoko.EmbedColor, Context.Client.CurrentUser.GetAvatarUrl(), "momoko",
                    TradingCardCore.Momoko.imgCompleteAllCard, Context.Guild.Id.ToString(),
                    Context.User.Id.ToString(), TradingCardCore.Momoko.roleCompletionist, Context.User.Username, Context.User.GetAvatarUrl())
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

                    await Bot.Doremi.client
                        .GetGuild(Context.Guild.Id)
                        .GetTextChannel(Context.Channel.Id)
                        .SendFileAsync(TradingCardCore.imgCompleteAllCardSpecial, null, embed: TradingCardCore
                        .userCompleteTheirList(TradingCardCore.roleCompletionistColor, Context.Client.CurrentUser.GetAvatarUrl(), "other",
                        TradingCardCore.imgCompleteAllCardSpecial, Context.Guild.Id.ToString(),
                        Context.User.Id.ToString(), TradingCardCore.roleCompletionistSpecial, Context.User.Username, Context.User.GetAvatarUrl())
                        .Build());

                }
            }
            
        }

        //list all cards that have been collected
        [Command("inventory", RunMode = RunMode.Async), Summary("List all **Doremi** trading cards that you have collected.")]
        public async Task trading_card_open_inventory(string category="")
        {
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
            var jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));

            string parent = "doremi";

            if (!File.Exists(playerDataDirectory)) //not registered yet
            {
                await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription($":x: I'm sorry, please register yourself first with **{Config.Doremi.PrefixParent[0]}card register** command.")
                .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build());
            }
            else
            {
                JArray arrList;
                var playerData = JObject.Parse(File.ReadAllText(playerDataDirectory));
                EmbedBuilder builder = new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor);

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
                    if (showAllInventory || category.ToLower() == "normal") {
                        category = "normal"; arrList = (JArray)playerData[parent][category];
                        if (arrList.Count >= 1)
                        {
                            PaginatedAppearanceOptions pao = new PaginatedAppearanceOptions();
                            pao.JumpDisplayOptions = JumpDisplayOptions.Never;
                            pao.DisplayInformationIcon = false;

                            await PagedReplyAsync(
                                TradingCardCore.printInventoryTemplate(Config.Doremi.EmbedColor, "doremi", "doremi", category, jObjTradingCardList, arrList, TradingCardCore.Doremi.maxNormal, Context.User.Username,
                                Context.User.GetAvatarUrl())
                                );

                        }
                        else
                        {
                            await ReplyAsync(embed: TradingCardCore.printEmptyInventoryTemplate(
                                Config.Doremi.EmbedColor, "doremi", category, TradingCardCore.Doremi.maxNormal, Context.User.Username)
                                .Build());
                        }
                    }

                    //platinum category
                    if (showAllInventory || category.ToLower() == "platinum")
                    {
                        category = "platinum"; arrList = (JArray)playerData[parent][category];
                        if (arrList.Count >= 1)
                        {
                            await PagedReplyAsync(
                                TradingCardCore.printInventoryTemplate(Config.Doremi.EmbedColor, "doremi", "doremi", category, jObjTradingCardList, arrList, TradingCardCore.Doremi.maxPlatinum, Context.User.Username,
                                Context.User.GetAvatarUrl())
                            );
                        }
                        else
                        {
                            await ReplyAsync(embed: TradingCardCore.printEmptyInventoryTemplate(
                                Config.Doremi.EmbedColor, "doremi", category, TradingCardCore.Doremi.maxPlatinum, Context.User.Username)
                                .Build());
                        }
                    }

                    //metal category
                    if (showAllInventory || category.ToLower() == "metal")
                    {
                        category = "metal"; arrList = (JArray)playerData[parent][category];
                        if (arrList.Count >= 1)
                        {
                            await PagedReplyAsync(
                                TradingCardCore.printInventoryTemplate(Config.Doremi.EmbedColor, "doremi", "doremi", category, jObjTradingCardList, arrList, TradingCardCore.Doremi.maxMetal, Context.User.Username,
                                Context.User.GetAvatarUrl())
                            );
                        }
                        else
                        {
                            await ReplyAsync(embed: TradingCardCore.printEmptyInventoryTemplate(
                                Config.Doremi.EmbedColor, "doremi", category, TradingCardCore.Doremi.maxMetal, Context.User.Username)
                                .Build());
                        }
                    }

                    //ojamajos category
                    if (showAllInventory || category.ToLower() == "ojamajos")
                    {
                        category = "ojamajos"; arrList = (JArray)playerData[parent][category];
                        if (arrList.Count >= 1)
                        {
                            await PagedReplyAsync(
                                TradingCardCore.printInventoryTemplate(Config.Doremi.EmbedColor, "doremi", "doremi", category, jObjTradingCardList, arrList, TradingCardCore.Doremi.maxOjamajos, Context.User.Username,
                                Context.User.GetAvatarUrl())
                            );
                        }
                        else
                        {
                            await ReplyAsync(embed: TradingCardCore.printEmptyInventoryTemplate(
                                Config.Doremi.EmbedColor, "doremi", category, TradingCardCore.Doremi.maxOjamajos, Context.User.Username)
                                .Build());
                        }
                    }

                    //special category
                    if (showAllInventory || category.ToLower() == "special")
                    {
                        category = "special"; arrList = (JArray)playerData["other"][category];
                        if (arrList.Count >= 1)
                        {
                            await PagedReplyAsync(
                                TradingCardCore.printInventoryTemplate(Config.Doremi.EmbedColor, "other", "other", category, jObjTradingCardList, arrList, TradingCardCore.maxSpecial, Context.User.Username,
                                Context.User.GetAvatarUrl())
                            );
                        }
                        else
                        {
                            await ReplyAsync(embed: TradingCardCore.printEmptyInventoryTemplate(
                                Config.Doremi.EmbedColor, "other", category, TradingCardCore.maxSpecial, Context.User.Username)
                                .Build());
                        }
                    }

                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
                
            }

        }

        [Command("detail", RunMode = RunMode.Async), Alias("info", "look"), Summary("See the detail of Doremi card information from the <card_id>.")]
        public async Task trading_card_look(string card_id)
        {
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;

            await ReplyAsync(embed: TradingCardCore.printCardDetailTemplate(Config.Doremi.EmbedColor, guildId.ToString(),
                clientId.ToString(), card_id, "doremi", TradingCardCore.Doremi.emojiError, ":x: Sorry, I can't find that card ID.")
                    .Build());
        }

        [Command("status", RunMode = RunMode.Async), Summary("Show your Trading Card Status report.")]
        public async Task trading_card_status()
        {
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;
            await ReplyAsync(embed: TradingCardCore.
                    printStatusTemplate(Config.Doremi.EmbedColor, Context.User.Username, guildId.ToString(), clientId.ToString(),
                    TradingCardCore.Doremi.emojiError,Context.User.GetAvatarUrl())
                    .Build());
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
        [Command("trade", RunMode = RunMode.Async), Summary("Open the trading card hub which lets you trade the card with each other.")]
        public async Task trading_card_trade()
        {
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;
            string userFolderDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}";

            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
            var jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));

            PaginatedAppearanceOptions pao = new PaginatedAppearanceOptions();
            pao.JumpDisplayOptions = JumpDisplayOptions.Never;
            pao.DisplayInformationIcon = false;

            if (!File.Exists(playerDataDirectory)) //not registered yet
            {
                await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription($":x: I'm sorry, please register yourself first with **{Config.Doremi.PrefixParent[0]}card register** command.")
                .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build());
                return;
            }

            var playerData = JObject.Parse(File.ReadAllText(playerDataDirectory));
            int fileCount = Directory.GetFiles(userFolderDirectory).Length;

            if (fileCount <= 1)
            {
                await ReplyAsync("Sorry, server need to have more than 1 user that register the trading card data.");
                return;
            }
            else
            {
                try
                {
                    //start read all user id
                    List<string> arrUserId = new List<string>();
                    List<string> pageContent = new List<string>();
                    List<string> pageContentUserList = new List<string>();
                    //List<string> pageUserContent = new List<string>();
                    //List<string> pageUserCardList = new List<string>();
                    //List<string> pageUserOtherCardList = new List<string>();

                    //selection variables
                    //other users
                    string selectionUserId = ""; string selectionOtherUserCardChoiceId = ""; string selectionOtherUserCardPack = "";
                    string selectionOtherUserCardCategory = "";
                    //your selection
                    string selectionYourCardChoiceId = ""; string selectionYourCardPack = "";
                    string selectionYourCardCategory = "";

                    DirectoryInfo d = new DirectoryInfo(userFolderDirectory);//Assuming Test is your Folder
                    FileInfo[] Files = d.GetFiles("*.json"); //Getting Text files

                    //user selection
                    string titleUserSelection = $"**Step 1 - Select the user with numbers**\n";
                    string tempVal = titleUserSelection;
                    int currentIndex = 0;

                    int ctr = 0;
                    foreach (FileInfo file in Files)
                    {
                        ulong otherUserId = Convert.ToUInt64(Path.GetFileNameWithoutExtension(file.Name));
                        if (otherUserId != clientId)
                        {
                            arrUserId.Add(otherUserId.ToString());
                            tempVal += $"**{ctr + 1}. {MentionUtils.MentionUser(otherUserId)}**\n";

                            if (currentIndex < 14) currentIndex++;
                            else
                            {
                                pageContentUserList.Add(tempVal);
                                currentIndex = 0;
                                tempVal = titleUserSelection;
                            }

                            if (ctr == fileCount - 2) pageContentUserList.Add(tempVal);
                            ctr++;
                        }
                    }

                    var pagerUserList = new PaginatedMessage
                    {
                        Pages = pageContentUserList,
                        Color = Config.Doremi.EmbedColor,
                        Options = pao
                    };
                    //end user selection

                    Boolean isTrading = true;
                    var timeoutDuration = TimeSpan.FromSeconds(60);
                    string replyTimeout = ":stopwatch: I'm sorry, but you have reach your timeout. " +
                        $"Please use the `{Config.Doremi.PrefixParent[0]}card trade` command again to retry the trade process.";
                    int stepProcess = 1;//0/1:select the user,
                                        //2: select your card pack, 3: select card category, 4: select 
                                        //5:review process
                    Boolean newStep = true;
                    //select user
                    await ReplyAsync(embed: new EmbedBuilder()
                        .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                        .WithDescription($"Welcome to {TradingCardCore.Doremi.embedName}. " +
                        $"Here you can trade your trading card with each other. " +
                        $"You can type **cancel**/**exit** anytime on each steps to cancel the trade process.\n" +
                        $"You can type **back** anytime on each steps to back into previous steps.\n" +
                        $"Some of the trade rules that will be applied:\n" +
                        $"-You **cannot** trade for more than once to the same user in the trading queue.\n" +
                        $"-You **cannot** trade card that you or that user already had.")
                        .WithColor(Config.Doremi.EmbedColor)
                        .Build());
                    await PagedReplyAsync(pagerUserList);
                    var response = await NextMessageAsync(timeout: timeoutDuration);
                    newStep = false;
                    while (isTrading)
                    {

                        List<string> arrUserCardList = new List<string>();
                        List<string> arrUserOtherCardList = new List<string>();

                        try
                        {
                            var checkNull = response.Content.ToLower().ToString();
                        }
                        catch
                        {
                            await ReplyAsync(replyTimeout);
                            isTrading = false;
                            return;
                        }

                        //response = await NextMessageAsync(timeout: timeoutDuration);
                        //string responseText = response.Content.ToLower().ToString();

                        if (response.Content.ToString().ToLower() == "cancel" ||
                            response.Content.ToString().ToLower() == "exit")
                        {
                            await ReplyAsync(embed: new EmbedBuilder()
                                .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                                .WithDescription($"You have cancel your trade. Thank you for using the {TradingCardCore.Doremi.embedName}")
                                .WithColor(Config.Doremi.EmbedColor)
                                .Build());
                            isTrading = false;
                            return;
                        }
                        else if (stepProcess == 1)
                        { //select user
                            var isNumeric = int.TryParse(response.Content.ToString().ToLower(), out int n);
                            if (newStep)
                            {
                                newStep = false;
                                await PagedReplyAsync(pagerUserList);
                                response = await NextMessageAsync(timeout: timeoutDuration);
                            }
                            else
                            {
                                if (!isNumeric)
                                {
                                    stepProcess = 1;
                                    selectionUserId = "";
                                    await ReplyAsync(":x: Please re-type the proper number selection.");
                                    await PagedReplyAsync(pagerUserList);
                                    response = await NextMessageAsync(timeout: timeoutDuration);
                                }
                                //array length:2[0,1], selected:2
                                else if (Convert.ToInt32(response.Content.ToLower().ToString()) <= 0 ||
                                    Convert.ToInt32(response.Content.ToString().ToLower()) > arrUserId.Count)
                                {
                                    stepProcess = 1;
                                    selectionUserId = "";
                                    await ReplyAsync(":x: That number choice is not on the list. Please re-type the proper number selection.");
                                    await PagedReplyAsync(pagerUserList);
                                    response = await NextMessageAsync(timeout: timeoutDuration);
                                }
                                else
                                {
                                    selectionUserId = arrUserId[Convert.ToInt32(response.Content.ToString().ToLower()) - 1];
                                    var otherUserData = JObject.Parse(File.ReadAllText(
                                    $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{selectionUserId}.json"
                                    ));
                                    if (((JObject)(otherUserData["trading_queue"])).ContainsKey(clientId.ToString()))
                                    {
                                        await ReplyAsync(embed: new EmbedBuilder()
                                            .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                                            .WithDescription($":x: Sorry, you cannot trade more than once with " +
                                            $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}.")
                                            .Build());
                                        await PagedReplyAsync(pagerUserList);
                                        response = await NextMessageAsync(timeout: timeoutDuration);
                                    }
                                    else
                                    {
                                        stepProcess = 2; newStep = true;
                                    }

                                }
                            }


                        }
                        else if (stepProcess == 2)
                        { //card pack & category selection from other user
                            var otherUserData = JObject.Parse(File.ReadAllText(
                                $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{selectionUserId}.json"
                                ));
                            List<string> listCardPackCategory = TradingCardCore.tradeListAllowed((JObject)otherUserData);

                            if (listCardPackCategory.Count <= 0)
                            {
                                await ReplyAsync(embed: new EmbedBuilder()
                                    .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                                    .WithColor(Config.Doremi.EmbedColor)
                                    .WithDescription($":x: Sorry, there are no cards that can be selected from " +
                                    $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}.\n" +
                                    $"Please select other users.")
                                    .Build());
                                stepProcess = 1; newStep = true;
                                response = await NextMessageAsync(timeout: timeoutDuration);
                            }
                            else
                            {
                                string textConcatDoremi = ""; string textConcatHazuki = ""; string textConcatAiko = "";
                                string textConcatOnpu = ""; string textConcatMomoko = "";
                                for (int i = 0; i < listCardPackCategory.Count; i++)
                                {
                                    if (listCardPackCategory[i].Contains("doremi"))
                                        textConcatDoremi += $"{listCardPackCategory[i]}\n";
                                    else if (listCardPackCategory[i].Contains("hazuki"))
                                        textConcatHazuki += $"{listCardPackCategory[i]}\n";
                                    else if (listCardPackCategory[i].Contains("aiko"))
                                        textConcatAiko += $"{listCardPackCategory[i]}\n";
                                    else if (listCardPackCategory[i].Contains("onpu"))
                                        textConcatOnpu += $"{listCardPackCategory[i]}\n";
                                    else if (listCardPackCategory[i].Contains("momoko"))
                                        textConcatMomoko += $"{listCardPackCategory[i]}\n";
                                }

                                if (textConcatDoremi == "") textConcatDoremi = "No card trade for this pack.";
                                if (textConcatHazuki == "") textConcatHazuki = "No card trade for this pack.";
                                if (textConcatAiko == "") textConcatAiko = "No card trade for this pack.";
                                if (textConcatOnpu == "") textConcatOnpu = "No card trade for this pack.";
                                if (textConcatMomoko == "") textConcatMomoko = "No card trade for this pack.";

                                if (newStep)
                                {
                                    newStep = false;
                                    await ReplyAsync(embed: new EmbedBuilder()
                                    .WithTitle("Step 2 - Card Pack & Category Selection")
                                    .WithDescription($"Type the **card pack & category** selection from " +
                                    $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}. Example: **doremi normal**.\n" +
                                    $"Type **back** to select other user.")
                                    .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                                    .WithColor(Config.Doremi.EmbedColor)
                                    .AddField("Doremi Card Pack", textConcatDoremi, true)
                                    .AddField("Hazuki Card Pack", textConcatHazuki, true)
                                    .AddField("Aiko Card Pack", textConcatAiko, true)
                                    .AddField("Onpu Card Pack", textConcatOnpu, true)
                                    .AddField("Momoko Card Pack", textConcatMomoko, true)
                                    .Build());
                                    response = await NextMessageAsync(timeout: timeoutDuration);
                                }
                                else
                                {
                                    if (response.Content.ToString().ToLower() == "back")
                                    {
                                        stepProcess = 1;
                                        newStep = true;
                                    }
                                    else if (!listCardPackCategory.Any(str => str.Contains(response.Content.ToString().ToLower())) ||
                                        !response.Content.ToString().ToLower().Contains(" "))
                                    {
                                        await ReplyAsync(":x: Please re-enter the proper card pack selection.",
                                        embed: new EmbedBuilder()
                                        .WithTitle("Step 2 - Card Pack & Category Selection")
                                        .WithDescription($"Type the card pack & category selection from " +
                                        $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}. Example: **doremi normal**.\n" +
                                        $"Type **back** to select other user.")
                                        .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                                        .WithColor(Config.Doremi.EmbedColor)
                                        .AddField("Doremi Card Pack", textConcatDoremi, true)
                                        .AddField("Hazuki Card Pack", textConcatHazuki, true)
                                        .AddField("Aiko Card Pack", textConcatAiko, true)
                                        .AddField("Onpu Card Pack", textConcatOnpu, true)
                                        .AddField("Momoko Card Pack", textConcatMomoko, true)
                                        .Build());
                                        response = await NextMessageAsync(timeout: timeoutDuration);
                                    }
                                    else
                                    {
                                        stepProcess = 3;
                                        string[] splittedChoice = response.Content.ToString().ToLower().Split(" ");
                                        selectionOtherUserCardPack = splittedChoice[0]; selectionOtherUserCardCategory = splittedChoice[1];
                                        newStep = true;
                                    }
                                }
                            }
                        }
                        else if (stepProcess == 3)
                        {
                            //select other user card id that you want to trade.
                            //your card id data
                            var yourData = JObject.Parse(File.ReadAllText(
                                $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{Convert.ToUInt64(clientId)}.json"
                                ));
                            var jYourData = (JArray)(yourData[selectionOtherUserCardPack][selectionOtherUserCardCategory]);
                            //other user card id data
                            var otherUserData = JObject.Parse(File.ReadAllText(
                                $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{selectionUserId}.json"
                                ));
                            var jOtherUserData = (JArray)(otherUserData[selectionOtherUserCardPack][selectionOtherUserCardCategory]);

                            //available list
                            var arrList = jOtherUserData;
                            var arrYourList = jYourData; int founded = 0;
                            //remove the card that you already have
                            for (int i = 0; i < arrList.Count; i++)
                            {
                                founded = 0;
                                for (int j = 0; j < arrYourList.Count; j++)
                                {
                                    if (arrList[i].ToString().ToLower() == arrYourList[j].ToString().ToLower())
                                    {
                                        founded = 1;
                                        j = arrYourList.Count;
                                    }
                                }
                                if (founded == 0)
                                    arrUserOtherCardList.Add(arrList[i].ToString());
                            }

                            pageContent = TradingCardCore.printTradeCardListTemplate(selectionOtherUserCardPack, selectionOtherUserCardCategory,
                                jObjTradingCardList, arrUserOtherCardList);
                            var pagerCardList = new PaginatedMessage
                            {
                                Pages = pageContent,
                                Color = Config.Doremi.EmbedColor,
                                Options = pao
                            };

                            //end available list

                            if (newStep)
                            {
                                newStep = false;

                                if (arrUserOtherCardList.Count >= 1)
                                {
                                    await ReplyAsync(embed: new EmbedBuilder()
                                    .WithTitle("Step 3 - Card Id Selection")
                                    .WithDescription($"Type the **card id** choice from " +
                                    $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}. Example: **do001**.\n" +
                                    $"Type **back** to select other card pack.")
                                    .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                                    .WithColor(Config.Doremi.EmbedColor)
                                    .Build());
                                    await PagedReplyAsync(pagerCardList);
                                    response = await NextMessageAsync(timeout: timeoutDuration);
                                }
                                else
                                {
                                    await ReplyAsync(embed: new EmbedBuilder()
                                        .WithTitle($"Step 3 - {GlobalFunctions.UppercaseFirst(selectionOtherUserCardPack)} {GlobalFunctions.UppercaseFirst(selectionOtherUserCardCategory)} Card Id Selection")
                                        .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                                        .WithDescription($":x: Sorry, there are no **{GlobalFunctions.UppercaseFirst(selectionOtherUserCardPack)} {GlobalFunctions.UppercaseFirst(selectionOtherUserCardCategory)}** card that you can choose from " +
                                        $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}.\n" +
                                        "Type **back** to select other card pack.")
                                        .WithColor(Config.Doremi.EmbedColor)
                                        .Build());
                                    response = await NextMessageAsync(timeout: timeoutDuration);
                                }
                            }
                            else
                            {
                                if (response.Content.ToString().ToLower() == "back")
                                {
                                    stepProcess = 2;
                                    newStep = true;
                                }
                                else if (arrUserOtherCardList.Count <= 0)
                                {
                                    await ReplyAsync(embed: new EmbedBuilder()
                                        .WithTitle("Step 3 - Card Id Selection")
                                        .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                                        .WithDescription($":x: Sorry, there are no **{GlobalFunctions.UppercaseFirst(selectionOtherUserCardPack)} {GlobalFunctions.UppercaseFirst(selectionOtherUserCardCategory)}** card that you can choose from " +
                                        $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}.\n" +
                                        "Type **back** to select other card pack.")
                                        .WithColor(Config.Doremi.EmbedColor)
                                        .Build());
                                    response = await NextMessageAsync(timeout: timeoutDuration);
                                }
                                else if (!arrUserOtherCardList.Contains(response.Content.ToString(), StringComparer.Ordinal))
                                {
                                    await ReplyAsync(":x: Please re-enter the correct **card id.**");
                                    await PagedReplyAsync(pagerCardList);
                                    response = await NextMessageAsync(timeout: timeoutDuration);
                                }
                                else
                                {
                                    stepProcess = 4; newStep = true;
                                    selectionOtherUserCardChoiceId = response.Content.ToString();
                                }
                            }
                        }
                        else if (stepProcess == 4)
                        {
                            //card pack & category selection from yours
                            var yourUserData = JObject.Parse(File.ReadAllText(
                                $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId.ToString()}.json"
                                ));
                            List<string> listCardPackCategory = TradingCardCore.tradeListAllowed((JObject)yourUserData);

                            if (listCardPackCategory.Count <= 0)
                            {
                                await ReplyAsync(embed: new EmbedBuilder()
                                    .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                                    .WithColor(Config.Doremi.EmbedColor)
                                    .WithTitle($"Step 4 - Select Your Available Card Pack")
                                    .WithDescription($"Sorry, there are no cards that you can trade. " +
                                    $"Your card trading process has been canceled.")
                                    .Build());
                                isTrading = false;
                                return;
                            }
                            else
                            {
                                string textConcatDoremi = ""; string textConcatHazuki = ""; string textConcatAiko = "";
                                string textConcatOnpu = ""; string textConcatMomoko = "";
                                for (int i = 0; i < listCardPackCategory.Count; i++)
                                {
                                    if (listCardPackCategory[i].Contains("doremi"))
                                        textConcatDoremi += $"{listCardPackCategory[i]}\n";
                                    else if (listCardPackCategory[i].Contains("hazuki"))
                                        textConcatHazuki += $"{listCardPackCategory[i]}\n";
                                    else if (listCardPackCategory[i].Contains("aiko"))
                                        textConcatAiko += $"{listCardPackCategory[i]}\n";
                                    else if (listCardPackCategory[i].Contains("onpu"))
                                        textConcatOnpu += $"{listCardPackCategory[i]}\n";
                                    else if (listCardPackCategory[i].Contains("momoko"))
                                        textConcatMomoko += $"{listCardPackCategory[i]}\n";
                                }

                                if (textConcatDoremi == "") textConcatDoremi = "No card trade for this pack.";
                                if (textConcatHazuki == "") textConcatHazuki = "No card trade for this pack.";
                                if (textConcatAiko == "") textConcatAiko = "No card trade for this pack.";
                                if (textConcatOnpu == "") textConcatOnpu = "No card trade for this pack.";
                                if (textConcatMomoko == "") textConcatMomoko = "No card trade for this pack.";

                                if (newStep)
                                {
                                    newStep = false;
                                    await ReplyAsync(embed: new EmbedBuilder()
                                    .WithTitle("Step 4 - Select Your Available Card Pack")
                                    .WithDescription($"Type the **card pack & category** selection from yours. Example: **doremi normal**.\n" +
                                    $"Type **back** to re-select other card from {MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}.")
                                    .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                                    .WithColor(Config.Doremi.EmbedColor)
                                    .AddField("Doremi Card Pack", textConcatDoremi, true)
                                    .AddField("Hazuki Card Pack", textConcatHazuki, true)
                                    .AddField("Aiko Card Pack", textConcatAiko, true)
                                    .AddField("Onpu Card Pack", textConcatOnpu, true)
                                    .AddField("Momoko Card Pack", textConcatMomoko, true)
                                    .Build());
                                    response = await NextMessageAsync(timeout: timeoutDuration);
                                }
                                else
                                {
                                    if (response.Content.ToString().ToLower() == "back")
                                    {
                                        stepProcess = 3;
                                        newStep = true;
                                    }
                                    else if (!listCardPackCategory.Contains(response.Content.ToString().ToLower(), StringComparer.OrdinalIgnoreCase) ||
                                      !response.Content.ToString().ToLower().Contains(" "))
                                    {
                                        await ReplyAsync(":x: Please re-enter the proper card pack selection.",
                                        embed: new EmbedBuilder()
                                        .WithTitle("Step 4 - Select Your Available Card Pack")
                                        .WithDescription($"Type the **card pack & category** selection from yours. Example: **doremi normal**.\n" +
                                        $"Type **back** to re-select other card from {MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}.")
                                        .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                                        .WithColor(Config.Doremi.EmbedColor)
                                        .AddField("Doremi Card Pack", textConcatDoremi, true)
                                        .AddField("Hazuki Card Pack", textConcatHazuki, true)
                                        .AddField("Aiko Card Pack", textConcatAiko, true)
                                        .AddField("Onpu Card Pack", textConcatOnpu, true)
                                        .AddField("Momoko Card Pack", textConcatMomoko, true)
                                        .Build());
                                        response = await NextMessageAsync(timeout: timeoutDuration);
                                    }
                                    else
                                    {
                                        stepProcess = 5;
                                        string[] splittedChoice = response.Content.ToString().ToLower().Split(" ");
                                        selectionYourCardPack = splittedChoice[0]; selectionYourCardCategory = splittedChoice[1];
                                        newStep = true;
                                    }
                                }
                            }

                        }
                        else if (stepProcess == 5)
                        {
                            //select other user card id that you want to trade.
                            //your card id data
                            var yourData = JObject.Parse(File.ReadAllText(
                                $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{Convert.ToUInt64(clientId)}.json"
                                ));
                            var jYourData = (JArray)(yourData[selectionYourCardPack][selectionYourCardCategory]);
                            //other user card id data
                            var otherUserData = JObject.Parse(File.ReadAllText(
                                $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{selectionUserId}.json"
                                ));
                            var jOtherUserData = (JArray)(otherUserData[selectionYourCardPack][selectionYourCardCategory]);

                            //available list
                            var arrList = jOtherUserData;
                            var arrYourList = jYourData; int founded = 0;
                            //remove the card that you already have
                            for (int i = 0; i < arrYourList.Count; i++)
                            {
                                founded = 0;
                                for (int j = 0; j < arrList.Count; j++)
                                {
                                    if (arrYourList[i].ToString().ToLower() == arrList[j].ToString().ToLower())
                                    {
                                        founded = 1;
                                        j = arrList.Count;
                                    }
                                }
                                if (founded == 0)
                                    arrUserCardList.Add(arrYourList[i].ToString());
                            }

                            pageContent = TradingCardCore.printTradeCardListTemplate(selectionYourCardPack, selectionYourCardCategory,
                                jObjTradingCardList, arrUserCardList);
                            var pagerYourCardList = new PaginatedMessage
                            {
                                Pages = pageContent,
                                Color = Config.Doremi.EmbedColor,
                                Options = pao
                            };

                            //end available list

                            if (newStep)
                            {
                                newStep = false;
                                if (arrUserCardList.Count <= 0)
                                {
                                    await ReplyAsync(embed: new EmbedBuilder()
                                    .WithTitle("Step 5 - Card Id Selection")
                                    .WithDescription($":x: Sorry, there are no **{GlobalFunctions.UppercaseFirst(selectionYourCardPack)} {GlobalFunctions.UppercaseFirst(selectionYourCardCategory)}** card that you can choose.\n" +
                                        "Type **back** to select other card pack.")
                                    .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                                    .WithColor(Config.Doremi.EmbedColor)
                                    .Build());
                                    response = await NextMessageAsync(timeout: timeoutDuration);
                                }
                                else
                                {
                                    await ReplyAsync(embed: new EmbedBuilder()
                                    .WithTitle($"Step 5 - {GlobalFunctions.UppercaseFirst(selectionYourCardPack)} {GlobalFunctions.UppercaseFirst(selectionYourCardCategory)} Card Id Selection")
                                    .WithDescription($"Type the **card id** selection from yours. Example: **do001**.\n" +
                                    $"Type **back** to select other card pack.")
                                    .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                                    .WithColor(Config.Doremi.EmbedColor)
                                    .Build());
                                    await PagedReplyAsync(pagerYourCardList);
                                    response = await NextMessageAsync(timeout: timeoutDuration);
                                }

                            }
                            else
                            {
                                if (response.Content.ToString().ToLower() == "back")
                                {
                                    stepProcess = 4;
                                    newStep = true;
                                }
                                else if (arrUserCardList.Count <= 0)
                                {
                                    await ReplyAsync(embed: new EmbedBuilder()
                                        .WithTitle("Step 5 - Card Id Selection")
                                        .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                                        .WithDescription($":x: Sorry, there are no **{GlobalFunctions.UppercaseFirst(selectionYourCardPack)} {GlobalFunctions.UppercaseFirst(selectionYourCardCategory)}** card that you can choose.\n" +
                                        "Type **back** to select other card pack.")
                                        .Build());
                                    response = await NextMessageAsync(timeout: timeoutDuration);
                                }
                                else if (!arrUserCardList.Contains(response.Content.ToString(), StringComparer.Ordinal))
                                {
                                    await ReplyAsync(":x: Please re-enter the correct **card id.**");
                                    await PagedReplyAsync(pagerYourCardList);
                                    response = await NextMessageAsync(timeout: timeoutDuration);
                                }
                                else
                                {
                                    stepProcess = 6; newStep = true;
                                    selectionYourCardChoiceId = response.Content.ToString();
                                }
                            }
                        }
                        else if (stepProcess == 6)
                        {
                            EmbedBuilder eb = new EmbedBuilder()
                            .WithTitle("Step 6 - Review Your Trade")
                            .WithDescription($"You will trade with {MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}. You can see your trade information review below.\n" +
                            $"Type **confirm** or **accept** to confirm the trade.\n" +
                            $"Type **back** to select other card pack.\n" +
                            $"Type **cancel** to cancel your trading process.")
                            .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                            //other user:
                            .AddField($"You will receive:",
                            $"-Card Pack: " +
                            $"{GlobalFunctions.UppercaseFirst(selectionOtherUserCardPack)} {GlobalFunctions.UppercaseFirst(selectionOtherUserCardCategory)}\n" +
                            $"-Card Name: " +
                            $"**[{selectionOtherUserCardChoiceId} - " +
                            $"{TradingCardCore.getCardProperty(selectionOtherUserCardPack, selectionOtherUserCardCategory, selectionOtherUserCardChoiceId, "name")}](" +
                            $"{TradingCardCore.getCardProperty(selectionOtherUserCardPack, selectionOtherUserCardCategory, selectionOtherUserCardChoiceId, "url")})**")
                            //yours
                            .AddField($"You will send:",
                            $"-Card Pack: " +
                            $"{GlobalFunctions.UppercaseFirst(selectionYourCardPack)} {GlobalFunctions.UppercaseFirst(selectionYourCardCategory)}\n" +
                            $"-Card Name: " +
                            $"**[{selectionYourCardChoiceId} - " +
                            $"{TradingCardCore.getCardProperty(selectionYourCardPack, selectionYourCardCategory, selectionYourCardChoiceId, "name")}](" +
                            $"{TradingCardCore.getCardProperty(selectionYourCardPack, selectionYourCardCategory, selectionYourCardChoiceId, "url")})**")
                            .WithColor(Config.Doremi.EmbedColor);

                            //review the trade
                            if (newStep)
                            {
                                newStep = false;
                                await ReplyAsync(embed: eb.Build());
                                response = await NextMessageAsync(timeout: timeoutDuration);
                            }
                            else
                            {
                                if (response.Content.ToString().ToLower() == "back")
                                {
                                    stepProcess = 5;
                                    newStep = true;
                                }
                                else if (response.Content.ToString().ToLower() != "accept" &&
                                  response.Content.ToString().ToLower() != "confirm")
                                {
                                    await ReplyAsync(":x: Please type with the valid choice: **accept/confirm**",
                                        embed: eb.Build());
                                    response = await NextMessageAsync(timeout: timeoutDuration);
                                }
                                else
                                {
                                    await ReplyAsync(embed: new EmbedBuilder()
                                        .WithTitle("✅ Trade Completed")
                                        .WithColor(Config.Doremi.EmbedColor)
                                        .WithDescription($"Your trade offer has been sent to " +
                                        $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}!\nThank you for using {TradingCardCore.Doremi.embedName}.")
                                        .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                                        .WithImageUrl(TradingCardCore.Doremi.emojiOk)
                                        .Build());

                                    await ReplyAsync($"You have a new card trade offer, {MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}.\n" +
                                        $"Please use **{Config.Doremi.PrefixParent[0]}card trade process** to process your trade offer.");

                                    //save to user
                                    string[] parameterNames = new string[] { selectionOtherUserCardChoiceId, selectionYourCardChoiceId };
                                    JArray jarrayObj = new JArray();
                                    foreach (string parameterName in parameterNames)
                                    {
                                        jarrayObj.Add(parameterName);
                                    }

                                    string otherUserDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{selectionUserId}.json";
                                    var JUserData = JObject.Parse(File.ReadAllText(otherUserDataDirectory));
                                    ((JObject)JUserData["trading_queue"]).Add(clientId.ToString(), new JArray(jarrayObj));


                                    //JArray item = (JArray)arrInventory[parent][spawnedCardCategory];
                                    //item.Add(spawnedCardId);
                                    File.WriteAllText(otherUserDataDirectory, JUserData.ToString());
                                    isTrading = false;
                                    return;
                                }
                            }
                        }

                        //What card pack do you want to trade? select with number/the name

                        //Please type the card id that you want to trade

                        //Please type the 
                        //
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

            }


            //you only allowed to trade for 2x each day
            /*json format:
             * "trading_queue": {
                "01929183481": ["do","on"]
            }
             */
        }

        [Command("trade process", RunMode = RunMode.Async), Summary("Trade one of your doremi trading card with other user.")]
        public async Task trading_card_queue_process()
        {
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;
            string userFolderDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}";
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";

            if (!File.Exists(playerDataDirectory)) //not registered yet
            {
                await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription($"I'm sorry, please register yourself first with **{Config.Doremi.PrefixParent[0]}card register** command.")
                .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build());
                return;
            }

            var jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));


            var playerData = JObject.Parse(File.ReadAllText(playerDataDirectory));
            int fileCount = Directory.GetFiles(userFolderDirectory).Length;

            if (fileCount <= 1)
            {
                await ReplyAsync("Sorry, server need to have more than 1 user that register the trading card data.");
                return;
            }
            else
            {
                try
                {
                    //start read all user id
                    List<string> arrUserId = new List<string>();
                    List<string> pageContent = new List<string>();
                    List<string> pageContentUserList = new List<string>();

                    string titleUserList = $"**Step 1 - Trade Process List Selection. Select with numbers.**\n";
                    string tempValUserList = titleUserList;

                    //list all users
                    var userList = (JObject)playerData["trading_queue"];

                    if (userList.Count <= 0)
                    {
                        await ReplyAsync(":x: There are no trade that you can process.");
                        return;
                    }

                    IList<JToken> objUserList = userList;
                    int currentIndex = 0;
                    for (int i = 0; i < userList.Count; i++)
                    {
                        var key = (JProperty)objUserList[i];

                        tempValUserList += $"**{i + 1}.** {MentionUtils.MentionUser(Convert.ToUInt64(key.Name))}\n";
                        arrUserId.Add(key.Name);

                        if (currentIndex < 14) currentIndex++;
                        else
                        {
                            pageContentUserList.Add(tempValUserList);
                            currentIndex = 0;
                            tempValUserList = titleUserList;
                        }

                        if (i == userList.Count - 1) pageContentUserList.Add(tempValUserList);

                    }

                    //selection variables
                    //other users
                    string selectionUserId = ""; string selectionOtherUserCardChoiceId = ""; string selectionYourCardChoiceId = "";
                    string selectionOtherUserCardPack = ""; string selectionOtherUserCardCategory = "";
                    string selectionYourCardPack = ""; string selectionYourCardCategory = "";

                    DirectoryInfo d = new DirectoryInfo(userFolderDirectory);//Assuming Test is your Folder
                    FileInfo[] Files = d.GetFiles("*.json"); //Getting Text files

                    //user selection
                    string titleUserSelection = $"**Step 1 - Select the user trade process with numbers**\n";
                    string tempVal = titleUserSelection;

                    Boolean isTrading = true;
                    var timeoutDuration = TimeSpan.FromSeconds(60);
                    string replyTimeout = ":stopwatch: I'm sorry, but you have reach your timeout. " +
                        "Please use the `card trade` command again to retry the trade process.";
                    int stepProcess = 1;//0/1:select the user,
                                        //2:review process
                    Boolean newStep = true;
                    //select user
                    await ReplyAsync(embed: new EmbedBuilder()
                        .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                        .WithDescription($"Welcome to {TradingCardCore.Doremi.embedName}. " +
                        $"Here you can process your trade offer that sent by someone. " +
                        $"You can type **cancel**/**exit** anytime to cancel the trade process.")
                        .WithColor(Config.Doremi.EmbedColor)
                        .Build());

                    //await PagedReplyAsync(pageContentUserList);
                    var response = await NextMessageAsync(timeout: timeoutDuration);
                    newStep = false;

                    while (isTrading)
                    {
                        try
                        {
                            var checkNull = response.Content.ToLower().ToString();
                        }
                        catch
                        {
                            await ReplyAsync(replyTimeout);
                            isTrading = false;
                            return;
                        }

                        if (response.Content.ToString().ToLower() == "cancel" ||
                            response.Content.ToString().ToLower() == "exit")
                        {
                            await ReplyAsync(embed: new EmbedBuilder()
                                .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                                .WithDescription($"You have cancel your trade process. Thank you for using the {TradingCardCore.Doremi.embedName}")
                                .WithColor(Config.Doremi.EmbedColor)
                                .Build());
                            isTrading = false;
                            return;
                        }
                        else if (stepProcess == 1)
                        {
                            //select user
                            var isNumeric = int.TryParse(response.Content.ToString().ToLower(), out int n);
                            if (newStep)
                            {
                                newStep = false;
                                //await PagedReplyAsync(pageContentUserList);
                                response = await NextMessageAsync(timeout: timeoutDuration);
                            }
                            else
                            {
                                if (!isNumeric)
                                {
                                    stepProcess = 1;
                                    selectionUserId = "";
                                    await ReplyAsync(":x: Please re-type the proper number selection.");
                                    //await PagedReplyAsync(pageContentUserList);
                                    response = await NextMessageAsync(timeout: timeoutDuration);
                                }
                                //array length:2[0,1], selected:2
                                else if (Convert.ToInt32(response.Content.ToLower().ToString()) <= 0 ||
                                    Convert.ToInt32(response.Content.ToString().ToLower()) > arrUserId.Count)
                                {
                                    stepProcess = 1;
                                    selectionUserId = "";
                                    await ReplyAsync(":x: That number choice is not on the list. Please re-type the proper number selection.");
                                    //await PagedReplyAsync(pageContentUserList);
                                    response = await NextMessageAsync(timeout: timeoutDuration);
                                }
                                else
                                {
                                    stepProcess = 2; newStep = true;
                                    selectionUserId = arrUserId[Convert.ToInt32(response.Content.ToString()) - 1];
                                    var selectedUserData = (JArray)userList[selectionUserId];
                                    //will be send
                                    selectionYourCardChoiceId = selectedUserData[0].ToString();
                                    selectionYourCardPack = TradingCardCore.getCardParent(selectionYourCardChoiceId);
                                    selectionYourCardCategory = TradingCardCore.getCardCategory(selectionYourCardChoiceId);
                                    //will be received
                                    selectionOtherUserCardChoiceId = selectedUserData[1].ToString();
                                    selectionOtherUserCardPack = TradingCardCore.getCardParent(selectionOtherUserCardChoiceId);
                                    selectionOtherUserCardCategory = TradingCardCore.getCardCategory(selectionOtherUserCardChoiceId);
                                }
                            }
                        }
                        else if (stepProcess == 2)
                        {
                            //check if your card/other user card still exists on the inventory or not
                            var JCheckUserData = JObject.Parse(File.ReadAllText(playerDataDirectory));
                            string otherUserDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/" +
                                $"{selectionUserId}.json";
                            var JCheckOtherUserData = JObject.Parse(File.ReadAllText(otherUserDataDirectory));

                            JArray notRequiredArrayYours = JArray.Parse(JCheckUserData[selectionYourCardPack][selectionYourCardCategory].ToString());
                            bool notExistsYours = notRequiredArrayYours.Any(t => t.Value<string>() == selectionYourCardChoiceId);//check yours have the cards in inventory
                            JArray requiredArrayYours = JArray.Parse(JCheckUserData[selectionOtherUserCardPack][selectionOtherUserCardCategory].ToString());
                            bool existsYours = requiredArrayYours.Any(t => t.Value<string>() == selectionOtherUserCardChoiceId);//check yours for duplicates

                            JArray notRequiredArrayOtherUser = JArray.Parse(JCheckOtherUserData[selectionOtherUserCardPack][selectionOtherUserCardCategory].ToString());
                            bool notExistsOtherUser = notRequiredArrayOtherUser.Any(t => t.Value<string>() == selectionOtherUserCardChoiceId);//check others have the cards in inventory
                            JArray requiredArrayOthers = JArray.Parse(JCheckOtherUserData[selectionYourCardPack][selectionYourCardCategory].ToString());
                            bool existsOthers = requiredArrayOthers.Any(t => t.Value<string>() == selectionYourCardChoiceId);//check others for duplicates

                            if (!notExistsYours || !notExistsOtherUser)
                            {//check if card still exists/not
                                await ReplyAsync(embed: new EmbedBuilder()
                                .WithTitle("🗑️ Trade Process Cancelled")
                                .WithColor(Config.Doremi.EmbedColor)
                                .WithDescription($"Your trade with " +
                                $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))} has been cancelled because one of you don't have the offered card anymore\n" +
                                $"Please use the **{Config.Doremi.PrefixParent[0]}card trade process** again to process other card offer.")
                                .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                                .Build());
                                isTrading = false;
                                //save the file
                                string yourUserDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
                                var JUserData = JObject.Parse(File.ReadAllText(yourUserDataDirectory));
                                ((JObject)JUserData["trading_queue"]).Remove(selectionUserId);
                                File.WriteAllText(yourUserDataDirectory, JUserData.ToString());
                                isTrading = false;
                                return;
                            }
                            else if (existsYours || existsOthers)
                            {//check for duplicates
                                await ReplyAsync(embed: new EmbedBuilder()
                                .WithTitle("🗑️ Trade Process Cancelled")
                                .WithColor(Config.Doremi.EmbedColor)
                                .WithDescription($"Your trade with " +
                                $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))} has been cancelled because one of you have the same card offer that being sent.\n" +
                                $"Please use the **{Config.Doremi.PrefixParent[0]}card trade process** again to process other card offer.")
                                .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                                .Build());
                                isTrading = false;
                                //save the file
                                string yourUserDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
                                var JUserData = JObject.Parse(File.ReadAllText(yourUserDataDirectory));
                                ((JObject)JUserData["trading_queue"]).Remove(selectionUserId);
                                File.WriteAllText(yourUserDataDirectory, JUserData.ToString());
                                isTrading = false;
                                return;
                            }

                            EmbedBuilder eb = new EmbedBuilder()
                            .WithTitle("Step 2 - Review Your Trade")
                            .WithDescription($"You can see your trade information review below from {MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}.\n" +
                            $"Type **confirm** or **accept** to confirm the trade.\n" +
                            $"Type **reject** to reject the trade.\n" +
                            $"Type **back** to select other card pack.\n" +
                            $"Type **cancel** to cancel your trading process.")
                            .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                            //other user:
                            .AddField($"You will receive:",
                            $"-Card Pack: " +
                            $"{GlobalFunctions.UppercaseFirst(selectionOtherUserCardPack)} {GlobalFunctions.UppercaseFirst(selectionOtherUserCardCategory)}\n" +
                            $"-Card Name: " +
                            $"**[{selectionOtherUserCardChoiceId} - " +
                            $"{TradingCardCore.getCardProperty(selectionOtherUserCardPack, selectionOtherUserCardCategory, selectionOtherUserCardChoiceId, "name")}](" +
                            $"{TradingCardCore.getCardProperty(selectionOtherUserCardPack, selectionOtherUserCardCategory, selectionOtherUserCardChoiceId, "url")})**")
                            //yours
                            .AddField($"You will send:",
                            $"-Card Pack: " +
                            $"{GlobalFunctions.UppercaseFirst(selectionYourCardPack)} {GlobalFunctions.UppercaseFirst(selectionYourCardCategory)}\n" +
                            $"-Card Name: " +
                            $"**[{selectionYourCardChoiceId} - " +
                            $"{TradingCardCore.getCardProperty(selectionYourCardPack, selectionYourCardCategory, selectionYourCardChoiceId, "name")}](" +
                            $"{TradingCardCore.getCardProperty(selectionYourCardPack, selectionYourCardCategory, selectionYourCardChoiceId, "url")})**")
                            .WithColor(Config.Doremi.EmbedColor);

                            //review the trade
                            if (newStep)
                            {
                                newStep = false;
                                await ReplyAsync(embed: eb.Build());
                                response = await NextMessageAsync(timeout: timeoutDuration);
                            }
                            else
                            {
                                if (response.Content.ToString().ToLower() == "back")
                                {
                                    stepProcess = 1;
                                    newStep = true;
                                }
                                else if (response.Content.ToString().ToLower() != "accept" &&
                                  response.Content.ToString().ToLower() != "confirm" &&
                                  response.Content.ToString().ToLower() != "reject")
                                {
                                    await ReplyAsync(":x: Please type with the valid choice: **accept/confirm/reject**.",
                                        embed: eb.Build());
                                    response = await NextMessageAsync(timeout: timeoutDuration);
                                }
                                else if (response.Content.ToString().ToLower() == "reject")
                                {
                                    await ReplyAsync(embed: new EmbedBuilder()
                                        .WithTitle("🗑️ Trade Process Rejected")
                                        .WithColor(Config.Doremi.EmbedColor)
                                        .WithDescription($"You have reject the trade offer from " +
                                        $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}.\nThank you for using {TradingCardCore.Doremi.embedName}.")
                                        .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                                        .Build());

                                    //save the file


                                    string yourUserDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
                                    var JUserData = JObject.Parse(File.ReadAllText(yourUserDataDirectory));
                                    ((JObject)JUserData["trading_queue"]).Remove(selectionUserId);
                                    File.WriteAllText(yourUserDataDirectory, JUserData.ToString());
                                    isTrading = false;
                                    return;
                                }
                                else
                                {
                                    await ReplyAsync(embed: new EmbedBuilder()
                                        .WithTitle("✅ Trade Process Completed")
                                        .WithColor(Config.Doremi.EmbedColor)
                                        .WithDescription($"You have successfully accepted the trade offer from " +
                                        $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}.\nThank you for using {TradingCardCore.Doremi.embedName}.")
                                        .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                                        .WithImageUrl(TradingCardCore.Doremi.emojiOk)
                                        .Build());

                                    //save to yours
                                    string yourUserDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
                                    var JYourData = JObject.Parse(File.ReadAllText(yourUserDataDirectory));
                                    //remove from yours
                                    JArray arrInventoryYoursRemove = JArray.Parse(JYourData[selectionYourCardPack][selectionYourCardCategory].ToString());
                                    for (int i = 0; i < arrInventoryYoursRemove.Count; i++)
                                    {
                                        if (arrInventoryYoursRemove[i].ToString() == selectionYourCardChoiceId)
                                            arrInventoryYoursRemove[i].Remove();
                                    }
                                    JYourData[selectionYourCardPack][selectionYourCardCategory] = arrInventoryYoursRemove;

                                    //add to yours
                                    JArray arrInventoryYoursAdd = JArray.Parse(JYourData[selectionOtherUserCardPack][selectionOtherUserCardCategory].ToString());
                                    arrInventoryYoursAdd.Add(selectionOtherUserCardChoiceId);
                                    JYourData[selectionOtherUserCardPack][selectionOtherUserCardCategory] = arrInventoryYoursAdd;
                                    //remove trading_queue
                                    ((JObject)JYourData["trading_queue"]).Remove(selectionUserId);
                                    File.WriteAllText(yourUserDataDirectory, JYourData.ToString());
                                    //==================================================================
                                    //save to other users
                                    var JOtherUserData = JObject.Parse(File.ReadAllText(otherUserDataDirectory));
                                    ((JObject)JOtherUserData["trading_queue"]).Remove(selectionUserId);
                                    //remove from others
                                    JArray arrInventoryOthersRemove = JArray.Parse(JOtherUserData[selectionOtherUserCardPack][selectionOtherUserCardCategory].ToString());
                                    for (int i = 0; i < arrInventoryOthersRemove.Count; i++)
                                    {
                                        if (arrInventoryOthersRemove[i].ToString() == selectionOtherUserCardChoiceId)
                                            arrInventoryOthersRemove[i].Remove();
                                    }
                                    JOtherUserData[selectionOtherUserCardPack][selectionOtherUserCardCategory] = arrInventoryOthersRemove;

                                    //add to others
                                    JArray arrInventoryOthersAdd = JArray.Parse(JOtherUserData[selectionYourCardPack][selectionYourCardCategory].ToString());
                                    arrInventoryOthersAdd.Add(selectionYourCardChoiceId);
                                    JOtherUserData[selectionYourCardPack][selectionYourCardCategory] = arrInventoryOthersAdd;
                                    File.WriteAllText(otherUserDataDirectory, JOtherUserData.ToString());

                                    isTrading = false;
                                    return;

                                }
                            }

                        }

                    }


                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        [Command("shop", RunMode = RunMode.Async), Summary("Open Doremi Card Shop Menu.")]
        public async Task openShop()
        {
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;
            string userFolderDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}";
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";

            if (!File.Exists(playerDataDirectory)) //not registered yet
            {
                await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription($":x: I'm sorry, please register yourself first with **{Config.Doremi.PrefixParent[0]}card register** command.")
                .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build());
                return;
            }

            JObject arrInventory = JObject.Parse(File.ReadAllText(playerDataDirectory));
            var playerData = JObject.Parse(File.ReadAllText(playerDataDirectory));
            string itemsListText = "1. Parara tap - 10 seeds\n" +
                "2. Peperuto Pollon - 3 seeds\n" +
                "3. Puwapuwa Pollon - 3 seeds\n" +
                "4. Poppun Pollon - 3 seeds\n" +
                "5. Apprentice Tap - 10 seeds\n" +
                "6. Rhythm Tap - 15 seeds\n" +
                "7. Kururu Pollon - 25 seeds\n" +
                "8. Picotto Pollon - 30 seeds\n" +
                "9. Patraine Call - 30 seeds\n" +
                "10. Wreath Pollon - 35 seeds\n" +
                "11. Jewelry Pollon - 45 seeds";
            Boolean isShopping = true; int stepProcess = 1;
            int selectionItem = 0; int priceConfirmation = 0;
            int magicSeeds = Convert.ToInt32(playerData["magic_seeds"].ToString());

            var timeoutDuration = TimeSpan.FromSeconds(60);
            string concatResponseSuccess = "";

            await ReplyAsync(embed: new EmbedBuilder()
                .WithAuthor("Doremi Card Shop", Config.Doremi.EmbedAvatarUrl)
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription("Welcome to Doremi Card Shop. Here you can purchase some items to help your card collecting progression.\n" +
                "Type **exit** or **cancel** anytime to close the shop menu.\n" +
                "Select with numbers from these list to browse & purchase:")
                .AddField("Item List", itemsListText)
                .Build());

            var response = await NextMessageAsync(timeout: timeoutDuration);
            string replyTimeout = ":stopwatch: I'm sorry, you're not giving valid selection yet. " +
            $"Please use the `{Config.Doremi.PrefixParent[0]}card shop` command to open shop menu again.";

            while (isShopping)
            {
                try
                {
                    var checkNull = response.Content.ToLower().ToString();
                }
                catch
                {
                    await ReplyAsync(replyTimeout);
                    isShopping = false;
                    return;
                }

                if (response.Content.ToString().ToLower() == "cancel" || response.Content.ToString().ToLower() == "exit")
                {
                    await ReplyAsync(embed: new EmbedBuilder()
                        .WithAuthor("Doremi Card Shop", Config.Doremi.EmbedAvatarUrl)
                        .WithDescription($"Thank you for visiting the Doremi Card Shop.")
                        .WithColor(Config.Doremi.EmbedColor)
                        .Build());
                    isShopping = false;
                    return;
                }

                if (stepProcess == 1)
                {
                    var isNumeric = int.TryParse(response.Content.ToString().ToLower(), out int n);
                    if (!isNumeric || Convert.ToInt32(response.Content.ToString()) <= 0 ||
                        Convert.ToInt32(response.Content.ToString()) >= 12)
                    {
                        await ReplyAsync(embed: new EmbedBuilder()
                        .WithColor(Config.Doremi.EmbedColor)
                        .WithAuthor("Doremi Card Shop", Config.Doremi.EmbedAvatarUrl)
                        .WithDescription(":x: Please re-select with valid number selection from these list to browse & purchase.\n" +
                        "Type **exit** or **cancel** to close the shop menu.")
                        .AddField("Item List", itemsListText)
                        .Build());
                        response = await NextMessageAsync(timeout: timeoutDuration);
                    }
                    else
                    {
                        selectionItem = Convert.ToInt32(response.Content.ToString());
                        stepProcess = 2;
                    }
                }
                else if (stepProcess == 2)
                {
                    if (selectionItem == 1)
                    {
                        priceConfirmation = 10;
                        await ReplyAsync("Type **confirm** to proceed with the purchase. Type **back** to go back to previous menu.",
                        embed: new EmbedBuilder()
                        .WithColor(Config.Doremi.EmbedColor)
                        .WithAuthor("Doremi Card Shop", Config.Doremi.EmbedAvatarUrl)
                        .WithTitle("Parara Tap Card")
                        .WithDescription("This card will give you another chance to catch card again.")
                        .AddField("Price:", priceConfirmation, true)
                        .AddField("Your Magic Seeds:", magicSeeds, true)
                        .WithThumbnailUrl("https://cdn.discordapp.com/attachments/708332290446721066/709045172461240330/pararatap.jpg")
                        .Build());
                    }
                    else if (selectionItem == 2)
                    {
                        priceConfirmation = 3;
                        await ReplyAsync("**:exclamation: Please note that purchasing this card will replace all status boost!**\n" +
                            "Type **confirm** to proceed with the purchase. Type **back** to go back to previous menu.",
                        embed: new EmbedBuilder()
                        .WithColor(Config.Doremi.EmbedColor)
                        .WithAuthor("Doremi Card Shop", Config.Doremi.EmbedAvatarUrl)
                        .WithTitle("Peperuto Pollon Card")
                        .WithDescription("This will give card boost for doremi card pack. Can only be used once.")
                        .AddField("Price:", priceConfirmation, true)
                        .AddField("Your Magic Seeds:", magicSeeds, true)
                        .AddField("Doremi Capture Rate Boost:", "Normal: 100%")
                        .WithThumbnailUrl("https://cdn.discordapp.com/attachments/708332290446721066/708332323489316964/poron1.jpg")
                        .Build());
                    }
                    else if (selectionItem == 3)
                    {
                        priceConfirmation = 3;
                        await ReplyAsync("**:exclamation: Please note that purchasing this card will replace all status boost!**\n" +
                            "Type **confirm** to proceed with the purchase. Type **back** to go back to previous menu.",
                        embed: new EmbedBuilder()
                        .WithColor(Config.Doremi.EmbedColor)
                        .WithAuthor("Doremi Card Shop", Config.Doremi.EmbedAvatarUrl)
                        .WithTitle("Puwapuwa Pollon Card")
                        .WithDescription("This will give card boost for hazuki card pack. Can only be used once.")
                        .AddField("Price:", priceConfirmation, true)
                        .AddField("Your Magic Seeds:", magicSeeds, true)
                        .AddField("Hazuki Capture Rate Boost:", "Normal: 100%")
                        .WithThumbnailUrl("https://cdn.discordapp.com/attachments/708332290446721066/708683707585790072/poronhazuki.jpg")
                        .Build());
                    }
                    else if (selectionItem == 4)
                    {
                        priceConfirmation = 3;
                        await ReplyAsync("**:exclamation: Please note that purchasing this card will replace all status boost!**\n" +
                            "Type **confirm** to proceed with the purchase. Type **back** to go back to previous menu.",
                        embed: new EmbedBuilder()
                        .WithColor(Config.Doremi.EmbedColor)
                        .WithAuthor("Doremi Card Shop", Config.Doremi.EmbedAvatarUrl)
                        .WithTitle("Poppun Pollon Card")
                        .WithDescription("This will give card boost for aiko card pack. Can only be used once.")
                        .AddField("Price:", priceConfirmation, true)
                        .AddField("Your Magic Seeds:", magicSeeds, true)
                        .AddField("Aiko Capture Rate Boost:", "Normal: 100%")
                        .WithThumbnailUrl("https://cdn.discordapp.com/attachments/708332290446721066/708332376505188502/poronaiko.jpg")
                        .Build());
                    }
                    else if (selectionItem == 5)
                    {
                        priceConfirmation = 10;
                        await ReplyAsync("**:exclamation: Please note that purchasing this card will replace all status boost!**\n" +
                            "Type **confirm** to proceed with the purchase. Type **back** to go back to previous menu.",
                        embed: new EmbedBuilder()
                        .WithColor(Config.Doremi.EmbedColor)
                        .WithAuthor("Doremi Card Shop", Config.Doremi.EmbedAvatarUrl)
                        .WithTitle("Apprentice Tap Pack Card")
                        .WithDescription("This will give card boost **each once** for doremi, hazuki, aiko, onpu, momoko and other card pack. " +
                        "Can only be used once with **each** card pack.")
                        .AddField("Price:", priceConfirmation, true)
                        .AddField("Your Magic Seeds:", magicSeeds, true)
                        .AddField("Doremi Capture Rate Boost:", "Normal: 100%\nPlatinum:60%\nMetal:50%\nOjamajos:30%")
                        .AddField("Hazuki Capture Rate Boost:", "Normal: 100%\nPlatinum:60%\nMetal:50%\nOjamajos:30%", true)
                        .AddField("Aiko Capture Rate Boost:", "Normal: 100%\nPlatinum:60%\nMetal:50%\nOjamajos:30%", true)
                        .AddField("Onpu Capture Rate Boost:", "Normal: 100%\nPlatinum:60%\nMetal:50%\nOjamajos:30%", true)
                        .AddField("Momoko Capture Rate Boost:", "Normal: 100%\nPlatinum:60%\nMetal:50%\nOjamajos:30%", true)
                        .AddField("Other Capture Rate Boost:", "Special: 60%", true)
                        .WithThumbnailUrl("https://cdn.discordapp.com/attachments/708332290446721066/708332389591679017/tap1.jpg")
                        .Build());
                    }
                    else if (selectionItem == 6)
                    {
                        priceConfirmation = 15;
                        await ReplyAsync("**:exclamation: Please note that purchasing this card will replace all status boost!**\n" +
                            "Type **confirm** to proceed with the purchase. Type **back** to go back to previous menu.",
                        embed: new EmbedBuilder()
                        .WithColor(Config.Doremi.EmbedColor)
                        .WithAuthor("Doremi Card Shop", Config.Doremi.EmbedAvatarUrl)
                        .WithTitle("Rythm Tap Pack Card")
                        .WithDescription("This will give card boost **each once** for doremi, hazuki, aiko, onpu, and other card pack. " +
                        "Can only be used once with **each** card pack.")
                        .AddField("Price:", priceConfirmation, true)
                        .AddField("Your Magic Seeds:", magicSeeds, true)
                        .AddField("Doremi Capture Rate Boost:", "Normal: 100%\nPlatinum:70%\nMetal:60%\nOjamajos:40%")
                        .AddField("Hazuki Capture Rate Boost:", "Normal: 100%\nPlatinum:70%\nMetal:60%\nOjamajos:40%", true)
                        .AddField("Aiko Capture Rate Boost:", "Normal: 100%\nPlatinum:70%\nMetal:60%\nOjamajos:40%", true)
                        .AddField("Onpu Capture Rate Boost:", "Normal: 100%\nPlatinum:70%\nMetal:60%\nOjamajos:40%", true)
                        .AddField("Other Capture Rate Boost:", "Special: 70%", true)
                        .WithThumbnailUrl("https://cdn.discordapp.com/attachments/708332290446721066/708332399003697212/tap2.jpg")
                        .Build());
                    }
                    else if (selectionItem == 7)
                    {
                        priceConfirmation = 25;
                        await ReplyAsync("**:exclamation: Please note that purchasing this card will replace all status boost!**\n" +
                            "Type **confirm** to proceed with the purchase. Type **back** to go back to previous menu.",
                        embed: new EmbedBuilder()
                        .WithColor(Config.Doremi.EmbedColor)
                        .WithAuthor("Doremi Card Shop", Config.Doremi.EmbedAvatarUrl)
                        .WithTitle("Kururu Pollon Pack Card")
                        .WithDescription("This will give card boost **each once** for doremi, hazuki, aiko, onpu, momoko and other card pack. " +
                        "Can only be used once with **each** card pack.")
                        .AddField("Price:", priceConfirmation, true)
                        .AddField("Your Magic Seeds:", magicSeeds, true)
                        .AddField("Doremi Capture Rate Boost:", "Normal: 100%\nPlatinum:70%\nMetal:70%\nOjamajos:50%")
                        .AddField("Hazuki Capture Rate Boost:", "Normal: 100%\nPlatinum:70%\nMetal:70%\nOjamajos:50%", true)
                        .AddField("Aiko Capture Rate Boost:", "Normal: 100%\nPlatinum:70%\nMetal:70%\nOjamajos:50%", true)
                        .AddField("Onpu Capture Rate Boost:", "Normal: 100%\nPlatinum:70%\nMetal:70%\nOjamajos:50%", true)
                        .AddField("Momoko Capture Rate Boost:", "Normal: 100%\nPlatinum:70%\nMetal:70%\nOjamajos:50%", true)
                        .AddField("Other Capture Rate Boost:", "Special: 70%", true)
                        .WithThumbnailUrl("https://cdn.discordapp.com/attachments/708332290446721066/708332357652054066/poron5.jpg")
                        .Build());
                    }
                    else if (selectionItem == 8)
                    {
                        priceConfirmation = 30;
                        await ReplyAsync("**:exclamation: Please note that purchasing this card will replace all status boost!**\n" +
                            "Type **confirm** to proceed with the purchase. Type **back** to go back to previous menu.",
                        embed: new EmbedBuilder()
                        .WithColor(Config.Doremi.EmbedColor)
                        .WithAuthor("Doremi Card Shop", Config.Doremi.EmbedAvatarUrl)
                        .WithTitle("Picotto Pollon Pack Card")
                        .WithDescription("This will give card boost **each once** for doremi, hazuki, aiko, onpu, and other card pack. " +
                        "Can only be used once with **each** card pack.")
                        .AddField("Price:", priceConfirmation, true)
                        .AddField("Your Magic Seeds:", magicSeeds, true)
                        .AddField("Doremi Capture Rate Boost:", "Normal: 100%\nPlatinum:70%\nMetal:80%\nOjamajos:60%")
                        .AddField("Hazuki Capture Rate Boost:", "Normal: 100%\nPlatinum:70%\nMetal:80%\nOjamajos:60%", true)
                        .AddField("Aiko Capture Rate Boost:", "Normal: 100%\nPlatinum:70%\nMetal:80%\nOjamajos:60%", true)
                        .AddField("Onpu Capture Rate Boost:", "Normal: 100%\nPlatinum:70%\nMetal:80%\nOjamajos:60%", true)
                        .AddField("Other Capture Rate Boost:", "Special: 70%", true)
                        .WithThumbnailUrl("https://cdn.discordapp.com/attachments/708332290446721066/709768272966320199/picotto.jpg")
                        .Build());
                    }
                    else if (selectionItem == 9)
                    {
                        priceConfirmation = 30;
                        await ReplyAsync("**:exclamation: Please note that purchasing this card will replace all status boost!**\n" +
                            "Type **confirm** to proceed with the purchase. Type **back** to go back to previous menu.",
                        embed: new EmbedBuilder()
                        .WithColor(Config.Doremi.EmbedColor)
                        .WithAuthor("Doremi Card Shop", Config.Doremi.EmbedAvatarUrl)
                        .WithTitle("Patraine Call Pack Card")
                        .WithDescription("This will give card boost **each once** for doremi, hazuki, aiko, onpu, and other card pack. " +
                        "Can only be used once with **each** card pack.")
                        .AddField("Price:", priceConfirmation, true)
                        .AddField("Your Magic Seeds:", magicSeeds, true)
                        .AddField("Doremi Capture Rate Boost:", "Normal: 100%\nPlatinum:80%\nMetal:70%\nOjamajos:70%")
                        .AddField("Hazuki Capture Rate Boost:", "Normal: 100%\nPlatinum:80%\nMetal:70%\nOjamajos:70%", true)
                        .AddField("Aiko Capture Rate Boost:", "Normal: 100%\nPlatinum:80%\nMetal:70%\nOjamajos:70%", true)
                        .AddField("Onpu Capture Rate Boost:", "Normal: 100%\nPlatinum:80%\nMetal:70%\nOjamajos:70%", true)
                        .AddField("Other Capture Rate Boost:", "Special: 60%", true)
                        .WithThumbnailUrl("https://cdn.discordapp.com/attachments/708332290446721066/708332428929794140/patraine_call.jpg")
                        .Build());
                    }
                    else if (selectionItem == 10)
                    {
                        priceConfirmation = 35;
                        await ReplyAsync("**:exclamation: Please note that purchasing this card will replace all status boost!**\n" +
                            "Type **confirm** to proceed with the purchase. Type **back** to go back to previous menu.",
                        embed: new EmbedBuilder()
                        .WithColor(Config.Doremi.EmbedColor)
                        .WithAuthor("Doremi Card Shop", Config.Doremi.EmbedAvatarUrl)
                        .WithTitle("Wreath Pollon Pack Card")
                        .WithDescription("This will give card boost **each once** for doremi, hazuki, aiko, onpu, and other card pack. " +
                        "Can only be used once with **each** card pack.")
                        .AddField("Price:", priceConfirmation, true)
                        .AddField("Your Magic Seeds:", magicSeeds, true)
                        .AddField("Doremi Capture Rate Boost:", "Normal: 100%\nPlatinum:90%\nMetal:70%\nOjamajos:80%")
                        .AddField("Hazuki Capture Rate Boost:", "Normal: 100%\nPlatinum:90%\nMetal:70%\nOjamajos:80%", true)
                        .AddField("Aiko Capture Rate Boost:", "Normal: 100%\nPlatinum:90%\nMetal:70%\nOjamajos:80%", true)
                        .AddField("Onpu Capture Rate Boost:", "Normal: 100%\nPlatinum:90%\nMetal:70%\nOjamajos:80%", true)
                        .AddField("Other Capture Rate Boost:", "Special: 80%", true)
                        .WithThumbnailUrl("https://cdn.discordapp.com/attachments/708332290446721066/708332406867755008/wreath.jpg")
                        .Build());
                    }
                    else if (selectionItem == 11)
                    {
                        priceConfirmation = 45;
                        await ReplyAsync("**:exclamation: Please note that purchasing this card will replace all status boost!**\n" +
                            "Type **confirm** to proceed with the purchase. Type **back** to go back to previous menu.",
                        embed: new EmbedBuilder()
                        .WithColor(Config.Doremi.EmbedColor)
                        .WithAuthor("Doremi Card Shop", Config.Doremi.EmbedAvatarUrl)
                        .WithTitle("Patraine Call Pack Card")
                        .WithDescription("This will give card boost **each once** for doremi, hazuki, aiko, onpu, momoko and other card pack. " +
                        "Can only be used once with **each** card pack.")
                        .AddField("Price:", priceConfirmation, true)
                        .AddField("Your Magic Seeds:", magicSeeds, true)
                        .AddField("Doremi Capture Rate Boost:", "Normal: 100%\nPlatinum:90%\nMetal:80%\nOjamajos:80%")
                        .AddField("Hazuki Capture Rate Boost:", "Normal: 100%\nPlatinum:90%\nMetal:80%\nOjamajos:80%", true)
                        .AddField("Aiko Capture Rate Boost:", "Normal: 100%\nPlatinum:90%\nMetal:80%\nOjamajos:80%", true)
                        .AddField("Onpu Capture Rate Boost:", "Normal: 100%\nPlatinum:90%\nMetal:80%\nOjamajos:80%", true)
                        .AddField("Momoko Capture Rate Boost:", "Normal: 100%\nPlatinum:90%\nMetal:80%\nOjamajos:80%", true)
                        .AddField("Other Capture Rate Boost:", "Special: 80%", true)
                        .WithThumbnailUrl("https://cdn.discordapp.com/attachments/708332290446721066/709034102288023613/poronjewelry.jpg")
                        .Build());
                    }

                    stepProcess = 3;
                    response = await NextMessageAsync(timeout: timeoutDuration);

                }
                else if (stepProcess == 3)
                {
                    if (response.Content.ToString().ToLower() != "confirm" &&
                        response.Content.ToString().ToLower() != "back")
                    {
                        await ReplyAsync("Sorry, that is not the valid **confirm/back** choices.");
                        stepProcess = 2;
                    }
                    else if (response.Content.ToString().ToLower() == "back")
                    {
                        stepProcess = 1;
                    }
                    else if (response.Content.ToString().ToLower() == "confirm")
                    {
                        stepProcess = 4;
                    }

                }
                else if (stepProcess == 4)
                {
                    //JArray item = (JArray)arrInventory[parent][spawnedCardCategory];
                    //item.Add(spawnedCardId);

                    if (magicSeeds >= priceConfirmation)
                    {
                        if (selectionItem >= 2)//reset all status boost
                        {
                            arrInventory["boost"]["doremi"]["normal"] = 0;
                            arrInventory["boost"]["doremi"]["platinum"] = 0;
                            arrInventory["boost"]["doremi"]["metal"] = 0;
                            arrInventory["boost"]["doremi"]["ojamajos"] = 0;

                            arrInventory["boost"]["hazuki"]["normal"] = 0;
                            arrInventory["boost"]["hazuki"]["platinum"] = 0;
                            arrInventory["boost"]["hazuki"]["metal"] = 0;
                            arrInventory["boost"]["hazuki"]["ojamajos"] = 0;

                            arrInventory["boost"]["aiko"]["normal"] = 0;
                            arrInventory["boost"]["aiko"]["platinum"] = 0;
                            arrInventory["boost"]["aiko"]["metal"] = 0;
                            arrInventory["boost"]["aiko"]["ojamajos"] = 0;

                            arrInventory["boost"]["onpu"]["normal"] = 0;
                            arrInventory["boost"]["onpu"]["platinum"] = 0;
                            arrInventory["boost"]["onpu"]["metal"] = 0;
                            arrInventory["boost"]["onpu"]["ojamajos"] = 0;

                            arrInventory["boost"]["momoko"]["normal"] = 0;
                            arrInventory["boost"]["momoko"]["platinum"] = 0;
                            arrInventory["boost"]["momoko"]["metal"] = 0;
                            arrInventory["boost"]["momoko"]["ojamajos"] = 0;

                            arrInventory["boost"]["other"]["special"] = 0;
                        }

                        if (selectionItem == 1)
                        {
                            arrInventory["catch_token"] = "";
                            File.WriteAllText(playerDataDirectory, arrInventory.ToString());
                            concatResponseSuccess = ":sparkles: You got 1 more catching attempt!";
                        } 
                        else if(selectionItem == 2)
                        {
                            arrInventory["boost"]["doremi"]["normal"] = 10;
                            concatResponseSuccess = ":sparkles: You received **Peperuto Pollon Card** Boost! Activate it on card spawn with **<bot>!card capture boost**";
                        } 
                        else if (selectionItem == 3)
                        {
                            arrInventory["boost"]["hazuki"]["normal"] = 10;
                            concatResponseSuccess = ":sparkles: You received **Puwapuwa Pollon Card** Boost! Activate it on card spawn with **<bot>!card capture boost**";
                        }
                        else if (selectionItem == 4)
                        {
                            arrInventory["boost"]["aiko"]["normal"] = 10;
                            concatResponseSuccess = ":sparkles: You received **Poppun Pollon Card** Boost! Activate it on card spawn with **<bot>!card capture boost**";
                        }
                        else if (selectionItem == 5)
                        {
                            arrInventory["boost"]["doremi"]["normal"] = 10;
                            arrInventory["boost"]["doremi"]["platinum"] = 6;
                            arrInventory["boost"]["doremi"]["metal"] = 5;
                            arrInventory["boost"]["doremi"]["ojamajos"] = 3;

                            arrInventory["boost"]["hazuki"]["normal"] = 10;
                            arrInventory["boost"]["hazuki"]["platinum"] = 6;
                            arrInventory["boost"]["hazuki"]["metal"] = 5;
                            arrInventory["boost"]["hazuki"]["ojamajos"] = 3;

                            arrInventory["boost"]["aiko"]["normal"] = 10;
                            arrInventory["boost"]["aiko"]["platinum"] = 6;
                            arrInventory["boost"]["aiko"]["metal"] = 5;
                            arrInventory["boost"]["aiko"]["ojamajos"] = 3;

                            arrInventory["boost"]["onpu"]["normal"] = 10;
                            arrInventory["boost"]["onpu"]["platinum"] = 6;
                            arrInventory["boost"]["onpu"]["metal"] = 5;
                            arrInventory["boost"]["onpu"]["ojamajos"] = 3;

                            arrInventory["boost"]["momoko"]["normal"] = 10;
                            arrInventory["boost"]["momoko"]["platinum"] = 6;
                            arrInventory["boost"]["momoko"]["metal"] = 5;
                            arrInventory["boost"]["momoko"]["ojamajos"] = 3;

                            arrInventory["boost"]["other"]["special"] = 6;
                            concatResponseSuccess = ":sparkles: You received **Apprentice Tap Card** Boost! Activate it on card spawn with **<bot>!card capture boost**";
                        }
                        else if (selectionItem == 6)
                        {
                            arrInventory["boost"]["doremi"]["normal"] = 10;
                            arrInventory["boost"]["doremi"]["platinum"] = 7;
                            arrInventory["boost"]["doremi"]["metal"] = 6;
                            arrInventory["boost"]["doremi"]["ojamajos"] = 4;

                            arrInventory["boost"]["hazuki"]["normal"] = 10;
                            arrInventory["boost"]["hazuki"]["platinum"] = 7;
                            arrInventory["boost"]["hazuki"]["metal"] = 6;
                            arrInventory["boost"]["hazuki"]["ojamajos"] = 4;

                            arrInventory["boost"]["aiko"]["normal"] = 10;
                            arrInventory["boost"]["aiko"]["platinum"] = 7;
                            arrInventory["boost"]["aiko"]["metal"] = 6;
                            arrInventory["boost"]["aiko"]["ojamajos"] = 4;

                            arrInventory["boost"]["onpu"]["normal"] = 10;
                            arrInventory["boost"]["onpu"]["platinum"] = 7;
                            arrInventory["boost"]["onpu"]["metal"] = 6;
                            arrInventory["boost"]["onpu"]["ojamajos"] = 4;

                            arrInventory["boost"]["other"]["special"] = 7;
                            concatResponseSuccess = ":sparkles: You received **Rythm Tap Card** Boost! Activate it on card spawn with **<bot>!card capture boost**";
                        }
                        else if (selectionItem == 7)
                        {
                            arrInventory["boost"]["doremi"]["normal"] = 10;
                            arrInventory["boost"]["doremi"]["platinum"] = 7;
                            arrInventory["boost"]["doremi"]["metal"] = 7;
                            arrInventory["boost"]["doremi"]["ojamajos"] = 5;

                            arrInventory["boost"]["hazuki"]["normal"] = 10;
                            arrInventory["boost"]["hazuki"]["platinum"] = 7;
                            arrInventory["boost"]["hazuki"]["metal"] = 7;
                            arrInventory["boost"]["hazuki"]["ojamajos"] = 5;

                            arrInventory["boost"]["aiko"]["normal"] = 10;
                            arrInventory["boost"]["aiko"]["platinum"] = 7;
                            arrInventory["boost"]["aiko"]["metal"] = 7;
                            arrInventory["boost"]["aiko"]["ojamajos"] = 5;

                            arrInventory["boost"]["onpu"]["normal"] = 10;
                            arrInventory["boost"]["onpu"]["platinum"] = 7;
                            arrInventory["boost"]["onpu"]["metal"] = 7;
                            arrInventory["boost"]["onpu"]["ojamajos"] = 5;

                            arrInventory["boost"]["momoko"]["normal"] = 10;
                            arrInventory["boost"]["momoko"]["platinum"] = 7;
                            arrInventory["boost"]["momoko"]["metal"] = 7;
                            arrInventory["boost"]["momoko"]["ojamajos"] = 5;

                            arrInventory["boost"]["other"]["special"] = 7;
                            concatResponseSuccess = ":sparkles: You received **Kururun Pollon Card** Boost! Activate it on card spawn with **<bot>!card capture boost**";
                        }
                        else if (selectionItem == 8)
                        {
                            arrInventory["boost"]["doremi"]["normal"] = 10;
                            arrInventory["boost"]["doremi"]["platinum"] = 7;
                            arrInventory["boost"]["doremi"]["metal"] = 8;
                            arrInventory["boost"]["doremi"]["ojamajos"] = 6;

                            arrInventory["boost"]["hazuki"]["normal"] = 10;
                            arrInventory["boost"]["hazuki"]["platinum"] = 7;
                            arrInventory["boost"]["hazuki"]["metal"] = 8;
                            arrInventory["boost"]["hazuki"]["ojamajos"] = 6;

                            arrInventory["boost"]["aiko"]["normal"] = 10;
                            arrInventory["boost"]["aiko"]["platinum"] = 7;
                            arrInventory["boost"]["aiko"]["metal"] = 8;
                            arrInventory["boost"]["aiko"]["ojamajos"] = 6;

                            arrInventory["boost"]["onpu"]["normal"] = 10;
                            arrInventory["boost"]["onpu"]["platinum"] = 7;
                            arrInventory["boost"]["onpu"]["metal"] = 8;
                            arrInventory["boost"]["onpu"]["ojamajos"] = 6;

                            arrInventory["boost"]["other"]["special"] = 7;
                            concatResponseSuccess = ":sparkles: You received **Picotto Pollon Card** Boost! Activate it on card spawn with **<bot>!card capture boost**";
                        }
                        else if (selectionItem == 9)
                        {
                            arrInventory["boost"]["doremi"]["normal"] = 10;
                            arrInventory["boost"]["doremi"]["platinum"] = 8;
                            arrInventory["boost"]["doremi"]["metal"] = 7;
                            arrInventory["boost"]["doremi"]["ojamajos"] = 7;

                            arrInventory["boost"]["hazuki"]["normal"] = 10;
                            arrInventory["boost"]["hazuki"]["platinum"] = 8;
                            arrInventory["boost"]["hazuki"]["metal"] = 7;
                            arrInventory["boost"]["hazuki"]["ojamajos"] = 7;

                            arrInventory["boost"]["aiko"]["normal"] = 10;
                            arrInventory["boost"]["aiko"]["platinum"] = 8;
                            arrInventory["boost"]["aiko"]["metal"] = 7;
                            arrInventory["boost"]["aiko"]["ojamajos"] = 7;

                            arrInventory["boost"]["onpu"]["normal"] = 10;
                            arrInventory["boost"]["onpu"]["platinum"] = 8;
                            arrInventory["boost"]["onpu"]["metal"] = 7;
                            arrInventory["boost"]["onpu"]["ojamajos"] = 7;

                            arrInventory["boost"]["other"]["special"] = 6;
                            concatResponseSuccess = ":sparkles: You received **Patraine Call Card** Boost! Activate it on card spawn with **<bot>!card capture boost**";
                        }
                        else if (selectionItem == 10)
                        {
                            arrInventory["boost"]["doremi"]["normal"] = 10;
                            arrInventory["boost"]["doremi"]["platinum"] = 9;
                            arrInventory["boost"]["doremi"]["metal"] = 7;
                            arrInventory["boost"]["doremi"]["ojamajos"] = 8;

                            arrInventory["boost"]["hazuki"]["normal"] = 10;
                            arrInventory["boost"]["hazuki"]["platinum"] = 9;
                            arrInventory["boost"]["hazuki"]["metal"] = 7;
                            arrInventory["boost"]["hazuki"]["ojamajos"] = 8;

                            arrInventory["boost"]["aiko"]["normal"] = 10;
                            arrInventory["boost"]["aiko"]["platinum"] = 9;
                            arrInventory["boost"]["aiko"]["metal"] = 7;
                            arrInventory["boost"]["aiko"]["ojamajos"] = 8;

                            arrInventory["boost"]["onpu"]["normal"] = 10;
                            arrInventory["boost"]["onpu"]["platinum"] = 9;
                            arrInventory["boost"]["onpu"]["metal"] = 7;
                            arrInventory["boost"]["onpu"]["ojamajos"] = 8;

                            arrInventory["boost"]["other"]["special"] = 8;
                            concatResponseSuccess = ":sparkles: You received **Wreath Pollon Card** Boost! Activate it on card spawn with **<bot>!card capture boost**";
                        }
                        else if (selectionItem == 11)
                        {
                            arrInventory["boost"]["doremi"]["normal"] = 10;
                            arrInventory["boost"]["doremi"]["platinum"] = 9;
                            arrInventory["boost"]["doremi"]["metal"] = 8;
                            arrInventory["boost"]["doremi"]["ojamajos"] = 8;

                            arrInventory["boost"]["hazuki"]["normal"] = 10;
                            arrInventory["boost"]["hazuki"]["platinum"] = 9;
                            arrInventory["boost"]["hazuki"]["metal"] = 8;
                            arrInventory["boost"]["hazuki"]["ojamajos"] = 8;

                            arrInventory["boost"]["aiko"]["normal"] = 10;
                            arrInventory["boost"]["aiko"]["platinum"] = 9;
                            arrInventory["boost"]["aiko"]["metal"] = 8;
                            arrInventory["boost"]["aiko"]["ojamajos"] = 8;

                            arrInventory["boost"]["onpu"]["normal"] = 10;
                            arrInventory["boost"]["onpu"]["platinum"] = 9;
                            arrInventory["boost"]["onpu"]["metal"] = 8;
                            arrInventory["boost"]["onpu"]["ojamajos"] = 8;

                            arrInventory["boost"]["momoko"]["normal"] = 10;
                            arrInventory["boost"]["momoko"]["platinum"] = 9;
                            arrInventory["boost"]["momoko"]["metal"] = 8;
                            arrInventory["boost"]["momoko"]["ojamajos"] = 8;

                            arrInventory["boost"]["other"]["special"] = 8;
                            concatResponseSuccess = ":sparkles: You received **Jewelry Pollon Card** Boost! Activate it on card spawn with **<bot>!card capture boost**";
                        }


                        arrInventory["magic_seeds"] = (magicSeeds - priceConfirmation).ToString();
                        File.WriteAllText(playerDataDirectory, arrInventory.ToString());

                        stepProcess = 5;
                    }
                    else
                    {
                        stepProcess = 2;
                        await ReplyAsync(":x: Sorry, you don't have enough magic seeds.");
                    }
                }
                else if (stepProcess == 5)
                {
                    await ReplyAsync(concatResponseSuccess + "\nThank you for purchasing the items. Please come again next time~");
                    isShopping = false;
                }


            }

            return;
        }

        [Command("boost", RunMode = RunMode.Async), Summary("Show card boost status.")]
        public async Task showCardBoostStatus()
        {
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;
            string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";

            if (!File.Exists(playerDataDirectory)) //not registered yet
            {
                await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription($":x: I'm sorry, please register yourself first with **{Config.Doremi.PrefixParent[0]}card register** command.")
                .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build());
                return;
            } else {
                await ReplyAsync(embed: TradingCardCore
                    .printCardBoostStatus(Config.Doremi.EmbedColor, guildId.ToString(), clientId,Context.User.Username)
                    .Build());
            }
        }

        [Command("rate", RunMode = RunMode.Async), Summary("Show card spawn & catch rate.")]
        public async Task showCardSpawnCatchRate()
        {
            await ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Doremi.EmbedColor)
            .WithTitle("Trading Card Catch Rate & Spawn Rate Information")
            .AddField("Catch Rate",
            $"**Normal: {TradingCardCore.captureRateNormal * 10}%\n" +
            $"Platinum: {TradingCardCore.captureRatePlatinum * 10}%\n" +
            $"Metal: {TradingCardCore.captureRateMetal * 10}%\n" +
            $"Ojamajos: {TradingCardCore.captureRateOjamajos * 10}%\n" +
            $"Special: {TradingCardCore.captureRateSpecial * 10}%**",true)
            .AddField("Spawn Rate",
            $"**Normal: {TradingCardCore.spawnRateNormal * 10}%\n" +
            $"Platinum: {TradingCardCore.spawnRatePlatinum * 10}%\n" +
            $"Metal: {TradingCardCore.spawnRateMetal * 10}%\n" +
            $"Ojamajos: {TradingCardCore.spawnRateOjamajos * 10}%**", true)
            .Build());
        }
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

        [Command("to be continue", RunMode = RunMode.Async), Alias("tbc"), Summary("Add to be continue image filter to the image.")]
        public async Task drawMemesdrawToBeContinue(string attachment=""){

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
            catch(Exception e) {
                await ReplyAsync($"Please give me some image to process on.");
                //Console.WriteLine(e.ToString());
            }
            
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

            await ReplyAsync(embed: MinigameCore.printLeaderboard(Context,Config.Doremi.EmbedColor,
                guildId.ToString(),userId.ToString()).Build());

        }

        [Command("rockpaperscissor", RunMode = RunMode.Async), Alias("rps"), Summary("Play the Rock Paper Scissor minigame with Doremi. 20 score points reward.")]
        public async Task RockPaperScissor(string guess = "")
        {
            if (guess == ""){
                await ReplyAsync($":x: Please enter the valid parameter: **rock** or **paper** or **scissor**");
                return;
            } else if (guess.ToLower() != "rock" && guess.ToLower() != "paper" && guess.ToLower() != "scissor") {
                await ReplyAsync($":x: Sorry **{Context.User.Username}**. " +
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

            Tuple<string, EmbedBuilder> result = MinigameCore.rockPaperScissor.rpsResults(Config.Doremi.EmbedColor, Config.Doremi.EmbedAvatarUrl, randomGuess, guess, "doremi", Context.User.Username,
                arrWinReaction, arrLoseReaction, arrDrawReaction,
                Context.Guild.Id, Context.User.Id);

            await Context.Channel.SendFileAsync(result.Item1, embed: result.Item2.Build());
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
                    else if (loweredResponse == randomedAnswer)
                    {
                        Config.Doremi.isRunningMinigame.Remove(Context.User.Id.ToString());

                        await ReplyAsync($"\uD83D\uDC4F Congratulations **{Context.User.Username}**, you guess the correct answer: **{randomedAnswer}**. Your **score+{scoreValue}**");

                        var guildId = Context.Guild.Id;
                        var userId = Context.User.Id;

                        //save the data
                        MinigameCore.updateScore(guildId.ToString(), userId.ToString(), scoreValue);
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

    //calculate how many days has been on the server
    //

}
