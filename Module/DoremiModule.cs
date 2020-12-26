using Config;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Lavalink.NET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OjamajoBot.Service;
using Spectacles.NET.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Victoria;
using Victoria.Enums;

using MySql.Data.MySqlClient;
using OjamajoBot.Database;
using System.Data;
using OjamajoBot.Database.Model;
using OjamajoBot.Core;

namespace OjamajoBot.Module
{
    [Name("General")]
    class DoremiModule : InteractiveBase
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

        [Name("help"), Command("help"), Summary("Show all Doremi bot Commands.")]
        public async Task Help([Remainder] string CategoryOrCommands = "")
        {
            EmbedBuilder output = new EmbedBuilder();
            output.Color = Config.Doremi.EmbedColor;

            if (CategoryOrCommands == "")
            {
                output.WithAuthor($"{Config.Doremi.EmbedName} Command List", Config.Doremi.EmbedAvatarUrl);
                output.Description = "Pretty Witchy Doremi Chi! You can call me with " +
                    $"**{Config.Doremi.PrefixParent[2]} or {Config.Doremi.PrefixParent[0]} or {Config.Doremi.PrefixParent[1]}** as starting prefix.\n" +
                    $"Use **{Config.Doremi.PrefixParent[0]}help <category or commands>** for more help details.\n" +
                    $"Example: **{Config.Doremi.PrefixParent[0]}help general** or **{Config.Doremi.PrefixParent[0]}help hello**";

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
                        await ReplyAsync(embed: output.Build());
                        return;
                    }
                    else
                    {
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

            if (!object.ReferenceEquals(group, null))
            {
                group = category + " ";
            }
            completedText += $"**Example:** `{Config.Doremi.PrefixParent[0]}{group}{commands}";
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

            if (arrImage.ContainsKey(form))
            {
                await ReplyAsync("Pretty Witchy Doremi Chi~\n", embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl(arrImage[form])
                .Build());
            }
            else
            {
                await ReplyAsync($"Sorry, I can't found that form. See `{Config.Doremi.PrefixParent[0]} help change` for help details");
            }
        }

        [Command("fairy"), Summary("I will show you my fairy info")]
        public async Task showFairy()
        {
            await ReplyAsync("Meet one of my fairy: Dodo.",
            embed: new EmbedBuilder()
            .WithAuthor("Dodo")
            .WithDescription("Dodo has fair skin and big mulberry eyes and blushed cheeks. She has pink antennae and short straightened bangs, and she has hair worn in large buns. Her dress is salmon-pink with a pale pink collar." +
            "In teen form, her hair buns grow smaller and she gains a full body.She wears a light pink dress with the shoulder cut out and a white collar.A salmon - pink top is worn under this, and at the chest is a pink gem.She also wears white booties and a white witch hat with a pale pink rim.")
            .WithColor(Config.Doremi.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e2/No.076.jpg")
            .WithFooter("[Ojamajo Witchling Wiki](https://ojamajowitchling.fandom.com/wiki/Dodo)")
            .Build());
        }

        [Command("birthday thismonth"), Alias("birthday thismonth"), Summary("Show all user that will have birthday on this month.")]
        public async Task showAllBirthdayThisMonth()
        {
            Boolean birthdayExisted = false;
            EmbedBuilder builder = new EmbedBuilder();
            var guildId = Context.Guild.Id;

            string query = $"SELECT * " +
                $" FROM {DBM_Guild_User_Birthday.tableName} " +
                $" WHERE {DBM_Guild_User_Birthday.Columns.id_guild}=@{DBM_Guild_User_Birthday.Columns.id_guild} AND " +
                $" MONTH({DBM_Guild_User_Birthday.Columns.birthday_date})=@{DBM_Guild_User_Birthday.Columns.birthday_date} ";
            Dictionary<string, object> columnsFilter = new Dictionary<string, object>();
            columnsFilter[DBM_Guild_User_Birthday.Columns.id_guild] = guildId.ToString();
            columnsFilter[DBM_Guild_User_Birthday.Columns.birthday_date] = DateTime.Now.ToString("MM");
            var result = new DBC().selectAll(query, columnsFilter);
            foreach (DataRow row in result.Rows)
            {
                try
                {
                    var username = Context.Guild.GetUser(
                        Convert.ToUInt64(row[DBM_Guild_User_Birthday.Columns.id_user].ToString())).Username;
                    var birthdayDate = DateTime.Parse(row[DBM_Guild_User_Birthday.Columns.birthday_date].ToString())
                        .ToString("dd-MMMM");
                    builder.AddField(username, birthdayDate, true);
                    birthdayExisted = true;
                }
                catch { }
            }

            if (birthdayExisted)
            {
                builder.Title = $"{Config.Emoji.birthdayCake} {DateTime.Now.ToString("MMMM")} Birthday List";
                builder.Description = $"Here are the list of all wonderful people that will have birthday on this month:";
                builder.Color = Config.Doremi.EmbedColor;
                await ReplyAsync(embed: builder.Build());
            }
            else
            {
                await ReplyAsync("We don't have birthday celebration for this month.");
            }

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
                Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour)
            {
                await ReplyAsync(arrResponse[new Random().Next(0, arrResponse.Length)],
                embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl(arrResponseImg[new Random().Next(0, arrResponseImg.Length)])
                .Build());
            }
            else
            {
                await ReplyAsync("Sorry, but it's not my birthday yet.",
                embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/6/6e/Hanabou2.jpg")
                .Build());
            }
        }

        [Command("hello"), Summary("Hello, I will greet you up")]
        public async Task doremiHello()
        {
            List<string> listRandomRespond = new List<string>() {
                    $"Hii hii {MentionUtils.MentionUser(Context.User.Id)}! ",
                    $"Hello {MentionUtils.MentionUser(Context.User.Id)}! ",
            };

            int rndIndex = new Random().Next(0, listRandomRespond.Count);
            string tempReply = listRandomRespond[rndIndex] + Config.Doremi.Status.currentActivityReply;

            await ReplyAsync(embed:new EmbedBuilder()
                .WithDescription(tempReply)
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl("https://cdn.discordapp.com/attachments/706770454697738300/790613769733799946/wMuLt-rH2mxh6mztcb9HTt4JE3KumFTgNSa7CTv4-AyUQRiQXAkSt529hIC8_n_S5lyHKwASh0oZ0cz96ModT2fCjE1ddj6NvBqo.png")
                .Build());
        }

        [Command("hug"), Summary("Give a hug for <username>")]
        public async Task HugUser([Remainder]string username)
        {
            List<string> listRandomImage = new List<string>() {
                "https://media.tenor.com/images/f51ff7041983283592e13e3e0c3b29b9/tenor.gif",
                "https://cdn.discordapp.com/attachments/706770454697738300/790768946894602290/hanahug.gif"
            };

            int rndIndex = new Random().Next(0, listRandomImage.Count);

            await ReplyAsync(embed:new EmbedBuilder()
                .WithDescription($"{MentionUtils.MentionUser(Context.User.Id)} has give a nice & friendly hug for {username}")
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl(listRandomImage[rndIndex])
                .Build());
        }

        [Command("invite"), Summary("Generate the related invitation links for ojamajo bot")]
        public async Task invite()
        {
            await ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Doremi.EmbedColor)
            .WithTitle("Bot Invitation Links")
            .WithDescription($"Pirika pirilala poporina peperuto! Generate the bot links!")
            .AddField("Doremi Bot", $"[Invite Doremi Bot](https://discordapp.com/api/oauth2/authorize?client_id={Bot.Doremi.client.CurrentUser.Id}&permissions=470154308&scope=bot)", true)
            .AddField("Hazuki Bot", $"[Invite Hazuki Bot](https://discordapp.com/api/oauth2/authorize?client_id={Bot.Hazuki.client.CurrentUser.Id}&permissions=470154440&scope=bot)", true)
            .AddField("Aiko Bot", $"[Invite Aiko Bot](https://discordapp.com/api/oauth2/authorize?client_id={Bot.Aiko.client.CurrentUser.Id}&permissions=470154440&scope=bot)", true)
            .AddField("Onpu Bot", $"[Invite Onpu Bot](https://discordapp.com/api/oauth2/authorize?client_id={Bot.Onpu.client.CurrentUser.Id}&permissions=470154440&scope=bot)", true)
            .AddField("Momoko Bot", $"[Invite Momoko Bot](https://discordapp.com/api/oauth2/authorize?client_id={Bot.Momoko.client.CurrentUser.Id}&permissions=470154440&scope=bot)", true)
            //.AddField("Pop Bot", "[Invite Pop Bot](https://discordapp.com/api/oauth2/authorize?client_id=" + Config.Pop.Id + "&permissions=238419008&scope=bot)", true)
            .Build());
        }

        [Command("magical stage"), Alias("magicalstage"), Summary("I will perform magical stage along with the other and make a <wishes>")]
        public async Task magicalStage([Remainder] string wishes = "")
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

        //dorememes disabled for now
        //[Command("dorememes"), Summary("I will give you some random doremi related memes. " +
        //    "You can fill <contributor> with one of the available to make it spesific contributor.\nUse `list` as parameter to list all people who have contribute the dorememes.")]
        //public async Task givedorememe([Remainder] string contributor = "")
        //{
        //    string finalUrl = ""; JArray getDataObject = null;
        //    contributor = contributor.ToLower();

        //    if (contributor == "list")
        //    {
        //        var key = Config.Doremi.jobjectdorememes.Properties().ToList();
        //        string listedContributor = "";
        //        for (int i = 0; i < key.Count; i++) listedContributor += $"{key[i].Name}\n";

        //        await base.ReplyAsync(embed: new EmbedBuilder()
        //            .WithTitle("Dorememes listed contributor")
        //            .WithDescription("Thank you to all of these peoples that have contributed the dorememes:")
        //            .AddField("Contributor in List", listedContributor)
        //            .WithColor(Config.Doremi.EmbedColor)
        //            .Build());
        //        return;
        //    }
        //    else if (contributor == "")
        //    {
        //        var key = Config.Doremi.jobjectdorememes.Properties().ToList();
        //        var randIndex = new Random().Next(0, key.Count);
        //        contributor = key[randIndex].Name;
        //        getDataObject = (JArray)Config.Doremi.jobjectdorememes[contributor];
        //        finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
        //    }
        //    else
        //    {
        //        if (Config.Doremi.jobjectdorememes.ContainsKey(contributor))
        //        {
        //            getDataObject = (JArray)Config.Doremi.jobjectdorememes[contributor];
        //            finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
        //        }
        //        else
        //        {
        //            await base.ReplyAsync($"Oops, I can't found the specified contributor. " +
        //                $"See `{Config.Doremi.PrefixParent[0]}help dorememe` for commands help.");
        //            return;
        //        }
        //    }

        //    await base.ReplyAsync(embed: new EmbedBuilder()
        //    .WithColor(Config.Doremi.EmbedColor)
        //    .WithImageUrl(finalUrl)
        //    .WithFooter("Contributed by: " + contributor)
        //    .Build());

        //}

        //[Command("meme", RunMode = RunMode.Async), Alias("memes"), Summary("I will give you some random memes")]
        //public async Task givememe()
        //{
        //    try
        //    {
        //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://meme-api.herokuapp.com/gimme/memes/10");
        //        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        //        StreamReader reader = new StreamReader(response.GetResponseStream());
        //        string jsonResp = reader.ReadToEnd().ToString();
        //        JObject jobject = JObject.Parse(jsonResp);

        //        int randomIndex = new Random().Next(0, 11);
        //        var description = jobject.GetValue("memes")[randomIndex]["title"];
        //        var imgUrl = jobject.GetValue("memes")[randomIndex]["url"];

        //        await base.ReplyAsync(embed: new EmbedBuilder()
        //        .WithDescription(description.ToString())
        //        .WithColor(Config.Doremi.EmbedColor)
        //        .WithImageUrl(imgUrl.ToString())
        //        .Build());

        //    }
        //    catch
        //    {
        //        //Console.Write(e.ToString());
        //    }

        //}

        [Command("ping"), Summary("Show the ping of ojamajo bot.")]
        public async Task printPing()
        {
            await ReplyAsync($"Hello! I'm running at **{Context.Client.Latency} ms**");
        }

        [Command("random"), Alias("moments"), Summary("Show any random Doremi moments. " +
            "Fill <moments> with **random/first/sharp/motto/naisho/dokkan** for spesific moments.")]
        public async Task randomthing(string moments = "")
        {
            string finalUrl = ""; string footerUrl = "";
            JArray getDataObject = null; moments = moments.ToLower();
            if (moments == "")
            {
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
                }
                else
                {
                    var key = Config.Doremi.jObjRandomMoments.Properties().ToList();
                    var randIndex = new Random().Next(0, key.Count);
                    moments = key[randIndex].Name;
                    getDataObject = (JArray)Config.Doremi.jObjRandomMoments[moments];
                    finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
                }
            }
            else
            {
                if (Config.Doremi.jObjRandomMoments.ContainsKey(moments))
                {
                    getDataObject = (JArray)Config.Doremi.jObjRandomMoments[moments];
                    finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
                }
                else
                {
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

        [Command("debug"), Summary("Debug command")]
        public async Task debug()
        {
            await TradingCardCore.generateCardSpawn(Context.Guild.Id);
        }

        [Command("stats"), Alias("bio"), Summary("I will show you my biography info")]
        public async Task showStats()
        {
            await ReplyAsync("Pirika pirilala poporina peperuto! Show my biography info!",
            embed: new EmbedBuilder()
            .WithAuthor("Doremi Harukaze", Config.Doremi.EmbedAvatarUrl)
            .WithDescription("Doremi Harukaze (春風どれみ, Harukaze Doremi) is the main character of Ojamajo Doremi. " +
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
            .WithImageUrl("https://static.wikia.nocookie.net/ojamajowitchling/images/0/07/O.D_LFMD%27_Doremi_Harukaze.png")
            .WithFooter("Source: [Ojamajo Doremi Wiki](https://ojamajowitchling.fandom.com/wiki/Doremi_Harukaze)")
            .Build());
        }

        //temporarily disabled
        //[Command("star"), Summary("I will pin this messsages if it has 5 stars reaction")]
        //[RequireBotPermission(ChannelPermission.ManageMessages,
        //    ErrorMessage = "Oops, I need `manage channels` permission to use this command")]
        //public async Task starMessages([Remainder] string MessagesOrWithAttachment = "")
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
        //        if (sentAttachment != null)
        //        {
        //            await sentAttachment.AddReactionAsync(new Discord.Emoji("\u2B50"));
        //        }
        //        else
        //        {
        //            await sentMessage.AddReactionAsync(new Discord.Emoji("\u2B50"));
        //        }

        //        File.Delete(completePath);
        //        return;

        //        //} else {
        //        //    await ReplyAsync($"Oops, sorry only ``.jpg/.jpeg/.png/.gif`` format is allowed to use ``star`` commands.");
        //        //    return;
        //        //}
        //    }
        //    catch
        //    {
        //        //Console.WriteLine(e.ToString());
        //    }

        //    if (MessagesOrWithAttachment == "")
        //    {
        //        await ReplyAsync("Please write some text to be starred on.");
        //        return;
        //    }
        //    await Context.Message.DeleteAsync();
        //    var sentWithoutAttached = await ReplyAsync($"{MentionUtils.MentionUser(Context.User.Id)} has a star messages:\n{MessagesOrWithAttachment}");
        //    await sentWithoutAttached.AddReactionAsync(new Discord.Emoji("\u2B50"));
        //}

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

            await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription(arrRandom[rndIndex, 0])
                .WithImageUrl(arrRandom[rndIndex, 1])
                .Build());
        }

        [Command("thank you"), Alias("thanks", "arigatou"), Summary("Say thank you to Doremi Bot")]
        public async Task thankYou([Remainder] string messages = "")
        {
            await ReplyAsync($"Your welcome, {MentionUtils.MentionUser(Context.User.Id)}. I'm glad that you're happy with it :smile:");
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
            await Context.Message.DeleteAsync();
            await ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Doremi.EmbedColor)
            .WithTitle($"Ojamajo Bot v{Config.Core.version}")
            .AddField("**Introducing: Avatar Profile**:",
            "This is a new feature where you can see your user avatar profile. " +
            "Level can be increased up to 200. " +
            "Everytime you post something on the server you'll have a chance to get 1 exp. " +
            "Available command:\n" +
            $"**{Config.Doremi.PrefixParent[0]}avatar profile <optional username>**: see your/other avatar\n" +
            $"**{Config.Doremi.PrefixParent[0]}avatar set info <some info>**: set your avatar info\n" +
            $"**{Config.Doremi.PrefixParent[0]}avatar set nickname <nickname>**: set your avatar nickname")
            .Build());
        }

        [Command("daily", RunMode = RunMode.Async), Alias("water plant"), Summary("Water the plant and receive daily magic seeds.")]
        public async Task claimDailyMagicalSeeds()
        {
            var clientId = Context.User.Id;

            //for table recreation purpose if it's not existed yet
            Dictionary<string, object> userData = UserDataCore.getUserData(clientId);
            Dictionary<string, object> userGardenData = GardenCore.getUserGardenData(clientId);

            if (userGardenData[DBM_User_Garden_Data.Columns.last_water_time].ToString() == "" ||
                DateTime.Parse(userGardenData[DBM_User_Garden_Data.Columns.last_water_time].ToString()).ToString("dd")
                != DateTime.Now.ToString("dd"))
            {
                int randomedReceive = new Random().Next(20, 31);
                int randomedGrowth = new Random().Next(Convert.ToInt32(GardenCore.weather[3]),
                   Convert.ToInt32(GardenCore.weather[4]) + 1);

                int newPlantGrowth = Convert.ToInt32(userGardenData["plant_growth"]) + randomedGrowth;

                GardenCore.waterPlant(clientId, randomedGrowth);
                UserDataCore.updateMagicSeeds(clientId, randomedReceive);

                if (newPlantGrowth >= 100)
                { //royal plant is bloomed
                    GardenCore.updatePlantProgress(clientId, 0);
                    UserDataCore.updateRoyalSeeds(clientId, 1);

                    //reload data
                    userData = UserDataCore.getUserData(clientId);
                    userGardenData = GardenCore.getUserGardenData(clientId);

                    await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithTitle($"{Context.User.Username}'s royal plant has been bloomed!")
                    .WithDescription($"With some determination and patience, " +
                    $"{MentionUtils.MentionUser(clientId)} royal plant has been bloomed and " +
                    $" received 1 royal seeds!")
                    .WithThumbnailUrl(GardenCore.imgRoyalSeeds)
                    .WithFooter($"Total royal seeds: {userData[DBM_User_Data.Columns.royal_seeds]}")
                    .Build());
                }
                else
                {
                    //reload data
                    userData = UserDataCore.getUserData(clientId);
                    userGardenData = GardenCore.getUserGardenData(clientId);

                    await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithDescription($":seedling: {MentionUtils.MentionUser(clientId)} " +
                    $"have watered the plant and received **{randomedReceive}** magic seed(s) & **{randomedGrowth}%** " +
                    $"plant growth progress from {GardenCore.weather[0]}**{GardenCore.weather[1]}** weather effect. " +
                    $"Thank you for watering the plant.")
                    .AddField("Current plant growth progress:", $"{userGardenData[DBM_User_Garden_Data.Columns.plant_growth]}%")
                    .WithThumbnailUrl(GardenCore.imgMagicSeeds)
                    .WithFooter($"Total magic seeds: {userData[DBM_User_Data.Columns.magic_seeds]}")
                    .Build());
                }
            }
            else
            {
                var now = DateTime.Now;
                var tomorrow = now.AddDays(1).Date;
                double totalHours = (tomorrow - now).TotalHours;

                await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription($":x: Sorry **{Context.User.Username}**, you already watered the plant today.\n" +
                $"Please wait for **{Math.Floor(totalHours)}** hour(s) " +
                $"**{Math.Ceiling(60 * (totalHours - Math.Floor(totalHours)))}** more minute(s) until the next growing time.")
                .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build());
            }

        }

        [Command("seeds"), Summary("See the total amount of seeds currency that you have.")]
        public async Task showTotalSeeds()
        {
            var clientId = Context.User.Id;
            var userAvatar = Context.User.GetAvatarUrl();

            Dictionary<string, object> userData = UserDataCore.getUserData(clientId);

            await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription($":seedling: {MentionUtils.MentionUser(clientId)} have:\n" +
                $"-**{userData["magic_seeds"]}/3000** magic seeds.\n" +
                $"-**{userData["royal_seeds"]}/10** royal seeds.")
                .WithThumbnailUrl(userAvatar)
                .Build());

        }

        [Command("profile", RunMode = RunMode.Async), Alias("status"), Summary("See your avatar profile. " +
            "You can also put username as optional parameter to see other user avatar.")]
        public async Task user_avatar_other([Remainder] SocketGuildUser username = null)
        {
            await ReplyAsync(embed:
                GuildUserAvatarCore.printAvatarStatus(Context, username).Build());
        }

        //[Command("achievement"), Alias("achievement"), Summary("See your achievements")]
        //public async Task showAchievements()
        //{
        //    ulong guildId = Context.Guild.Id;
        //    ulong clientId = Context.User.Id;

        //    await PagedReplyAsync(AchievementsCore.printAchievementsStatus(
        //        Config.Doremi.EmbedColor,guildId,clientId,Context.User.Username,Context.User.GetAvatarUrl()
        //    ));
        //}

        //event schedule/reminder
        //vote for best characters
        //change into pet form for doremi & other bot
        //present: give a random present on reaction unwrapped
        //present to someone: give a random present on reaction unwrapped
        //todo/more upcoming commands: easter egg/hidden commands, set daily message announcement 
        //contribute caption for random things
        //user card maker, sing lyrics together with other ojamajo bot, voting for best ojamajo bot, witch seeds to cast a spells
    }

    [Name("Garden"), Group("garden"), Summary("This commands category related with gardening.")]
    public class DoremiGardenInteractive : InteractiveBase
    {
        [Command("weather", RunMode = RunMode.Async), Summary("See the weather forecast for today.")]
        public async Task showCurrentWeather()
        {
            //{$"☀️", "sunny","It's a sunny day!","5"}
            await ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Doremi.EmbedColor)
            .WithTitle($"{GardenCore.weather[0]} It's {GardenCore.weather[1]} now.")
            .WithDescription(GardenCore.weather[2])
            .AddField("Plant growth rate:", $"{GardenCore.weather[3]}-{GardenCore.weather[4]}%")
            .WithFooter("The weather will change every 2 hours.")
            .Build());
        }

        [Command("progress", RunMode = RunMode.Async), Summary("Check your plant growth progress.")]
        public async Task getGardenProgress()
        {
            var userId = Context.User.Id;
            Dictionary<string, object> userGardenData = GardenCore.getUserGardenData(userId);

            string reminder = "";
            if (DateTime.Parse(userGardenData[DBM_User_Garden_Data.Columns.last_water_time].ToString()).ToString("dd")
                != DateTime.Now.ToString("dd"))
                reminder = "Friendly reminder: the plant is not watered yet today.";

            await ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Doremi.EmbedColor)
            .WithAuthor($"Context.User.Username,Context.User.GetAvatarUrl()' Garden Progress")
            .WithDescription($"{MentionUtils.MentionUser(userId)} plant growth progress currently at: " +
            $"**{userGardenData[DBM_User_Garden_Data.Columns.plant_growth]}%**")
            .WithFooter(reminder)
            .Build());
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

                        if (currentIndex <= 10)
                        {
                            currentIndex++;
                        }
                        else
                        {
                            pageContent.Add(tempVal);
                            currentIndex = 0;
                            tempVal = "";
                            indexPage += 1;
                        }

                        if (i == arrList.Count - 1) pageContent.Add(tempVal);

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

            }
            else
            {
                await ReplyAsync($"I'm sorry, but I can't find that season. See `{Config.Doremi.PrefixParent[0]}help wiki episodes` for commands help.");
            }

        }

        [Command("witches", RunMode = RunMode.Async), Alias("witch"), Summary("I will give all witches characters list. " +
            "Fill the optional <characters> parameter with the available witches characters name.")]
        public async Task showCharactersWitches([Remainder] string characters = "")
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

        [Command("wizards", RunMode = RunMode.Async), Alias("wizard"), Summary("Show all ojamajo doremi wizards characters list. " +
            "Fill the optional <characters> parameter with the available wizards characters name.")]
        public async Task showCharactersWizards([Remainder] string characters = "")
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

    //archived for now
    //[Name("role"), Group("role"), Summary("These contains role commands category.")]
    //public class DoremiRoles : InteractiveBase
    //{
    //    [Command("statistics"), Summary("Display the role statistics.")]
    //    public async Task showRoleStatistics()
    //    {
    //        var guildId = Context.Guild.Id;
    //        var guildJsonFile = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json"));

    //        PaginatedAppearanceOptions pao = new PaginatedAppearanceOptions();
    //        pao.JumpDisplayOptions = JumpDisplayOptions.Never;
    //        pao.DisplayInformationIcon = false;

    //        List<string> pageContent = new List<string>();
    //        string title = $"";
    //        JArray arrList = (JArray)guildJsonFile["roles_list"];

    //        string tempVal = title;
    //        int currentIndex = 0;
    //        for (int i = 0; i < arrList.ToList().Count(); i++)
    //        {
    //            var roleSearch = Context.Guild.Roles.FirstOrDefault(x => x.Id == Convert.ToUInt64(arrList[i]));
    //            var roleSearchMembers = Context.Guild.Roles.FirstOrDefault(x => x.Id == Convert.ToUInt64(arrList[i])).Members;

    //            if (roleSearch != null)
    //            {
    //                var usersList = Context.Guild.Users.Select(
    //                    x => x.Roles.FirstOrDefault(y => y.Id == Convert.ToUInt64(arrList[i]))
    //                ).ToList();
    //                int tempCtr = 0;
    //                for (int j = 0; j < usersList.Count; j++)
    //                {
    //                    if (usersList[j] != null)
    //                    {
    //                        if (usersList[j].Id == roleSearch.Id)
    //                            tempCtr++;
    //                    }
    //                }

    //                tempVal += $"**{MentionUtils.MentionRole(roleSearch.Id)}**: **{tempCtr} members**\n";
    //            }

    //            if (currentIndex <= 10) currentIndex++;
    //            else
    //            {
    //                pageContent.Add(tempVal);
    //                currentIndex = 0;
    //                tempVal = title;
    //            }

    //            if (i == arrList.ToList().Count - 1) pageContent.Add(tempVal);
    //        }

    //        if (arrList.ToList().Count == 0)
    //        {
    //            tempVal = "There are no self assignable role list yet.";
    //            pageContent.Add(tempVal);
    //        }

    //        var pager = new PaginatedMessage
    //        {
    //            Title = $"**Role Statistics**\n",
    //            Pages = pageContent,
    //            Color = Config.Doremi.EmbedColor,
    //            Options = pao
    //        };

    //        await Context.Message.DeleteAsync();
    //        await PagedReplyAsync(pager);

    //    }

    //    [Command("list"), Summary("Show all the available self assignable role list.")]
    //    public async Task showRoleList()
    //    {
    //        var guildId = Context.Guild.Id;
    //        var guildJsonFile = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json"));

    //        PaginatedAppearanceOptions pao = new PaginatedAppearanceOptions();
    //        pao.JumpDisplayOptions = JumpDisplayOptions.Never;
    //        pao.DisplayInformationIcon = false;

    //        List<string> pageContent = new List<string>();
    //        string title = $"";
    //        JArray arrList = (JArray)guildJsonFile["roles_list"];

    //        string tempVal = title;
    //        int currentIndex = 0;
    //        for (int i = 0; i < arrList.ToList().Count(); i++)
    //        {
    //            var roleSearch = Context.Guild.Roles.FirstOrDefault(x => x.Id == Convert.ToUInt64(arrList[i]));
    //            var roleSearchMembers = Context.Guild.Roles.FirstOrDefault(x => x.Id == Convert.ToUInt64(arrList[i])).Members;

    //            if (roleSearch != null)
    //            {
    //                var usersList = Context.Guild.Users.Select(
    //                    x => x.Roles.FirstOrDefault(y => y.Id == Convert.ToUInt64(arrList[i]))
    //                ).ToList();
    //                int tempCtr = 0;
    //                for (int j = 0; j < usersList.Count; j++)
    //                {
    //                    if (usersList[j] != null)
    //                    {
    //                        if (usersList[j].Id == roleSearch.Id)
    //                            tempCtr++;
    //                    }
    //                }

    //                tempVal += $"**{MentionUtils.MentionRole(roleSearch.Id)}**: **{tempCtr} members**\n";
    //            }

    //            if (currentIndex <= 10) currentIndex++;
    //            else
    //            {
    //                pageContent.Add(tempVal);
    //                currentIndex = 0;
    //                tempVal = title;
    //            }

    //            if (i == arrList.ToList().Count - 1) pageContent.Add(tempVal);
    //        }

    //        if (arrList.ToList().Count == 0)
    //        {
    //            tempVal = "There are no self assignable role list yet.";
    //            pageContent.Add(tempVal);
    //        }

    //        var pager = new PaginatedMessage
    //        {
    //            Title = $"**Self Assignable Role List**\n",
    //            Pages = pageContent,
    //            Color = Config.Doremi.EmbedColor,
    //            Options = pao
    //        };

    //        await PagedReplyAsync(pager);

    //    }

    //    [Command("set"), Summary("Set your roles with given role parameter. Use `do!role list` to display all self assignable roles list.")]
    //    public async Task setRole([Remainder] string role)
    //    {
    //        var guildId = Context.Guild.Id;
    //        var guildJsonFile = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json"));
    //        var embed = new EmbedBuilder().WithColor(Config.Doremi.EmbedColor);

    //        var roleSearch = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == role.ToLower());
    //        var userRoles = Context.Guild.GetUser(Context.User.Id).Roles.FirstOrDefault(x => x.Name.ToLower() == role.ToLower());

    //        if (roleSearch == null)
    //            await ReplyAsync($"Sorry, I can't find that role. See the available roles with **{Config.Doremi.PrefixParent[0]}role list**");
    //        else
    //        {
    //            JArray item = (JArray)guildJsonFile["roles_list"];
    //            if (userRoles != null)
    //            {
    //                await ReplyAsync("You already have that roles.");
    //            }
    //            else
    //            {
    //                if (item.ToString().Contains(roleSearch.Id.ToString()))
    //                {
    //                    await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(roleSearch);
    //                    await ReplyAsync(embed: embed
    //                        .WithTitle("Role updated!")
    //                        .WithDescription($":white_check_mark: **{Context.User.Username}** have new role: {MentionUtils.MentionRole(roleSearch.Id)}")
    //                        .Build());
    //                }
    //                else
    //                {
    //                    await ReplyAsync("Sorry, you can't assign into that role.");
    //                }
    //            }
    //        }
    //    }

    //    [Command("remove"), Summary("Remove your roles from given role parameter.")]
    //    public async Task removeRole([Remainder] string role)
    //    {
    //        var guildId = Context.Guild.Id;
    //        var embed = new EmbedBuilder().WithColor(Config.Doremi.EmbedColor);

    //        var roleSearch = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == role.ToLower());
    //        var userRoles = Context.Guild.GetUser(Context.User.Id).Roles.FirstOrDefault(x => x.Name.ToLower() == role.ToLower());

    //        if (roleSearch == null)
    //        {
    //            await ReplyAsync($"Sorry, I can't find that role. See the available roles with **{Config.Doremi.PrefixParent[0]}role list**");
    //        }
    //        else if (userRoles == null)
    //        {
    //            await ReplyAsync("You already have that roles.");
    //        }
    //        else
    //        {
    //            await Context.Guild.GetUser(Context.User.Id).RemoveRoleAsync(roleSearch);
    //            await ReplyAsync(embed: embed
    //                .WithTitle("Role removed!")
    //                .WithDescription($":white_check_mark: **{Context.User.Username}** role has been removed from: {MentionUtils.MentionRole(roleSearch.Id)}")
    //                .Build());
    //        }
    //    }
    //}

    [Name("mod"), Group("mod"), Summary("Basic moderator commands. Require `manage channels` & `manage roles` permission")]
    [RequireUserPermission(GuildPermission.ManageChannels,
        ErrorMessage = "Sorry, you need the `manage channels` permission",
        NotAGuildErrorMessage = "Sorry, you need the `manage channels` permission")]
    [RequireUserPermission(GuildPermission.ManageRoles,
        ErrorMessage = "Sorry, you need the `manage roles` permission",
        NotAGuildErrorMessage = "Sorry, you need the `manage roles` permission")]
    public class DoremiModerator : InteractiveBase
    {
        [Command("customprefix"), Summary("Update the custom prefix for custom command. Maximum length:1. " +
            "If the parameter is empty then it'll be removed.")]
        public async Task updateCustomPrefix(string prefix = "")
        {
            var idGuild = Context.Guild.Id;
            if (prefix.Length >1)
            {
                await ReplyAsync("Custom prefix maximum length is 1.");
                return;
            }

            string query = $"UPDATE {DBM_Guild.tableName} " +
                $" SET {DBM_Guild.Columns.custom_prefix}=@{DBM_Guild.Columns.custom_prefix} " +
                $" WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild} ";
            Dictionary<string, object> columns = new Dictionary<string, object>();
            columns[DBM_Guild.Columns.custom_prefix] = prefix.ToString();
            columns[DBM_Guild.Columns.id_guild] = idGuild.ToString();
            new DBC().update(query, columns);

            Config.Core.customPrefix[idGuild.ToString()] = prefix;

            if (prefix == "")
                await ReplyAsync("Custom prefix has removed.");
            else
                await ReplyAsync($"Custom prefix has been updated with **{prefix}**");
        }

        [Name("mod command"), Group("command"), Summary("These commands contains all custom command related. " +
        "Requires `manage roles permission`.")]
        public class DoremiModeratorCustomCommand : InteractiveBase
        {
            [Command("list"), Summary("List the custom command that has been added. " +
                "You can put the custom command as parameter to list all the custom command that has been added.")]
            public async Task listCustomCommand(string command="")
            {
                ulong guildId = Context.Guild.Id;
                if (command == "")
                {
                    string query = @$"select distinct({DBM_Guild_Custom_Command.Columns.command}) as {DBM_Guild_Custom_Command.Columns.command} 
                    from {DBM_Guild_Custom_Command.tableName} 
                    where {DBM_Guild_Custom_Command.Columns.id_guild}=@{DBM_Guild_Custom_Command.Columns.id_guild} 
                    order by {DBM_Guild_Custom_Command.Columns.command} asc";
                    Dictionary<string, object> columns = new Dictionary<string, object>();
                    columns[DBM_Guild_Custom_Command.Columns.id_guild] = guildId.ToString();
                    var results = new DBC().selectAll(query, columns);
                    if (results.Rows.Count <= 0)
                    {
                        await ReplyAsync($"There are no custom command that has been added yet"); return;
                    }
                    string customCommand = "";
                    foreach(DataRow row in results.Rows)
                        customCommand += $"`{row[DBM_Guild_Custom_Command.Columns.command]}`,";
                    
                    customCommand = customCommand.TrimEnd(',');
                    await ReplyAsync($"Custom command that has been added: {customCommand}");

                } else
                {
                    string query = $"SELECT * " +
                                $" FROM {DBM_Guild_Custom_Command.tableName} " +
                                $" WHERE {DBM_Guild_Custom_Command.Columns.id_guild}=@{DBM_Guild_Custom_Command.Columns.id_guild} AND " +
                                $" {DBM_Guild_Custom_Command.Columns.command}=@{DBM_Guild_Custom_Command.Columns.command}";
                    Dictionary<string, object> columns = new Dictionary<string, object>();
                    columns[DBM_Guild_Custom_Command.Columns.id_guild] = guildId.ToString();
                    columns[DBM_Guild_Custom_Command.Columns.command] = command;
                    var results = new DBC().selectAll(query, columns);

                    if (results.Rows.Count <= 0)
                    {
                        await ReplyAsync($"Sorry, I cannot find any custom command: **{command}**"); return;
                    }

                    List<string> pageContent = new List<string>();
                    string title = $"";

                    string tempVal = title;

                    var i = 0;
                    int currentIndex = 0;

                    foreach (DataRow row in results.Rows)
                    {
                        string id = row[DBM_Guild_Custom_Command.Columns.id].ToString();
                        string content = row[DBM_Guild_Custom_Command.Columns.content].ToString();
                        if (row[DBM_Guild_Custom_Command.Columns.content].ToString().Length >= 30)
                            content = $"{row[DBM_Guild_Custom_Command.Columns.content].ToString().Substring(0, 28)}...";

                        tempVal += $"{id}: {content}\n";

                        if (i == results.Rows.Count - 1)
                        {
                            pageContent.Add(tempVal);
                        }
                        else
                        {
                            if (currentIndex < 5) currentIndex++;
                            else
                            {
                                pageContent.Add(tempVal);
                                currentIndex = 0;
                                tempVal = title;
                            }
                        }
                        i++;
                    }

                    PaginatedAppearanceOptions pao = new PaginatedAppearanceOptions();
                    pao.JumpDisplayOptions = JumpDisplayOptions.Never;
                    pao.DisplayInformationIcon = false;
                    var pager = new PaginatedMessage
                    {
                        Title = $"**Custom Command: {command}**\n",
                        Pages = pageContent,
                        Color = Config.Doremi.EmbedColor,
                        Options = pao
                    };

                    await PagedReplyAsync(pager);
                }
            }

            [Command("add"), Summary("Add the custom command.")]
            public async Task addCustomCommand(string command, [Remainder] string content)
            {
                var idGuild = Context.Guild.Id;
                Dictionary<string, object> columns = new Dictionary<string, object>();
                columns[DBM_Guild_Custom_Command.Columns.id_guild] = idGuild.ToString();
                columns[DBM_Guild_Custom_Command.Columns.command] = command;
                columns[DBM_Guild_Custom_Command.Columns.content] = content;
                new DBC().insert(DBM_Guild_Custom_Command.tableName, columns);

                //get latest id
                string query = $"SELECT * " +
                $" FROM {DBM_Guild_Custom_Command.tableName} " +
                $" WHERE {DBM_Guild_Custom_Command.Columns.id_guild}=@{DBM_Guild_Custom_Command.Columns.id_guild} AND " +
                $" {DBM_Guild_Custom_Command.Columns.command}=@{DBM_Guild_Custom_Command.Columns.command} " +
                $" ORDER BY {DBM_Guild_Custom_Command.Columns.created_at} desc " +
                $" LIMIT 1";
                columns = new Dictionary<string, object>();
                columns[DBM_Guild_Custom_Command.Columns.id_guild] = idGuild.ToString();
                columns[DBM_Guild_Custom_Command.Columns.command] = command.ToString();
                var result = new DBC().selectAll(query, columns);
                string newId = "";
                foreach (DataRow row in result.Rows)
                    newId = row[DBM_Guild_Custom_Command.Columns.id].ToString();
                
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithTitle($"Custom command added with id: **{newId}**")
                    .Build());
            }

            [Command("remove"), Summary("Remove the custom command with **id** parameter.")]
            public async Task removeCustomCommand(string id)
            {
                var guildId = Context.Guild.Id;
                string query = $"SELECT * " +
                                $" FROM {DBM_Guild_Custom_Command.tableName} " +
                                $" WHERE {DBM_Guild_Custom_Command.Columns.id_guild}=@{DBM_Guild_Custom_Command.Columns.id_guild} AND " +
                                $" {DBM_Guild_Custom_Command.Columns.id}=@{DBM_Guild_Custom_Command.Columns.id}";
                Dictionary<string, object> columns = new Dictionary<string, object>();
                columns[DBM_Guild_Custom_Command.Columns.id_guild] = guildId.ToString();
                columns[DBM_Guild_Custom_Command.Columns.id] = Convert.ToInt32(id);
                var results = new DBC().selectAll(query, columns);
                if (results.Rows.Count > 0)
                {
                    query = $"DELETE FROM {DBM_Guild_Custom_Command.tableName} " +
                    $" WHERE {DBM_Guild_Custom_Command.Columns.id_guild}=@{DBM_Guild_Custom_Command.Columns.id_guild} AND " +
                    $" {DBM_Guild_Custom_Command.Columns.id}=@{DBM_Guild_Custom_Command.Columns.id} ";

                    columns = new Dictionary<string, object>();
                    columns[DBM_Guild_Custom_Command.Columns.id_guild] = guildId.ToString();
                    columns[DBM_Guild_Custom_Command.Columns.id] = Convert.ToInt32(id);
                    new DBC().delete(query, columns);
                    await ReplyAsync($"Custom command with id: **{id}** has been removed.");
                }
                else
                {
                    await ReplyAsync($"Cannot find custom command with id: **{id}**");
                }
            }

            [Command("view"), Summary("Preview the custom command with **id** parameter.")]
            public async Task previewCustomCommand(string id)
            {
                var isNumeric = int.TryParse(id, out _);
                if (!isNumeric)
                {
                    await ReplyAsync($"Please enter the id with number format.");
                    return;
                }

                var guildId = Context.Guild.Id;
                string query = $"SELECT * " +
                                $" FROM {DBM_Guild_Custom_Command.tableName} " +
                                $" WHERE {DBM_Guild_Custom_Command.Columns.id_guild}=@{DBM_Guild_Custom_Command.Columns.id_guild} AND " +
                                $" {DBM_Guild_Custom_Command.Columns.id}=@{DBM_Guild_Custom_Command.Columns.id}";
                Dictionary<string, object> columns = new Dictionary<string, object>();
                columns[DBM_Guild_Custom_Command.Columns.id_guild] = guildId.ToString();
                columns[DBM_Guild_Custom_Command.Columns.id] = Convert.ToInt32(id);
                var results = new DBC().selectAll(query, columns);
                if (results.Rows.Count > 0)
                {
                    foreach(DataRow row in results.Rows)
                    {
                        await ReplyAsync($"Preview of custom command with ID: **{id}** \n" +
                            row[DBM_Guild_Custom_Command.Columns.content].ToString());
                    }
                }
                else
                {
                    await ReplyAsync($"Cannot find custom command with id: **{id}**");
                }
            }

        }

        [Command("log"), Summary("Show the warning log.")]
        public async Task warningLog(SocketUser user)
        {
            var guildId = Context.Guild.Id;
            var userId = user.Id;
            EmbedBuilder eb = new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithTitle(user.Username)
                .WithDescription("Warning Log:")
                .WithThumbnailUrl(user.GetAvatarUrl());

            string query = $"SELECT * " +
                $" FROM {DBM_Guild_Warn_Log.tableName} " +
                $" WHERE {DBM_Guild_Warn_Log.Columns.id_guild}=@{DBM_Guild_Warn_Log.Columns.id_guild} AND " +
                $" {DBM_Guild_Warn_Log.Columns.id_user}=@{DBM_Guild_Warn_Log.Columns.id_user} " +
                $" ORDER BY {DBM_Guild_Warn_Log.Columns.created_at} asc ";
            Dictionary<string, object> columns = new Dictionary<string, object>();
            columns[DBM_Guild_Warn_Log.Columns.id_guild] = guildId.ToString();
            columns[DBM_Guild_Warn_Log.Columns.id_user] = userId.ToString();
            DBC db = new DBC();
            var result = db.selectAll(query, columns);
            if (result.Rows.Count >= 1)
            {
                var i = 1;
                foreach (DataRow row in result.Rows)
                {
                    eb.AddField($"{row[DBM_Guild_Warn_Log.Columns.created_at]}:",
                        row[DBM_Guild_Warn_Log.Columns.message]);
                    i++;
                }
            }
            else
            {
                eb.WithDescription("There are no warning log for this user.");
            }

            await ReplyAsync(embed: eb.Build());

        }

        [Command("warn"), Summary("Send a warning and put the user into detention role.")]
        public async Task warnMessage(SocketUser user, [Remainder] string message)
        {
            if (user.Id == Context.User.Id)
            {
                await ReplyAsync("You cannot send warning to yourself.");
            }
            ulong guildId = Context.Guild.Id;
            ulong userId = user.Id;
            var username = user.Username;
            var warning = "first";

            //get detention role 
            string detentionRole = Config.Guild.getGuildData(guildId)[DBM_Guild.Columns.role_detention].ToString();
            if (detentionRole != "" &&
                Context.Guild.Roles.Where(x => x.Name == detentionRole).ToList().Count >= 1)
            {
                string query = $"SELECT * " +
                $" FROM {DBM_Guild_Warn_Log.tableName} " +
                $" WHERE {DBM_Guild_Warn_Log.Columns.id_guild}=@{DBM_Guild_Warn_Log.Columns.id_guild} AND " +
                $" {DBM_Guild_Warn_Log.Columns.id_user}=@{DBM_Guild_Warn_Log.Columns.id_user} ";
                Dictionary<string, object> columns = new Dictionary<string, object>();
                columns[DBM_Guild_Warn_Log.Columns.id_guild] = guildId.ToString();
                columns[DBM_Guild_Warn_Log.Columns.id_user] = userId.ToString();
                var result = new DBC().selectAll(query, columns);
                int total = result.Rows.Count;
                int duration = 1;//in minutes
                EmbedBuilder eb = new EmbedBuilder();

                if (total <= 0)
                {
                    eb = new EmbedBuilder()
                    .WithTitle($"First warning!")
                    .WithDescription($"First warning, {username}! " +
                    $" Please read the rule on the group next time and make sure to always follow it to avoid this warning. ")
                    .AddField("Reason:", message)
                    .AddField("Penalty:", $"You have been placed in detention for {duration} minutes. " +
                    "Should your detention is not lifted within that amount of time you can DM one of the mods.")
                    .WithFooter($"From: {Context.Guild.Name}");
                }
                else if (total <= 0)
                {
                    duration = 2;
                    warning = "second";

                    eb = new EmbedBuilder()
                    .WithTitle($"Second warning!")
                    .WithDescription($"This is your second time for breaking the rule, {username}. " +
                    $"Again, please read the rule on the group next time and make sure to always follow it to avoid this warning. ")
                    .AddField("Reason:", message)
                    .AddField("Penalty:", $"You have been placed in detention for {duration} minutes. " +
                    "Should your detention is not lifted within that amount of time you can DM one of the mods.")
                    .WithFooter($"From: {Context.Guild.Name}");
                }
                else if (total <= 1)
                {
                    duration = 2;
                    warning = "final";
                    eb = new EmbedBuilder()
                    .WithTitle($"Final warning!")
                    .WithDescription($"This will be your final warning, {username} " +
                    $" and there will be no more next warning for you. " +
                    $" Again, please read the rule on the group and make sure to always follow it!")
                    .AddField("Reason:", message)
                    .AddField("Penalty:", $"You have been placed in detention for {duration} minutes. " +
                    "Should your detention is not lifted within that amount of time you can DM one of the mods.")
                    .WithFooter($"From: {Context.Guild.Name}");
                }
                else if (total >= 3)
                {
                    await Context.Guild.AddBanAsync(user, reason: message);
                    return;
                }

                //insert to database
                columns = new Dictionary<string, object>();
                columns[DBM_Guild_Warn_Log.Columns.id_guild] = guildId.ToString();
                columns[DBM_Guild_Warn_Log.Columns.id_user] = userId.ToString();
                columns[DBM_Guild_Warn_Log.Columns.message] = message;

                new DBC().insert(DBM_Guild_Warn_Log.tableName, columns);

                //set the detention role
                if (Context.Guild.Roles.Where(x => x.Name == detentionRole).ToList().Count >= 1)
                {
                    int finalDuration = (duration * 60) * 1000;
                    await Context.Guild.GetUser(userId).AddRoleAsync(
                        Context.Guild.Roles.First(x => x.Name == detentionRole)
                    );
                    //remove detention role
                    Timer _timerStatus = new Timer(async _ => {
                        await Context.Guild.GetUser(userId).RemoveRoleAsync(
                            Context.Guild.Roles.First(x => x.Name == detentionRole)
                        );
                    },
                    null,
                    finalDuration, //time to wait before executing the timer for the first time (set first status)
                    Timeout.Infinite//time to wait before executing the timer again (set new status - repeats indifinitely every 10 seconds)
                    );
                }

                var dmchannel = await user.GetOrCreateDMChannelAsync();

                await dmchannel.SendMessageAsync(embed: eb.Build());
                await ReplyAsync($"**{warning}** warning message to **{user.Username}** has been sent.");
            }
            else
            {
                await ReplyAsync($"Please set the detention role first with " +
                    $"**{Config.Doremi.PrefixParent[0]} mod role detention <role name>**");
            }
        }

        [Name("mod role react"), Group("role react"), Summary("These commands contains all self assignable role react list command. " +
        "Requires `manage roles permission`.")]
        public class DoremiModeratorRolesReact : InteractiveBase
        {
            [Command("info", RunMode = RunMode.Async), Summary("See the role react list that have been assigned on the given message link.")]
            public async Task seeSelfAssignableRoles(string messageLink = "")
            {
                var guildId = Context.Guild.Id;
                ulong messageId = 0;
                var channelId = 0;

                SocketRole roleSelection = null; GuildEmote emoteSelection = null;

                await Context.Message.DeleteAsync();

                if (messageLink == "")
                {
                    await ReplyAsync("Please enter the message link that you want to see.");
                }
                else
                {
                    try
                    {
                        var validChannelId = UInt64.TryParse(messageLink.Split('/')[5], out UInt64 _channelId);
                        var validMessageId = UInt64.TryParse(messageLink.Split('/').Last(), out UInt64 _messageId);
                        messageId = _messageId;
                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine(e.ToString());
                        await ReplyAsync("Sorry, that is not the valid message link.");
                        return;
                    }

                    string query = $"SELECT * " +
                    $" FROM {DBM_Guild_Role_React.tableName} " +
                    $" WHERE {DBM_Guild_Role_React.Columns.id_guild}=@{DBM_Guild_Role_React.Columns.id_guild} AND " +
                    $" {DBM_Guild_Role_React.Columns.id_message}=@{DBM_Guild_Role_React.Columns.id_message} ";
                    Dictionary<string, object> columns = new Dictionary<string, object>();
                    columns[DBM_Guild_Role_React.Columns.id_guild] = guildId;
                    columns[DBM_Guild_Role_React.Columns.id_message] = messageId.ToString();
                    var result = new DBC().selectAll(query, columns);

                    EmbedBuilder embed = new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor);

                    if (result.Rows.Count>=1)
                    {
                        foreach (DataRow row in result.Rows)
                        {
                            try
                            {
                                //check emote
                                string finalEmoteSelection = "**Missing emotes**";
                                if (GlobalFunctions.checkNonCustomEmojiMatched(
                                    row[DBM_Guild_Role_React.Columns.emoji].ToString()))
                                    finalEmoteSelection = new Discord.Emoji(row[DBM_Guild_Role_React.Columns.emoji].ToString()).ToString();
                                else
                                {
                                    emoteSelection = Context.Guild.Emotes.FirstOrDefault(x => x.ToString() == row[DBM_Guild_Role_React.Columns.emoji].ToString());
                                    if (emoteSelection != null)
                                    {
                                        finalEmoteSelection = emoteSelection.ToString();
                                    }
                                }

                                //check role
                                string finalRoleSelection = "**Missing roles**";

                                roleSelection = Context.Guild.Roles.FirstOrDefault(x => x.Id == Convert.ToUInt64(row[DBM_Guild_Role_React.Columns.id_role].ToString()));
                                if (roleSelection != null)
                                    finalRoleSelection = roleSelection.Mention;
                                string convertedMessageLink = $"[message link]({messageLink})";

                                await ReplyAsync(embed: new EmbedBuilder()
                                    .WithColor(Config.Doremi.EmbedColor)
                                    .AddField("Role:", finalRoleSelection, true)
                                    .AddField("Reaction:", finalEmoteSelection, true)
                                    .AddField("Message Link:", convertedMessageLink, true)
                                    .Build());
                            }
                            catch (Exception e)
                            {
                                //Console.WriteLine(e.ToString());
                            }
                        }
                    }
                    else
                    {
                        await ReplyAsync("No Role reaction data yet on this message link.");
                    }

                }
            }

            [Command("add", RunMode = RunMode.Async), Summary("Add role to self assignable role react list. " +
                "Parameter: **<message_link> <role> <emoji>**")]
            public async Task addSelfAssignableRoles(string messageLink,SocketRole role,string emoji)
            {
                var messageId = "";
                var guildId = Context.Guild.Id;
                IMessage imessage = null;
                GuildEmote emoteSelection = null;
                Discord.Emoji nonCustomEmoteSelection = null;

                var roleMaster = Context.Guild.Roles.FirstOrDefault(x => x.Id == Convert.ToUInt64(role.Id));
                if (!messageLink.StartsWith("https://"))
                {
                    await ReplyAsync("Sorry, that is not the valid message format.");
                    return;
                }
                else if (roleMaster == null)
                {
                    await ReplyAsync("Please enter the correct role.");
                    return;
                } else if (!GlobalFunctions.checkNonCustomEmojiMatched(emoji))
                {
                    //custom emoji
                    try
                    {
                        var emojiMaster = Context.Guild.Emotes.FirstOrDefault(x => x.ToString() == emoji);
                        if (emojiMaster.Name.ToString()=="")
                        {
                            await ReplyAsync("Please enter the correct emotes.");
                            return;
                        }
                    } catch(Exception e)
                    {
                        await ReplyAsync("Please enter the correct emoji.");
                        return;
                    }
                }

                try
                {
                    var validChannelId = UInt64.TryParse(messageLink.Split('/')[5], out UInt64 _channelId);
                    messageId = messageLink.Split('/').Last();
                    imessage = await Context.Client
                            .GetGuild(guildId)
                            .GetTextChannel(_channelId)
                            .GetMessageAsync(Convert.ToUInt64(messageId));
                }
                catch
                {
                    await ReplyAsync(embed: new EmbedBuilder()
                            .WithDescription(":x: Sorry, that is not the valid message link.")
                            .Build());
                    return;
                }

                if (GlobalFunctions.checkNonCustomEmojiMatched(emoji))
                {
                    nonCustomEmoteSelection = new Discord.Emoji(emoji);
                } else
                {
                    emoteSelection = Context.Guild.Emotes.FirstOrDefault(x => x.ToString() == emoji);
                }

                Dictionary<string, object> columns = new Dictionary<string, object>();
                string querySearch = $"SELECT * " +
                    $" FROM {DBM_Guild_Role_React.tableName} " +
                    $" WHERE {DBM_Guild_Role_React.Columns.id_guild}=@{DBM_Guild_Role_React.Columns.id_guild} AND " +
                    $" {DBM_Guild_Role_React.Columns.id_message}=@{DBM_Guild_Role_React.Columns.id_message} AND " +
                    $" {DBM_Guild_Role_React.Columns.emoji}=@{DBM_Guild_Role_React.Columns.emoji} ";
                columns[DBM_Guild_Role_React.Columns.id_guild] = guildId.ToString();
                columns[DBM_Guild_Role_React.Columns.id_message] = messageId.ToString();
                columns[DBM_Guild_Role_React.Columns.emoji] = emoji;
                var result = new DBC().selectAll(querySearch,columns);
                if (result.Rows.Count >=1)
                {
                    await ReplyAsync("Sorry, you already entered that same emoji & message Id.");
                } else
                {
                    //insert
                    columns = new Dictionary<string, object>();
                    columns[DBM_Guild_Role_React.Columns.id_guild] = guildId.ToString();
                    columns[DBM_Guild_Role_React.Columns.id_message] = messageId.ToString();
                    columns[DBM_Guild_Role_React.Columns.id_role] = role.Id.ToString();
                    
                    EmbedBuilder eb = new EmbedBuilder()
                        .WithColor(Config.Doremi.EmbedColor)
                        .WithTitle($"Role reaction successfully added")
                        .AddField("Roles:",MentionUtils.MentionRole(role.Id),true)
                        .AddField("Emotes:", emoji, true)
                        .WithFooter($"Msg ID: {messageId}");

                    if (nonCustomEmoteSelection != null)
                    {
                        await imessage.AddReactionAsync(nonCustomEmoteSelection);
                        string hexValue = "";
                        for (var i = 0; i < emoji.ToString().Length; i += char.IsSurrogatePair(emoji.ToString(), i) ? 2 : 1)
                        {
                            var decValue = char.ConvertToUtf32(emoji.ToString(), i);
                            hexValue += "+"+decValue.ToString("X");
                        }

                        columns[DBM_Guild_Role_React.Columns.emoji] = hexValue;
                    }
                    else
                    {
                        await imessage.AddReactionAsync(emoteSelection);
                        columns[DBM_Guild_Role_React.Columns.emoji] = emoji.ToString();
                    }

                    new DBC().insert(DBM_Guild_Role_React.tableName, columns);
                    await ReplyAsync(embed:eb.Build());
                }

            }

        }

        [Name("mod welcome"), Group("welcome"), Summary("These commands contains all welcome message that can be set. " +
        "Requires `manage channel permission`.")]
        public class DoremiModeratorWelcome : InteractiveBase
        {
            [Command("remove"), Summary("Remove all welcome message settings")]
            public async Task removeWelcomeMessageSettings()
            {
                ulong guildId = Context.Guild.Id;
                string query = $"UPDATE {DBM_Guild.tableName} " +
                    $" SET {DBM_Guild.Columns.welcome_title}=@{DBM_Guild.Columns.welcome_title}, " +
                    $" {DBM_Guild.Columns.welcome_message}=@{DBM_Guild.Columns.welcome_message}, " +
                    $" {DBM_Guild.Columns.welcome_image}=@{DBM_Guild.Columns.welcome_image} " +
                    $" WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild}";
                Dictionary<string, object> column = new Dictionary<string, object>();
                column[DBM_Guild.Columns.welcome_title] = "";
                column[DBM_Guild.Columns.welcome_message] = "";
                column[DBM_Guild.Columns.welcome_image] = "";
                column[DBM_Guild.Columns.id_guild] = guildId.ToString();
                new DBC().update(query, column);
                await ReplyAsync($"Welcome message announcement settings has been removed.");
            }

            [Command("color"), Summary("Set the embed welcome message color")]
            public async Task setWelcomeColor(string r, string g, string b)
            {
                var isNumericR = int.TryParse(r, out _);
                var isNumericG = int.TryParse(g, out _);
                var isNumericB = int.TryParse(b, out _);

                if (!isNumericR && !isNumericG && !isNumericB)
                {
                    await ReplyAsync("Please enter rgb values between 0 and 254"); return;
                }

                if (Convert.ToInt32(r) < 0 || Convert.ToInt32(r) > 254 ||
                Convert.ToInt32(g) < 0 || Convert.ToInt32(g) > 254 ||
                Convert.ToInt32(b) < 0 || Convert.ToInt32(b) > 254)
                {
                    await ReplyAsync("Please enter rgb values between 0 and 254"); return;
                }

                ulong guildId = Context.Guild.Id;
                string query = $"UPDATE {DBM_Guild.tableName} " +
                    $" SET {DBM_Guild.Columns.welcome_color}=@{DBM_Guild.Columns.welcome_color} " +
                    $" WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild}";
                Dictionary<string, object> column = new Dictionary<string, object>();
                column[DBM_Guild.Columns.welcome_color] = $"{r},{g},{b}";
                column[DBM_Guild.Columns.id_guild] = guildId.ToString();
                new DBC().update(query, column);

                await ReplyAsync("Welcome message embed color has been set");

            }

            [Command("title"), Summary("Set the welcome message title announcement. " +
            "Some available built-in parameter that can be used:\n" +
            "**$servername$:** give the server name. **$user$:** mention the username. " +
            "If the parameter is blank it'll be removed.")]
            public async Task setWelcomeHeaderAnnouncementMessage([Remainder] string headerMessage = "")
            {
                ulong guildId = Context.Guild.Id;
                string query = $"UPDATE {DBM_Guild.tableName} " +
                    $" SET {DBM_Guild.Columns.welcome_title}=@{DBM_Guild.Columns.welcome_title} " +
                    $" WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild}";
                Dictionary<string, object> column = new Dictionary<string, object>();
                column[DBM_Guild.Columns.welcome_title] = headerMessage;
                column[DBM_Guild.Columns.id_guild] = guildId.ToString();
                new DBC().update(query, column);

                if(headerMessage=="")
                    await ReplyAsync($"Welcome title message announcement has been removed.");
                else
                    await ReplyAsync($"Welcome title message announcement has been updated.");
            }

            [Command("message"), Summary("Set the welcome message content announcement. " +
                "Some available built-in parameter that can be used:\n" +
                "**$servername$:** give the server name. **$user$:** mention the user. " +
                "**#channelid>** can be placed with any channel that you want. " +
                "If the parameter is blank it'll be removed.")]
            public async Task setWelcomeAnnouncementMessage([Remainder] string message = "")
            {
                ulong guildId = Context.Guild.Id;
                string query = $"UPDATE {DBM_Guild.tableName} " +
                    $" SET {DBM_Guild.Columns.welcome_message}=@{DBM_Guild.Columns.welcome_message} " +
                    $" WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild}";
                Dictionary<string, object> column = new Dictionary<string, object>();
                column[DBM_Guild.Columns.welcome_message] = message;
                column[DBM_Guild.Columns.id_guild] = guildId.ToString();
                new DBC().update(query, column);

                if (message=="")
                    await ReplyAsync($"Welcome message announcement has been removed.");
                 else
                    await ReplyAsync($"Welcome message announcement has been updated.");
                
                
            }

            [Command("image"), Summary("Set the welcome image url. If the parameter is blank it'll be removed.")]
            public async Task setWelcomeAnnouncementImage(string imageurl = "")
            {
                ulong guildId = Context.Guild.Id;
                string query = $"UPDATE {DBM_Guild.tableName} " +
                    $" SET {DBM_Guild.Columns.welcome_image}=@{DBM_Guild.Columns.welcome_image} " +
                    $" WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild}";
                Dictionary<string, object> column = new Dictionary<string, object>();
                column[DBM_Guild.Columns.welcome_image] = imageurl;
                column[DBM_Guild.Columns.id_guild] = guildId.ToString();
                new DBC().update(query, column);

                if (imageurl == "")
                    await ReplyAsync($"Welcome image announcement has been removed.");
                else
                    await ReplyAsync($"Welcome image announcement has been updated.");
            }

            [Command("view"), Summary("Display the current custom welcome message template")]
            public async Task viewWelcomeMessage()
            {
                ulong guildId = Context.Guild.Id;
                var guildData = Config.Guild.getGuildData(guildId);

                string welcomeTitle = guildData[DBM_Guild.Columns.welcome_title].ToString();
                string welcomeMessage = guildData[DBM_Guild.Columns.welcome_message].ToString();
                string welcomeImage = guildData[DBM_Guild.Columns.welcome_image].ToString();

                if (welcomeMessage != "")
                {
                    var channel = Bot.Doremi.client.GetChannel(
                    Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_notification_user_welcome].ToString()))
                    as SocketTextChannel;

                    if (channel != null)
                    {
                        EmbedBuilder eb = new EmbedBuilder();
                        if (guildData[DBM_Guild.Columns.welcome_color].ToString() != "")
                        {
                            string[] welcomeColor = guildData[DBM_Guild.Columns.welcome_color].ToString().Split(",");
                            int colorR = Convert.ToInt32(welcomeColor[0]);
                            int colorG = Convert.ToInt32(welcomeColor[1]);
                            int colorB = Convert.ToInt32(welcomeColor[2]);
                            eb = eb.WithColor(colorR, colorG, colorB);
                        }
                        else
                        {
                            eb = eb.WithColor(Config.Doremi.EmbedColor);
                        }

                        if (welcomeTitle != "")
                        {
                            eb = eb.WithTitle(welcomeTitle);
                        }

                        if (welcomeImage != "")
                        {
                            eb = eb.WithImageUrl(welcomeImage);
                        }

                        welcomeMessage = welcomeMessage.Replace("#", "<#");
                        welcomeMessage = welcomeMessage.Replace("$servername$", channel.Guild.Name);

                        await ReplyAsync(
                            $"Welcome message will be announced at {channel.Mention}",
                            embed: eb
                            .WithDescription(welcomeMessage)
                            .Build());
                    } else
                    {
                        await ReplyAsync("I can't find the welcome message announcement channel. " +
                            "Please set the welcome announcement channel first.");
                    }
                } else
                {
                    await ReplyAsync("Welcome message is not set yet.");
                }

            }
        }

        [Name("mod autorole"), Group("autorole"), Summary("These commands contains all autorole command. " +
        "Requires `manage roles permission`.")]
        public class DoremiModeratorAutoRoles : InteractiveBase
        {
            [Command("join"), Summary("Set the autorole when user joined the guild. " +
                "Make sure the roles that will be applied are in lower positions than the bots role. " +
                "If the parameter is empty then it will be removed.")]
            public async Task setAutoroleJoinGuild(SocketRole role=null)
            {
                var guildId = Context.Guild.Id;
                var embed = new EmbedBuilder().WithColor(Config.Doremi.EmbedColor);

                string query = $"UPDATE {DBM_Guild.tableName} " +
                    $" SET {DBM_Guild.Columns.id_autorole_user_join}=@{DBM_Guild.Columns.id_autorole_user_join} " +
                    $" WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild} ";
                //check if level & role exists/not
                Dictionary<string, object> columns = new Dictionary<string, object>();
                columns[DBM_Guild.Columns.id_guild] = guildId;
                
                if (role == null)
                {
                    columns[DBM_Guild.Columns.id_autorole_user_join] = "";
                    await ReplyAsync(embed: embed
                    .WithDescription($"**User join** autorole has been removed.")
                    .Build());
                } else
                {
                    columns[DBM_Guild.Columns.id_autorole_user_join] = role.Id.ToString();
                    await ReplyAsync(embed: embed
                    .WithDescription($"**User join** autorole has been set with " +
                    $" {MentionUtils.MentionRole(Convert.ToUInt64(role.Id))}.")
                    .Build());
                }

                new DBC().update(query, columns);
            }

            [Command("add"), Summary("Add the autorole with minimum level parameter. " +
                "Make sure the roles that will be applied are in lower positions than the bots role.")]
            public async Task setAutoroleLevel(int level_min, SocketRole role)
            {
                var guildId = Context.Guild.Id;
                var eb = new EmbedBuilder().WithColor(Config.Doremi.EmbedColor);
                var roleSearch = Context.Guild.Roles.FirstOrDefault(x => x.Id == Convert.ToUInt64(role.Id));
                if (roleSearch == null)
                    await ReplyAsync($"Sorry, I can't find that role.");
                else
                {
                    string query = $"SELECT * " +
                        $" FROM {DBM_Guild_Autorole_Level.tableName} " +
                        $" WHERE {DBM_Guild_Autorole_Level.Columns.id_guild}=@{DBM_Guild_Autorole_Level.Columns.id_guild} AND " +
                        $" {DBM_Guild_Autorole_Level.Columns.id_role}=@{DBM_Guild_Autorole_Level.Columns.id_role} AND " +
                        $" {DBM_Guild_Autorole_Level.Columns.level_min}=@{DBM_Guild_Autorole_Level.Columns.level_min} ";
                    //check if level & role exists/not
                    Dictionary<string, object> columns = new Dictionary<string, object>();
                    columns[DBM_Guild_Autorole_Level.Columns.id_guild] = guildId.ToString();
                    columns[DBM_Guild_Autorole_Level.Columns.id_role] = role.Id.ToString();
                    columns[DBM_Guild_Autorole_Level.Columns.level_min] = level_min;
                    var result = new DBC().selectAll(query,columns);
                    if (result.Rows.Count <= 0)
                    {
                        new DBC().insert(DBM_Guild_Autorole_Level.tableName, columns);

                        await ReplyAsync(embed: eb
                        .WithDescription($"Auto role level {level_min} has been set with " +
                        $" {MentionUtils.MentionRole(Convert.ToUInt64(roleSearch.Id))}.")
                        .Build());
                    } else
                    {
                        await ReplyAsync("Sorry, this level & role already existed set.");
                    }
                }
            }

            [Command("remove"), Summary("Remove the autorole with the minimum level parameter.")]
            public async Task removeAutoroleLevel(int level_min)
            {
                var guildId = Context.Guild.Id;
                var embed = new EmbedBuilder().WithColor(Config.Doremi.EmbedColor);
                
                //check if level & role exists/not
                string query = $"DELETE FROM {DBM_Guild_Autorole_Level.tableName} " +
                    $" WHERE {DBM_Guild_Autorole_Level.Columns.id_guild}=@{DBM_Guild_Autorole_Level.Columns.id_guild} AND " +
                    $" {DBM_Guild_Autorole_Level.Columns.id_role}=@{DBM_Guild_Autorole_Level.Columns.id_role} AND " +
                    $" {DBM_Guild_Autorole_Level.Columns.level_min}=@{DBM_Guild_Autorole_Level.Columns.level_min} ";
                Dictionary<string, object> columns = new Dictionary<string, object>();
                columns[DBM_Guild_Autorole_Level.Columns.id_guild] = guildId.ToString();
                columns[DBM_Guild_Autorole_Level.Columns.level_min] = level_min;
                new DBC().delete(query, columns);

                await ReplyAsync(embed: embed
                .WithDescription($"Autorole level {level_min} has been removed.")
                .Build());
                
            }

            [Command("view"), Summary("View the current active autorole for new user that has join.")]
            public async Task viewAutoroleJoinGuild() {
                //current autorole settings:
                ulong guildId = Context.Guild.Id;
                var guildData = Config.Guild.getGuildData(guildId);

                if (guildData[DBM_Guild.Columns.id_autorole_user_join].ToString() != "")
                {
                    var eb = new EmbedBuilder().WithColor(Config.Doremi.EmbedColor);
                    var roleSearch = Context.Guild.Roles.FirstOrDefault(x => x.Id == 
                    Convert.ToUInt64(guildData[DBM_Guild.Columns.id_autorole_user_join].ToString()));
                    if (roleSearch != null)
                        await ReplyAsync(embed: eb
                            .WithDescription($"Current active autorole for new user: " +
                            $"{MentionUtils.MentionRole(roleSearch.Id)}")
                            .Build());
                    else
                        await ReplyAsync("Autorole for new user is not set yet.");
                } else
                {
                    await ReplyAsync("Autorole for new user is not set yet.");
                }
            }
        }

        [Name("mod role"), Group("role"), Summary("These commands contains all self assignable role list command. " +
        "Requires `manage roles permission`.")]
        public class DoremiModeratorRoles : InteractiveBase
        {
            [Command("detention"), Summary("Add the detention role.")]
            public async Task addWarnRole(SocketRole role)
            {
                var guildId = Context.Guild.Id;
                var embed = new EmbedBuilder().WithColor(Config.Doremi.EmbedColor);
                var roleSearch = Context.Guild.Roles.FirstOrDefault(x => x.Id == Convert.ToUInt64(role.Id));
                if (roleSearch == null)
                    await ReplyAsync($"Sorry, I can't find that role.");
                else
                {
                    string query = $"UPDATE {DBM_Guild.tableName} " +
                        $" SET {DBM_Guild.Columns.role_detention}=@{DBM_Guild.Columns.role_detention}" +
                        $" WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild}";
                    Dictionary<string, object> columns = new Dictionary<string, object>();
                    columns[DBM_Guild.Columns.role_detention] = role.Id.ToString();
                    columns[DBM_Guild.Columns.id_guild] = guildId.ToString();
                    new DBC().update(query, columns);

                    await ReplyAsync(embed: embed
                    .WithDescription($"Detention role has been set into: {MentionUtils.MentionRole(Convert.ToUInt64(roleSearch.Id))}.")
                    .Build());
                }
            }

            [Command("cardcatcher"), Summary("Add card catcher roles for ojamajo trading card. " +
                "Parameter need to be filled with the mentioned role/role Id.")]
            public async Task addCardCatcherRoles(SocketRole role=null)
            {
                var guildId = Context.Guild.Id;
                var embed = new EmbedBuilder().WithColor(Config.Doremi.EmbedColor);
                var roleSearch = Context.Guild.Roles.FirstOrDefault(x => x.Id == Convert.ToUInt64(role.Id));
                if (roleSearch == null)
                    await ReplyAsync($"Sorry, I can't find that role id.");
                else
                {
                    string query = $"UPDATE {DBM_Trading_Card_Guild.tableName} " +
                        $" SET {DBM_Trading_Card_Guild.Columns.id_card_catcher}=@{DBM_Trading_Card_Guild.Columns.id_card_catcher}" +
                        $" WHERE {DBM_Trading_Card_Guild.Columns.id_guild}=@{DBM_Trading_Card_Guild.Columns.id_guild}";
                    Dictionary<string, object> columns = new Dictionary<string, object>();
                    columns[DBM_Trading_Card_Guild.Columns.id_card_catcher] = role.Id.ToString();
                    columns[DBM_Trading_Card_Guild.Columns.id_guild] = guildId.ToString();
                    new DBC().update(query, columns);

                    await ReplyAsync(embed: embed
                    .WithDescription($"Card catcher role has been set into: {MentionUtils.MentionRole(Convert.ToUInt64(roleSearch.Id))}.")
                    .Build());
                }
            }

            [Command("remove"), Summary("Remove role settings. " +
                "Available parameter: **detention/cardcatcher**")]
            public async Task removeRoleConfig(string config)
            {
                
                var guildId = Context.Guild.Id;
                var embed = new EmbedBuilder().WithColor(Config.Doremi.EmbedColor);

                string query = $"";
                Dictionary<string, object> columns = new Dictionary<string, object>();
                columns[DBM_Trading_Card_Guild.Columns.id_guild] = guildId.ToString();

                switch (config)
                {
                    case "detention":
                        query = $"UPDATE {DBM_Guild.tableName} " +
                        $" SET {DBM_Guild.Columns.role_detention}=@{DBM_Guild.Columns.role_detention}" +
                        $" WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild}";
                        await ReplyAsync(embed: embed
                        .WithDescription($"Detention role has been removed.")
                        .Build());
                        break;
                    case "cardcatcher":
                        query = $"UPDATE {DBM_Trading_Card_Guild.tableName} " +
                        $" SET {DBM_Trading_Card_Guild.Columns.id_card_catcher}=@{DBM_Trading_Card_Guild.Columns.id_card_catcher}" +
                        $" WHERE {DBM_Trading_Card_Guild.Columns.id_guild}=@{DBM_Trading_Card_Guild.Columns.id_guild}";
                        await ReplyAsync(embed: embed
                        .WithDescription($"Card catcher role has been removed.")
                        .Build());
                        break;
                    default:
                        await ReplyAsync($"Sorry, I can't find that settings.");
                        return;
                }

                new DBC().update(query, columns);
            }

        }

        [Command("server info"), Summary("Give the Server Information")]
        public async Task getServerInfo()
        {
            await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithTitle(Context.Guild.Name)
                .AddField("Created at:",
                $"{Context.Guild.CreatedAt.Day}/{Context.Guild.CreatedAt.Month}/{Context.Guild.CreatedAt.Year}")
                .AddField("Guild ID:", Context.Guild.Id.ToString())
                .Build());
        }

        [Name("mod birthday"), Group("birthday"), Summary("These commands require `Manage Channel` permissions.")]
        public class DoremiModeratorBirthday : InteractiveBase
        {
            [Command("ojamajo"), Alias("ojamajos"), Summary("Set the ojamajo birthday announcement with parameter: **on** or **off**.")]
            public async Task updateOjamajoBirthdayAnnouncement(string settings)
            {
                var idGuild = Context.Guild.Id;
                if (settings != "on" && settings != "off")
                {
                    await ReplyAsync("Please enter the parameter with **on** or **off**");
                    return;
                }

                string query = $"UPDATE {DBM_Guild.tableName} " +
                    $" SET {DBM_Guild.Columns.birthday_announcement_ojamajo}=@{DBM_Guild.Columns.birthday_announcement_ojamajo} " +
                    $" WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild}";
                Dictionary<string, object> columns = new Dictionary<string, object>();
                columns[DBM_Guild.Columns.id_guild] = idGuild.ToString();
                columns[DBM_Guild.Columns.birthday_announcement_ojamajo] = Convert.ToInt32(settings.Replace("on", "1").Replace("off", "0"));
                new DBC().update(query, columns);

                await ReplyAsync($"Ojamajo birthday announcement has been set into **{settings}**");
            }

            [Command("add"), Summary("Add new custom birthday message announcement. " +
                "Parameter that must be filled with: `Image Url` `message`")]
            public async Task addCustomBirthdayMessage(string imageUrl, [Remainder] string message)
            {
                var guildId = Context.Guild.Id;
                Dictionary<string, object> columns = new Dictionary<string, object>();
                columns[DBM_Guild_Custom_Birthday.Columns.id_guild] = guildId.ToString();
                columns[DBM_Guild_Custom_Birthday.Columns.img_url] = imageUrl;
                columns[DBM_Guild_Custom_Birthday.Columns.message] = message;
                new DBC().insert(DBM_Guild_Custom_Birthday.tableName, columns);

                EmbedBuilder eb = new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithTitle($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy Birthday!")
                    .WithDescription(message)
                    .WithImageUrl(imageUrl);

                //get latest id
                string query = $"SELECT * " +
                    $" FROM {DBM_Guild_Custom_Birthday.tableName} " +
                    $" WHERE {DBM_Guild_Custom_Birthday.Columns.id_guild}=@{DBM_Guild_Custom_Birthday.Columns.id_guild} " +
                    $" ORDER BY {DBM_Guild_Custom_Birthday.Columns.created_at} desc " +
                    $" LIMIT 1";
                columns = new Dictionary<string, object>();
                columns[DBM_Guild_Custom_Birthday.Columns.id_guild] = guildId.ToString();
                var results = new DBC().selectAll(query, columns);

                foreach (DataRow row in results.Rows)
                {
                    await ReplyAsync($"Custom birthday with id: **{row[DBM_Guild_Custom_Birthday.Columns.id].ToString()}** has been added with this preview:",
                    embed: eb
                    .WithFooter($"Best wishes from: {Context.Guild.Name} & friends")
                    .Build());
                }

            }

            [Command("list"), Summary("List all custom birthday announcement that has been added.")]
            public async Task listCustomBirthdayMessage()
            {
                ulong guildId = Context.Guild.Id;
                string query = $"SELECT * " +
                                $" FROM {DBM_Guild_Custom_Birthday.tableName} " +
                                $" WHERE {DBM_Guild_Custom_Birthday.Columns.id_guild}=@{DBM_Guild_Custom_Command.Columns.id_guild}";
                Dictionary<string, object> columns = new Dictionary<string, object>();
                columns[DBM_Guild_Custom_Command.Columns.id_guild] = guildId.ToString();
                var results = new DBC().selectAll(query, columns);

                List<string> pageContent = new List<string>();

                string title = $"";

                string tempVal = title;

                var i = 0;
                int currentIndex = 0;

                foreach (DataRow row in results.Rows)
                {
                    string id = row[DBM_Guild_Custom_Birthday.Columns.id].ToString();
                    string content = row[DBM_Guild_Custom_Birthday.Columns.message].ToString();
                    if (row[DBM_Guild_Custom_Birthday.Columns.message].ToString().Length >= 30)
                        content = $"{row[DBM_Guild_Custom_Birthday.Columns.message].ToString().Substring(0, 25)}...";

                    tempVal += $"{id}: {content}\n";

                    if (i == results.Rows.Count - 1)
                    {
                        pageContent.Add(tempVal);
                    }
                    else
                    {
                        if (currentIndex < 5) currentIndex++;
                        else
                        {
                            pageContent.Add(tempVal);
                            currentIndex = 0;
                            tempVal = title;
                        }
                    }
                    i++;
                }

                PaginatedAppearanceOptions pao = new PaginatedAppearanceOptions();
                pao.JumpDisplayOptions = JumpDisplayOptions.Never;
                pao.DisplayInformationIcon = false;
                var pager = new PaginatedMessage
                {
                    Title = $"**Custom Birthday Command List:**\n",
                    Pages = pageContent,
                    Color = Config.Doremi.EmbedColor,
                    Options = pao
                };

                if (results.Rows.Count >= 1)
                {
                    await PagedReplyAsync(pager);
                }
                else
                {
                    await ReplyAsync("There are no custom birthday message yet.");
                }

            }

            [Command("remove"), Summary("Remove the custom birthday announcement with the ID parameter")]
            public async Task removeCustomBirthdayMessage(string id)
            {
                var isNumeric = int.TryParse(id, out int n);
                if (!isNumeric)
                {
                    await ReplyAsync("Please enter the id in number format."); return;
                }

                ulong guildId = Context.Guild.Id;
                string query = $"SELECT * " +
                                $" FROM {DBM_Guild_Custom_Birthday.tableName} " +
                                $" WHERE {DBM_Guild_Custom_Birthday.Columns.id_guild}=@{DBM_Guild_Custom_Command.Columns.id_guild} AND " +
                                $" {DBM_Guild_Custom_Birthday.Columns.id}=@{DBM_Guild_Custom_Command.Columns.id} ";
                Dictionary<string, object> columns = new Dictionary<string, object>();
                columns[DBM_Guild_Custom_Command.Columns.id_guild] = guildId.ToString();
                columns[DBM_Guild_Custom_Command.Columns.id] = Convert.ToInt32(id);
                var results = new DBC().selectAll(query, columns);
                if (results.Rows.Count <= 0)
                    await ReplyAsync("I can't find that custom birthday ID.");
                else
                {
                    new DBC().delete(query, columns);
                    await ReplyAsync($"Custom birthday ID: **{id}** has been removed");
                }
            }

            [Command("view"), Summary("Preview the custom birthday announcement with the ID parameter")]
            public async Task viewCustomBirthdayMessage(string id)
            {
                var isNumeric = int.TryParse(id, out int n);
                if (!isNumeric)
                {
                    await ReplyAsync("Please enter the id in number format."); return;
                }

                ulong guildId = Context.Guild.Id;
                string query = $"SELECT * " +
                                $" FROM {DBM_Guild_Custom_Birthday.tableName} " +
                                $" WHERE {DBM_Guild_Custom_Birthday.Columns.id_guild}=@{DBM_Guild_Custom_Command.Columns.id_guild} AND " +
                                $" {DBM_Guild_Custom_Birthday.Columns.id}=@{DBM_Guild_Custom_Command.Columns.id} ";
                Dictionary<string, object> columns = new Dictionary<string, object>();
                columns[DBM_Guild_Custom_Command.Columns.id_guild] = guildId.ToString();
                columns[DBM_Guild_Custom_Command.Columns.id] = Convert.ToInt32(id);
                var results = new DBC().selectAll(query, columns);
                if (results.Rows.Count <= 0)
                    await ReplyAsync("I can't find that custom birthday ID.");
                else
                {
                    foreach (DataRow row in results.Rows)
                    {
                        await ReplyAsync(embed:new EmbedBuilder()
                        .WithColor(Config.Doremi.EmbedColor)
                        .WithTitle($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy Birthday!")
                        .WithDescription(row[DBM_Guild_Custom_Birthday.Columns.message].ToString())
                        .WithImageUrl(row[DBM_Guild_Custom_Birthday.Columns.img_url].ToString())
                        .WithFooter($"Best wishes from: {Context.Guild.Name} & friends")
                        .Build());
                    }
                    

                }
            }
        }

        [Name("mod channel"), Group("channel"), Summary("These commands require `Manage Channel` permissions.")]
        public class DoremiModeratorChannels : ModuleBase<SocketCommandContext>
        {
            //archived for now
            //[Command("birthday"), Summary("Set Doremi Bot to make birthday announcement on <channel_name>.")]
            //public async Task assignBirthdayChannel(SocketGuildChannel channel_name)
            //{
            //    var guildId = channel_name.Guild.Id;
            //    var socketClient = Context.Client;
                
            //    //start update
            //    string queryUpdate = $"UPDATE {DBM_Guild.tableName} " +
            //        $" SET {DBM_Guild.Columns.id_channel_birthday_announcement}=@{DBM_Guild.Columns.id_channel_birthday_announcement} " +
            //        $" WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild} ";
            //    Dictionary<string, object> columns = new Dictionary<string, object>();
            //    columns[DBM_Guild.Columns.id_channel_birthday_announcement] = channel_name.Id.ToString();
            //    columns[DBM_Guild.Columns.id_guild] = guildId.ToString();
            //    new DBC().update(queryUpdate, columns);

            //    if (Config.Doremi._timerBirthdayAnnouncement.ContainsKey(guildId.ToString()))
            //        Config.Doremi._timerBirthdayAnnouncement[guildId.ToString()].Change(Timeout.Infinite, Timeout.Infinite);
            //    if (Config.Hazuki._timerBirthdayAnnouncement.ContainsKey(guildId.ToString()))
            //        Config.Hazuki._timerBirthdayAnnouncement[guildId.ToString()].Change(Timeout.Infinite, Timeout.Infinite);
            //    if (Config.Aiko._timerBirthdayAnnouncement.ContainsKey(guildId.ToString()))
            //        Config.Aiko._timerBirthdayAnnouncement[guildId.ToString()].Change(Timeout.Infinite, Timeout.Infinite);
            //    if (Config.Onpu._timerBirthdayAnnouncement.ContainsKey(guildId.ToString()))
            //        Config.Onpu._timerBirthdayAnnouncement[guildId.ToString()].Change(Timeout.Infinite, Timeout.Infinite);
            //    if (Config.Momoko._timerBirthdayAnnouncement.ContainsKey(guildId.ToString()))
            //        Config.Momoko._timerBirthdayAnnouncement[guildId.ToString()].Change(Timeout.Infinite, Timeout.Infinite);

            //    var guildData = Config.Guild.getGuildData(guildId);

            //    string guildBirthdayLastAnnouncement = "";
            //    if (guildData[DBM_Guild.Columns.birthday_announcement_date_last].ToString() != "")
            //        guildBirthdayLastAnnouncement = guildData[DBM_Guild.Columns.birthday_announcement_date_last].ToString();
            //    else
            //        guildBirthdayLastAnnouncement = "1";

            //    //set doremi timer
            //    Config.Doremi._timerBirthdayAnnouncement[guildId.ToString()] = new Timer(async _ =>
            //    {
            //        guildData = Config.Guild.getGuildData(guildId);
            //        if (guildData[DBM_Guild.Columns.birthday_announcement_date_last].ToString() != "")
            //            guildBirthdayLastAnnouncement = guildData[DBM_Guild.Columns.birthday_announcement_date_last].ToString();
            //        else
            //            guildBirthdayLastAnnouncement = "1";

            //        EmbedBuilder eb = new EmbedBuilder()
            //        .WithColor(Config.Doremi.EmbedColor);

            //        //set birthday announcement timer
            //        if (guildBirthdayLastAnnouncement != DateTime.Now.ToString("dd"))
            //        {
            //            Boolean birthdayExisted = false;

            //            //announce hazuki birthday
            //            if (Config.Hazuki.Status.isBirthday() &&
            //            Convert.ToInt32(guildData[DBM_Guild.Columns.birthday_announcement_ojamajo]) == 1)
            //            {
            //                eb = eb.WithTitle($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy Birthday, Hazuki Chan!")
            //                .WithDescription($"Happy birthday to you, Hazuki chan. " +
            //                $"She has turned into {Config.Hazuki.birthdayCalculatedYear} this year. Let's give wonderful birthday wishes for her.")
            //                .WithImageUrl("https://cdn.discordapp.com/attachments/706770454697738300/791510757778784306/hazuki_birthday.jpg");
            //                await Bot.Doremi.client
            //                .GetGuild(guildId)
            //                .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement]))
            //                .SendMessageAsync(embed: eb.Build());
            //                birthdayExisted = true;
            //            }

            //            //announce aiko birthday
            //            if (Config.Aiko.Status.isBirthday() &&
            //            Convert.ToInt32(guildData[DBM_Guild.Columns.birthday_announcement_ojamajo]) == 1)
            //            {
            //                eb = eb.WithTitle($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy Birthday, Aiko Chan!")
            //                .WithDescription($"Happy birthday to our dear osakan friend: Aiko chan. " +
            //                $"She has turned into {Config.Aiko.birthdayCalculatedYear} this year. Let's give some takoyaki and wonderful birthday wishes for her.")
            //                .WithImageUrl("https://cdn.discordapp.com/attachments/706770454697738300/791579373324992512/happy_birthday_aiko_chan.jpg")
            //                .WithFooter("Art By: Letter Three");

            //                await Bot.Doremi.client
            //                .GetGuild(guildId)
            //                .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement]))
            //                .SendMessageAsync(embed: eb.Build());
            //                birthdayExisted = true;
            //            }

            //            //announce onpu birthday
            //            if (Config.Onpu.Status.isBirthday() &&
            //            Convert.ToInt32(guildData[DBM_Guild.Columns.birthday_announcement_ojamajo]) == 1)
            //            {
            //                eb = eb.WithTitle($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy Birthday, Onpu Chan!")
            //                .WithDescription($"Happy birthday to our wonderful idol friend: Onpu chan. " +
            //                $"She has turned into {Config.Onpu.birthdayCalculatedYear} this year. Let's give some wonderful birthday wishes for her.")
            //                .WithImageUrl("https://cdn.discordapp.com/attachments/706770454697738300/790803480482021377/Onpu__Nintendo_Switch_Birthday_Pic.png")
            //                .WithFooter("Art By: Letter Three");

            //                await Bot.Doremi.client
            //                .GetGuild(guildId)
            //                .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement]))
            //                .SendMessageAsync(embed: eb.Build());
            //                birthdayExisted = true;
            //            }

            //            //announce momoko birthday
            //            if (Config.Momoko.Status.isBirthday() &&
            //            Convert.ToInt32(guildData[DBM_Guild.Columns.birthday_announcement_ojamajo]) == 1)
            //            {
            //                eb = eb.WithTitle($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy Birthday, Momoko Chan!")
            //                .WithDescription($"Happy birthday to our wonderful friend: Momoko chan. " +
            //                $"She has turned into {Config.Momoko.birthdayCalculatedYear} this year. Let's give some wonderful birthday wishes for her.")
            //                .WithImageUrl("https://cdn.discordapp.com/attachments/706770454697738300/790803547209203722/Momoko_Birthday_Pic.png")
            //                .WithFooter("Art By: Letter Three");

            //                await Bot.Doremi.client
            //                .GetGuild(guildId)
            //                .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement]))
            //                .SendMessageAsync();
            //                birthdayExisted = true;
            //            }

            //            //announce member birthday
            //            DBC db = new DBC();
            //            string query = @$"select * 
            //            from {DBM_Guild_User_Birthday.tableName} 
            //            where {DBM_Guild_User_Birthday.Columns.id_guild}=@{DBM_Guild_User_Birthday.Columns.id_guild} and 
            //            month({DBM_Guild_User_Birthday.Columns.birthday_date}) = month(curdate()) and 
            //            day({DBM_Guild_User_Birthday.Columns.birthday_date}) = day(curdate())";

            //            Dictionary<string, object> colSelect = new Dictionary<string, object>();
            //            colSelect[DBM_Guild_User_Birthday.Columns.id_guild] = guildId.ToString();

            //            var result = db.selectAll(query, colSelect);

            //            string birthdayPeople = "";

            //            foreach (DataRow row in result.Rows)
            //            {
            //                //check if user exists/not on the server
            //                try
            //                {
            //                    SocketUser masterUser =
            //                    Context.Guild.Users.FirstOrDefault(x => x.Id ==
            //                    Convert.ToUInt64(row[DBM_Guild_User_Birthday.Columns.id_user].ToString()));
            //                    if (masterUser != null)
            //                    {
            //                        birthdayPeople += MentionUtils.MentionUser(
            //                            Convert.ToUInt64(row[DBM_Guild_User_Birthday.Columns.id_user].ToString()));
            //                        birthdayExisted = true;
            //                    }
            //                }
            //                catch (Exception e) { }
            //            }

            //            if (birthdayExisted)
            //            {
            //                if (birthdayPeople != "")
            //                {
            //                    //check if custom birthday message exists/not
            //                    query = $"SELECT * " +
            //                    $" FROM {DBM_Guild_Custom_Birthday.tableName} " +
            //                    $" WHERE {DBM_Guild_Custom_Birthday.Columns.id_guild}=@{DBM_Custom_Command.Columns.id_guild} " +
            //                    $" ORDER BY RAND() " +
            //                    $" LIMIT 1";
            //                    colSelect = new Dictionary<string, object>();
            //                    colSelect[DBM_Guild_Custom_Birthday.Columns.id_guild] = guildId.ToString();
            //                    var resultCustomBirthday = new DBC().selectAll(query, colSelect);
            //                    if (resultCustomBirthday.Rows.Count >= 1)
            //                    {
            //                        foreach (DataRow row in resultCustomBirthday.Rows)
            //                        {
            //                            var custBirthdayMessage = row[DBM_Guild_Custom_Birthday.Columns.message].ToString();
            //                            var custBirthdayImage = row[DBM_Guild_Custom_Birthday.Columns.img_url].ToString();

            //                            await Bot.Doremi.client
            //                            .GetGuild(guildId)
            //                            .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement].ToString()))
            //                            .SendMessageAsync(
            //                                $"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday to: {birthdayPeople}",
            //                                embed: new EmbedBuilder()
            //                                .WithColor(Config.Doremi.EmbedColor)
            //                                .WithDescription(custBirthdayMessage)
            //                                .WithImageUrl(custBirthdayImage)
            //                                .WithFooter($"From: {Context.Guild.Name} & friends")
            //                                .Build());
            //                        }
            //                    }
            //                    else
            //                    {
            //                        string[] arrRandomedMessage = {
            //                        $"Everyone, please give some wonderful birthday wishes. ",
            //                        $"Wishing you all the best and hapiness always."
            //                    };

            //                        await Bot.Doremi.client
            //                        .GetGuild(guildId)
            //                        .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement].ToString()))
            //                        .SendMessageAsync(
            //                            $"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday to: {birthdayPeople}",
            //                            embed: new EmbedBuilder()
            //                            .WithColor(Config.Doremi.EmbedColor)
            //                            .WithDescription(arrRandomedMessage[new Random().Next(0, arrRandomedMessage.Length)])
            //                            .WithImageUrl("https://media.discordapp.net/attachments/706770454697738300/745492527070576670/1508005628768.png")
            //                            .WithFooter($"Best wishes from: {Context.Guild.Name} & friends")
            //                            .Build());
            //                    }
            //                }

            //                query = @$"UPDATE {DBM_Guild.tableName} 
            //                    SET {DBM_Guild.Columns.birthday_announcement_date_last}=@{DBM_Guild.Columns.birthday_announcement_date_last} 
            //                    WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild}";
            //                Dictionary<string, object> columnsFilter = new Dictionary<string, object>();
            //                columnsFilter[DBM_Guild.Columns.birthday_announcement_date_last] = DateTime.Now.ToString("dd");
            //                columnsFilter[DBM_Guild.Columns.id_guild] = guildId.ToString();
            //                new DBC().update(query, columnsFilter);
            //            }
            //        }
            //    },
            //    null,
            //    TimeSpan.FromSeconds(10), //time to wait before executing the timer for the first time
            //    TimeSpan.FromMinutes(1) //time to wait before executing the timer again
            //    );

            //    //hazuki timer
            //    Config.Hazuki._timerBirthdayAnnouncement[guildId.ToString()] = new Timer(async _ =>
            //    {
            //        //set birthday announcement timer
            //        if (guildBirthdayLastAnnouncement != DateTime.Now.ToString("dd") &&
            //        Config.Doremi.Status.isBirthday())
            //        {
            //            await Bot.Hazuki.client
            //            .GetGuild(guildId)
            //            .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement].ToString()))
            //            .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday, {MentionUtils.MentionUser(Config.Doremi.Id)} chan. " +
            //            $"She has turned into {Config.Doremi.birthdayCalculatedYear} this year. Let's give some big steak and wonderful birthday wishes for her.");
            //        }
            //    },
            //    null,
            //    TimeSpan.FromSeconds(10), //time to wait before executing the timer for the first time
            //    TimeSpan.FromMinutes(1) //time to wait before executing the timer again
            //    );

            //    //aiko timer
            //    Config.Aiko._timerBirthdayAnnouncement[guildId.ToString()] = new Timer(async _ =>
            //    {
            //        //announce doremi birthday
            //        if (guildBirthdayLastAnnouncement != DateTime.Now.ToString("dd") &&
            //            Config.Doremi.Status.isBirthday())
            //        {
            //            await Bot.Aiko.client
            //            .GetGuild(guildId)
            //            .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement].ToString()))
            //            .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday, {MentionUtils.MentionUser(Config.Doremi.Id)} chan. " +
            //            $"She has turned into {Config.Doremi.birthdayCalculatedYear} this year. Let's give some big steak and wonderful birthday wishes for her.",
            //            embed: new EmbedBuilder()
            //            .WithColor(Config.Aiko.EmbedColor)
            //            .WithImageUrl("https://cdn.discordapp.com/attachments/706770454697738300/790803590482493450/Doremi_Birthday_Pic.png")
            //            .Build());

            //        }
            //    },
            //    null,
            //    TimeSpan.FromSeconds(10), //time to wait before executing the timer for the first time
            //    TimeSpan.FromMinutes(1) //time to wait before executing the timer again
            //    );

            //    //onpu timer 
            //    Config.Onpu._timerBirthdayAnnouncement[guildId.ToString()] = new Timer(async _ =>
            //    {
            //        //announce doremi birthday
            //        if (guildBirthdayLastAnnouncement != DateTime.Now.ToString("dd") &&
            //            Config.Doremi.Status.isBirthday())
            //        {
            //            await Bot.Onpu.client
            //            .GetGuild(guildId)
            //            .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement].ToString()))
            //            .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday, {MentionUtils.MentionUser(Config.Doremi.Id)} chan. " +
            //            $"She has turned into {Config.Doremi.birthdayCalculatedYear} this year. Let's give some big steak and wonderful birthday wishes for her.");

            //        }
            //    },
            //    null,
            //    TimeSpan.FromSeconds(10), //time to wait before executing the timer for the first time
            //    TimeSpan.FromMinutes(1) //time to wait before executing the timer again
            //    );

            //    //momoko timer
            //    Config.Momoko._timerBirthdayAnnouncement[guildId.ToString()] = new Timer(async _ =>
            //    {
            //        //announce doremi birthday
            //        if (guildBirthdayLastAnnouncement != DateTime.Now.ToString("dd") &&
            //            Config.Doremi.Status.isBirthday())
            //        {
            //            await Bot.Momoko.client
            //            .GetGuild(guildId)
            //            .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement].ToString()))
            //            .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday, {MentionUtils.MentionUser(Config.Doremi.Id)} chan. " +
            //            $"She has turned into {Config.Doremi.birthdayCalculatedYear} on this year. Let's give some big steak and wonderful birthday wishes for her.");

            //            //update last birthday announcement date
            //            string query = @$"UPDATE {DBM_Guild.tableName} 
            //            SET {DBM_Guild.Columns.birthday_announcement_date_last}=@{DBM_Guild.Columns.birthday_announcement_date_last} 
            //            WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild}";
            //            Dictionary<string, object> columnsFilter = new Dictionary<string, object>();
            //            columnsFilter[DBM_Guild.Columns.birthday_announcement_date_last] = DateTime.Now.ToString("dd");
            //            columnsFilter[DBM_Guild.Columns.id_guild] = guildId.ToString();
            //            new DBC().update(query, columnsFilter);
            //        }
            //    },
            //    null,
            //    TimeSpan.FromSeconds(10), //time to wait before executing the timer for the first time
            //    TimeSpan.FromMinutes(1) //time to wait before executing the timer again
            //    );

            //    await ReplyAsync($"{Config.Emoji.birthdayCake} **Birthday Announcement Channels** has been assigned at: {MentionUtils.MentionChannel(channel_name.Id)}");

            //}

            [Command("update"), Summary("Update & set the channel announcement settings. " +
                "Current available settings: `welcome`/`avatarlevel`/`birthday`/`userleaving`")]
            public async Task updateChannelSettings(string settings, SocketGuildChannel channel_name)
            {
                if (settings == "")
                {
                    await ReplyAsync("Please enter the channel settings."); return;
                }
                string property = ""; string queryUpdate = "";
                ulong guildId = Context.Guild.Id;
                Dictionary<string, object> columnFilter = new Dictionary<string, object>();
                columnFilter[DBM_Guild.Columns.id_guild] = guildId.ToString();

                switch (settings.ToLower())
                {
                    case "welcome":
                        property = $"Welcome Announcement";
                        queryUpdate = $"UPDATE {DBM_Guild.tableName} " +
                                $" SET {DBM_Guild.Columns.id_channel_notification_user_welcome}=@{DBM_Guild.Columns.id_channel_notification_user_welcome} " +
                                $" WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild} ";
                        columnFilter[DBM_Guild.Columns.id_channel_notification_user_welcome] = channel_name.Id.ToString();
                        new DBC().update(queryUpdate, columnFilter);
                        break;
                    case "avatarlevel":
                        property = $"Avatar Level Up Notification";
                        queryUpdate = $"UPDATE {DBM_Guild.tableName} " +
                                $" SET {DBM_Guild.Columns.id_channel_notification_chat_level_up}=@{DBM_Guild.Columns.id_channel_notification_chat_level_up} " +
                                $" WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild} ";
                        columnFilter[DBM_Guild.Columns.id_channel_notification_chat_level_up] = channel_name.Id.ToString();
                        new DBC().update(queryUpdate, columnFilter);
                        break;
                    case "userleaving":
                        property = $"User Leaving Log";
                        queryUpdate = $"UPDATE {DBM_Guild.tableName} " +
                                $" SET {DBM_Guild.Columns.id_channel_user_leaving_log}=@{DBM_Guild.Columns.id_channel_user_leaving_log} " +
                                $" WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild} ";
                        columnFilter[DBM_Guild.Columns.id_channel_user_leaving_log] = channel_name.Id.ToString();
                        new DBC().update(queryUpdate, columnFilter);
                        break;
                    case "birthday":
                        var socketClient = Context.Client;

                        //start update
                        queryUpdate = $"UPDATE {DBM_Guild.tableName} " +
                            $" SET {DBM_Guild.Columns.id_channel_birthday_announcement}=@{DBM_Guild.Columns.id_channel_birthday_announcement} " +
                            $" WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild} ";
                        Dictionary<string, object> columns = new Dictionary<string, object>();
                        columns[DBM_Guild.Columns.id_channel_birthday_announcement] = channel_name.Id.ToString();
                        columns[DBM_Guild.Columns.id_guild] = guildId.ToString();
                        new DBC().update(queryUpdate, columns);

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

                        var guildData = Config.Guild.getGuildData(guildId);

                        string guildBirthdayLastAnnouncement = "";
                        if (guildData[DBM_Guild.Columns.birthday_announcement_date_last].ToString() != "")
                            guildBirthdayLastAnnouncement = guildData[DBM_Guild.Columns.birthday_announcement_date_last].ToString();
                        else
                            guildBirthdayLastAnnouncement = "1";

                        //set doremi timer
                        Config.Doremi._timerBirthdayAnnouncement[guildId.ToString()] = new Timer(async _ =>
                        {
                            guildData = Config.Guild.getGuildData(guildId);
                            if (guildData[DBM_Guild.Columns.birthday_announcement_date_last].ToString() != "")
                                guildBirthdayLastAnnouncement = guildData[DBM_Guild.Columns.birthday_announcement_date_last].ToString();
                            else
                                guildBirthdayLastAnnouncement = "1";

                            EmbedBuilder eb = new EmbedBuilder()
                            .WithColor(Config.Doremi.EmbedColor);

                            //set birthday announcement timer
                            if (guildBirthdayLastAnnouncement != DateTime.Now.ToString("dd"))
                            {
                                Boolean birthdayExisted = false;

                                //announce hazuki birthday
                                if (Config.Hazuki.Status.isBirthday() &&
                                Convert.ToInt32(guildData[DBM_Guild.Columns.birthday_announcement_ojamajo]) == 1)
                                {
                                    eb = eb.WithTitle($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy Birthday, Hazuki Chan!")
                                    .WithDescription($"Happy birthday to you, Hazuki chan. " +
                                    $"She has turned into {Config.Hazuki.birthdayCalculatedYear} this year. Let's give wonderful birthday wishes for her.")
                                    .WithImageUrl("https://cdn.discordapp.com/attachments/706770454697738300/791510757778784306/hazuki_birthday.jpg");
                                    await Bot.Doremi.client
                                    .GetGuild(guildId)
                                    .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement]))
                                    .SendMessageAsync(embed: eb.Build());
                                    birthdayExisted = true;
                                }

                                //announce aiko birthday
                                if (Config.Aiko.Status.isBirthday() &&
                                Convert.ToInt32(guildData[DBM_Guild.Columns.birthday_announcement_ojamajo]) == 1)
                                {
                                    eb = eb.WithTitle($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy Birthday, Aiko Chan!")
                                    .WithDescription($"Happy birthday to our dear osakan friend: Aiko chan. " +
                                    $"She has turned into {Config.Aiko.birthdayCalculatedYear} this year. Let's give some takoyaki and wonderful birthday wishes for her.")
                                    .WithImageUrl("https://cdn.discordapp.com/attachments/706770454697738300/791579373324992512/happy_birthday_aiko_chan.jpg")
                                    .WithFooter("Art By: Letter Three");

                                    await Bot.Doremi.client
                                    .GetGuild(guildId)
                                    .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement]))
                                    .SendMessageAsync(embed: eb.Build());
                                    birthdayExisted = true;
                                }

                                //announce onpu birthday
                                if (Config.Onpu.Status.isBirthday() &&
                                Convert.ToInt32(guildData[DBM_Guild.Columns.birthday_announcement_ojamajo]) == 1)
                                {
                                    eb = eb.WithTitle($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy Birthday, Onpu Chan!")
                                    .WithDescription($"Happy birthday to our wonderful idol friend: Onpu chan. " +
                                    $"She has turned into {Config.Onpu.birthdayCalculatedYear} this year. Let's give some wonderful birthday wishes for her.")
                                    .WithImageUrl("https://cdn.discordapp.com/attachments/706770454697738300/790803480482021377/Onpu__Nintendo_Switch_Birthday_Pic.png")
                                    .WithFooter("Art By: Letter Three");

                                    await Bot.Doremi.client
                                    .GetGuild(guildId)
                                    .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement]))
                                    .SendMessageAsync(embed: eb.Build());
                                    birthdayExisted = true;
                                }

                                //announce momoko birthday
                                if (Config.Momoko.Status.isBirthday() &&
                                Convert.ToInt32(guildData[DBM_Guild.Columns.birthday_announcement_ojamajo]) == 1)
                                {
                                    eb = eb.WithTitle($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy Birthday, Momoko Chan!")
                                    .WithDescription($"Happy birthday to our wonderful friend: Momoko chan. " +
                                    $"She has turned into {Config.Momoko.birthdayCalculatedYear} this year. Let's give some wonderful birthday wishes for her.")
                                    .WithImageUrl("https://cdn.discordapp.com/attachments/706770454697738300/790803547209203722/Momoko_Birthday_Pic.png")
                                    .WithFooter("Art By: Letter Three");

                                    await Bot.Doremi.client
                                    .GetGuild(guildId)
                                    .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement]))
                                    .SendMessageAsync(embed: eb.Build());
                                    birthdayExisted = true;
                                }

                                //announce member birthday
                                DBC db = new DBC();
                                string query = @$"select * 
                                from {DBM_Guild_User_Birthday.tableName} 
                                where {DBM_Guild_User_Birthday.Columns.id_guild}=@{DBM_Guild_User_Birthday.Columns.id_guild} and 
                                month({DBM_Guild_User_Birthday.Columns.birthday_date}) = month(curdate()) and 
                                day({DBM_Guild_User_Birthday.Columns.birthday_date}) = day(curdate())";

                                Dictionary<string, object> colSelect = new Dictionary<string, object>();
                                colSelect[DBM_Guild_User_Birthday.Columns.id_guild] = guildId.ToString();

                                var result = db.selectAll(query, colSelect);

                                string birthdayPeople = "";

                                foreach (DataRow row in result.Rows)
                                {
                                    //check if user exists/not on the server
                                    try
                                    {
                                        SocketUser masterUser =
                                        Context.Guild.Users.FirstOrDefault(x => x.Id ==
                                        Convert.ToUInt64(row[DBM_Guild_User_Birthday.Columns.id_user].ToString()));
                                        if (masterUser != null)
                                        {
                                            birthdayPeople += $"{MentionUtils.MentionUser(Convert.ToUInt64(row[DBM_Guild_User_Birthday.Columns.id_user].ToString()))} ";
                                            birthdayExisted = true;
                                        }
                                    }
                                    catch (Exception e) { }
                                }

                                if (birthdayExisted)
                                {
                                    if (birthdayPeople != "")
                                    {
                                        //check if custom birthday message exists/not
                                        query = $"SELECT * " +
                                        $" FROM {DBM_Guild_Custom_Birthday.tableName} " +
                                        $" WHERE {DBM_Guild_Custom_Birthday.Columns.id_guild}=@{DBM_Guild_Custom_Command.Columns.id_guild} " +
                                        $" ORDER BY RAND() " +
                                        $" LIMIT 1";
                                        colSelect = new Dictionary<string, object>();
                                        colSelect[DBM_Guild_Custom_Birthday.Columns.id_guild] = guildId.ToString();
                                        var resultCustomBirthday = new DBC().selectAll(query, colSelect);
                                        if (resultCustomBirthday.Rows.Count >= 1)
                                        {
                                            foreach (DataRow row in resultCustomBirthday.Rows)
                                            {
                                                var custBirthdayMessage = row[DBM_Guild_Custom_Birthday.Columns.message].ToString();
                                                var custBirthdayImage = row[DBM_Guild_Custom_Birthday.Columns.img_url].ToString();

                                                await Bot.Doremi.client
                                                .GetGuild(guildId)
                                                .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement].ToString()))
                                                .SendMessageAsync(
                                                    $"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday to: {birthdayPeople}",
                                                    embed: new EmbedBuilder()
                                                    .WithColor(Config.Doremi.EmbedColor)
                                                    .WithDescription(custBirthdayMessage)
                                                    .WithImageUrl(custBirthdayImage)
                                                    .WithFooter($"Best wishes from: {Context.Guild.Name} & friends")
                                                    .Build());
                                            }
                                        }
                                        else
                                        {
                                            string[] arrRandomedMessage = {
                                    $"Everyone, please give some wonderful birthday wishes. ",
                                    $"Wishing you all the best and hapiness always."
                                };

                                            await Bot.Doremi.client
                                            .GetGuild(guildId)
                                            .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement].ToString()))
                                            .SendMessageAsync(
                                                $"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday to: {birthdayPeople}",
                                                embed: new EmbedBuilder()
                                                .WithColor(Config.Doremi.EmbedColor)
                                                .WithDescription(arrRandomedMessage[new Random().Next(0, arrRandomedMessage.Length)])
                                                .WithImageUrl("https://media.discordapp.net/attachments/706770454697738300/745492527070576670/1508005628768.png")
                                                .WithFooter($"Best wishes from: {Context.Guild.Name} & friends")
                                                .Build());
                                        }
                                    }

                                    query = @$"UPDATE {DBM_Guild.tableName} 
                                SET {DBM_Guild.Columns.birthday_announcement_date_last}=@{DBM_Guild.Columns.birthday_announcement_date_last} 
                                WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild}";
                                    Dictionary<string, object> columnsFilter = new Dictionary<string, object>();
                                    columnsFilter[DBM_Guild.Columns.birthday_announcement_date_last] = DateTime.Now.ToString("dd");
                                    columnsFilter[DBM_Guild.Columns.id_guild] = guildId.ToString();
                                    new DBC().update(query, columnsFilter);
                                }
                            }
                        },
                        null,
                        TimeSpan.FromSeconds(10), //time to wait before executing the timer for the first time
                        TimeSpan.FromMinutes(40) //time to wait before executing the timer again
                        );

                        //hazuki timer
                        Config.Hazuki._timerBirthdayAnnouncement[guildId.ToString()] = new Timer(async _ =>
                        {
                            //set birthday announcement timer
                            if (guildBirthdayLastAnnouncement != DateTime.Now.ToString("dd") &&
                            Config.Doremi.Status.isBirthday())
                            {
                                await Bot.Hazuki.client
                                .GetGuild(guildId)
                                .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement].ToString()))
                                .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday, {MentionUtils.MentionUser(Config.Doremi.Id)} chan. " +
                                $"She has turned into {Config.Doremi.birthdayCalculatedYear} this year. Let's give some big steak and wonderful birthday wishes for her.");
                            }
                        },
                        null,
                        TimeSpan.FromSeconds(10), //time to wait before executing the timer for the first time
                        TimeSpan.FromMinutes(40) //time to wait before executing the timer again
                        );

                        //aiko timer
                        Config.Aiko._timerBirthdayAnnouncement[guildId.ToString()] = new Timer(async _ =>
                        {
                            //announce doremi birthday
                            if (guildBirthdayLastAnnouncement != DateTime.Now.ToString("dd") &&
                                Config.Doremi.Status.isBirthday())
                            {
                                await Bot.Aiko.client
                                .GetGuild(guildId)
                                .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement].ToString()))
                                .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday, {MentionUtils.MentionUser(Config.Doremi.Id)} chan. " +
                                $"She has turned into {Config.Doremi.birthdayCalculatedYear} this year. Let's give some big steak and wonderful birthday wishes for her.",
                                embed: new EmbedBuilder()
                                .WithColor(Config.Aiko.EmbedColor)
                                .WithImageUrl("https://cdn.discordapp.com/attachments/706770454697738300/790803590482493450/Doremi_Birthday_Pic.png")
                                .Build());

                                //update last birthday announcement date
                                string query = @$"UPDATE {DBM_Guild.tableName} 
                                SET {DBM_Guild.Columns.birthday_announcement_date_last}=@{DBM_Guild.Columns.birthday_announcement_date_last} 
                                WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild}";
                                Dictionary<string, object> columnsFilter = new Dictionary<string, object>();
                                columnsFilter[DBM_Guild.Columns.birthday_announcement_date_last] = DateTime.Now.ToString("dd");
                                columnsFilter[DBM_Guild.Columns.id_guild] = guildId.ToString();
                                new DBC().update(query, columnsFilter);

                            }
                        },
                        null,
                        TimeSpan.FromSeconds(10), //time to wait before executing the timer for the first time
                        TimeSpan.FromMinutes(40) //time to wait before executing the timer again
                        );

                        //onpu timer 
                        Config.Onpu._timerBirthdayAnnouncement[guildId.ToString()] = new Timer(async _ =>
                        {
                            //announce doremi birthday
                            if (guildBirthdayLastAnnouncement != DateTime.Now.ToString("dd") &&
                                Config.Doremi.Status.isBirthday())
                            {
                                await Bot.Onpu.client
                                .GetGuild(guildId)
                                .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement].ToString()))
                                .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday, {MentionUtils.MentionUser(Config.Doremi.Id)} chan. " +
                                $"She has turned into {Config.Doremi.birthdayCalculatedYear} this year. Let's give some big steak and wonderful birthday wishes for her.");

                            }
                        },
                        null,
                        TimeSpan.FromSeconds(10), //time to wait before executing the timer for the first time
                        TimeSpan.FromMinutes(40) //time to wait before executing the timer again
                        );

                        //momoko timer
                        Config.Momoko._timerBirthdayAnnouncement[guildId.ToString()] = new Timer(async _ =>
                        {
                            //announce doremi birthday
                            if (guildBirthdayLastAnnouncement != DateTime.Now.ToString("dd") &&
                                Config.Doremi.Status.isBirthday())
                            {
                                await Bot.Momoko.client
                                .GetGuild(guildId)
                                .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement].ToString()))
                                .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday, {MentionUtils.MentionUser(Config.Doremi.Id)} chan. " +
                                $"She has turned into {Config.Doremi.birthdayCalculatedYear} on this year. Let's give some big steak and wonderful birthday wishes for her.");
                            }
                        },
                        null,
                        TimeSpan.FromSeconds(10), //time to wait before executing the timer for the first time
                        TimeSpan.FromMinutes(40) //time to wait before executing the timer again
                        );
                        property = "Birthday";
                        break;
                    default:
                        await ReplyAsync($"Sorry, I can't found that channel settings");
                        return;
                }

                await ReplyAsync($"**{property} Channel** settings has been updated.");
            }

            [Command("remove"), Summary("Remove the settings on the assigned channels. " +
                "Current available settings: `welcome`/`avatarlevel`/`birthday`/`chatlevel`")]
            public async Task removeChannelSettings([Remainder] string settings)
            {
                string property = ""; string queryUpdate = "";
                var guildId = Context.Guild.Id;

                //for guild initialization
                var guildData = Config.Guild.getGuildData(guildId);
                Dictionary<string, object> columnFilter = new Dictionary<string, object>();

                switch (settings.ToLower())
                {
                    case "welcome":
                        property = $"Welcome Announcement";
                        queryUpdate = $"UPDATE {DBM_Guild.tableName} " +
                                $" SET {DBM_Guild.Columns.id_channel_notification_user_welcome}=@{DBM_Guild.Columns.id_channel_notification_user_welcome} " +
                                $" WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild} ";
                        columnFilter[DBM_Guild.Columns.id_channel_notification_user_welcome] = "";
                        new DBC().update(queryUpdate, columnFilter);
                        break;
                    case "birthday":
                        property = $"{Config.Emoji.birthdayCake} Birthday Announcement";

                        queryUpdate = $"UPDATE {DBM_Guild.tableName} " +
                                $" SET {DBM_Guild.Columns.id_channel_birthday_announcement}=@{DBM_Guild.Columns.id_channel_birthday_announcement}," +
                                $" {DBM_Guild.Columns.birthday_announcement_ojamajo}=@{DBM_Guild.Columns.birthday_announcement_ojamajo} " +
                                $" WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild} ";
                        columnFilter[DBM_Guild.Columns.id_channel_birthday_announcement] = "";
                        columnFilter[DBM_Guild.Columns.birthday_announcement_ojamajo] = 0;
                        columnFilter[DBM_Guild.Columns.id_guild] = guildId.ToString();
                        new DBC().update(queryUpdate, columnFilter);

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
                        break;
                    case "userleaving":
                        property = $"User Leaving Log";
                        queryUpdate = $"UPDATE {DBM_Guild.tableName} " +
                                $" SET {DBM_Guild.Columns.id_channel_user_leaving_log}=@{DBM_Guild.Columns.id_channel_user_leaving_log} " +
                                $" WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild} ";
                        columnFilter[DBM_Guild.Columns.id_channel_user_leaving_log] = "";
                        new DBC().update(queryUpdate, columnFilter);
                        break;
                    case "avatarlevel":
                        property = $"Avatar Level Up notification";
                        queryUpdate = $"UPDATE {DBM_Guild.tableName} " +
                                $" SET {DBM_Guild.Columns.id_channel_birthday_announcement}=@{DBM_Guild.Columns.id_channel_birthday_announcement} " +
                                $" WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild} ";
                        columnFilter[DBM_Guild.Columns.id_channel_notification_chat_level_up] = "";
                        columnFilter[DBM_Guild.Columns.id_guild] = guildId.ToString();
                        new DBC().update(queryUpdate, columnFilter);
                        break;
                    default:
                        await ReplyAsync($"Sorry, I can't found that channel settings"); return;
                }

                await ReplyAsync($"**{property} Channels** settings has been removed.");
            }

        }

        [Name("mod trading card"), Group("trading card"), Summary("These commands require `Manage Roles` permissions.")]
        public class DoremiModeratorTradingCards : ModuleBase<SocketCommandContext>
        {
            //trading card configuration section
            //assign trading card spawning channel
            [Command("spawn"), Summary("Set Doremi Bot and the others to make the trading card spawned at <channel_name> " +
                " within <interval_minutes> minutes interval.")]
            public async Task assignTradingCardSpawnChannel(SocketGuildChannel channel_name, int interval_minutes)
            {
                var guildId = channel_name.Guild.Id;
                var guildCardSpawnData = TradingCardGuildCore.getGuildData(guildId);

                if (interval_minutes < 5 || interval_minutes > 1440) await ReplyAsync($"Please enter the card spawn interval(in minutes) between 5-1440");
                else
                {
                    //update data
                    string query = $"UPDATE {DBM_Trading_Card_Guild.tableName} " +
                        $" SET {DBM_Trading_Card_Guild.Columns.id_channel_spawn}=@{DBM_Trading_Card_Guild.Columns.id_channel_spawn}," +
                        $" {DBM_Trading_Card_Guild.Columns.spawn_interval}=@{DBM_Trading_Card_Guild.Columns.spawn_interval} " +
                        $" WHERE {DBM_Trading_Card_Guild.Columns.id_guild}=@{DBM_Trading_Card_Guild.Columns.id_guild} ";
                    Dictionary<string, object> columnsFilter = new Dictionary<string, object>();
                    columnsFilter[DBM_Trading_Card_Guild.Columns.id_channel_spawn] = channel_name.Id.ToString();
                    columnsFilter[DBM_Trading_Card_Guild.Columns.spawn_interval] = Convert.ToInt32(interval_minutes);
                    columnsFilter[DBM_Trading_Card_Guild.Columns.id_guild] = guildId.ToString();
                    new DBC().update(query, columnsFilter);

                    //get updated data
                    guildCardSpawnData = TradingCardGuildCore.getGuildData(guildId);

                    if (Config.Doremi._stopwatchCardSpawn.ContainsKey(Context.Guild.Id.ToString()))
                    {
                        Config.Doremi._timerTradingCardSpawn[guildId.ToString()].Change(
                            TimeSpan.FromMinutes(Convert.ToInt32(guildCardSpawnData[DBM_Trading_Card_Guild.Columns.spawn_interval].ToString())),
                            TimeSpan.FromMinutes(Convert.ToInt32(guildCardSpawnData[DBM_Trading_Card_Guild.Columns.spawn_interval].ToString()))
                        );
                        Config.Doremi._stopwatchCardSpawn[guildId.ToString()].Restart();
                    } else
                    {
                        Config.Doremi._timerTradingCardSpawn[guildId.ToString()] = new Timer(async _ =>
                        {
                            await TradingCardCore.generateCardSpawn(guildId);
                        },
                        null,
                        TimeSpan.FromMinutes(Convert.ToInt32(guildCardSpawnData[DBM_Trading_Card_Guild.Columns.spawn_interval])), //time to wait before executing the timer for the first time
                        TimeSpan.FromMinutes(Convert.ToInt32(guildCardSpawnData[DBM_Trading_Card_Guild.Columns.spawn_interval])) //time to wait before executing the timer again
                        );
                    }

                    await ReplyAsync($"**Trading Card** will be spawned at {MentionUtils.MentionChannel(channel_name.Id)} " +
                        $"within {interval_minutes} minute(s) interval");
                }
            }

            //set spawning interval
            //[Command("interval"), Summary("Set the trading card spawn interval (in minutes).")]
            //public async Task setTradingCardSpawnInterval(int interval_minutes)
            //{
            //    if (interval_minutes < 5 || interval_minutes > 1440) await ReplyAsync($"Please enter the card spawn interval(in minutes) between 5-1440");
            //    else
            //    {
            //        var guildId = Context.Guild.Id;
            //        var guildCardSpawnData = TradingCardGuildCore.getGuildData(guildId);

            //        //update data
            //        Dictionary<string, object> columnsFilter = new Dictionary<string, object>();
            //        columnsFilter[DBM_Trading_Card_Guild.Columns.spawn_interval] = interval_minutes;
            //        columnsFilter[DBM_Trading_Card_Guild.Columns.id_guild] = guildId.ToString();
            //        string query = $"UPDATE {DBM_Trading_Card_Guild.tableName} " +
            //            $" SET {DBM_Trading_Card_Guild.Columns.id_channel_spawn}=@{DBM_Trading_Card_Guild.Columns.id_channel_spawn} " +
            //            $" WHERE {DBM_Trading_Card_Guild.Columns.id_guild}=@{DBM_Trading_Card_Guild.Columns.id_guild} ";
            //        new DBC().update(query, columnsFilter);

            //        //Config.Guild.setPropertyValue(guildId, "trading_card_spawn_interval", interval_minutes.ToString());
            //        await ReplyAsync($"**Trading Card Spawn interval** has been set into **{interval_minutes}** minute(s)");

            //        if (Config.Doremi._stopwatchCardSpawn.ContainsKey(Context.Guild.Id.ToString()))
            //            Config.Doremi._stopwatchCardSpawn[guildId.ToString()].Restart();
                    
            //        //get updated data
            //        guildCardSpawnData = TradingCardGuildCore.getGuildData(guildId);

            //        //set random card spawn timer
            //        if (Config.Doremi._timerTradingCardSpawn.ContainsKey(guildId.ToString()))
            //        {
            //            Config.Doremi._timerTradingCardSpawn[guildId.ToString()].Change(
            //                TimeSpan.FromMinutes(Convert.ToInt32(guildCardSpawnData[DBM_Trading_Card_Guild.Columns.spawn_interval].ToString())),
            //                TimeSpan.FromMinutes(Convert.ToInt32(guildCardSpawnData[DBM_Trading_Card_Guild.Columns.spawn_interval].ToString()))
            //            );
            //        }
            //    }
            //}

            [Command("create completionist role"), Alias("create badge role"), Summary("Register the trading card role completionist.")]
            public async Task initTradingCardRoleCompletionist()
            {
                var roleDoremi = Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Doremi.roleCompletionist).ToList();
                var roleHazuki = Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Hazuki.roleCompletionist).ToList();
                var roleAiko = Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Aiko.roleCompletionist).ToList();
                var roleOnpu = Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Onpu.roleCompletionist).ToList();
                var roleMomoko = Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Momoko.roleCompletionist).ToList();
                var roleSpecial = Context.Guild.Roles.Where(x => x.Name == TradingCardCore.roleCompletionistSpecial).ToList();
                var rolePop = Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Pop.roleCompletionist).ToList();
                var roleHana = Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Hana.roleCompletionist).ToList();

                if (roleDoremi.Count <= 0)
                    await Context.Guild.CreateRoleAsync(
                        TradingCardCore.Doremi.roleCompletionist, null, color: Config.Doremi.EmbedColor, false, false
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

                if (rolePop.Count <= 0)
                    await Context.Guild.CreateRoleAsync(
                        TradingCardCore.Pop.roleCompletionist, null, color: TradingCardCore.Pop.embedColor, false, false
                    );

                if (roleHana.Count <= 0)
                    await Context.Guild.CreateRoleAsync(
                        TradingCardCore.Hana.roleCompletionist, null, color: TradingCardCore.Hana.embedColor, false, false
                    );

                await ReplyAsync($":white_check_mark: **Trading Card Badge Role** has been initialized!");

            }

            [Command("remove"), Summary("Remove the trading card spawn settings.")]
            public async Task removeTradingCardSpawn()
            {
                var guildId = Context.Guild.Id;
                var guildTradingCardData = TradingCardGuildCore.getGuildData(guildId);
                
                if (Config.Doremi._stopwatchCardSpawn.ContainsKey(Context.Guild.Id.ToString()))
                    Config.Doremi._stopwatchCardSpawn[guildId.ToString()].Reset();
                    
                if (Config.Doremi._timerTradingCardSpawn.ContainsKey(Context.Guild.Id.ToString()))
                    Config.Doremi._timerTradingCardSpawn[Context.Guild.Id.ToString()].Change(Timeout.Infinite, Timeout.Infinite);

                //reset spawn settings
                string queryUpdate = $"UPDATE {DBM_Trading_Card_Guild.tableName} " +
                    $" SET {DBM_Trading_Card_Guild.Columns.id_channel_spawn}=@{DBM_Trading_Card_Guild.Columns.id_channel_spawn}," +
                    $" {DBM_Trading_Card_Guild.Columns.spawn_id}=@{DBM_Trading_Card_Guild.Columns.spawn_id}, " +
                    $" {DBM_Trading_Card_Guild.Columns.spawn_parent}=@{DBM_Trading_Card_Guild.Columns.spawn_parent}, " +
                    $" {DBM_Trading_Card_Guild.Columns.spawn_category}=@{DBM_Trading_Card_Guild.Columns.spawn_category}, " +
                    $" {DBM_Trading_Card_Guild.Columns.spawn_badcard_question}=@{DBM_Trading_Card_Guild.Columns.spawn_badcard_question}, " +
                    $" {DBM_Trading_Card_Guild.Columns.spawn_badcard_answer}=@{DBM_Trading_Card_Guild.Columns.spawn_badcard_answer}, " +
                    $" {DBM_Trading_Card_Guild.Columns.spawn_interval}=@{DBM_Trading_Card_Guild.Columns.spawn_interval}, " +
                    $" {DBM_Trading_Card_Guild.Columns.spawn_is_badcard}=@{DBM_Trading_Card_Guild.Columns.spawn_is_badcard}, " +
                    $" {DBM_Trading_Card_Guild.Columns.spawn_is_mystery}=@{DBM_Trading_Card_Guild.Columns.spawn_is_mystery}, " +
                    $" {DBM_Trading_Card_Guild.Columns.spawn_is_zone}=@{DBM_Trading_Card_Guild.Columns.spawn_is_zone}, " +
                    $" {DBM_Trading_Card_Guild.Columns.spawn_token}=@{DBM_Trading_Card_Guild.Columns.spawn_token} " +
                    $" WHERE {DBM_Trading_Card_Guild.Columns.id_guild}=@{DBM_Trading_Card_Guild.Columns.id_guild} ";

                Dictionary<string, object> column = new Dictionary<string, object>();
                column[DBM_Trading_Card_Guild.Columns.id_channel_spawn] = "";
                column[DBM_Trading_Card_Guild.Columns.spawn_id] = "";
                column[DBM_Trading_Card_Guild.Columns.spawn_parent] = "";
                column[DBM_Trading_Card_Guild.Columns.spawn_category] = "";
                column[DBM_Trading_Card_Guild.Columns.spawn_badcard_question] = "";
                column[DBM_Trading_Card_Guild.Columns.spawn_badcard_answer] = "";
                column[DBM_Trading_Card_Guild.Columns.spawn_interval] = 40;
                column[DBM_Trading_Card_Guild.Columns.spawn_is_badcard] = 0;
                column[DBM_Trading_Card_Guild.Columns.spawn_is_mystery] = 0;
                column[DBM_Trading_Card_Guild.Columns.spawn_is_zone] = 0;
                column[DBM_Trading_Card_Guild.Columns.spawn_token] = "";
                column[DBM_Trading_Card_Guild.Columns.id_guild] = guildId.ToString();
                new DBC().update(queryUpdate, column);

                await ReplyAsync($"**Trading Card Spawn Channels** settings has been removed.");
            }
        }

    }

    [Name("Avatar"), Group("avatar"), Summary("This category contains all User Avatar Command.")]
    public class DoremiUserAvatarInteractive : InteractiveBase
    {
        [Command("profile", RunMode = RunMode.Async), Alias("status"), Summary("See your avatar profile. " +
            "You can also put username as optional parameter to see other user avatar.")]
        public async Task user_avatar_other([Remainder] SocketGuildUser username = null)
        {
            await ReplyAsync(embed:
                GuildUserAvatarCore.printAvatarStatus(Context, username).Build());
        }

        [Command("set info", RunMode = RunMode.Async), Summary("Set your avatar info. " +
            "You can put empty parameter to erase the info.")]
        public async Task set_avatar_info([Remainder] string info = "")
        {
            if (info.Length >= 75)
            {
                await ReplyAsync("Please enter shorter info. Maximum characters allowed: 75");
                return;
            }

            var guildId = Context.Guild.Id;
            var userId = Context.User.Id;

            var userData = GuildUserAvatarCore.getUserData(guildId, userId);

            string query = $"UPDATE {DBM_Guild_User_Avatar.tableName} " +
                $" SET {DBM_Guild_User_Avatar.Columns.info}=@{DBM_Guild_User_Avatar.Columns.info} " +
                $" WHERE {DBM_Guild_User_Avatar.Columns.id_guild}=@{DBM_Guild_User_Avatar.Columns.id_guild} AND " +
                $" {DBM_Guild_User_Avatar.Columns.id_user}=@{DBM_Guild_User_Avatar.Columns.id_user} ";
            Dictionary<string, object> columnFilter = new Dictionary<string, object>();
            columnFilter[DBM_Guild_User_Avatar.Columns.info] = info;
            columnFilter[DBM_Guild_User_Avatar.Columns.id_guild] = guildId.ToString();
            columnFilter[DBM_Guild_User_Avatar.Columns.id_user] = userId.ToString();
            new DBC().update(query, columnFilter);

            EmbedBuilder eb = new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithThumbnailUrl(TradingCardCore.Doremi.emojiOk);

            if (info == "")
                eb.WithDescription($"Your avatar info has been removed.");
            else
                eb.WithDescription($"Your avatar info has been updated.");

            await ReplyAsync(embed: eb
                .Build());
        }

        [Command("set nickname", RunMode = RunMode.Async), Summary("Set your avatar nickname." +
            "You can put empty parameter to remove the nickname.")]
        public async Task set_avatar_nickname([Remainder] string nickname = "")
        {
            if (nickname.Length >= 15)
            {
                await ReplyAsync("Please enter shorter nickname. Maximum characters allowed: 15");
                return;
            }
            var guildId = Context.Guild.Id;
            var userId = Context.User.Id;

            var userData = GuildUserAvatarCore.getUserData(guildId, userId);

            string query = $"UPDATE {DBM_Guild_User_Avatar.tableName} " +
                $" SET {DBM_Guild_User_Avatar.Columns.nickname}=@{DBM_Guild_User_Avatar.Columns.nickname} " +
                $" WHERE {DBM_Guild_User_Avatar.Columns.id_guild}=@{DBM_Guild_User_Avatar.Columns.id_guild} AND " +
                $" {DBM_Guild_User_Avatar.Columns.id_user}=@{DBM_Guild_User_Avatar.Columns.id_user} ";
            Dictionary<string, object> columnFilter = new Dictionary<string, object>();
            columnFilter[DBM_Guild_User_Avatar.Columns.nickname] = nickname;
            columnFilter[DBM_Guild_User_Avatar.Columns.id_guild] = guildId.ToString();
            columnFilter[DBM_Guild_User_Avatar.Columns.id_user] = userId.ToString();
            new DBC().update(query, columnFilter);

            EmbedBuilder eb = new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithThumbnailUrl(TradingCardCore.Doremi.emojiOk);

            if (nickname == "")
                await ReplyAsync(embed: eb
                .WithDescription($"Your avatar nickname has been removed.")
                .Build());
            else
                await ReplyAsync(embed: eb
                .WithDescription($"Your avatar nickname has been updated.")
                .Build());
        }

        [Command("set birthday", RunMode = RunMode.Async), Summary(
            "Set your avatar birthday date with parameter format:`dd mm`.Example: `31 12`" +
            "You can put empty parameter to remove the birthday date.")]
        public async Task set_avatar_birthday(string d = "", string m = "")
        {
            var guildId = Context.Guild.Id;
            var userId = Context.User.Id;

            var userData = GuildUserAvatarCore.getUserData(guildId, userId);

            //search birthday if exists/not
            string query = $"SELECT * " +
                $" FROM {DBM_Guild_User_Birthday.tableName} " +
                $" WHERE {DBM_Guild_User_Birthday.Columns.id_guild}=@{DBM_Guild_User_Birthday.Columns.id_guild} AND " +
                $" {DBM_Guild_User_Birthday.Columns.id_user}=@{DBM_Guild_User_Birthday.Columns.id_user} ";
            Dictionary<string, object> columnFilter = new Dictionary<string, object>();
            columnFilter[DBM_Guild_User_Birthday.Columns.id_guild] = guildId.ToString();
            columnFilter[DBM_Guild_User_Birthday.Columns.id_user] = userId.ToString();
            var birthdayExists = new DBC().selectAll(query, columnFilter);

            if (d != "" && m != "")
            {
                int birthdayDate = 0; int birthdayMonth = 0;
                var isNumericDate = int.TryParse(d, out birthdayDate);
                var isNumericMonth = int.TryParse(m, out birthdayMonth);
                if (birthdayDate < 1 || birthdayDate > 31 ||
                    birthdayMonth < 1 || birthdayMonth > 12)
                {
                    await ReplyAsync("Please enter the correct birthday format: **dd/mm**.");
                    return;
                }

                columnFilter = new Dictionary<string, object>();
                columnFilter[DBM_Guild_User_Birthday.Columns.birthday_date] = DateTime.Parse($"1990-{m}-{d} 00:00:00");
                columnFilter[DBM_Guild_User_Birthday.Columns.id_guild] = guildId.ToString();
                columnFilter[DBM_Guild_User_Birthday.Columns.id_user] = userId.ToString();

                if (birthdayExists.Rows.Count <= 0)
                {
                    new DBC().insert(DBM_Guild_User_Birthday.tableName, columnFilter);
                }
                else
                {
                    query = $"UPDATE {DBM_Guild_User_Birthday.tableName} " +
                    $" SET {DBM_Guild_User_Birthday.Columns.birthday_date}=@{DBM_Guild_User_Birthday.Columns.birthday_date} " +
                    $" WHERE {DBM_Guild_User_Birthday.Columns.id_guild}=@{DBM_Guild_User_Birthday.Columns.id_guild} AND " +
                    $" {DBM_Guild_User_Birthday.Columns.id_user}=@{DBM_Guild_User_Birthday.Columns.id_user} ";
                    new DBC().update(query, columnFilter);
                }

                await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithDescription($"Your avatar birthday date has been updated.")
                    .WithThumbnailUrl(TradingCardCore.Doremi.emojiOk)
                    .Build());
            }
            else
            {//remove the birthday date
                query = $"DELETE FROM {DBM_Guild_User_Birthday.tableName} " +
                $" WHERE {DBM_Guild_User_Birthday.Columns.id_guild}=@{DBM_Guild_User_Birthday.Columns.id_guild} AND " +
                $" {DBM_Guild_User_Birthday.Columns.id_user}=@{DBM_Guild_User_Birthday.Columns.id_user}";
                columnFilter = new Dictionary<string, object>();
                columnFilter[DBM_Guild_User_Birthday.Columns.id_guild] = guildId.ToString();
                columnFilter[DBM_Guild_User_Birthday.Columns.id_user] = userId.ToString();
                new DBC().delete(query, columnFilter);

                await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithDescription($"Your birthday date has been removed.")
                    .WithThumbnailUrl(TradingCardCore.Doremi.emojiOk)
                    .Build());
            }

        }

        [Command("set color", RunMode = RunMode.Async), Summary("Set your avatar embed color with rgb color format parameter." +
            "Example: `254 254 254`")]
        public async Task set_avatar_color(string r, string g, string b)
        {
            var isNumericR = int.TryParse(r, out _);
            var isNumericG = int.TryParse(g, out _);
            var isNumericB = int.TryParse(b, out _);

            if (!isNumericR && !isNumericG && !isNumericB)
            {
                await ReplyAsync("Please enter rgb values between 0 and 254"); return;
            }

            if (Convert.ToInt32(r) < 0 || Convert.ToInt32(r) > 254 ||
            Convert.ToInt32(g) < 0 || Convert.ToInt32(g) > 254 ||
            Convert.ToInt32(b) < 0 || Convert.ToInt32(b) > 254)
            {
                await ReplyAsync("Please enter rgb values between 0 and 254"); return;
            }
            var guildId = Context.Guild.Id;
            var userId = Context.User.Id;
            string color = $"{r},{g},{b}";
            var userData = GuildUserAvatarCore.getUserData(guildId, userId);

            string query = $"UPDATE {DBM_Guild_User_Avatar.tableName} " +
                $" SET {DBM_Guild_User_Avatar.Columns.color}=@{DBM_Guild_User_Avatar.Columns.color} " +
                $" WHERE {DBM_Guild_User_Avatar.Columns.id_guild}=@{DBM_Guild_User_Avatar.Columns.id_guild} AND " +
                $" {DBM_Guild_User_Avatar.Columns.id_user}=@{DBM_Guild_User_Avatar.Columns.id_user} ";
            Dictionary<string, object> columnFilter = new Dictionary<string, object>();
            columnFilter[DBM_Guild_User_Avatar.Columns.color] = color;
            columnFilter[DBM_Guild_User_Avatar.Columns.id_guild] = guildId.ToString();
            columnFilter[DBM_Guild_User_Avatar.Columns.id_user] = userId.ToString();
            new DBC().update(query, columnFilter);

            await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription($"Your avatar embed color has been updated!")
                .WithThumbnailUrl(TradingCardCore.Doremi.emojiOk)
                .Build());
        }

        [Command("set banner", RunMode = RunMode.Async),Alias("set image"), Summary("Set your avatar image banner with the provided link." +
            " You can the empty parameter to remove the banner image.")]
        public async Task set_avatar_banner(string image_url = "")
        {
            var guildId = Context.Guild.Id;
            var userId = Context.User.Id;

            string query = $"UPDATE {DBM_Guild_User_Avatar.tableName} " +
                $" SET {DBM_Guild_User_Avatar.Columns.image_url}=@{DBM_Guild_User_Avatar.Columns.image_url} " +
                $" WHERE {DBM_Guild_User_Avatar.Columns.id_guild}=@{DBM_Guild_User_Avatar.Columns.id_guild} AND " +
                $" {DBM_Guild_User_Avatar.Columns.id_user}=@{DBM_Guild_User_Avatar.Columns.id_user} ";
            Dictionary<string, object> columnFilter = new Dictionary<string, object>();
            columnFilter[DBM_Guild_User_Avatar.Columns.image_url] = image_url.ToString();
            columnFilter[DBM_Guild_User_Avatar.Columns.id_guild] = guildId.ToString();
            columnFilter[DBM_Guild_User_Avatar.Columns.id_user] = userId.ToString();
            new DBC().update(query, columnFilter);

            EmbedBuilder eb = new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithThumbnailUrl(TradingCardCore.Doremi.emojiOk);

            if (image_url == "")
                await ReplyAsync(embed: eb
                .WithDescription($"Your avatar banner has been removed.")
                .Build());
            else
                await ReplyAsync(embed: eb
                .WithDescription($"Your avatar banner has been updated.")
                .Build());
        }
    }

    [Name("Card"), Group("card"), Summary("This category contains all Doremi Trading card command.")]
    public class DoremiTradingCardInteractive : InteractiveBase
    {
        //archived for now
        //[Command("register", RunMode = RunMode.Async), Summary("Register your configuration for trading cards group command.")]
        //public async Task trading_card_register()
        //{
        //    var guildId = Context.Guild.Id;
        //    var clientId = Context.User.Id;
        //    string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
        //    string playerSaveDataDirectory = $"config/{Core.headTradingCardSaveConfigFolder}/{Context.User.Id}.json";

        //    if (!File.Exists(playerDataDirectory))
        //    {
        //        if (File.Exists(playerSaveDataDirectory))
        //        {
        //            File.Copy(playerSaveDataDirectory, $@"{playerDataDirectory}");
        //            //modify the catch token
        //            try
        //            {
        //                JObject arrInventory = JObject.Parse(File.ReadAllText(playerDataDirectory));
        //                arrInventory["catch_token"] = Config.Guild.getPropertyValue(guildId, TradingCardCore.propertyToken);
        //                File.WriteAllText(playerDataDirectory, arrInventory.ToString());

        //                await ReplyAsync(embed: new EmbedBuilder()
        //                .WithColor(Config.Doremi.EmbedColor)
        //                .WithDescription($":white_check_mark: Your trading card data has been successfully loaded! " +
        //                $"To keep the progress balanced, you can't catch any card on this spawn turn.")
        //                .WithThumbnailUrl(TradingCardCore.Doremi.emojiOk).Build());

        //            }
        //            catch {
        //                await ReplyAsync(embed: new EmbedBuilder()
        //                .WithColor(Config.Doremi.EmbedColor)
        //                .WithDescription($":x: Sorry, something went wrong. Please try the command again or seek help assistance from bot support team.")
        //                .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build());
        //            }
        //        } else
        //        {
        //            File.Copy($@"{Config.Core.headConfigFolder}trading_card_template_data.json", $@"{playerDataDirectory}");
        //            await ReplyAsync(embed: new EmbedBuilder()
        //            .WithColor(Config.Doremi.EmbedColor)
        //            .WithDescription($":white_check_mark: Your trading card data has been successfully registered. " +
        //            $"You can see the tutorial/guide with: **{Config.Doremi.PrefixParent[0]}card guide starter**/" +
        //            $"**{Config.Doremi.PrefixParent[0]}card guide mystery guide**/" +
        //            $"**{Config.Doremi.PrefixParent[0]}card guide bad card**")
        //            .WithThumbnailUrl(TradingCardCore.Doremi.emojiOk).Build());
        //        }

        //    }
        //    else
        //    {
        //        await ReplyAsync(embed: new EmbedBuilder()
        //        .WithColor(Config.Doremi.EmbedColor)
        //        .WithDescription($":x: Sorry, your trading card data has been registered already. " +
        //        $"Please delete with **{Config.Doremi.PrefixParent[0]}card delete** if you want to starting over from beginning.")
        //        .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build());
        //    }
        //}

        //[Command("save", RunMode = RunMode.Async), Summary("Make a save file backup from this server that you can continue on another server.")]
        //public async Task trading_card_save_file()
        //{
        //    string sourceFileName = $"{Config.Core.headConfigGuildFolder}{Context.Guild.Id}/{Config.Core.headTradingCardConfigFolder}/{Context.User.Id}.json";
        //    string destFileName = $"config/{Core.headTradingCardSaveConfigFolder}/{Context.User.Id}.json";

        //    if (File.Exists(sourceFileName))
        //    {
        //        File.Copy(sourceFileName, destFileName, true);
        //        await ReplyAsync(embed: new EmbedBuilder()
        //        .WithColor(Config.Doremi.EmbedColor)
        //        .WithDescription($":white_check_mark: Your trading card data on this server: **{Context.Guild.Name}** has been successfully saved! " +
        //        $"On the next time you're using the register command, your card data will be continued with the latest save data.")
        //        .WithThumbnailUrl(TradingCardCore.Doremi.emojiOk).Build());
        //    }
        //    else
        //    {
        //        await ReplyAsync(embed: new EmbedBuilder()
        //        .WithColor(Config.Doremi.EmbedColor)
        //        .WithDescription(":x: Sorry, you don't have trading card data on this server yet.")
        //        .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build());
        //    }
        //}

        //[Command("delete", RunMode = RunMode.Async), Summary("Delete all of your trading card data on this server.")]
        //public async Task trading_card_new_file()
        //{
        //    var guildId = Context.Guild.Id;
        //    var clientId = Context.User.Id;

        //    string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId.ToString()}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
        //    string sourceFileName = $"config/{Core.headTradingCardSaveConfigFolder}/{Context.User.Id}.json";


        //    if (!File.Exists(playerDataDirectory))
        //    {
        //        await ReplyAsync(embed: new EmbedBuilder()
        //        .WithColor(Config.Doremi.EmbedColor)
        //        .WithDescription($"I'm sorry, please register yourself first with **{Config.Doremi.PrefixParent[0]}card register** command.")
        //        .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build());
        //    }
        //    else
        //    {
        //        var timeoutDuration = TimeSpan.FromSeconds(60);

        //        //select user
        //        string captcha = GlobalFunctions.RandomString(5);

        //        IUserMessage msg = await ReplyAsync(embed: new EmbedBuilder()
        //            .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //            .WithTitle("Are you sure you want to delete your card data on this server?")
        //            .WithDescription($"Please read these rules & notes that applied bellow:\n" +
        //            $"-**THIS ACTIONS ARE NOT REVERSIBLE! " +
        //            $"MAKE SURE YOU HAVE CREATE YOUR SAVE FILE IF YOU WANT TO LOAD YOUR SAVE DATA AGAIN!**\n" +
        //            $"-All of your card data on this server will be deleted!\n" +
        //            $"If you really want to delete the save data on this server please enter this confirmation code: **{captcha}**")
        //            .WithColor(Config.Doremi.EmbedColor)
        //            .Build());
        //        var response = await NextMessageAsync(timeout: timeoutDuration);

        //        try
        //        {
        //            var checkNull = response.Content.ToLower().ToString();
        //        }
        //        catch
        //        {
        //            await Context.Channel.DeleteMessageAsync(response.Id);
        //            await Context.Channel.DeleteMessageAsync(msg);
        //            await ReplyAsync(":stopwatch: I'm sorry, you have reach your timeout. " +
        //            $"Please use the `{Config.Doremi.PrefixParent[0]}card delete` command again to retry the delete process.");
        //            return;
        //        }

        //        await Context.Channel.DeleteMessageAsync(response.Id);
        //        await Context.Channel.DeleteMessageAsync(msg);

        //        if (response.Content.ToString() == captcha)
        //        {
        //            File.Delete(playerDataDirectory);
        //            await ReplyAsync(embed: new EmbedBuilder()
        //            .WithTitle("Your card data has been deleted!")
        //            .WithColor(Config.Doremi.EmbedColor)
        //            .Build());
        //        } else
        //        {
        //            await ReplyAsync(embed: new EmbedBuilder()
        //            .WithTitle(":x: That is not the correct confirmation code. Card data deletion process has been cancelled.")
        //            .WithColor(Config.Doremi.EmbedColor)
        //            .Build());
        //        }

        //        return;

        //    }
        //}

        [Name("card guide"), Group("guide"), Alias("tutorial"), Summary("These commands contains FAQ/guide for ojamajo trading card.")]
        public class DoremiModeratorTradingCardsFAQ : ModuleBase<SocketCommandContext>
        {
            [Command("starter", RunMode = RunMode.Async), Summary("Shows the basic guide for ojamajo trading card.")]
            public async Task trading_card_faq_gettingStarted()
            {
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithTitle($"📚 Ojamajo Trading Card - Guide 101")
                    .AddField("How can I capture the card?",
                    $"You can use **<bot>card capture** to capture the card. " +
                    $"<bot> prefix available are: **{Config.Doremi.PrefixParent[0]} / {Config.Hazuki.PrefixParent[0]} / " +
                    $"{Config.Aiko.PrefixParent[0]} / {Config.Onpu.PrefixParent[0]} / " +
                    $"{Config.Momoko.PrefixParent[0]}**," +
                    $"but **BEWARE OF MYSTERY CARD & BAD CARD!** (More explanation of mystery/bad card can be see on each faq). " +
                    $"If the card has been captured by someone you need to wait it until the next card spawn.")
                    .AddField("**How many card pack & type available upon the card spawn?**",
                    $"-6 cards pack: Doremi, Hazuki, Aiko, Onpu, Momoko & Other Pack.\n" +
                    $"-4 cards type: Normal ({TradingCardCore.captureRateNormal * 10} catch rate%), " +
                    $"Platinum ({TradingCardCore.captureRatePlatinum * 10}% catch rate), " +
                    $"Metal ({TradingCardCore.captureRateMetal * 10}% catch rate) & " +
                    $"Special({TradingCardCore.captureRateSpecial * 10}% catch rate, exclusive for **other-special** card Pack only).")
                    .AddField("What is Rank on my card status info?",
                    "Your rank are determined based from your card capture exp. You will start from rank 1 and can be raised up to 5. " +
                    "For each time you're using the card capture command you'll get **1 exp** and will get rank up for every 100 exp. " +
                    "Starting from rank 2 and above you will gain free **+10/20/30/40% capture rate benefit** for each rank that you have.")
                    .AddField("**Getting Started**",
                    $"-Gather daily magic seeds everyday (24 hour bot server time reset) with **{Config.Doremi.PrefixParent[0]}daily**. " +
                    $"Magic seeds can be used for buying item for card collecting progression.\n" +
                    $"-Capture the card based from the card spawn rules. Example: {Config.Doremi.PrefixParent[0]}card capture\n" +
                    $"-You can visit card shop for card collecting progression with: **{Config.Doremi.PrefixParent[0]}card shop**.\n" +
                    $"-To see your card report, you can use **<bot>!card inventory** or **<bot>!card status**\n" +
                    $"For more card command & help you can use **<bot>!help card**")
                    .WithThumbnailUrl("https://cdn.discordapp.com/attachments/706770454697738300/706770837558263928/TW361403.png")
                    .WithFooter($"Latest Revision: v1.31")
                    .Build());
            }

            [Command("mystery card", RunMode = RunMode.Async), Alias("mysterycard"), Summary("Shows all information regarding the mystery card.")]
            public async Task trading_card_faq_mysteryCard()
            {
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithTitle($"❓ Ojamajo Trading Card - Mystery Card FAQ")
                    .WithDescription($"Mystery card is a hidden card that contain more capture rate with a provided clue that you need to guess/answer it " +
                    $"by calling the correct ojamajo bot. " +
                    $"Should you answer it wrong you'll lose a chance to capture the card for that turn.")
                    .AddField("**How to capture mystery card?**",
                    "It's same one with normal capture flow & rules, but like previously explained: there will be a set of provided clue and you need to guess/answer it " +
                    $"with the correct ojamajo bot. " +
                    $"You can find the answer by looking up through the **bio** " +
                    $"You only have a chance to guess the mystery card, if you guess it wrong you have to wait for the next card spawn.")
                    .AddField("Provided hint/clue:",
                    "Number translator: Translate a set of number based from the alphabet order number. Example:1:A,2:B,3:C,etc. \n" +
                    ":birthday: Birthday date\n" +
                    ":woman_fairy: The ojamajos that have this fairy\n" +
                    ":sparkles: Guess the missing spell name that don't have this spell\n" +
                    ":girl: The front/surname of the ojamajos\n" +
                    ":fork_and_knife: Favorite food")
                    .WithThumbnailUrl("https://cdn.discordapp.com/attachments/709293222387777626/710869697972797440/mystery.jpg")
                    .WithFooter($"Latest Revision: v1.31")
                    .Build());
            }

            [Command("bad card", RunMode = RunMode.Async), Alias("badcard"), Summary("Shows all information regarding the bad card.")]
            public async Task trading_card_faq_badCard()
            {
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithTitle($"💀 Ojamajo Trading Card - Bad Card FAQ")
                    .WithDescription($"Bad card are attached upon card spawn and extremely **dangerous** when capture command is called. " +
                    $"It need to be removed first with **{Config.Doremi.PrefixParent[0]}card pureleine** before using card capture commands!")
                    .AddField("**How many type of bad cards?**", "There are 3 type of bad cards:\n" +
                    "-**curse**: Steal one of your card after catch attempt. A **normal** bonus card will be rewarded upon removed.\n" +
                    "-**failure**: Drop your card catch rate into 0%. A **normal** bonus card will be rewarded upon removed.\n" +
                    "-**seeds**: Steal your magic seeds after catch attempt. Some magic seeds will be rewarded upon removed.")
                    .AddField("How to remove bad card?", $"You can remove the bad cards with **{Config.Doremi.PrefixParent[0]}card pureleine**. After seeing the question you need to answer with **do!card pureleine <answer>** commands. " +
                    "When oyajide have clean the bad card, you can safely capture the card again.")
                    .AddField("How to notice bad card?", 
                    " Bad cards are marked on the card spawn where there'll be a bad card image/mark attached upon it as seen on this guide.")
                    .AddField("Related command:",
                    "<bot>!card pureleine: see the bad card question\n" +
                    "<bot>!card pureleine <answer> : remove the bad card with given <answer> parameter. Example: **do!card pureleine 10**")
                    .WithThumbnailUrl("https://cdn.discordapp.com/attachments/706770454697738300/715945298370887722/latest.png")
                    .WithFooter($"Latest Revision: v1.31")
                    .Build());
            }

            [Command("zone card", RunMode = RunMode.Async), Alias("badcard"), Summary("Shows all information regarding the zone card.")]
            public async Task trading_card_faq_zoneCard()
            {
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithTitle($"Ojamajo Trading Card - Card Zone FAQ")
                    .WithDescription($"Zone card are a card type that is limited to: doremi/hazuki/aiko/onpu/momoko card pack. " +
                    $"This card spawn type let you catch card pack based from the assigned zone that you have set. " +
                    $"With some note: this card spawn are instanced individually so everyone will get a chance to catch a card even if the card has been captured by the others. " +
                    $"By default you will be assigned to **doremi normal** card zone.")
                    .AddField("Related command:",
                    "<bot>!card zone set <category>: set the card zone on doremi. Example: **do!card zone set platinum**\n" +
                    $"<bot>!card zone where: see your assigned card zone.\n" +
                    $"{Config.Doremi.PrefixParent[0]}card zone price: see the card zone price.")
                    .WithThumbnailUrl("https://cdn.discordapp.com/attachments/709293222387777626/710869697972797440/mystery.jpg")
                    .WithFooter($"Latest Revision: v1.31")
                    .Build());
            }
        }

        [Command("capture", RunMode = RunMode.Async), Alias("catch"), Summary("Capture spawned card with Doremi.")]
        public async Task<RuntimeResult> trading_card_doremi_capture(string boost = "")
        {
            //reference: https://www.newtonsoft.com/json/help/html/ModifyJson.htm
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;

            var guildSpawnData = TradingCardGuildCore.getGuildData(guildId);
            if (Convert.ToInt32(guildSpawnData[DBM_Trading_Card_Guild.Columns.spawn_is_zone]) == 1)
            {
                var userTradingCardData = UserTradingCardDataCore.getUserData(clientId);
                string userCardZone = userTradingCardData[DBM_User_Trading_Card_Data.Columns.card_zone].ToString();
                if (!userCardZone.Contains("doremi"))
                {
                    await ReplyAsync(":x: Sorry, you are not on the correct card zone. " +
                        $"Please assign yourself on the correct card zone with **{Config.Doremi.PrefixParent[0]}card zone set <category>** command.");
                    return Ok();
                }
            }

            //var cardCaptureReturn = TradingCardCore.cardCapture(Config.Doremi.EmbedColor, Context.Client.CurrentUser.GetAvatarUrl(), guildId, clientId, Context.User.Username,
            //TradingCardCore.Doremi.emojiError, "doremi", boost, Config.Doremi.PrefixParent[0], "do",
            //TradingCardCore.Doremi.maxNormal, TradingCardCore.Doremi.maxPlatinum, TradingCardCore.Doremi.maxMetal, TradingCardCore.Doremi.maxOjamajos);

            var cardCaptureReturn = TradingCardCore.cardCapture(Context, Config.Doremi.EmbedColor,
                TradingCardCore.Doremi.emojiError, "doremi", boost, "do");

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
                    .SendFileAsync(TradingCardCore.Aiko.imgCompleteAllCard, null, embed: TradingCardCore
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
                    .SendFileAsync(TradingCardCore.Aiko.imgCompleteAllCard, null, embed: TradingCardCore
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

                await Bot.Doremi.client
                    .GetGuild(Context.Guild.Id)
                    .GetTextChannel(Context.Channel.Id)
                    .SendFileAsync(TradingCardCore.Doremi.imgCompleteAllCard, null, embed: TradingCardCore
                    .userCompleteTheirList(Context, Config.Doremi.EmbedColor, Config.Doremi.EmbedAvatarUrl, "other",
                    TradingCardCore.imgCompleteAllCardSpecial, TradingCardCore.roleCompletionistSpecial)
                    .Build());

            }
            return Ok();

        }

        [Command("pureleine", RunMode = RunMode.Async), Alias("oyajide"), Summary("Detect the bad card with the help from oyajide & pureleine computer. " +
            "Insert the answer as parameter to remove the bad cards if it's existed. Example: do!card pureleine 10")]
        public async Task trading_card_pureleine(string answer = "")
        {
            await ReplyAsync(embed: TradingCardCore.activatePureleine(Context, answer).Build());
        }

        [Command("inventory", RunMode = RunMode.Async), Summary("Show the inventory of **Doremi** card category. " +
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
                        TradingCardCore.printInventory(Context, Config.Doremi.EmbedColor, "doremi", category, TradingCardCore.Doremi.maxNormal));
                }

                //platinum category
                if (showAllInventory || category.ToLower() == "platinum")
                {
                    category = "platinum";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Doremi.EmbedColor, "doremi", category, TradingCardCore.Doremi.maxPlatinum));
                }

                //metal category
                if (showAllInventory || category.ToLower() == "metal")
                {
                    category = "metal";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Doremi.EmbedColor, "doremi", category, TradingCardCore.Doremi.maxMetal));
                }

                //ojamajos category
                if (showAllInventory || category.ToLower() == "ojamajos")
                {
                    category = "ojamajos";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Doremi.EmbedColor, "doremi", category, TradingCardCore.Doremi.maxOjamajos));
                }

                //special category
                if (showAllInventory || category.ToLower() == "special")
                {
                    category = "special";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Doremi.EmbedColor, "other", category, TradingCardCore.maxSpecial));
                }


            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }

        }

        [Command("inventory", RunMode = RunMode.Async), Summary("Show the inventory of **Doremi** card category. " +
            "You can put optional parameter with this format: <bot>!card inventory <category> <username>.")]
        public async Task trading_card_inventory_other([Remainder] SocketGuildUser username = null)
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
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithDescription($"Sorry, I can't find that username. Please enter the correct username.")
                    .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build());
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
                        TradingCardCore.printInventory(Context, Config.Doremi.EmbedColor, "doremi", category, TradingCardCore.Doremi.maxNormal, username));
                }

                //platinum category
                if (showAllInventory || category.ToLower() == "platinum")
                {
                    category = "platinum";
                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Doremi.EmbedColor, "doremi", category, TradingCardCore.Doremi.maxPlatinum, username));
                }

                //metal category
                if (showAllInventory || category.ToLower() == "metal")
                {
                    category = "metal";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Doremi.EmbedColor, "doremi", category, TradingCardCore.Doremi.maxMetal, username));
                }

                //ojamajos category
                if (showAllInventory || category.ToLower() == "ojamajos")
                {
                    category = "ojamajos";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Doremi.EmbedColor, "doremi", category, TradingCardCore.Doremi.maxOjamajos, username));
                }

                //special category
                if (showAllInventory || category.ToLower() == "special")
                {
                    category = "special";
                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Doremi.EmbedColor, "other", category, TradingCardCore.maxSpecial, username));
                }

            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }


        }

        [Command("inventory", RunMode = RunMode.Async), Summary("Show the inventory of **Doremi** card category. " +
            "You can put optional parameter with this format: <bot>!card inventory <category> <username>.")]
        public async Task trading_card_inventory_category_other(string category = "", [Remainder] SocketGuildUser username = null)
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
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithDescription($"Sorry, I can't find that username. Please mention the correct username.")
                    .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build());
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
                        TradingCardCore.printInventory(Context, Config.Doremi.EmbedColor, "doremi", category, TradingCardCore.Doremi.maxNormal, username));
                }

                //platinum category
                if (showAllInventory || category.ToLower() == "platinum")
                {
                    category = "platinum";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Doremi.EmbedColor, "doremi", category, TradingCardCore.Doremi.maxPlatinum, username));
                }

                //metal category
                if (showAllInventory || category.ToLower() == "metal")
                {
                    category = "metal";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Doremi.EmbedColor, "doremi", category, TradingCardCore.Doremi.maxMetal, username));
                }

                //ojamajos category
                if (showAllInventory || category.ToLower() == "ojamajos")
                {
                    category = "ojamajos";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Doremi.EmbedColor, "doremi", category, TradingCardCore.Doremi.maxOjamajos, username));
                }

                //special category
                if (showAllInventory || category.ToLower() == "special")
                {
                    category = "special";
                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Doremi.EmbedColor, "other", category, TradingCardCore.maxSpecial, username));
                }

            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }


        }

        //pop card pack
        [Command("inventory pop", RunMode = RunMode.Async), Summary("Show the inventory of **Pop** card category.")]
        public async Task trading_card_inventory_pop_self(string category = "")
        {

            Boolean showAllInventory = true;
            if (category.ToLower() != "normal" && category.ToLower() != "")
            {
                await ReplyAsync($":x: Sorry, that is not the valid **pop** card category. " +
                $"Valid category: **normal**");
                return;
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
                        TradingCardCore.printInventory(Context, TradingCardCore.Pop.embedColor, "pop", category, TradingCardCore.Pop.maxNormal));
                }

                //check/trigger completionist:
                //if (((JArray)playerData["pop"]["normal"]).Count >= TradingCardCore.Pop.maxNormal)
                //{
                //    if (Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Pop.roleCompletionist).ToList().Count >= 1)
                //    {
                //        await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(
                //                            Context.Guild.Roles.First(x => x.Name == TradingCardCore.Pop.roleCompletionist)
                //                            );

                //        await Bot.Doremi.client
                //        .GetGuild(Context.Guild.Id)
                //        .GetTextChannel(Context.Channel.Id)
                //        .SendFileAsync(TradingCardCore.Pop.imgCompleteAllCard, null, embed: TradingCardCore
                //        .userCompleteTheirList(TradingCardCore.Pop.embedColor, Context.Client.CurrentUser.GetAvatarUrl(), "pop",
                //        TradingCardCore.Pop.imgCompleteAllCard, Context.Guild.Id.ToString(),
                //        Context.User.Id.ToString(), TradingCardCore.Pop.roleCompletionist, Context.User.Username, Context.User.GetAvatarUrl())
                //        .Build());
                //    }
                //}

            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }


        }

        [Command("inventory pop", RunMode = RunMode.Async), Summary("Show the inventory of **Pop** card category. " +
            "You can put optional parameter with this format: <bot>!card inventory pop <username>.")]
        public async Task trading_card_inventory_pop_other([Remainder] SocketGuildUser username = null)
        {
            string category = "";

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
                    .WithColor(Config.Pop.EmbedColor)
                    .WithDescription($"Sorry, I can't find that username. Please mention the correct username.")
                    .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build());
                    return;
                }
            }

            Boolean showAllInventory = true;

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
                        TradingCardCore.printInventory(Context, TradingCardCore.Pop.embedColor, "pop", category, TradingCardCore.Pop.maxNormal, username));
                }

            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }


        }

        //hana card pack
        [Command("inventory hana", RunMode = RunMode.Async), Summary("Show the inventory of **Hana** card category.")]
        public async Task trading_card_inventory_hana_self(string category = "")
        {
            Boolean showAllInventory = true;
            if (category.ToLower() != "normal" && category.ToLower() != "platinum" &&
                category.ToLower() != "metal" && category.ToLower() != "")
            {
                await ReplyAsync($":x: Sorry, that is not the valid **hana** card category. " +
                $"Valid category: **normal**/**platinum**/**metal**");
                return;
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
                        TradingCardCore.printInventory(Context, TradingCardCore.Hana.embedColor, "hana", category, TradingCardCore.Hana.maxNormal));
                }

                //platinum category
                if (showAllInventory || category.ToLower() == "platinum")
                {
                    category = "platinum";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, TradingCardCore.Hana.embedColor, "hana", category, TradingCardCore.Hana.maxPlatinum));
                }

                //metal category
                if (showAllInventory || category.ToLower() == "metal")
                {
                    category = "metal";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, TradingCardCore.Hana.embedColor, "hana", category, TradingCardCore.Hana.maxMetal));
                }

            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }

        }

        [Command("verify", RunMode = RunMode.Async), Summary("Verify the doremi,other,pop & hana card pack " +
            "to get the card completion role & badge leaderboard on this server  if you have complete it already.")]
        public async Task verify_card_completion()
        {
            var guildId = Context.Guild.Id;
            var userId = Context.User.Id;
            string userAvatarUrl = Context.User.GetAvatarUrl();
            string username = Context.User.Username;

            string cardPack = "doremi";

            if (UserTradingCardDataCore.checkCardCompletion(userId, cardPack))
            {
                try
                {
                    if (Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Doremi.roleCompletionist).ToList().Count >= 1)
                    {
                        await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(
                            Context.Guild.Roles.First(x => x.Name == TradingCardCore.Doremi.roleCompletionist)
                        );
                    }
                } catch(Exception e) { }
                

                EmbedBuilder embedReturn = TradingCardCore
                    .userCompleteTheirList(Context, Config.Doremi.EmbedColor, Config.Doremi.EmbedAvatarUrl, cardPack,
                    TradingCardCore.Doremi.imgCompleteAllCard, TradingCardCore.Doremi.roleCompletionist);

                if (embedReturn != null)
                {
                    await Bot.Doremi.client
                    .GetGuild(Context.Guild.Id)
                    .GetTextChannel(Context.Channel.Id)
                    .SendFileAsync(TradingCardCore.Doremi.imgCompleteAllCard, null, embed: embedReturn
                    .Build());
                }
                else
                {
                    await ReplyAsync(":white_check_mark: Your **doremi** card completion status has been verified");
                }

            }

            if (UserTradingCardDataCore.checkCardCompletion(userId, "other"))
            {
                try
                {
                    if (Context.Guild.Roles.Where(x => x.Name == TradingCardCore.roleCompletionistSpecial).ToList().Count >= 1)
                    {
                        await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(
                            Context.Guild.Roles.First(x => x.Name == TradingCardCore.roleCompletionistSpecial)
                        );
                    }
                } catch(Exception e) { }

                EmbedBuilder embedReturn = TradingCardCore
                    .userCompleteTheirList(Context, Config.Doremi.EmbedColor, Config.Doremi.EmbedAvatarUrl, "other",
                    TradingCardCore.imgCompleteAllCardSpecial, TradingCardCore.roleCompletionistSpecial);

                if (embedReturn != null)
                {
                    await Bot.Doremi.client
                    .GetGuild(Context.Guild.Id)
                    .GetTextChannel(Context.Channel.Id)
                    .SendFileAsync(TradingCardCore.imgCompleteAllCardSpecial, null, embed: embedReturn
                    .Build());
                } else
                {
                    await ReplyAsync(":white_check_mark: Your **special** card completion status has been verified");
                }

            }

            if (UserTradingCardDataCore.checkCardCompletion(userId, "pop"))
            {
                try
                {
                    if (Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Pop.roleCompletionist).ToList().Count >= 1)
                    {
                        await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(
                            Context.Guild.Roles.First(x => x.Name == TradingCardCore.Pop.roleCompletionist)
                        );
                    }
                } catch(Exception e) { }
                
                EmbedBuilder embedReturn = TradingCardCore
                .userCompleteTheirList(Context, TradingCardCore.Hana.embedColor, Config.Doremi.EmbedAvatarUrl, "pop",
                TradingCardCore.Pop.imgCompleteAllCard, TradingCardCore.Pop.roleCompletionist);

                if (embedReturn != null)
                {
                    await Bot.Doremi.client
                    .GetGuild(Context.Guild.Id)
                    .GetTextChannel(Context.Channel.Id)
                    .SendFileAsync(TradingCardCore.Pop.imgCompleteAllCard, null, embed: embedReturn
                    .Build());
                } else
                {
                    await ReplyAsync(":white_check_mark: Your **pop** card completion status has been verified");
                }

            }

            if (UserTradingCardDataCore.checkCardCompletion(userId, "hana"))
            {
                try
                {
                    if (Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Hana.roleCompletionist).ToList().Count >= 1)
                    {
                        await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(
                            Context.Guild.Roles.First(x => x.Name == TradingCardCore.Hana.roleCompletionist)
                        );
                    }
                } catch(Exception e){}
                    

                EmbedBuilder embedReturn = TradingCardCore
                .userCompleteTheirList(Context, TradingCardCore.Hana.embedColor, Config.Doremi.EmbedAvatarUrl, "hana",
                TradingCardCore.Hana.imgCompleteAllCard, TradingCardCore.Hana.roleCompletionist);

                if (embedReturn != null)
                {
                    await Bot.Doremi.client
                    .GetGuild(Context.Guild.Id)
                    .GetTextChannel(Context.Channel.Id)
                    .SendFileAsync(TradingCardCore.Hana.imgCompleteAllCard, null, embed: embedReturn
                    .Build());
                } else
                {
                    await ReplyAsync(":white_check_mark: Your **hana** card completion status has been verified");
                }
            }

        }


        [Command("inventory hana", RunMode = RunMode.Async), Summary("Show the inventory of **Hana** card category. " +
           "You can put optional parameter with this format: <bot>!card inventory hana <category>.")]
        public async Task trading_card_inventory_hana_other([Remainder] SocketGuildUser username = null)
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
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithDescription($"Sorry, I can't find that username. Please mention the correct username.")
                    .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build());
                    return;
                }
            }

            string category = "";

            Boolean showAllInventory = true;

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
                        TradingCardCore.printInventory(Context, TradingCardCore.Hana.embedColor, "hana", category, TradingCardCore.Hana.maxNormal, username));
                }

                //platinum category
                if (showAllInventory || category.ToLower() == "platinum")
                {
                    category = "platinum";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, TradingCardCore.Hana.embedColor, "hana", category, TradingCardCore.Hana.maxPlatinum, username));
                }

                //metal category
                if (showAllInventory || category.ToLower() == "metal")
                {
                    category = "metal";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, TradingCardCore.Hana.embedColor, "hana", category, TradingCardCore.Hana.maxMetal, username));
                }

            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }


        }

        [Command("inventory hana", RunMode = RunMode.Async), Summary("Show the inventory of **Hana** card category. " +
            "You can put optional parameter with this format: <bot>!card inventory hana <category> <username>.")]
        public async Task trading_card_inventory_hana_category_other(string category = "", [Remainder] SocketGuildUser username = null)
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
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithDescription($"Sorry, I can't find that username. Please mention the correct username.")
                    .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build());
                    return;
                }
            }


            Boolean showAllInventory = true;
            if (category.ToLower() != "normal" && category.ToLower() != "platinum" && category.ToLower() != "metal" &&
                category.ToLower() != "")
            {
                await ReplyAsync($":x: Sorry, that is not the valid **hana** pack category. " +
                $"Valid category: **normal**/**platinum**/**metal**");
                return;
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
                        TradingCardCore.printInventory(Context, TradingCardCore.Hana.embedColor, "hana", category, TradingCardCore.Hana.maxNormal, username));
                }

                //platinum category
                if (showAllInventory || category.ToLower() == "platinum")
                {
                    category = "platinum";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, TradingCardCore.Hana.embedColor, "hana", category, TradingCardCore.Hana.maxNormal, username));
                }

                //metal category
                if (showAllInventory || category.ToLower() == "metal")
                {
                    category = "metal";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, TradingCardCore.Hana.embedColor, "hana", category, TradingCardCore.Hana.maxNormal, username));
                }

            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }

        }



        //[Command("part inventory", RunMode = RunMode.Async), Summary("Open the card part inventory.")]
        //public async Task open_card_part_inventory()
        //{
        //    var guildId = Context.Guild.Id;
        //    var clientId = Context.User.Id;

        //    string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId.ToString()}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";

        //    if (!File.Exists(playerDataDirectory))
        //    {
        //        await ReplyAsync(embed: new EmbedBuilder()
        //        .WithColor(Config.Doremi.EmbedColor)
        //        .WithDescription($"I'm sorry, please register yourself first with **{Config.Doremi.PrefixParent[0]}card register** command.")
        //        .WithThumbnailUrl(TradingCardCore.Doremi.emojiError)
        //        .Build()); return;
        //    }
        //    else
        //    {
        //        JObject arrInventory = JObject.Parse(File.ReadAllText(playerDataDirectory));
        //        if (((JArray)arrInventory["event_inventory"]).Count() >= 1)
        //        {
        //            await PagedReplyAsync(
        //            TradingCardCore.CardEvent.printInventoryTemplate(
        //                Config.Doremi.EmbedColor, arrInventory, Context.User.Username, Context.User.GetAvatarUrl())
        //                );
        //        }
        //        else
        //        {
        //            await ReplyAsync(
        //            embed: TradingCardCore.CardEvent.printEmptyInventoryTemplate(Config.Doremi.EmbedColor, Context.User.Username)
        //            .Build());
        //        }

        //    }
        //}

        [Command("detail", RunMode = RunMode.Async), Alias("info", "look"), Summary("See the detail of Doremi card information from the <card_id>.")]
        public async Task trading_card_look(string card_id)
        {
            await ReplyAsync(null, embed: TradingCardCore.printCardDetailTemplate(Context, Config.Doremi.EmbedColor, card_id, TradingCardCore.Doremi.emojiError)
                    .Build());
        }

        [Command("status", RunMode = RunMode.Async), Summary("Show your Trading Card Status. " +
            "You can add the optional username parameter to see the card status of that user.")]
        public async Task trading_card_status([Remainder] SocketGuildUser otherUser = null)
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
                        printStatusTemplate(Context, Config.Doremi.EmbedColor, otherUser)
                        .Build());

        }

        [Command("badge", RunMode = RunMode.Async),Alias("status complete"), Summary("Show your trading card badge/completionist date status. " +
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
                        printStatusComplete(Context, Config.Doremi.EmbedColor, otherUser)
                        .Build());
        }

        //show top 5 that capture each card pack
        [Command("leaderboard", RunMode = RunMode.Async), Summary("Show top 5 doremi trading card leaderboard status.")]
        public async Task trading_card_leaderboard()
        {
            await ReplyAsync(embed: TradingCardCore.
                    printLeaderboardTemplate(Context, Config.Doremi.EmbedColor, "doremi")
                    .Build());
        }

        //trade
        //[Command("trade", RunMode = RunMode.Async), Summary("Open the trading card hub which lets you trade the card with each other.")]
        //public async Task<RuntimeResult> trading_card_trade()
        //{
        //    await ReplyAndDeleteAsync(null, embed: new EmbedBuilder()
        //            .WithColor(Config.Doremi.EmbedColor)
        //            .WithDescription($":x: Sorry, trade command is under maintenance now.")
        //            .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build(), timeout: TimeSpan.FromSeconds(10));

        //    return Ok();

        //    var guildId = Context.Guild.Id;
        //    var clientId = Context.User.Id;

        //    if (!Config.Doremi.isRunningInteractive.ContainsKey(Context.User.Id.ToString()))
        //        Config.Doremi.isRunningInteractive.Add(Context.User.Id.ToString(), false);

        //    if (!Config.Doremi.isRunningInteractive[Context.User.Id.ToString()])
        //    {
        //        string userFolderDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}";

        //        string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
        //        var jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));

        //        PaginatedAppearanceOptions pao = new PaginatedAppearanceOptions();
        //        pao.JumpDisplayOptions = JumpDisplayOptions.Never;
        //        pao.DisplayInformationIcon = false;

        //        if (!File.Exists(playerDataDirectory)) //not registered yet
        //        {
        //            await Context.Message.DeleteAsync();
        //            await ReplyAndDeleteAsync(null, embed: new EmbedBuilder()
        //            .WithColor(Config.Doremi.EmbedColor)
        //            .WithDescription($":x: I'm sorry, please register yourself first with **{Config.Doremi.PrefixParent[0]}card register** command.")
        //            .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build(), timeout: TimeSpan.FromSeconds(10));

        //            return Ok();
        //        }
        //        else
        //        {
        //            var playerData = JObject.Parse(File.ReadAllText(playerDataDirectory));
        //            int fileCount = Directory.GetFiles(userFolderDirectory).Length;

        //            if (fileCount <= 1)
        //            {
        //                await ReplyAndDeleteAsync("Sorry, server need to have more than 1 user that register the trading card data.", timeout: TimeSpan.FromSeconds(10));
        //                return Ok();
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    //start read all user id
        //                    List<string> arrUserId = new List<string>();
        //                    List<string> pageContent = new List<string>();
        //                    List<string> pageContentUserList = new List<string>();
        //                    //List<string> pageUserContent = new List<string>();
        //                    //List<string> pageUserCardList = new List<string>();
        //                    //List<string> pageUserOtherCardList = new List<string>();

        //                    //selection variables
        //                    //other users
        //                    string selectionUserId = ""; string selectionOtherUserCardChoiceId = ""; string selectionOtherUserCardPack = "";
        //                    string selectionOtherUserCardCategory = "";
        //                    //your selection
        //                    string selectionYourCardChoiceId = ""; string selectionYourCardPack = "";
        //                    string selectionYourCardCategory = "";

        //                    DirectoryInfo d = new DirectoryInfo(userFolderDirectory);//Assuming Test is your Folder
        //                    FileInfo[] Files = d.GetFiles("*.json"); //Getting Text files

        //                    //user selection
        //                    string titleUserSelection = $"**Step 1 - Select the user with numbers**\n";
        //                    string tempVal = titleUserSelection;
        //                    int currentIndex = 0;

        //                    int ctr = 0;
        //                    foreach (FileInfo file in Files)
        //                    {
        //                        ulong otherUserId = Convert.ToUInt64(Path.GetFileNameWithoutExtension(file.Name));
        //                        var iguilduser = Context.Guild.Users.FirstOrDefault(x => x.Id == otherUserId);

        //                        //var available = iguilduser.Guild.GetUserAsync(otherUserId);
        //                        if (otherUserId != clientId && iguilduser != null)
        //                        {
        //                            arrUserId.Add(otherUserId.ToString());
        //                        }
        //                    }

        //                    for (int i = 0; i < arrUserId.Count; i++)
        //                    {
        //                        var iguilduser = Context.Guild.Users.FirstOrDefault(x => x.Id == Convert.ToUInt64(arrUserId[i]));
        //                        tempVal += $"**{i + 1}. {MentionUtils.MentionUser(iguilduser.Id)}**\n";

        //                        if (i == arrUserId.Count - 2) pageContentUserList.Add(tempVal);
        //                        else
        //                        {
        //                            if (currentIndex < 14) currentIndex++;
        //                            else
        //                            {
        //                                pageContentUserList.Add(tempVal);
        //                                currentIndex = 0;
        //                                tempVal = titleUserSelection;
        //                            }
        //                        }
        //                    }

        //                    var pagerUserList = new PaginatedMessage
        //                    {
        //                        Pages = pageContentUserList,
        //                        Color = Config.Doremi.EmbedColor,
        //                        Options = pao
        //                    };
        //                    //end user selection

        //                    Boolean isTrading = true;
        //                    var timeoutDuration = TimeSpan.FromSeconds(60);
        //                    string replyTimeout = ":stopwatch: I'm sorry, but you have reach your timeout. " +
        //                        $"Please use the `{Config.Doremi.PrefixParent[0]}card trade` command again to retry the trade process.";
        //                    int stepProcess = 1;//0/1:select the user,
        //                                        //2: select your card pack, 3: select card category, 4: select 
        //                                        //5:review process
        //                    Boolean newStep = true;
        //                    IUserMessage msg; IUserMessage msg2;
        //                    //select user
        //                    msg = await ReplyAsync(embed: new EmbedBuilder()
        //                        .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                        .WithDescription($"Welcome to {TradingCardCore.Doremi.embedName}. " +
        //                        $"Here you can trade your trading card with each other. " +
        //                        $"You can type **cancel**/**exit** anytime on each steps to cancel the trade process.\n" +
        //                        $"You can type **back** anytime on each steps to back into previous steps.\n" +
        //                        $"Some of the trade rules that will be applied:\n" +
        //                        $"-You **cannot** trade more than once to the same user at the same trade queue.\n" +
        //                        $"-You **cannot** trade card that you or that user already had.")
        //                        .WithColor(Config.Doremi.EmbedColor)
        //                        .Build());
        //                    Config.Doremi.isRunningInteractive[Context.User.Id.ToString()] = true;
        //                    msg2 = await PagedReplyAsync(pagerUserList);
        //                    var response = await NextMessageAsync(timeout: timeoutDuration);
        //                    newStep = false;
        //                    while (isTrading)
        //                    {
        //                        List<string> arrUserCardList = new List<string>();
        //                        List<string> arrUserOtherCardList = new List<string>();

        //                        try
        //                        {
        //                            var checkNull = response.Content.ToLower().ToString();
        //                        }
        //                        catch
        //                        {
        //                            try
        //                            {
        //                                await Context.Channel.DeleteMessageAsync(msg.Id);
        //                                await Context.Channel.DeleteMessageAsync(msg2.Id);
        //                            }
        //                            catch { }

        //                            await ReplyAndDeleteAsync(replyTimeout, timeout: TimeSpan.FromSeconds(15));
        //                            isTrading = false;
        //                            Config.Doremi.isRunningInteractive[Context.User.Id.ToString()] = false;
        //                            return Ok();
        //                        }

        //                        //response = await NextMessageAsync(timeout: timeoutDuration);
        //                        //string responseText = response.Content.ToLower().ToString();

        //                        if (response.Content.ToString().ToLower() == "cancel" ||
        //                            response.Content.ToString().ToLower() == "exit")
        //                        {
        //                            try
        //                            {
        //                                await Context.Channel.DeleteMessageAsync(response.Id);
        //                                await Context.Channel.DeleteMessageAsync(msg.Id);
        //                            }
        //                            catch
        //                            {

        //                            }
        //                            try
        //                            {
        //                                await Context.Channel.DeleteMessageAsync(msg2.Id);
        //                            }
        //                            catch
        //                            {

        //                            }
        //                            await ReplyAsync(embed: new EmbedBuilder()
        //                                .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                                .WithDescription($"You have cancel your trade. Thank you for using the {TradingCardCore.Doremi.embedName}")
        //                                .WithColor(Config.Doremi.EmbedColor)
        //                                .Build());
        //                            isTrading = false;
        //                            Config.Doremi.isRunningInteractive[Context.User.Id.ToString()] = false;
        //                            return Ok();
        //                        }
        //                        else if (stepProcess == 1)
        //                        { //select user
        //                            var isNumeric = int.TryParse(response.Content.ToString().ToLower(), out int n);
        //                            if (newStep)
        //                            {
        //                                newStep = false;
        //                                try
        //                                {
        //                                    await Context.Channel.DeleteMessageAsync(response.Id);
        //                                    await Context.Channel.DeleteMessageAsync(msg.Id);
        //                                }
        //                                catch
        //                                {

        //                                }
        //                                try
        //                                {
        //                                    await Context.Channel.DeleteMessageAsync(msg2.Id);
        //                                }
        //                                catch
        //                                {

        //                                }
        //                                msg = await PagedReplyAsync(pagerUserList);
        //                                response = await NextMessageAsync(timeout: timeoutDuration);
        //                            }
        //                            else
        //                            {
        //                                if (!isNumeric)
        //                                {
        //                                    try
        //                                    {
        //                                        await Context.Channel.DeleteMessageAsync(response.Id);
        //                                        await Context.Channel.DeleteMessageAsync(msg.Id);
        //                                    }
        //                                    catch
        //                                    {

        //                                    }
        //                                    try
        //                                    {
        //                                        await Context.Channel.DeleteMessageAsync(msg2.Id);
        //                                    }
        //                                    catch
        //                                    {

        //                                    }
        //                                    stepProcess = 1;
        //                                    selectionUserId = "";
        //                                    await ReplyAndDeleteAsync(":x: Please re-type the proper number selection.", timeout: TimeSpan.FromSeconds(10));
        //                                    msg = await PagedReplyAsync(pagerUserList);
        //                                    response = await NextMessageAsync(timeout: timeoutDuration);
        //                                }
        //                                //array length:2[0,1], selected:2
        //                                else if (Convert.ToInt32(response.Content.ToLower().ToString()) <= 0 ||
        //                                Convert.ToInt32(response.Content.ToString().ToLower()) > arrUserId.Count)
        //                                {
        //                                    try
        //                                    {
        //                                        await Context.Channel.DeleteMessageAsync(response.Id);
        //                                        await Context.Channel.DeleteMessageAsync(msg.Id);
        //                                    }
        //                                    catch
        //                                    {

        //                                    }
        //                                    try
        //                                    {
        //                                        await Context.Channel.DeleteMessageAsync(msg2.Id);
        //                                    }
        //                                    catch
        //                                    {

        //                                    }

        //                                    stepProcess = 1;
        //                                    selectionUserId = "";
        //                                    await ReplyAndDeleteAsync(":x: That number choice is not on the list. Please re-type the proper number selection.", timeout: TimeSpan.FromSeconds(10));
        //                                    msg2 = await PagedReplyAsync(pagerUserList);
        //                                    response = await NextMessageAsync(timeout: timeoutDuration);
        //                                }
        //                                else
        //                                {
        //                                    selectionUserId = arrUserId[Convert.ToInt32(response.Content.ToString().ToLower()) - 1];
        //                                    var otherUserData = JObject.Parse(File.ReadAllText(
        //                                    $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{selectionUserId}.json"
        //                                    ));
        //                                    if (((JObject)(otherUserData["trading_queue"])).ContainsKey(clientId.ToString()))
        //                                    {
        //                                        try
        //                                        {
        //                                            await Context.Channel.DeleteMessageAsync(response.Id);
        //                                            await Context.Channel.DeleteMessageAsync(msg.Id);
        //                                        }
        //                                        catch
        //                                        {

        //                                        }
        //                                        try
        //                                        {
        //                                            await Context.Channel.DeleteMessageAsync(msg2.Id);
        //                                        }
        //                                        catch
        //                                        {

        //                                        }

        //                                        msg = await ReplyAsync(embed: new EmbedBuilder()
        //                                            .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                                            .WithDescription($":x: Sorry, you cannot trade more than once with " +
        //                                            $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}.")
        //                                            .WithThumbnailUrl(TradingCardCore.Doremi.emojiError)
        //                                            .Build());
        //                                        msg2 = await PagedReplyAsync(pagerUserList);
        //                                        response = await NextMessageAsync(timeout: timeoutDuration);
        //                                    }
        //                                    else
        //                                    {
        //                                        stepProcess = 2; newStep = true;
        //                                    }

        //                                }
        //                            }


        //                        }
        //                        else if (stepProcess == 2)
        //                        { //card pack & category selection from other user
        //                            var otherUserData = JObject.Parse(File.ReadAllText(
        //                                $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{selectionUserId}.json"
        //                                ));
        //                            List<string> listCardPackCategory = TradingCardCore.tradeListAllowed((JObject)otherUserData);

        //                            try
        //                            {
        //                                await Context.Channel.DeleteMessageAsync(response.Id);
        //                                await Context.Channel.DeleteMessageAsync(msg.Id);
        //                            }
        //                            catch
        //                            {

        //                            }
        //                            try
        //                            {
        //                                await Context.Channel.DeleteMessageAsync(msg2.Id);
        //                            }
        //                            catch
        //                            {

        //                            }

        //                            if (listCardPackCategory.Count <= 0)
        //                            {

        //                                msg = await ReplyAsync(embed: new EmbedBuilder()
        //                                    .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                                    .WithColor(Config.Doremi.EmbedColor)
        //                                    .WithDescription($":x: Sorry, there are no cards that can be selected from " +
        //                                    $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}.\n" +
        //                                    $"Please select other users.")
        //                                    .Build());
        //                                stepProcess = 1; newStep = true;
        //                                response = await NextMessageAsync(timeout: timeoutDuration);
        //                            }
        //                            else
        //                            {
        //                                string textConcatDoremi = ""; string textConcatHazuki = ""; string textConcatAiko = "";
        //                                string textConcatOnpu = ""; string textConcatMomoko = "";
        //                                for (int i = 0; i < listCardPackCategory.Count; i++)
        //                                {
        //                                    if (listCardPackCategory[i].Contains("doremi"))
        //                                        textConcatDoremi += $"{listCardPackCategory[i]}\n";
        //                                    else if (listCardPackCategory[i].Contains("hazuki"))
        //                                        textConcatHazuki += $"{listCardPackCategory[i]}\n";
        //                                    else if (listCardPackCategory[i].Contains("aiko"))
        //                                        textConcatAiko += $"{listCardPackCategory[i]}\n";
        //                                    else if (listCardPackCategory[i].Contains("onpu"))
        //                                        textConcatOnpu += $"{listCardPackCategory[i]}\n";
        //                                    else if (listCardPackCategory[i].Contains("momoko"))
        //                                        textConcatMomoko += $"{listCardPackCategory[i]}\n";
        //                                }

        //                                if (textConcatDoremi == "") textConcatDoremi = "No card trade for this pack.";
        //                                if (textConcatHazuki == "") textConcatHazuki = "No card trade for this pack.";
        //                                if (textConcatAiko == "") textConcatAiko = "No card trade for this pack.";
        //                                if (textConcatOnpu == "") textConcatOnpu = "No card trade for this pack.";
        //                                if (textConcatMomoko == "") textConcatMomoko = "No card trade for this pack.";

        //                                if (newStep)
        //                                {

        //                                    newStep = false;
        //                                    msg = await ReplyAsync(embed: new EmbedBuilder()
        //                                    .WithTitle("Step 2 - Card Pack & Category Selection")
        //                                    .WithDescription($"Type the **card pack & category** selection from " +
        //                                    $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}. Example: **doremi normal**.\n" +
        //                                    $"Type **back** to select other user.")
        //                                    .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                                    .WithColor(Config.Doremi.EmbedColor)
        //                                    .AddField("Doremi Card Pack", textConcatDoremi, true)
        //                                    .AddField("Hazuki Card Pack", textConcatHazuki, true)
        //                                    .AddField("Aiko Card Pack", textConcatAiko, true)
        //                                    .AddField("Onpu Card Pack", textConcatOnpu, true)
        //                                    .AddField("Momoko Card Pack", textConcatMomoko, true)
        //                                    .Build());
        //                                    response = await NextMessageAsync(timeout: timeoutDuration);
        //                                }
        //                                else
        //                                {
        //                                    if (response.Content.ToString().ToLower() == "back")
        //                                    {
        //                                        stepProcess = 1;
        //                                        newStep = true;
        //                                    }
        //                                    else if (!listCardPackCategory.Any(str => str.Contains(response.Content.ToString().ToLower())) ||
        //                                        !response.Content.ToString().ToLower().Contains(" "))
        //                                    {
        //                                        msg = await ReplyAsync(":x: Please re-enter the proper card pack selection.",
        //                                        embed: new EmbedBuilder()
        //                                        .WithTitle("Step 2 - Card Pack & Category Selection")
        //                                        .WithDescription($"Type the card pack & category selection from " +
        //                                        $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}. Example: **doremi normal**.\n" +
        //                                        $"Type **back** to select other user.")
        //                                        .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                                        .WithColor(Config.Doremi.EmbedColor)
        //                                        .AddField("Doremi Card Pack", textConcatDoremi, true)
        //                                        .AddField("Hazuki Card Pack", textConcatHazuki, true)
        //                                        .AddField("Aiko Card Pack", textConcatAiko, true)
        //                                        .AddField("Onpu Card Pack", textConcatOnpu, true)
        //                                        .AddField("Momoko Card Pack", textConcatMomoko, true)
        //                                        .Build());
        //                                        response = await NextMessageAsync(timeout: timeoutDuration);
        //                                    }
        //                                    else
        //                                    {
        //                                        stepProcess = 3;
        //                                        string[] splittedChoice = response.Content.ToString().ToLower().Split(" ");
        //                                        selectionOtherUserCardPack = splittedChoice[0]; selectionOtherUserCardCategory = splittedChoice[1];
        //                                        newStep = true;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        else if (stepProcess == 3)
        //                        {
        //                            //select other user card id that you want to trade.
        //                            //your card id data
        //                            var yourData = JObject.Parse(File.ReadAllText(
        //                                $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{Convert.ToUInt64(clientId)}.json"
        //                                ));
        //                            var jYourData = (JArray)(yourData[selectionOtherUserCardPack][selectionOtherUserCardCategory]);
        //                            //other user card id data
        //                            var otherUserData = JObject.Parse(File.ReadAllText(
        //                                $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{selectionUserId}.json"
        //                                ));
        //                            var jOtherUserData = (JArray)(otherUserData[selectionOtherUserCardPack][selectionOtherUserCardCategory]);

        //                            //available list
        //                            var arrList = jOtherUserData;
        //                            var arrYourList = jYourData; int founded = 0;
        //                            //remove the card that you already have
        //                            for (int i = 0; i < arrList.Count; i++)
        //                            {
        //                                founded = 0;
        //                                for (int j = 0; j < arrYourList.Count; j++)
        //                                {
        //                                    if (arrList[i].ToString().ToLower() == arrYourList[j].ToString().ToLower())
        //                                    {
        //                                        founded = 1;
        //                                        j = arrYourList.Count;
        //                                    }
        //                                }
        //                                if (founded == 0)
        //                                    arrUserOtherCardList.Add(arrList[i].ToString());
        //                            }

        //                            pageContent = TradingCardCore.printTradeCardListTemplate(selectionOtherUserCardPack, selectionOtherUserCardCategory,
        //                                jObjTradingCardList, arrUserOtherCardList);
        //                            var pagerCardList = new PaginatedMessage
        //                            {
        //                                Pages = pageContent,
        //                                Color = Config.Doremi.EmbedColor,
        //                                Options = pao
        //                            };

        //                            //end available list
        //                            try
        //                            {
        //                                await Context.Channel.DeleteMessageAsync(response.Id);
        //                                await Context.Channel.DeleteMessageAsync(msg.Id);
        //                            }
        //                            catch
        //                            {

        //                            }
        //                            try
        //                            {
        //                                await Context.Channel.DeleteMessageAsync(msg2.Id);
        //                            }
        //                            catch
        //                            {

        //                            }

        //                            if (newStep)
        //                            {
        //                                newStep = false;

        //                                if (arrUserOtherCardList.Count >= 1)
        //                                {
        //                                    msg = await ReplyAsync(embed: new EmbedBuilder()
        //                                    .WithTitle("Step 3 - Card Id Selection")
        //                                    .WithDescription($"Type the **card id** choice from " +
        //                                    $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}. Example: **do001**.\n" +
        //                                    $"Type **back** to select other card pack.")
        //                                    .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                                    .WithColor(Config.Doremi.EmbedColor)
        //                                    .Build());
        //                                    msg2 = await PagedReplyAsync(pagerCardList);
        //                                    response = await NextMessageAsync(timeout: timeoutDuration);
        //                                }
        //                                else
        //                                {
        //                                    msg = await ReplyAsync(embed: new EmbedBuilder()
        //                                        .WithTitle($"Step 3 - {GlobalFunctions.UppercaseFirst(selectionOtherUserCardPack)} {GlobalFunctions.UppercaseFirst(selectionOtherUserCardCategory)} Card Id Selection")
        //                                        .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                                        .WithDescription($":x: Sorry, there are no **{GlobalFunctions.UppercaseFirst(selectionOtherUserCardPack)} {GlobalFunctions.UppercaseFirst(selectionOtherUserCardCategory)}** card that you can choose from " +
        //                                        $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}.\n" +
        //                                        "Type **back** to select other card pack.")
        //                                        .WithColor(Config.Doremi.EmbedColor)
        //                                        .Build());
        //                                    response = await NextMessageAsync(timeout: timeoutDuration);
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (response.Content.ToString().ToLower() == "back")
        //                                {
        //                                    stepProcess = 2;
        //                                    newStep = true;
        //                                }
        //                                else if (arrUserOtherCardList.Count <= 0)
        //                                {
        //                                    msg = await ReplyAsync(embed: new EmbedBuilder()
        //                                        .WithTitle("Step 3 - Card Id Selection")
        //                                        .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                                        .WithDescription($":x: Sorry, there are no **{GlobalFunctions.UppercaseFirst(selectionOtherUserCardPack)} {GlobalFunctions.UppercaseFirst(selectionOtherUserCardCategory)}** card that you can choose from " +
        //                                        $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}.\n" +
        //                                        "Type **back** to select other card pack.")
        //                                        .WithColor(Config.Doremi.EmbedColor)
        //                                        .Build());
        //                                    response = await NextMessageAsync(timeout: timeoutDuration);
        //                                }
        //                                else if (!arrUserOtherCardList.Contains(response.Content.ToString(), StringComparer.Ordinal))
        //                                {
        //                                    await ReplyAndDeleteAsync(":x: Please re-enter the correct **card id.**", timeout: TimeSpan.FromSeconds(10));
        //                                    msg2 = await PagedReplyAsync(pagerCardList);
        //                                    response = await NextMessageAsync(timeout: timeoutDuration);
        //                                }
        //                                else
        //                                {
        //                                    stepProcess = 4; newStep = true;
        //                                    selectionOtherUserCardChoiceId = response.Content.ToString();
        //                                }
        //                            }
        //                        }
        //                        else if (stepProcess == 4)
        //                        {
        //                            //card pack & category selection from yours
        //                            var yourUserData = JObject.Parse(File.ReadAllText(
        //                                $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId.ToString()}.json"
        //                                ));
        //                            List<string> listCardPackCategory = TradingCardCore.tradeListAllowed((JObject)yourUserData);

        //                            try
        //                            {
        //                                await Context.Channel.DeleteMessageAsync(response.Id);
        //                                await Context.Channel.DeleteMessageAsync(msg.Id);
        //                            }
        //                            catch
        //                            {

        //                            }
        //                            try
        //                            {
        //                                await Context.Channel.DeleteMessageAsync(msg2.Id);
        //                            }
        //                            catch
        //                            {

        //                            }

        //                            if (listCardPackCategory.Count <= 0)
        //                            {
        //                                msg = await ReplyAsync(embed: new EmbedBuilder()
        //                                    .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                                    .WithColor(Config.Doremi.EmbedColor)
        //                                    .WithTitle($"Step 4 - Select Your Available Card Pack")
        //                                    .WithDescription($"Sorry, there are no cards that you can trade. " +
        //                                    $"Your card trading process has been canceled.")
        //                                    .Build());
        //                                isTrading = false;
        //                                Config.Doremi.isRunningInteractive[Context.User.Id.ToString()] = false;
        //                                return Ok();
        //                            }
        //                            else
        //                            {
        //                                string textConcatDoremi = ""; string textConcatHazuki = ""; string textConcatAiko = "";
        //                                string textConcatOnpu = ""; string textConcatMomoko = "";
        //                                for (int i = 0; i < listCardPackCategory.Count; i++)
        //                                {
        //                                    if (listCardPackCategory[i].Contains("doremi"))
        //                                        textConcatDoremi += $"{listCardPackCategory[i]}\n";
        //                                    else if (listCardPackCategory[i].Contains("hazuki"))
        //                                        textConcatHazuki += $"{listCardPackCategory[i]}\n";
        //                                    else if (listCardPackCategory[i].Contains("aiko"))
        //                                        textConcatAiko += $"{listCardPackCategory[i]}\n";
        //                                    else if (listCardPackCategory[i].Contains("onpu"))
        //                                        textConcatOnpu += $"{listCardPackCategory[i]}\n";
        //                                    else if (listCardPackCategory[i].Contains("momoko"))
        //                                        textConcatMomoko += $"{listCardPackCategory[i]}\n";
        //                                }

        //                                if (textConcatDoremi == "") textConcatDoremi = "No card trade for this pack.";
        //                                if (textConcatHazuki == "") textConcatHazuki = "No card trade for this pack.";
        //                                if (textConcatAiko == "") textConcatAiko = "No card trade for this pack.";
        //                                if (textConcatOnpu == "") textConcatOnpu = "No card trade for this pack.";
        //                                if (textConcatMomoko == "") textConcatMomoko = "No card trade for this pack.";

        //                                if (newStep)
        //                                {
        //                                    newStep = false;
        //                                    msg = await ReplyAsync(embed: new EmbedBuilder()
        //                                    .WithTitle("Step 4 - Select Your Available Card Pack")
        //                                    .WithDescription($"Type the **card pack & category** selection from yours. Example: **doremi normal**.\n" +
        //                                    $"Type **back** to re-select other card from {MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}.")
        //                                    .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                                    .WithColor(Config.Doremi.EmbedColor)
        //                                    .AddField("Doremi Card Pack", textConcatDoremi, true)
        //                                    .AddField("Hazuki Card Pack", textConcatHazuki, true)
        //                                    .AddField("Aiko Card Pack", textConcatAiko, true)
        //                                    .AddField("Onpu Card Pack", textConcatOnpu, true)
        //                                    .AddField("Momoko Card Pack", textConcatMomoko, true)
        //                                    .Build());
        //                                    response = await NextMessageAsync(timeout: timeoutDuration);
        //                                }
        //                                else
        //                                {
        //                                    if (response.Content.ToString().ToLower() == "back")
        //                                    {
        //                                        stepProcess = 3;
        //                                        newStep = true;
        //                                    }
        //                                    else if (!listCardPackCategory.Contains(response.Content.ToString().ToLower(), StringComparer.OrdinalIgnoreCase) ||
        //                                      !response.Content.ToString().ToLower().Contains(" "))
        //                                    {
        //                                        msg = await ReplyAsync(":x: Please re-enter the proper card pack selection.",
        //                                        embed: new EmbedBuilder()
        //                                        .WithTitle("Step 4 - Select Your Available Card Pack")
        //                                        .WithDescription($"Type the **card pack & category** selection from yours. Example: **doremi normal**.\n" +
        //                                        $"Type **back** to re-select other card from {MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}.")
        //                                        .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                                        .WithColor(Config.Doremi.EmbedColor)
        //                                        .AddField("Doremi Card Pack", textConcatDoremi, true)
        //                                        .AddField("Hazuki Card Pack", textConcatHazuki, true)
        //                                        .AddField("Aiko Card Pack", textConcatAiko, true)
        //                                        .AddField("Onpu Card Pack", textConcatOnpu, true)
        //                                        .AddField("Momoko Card Pack", textConcatMomoko, true)
        //                                        .Build());
        //                                        response = await NextMessageAsync(timeout: timeoutDuration);
        //                                    }
        //                                    else
        //                                    {
        //                                        stepProcess = 5;
        //                                        string[] splittedChoice = response.Content.ToString().ToLower().Split(" ");
        //                                        selectionYourCardPack = splittedChoice[0]; selectionYourCardCategory = splittedChoice[1];
        //                                        newStep = true;
        //                                    }
        //                                }
        //                            }

        //                        }
        //                        else if (stepProcess == 5)
        //                        {
        //                            //select other user card id that you want to trade.
        //                            //your card id data
        //                            var yourData = JObject.Parse(File.ReadAllText(
        //                                $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{Convert.ToUInt64(clientId)}.json"
        //                                ));
        //                            var jYourData = (JArray)(yourData[selectionYourCardPack][selectionYourCardCategory]);
        //                            //other user card id data
        //                            var otherUserData = JObject.Parse(File.ReadAllText(
        //                                $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{selectionUserId}.json"
        //                                ));
        //                            var jOtherUserData = (JArray)(otherUserData[selectionYourCardPack][selectionYourCardCategory]);

        //                            //available list
        //                            var arrList = jOtherUserData;
        //                            var arrYourList = jYourData; int founded = 0;
        //                            //remove the card that you already have
        //                            for (int i = 0; i < arrYourList.Count; i++)
        //                            {
        //                                founded = 0;
        //                                for (int j = 0; j < arrList.Count; j++)
        //                                {
        //                                    if (arrYourList[i].ToString().ToLower() == arrList[j].ToString().ToLower())
        //                                    {
        //                                        founded = 1;
        //                                        j = arrList.Count;
        //                                    }
        //                                }
        //                                if (founded == 0)
        //                                    arrUserCardList.Add(arrYourList[i].ToString());
        //                            }

        //                            pageContent = TradingCardCore.printTradeCardListTemplate(selectionYourCardPack, selectionYourCardCategory,
        //                                jObjTradingCardList, arrUserCardList);
        //                            var pagerYourCardList = new PaginatedMessage
        //                            {
        //                                Pages = pageContent,
        //                                Color = Config.Doremi.EmbedColor,
        //                                Options = pao
        //                            };

        //                            //end available list

        //                            try
        //                            {
        //                                await Context.Channel.DeleteMessageAsync(response.Id);
        //                                await Context.Channel.DeleteMessageAsync(msg.Id);
        //                            }
        //                            catch
        //                            {

        //                            }
        //                            try
        //                            {
        //                                await Context.Channel.DeleteMessageAsync(msg2.Id);
        //                            }
        //                            catch
        //                            {

        //                            }

        //                            if (newStep)
        //                            {
        //                                newStep = false;
        //                                if (arrUserCardList.Count <= 0)
        //                                {
        //                                    msg = await ReplyAsync(embed: new EmbedBuilder()
        //                                    .WithTitle("Step 5 - Card Id Selection")
        //                                    .WithDescription($":x: Sorry, there are no **{GlobalFunctions.UppercaseFirst(selectionYourCardPack)} {GlobalFunctions.UppercaseFirst(selectionYourCardCategory)}** card that you can choose.\n" +
        //                                        "Type **back** to select other card pack.")
        //                                    .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                                    .WithColor(Config.Doremi.EmbedColor)
        //                                    .Build());
        //                                    response = await NextMessageAsync(timeout: timeoutDuration);
        //                                }
        //                                else
        //                                {
        //                                    msg = await ReplyAsync(embed: new EmbedBuilder()
        //                                    .WithTitle($"Step 5 - {GlobalFunctions.UppercaseFirst(selectionYourCardPack)} {GlobalFunctions.UppercaseFirst(selectionYourCardCategory)} Card Id Selection")
        //                                    .WithDescription($"Type the **card id** selection from yours. Example: **do001**.\n" +
        //                                    $"Type **back** to select other card pack.")
        //                                    .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                                    .WithColor(Config.Doremi.EmbedColor)
        //                                    .Build());
        //                                    msg2 = await PagedReplyAsync(pagerYourCardList);
        //                                    response = await NextMessageAsync(timeout: timeoutDuration);
        //                                }

        //                            }
        //                            else
        //                            {
        //                                if (response.Content.ToString().ToLower() == "back")
        //                                {
        //                                    stepProcess = 4;
        //                                    newStep = true;
        //                                }
        //                                else if (arrUserCardList.Count <= 0)
        //                                {
        //                                    msg = await ReplyAsync(embed: new EmbedBuilder()
        //                                        .WithTitle("Step 5 - Card Id Selection")
        //                                        .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                                        .WithDescription($":x: Sorry, there are no **{GlobalFunctions.UppercaseFirst(selectionYourCardPack)} {GlobalFunctions.UppercaseFirst(selectionYourCardCategory)}** card that you can choose.\n" +
        //                                        "Type **back** to select other card pack.")
        //                                        .Build());
        //                                    response = await NextMessageAsync(timeout: timeoutDuration);
        //                                }
        //                                else if (!arrUserCardList.Contains(response.Content.ToString(), StringComparer.Ordinal))
        //                                {
        //                                    await ReplyAndDeleteAsync(":x: Please re-enter the correct **card id.**", timeout: TimeSpan.FromSeconds(10));
        //                                    msg2 = await PagedReplyAsync(pagerYourCardList);
        //                                    response = await NextMessageAsync(timeout: timeoutDuration);
        //                                }
        //                                else
        //                                {
        //                                    stepProcess = 6; newStep = true;
        //                                    selectionYourCardChoiceId = response.Content.ToString();
        //                                }
        //                            }
        //                        }
        //                        else if (stepProcess == 6)
        //                        {
        //                            try
        //                            {
        //                                await Context.Channel.DeleteMessageAsync(response.Id);
        //                                await Context.Channel.DeleteMessageAsync(msg.Id);
        //                            }
        //                            catch
        //                            {

        //                            }
        //                            try
        //                            {
        //                                await Context.Channel.DeleteMessageAsync(msg2.Id);
        //                            }
        //                            catch
        //                            {

        //                            }

        //                            EmbedBuilder eb = new EmbedBuilder()
        //                            .WithTitle("Step 6 - Review Your Trade")
        //                            .WithDescription($"You will trade with {MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}. You can see your trade information review below.\n" +
        //                            $"Type **confirm** or **accept** to confirm the trade.\n" +
        //                            $"Type **back** to select other card pack.\n" +
        //                            $"Type **cancel** to cancel your trading process.")
        //                            .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                            //other user:
        //                            .AddField($"You will receive:",
        //                            $"-Card Pack: " +
        //                            $"{GlobalFunctions.UppercaseFirst(selectionOtherUserCardPack)} {GlobalFunctions.UppercaseFirst(selectionOtherUserCardCategory)}\n" +
        //                            $"-Card Name: " +
        //                            $"**[{selectionOtherUserCardChoiceId} - " +
        //                            $"{TradingCardCore.getCardProperty(selectionOtherUserCardPack, selectionOtherUserCardCategory, selectionOtherUserCardChoiceId, "name")}](" +
        //                            $"{TradingCardCore.getCardProperty(selectionOtherUserCardPack, selectionOtherUserCardCategory, selectionOtherUserCardChoiceId, "url")})**")
        //                            //yours
        //                            .AddField($"You will send:",
        //                            $"-Card Pack: " +
        //                            $"{GlobalFunctions.UppercaseFirst(selectionYourCardPack)} {GlobalFunctions.UppercaseFirst(selectionYourCardCategory)}\n" +
        //                            $"-Card Name: " +
        //                            $"**[{selectionYourCardChoiceId} - " +
        //                            $"{TradingCardCore.getCardProperty(selectionYourCardPack, selectionYourCardCategory, selectionYourCardChoiceId, "name")}](" +
        //                            $"{TradingCardCore.getCardProperty(selectionYourCardPack, selectionYourCardCategory, selectionYourCardChoiceId, "url")})**")
        //                            .WithColor(Config.Doremi.EmbedColor);

        //                            //review the trade
        //                            if (newStep)
        //                            {
        //                                newStep = false;
        //                                msg = await ReplyAsync(embed: eb.Build());
        //                                response = await NextMessageAsync(timeout: timeoutDuration);
        //                            }
        //                            else
        //                            {
        //                                if (response.Content.ToString().ToLower() == "back")
        //                                {
        //                                    stepProcess = 5;
        //                                    newStep = true;
        //                                }
        //                                else if (response.Content.ToString().ToLower() != "accept" &&
        //                                  response.Content.ToString().ToLower() != "confirm")
        //                                {
        //                                    msg = await ReplyAsync(":x: Please type with the valid choice: **accept/confirm/back/exit**",
        //                                        embed: eb.Build());
        //                                    response = await NextMessageAsync(timeout: timeoutDuration);
        //                                }
        //                                else
        //                                {
        //                                    await ReplyAsync(embed: new EmbedBuilder()
        //                                        .WithTitle("✅ Trade Offer has been sent succesfully!")
        //                                        .WithColor(Config.Doremi.EmbedColor)
        //                                        .WithDescription($"Your trade offer has been sent to " +
        //                                        $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}!\nThank you for using {TradingCardCore.Doremi.embedName}.")
        //                                        .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                                        .WithThumbnailUrl(TradingCardCore.Doremi.emojiOk)
        //                                        .Build());

        //                                    await ReplyAsync($"You have a new card trade offer, {MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}.\n" +
        //                                        $"Please use **{Config.Doremi.PrefixParent[0]}card trade process** to process your trade offer.");

        //                                    //save to user
        //                                    if (selectionOtherUserCardCategory.Contains("ojamajos"))
        //                                        selectionOtherUserCardChoiceId = $"{selectionOtherUserCardPack} {selectionOtherUserCardChoiceId}";
        //                                    if (selectionYourCardCategory.Contains("ojamajos"))
        //                                        selectionYourCardChoiceId = $"{selectionYourCardPack} {selectionYourCardChoiceId}";

        //                                    string[] parameterNames = new string[] { selectionOtherUserCardChoiceId, selectionYourCardChoiceId };
        //                                    JArray jarrayObj = new JArray();
        //                                    foreach (string parameterName in parameterNames)
        //                                    {
        //                                        jarrayObj.Add(parameterName);
        //                                    }

        //                                    string otherUserDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{selectionUserId}.json";
        //                                    var JUserData = JObject.Parse(File.ReadAllText(otherUserDataDirectory));
        //                                    ((JObject)JUserData["trading_queue"]).Add(clientId.ToString(), new JArray(jarrayObj));


        //                                    //JArray item = (JArray)arrInventory[parent][spawnedCardCategory];
        //                                    //item.Add(spawnedCardId);
        //                                    File.WriteAllText(otherUserDataDirectory, JUserData.ToString());
        //                                    isTrading = false;
        //                                    Config.Doremi.isRunningInteractive[Context.User.Id.ToString()] = false;
        //                                    return Ok();
        //                                }
        //                            }
        //                        }

        //                        //What card pack do you want to trade? select with number/the name

        //                        //Please type the card id that you want to trade

        //                        //Please type the 
        //                        //
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    Console.WriteLine(e.ToString());
        //                }

        //            }

        //        }

        //    }
        //    else
        //    {
        //        await ReplyAndDeleteAsync(null, embed: new EmbedBuilder()
        //        .WithColor(Config.Doremi.EmbedColor)
        //        .WithDescription($":x: I'm sorry, you are still running the trade command. Please finish it first.")
        //        .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build(), timeout: TimeSpan.FromSeconds(10));
        //    }

        //    return Ok();

        //    /*json format:
        //     * "trading_queue": {
        //        "01929183481": ["do","on"]
        //    }
        //     */
        //}

        //[Command("trade process", RunMode = RunMode.Async), Summary("Process the trade offer of ojamajos trading card.")]
        //public async Task<RuntimeResult> trading_card_queue_process()
        //{
        //    await ReplyAndDeleteAsync(null, embed: new EmbedBuilder()
        //            .WithColor(Config.Doremi.EmbedColor)
        //            .WithDescription($":x: Sorry, trade command is not available for now.")
        //            .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build(), timeout: TimeSpan.FromSeconds(10));

        //    return Ok();

        //    var guildId = Context.Guild.Id;
        //    var clientId = Context.User.Id;
        //    string userFolderDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}";
        //    string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";

        //    if (!Config.Doremi.isRunningInteractive.ContainsKey(Context.User.Id.ToString()))
        //        Config.Doremi.isRunningInteractive.Add(Context.User.Id.ToString(), false);

        //    if (!Config.Doremi.isRunningInteractive[Context.User.Id.ToString()])
        //    {
        //        if (!File.Exists(playerDataDirectory)) //not registered yet
        //        {
        //            await ReplyAndDeleteAsync(null, embed: new EmbedBuilder()
        //            .WithColor(Config.Doremi.EmbedColor)
        //            .WithDescription($"I'm sorry, please register yourself first with **{Config.Doremi.PrefixParent[0]}card register** command.")
        //            .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build(), timeout: TimeSpan.FromSeconds(10));
        //            return Ok();
        //        }

        //        var jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));

        //        var playerData = JObject.Parse(File.ReadAllText(playerDataDirectory));
        //        int fileCount = Directory.GetFiles(userFolderDirectory).Length;

        //        if (fileCount <= 1)
        //        {
        //            await ReplyAndDeleteAsync(":x: Sorry, server need to have more than 1 user that register the trading card data.",
        //                timeout: TimeSpan.FromSeconds(10));
        //            return Ok();
        //        }
        //        else
        //        {
        //            try
        //            {
        //                //start read all user id
        //                List<string> arrUserId = new List<string>();
        //                List<string> pageContent = new List<string>();
        //                List<string> pageContentUserList = new List<string>();

        //                string titleUserList = $"**Step 1 - Trade Process List Selection. Select with numbers.**\n";
        //                string tempValUserList = titleUserList;

        //                //list all users
        //                var userList = (JObject)playerData["trading_queue"];

        //                if (userList.Count <= 0)
        //                {
        //                    await ReplyAsync(":x: There are no card trade that you can process.");
        //                    return Ok();
        //                }

        //                IList<JToken> objUserList = userList;
        //                int currentIndex = 0;
        //                for (int i = 0; i < userList.Count; i++)
        //                {
        //                    var key = (JProperty)objUserList[i];

        //                    tempValUserList += $"**{i + 1}.** {MentionUtils.MentionUser(Convert.ToUInt64(key.Name))}\n";
        //                    arrUserId.Add(key.Name);

        //                    if (currentIndex < 14) currentIndex++;
        //                    else
        //                    {
        //                        pageContentUserList.Add(tempValUserList);
        //                        currentIndex = 0;
        //                        tempValUserList = titleUserList;
        //                    }

        //                    if (i == userList.Count - 1) pageContentUserList.Add(tempValUserList);

        //                }

        //                //selection variables
        //                //other users
        //                string selectionUserId = ""; string selectionOtherUserCardChoiceId = ""; string selectionYourCardChoiceId = "";
        //                string selectionOtherUserCardPack = ""; string selectionOtherUserCardCategory = "";
        //                string selectionYourCardPack = ""; string selectionYourCardCategory = "";

        //                //DirectoryInfo d = new DirectoryInfo(userFolderDirectory);//Assuming Test is your Folder
        //                //FileInfo[] Files = d.GetFiles("*.json"); //Getting Text files

        //                IUserMessage msg; IUserMessage msg2;
        //                //user selection
        //                string titleUserSelection = $"**Step 1 - Select the trade process from a user with numbers**\n";
        //                string tempVal = titleUserSelection;

        //                Boolean isTrading = true;
        //                var timeoutDuration = TimeSpan.FromSeconds(60);
        //                string replyTimeout = ":stopwatch: I'm sorry, but you have reach your timeout. " +
        //                    "Please use the `card trade` command again to retry the trade process.";
        //                int stepProcess = 1;//0/1:select the user,
        //                                    //2:review process
        //                Boolean newStep = true;
        //                //select user
        //                msg = await ReplyAsync(embed: new EmbedBuilder()
        //                    .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                    .WithDescription($"Welcome to {TradingCardCore.Doremi.embedName}. " +
        //                    $"Here you can process your trade offer that sent by someone. " +
        //                    $"You can type **cancel**/**exit** anytime to cancel the trade process.")
        //                    .WithColor(Config.Doremi.EmbedColor)
        //                    .Build());

        //                PaginatedAppearanceOptions pao = new PaginatedAppearanceOptions();
        //                pao.JumpDisplayOptions = JumpDisplayOptions.Never;
        //                pao.DisplayInformationIcon = false;

        //                var pager = new PaginatedMessage
        //                {
        //                    Pages = pageContentUserList,
        //                    Color = Config.Doremi.EmbedColor,
        //                    Options = pao
        //                };

        //                msg2 = await PagedReplyAsync(pageContentUserList);
        //                Config.Doremi.isRunningInteractive[Context.User.Id.ToString()] = true;
        //                var response = await NextMessageAsync(timeout: timeoutDuration);
        //                newStep = false;

        //                while (isTrading)
        //                {
        //                    try
        //                    {
        //                        var checkNull = response.Content.ToLower().ToString();
        //                    }
        //                    catch
        //                    {
        //                        await ReplyAndDeleteAsync(replyTimeout, timeout: TimeSpan.FromSeconds(10));
        //                        isTrading = false;
        //                        Config.Doremi.isRunningInteractive[Context.User.Id.ToString()] = false;
        //                        return Ok();
        //                    }

        //                    if (response.Content.ToString().ToLower() == "cancel" ||
        //                        response.Content.ToString().ToLower() == "exit")
        //                    {
        //                        try
        //                        {
        //                            await Context.Channel.DeleteMessageAsync(response.Id);
        //                            await Context.Channel.DeleteMessageAsync(msg);
        //                        }
        //                        catch { }
        //                        try
        //                        {
        //                            await Context.Channel.DeleteMessageAsync(msg2);
        //                        }
        //                        catch { }

        //                        await ReplyAsync(embed: new EmbedBuilder()
        //                            .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                            .WithDescription($"You have cancel your trade process. Thank you for using the {TradingCardCore.Doremi.embedName}")
        //                            .WithColor(Config.Doremi.EmbedColor)
        //                            .Build());
        //                        isTrading = false;
        //                        Config.Doremi.isRunningInteractive[Context.User.Id.ToString()] = false;
        //                        return Ok();
        //                    }
        //                    else if (stepProcess == 1)
        //                    {
        //                        try
        //                        {
        //                            await Context.Channel.DeleteMessageAsync(response.Id);
        //                            await Context.Channel.DeleteMessageAsync(msg);
        //                        }
        //                        catch { }
        //                        try
        //                        {
        //                            await Context.Channel.DeleteMessageAsync(msg2);
        //                        }
        //                        catch { }

        //                        //select user
        //                        var isNumeric = int.TryParse(response.Content.ToString().ToLower(), out int n);
        //                        if (newStep)
        //                        {
        //                            newStep = false;
        //                            msg2 = await PagedReplyAsync(pageContentUserList);
        //                            response = await NextMessageAsync(timeout: timeoutDuration);
        //                        }
        //                        else
        //                        {
        //                            if (!isNumeric)
        //                            {
        //                                stepProcess = 1;
        //                                selectionUserId = "";
        //                                await ReplyAndDeleteAsync(":x: Please re-type the proper number selection.",
        //                                    timeout: TimeSpan.FromSeconds(10));
        //                                //await PagedReplyAsync(pageContentUserList);
        //                                response = await NextMessageAsync(timeout: timeoutDuration);
        //                            }
        //                            //array length:2[0,1], selected:2
        //                            else if (Convert.ToInt32(response.Content.ToLower().ToString()) <= 0 ||
        //                                Convert.ToInt32(response.Content.ToString().ToLower()) > arrUserId.Count)
        //                            {
        //                                stepProcess = 1;
        //                                selectionUserId = "";
        //                                await ReplyAndDeleteAsync(":x: That number choice is not on the list. Please re-type the proper number selection.",
        //                                    timeout: TimeSpan.FromSeconds(10));
        //                                msg2 = await PagedReplyAsync(pageContentUserList);
        //                                response = await NextMessageAsync(timeout: timeoutDuration);
        //                            }
        //                            else
        //                            {
        //                                stepProcess = 2; newStep = true;
        //                                selectionUserId = arrUserId[Convert.ToInt32(response.Content.ToString()) - 1];
        //                                var selectedUserData = (JArray)userList[selectionUserId];
        //                                //will be send--
        //                                selectionYourCardChoiceId = selectedUserData[0].ToString();

        //                                if (selectionYourCardChoiceId.Contains(" "))
        //                                {//ojamajos category
        //                                    //example: hazuki ojt...
        //                                    string[] splittedYourChoice = selectionYourCardChoiceId.Split(" ");
        //                                    selectionYourCardPack = splittedYourChoice[0];
        //                                    selectionYourCardChoiceId = splittedYourChoice[1];
        //                                }
        //                                else
        //                                    selectionYourCardPack = TradingCardCore.getCardParent(selectionYourCardChoiceId);

        //                                selectionYourCardCategory = TradingCardCore.getCardCategory(selectionYourCardChoiceId);

        //                                //will be received
        //                                selectionOtherUserCardChoiceId = selectedUserData[1].ToString();

        //                                if (selectionOtherUserCardChoiceId.Contains(" "))
        //                                {//ojamajos category
        //                                    //example: hazuki ojt...
        //                                    string[] splittedOtherChoice = selectionOtherUserCardChoiceId.Split(" ");
        //                                    selectionOtherUserCardPack = splittedOtherChoice[0];
        //                                    selectionOtherUserCardChoiceId = splittedOtherChoice[1];
        //                                }
        //                                else
        //                                    selectionOtherUserCardPack = TradingCardCore.getCardParent(selectionOtherUserCardChoiceId);

        //                                selectionOtherUserCardCategory = TradingCardCore.getCardCategory(selectionOtherUserCardChoiceId);

        //                            }
        //                        }
        //                    }
        //                    else if (stepProcess == 2)
        //                    {
        //                        try
        //                        {
        //                            await Context.Channel.DeleteMessageAsync(response.Id);
        //                            await Context.Channel.DeleteMessageAsync(msg);
        //                        }
        //                        catch { }
        //                        try
        //                        {
        //                            await Context.Channel.DeleteMessageAsync(msg2);
        //                        }
        //                        catch { }

        //                        //check if your card/other user card still exists on the inventory or not
        //                        var JCheckUserData = JObject.Parse(File.ReadAllText(playerDataDirectory));
        //                        string otherUserDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/" +
        //                            $"{selectionUserId}.json";
        //                        var JCheckOtherUserData = JObject.Parse(File.ReadAllText(otherUserDataDirectory));

        //                        JArray notRequiredArrayYours = JArray.Parse(JCheckUserData[selectionYourCardPack][selectionYourCardCategory].ToString());
        //                        bool notExistsYours = notRequiredArrayYours.Any(t => t.Value<string>() == selectionYourCardChoiceId);//check yours have the cards in inventory
        //                        JArray requiredArrayYours = JArray.Parse(JCheckUserData[selectionOtherUserCardPack][selectionOtherUserCardCategory].ToString());
        //                        bool existsYours = requiredArrayYours.Any(t => t.Value<string>() == selectionOtherUserCardChoiceId);//check yours for duplicates

        //                        JArray notRequiredArrayOtherUser = JArray.Parse(JCheckOtherUserData[selectionOtherUserCardPack][selectionOtherUserCardCategory].ToString());
        //                        bool notExistsOtherUser = notRequiredArrayOtherUser.Any(t => t.Value<string>() == selectionOtherUserCardChoiceId);//check others have the cards in inventory
        //                        JArray requiredArrayOthers = JArray.Parse(JCheckOtherUserData[selectionYourCardPack][selectionYourCardCategory].ToString());
        //                        bool existsOthers = requiredArrayOthers.Any(t => t.Value<string>() == selectionYourCardChoiceId);//check others for duplicates

        //                        if (!notExistsYours || !notExistsOtherUser)
        //                        {//check if card still exists/not
        //                            await ReplyAsync(embed: new EmbedBuilder()
        //                            .WithTitle("🗑️ Trade Process Cancelled")
        //                            .WithColor(Config.Doremi.EmbedColor)
        //                            .WithDescription($"Your trade with " +
        //                            $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))} has been cancelled because one of you don't have the offered card anymore.\n" +
        //                            $"Please use the **{Config.Doremi.PrefixParent[0]}card trade process** again to process other card offer.")
        //                            .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                            .Build());
        //                            isTrading = false;
        //                            //save the file
        //                            string yourUserDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
        //                            var JUserData = JObject.Parse(File.ReadAllText(yourUserDataDirectory));
        //                            ((JObject)JUserData["trading_queue"]).Remove(selectionUserId);
        //                            File.WriteAllText(yourUserDataDirectory, JUserData.ToString());
        //                            isTrading = false;
        //                            Config.Doremi.isRunningInteractive[Context.User.Id.ToString()] = false;
        //                            return Ok();
        //                        }
        //                        else if (existsYours || existsOthers)
        //                        {//check for duplicates
        //                            await ReplyAsync(embed: new EmbedBuilder()
        //                            .WithTitle("🗑️ Trade Process Cancelled")
        //                            .WithColor(Config.Doremi.EmbedColor)
        //                            .WithDescription($"Your trade with " +
        //                            $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))} has been cancelled because one of you already have the same card offer that being sent.\n" +
        //                            $"Please use the **{Config.Doremi.PrefixParent[0]}card trade process** again to process another card offer.")
        //                            .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                            .Build());
        //                            //save the file
        //                            string yourUserDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
        //                            var JUserData = JObject.Parse(File.ReadAllText(yourUserDataDirectory));
        //                            ((JObject)JUserData["trading_queue"]).Remove(selectionUserId);
        //                            File.WriteAllText(yourUserDataDirectory, JUserData.ToString());
        //                            isTrading = false;
        //                            Config.Doremi.isRunningInteractive[Context.User.Id.ToString()] = false;
        //                            return Ok();
        //                        }

        //                        EmbedBuilder eb = new EmbedBuilder()
        //                        .WithTitle("Step 2 - Review Your Trade")
        //                        .WithDescription($"You can see your trade information review below from {MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}.\n" +
        //                        $"Type **confirm** or **accept** to confirm the trade.\n" +
        //                        $"Type **reject** to reject the trade.\n" +
        //                        $"Type **back** to select other card pack.\n" +
        //                        $"Type **exit** or **cancel** to exit the card trade process.")
        //                        .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                        //other user:
        //                        .AddField($"You will receive:",
        //                        $"-Card Pack: " +
        //                        $"{GlobalFunctions.UppercaseFirst(selectionOtherUserCardPack)} {GlobalFunctions.UppercaseFirst(selectionOtherUserCardCategory)}\n" +
        //                        $"-Card Name: " +
        //                        $"**[{selectionOtherUserCardChoiceId} - " +
        //                        $"{TradingCardCore.getCardProperty(selectionOtherUserCardPack, selectionOtherUserCardCategory, selectionOtherUserCardChoiceId, "name")}](" +
        //                        $"{TradingCardCore.getCardProperty(selectionOtherUserCardPack, selectionOtherUserCardCategory, selectionOtherUserCardChoiceId, "url")})**")
        //                        //yours
        //                        .AddField($"You will send:",
        //                        $"-Card Pack: " +
        //                        $"{GlobalFunctions.UppercaseFirst(selectionYourCardPack)} {GlobalFunctions.UppercaseFirst(selectionYourCardCategory)}\n" +
        //                        $"-Card Name: " +
        //                        $"**[{selectionYourCardChoiceId} - " +
        //                        $"{TradingCardCore.getCardProperty(selectionYourCardPack, selectionYourCardCategory, selectionYourCardChoiceId, "name")}](" +
        //                        $"{TradingCardCore.getCardProperty(selectionYourCardPack, selectionYourCardCategory, selectionYourCardChoiceId, "url")})**")
        //                        .WithColor(Config.Doremi.EmbedColor);

        //                        //review the trade
        //                        if (newStep)
        //                        {
        //                            newStep = false;
        //                            msg = await ReplyAsync(embed: eb.Build());
        //                            response = await NextMessageAsync(timeout: timeoutDuration);
        //                        }
        //                        else
        //                        {
        //                            if (response.Content.ToString().ToLower() == "back")
        //                            {
        //                                stepProcess = 1;
        //                                newStep = true;
        //                            }
        //                            else if (response.Content.ToString().ToLower() != "accept" &&
        //                              response.Content.ToString().ToLower() != "confirm" &&
        //                              response.Content.ToString().ToLower() != "reject")
        //                            {
        //                                msg = await ReplyAsync(":x: Please type with the valid choice: **accept/confirm/reject**.",
        //                                    embed: eb.Build());
        //                                response = await NextMessageAsync(timeout: timeoutDuration);
        //                            }
        //                            else if (response.Content.ToString().ToLower() == "reject")
        //                            {
        //                                await ReplyAsync(embed: new EmbedBuilder()
        //                                    .WithTitle(":no_entry_sign: Trade Process Rejected")
        //                                    .WithColor(Config.Doremi.EmbedColor)
        //                                    .WithDescription($"You have reject the card trade offer from " +
        //                                    $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}.\nThank you for using {TradingCardCore.Doremi.embedName}.")
        //                                    .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                                    .Build());

        //                                //save the file
        //                                string yourUserDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
        //                                var JUserData = JObject.Parse(File.ReadAllText(yourUserDataDirectory));
        //                                ((JObject)JUserData["trading_queue"]).Remove(selectionUserId);
        //                                File.WriteAllText(yourUserDataDirectory, JUserData.ToString());
        //                                isTrading = false;
        //                                Config.Doremi.isRunningInteractive[Context.User.Id.ToString()] = false;
        //                                return Ok();
        //                            }
        //                            else
        //                            {
        //                                await ReplyAsync(embed: new EmbedBuilder()
        //                                    .WithTitle("✅ Trade Process Completed")
        //                                    .WithColor(Config.Doremi.EmbedColor)
        //                                    .WithDescription($"You have successfully accepted the trade offer from " +
        //                                    $"{MentionUtils.MentionUser(Convert.ToUInt64(selectionUserId))}.\n" +
        //                                    $"Thank you for using {TradingCardCore.Doremi.embedName}.")
        //                                    .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
        //                                    .WithThumbnailUrl(TradingCardCore.Doremi.emojiOk)
        //                                    .Build());

        //                                //save to yours
        //                                string yourUserDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{clientId}.json";
        //                                var JYourData = JObject.Parse(File.ReadAllText(yourUserDataDirectory));
        //                                //remove from yours
        //                                JArray arrInventoryYoursRemove = JArray.Parse(JYourData[selectionYourCardPack][selectionYourCardCategory].ToString());
        //                                for (int i = 0; i < arrInventoryYoursRemove.Count; i++)
        //                                {
        //                                    if (arrInventoryYoursRemove[i].ToString() == selectionYourCardChoiceId)
        //                                        arrInventoryYoursRemove[i].Remove();
        //                                }
        //                                JYourData[selectionYourCardPack][selectionYourCardCategory] = arrInventoryYoursRemove;

        //                                //add to yours
        //                                JArray arrInventoryYoursAdd = JArray.Parse(JYourData[selectionOtherUserCardPack][selectionOtherUserCardCategory].ToString());
        //                                arrInventoryYoursAdd.Add(selectionOtherUserCardChoiceId);
        //                                JYourData[selectionOtherUserCardPack][selectionOtherUserCardCategory] = arrInventoryYoursAdd;
        //                                //remove trading_queue
        //                                ((JObject)JYourData["trading_queue"]).Remove(selectionUserId);
        //                                File.WriteAllText(yourUserDataDirectory, JYourData.ToString());
        //                                //==================================================================
        //                                //save to other users
        //                                var JOtherUserData = JObject.Parse(File.ReadAllText(otherUserDataDirectory));
        //                                ((JObject)JOtherUserData["trading_queue"]).Remove(selectionUserId);
        //                                //remove from others
        //                                JArray arrInventoryOthersRemove = JArray.Parse(JOtherUserData[selectionOtherUserCardPack][selectionOtherUserCardCategory].ToString());
        //                                for (int i = 0; i < arrInventoryOthersRemove.Count; i++)
        //                                {
        //                                    if (arrInventoryOthersRemove[i].ToString() == selectionOtherUserCardChoiceId)
        //                                        arrInventoryOthersRemove[i].Remove();
        //                                }
        //                                JOtherUserData[selectionOtherUserCardPack][selectionOtherUserCardCategory] = arrInventoryOthersRemove;

        //                                //add to others
        //                                JArray arrInventoryOthersAdd = JArray.Parse(JOtherUserData[selectionYourCardPack][selectionYourCardCategory].ToString());
        //                                arrInventoryOthersAdd.Add(selectionYourCardChoiceId);
        //                                JOtherUserData[selectionYourCardPack][selectionYourCardCategory] = arrInventoryOthersAdd;
        //                                File.WriteAllText(otherUserDataDirectory, JOtherUserData.ToString());

        //                                isTrading = false;
        //                                Config.Doremi.isRunningInteractive[Context.User.Id.ToString()] = false;
        //                                return Ok();

        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                //Console.WriteLine(e.ToString());
        //            }
        //        }

        //    }
        //    else
        //    {
        //        await ReplyAndDeleteAsync(null, embed: new EmbedBuilder()
        //        .WithColor(Config.Doremi.EmbedColor)
        //        .WithDescription($"I'm sorry, you're still running the card trade process. Please finish it first.")
        //        .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build(), timeout: TimeSpan.FromSeconds(10));
        //    }

        //    return Ok();
        //}

        //[Command("trade fragment", RunMode = RunMode.Async), Summary("Trade fragment pieces with any card.")]
        //public async Task<RuntimeResult> trade_fragment(string cardId="")
        //{
        //    return Ok();
        //    var guildId = Context.Guild.Id;
        //    var userId = Context.User.Id;

        //    var userData = UserTradingCardDataCore.getUserData(userId);
        //    var cardCategory = TradingCardCore.getCardCategory(cardId);
        //    int userFragmentPoint = Convert.ToInt32(userData[DBM_User_Trading_Card_Data.Columns.fragment_point]);

        //    if (cardId == "")
        //    {
        //        await ReplyAndDeleteAsync("Please enter the Card Id that you want to exchange. " +
        //        $"You can see the list of the card that you want to exchange with **inventory command/" +
        //        $"{Doremi.PrefixParent[0]}card inventory**.",
        //        timeout: TimeSpan.FromSeconds(10));
        //        return Ok();
        //    } else if (TradingCardCore.checkUserHaveCard(userId, "doremi", cardId))
        //    {
        //        await ReplyAndDeleteAsync("Sorry, you already have that card.",
        //        timeout: TimeSpan.FromSeconds(10));
        //        return Ok();
        //    } else if(userFragmentPoint <10 && cardCategory == "normal")
        //    {
        //        await ReplyAndDeleteAsync("Sorry, you need **10 fragment points** to trade normal card category.",
        //        timeout: TimeSpan.FromSeconds(10));
        //        return Ok();
        //    }
        //    else if (userFragmentPoint < 20 && cardCategory == "platinum")
        //    {
        //        await ReplyAndDeleteAsync("Sorry, you need **20 fragment points** to trade platinum card category.",
        //        timeout: TimeSpan.FromSeconds(10));
        //        return Ok();
        //    }
        //    else if (userFragmentPoint < 30 && cardCategory == "metal")
        //    {
        //        await ReplyAndDeleteAsync("Sorry, you need **20 fragment points** to trade metal card category.",
        //        timeout: TimeSpan.FromSeconds(10));
        //        return Ok();
        //    }
        //    else if (userFragmentPoint < 20 && cardCategory == "ojamajos")
        //    {
        //        await ReplyAndDeleteAsync("Sorry, you need **20 fragment points** to trade ojamajos card category.",
        //        timeout: TimeSpan.FromSeconds(10));
        //        return Ok();
        //    }

        //    int price = -10; //default price
        //    switch (cardCategory)
        //    {
        //        case "platinum":
        //            price = -20; break;
        //        case "metal":
        //            price = -30; break;
        //        case "ojamajos":
        //            price = -20; break;
        //    }
        //    //if (spawnedCardCategory == "ojamajos")
        //    //{
        //    //    //ojamajo card category
        //    //    string query = $"SELECT * " +
        //    //    $" FROM {DBM_Trading_Card_Data_Ojamajos.tableName} " +
        //    //    $" WHERE {DBM_Trading_Card_Data_Ojamajos.Columns.id_card}=@{DBM_Trading_Card_Data_Ojamajos.Columns.id_card}";
        //    //    Dictionary<string, object> columnFilter = new Dictionary<string, object>();
        //    //    columnFilter[DBM_Trading_Card_Data_Ojamajos.Columns.id_card] = spawnedCardId;
        //    //    var selectedCard = db.selectAll(query, columnFilter);
        //    //    foreach (DataRow rows in selectedCard.Rows)
        //    //    {
        //    //        name = rows[DBM_Trading_Card_Data.Columns.name].ToString();
        //    //        imgUrl = rows[DBM_Trading_Card_Data.Columns.url].ToString();
        //    //        rank = rows[DBM_Trading_Card_Data.Columns.attr_0].ToString();
        //    //        star = rows[DBM_Trading_Card_Data.Columns.attr_1].ToString();
        //    //        point = rows[DBM_Trading_Card_Data.Columns.url].ToString();
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    string query = $"SELECT * " +
        //    //    $" FROM {DBM_Trading_Card_Data.tableName} " +
        //    //    $" WHERE {DBM_Trading_Card_Data.Columns.id_card}=@{DBM_Trading_Card_Data.Columns.id_card}";
        //    //    Dictionary<string, object> columnFilter = new Dictionary<string, object>();
        //    //    columnFilter[DBM_Trading_Card_Data.Columns.id_card] = spawnedCardId;
        //    //    var selectedCard = db.selectAll(query, columnFilter);
        //    //    foreach (DataRow rows in selectedCard.Rows)
        //    //    {
        //    //        name = rows[DBM_Trading_Card_Data.Columns.name].ToString();
        //    //        imgUrl = rows[DBM_Trading_Card_Data.Columns.url].ToString();
        //    //        rank = rows[DBM_Trading_Card_Data.Columns.attr_0].ToString();
        //    //        star = rows[DBM_Trading_Card_Data.Columns.attr_1].ToString();
        //    //        point = rows[DBM_Trading_Card_Data.Columns.url].ToString();
        //    //    }
        //    //}

        //    UserTradingCardDataCore.updateFragmentPoints(userId, price);

        //}

        [Command("shop exclusive", RunMode = RunMode.Async), Summary("Open Card Shop Exclusive Menu.")]
        public async Task<RuntimeResult> openRoyalShopMenu()
        {
            var guildId = Context.Guild.Id;
            var userId = Context.User.Id;

            var userData = UserDataCore.getUserData(userId);

            if (!Config.Doremi.isRunningInteractive.ContainsKey(Context.User.Id.ToString()))
                Config.Doremi.isRunningInteractive.Add(Context.User.Id.ToString(), false);

            if (!Config.Doremi.isRunningInteractive[Context.User.Id.ToString()])
            {
                string playerDataDirectory = $"{Config.Core.headConfigGuildFolder}{guildId}/{Config.Core.headTradingCardConfigFolder}/{userId}.json";
                var jObjTradingCardList = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}{Config.Core.headTradingCardConfigFolder}/trading_card_list.json"));

                string embedName = "Exclusive Card Shop";
                Boolean isRunning = true;
                var timeoutDuration = TimeSpan.FromSeconds(240);
                string replyTimeout = ":stopwatch: I'm sorry, but you have reach your timeout. " +
                    $"Please use the `{Config.Doremi.PrefixParent[0]}card shop exclusive` command to come back again.";
                int stepProcess = 1;//1: select the card pack that you want, 2: select the card category, 3:review process

                Boolean newStep = true;
                string selectedBox = ""; string selectedParent = ""; string selectedColor = ""; string selectedCardId = "";
                int selectedPriceMagicSeeds = -1; int selectedPriceRoyalSeeds = -1;
                int userMagicSeeds = 0; int userRoyalSeeds = 0;
                string concatCardListDisplay = "";

                //var playerData = JObject.Parse(File.ReadAllText(playerDataDirectory));
                userMagicSeeds = Convert.ToInt32(userData[DBM_User_Data.Columns.magic_seeds]);
                userRoyalSeeds = Convert.ToInt32(userData[DBM_User_Data.Columns.royal_seeds]);


                IDictionary<string, string> parentColor = new Dictionary<string, string>();
                parentColor["pink"] = "doremi"; parentColor["orange"] = "hazuki"; parentColor["blue"] = "aiko";
                parentColor["purple"] = "onpu"; parentColor["yellow"] = "momoko";

                PaginatedAppearanceOptions pao = new PaginatedAppearanceOptions();
                pao.JumpDisplayOptions = JumpDisplayOptions.Never;
                pao.DisplayInformationIcon = false;

                List<string> pageContent = new List<string>();

                string[] availableItemNormalSelection = { "pink", "orange", "blue", "purple", "yellow" };

                EmbedBuilder ebTop = new EmbedBuilder()
                .WithAuthor(embedName, Config.Doremi.EmbedAvatarUrl)
                .WithDescription($"Welcome to the Exclusive Card Shop, here you can exchange your royal seeds with the royal box." +
                $"**Please enter the card pack that you want to browse.**\n" +
                $"Type **cancel**/**exit** anytime to exit.\n")
                .AddField($"Your available seeds:",
                $"{userRoyalSeeds} royal seeds")
                .AddField("Select the royal box that you want to open:",
                "**pink**: 1 royal seeds (doremi)\n" +
                "**orange**: 1 royal seeds (hazuki)\n" +
                "**blue**: 1 royal seeds (aiko)\n" +
                "**purple**: 1 royal seeds (onpu)\n" +
                "**yellow**: 1 royal seeds (momoko)")
                .WithColor(Config.Doremi.EmbedColor);

                IUserMessage msg; IUserMessage msg2 = null;
                msg = await ReplyAsync(embed: ebTop.Build());
                Config.Doremi.isRunningInteractive[Context.User.Id.ToString()] = true;
                var response = await NextMessageAsync(timeout: timeoutDuration);
                newStep = false;

                Dictionary<string, string> arrAvailableSelected = new Dictionary<string, string>();

                while (isRunning)
                {
                    EmbedBuilder ebTemplate = new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor);

                    try
                    {
                        var checkNull = response.Content.ToLower().ToString();
                    }
                    catch
                    {
                        try
                        {
                            await Context.Channel.DeleteMessageAsync(msg.Id);
                        }
                        catch { }

                        try
                        {
                            await Context.Channel.DeleteMessageAsync(msg2.Id);
                        }
                        catch
                        {

                        }

                        await ReplyAndDeleteAsync(replyTimeout, timeout: TimeSpan.FromSeconds(15));
                        isRunning = false;
                        Config.Doremi.isRunningInteractive[Context.User.Id.ToString()] = false;
                        return Ok();
                    }

                    //response = await NextMessageAsync(timeout: timeoutDuration);
                    //string responseText = response.Content.ToLower().ToString();

                    if (response.Content.ToString().ToLower() == "cancel" ||
                        response.Content.ToString().ToLower() == "exit")
                    {
                        try
                        {
                            await Context.Channel.DeleteMessageAsync(response.Id);
                            await Context.Channel.DeleteMessageAsync(msg.Id);
                        }
                        catch
                        {

                        }

                        try
                        {
                            await Context.Channel.DeleteMessageAsync(msg2.Id);
                        }
                        catch
                        {

                        }


                        await ReplyAsync(embed: new EmbedBuilder()
                            .WithAuthor(TradingCardCore.Doremi.embedName, Config.Doremi.EmbedAvatarUrl)
                            .WithDescription($"Thank you for visiting the exclusive card shop. Please come back again next time.")
                            .WithColor(Config.Doremi.EmbedColor)
                            .Build());
                        isRunning = false;
                        Config.Doremi.isRunningInteractive[Context.User.Id.ToString()] = false;
                        return Ok();
                    }
                    else if (stepProcess == 1)
                    {
                        if (newStep)
                        {
                            newStep = false;
                            try
                            {
                                await Context.Channel.DeleteMessageAsync(response.Id);
                                await Context.Channel.DeleteMessageAsync(msg.Id);
                            }
                            catch
                            {

                            }

                            try
                            {
                                await Context.Channel.DeleteMessageAsync(msg2.Id);
                            }
                            catch
                            {

                            }

                            msg = await ReplyAsync(embed: ebTop.Build());
                            response = await NextMessageAsync(timeout: timeoutDuration);
                        }
                        else
                        {
                            selectedColor = response.Content.ToString().ToLower();
                            if (!availableItemNormalSelection.Contains(selectedColor))
                            {
                                await ReplyAndDeleteAsync($":x: Sorry, that is not the valid box selection. " +
                                    $"Please re-enter the valid box selection.", timeout: TimeSpan.FromSeconds(10));
                                msg = await ReplyAsync(embed: ebTop.Build());
                                response = await NextMessageAsync(timeout: timeoutDuration);
                            }
                            else
                            {
                                Boolean enoughSeed = false;

                                selectedParent = parentColor[selectedColor];

                                if ((selectedParent == "doremi" || selectedParent == "hazuki" || selectedParent == "aiko" ||
                                    selectedParent == "onpu" || selectedParent == "momoko") && userRoyalSeeds >= 1)
                                {
                                    enoughSeed = true;
                                }

                                if (enoughSeed)
                                {
                                    pageContent.Clear();

                                    int currentIndex = 0;

                                    string title = $"";
                                    concatCardListDisplay = title;

                                    DBC db = new DBC();
                                    string query = @$"select tc.id_card,tc.name,tc.pack,tc.url,tc.attr_0,tc.attr_1,tc.attr_2,inv.id_user as owned 
		                            from {DBM_Trading_Card_Data.tableName} tc 
                                    left join {DBM_User_Trading_Card_Inventory.tableName} inv 
                                    on inv.{DBM_User_Trading_Card_Inventory.Columns.id_user}=@{DBM_User_Trading_Card_Inventory.Columns.id_user} and 
                                    inv.{DBM_User_Trading_Card_Inventory.Columns.id_card}=tc.{DBM_Trading_Card_Data.Columns.id_card} 
                                    where tc.{DBM_Trading_Card_Data.Columns.pack}=@{DBM_Trading_Card_Data.Columns.pack} 
                                    order by rand() 
                                    limit 10";

                                    Dictionary<string, object> colFilter = new Dictionary<string, object>();
                                    colFilter[DBM_User_Trading_Card_Inventory.Columns.id_user] = userId;
                                    colFilter[DBM_Trading_Card_Data.Columns.pack] = selectedParent;

                                    var result = new DBC().selectAll(query, colFilter);
                                    var i = 0;
                                    foreach (DataRow row in result.Rows)
                                    {
                                        string chosenId = row[DBM_Trading_Card_Data.Columns.pack].ToString();
                                        string chosenName = row[DBM_Trading_Card_Data.Columns.pack].ToString();
                                        string chosenUrl = row[DBM_Trading_Card_Data.Columns.url].ToString();
                                        string owned = row["owned"].ToString();
                                        if (owned != "")
                                            concatCardListDisplay += ":white_check_mark: ";
                                        else
                                        {
                                            concatCardListDisplay += ":x: ";
                                            arrAvailableSelected[chosenId] = $"[{chosenId} - {chosenName}]({chosenUrl})";
                                        }

                                        concatCardListDisplay += $"[{chosenId} - {chosenName}]({chosenUrl})\n";

                                        if (i == 9)
                                        {
                                            pageContent.Add(concatCardListDisplay);
                                        }
                                        else
                                        {
                                            if (currentIndex < 9) currentIndex++;
                                            else
                                            {
                                                pageContent.Add(concatCardListDisplay);
                                                currentIndex = 0;
                                                concatCardListDisplay = title;
                                            }
                                        }
                                    }


                                    await ReplyAndDeleteAsync(null, embed: new EmbedBuilder()
                                    .WithDescription($"Magical Stage! Give {MentionUtils.MentionUser(userId)} " +
                                    $"the **{selectedColor} Box**!")
                                    .WithColor(Config.Doremi.EmbedColor)
                                    .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/5b/MOD-EP1-078.png")
                                    .Build(), timeout: TimeSpan.FromSeconds(15));
                                    stepProcess = 2;
                                    newStep = true;

                                    UserDataCore.updateRoyalSeeds(userId, -1);
                                }
                                else
                                {
                                    await ReplyAndDeleteAsync($":x: Sorry, you don't have enough seeds to exchange that box.",
                                        timeout: TimeSpan.FromSeconds(10));
                                    msg = await ReplyAsync(embed: ebTop.Build());
                                    response = await NextMessageAsync(timeout: timeoutDuration);

                                    stepProcess = 1;
                                    newStep = true;
                                }
                            }
                        }
                    }
                    else if (stepProcess == 2)
                    {

                        var pager = new PaginatedMessage
                        {
                            Title = $"**Available Card Selection**:\n",
                            Pages = pageContent,
                            Color = Config.Doremi.EmbedColor,
                            Options = pao
                        };

                        if (newStep)
                        {
                            newStep = false;
                            msg = await ReplyAsync("Select 1 **Card Id** that you want to take. Type **exit/cancel** to exit the card selection.");
                            msg2 = await PagedReplyAsync(pager);
                            response = await NextMessageAsync(timeout: timeoutDuration);

                            try
                            {
                                await Context.Channel.DeleteMessageAsync(response.Id);
                                await Context.Channel.DeleteMessageAsync(msg.Id);
                            }
                            catch
                            {

                            }

                            try
                            {
                                await Context.Channel.DeleteMessageAsync(msg2.Id);
                            }
                            catch
                            {

                            }

                        }
                        else
                        {

                            selectedCardId = response.Content.ToString();

                            if (arrAvailableSelected.ContainsKey(selectedCardId))
                            {
                                string[] splittedImgUrl = arrAvailableSelected[selectedCardId].Split("](");
                                string imgUrl = splittedImgUrl[1].Remove(splittedImgUrl[1].Length - 1);

                                await ReplyAsync(embed: new EmbedBuilder()
                                    .WithDescription($"You have received: **{arrAvailableSelected[selectedCardId]}**.\n" +
                                    $"Thank you for visiting the **exclusive card shop**.")
                                    .WithImageUrl(imgUrl)
                                    .WithColor(Config.Doremi.EmbedColor)
                                    .Build());

                                //playerData["magic_seeds"] = userRoyalSeeds;
                                //add card
                                Dictionary<string, object> columnInsert = new Dictionary<string, object>();
                                columnInsert[DBM_User_Trading_Card_Inventory.Columns.id_card] = selectedCardId;
                                columnInsert[DBM_User_Trading_Card_Inventory.Columns.id_user] = userId;
                                new DBC().insert(DBM_User_Trading_Card_Inventory.tableName, columnInsert);

                                //terminate the interactive
                                Config.Doremi.isRunningInteractive[Context.User.Id.ToString()] = false;
                                isRunning = false;
                            }
                            else
                            {
                                await ReplyAndDeleteAsync($":x: Sorry, that is not the valid **card id** selection.", timeout: TimeSpan.FromSeconds(10));
                                msg = await ReplyAsync("Select 1 **Card Id** that you want to take. Type **exit/cancel** to exit the card selection.");
                                msg2 = await PagedReplyAsync(pager);
                                response = await NextMessageAsync(timeout: timeoutDuration);
                            }
                        }
                    }
                }

            }
            else
            {
                await ReplyAndDeleteAsync(null, embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription($":x: I'm sorry, you are still running the interactive command. Please finish it first.")
                .WithThumbnailUrl(TradingCardCore.Doremi.emojiError).Build(), timeout: TimeSpan.FromSeconds(10));
            }

            return Ok();
        }

        [Command("shop", RunMode = RunMode.Async), Summary("Open Doremi Card Shop Menu.")]
        public async Task<RuntimeResult> openShop()
        {
            DBC db = new DBC();
            var userId = Context.User.Id;

            var userTradingCardData = UserTradingCardDataCore.getUserData(userId);
            var userData = UserDataCore.getUserData(userId);
            Dictionary<string, object> columns = new Dictionary<string, object>();
            string itemsListText = "1. Parara tap - 30 seeds\n" +
                "2. Peperuto Pollon - 15 seeds\n" +
                "3. Puwapuwa Pollon - 15 seeds\n" +
                "4. Poppun Pollon - 15 seeds\n" +
                "5. Apprentice Tap - 20 seeds\n" +
                "6. Rhythm Tap - 35 seeds\n" +
                "7. Kururu Pollon - 55 seeds\n" +
                "8. Picotto Pollon - 60 seeds\n" +
                "9. Patraine Call - 60 seeds\n" +
                "10. Wreath Pollon - 65 seeds\n" +
                "11. Jewelry Pollon - 85 seeds";
            Boolean isShopping = true; int stepProcess = 1;
            int selectionItem = 0; int priceConfirmation = 0;
            int magicSeeds = Convert.ToInt32(userData[DBM_User_Data.Columns.magic_seeds].ToString());

            var timeoutDuration = TimeSpan.FromSeconds(60);
            string concatResponseSuccess = "";

            IUserMessage answerUserTemp = null;
            IUserMessage messageTemp = null;
            messageTemp = await ReplyAsync(embed: new EmbedBuilder()
                .WithAuthor("Doremi Card Shop", Config.Doremi.EmbedAvatarUrl)
                .WithColor(Config.Doremi.EmbedColor)
                .WithDescription("Welcome to Doremi Card Shop. " +
                "Here you can purchase some items to help your card collecting progression.\n" +
                "Type **exit** or **cancel** anytime to close the shop menu.\n" +
                "Select with numbers from these list to browse & purchase:")
                .AddField("Item List", itemsListText)
                .Build());

            var response = await NextMessageAsync(timeout: timeoutDuration);

            try
            {
                await Context.Message.DeleteAsync();
                await Context.Channel.DeleteMessageAsync(messageTemp.Id);
            }
            catch
            {

            }

            string replyTimeout = ":stopwatch: I'm sorry, you're not giving valid selection yet. " +
            $"Please use the `{Config.Doremi.PrefixParent[0]}card shop` command to open shop menu again.";

            while (isShopping)
            {
                try
                {
                    var checkNull = response.Content.ToLower().ToString();
                    answerUserTemp = (IUserMessage)response;
                }
                catch
                {
                    await Context.Channel.DeleteMessageAsync(answerUserTemp.Id);
                    await Context.Channel.DeleteMessageAsync(messageTemp.Id);

                    await ReplyAndDeleteAsync(replyTimeout, timeout: TimeSpan.FromSeconds(15));
                    //isShopping = false;
                    return Ok();
                }

                if (response.Content.ToString().ToLower() == "cancel" || response.Content.ToString().ToLower() == "exit")
                {
                    try
                    {
                        await Context.Channel.DeleteMessageAsync(answerUserTemp.Id);
                        await Context.Channel.DeleteMessageAsync(messageTemp.Id);
                    }
                    catch
                    {

                    }

                    await ReplyAsync(embed: new EmbedBuilder()
                        .WithAuthor("Doremi Card Shop", Config.Doremi.EmbedAvatarUrl)
                        .WithDescription($"Thank you for visiting the Doremi Card Shop.")
                        .WithColor(Config.Doremi.EmbedColor)
                        .Build());
                    //isShopping = false;
                    return Ok();
                }

                if (stepProcess == 1)
                {
                    var isNumeric = int.TryParse(response.Content.ToString().ToLower(), out int n);
                    if (!isNumeric || Convert.ToInt32(response.Content.ToString()) <= 0 ||
                        Convert.ToInt32(response.Content.ToString()) >= 12)
                    {
                        try
                        {
                            await Context.Channel.DeleteMessageAsync(answerUserTemp.Id);
                            await Context.Channel.DeleteMessageAsync(messageTemp.Id);
                        }
                        catch
                        {

                        }

                        messageTemp = await ReplyAsync(embed: new EmbedBuilder()
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
                    try
                    {
                        await Context.Channel.DeleteMessageAsync(answerUserTemp.Id);
                        await Context.Channel.DeleteMessageAsync(messageTemp.Id);
                    }
                    catch
                    {

                    }

                    //menu selection
                    switch (selectionItem)
                    {
                        case 1:
                            priceConfirmation = 30;
                            messageTemp = await ReplyAsync("Type **confirm** to proceed with the purchase/**back** to go back to previous menu/**exit** to exit the shop menu.",
                            embed: new EmbedBuilder()
                            .WithColor(Config.Doremi.EmbedColor)
                            .WithAuthor("Doremi Card Shop", Config.Doremi.EmbedAvatarUrl)
                            .WithTitle("Parara Tap Card")
                            .WithDescription("This card will give you another chance to catch card again.")
                            .AddField("Price:", priceConfirmation, true)
                            .AddField("Your Magic Seeds:", magicSeeds, true)
                            .WithThumbnailUrl("https://cdn.discordapp.com/attachments/708332290446721066/709045172461240330/pararatap.jpg")
                            .Build());
                            break;
                        case 2:
                            priceConfirmation = 15;
                            messageTemp = await ReplyAsync("**:exclamation: Please note that purchasing this card will replace all status boost!**\n" +
                                "Type **confirm** to proceed with the purchase/**back** to go back to previous menu/**exit** to exit the shop menu.",
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
                            break;
                        case 3:
                            priceConfirmation = 15;
                            messageTemp = await ReplyAsync("**:exclamation: Please note that purchasing this card will replace all status boost!**\n" +
                                "Type **confirm** to proceed with the purchase/**back** to go back to previous menu/**exit** to exit the shop menu.",
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
                            break;
                        case 4:
                            priceConfirmation = 15;
                            messageTemp = await ReplyAsync("**:exclamation: Please note that purchasing this card will replace all status boost!**\n" +
                                "Type **confirm** to proceed with the purchase/**back** to go back to previous menu/**exit** to exit the shop menu.",
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
                            break;
                        case 5:
                            priceConfirmation = 20;
                            messageTemp = await ReplyAsync("**:exclamation: Please note that purchasing this card will replace all status boost!**\n" +
                                "Type **confirm** to proceed with the purchase/**back** to go back to previous menu/**exit** to exit the shop menu.",
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
                            break;
                        case 6:
                            priceConfirmation = 35;
                            messageTemp = await ReplyAsync("**:exclamation: Please note that purchasing this card will replace all status boost!**\n" +
                                "Type **confirm** to proceed with the purchase/**back** to go back to previous menu/**exit** to exit the shop menu.",
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
                            break;
                        case 7:
                            priceConfirmation = 55;
                            messageTemp = await ReplyAsync("**:exclamation: Please note that purchasing this card will replace all status boost!**\n" +
                                "Type **confirm** to proceed with the purchase/**back** to go back to previous menu/**exit** to exit the shop menu.",
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
                            break;
                        case 8:
                            priceConfirmation = 60;
                            messageTemp = await ReplyAsync("**:exclamation: Please note that purchasing this card will replace all status boost!**\n" +
                                "Type **confirm** to proceed with the purchase/**back** to go back to previous menu/**exit** to exit the shop menu.",
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
                            break;
                        case 9:
                            priceConfirmation = 60;
                            messageTemp = await ReplyAsync("**:exclamation: Please note that purchasing this card will replace all status boost!**\n" +
                                "Type **confirm** to proceed with the purchase/**back** to go back to previous menu/**exit** to exit the shop menu.",
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
                            break;
                        case 10:
                            priceConfirmation = 65;
                            messageTemp = await ReplyAsync("**:exclamation: Please note that purchasing this card will replace all status boost!**\n" +
                                "Type **confirm** to proceed with the purchase/**back** to go back to previous menu/**exit** to exit the shop menu.",
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
                            break;
                        case 11:
                            priceConfirmation = 85;
                            messageTemp = await ReplyAsync("**:exclamation: Please note that purchasing this card will replace all status boost!**\n" +
                                "Type **confirm** to proceed with the purchase/**back** to go back to previous menu/**exit** to exit the shop menu.",
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
                            break;
                    }

                    stepProcess = 3;
                    response = await NextMessageAsync(timeout: timeoutDuration);
                }
                else if (stepProcess == 3)
                {
                    if (response.Content.ToString().ToLower() != "confirm" &&
                        response.Content.ToString().ToLower() != "back")
                    {
                        try
                        {
                            //await Context.Channel.DeleteMessageAsync(answerUserTemp.Id);
                            await Context.Channel.DeleteMessageAsync(messageTemp.Id);
                        }
                        catch
                        {

                        }

                        await ReplyAndDeleteAsync(":x: Sorry, that is not the valid **confirm/back** choices.", timeout: TimeSpan.FromSeconds(10));
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
                    if (magicSeeds >= priceConfirmation)
                    {
                        try
                        {
                            await Context.Channel.DeleteMessageAsync(answerUserTemp.Id);
                            await Context.Channel.DeleteMessageAsync(messageTemp.Id);
                        }
                        catch
                        {

                        }

                        concatResponseSuccess = $":sparkles: **{Context.User.Username}** ";

                        if (selectionItem == 1)
                        {
                            //update catch token
                            string query = $"UPDATE {DBM_User_Trading_Card_Data.tableName} " +
                                $" SET {DBM_User_Trading_Card_Data.Columns.catch_token}=@{DBM_User_Trading_Card_Data.Columns.catch_token}  " +
                                $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user} ";

                            columns = new Dictionary<string, object>();
                            columns[DBM_User_Trading_Card_Data.Columns.catch_token] = "";
                            columns[DBM_User_Trading_Card_Data.Columns.id_user] = userId.ToString();
                            db.update(query, columns);

                            //userTradingCardData[DBM_User_Trading_Card_Data.Columns.catch_token] 
                            //arrInventory["catch_token"] = "";
                            concatResponseSuccess += $"has received 1 more card capture attempt!";
                        }
                        else if (selectionItem == 2)
                        {

                            string query = $"UPDATE {DBM_User_Trading_Card_Data.tableName} " +
                                $" SET {DBM_User_Trading_Card_Data.Columns.boost_doremi_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_normal}  " +
                                $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user} ";

                            columns = new Dictionary<string, object>();
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.id_user] = userId.ToString();
                            db.update(query, columns);

                            concatResponseSuccess += $"received **Peperuto Pollon Card** Boost!";
                        }
                        else if (selectionItem == 3)
                        {
                            string query = $"UPDATE {DBM_User_Trading_Card_Data.tableName} " +
                                $" SET {DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal}  " +
                                $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user} ";

                            columns = new Dictionary<string, object>();
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.id_user] = userId.ToString();
                            db.update(query, columns);

                            concatResponseSuccess += $"received **Puwapuwa Pollon Card** Boost!";
                        }
                        else if (selectionItem == 4)
                        {
                            string query = $"UPDATE {DBM_User_Trading_Card_Data.tableName} " +
                                $" SET {DBM_User_Trading_Card_Data.Columns.boost_aiko_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_normal}  " +
                                $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user} ";

                            columns = new Dictionary<string, object>();
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.id_user] = userId.ToString();
                            db.update(query, columns);

                            concatResponseSuccess += $"received **Poppun Pollon Card** Boost!";
                        }
                        else if (selectionItem == 5)
                        {
                            string query = $"UPDATE {DBM_User_Trading_Card_Data.tableName} " +
                            $" SET {DBM_User_Trading_Card_Data.Columns.boost_doremi_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_normal}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_metal}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_momoko_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_momoko_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_momoko_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_momoko_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_momoko_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_momoko_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_momoko_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_momoko_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_other_special}=@{DBM_User_Trading_Card_Data.Columns.boost_other_special} " +

                            $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user} ";

                            columns = new Dictionary<string, object>();
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum] = 6;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_metal] = 5;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos] = 3;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum] = 6;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal] = 5;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos] = 3;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum] = 6;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_metal] = 5;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos] = 3;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum] = 6;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_metal] = 5;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos] = 3;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_momoko_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_momoko_platinum] = 6;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_momoko_metal] = 5;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_momoko_ojamajos] = 3;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_other_special] = 6;
                            columns[DBM_User_Trading_Card_Data.Columns.id_user] = userId.ToString();

                            db.update(query, columns);

                            concatResponseSuccess += "received **Apprentice Tap Card** Boost!";
                        }
                        else if (selectionItem == 6)
                        {
                            string query = $"UPDATE {DBM_User_Trading_Card_Data.tableName} " +
                            $" SET {DBM_User_Trading_Card_Data.Columns.boost_doremi_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_normal}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_metal}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_other_special}=@{DBM_User_Trading_Card_Data.Columns.boost_other_special} " +

                            $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user} ";

                            columns = new Dictionary<string, object>();
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_metal] = 6;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos] = 4;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal] = 6;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos] = 4;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_metal] = 6;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos] = 4;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_metal] = 6;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos] = 4;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_other_special] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.id_user] = userId.ToString();

                            db.update(query, columns);
                            concatResponseSuccess += "received **Rythm Tap Card** Boost!";
                        }
                        else if (selectionItem == 7)
                        {
                            string query = $"UPDATE {DBM_User_Trading_Card_Data.tableName} " +
                            $" SET {DBM_User_Trading_Card_Data.Columns.boost_doremi_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_normal}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_metal}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_momoko_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_momoko_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_momoko_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_momoko_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_momoko_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_momoko_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_momoko_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_momoko_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_other_special}=@{DBM_User_Trading_Card_Data.Columns.boost_other_special} " +

                            $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user} ";

                            columns = new Dictionary<string, object>();
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_metal] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos] = 5;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos] = 5;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_metal] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos] = 5;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_metal] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos] = 5;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_momoko_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_momoko_platinum] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_momoko_metal] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_momoko_ojamajos] = 5;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_other_special] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.id_user] = userId.ToString();

                            db.update(query, columns);

                            concatResponseSuccess += "received **Kururun Pollon Card** Boost!";
                        }
                        else if (selectionItem == 8)
                        {
                            string query = $"UPDATE {DBM_User_Trading_Card_Data.tableName} " +
                            $" SET {DBM_User_Trading_Card_Data.Columns.boost_doremi_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_normal}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_metal}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_momoko_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_momoko_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_momoko_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_momoko_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_momoko_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_momoko_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_momoko_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_momoko_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_other_special}=@{DBM_User_Trading_Card_Data.Columns.boost_other_special} " +

                            $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user} ";

                            columns = new Dictionary<string, object>();
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_metal] = 8;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos] = 6;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum] = 6;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal] = 5;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos] = 3;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum] = 6;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_metal] = 5;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos] = 3;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum] = 6;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_metal] = 5;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos] = 3;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_momoko_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_momoko_platinum] = 6;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_momoko_metal] = 5;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_momoko_ojamajos] = 3;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_other_special] = 6;
                            columns[DBM_User_Trading_Card_Data.Columns.id_user] = userId.ToString();

                            db.update(query, columns);

                            concatResponseSuccess += "received **Picotto Pollon Card** Boost!";
                        }
                        else if (selectionItem == 9)
                        {
                            string query = $"UPDATE {DBM_User_Trading_Card_Data.tableName} " +
                            $" SET {DBM_User_Trading_Card_Data.Columns.boost_doremi_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_normal}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_metal}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos}, " +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_other_special}=@{DBM_User_Trading_Card_Data.Columns.boost_other_special} " +

                            $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user} ";

                            columns = new Dictionary<string, object>();
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum] = 8;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_metal] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos] = 7;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum] = 8;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos] = 7;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum] = 8;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_metal] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos] = 7;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum] = 8;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_metal] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos] = 7;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_other_special] = 6;
                            columns[DBM_User_Trading_Card_Data.Columns.id_user] = userId.ToString();

                            db.update(query, columns);

                            concatResponseSuccess += "received **Patraine Call Card** Boost!";
                        }
                        else if (selectionItem == 10)
                        {
                            string query = $"UPDATE {DBM_User_Trading_Card_Data.tableName} " +
                            $" SET {DBM_User_Trading_Card_Data.Columns.boost_doremi_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_normal}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_metal}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_other_special}=@{DBM_User_Trading_Card_Data.Columns.boost_other_special} " +

                            $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user} ";

                            columns = new Dictionary<string, object>();
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum] = 9;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_metal] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos] = 8;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum] = 9;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos] = 8;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum] = 9;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_metal] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos] = 8;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum] = 9;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_metal] = 7;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos] = 8;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_other_special] = 8;
                            columns[DBM_User_Trading_Card_Data.Columns.id_user] = userId.ToString();

                            db.update(query, columns);
                            concatResponseSuccess += "received **Wreath Pollon Card** Boost!";
                        }
                        else if (selectionItem == 11)
                        {
                            string query = $"UPDATE {DBM_User_Trading_Card_Data.tableName} " +
                            $" SET {DBM_User_Trading_Card_Data.Columns.boost_doremi_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_normal}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_metal}, " +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_momoko_normal}=@{DBM_User_Trading_Card_Data.Columns.boost_momoko_normal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_momoko_platinum}=@{DBM_User_Trading_Card_Data.Columns.boost_momoko_platinum}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_momoko_metal}=@{DBM_User_Trading_Card_Data.Columns.boost_momoko_metal}," +
                            $" {DBM_User_Trading_Card_Data.Columns.boost_momoko_ojamajos}=@{DBM_User_Trading_Card_Data.Columns.boost_momoko_ojamajos}," +

                            $" {DBM_User_Trading_Card_Data.Columns.boost_other_special}=@{DBM_User_Trading_Card_Data.Columns.boost_other_special} " +

                            $" WHERE {DBM_User_Trading_Card_Data.Columns.id_user}=@{DBM_User_Trading_Card_Data.Columns.id_user} ";

                            columns = new Dictionary<string, object>();
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_platinum] = 9;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_metal] = 8;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_doremi_ojamajos] = 8;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_platinum] = 9;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_metal] = 8;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_hazuki_ojamajos] = 8;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_platinum] = 9;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_metal] = 8;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_aiko_ojamajos] = 8;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_platinum] = 9;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_metal] = 8;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_onpu_ojamajos] = 8;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_momoko_normal] = 10;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_momoko_platinum] = 9;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_momoko_metal] = 8;
                            columns[DBM_User_Trading_Card_Data.Columns.boost_momoko_ojamajos] = 8;

                            columns[DBM_User_Trading_Card_Data.Columns.boost_other_special] = 8;
                            columns[DBM_User_Trading_Card_Data.Columns.id_user] = userId.ToString();

                            db.update(query, columns);
                            concatResponseSuccess += "received **Jewelry Pollon Card** Boost!";
                        }

                        if (selectionItem >= 2)
                        {
                            concatResponseSuccess += " You can activate it on the card that has spawned with **<bot>!card capture boost**";
                        }

                        UserDataCore.updateMagicSeeds(userId, -priceConfirmation);

                        stepProcess = 5;
                    }
                    else
                    {
                        try
                        {
                            await Context.Channel.DeleteMessageAsync(answerUserTemp.Id);
                            await Context.Channel.DeleteMessageAsync(messageTemp.Id);
                        }
                        catch
                        {

                        }

                        stepProcess = 2;
                        await ReplyAndDeleteAsync(":x: Sorry, you don't have enough magic seeds.", timeout: TimeSpan.FromSeconds(10));
                    }
                }
                else if (stepProcess == 5)
                {
                    await ReplyAsync(concatResponseSuccess + "\nThank you for purchasing the items. Please come again next time~");
                    //isShopping = false;
                    return Ok();
                }

            }

            return Ok();
        }

        [Command("boost", RunMode = RunMode.Async), Summary("Show card boost status.")]
        public async Task showCardBoostStatus()
        {
            await ReplyAsync(embed: TradingCardCore
                    .printCardBoostStatus(Context, Config.Doremi.EmbedColor)
                    .Build());
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
            $"Special: {TradingCardCore.captureRateSpecial * 10}%**", true)
            .AddField("Spawn Rate",
            $"**Normal: {TradingCardCore.spawnRateNormal * 10}%\n" +
            $"Platinum: {TradingCardCore.spawnRatePlatinum * 10}%\n" +
            $"Metal: {TradingCardCore.spawnRateMetal * 10}%\n" +
            $"Ojamajos: {TradingCardCore.spawnRateOjamajos * 10}%**", true)
            .Build());
        }

        [Command("timer", RunMode = RunMode.Async), Summary("Check the next card spawn timer.")]
        public async Task<RuntimeResult> trading_card_timer_spawn()
        {
            var guildId = Context.Guild.Id;
            var guildSpawnData = TradingCardGuildCore.getGuildData(guildId);

            var totMinutes = Config.Doremi._stopwatchCardSpawn[guildId.ToString()].Elapsed.TotalMinutes;
            var guildSpawnInterval = Convert.ToInt32(guildSpawnData[DBM_Trading_Card_Guild.Columns.spawn_interval]);
            var nextSpawn = Convert.ToInt32(guildSpawnInterval) - Convert.ToInt32(totMinutes);
            string finalSpawn = nextSpawn.ToString();
            if (nextSpawn <= 0) finalSpawn = "less than 1";

            try
            {
                await Task.Run(async () =>
                    await Context.Message.DeleteAsync()
                );

                await ReplyAndDeleteAsync(null, embed: new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithDescription($":stopwatch: Next card will spawn approximately at **{finalSpawn} minute(s).**")
                    .Build(), timeout: TimeSpan.FromSeconds(15));
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
            }
            return Ok();
        }

        [Command("zone price"), Alias("region price"), Summary("See the card zone price.")]
        public async Task setCardZone()
        {
            await ReplyAsync(embed: new EmbedBuilder()
                .WithDescription("**normal**: 40 magic seeds.\n" +
                "**platinum**: 60 magic seeds.\n" +
                "**metal**: 80 magic seeds.\n" +
                "**ojamajos**: 40 magic seeds.")
                .Build());
        }

        [Command("zone set"), Alias("region set"), Summary("Set your card zone at **doremi** and the entered category." +
            "Example: **do!card zone platinum**.")]
        public async Task setCardZone(string category = "")
        {
            await ReplyAsync(embed: TradingCardCore.assignZone(Context, "doremi", category, Config.Doremi.EmbedColor)
                .Build());
        }

        [Command("zone where"), Alias("region where"), Summary("Get your assigned card zone.")]
        public async Task lookCardZone()
        {
            await ReplyAsync(embed: TradingCardCore.lookZone(Context, Config.Doremi.EmbedColor)
                .Build());
        }

        [Command("updates"), Alias("update"), Summary("Show the Ojamajo Trading Card Updates.")]
        public async Task showCardUpdates()
        {
            await Context.Message.DeleteAsync();
            await ReplyAsync(embed: TradingCardCore.
                    printUpdatesNote()
                    .Build());
        }

    }

    [Name("memesdraw"), Group("memesdraw"), Summary("Memes Draw Category.")]
    public class DorememesModule : ModuleBase<SocketCommandContext>
    {
        [Command("template list", RunMode = RunMode.Async), Summary("Show all available dorememes generator template.")]
        public async Task showAllDorememesTemplate()
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.Color = Config.Doremi.EmbedColor;
            builder.Title = "Dorememes Generator Template List";
            builder.Description = "Here are the available dorememes generator template that can be used on `dorememes draw` commands.";

            var guildId = Context.Guild.Id;
            var guildJsonFile = (JObject)JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}ojamajo_meme_template/list.json")).GetValue("list");
            var jobjmemelist = guildJsonFile.Properties().ToList();
            string finalList = "";

            for (int i = 0; i < jobjmemelist.Count; i++)
            {
                finalList += $"{jobjmemelist[i].Name} : {Path.GetFileNameWithoutExtension(jobjmemelist[i].Value.ToString())}\n";
            }
            builder.AddField("[Numbers] : Title", finalList);

            await ReplyAsync(embed: builder.Build());
        }

        [Command("template show", RunMode = RunMode.Async), Summary("Show the image preview of dorememes template from `dorememes template list` commands.\n" +
            "You can fill the <choices> parameter with numbers/title.")]
        public async Task showDorememesImageTemplate([Remainder] string choices)
        {
            bool isNumeric = choices.All(char.IsDigit);

            var guildId = Context.Guild.Id;
            var guildJsonFile = (JObject)JObject.Parse(File.ReadAllText($"{Config.Core.headConfigFolder}ojamajo_meme_template/list.json")).GetValue("list");
            var jobjmemelist = guildJsonFile.Properties().ToList();
            bool isFounded = false;

            string selectedFile = $"{Config.Core.headConfigFolder}ojamajo_meme_template/";

            for (int i = 0; i < jobjmemelist.Count; i++)
            {
                var checkedKey = jobjmemelist[i].Name.ToString();
                var checkedValue = jobjmemelist[i].Value.ToString();

                if ((isNumeric && choices == checkedKey) ||
                    (!isNumeric && choices == Path.GetFileNameWithoutExtension(checkedValue)))
                {
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
            string template; string text; string[] splittedParameter; string positions = "top";

            if (parameter.Contains(";"))
            {
                splittedParameter = parameter.Split(";");
                template = splittedParameter[0];
                text = splittedParameter[1];
                if (text.Length >= 40)
                {
                    await ReplyAsync($"Sorry, that text is too long. Please use shorter text.");
                    return;
                }
                if (2 < splittedParameter.Length)
                {
                    if (splittedParameter[2].ToLower() != "top" || splittedParameter[2].ToLower() != "bottom")
                        positions = splittedParameter[2].ToLower();
                    else
                    {
                        await ReplyAsync($"Sorry, **positions** parameter need to be `top` or `bottom`.");
                        return;
                    }
                }
            }
            else
            {
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

            for (int i = 0; i < jobjmemelist.Count; i++)
            {
                var checkedKey = jobjmemelist[i].Name.ToString();
                var checkedValue = jobjmemelist[i].Value.ToString();

                if ((isNumeric && template == checkedKey) ||
                    (!isNumeric && template == Path.GetFileNameWithoutExtension(checkedValue)))
                {
                    selectedGetFileName = checkedValue;
                    selectedFile += checkedValue;
                    isFounded = true;
                    nowProcessing = await ReplyAsync($"\u23F3 Processing the dorememes, please wait for a moment...");
                    break;
                }
            }

            if (!isFounded)
            {
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
                        if (positions == "top")
                        {
                            PointF topLocation = new PointF(bitmap.Width / 2, 100f);
                            graphics.DrawString(text, goodFont, Brushes.White, topLocation, sf);
                        }
                        else if (positions == "bottom")
                        {
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
        public async Task drawMemesdrawToBeContinue(string attachment = "")
        {

            try
            {
                var attachments = Context.Message.Attachments;
                WebClient myWebClient = new WebClient();

                string file = attachments.ElementAt(0).Filename;
                string url = attachments.ElementAt(0).Url;
                string extension = Path.GetExtension(attachments.ElementAt(0).Filename).ToLower();
                string randomedFileName = "jojofication_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + new Random().Next(0, 10000) + extension;
                string completePath = $"attachments/{Context.Guild.Id}/{randomedFileName}";
                string toBeContinueImagePath = $"{Config.Core.headConfigFolder}ojamajo_meme_template/to be continue.png";
                string resizedToBeContinueImagePath = $"attachments/{Context.Guild.Id}/to be continue.png";

                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif")
                {
                    IMessage nowProcessing = await ReplyAsync($"\u23F3 Processing the image, please wait for a moment...");

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

                }
                else
                {
                    await ReplyAsync($"Oops, sorry I can only process `.jpg/.jpeg/.png/.gif` image format.");
                    return;
                }

            }
            catch (Exception e)
            {
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
            if (Context.User.Id == Config.Momoko.Id)
            {
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
        [Command("score"), Summary("Show your minigame score points.")]
        public async Task Show_Minigame_Score()
        {//show the player score
            await ReplyAsync(embed: MinigameCore.printScore(Context, Config.Doremi.EmbedColor).Build());
        }

        [Command("leaderboard"), Summary("Show the top 10 player with the highest score points.")]
        public async Task Show_Minigame_Leaderboard()
        {//show top 10 player score
            await ReplyAsync(embed: MinigameCore.printLeaderboard(Context, Config.Doremi.EmbedColor).Build());
        }

        [Command("jankenpon", RunMode = RunMode.Async), Alias("rps","rockpaperscissors"), Summary("Play the Rock Paper Scissors minigame with Doremi. " +
            "Reward: 20 minigame score points & 1 magic seeds.")]
        public async Task RockPaperScissors(string guess = "")
        {
            if (guess == "")
            {
                await ReplyAsync($":x: Please enter the valid parameter: **rock** or **paper** or **scissors**");
                return;
            }
            else if (guess.ToLower() != "rock" && guess.ToLower() != "paper" && guess.ToLower() != "scissors")
            {
                await ReplyAsync($":x: Sorry **{Context.User.Username}**. " +
                    $"Please enter the valid parameter: **rock** or **paper** or **scissors**");
                return;
            }

            guess = guess.ToLower();//lower the text
            int randomGuess = new Random().Next(0, 3);//generate random

            string[] arrWinReaction = { "Looks like I win the game this time.",
                $"Sorry {Context.User.Username}, better luck next time.",
                $"No way! I guess you will have to pay me a {Config.Emoji.steak}"};//bot win
            string[] arrLoseReaction = { "I'm the world unluckiest pretty girl! :sob:",
                "Oh no, looks like I lose the game."};//bot lose
            string[] arrDrawReaction = { "Ehh, it's a draw!", "We got a draw this time." };//bot draw

            Tuple<string, EmbedBuilder, Boolean> result = MinigameCore.rockPaperScissors.rpsResults(
                Config.Doremi.EmbedColor, Config.Doremi.EmbedAvatarUrl, randomGuess, guess, "doremi", Context.User.Username,
                arrWinReaction, arrLoseReaction, arrDrawReaction,
                Context.Guild.Id, Context.User.Id);

            //isWin?
            if (result.Item3)
            {

            }

            await Context.Channel.SendFileAsync(result.Item1, embed: result.Item2.Build());
        }

        [Command("hangman", RunMode = RunMode.Async), Summary("Play the hangman game with the available category.\n**Available category:** `random`/`characters`/`color`/`fruit`/`animal`\n" +
            "**Available difficulty:**\n" +
            "**easy:** 30 seconds, 10 lives, score+10,magic seeds+5\n" +
            "**medium:** 20 seconds, 7 lives, score+20,magic seeds+8\n" +
            "**hard:** 15 seconds, 5 lives, score+30,magic seeds+10")]
        public async Task<RuntimeResult> Interact_Quiz_Hangman(string category = "random", string difficulty = "easy")
        {
            TimeSpan autoDeleteTimespan = TimeSpan.FromSeconds(15);
            IMessage respondBot;
            //check first if category available on quiz.json/not
            if (category.ToLower() != "random" && !Config.Core.jobjectQuiz.ContainsKey(category.ToLower()))
            {
                await ReplyAndDeleteAsync($"Sorry, I can't find that category. Available category options: **random**/**characters**/**color**/**fruit**/**animal**",
                    timeout: autoDeleteTimespan);
                return Ok();
            }

            if (difficulty.ToLower() != "easy" && difficulty.ToLower() != "medium" && difficulty.ToLower() != "hard")
            {
                await ReplyAndDeleteAsync($"Sorry, I can't find that difficulty. Available difficulty options: **easy**/**medium**/**hard**",
                    timeout: autoDeleteTimespan);
                return Ok();
            }

            if (!Config.Doremi.isRunningMinigame.ContainsKey(Context.User.Id.ToString()))
                Config.Doremi.isRunningMinigame.Add(Context.User.Id.ToString(), false);

            if (!Config.Doremi.isRunningMinigame[Context.User.Id.ToString()])
            {
                Config.Doremi.isRunningMinigame[Context.User.Id.ToString()] = true;
                //default difficulty: easy
                int lives = 10; var timeoutDuration = 30;//in seconds
                int scoreValue = 10;//default score
                int magicSeedsValue = 5;//default magic seeds reward

                if (difficulty.ToLower() == "medium")
                {
                    lives = 7; timeoutDuration = 20; scoreValue = 20; magicSeedsValue = 8;
                }
                else if (difficulty.ToLower() == "hard")
                {
                    lives = 5; timeoutDuration = 15; scoreValue = 30; magicSeedsValue = 10;
                }

                string key = category;//default:random

                if (category.ToLower() == "random")
                {
                    //default: random
                    var jobjquiz = Config.Core.jobjectQuiz.Properties().ToList();
                    key = jobjquiz[new Random().Next(0, jobjquiz.Count)].Name;
                }

                var arrRandomed = (JArray)Config.Core.jobjectQuiz.GetValue(key);
                string randomedAnswer = arrRandomed[new Random().Next(0, arrRandomed.Count)].ToString();
                string replacedAnswer = ""; string[] containedAnswer = { }; List<string> guessedWord = new List<string>();
                for (int i = 0; i < randomedAnswer.Length; i++)
                {
                    if (randomedAnswer.Substring(i, 1) != " ")
                    {
                        replacedAnswer += randomedAnswer.Substring(i, 1).Replace(randomedAnswer.Substring(i, 1), "_ ");
                    }
                    else if (randomedAnswer.Substring(i, 1) == " ")
                    {
                        replacedAnswer += "  ";
                    }

                }

                string tempRandomedAnswer = string.Join(" ", randomedAnswer.ToCharArray()) + " "; //with space

                string questionsFormat = $"Can you guess what **{key}** is this?```{replacedAnswer}```";

                if (category.ToLower() == "characters")
                {
                    questionsFormat = $"Guess one of the ojamajo doremi characters name:```{replacedAnswer}```";
                }

                respondBot = await ReplyAsync($"{Context.User.Username}, \u23F1 You have **{timeoutDuration}** seconds each turn, with **{lives}** \u2764. " +
                    $"Type **exit** to exit from the games.\n" +
                    questionsFormat);

                var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(timeoutDuration));

                while (lives > 0 && response != null)
                {
                    Boolean isGuessed = false;
                    string loweredResponse = response.Content.ToLower();

                    await Context.Channel.DeleteMessageAsync(response.Id);

                    if (loweredResponse == "exit")
                    {
                        try
                        {
                            await Context.Channel.DeleteMessageAsync(respondBot.Id);
                            await Context.Channel.DeleteMessageAsync(response.Id);
                        }
                        catch { }
                        Config.Doremi.isRunningMinigame.Remove(Context.User.Id.ToString());
                        await ReplyAndDeleteAsync($"**{Context.User.Username}** has leave the hangman minigame.", timeout: autoDeleteTimespan);
                        return Ok();
                    }
                    else if (loweredResponse == randomedAnswer)
                    { //double the reward if guess correctly
                        scoreValue *= 2;
                        magicSeedsValue *= 2;
                        Config.Doremi.isRunningMinigame.Remove(Context.User.Id.ToString());

                        await ReplyAsync($"\uD83D\uDC4F Bing Bong! **{Context.User.Username}**, you guess the answer correctly: **{randomedAnswer}**. " +
                            $"You got {scoreValue} score & {magicSeedsValue} magic seeds.");

                        var guildId = Context.Guild.Id;
                        var userId = Context.User.Id;

                        //save the data
                        MinigameCore.updateScore(guildId, userId, scoreValue);

                        //save garden data
                        UserDataCore.updateMagicSeeds(userId, magicSeedsValue);

                        return Ok();
                    }
                    else if (loweredResponse.Length > 1)
                        await ReplyAndDeleteAsync($":x: Sorry **{Context.User.Username}**, you can only guess a word each turn.",
                            timeout: autoDeleteTimespan);
                    else if (loweredResponse == " ")
                        await ReplyAndDeleteAsync($":x: Sorry **{Context.User.Username}**, you can't enter a whitespace character.",
                            timeout: autoDeleteTimespan);
                    else if (loweredResponse.Length <= 1)
                    {

                        foreach (string x in guessedWord)
                        {
                            if (loweredResponse.Contains(x))
                            {
                                await ReplyAndDeleteAsync($":x: Sorry **{Context.User.Username}**, you already guessed **{x}**",
                                    timeout: autoDeleteTimespan);
                                isGuessed = true;
                                break;
                            }
                        }

                        guessedWord.Add(loweredResponse);

                        if (!tempRandomedAnswer.Contains(loweredResponse) && !isGuessed)
                        {
                            lives -= 1;
                            if (lives > 0)
                            {
                                try
                                {
                                    await Context.Channel.DeleteMessageAsync(respondBot.Id);
                                }
                                catch { }
                                Config.Doremi.isRunningMinigame.Remove(Context.User.Id.ToString());
                                respondBot = await ReplyAsync($"\u274C Sorry **{Context.User.Username}**, you guess it wrong. \u2764: **{lives}** . Category:**{key}**```{replacedAnswer}```");
                            }
                            else
                            {
                                lives = 0;
                                Config.Doremi.isRunningMinigame.Remove(Context.User.Id.ToString());
                                await ReplyAsync($"\u274C Sorry **{Context.User.Username}**, you're running out of guessing attempt. The correct answer is : **{randomedAnswer}**");
                                return Ok();
                            }

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
                            if (replacedAnswer.Contains("_"))
                            {
                                try
                                {
                                    await Context.Channel.DeleteMessageAsync(respondBot.Id);
                                }
                                catch { }
                                respondBot = await ReplyAsync($":white_check_mark: **{Context.User.Username}**. Category:**{key}**\n```{replacedAnswer}```");
                            }
                            else
                            {
                                Config.Doremi.isRunningMinigame.Remove(Context.User.Id.ToString());

                                await ReplyAsync($"\uD83D\uDC4F Congratulations **{Context.User.Username}**, you guess the correct answer: **{randomedAnswer}**. " +
                                    $"You got {scoreValue} score & {magicSeedsValue} magic seeds.");

                                var guildId = Context.Guild.Id;
                                var userId = Context.User.Id;

                                //save the data
                                MinigameCore.updateScore(guildId, userId, scoreValue);

                                //save garden data
                                UserDataCore.updateMagicSeeds(userId, magicSeedsValue);

                                return Ok();
                            }
                        }

                    }

                    response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(timeoutDuration));

                }

                lives = 0;
                Config.Doremi.isRunningMinigame.Remove(Context.User.Id.ToString());
                await ReplyAsync($"\u23F1 Time's up **{Context.User.Username}**. The correct answer is : **{randomedAnswer}**.");
                return Ok();

            }
            else
            {
                await ReplyAsync($"Sorry **{Context.User.Username}**, you're still running the **minigame** interactive commands, please finish it first.");
                return Ok();
            }


        }

        //[Command("dorequiz", RunMode = RunMode.Async), Summary("I will give you some quiz about Doremi.")]
        //public async Task Interact_Quiz()
        //{
        //    Random rnd = new Random();
        //    int rndQuiz = rnd.Next(0, 4);

        //    string question, replyCorrect, replyWrong, replyEmbed;
        //    List<string> answer = new List<string>();
        //    string replyTimeout = "Time's up. Sorry but it seems you haven't answered yet.";

        //    if (rndQuiz == 1)
        //    {
        //        question = "What is my favorite food?";
        //        answer.Add("steak");
        //        replyCorrect = "Ding Dong, correct! I love steak very much";
        //        replyWrong = "Sorry but that's wrong. Please retype the correct answer.";
        //        replyTimeout = "Time's up. My favorite food is steak.";
        //        replyEmbed = "https://66.media.tumblr.com/337aaf42d3fb0992c74f7f9e2a0bf4f6/tumblr_olqtewoJDS1r809wso1_500.png";
        //    }
        //    else if (rndQuiz == 2)
        //    {
        //        question = "Where do I attend my school?";
        //        answer.Add("misora elementary school"); answer.Add("misora elementary"); answer.Add("misora school");
        //        replyCorrect = "Ding Dong, correct!";
        //        replyWrong = "Sorry but that's wrong. Please retype the correct answer.";
        //        replyTimeout = "Time's up. I went to Misora Elementary School.";
        //        replyEmbed = "https://vignette.wikia.nocookie.net/ojamajowitchling/images/d/df/E.JPG";
        //    }
        //    else if (rndQuiz == 3)
        //    {
        //        question = "What is my full name?";
        //        answer.Add("doremi harukaze"); answer.Add("harukaze doremi");
        //        replyCorrect = "Ding Dong, correct! Doremi Harukaze is my full name.";
        //        replyWrong = "Sorry but that's wrong. Please retype the correct answer.";
        //        replyTimeout = "Time's up. Doremi Harukaze is my full name.";
        //        replyEmbed = "https://i.pinimg.com/originals/e7/1c/ce/e71cce7499e4ea9f9520c6143c9672e7.jpg";
        //    }
        //    else
        //    {
        //        question = "What is my sister name?";
        //        answer.Add("pop"); answer.Add("harukaze pop"); answer.Add("pop harukaze");
        //        replyCorrect = "Ding Dong, that's correct. Pop Harukaze is my sister name.";
        //        replyWrong = "Sorry, wrong answer. Please retype the correct answer.";
        //        replyTimeout = "Time's up. My sister name is Pop Harukaze.";
        //        replyEmbed = "https://images-wixmp-ed30a86b8c4ca887773594c2.wixmp.com/f/6e3bcaa4-2e3a-4390-a51a-652dff45c0b6/d6r5yu6-bffc8dba-af11-4af3-856c-d8ce82efaba3.png/v1/fill/w_333,h_250,q_70,strp/pop_harukaze_by_xdnobody_d6r5yu6-250t.jpg?token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1cm46YXBwOjdlMGQxODg5ODIyNjQzNzNhNWYwZDQxNWVhMGQyNmUwIiwiaXNzIjoidXJuOmFwcDo3ZTBkMTg4OTgyMjY0MzczYTVmMGQ0MTVlYTBkMjZlMCIsIm9iaiI6W1t7ImhlaWdodCI6Ijw9MzAwIiwicGF0aCI6IlwvZlwvNmUzYmNhYTQtMmUzYS00MzkwLWE1MWEtNjUyZGZmNDVjMGI2XC9kNnI1eXU2LWJmZmM4ZGJhLWFmMTEtNGFmMy04NTZjLWQ4Y2U4MmVmYWJhMy5wbmciLCJ3aWR0aCI6Ijw9NDAwIn1dXSwiYXVkIjpbInVybjpzZXJ2aWNlOmltYWdlLm9wZXJhdGlvbnMiXX0.ZOzOlhlXguuSwk-EKwPjNIWywfRYeWRWKLOBQK4i5HY";
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
        //            int scoreValue = 20;
        //            var guildId = Context.Guild.Id;
        //            var userId = Context.User.Id;

        //            //save the data
        //            MinigameCore.updateScore(guildId.ToString(), userId.ToString(), scoreValue);

        //            replyCorrect += $". Your **score+{scoreValue}**";
        //            await ReplyAsync(replyCorrect, embed: new EmbedBuilder()
        //            .WithColor(Config.Doremi.EmbedColor)
        //            .WithImageUrl(replyEmbed)
        //            .Build());

        //            correctAnswer = true;
        //        }
        //        else
        //        {
        //            await ReplyAsync(replyWrong);
        //        }
        //    }
        //}

        [Command("numbers", RunMode = RunMode.Async), Alias("number", "dice"), Summary("Guess if the number is lower/higher than the one I give.")]
        public async Task Interact_Minigame_Guess_Numbers()
        {
            var guildId = Context.Guild.Id;
            var userId = Context.User.Id;

            int scoreValue = 20;
            int timeoutDuration = 15;
            int randomNumbers = new Random().Next(6, 11);
            if (!Config.Doremi.isRunningMinigame.ContainsKey(Context.User.Id.ToString()))
                Config.Doremi.isRunningMinigame.Add(Context.User.Id.ToString(), false);

            if (!Config.Doremi.isRunningMinigame[Context.User.Id.ToString()])
            {
                Boolean isPlaying = true;
                await ReplyAsync($"{Context.User.Username}, \uD83C\uDFB2 Number **{randomNumbers}** out of **12** has been selected.\n" +
                    $"\u23F1 You have **{timeoutDuration}** seconds to guess if the next number will be **lower** or **higher** or **same**. " +
                    $"Type **exit** to exit from the minigame.");

                var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(timeoutDuration));

                while (isPlaying && response != null)
                {
                    string loweredResponse = response.Content.ToLower();

                    if (loweredResponse == "exit")
                    {
                        Config.Doremi.isRunningMinigame.Remove(Context.User.Id.ToString());
                        await ReplyAsync($"**{Context.User.Username}** has left the numbers minigame.");
                        return;
                    }
                    else if (loweredResponse == "same" || loweredResponse == "lower" || loweredResponse == "higher")
                    {
                        int nextRandomNumbers = new Random().Next(1, 13);
                        string responseResult = "";
                        Boolean isCorrect = true;//default

                        if (randomNumbers == nextRandomNumbers && loweredResponse == "same")
                        {
                            responseResult = $"\uD83D\uDC4F Congratulations, you guess it **correct**. You got **{scoreValue}** score points & 1 magic seeds.";
                            UserDataCore.updateMagicSeeds(userId, 1);
                        }
                        else if (nextRandomNumbers < randomNumbers && loweredResponse == "lower")
                        {
                            responseResult = $"\uD83D\uDC4F Congratulations, you guess it **correct**. You got **{scoreValue}** score points & 1 magic seeds.";
                            UserDataCore.updateMagicSeeds(userId, 1);
                        }
                        else if (nextRandomNumbers > randomNumbers && loweredResponse == "higher")
                        {
                            responseResult = $"\uD83D\uDC4F Congratulations, you guess it **correct**. You got **{scoreValue}** score points & 1 magic seeds.";
                            UserDataCore.updateMagicSeeds(userId, 1);
                        }
                        else
                        {
                            responseResult = "\u274C Sorry, you guess it **wrong**.";
                            isCorrect = false;
                        }
                        await ReplyAsync($"\uD83C\uDFB2 First number was:**{randomNumbers}**, the next selected number was: **{nextRandomNumbers}** and you guess it: **{loweredResponse}**.\n{responseResult}");

                        if (isCorrect)
                        {
                            //save the data
                            MinigameCore.updateScore(guildId, userId, scoreValue);
                        }
                        return;
                    }
                    else if (loweredResponse != "same" || loweredResponse != "lower" || loweredResponse != "higher")
                    {
                        await ReplyAsync("Sorry, please answer it with **lower** or **higher** or **same**.");
                        response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(timeoutDuration));
                    }
                }

                Config.Doremi.isRunningMinigame.Remove(Context.User.Id.ToString());
                await ReplyAsync($"\u23F1 Time's up, **{Context.User.Username}**.");
                return;
            }
            else
                await ReplyAsync($"Sorry **{Context.User.Username}**, you're still running the **minigame** interactive commands, please finish it first.");
        }

    }

}
