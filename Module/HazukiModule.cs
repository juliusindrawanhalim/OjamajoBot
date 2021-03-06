﻿using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using OjamajoBot.Database.Model;
using Spectacles.NET.Types;
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
    class HazukiModule : ModuleBase<SocketCommandContext>
    {

        //start
        private readonly CommandService _commands;
        private readonly IServiceProvider _map;

        public HazukiModule(IServiceProvider map, CommandService commands)
        {
            _commands = commands;
            _map = map;
        }

        [Name("help"), Command("help"), Summary("Show all Hazuki bot Commands.")]
        public async Task Help([Remainder]string CategoryOrCommands = "")
        {
            EmbedBuilder output = new EmbedBuilder();
            output.Color = Config.Hazuki.EmbedColor;

            if (CategoryOrCommands == "")
            {
                output.WithAuthor(Config.Hazuki.EmbedName, Config.Hazuki.EmbedAvatarUrl);
                output.Title = $"Command List";
                output.Description = "Pretty Witchy Hazuki Chi~ You can tell me what to do with " +
                    $"**{Config.Hazuki.PrefixParent[2]} or {Config.Hazuki.PrefixParent[0]} or {Config.Hazuki.PrefixParent[1]}** as starting prefix.\n" +
                    $"Use **{Config.Hazuki.PrefixParent[0]}help <category or commands>** for more help details.\n" +
                    $"Example: **{Config.Hazuki.PrefixParent[0]}help general** or **{Config.Hazuki.PrefixParent[0]}help hello**";

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
                        await ReplyAsync($"Sorry, I can't find any related help that you search for. " +
                            $"See `{Config.Hazuki.PrefixParent[0]}help <commands or category>` for command help.");
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
            completedText += $"**Example:** `{Config.Hazuki.PrefixParent[0]}{group}{commands}";
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
                output = Config.Hazuki.PrefixParent[0];
            }
            output = Config.Hazuki.PrefixParent[0];
            return output;
        }

        //end

        //[Command]
        //public async Task defaultMention()
        //{
        //    string tempReply = "";
        //    List<string> listRandomRespond = new List<string>() {
        //        $"Hello there {MentionUtils.MentionUser(Context.User.Id)}. ",
        //        $"Hello, {MentionUtils.MentionUser(Context.User.Id)}. "
        //    };

        //    int rndIndex = new Random().Next(0, listRandomRespond.Count);
        //    tempReply = $"{listRandomRespond[rndIndex]}I noticed that you're calling for me. Use {Config.Hazuki.PrefixParent}help <commands or category> if you need help with the commands.";
        //    await ReplyAsync(tempReply);
        //}

        [Command("change"), Alias("henshin"), Summary("I will change into the ojamajo form. " +
            "Fill <form> with: **default/sharp/royal/motto/dokkan** to make it spesific form.")]
        public async Task transform(string form = "dokkan")
        {
            IDictionary<string, string> arrImage = new Dictionary<string, string>();
            arrImage["default"] = "https://cdn.discordapp.com/attachments/706812058175406210/706813870827765811/default.gif";
            arrImage["sharp"] = "https://cdn.discordapp.com/attachments/706812058175406210/706814005921972314/sharp.gif";
            arrImage["royal"] = "https://cdn.discordapp.com/attachments/706812058175406210/706814304296632380/royal.gif";
            arrImage["motto"] = "https://cdn.discordapp.com/attachments/706812058175406210/706814145055424562/motto.gif";
            arrImage["dokkan"] = "https://cdn.discordapp.com/attachments/706812058175406210/706814281701916712/dokkan.gif";

            if (arrImage.ContainsKey(form))
            {
                await ReplyAsync("Pretty Witchy Hazuki Chi~",
                embed: new EmbedBuilder()
                .WithColor(Config.Hazuki.EmbedColor)
                .WithImageUrl(arrImage[form])
                .Build());
            }
            else
            {
                await ReplyAsync("I'm sorry, but I can't found that form.");
            }


        }

        [Command("dabzuki"), Alias("dab"), Summary("Do the dab with me")]
        public async Task dabzuki()
        {
            string[] arrRandom ={
                $":sunglasses: Keep calm and dab on {Config.Emoji.dabzuki}",
                $":sunglasses: **D**esire **A**spire **B**elieve **I**nspire  **T**ake fire {Config.Emoji.dabzuki}",
                $":sunglasses: Just Dab and Let it go! {Config.Emoji.dabzuki}",
                $":sunglasses: No Bad Vibes {Config.Emoji.dabzuki}",
                $":sunglasses: Dab checked \u2705 {Config.Emoji.dabzuki}",
                $":sunglasses: Let's do the dab with me, everyone! {Config.Emoji.dabzuki}",
                $":sunglasses: Please dab with me, {MentionUtils.MentionUser(Context.User.Id)} {Config.Emoji.dabzuki}",
                $"Don't tell me to do the dab, {MentionUtils.MentionUser(Context.User.Id)} {Config.Emoji.dabzuki}",
                $":regional_indicator_d:ab, dab and dab {Config.Emoji.dabzuki}",
                $":regional_indicator_d::regional_indicator_a::regional_indicator_b::regional_indicator_z::regional_indicator_u::regional_indicator_k::regional_indicator_i: in action! {Config.Emoji.dabzuki}"
            };

            await ReplyAsync(arrRandom[new Random().Next(0, arrRandom.GetLength(0))],
                    embed: new EmbedBuilder()
                    .WithColor(Config.Hazuki.EmbedColor)
                    .WithImageUrl("https://cdn.discordapp.com/attachments/706770454697738300/790615761152507994/Dabzuki.png")
                    .Build());

        }

        [Command("fairy"), Summary("I will show you my fairy info")]
        public async Task showFairy()
        {
            await ReplyAsync("This is my fairy: Rere.",
            embed: new EmbedBuilder()
            .WithAuthor("Rere")
            .WithDescription("Rere has fair skin with warm brown eyes and blushed cheeks. Her pale orange hair is shaped into four-points, reminiscent of a bow, and she has two tufts for bangs. " +
            "Like Hazuki she wears glasses, along with a pale orange dress that has a cream collar. In teen form, her hair points now stick out at each part of her head and she gains a full body. She wears a pale orange dress with the shoulder cut out and a white-collar. A pastel orange top is worn under this, and at the chest is an orange gem. She also wears white booties and a white witch hat with a cream rim.")
            .WithColor(Config.Hazuki.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/d/dd/No.077.jpg")
            .WithFooter("[Ojamajo Witchling Wiki](https://ojamajowitchling.fandom.com/wiki/Rere)")
            .Build());
        }

        [Command("happy birthday"), Summary("Give Hazuki some wonderful birthday wishes. This commands only available on her birthday.")]
        public async Task hazukiBirthday([Remainder]string wishes = "")
        {
            string[] arrResponse = new string[] { $":blush: Thank you for your wonderful birthday wishes, {Context.User.Mention}.",
                $":blush: Thank you {Context.User.Mention}, for the wonderful birthday wishes."};
            string[] arrResponseImg = new string[]{
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/f/fe/15.03.JPG",
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/85/OD-EP10-35.png"
            };

            if (DateTime.Now.ToString("dd") == Config.Hazuki.birthdayDate.ToString("dd") &&
                DateTime.Now.ToString("MM") == Config.Hazuki.birthdayDate.ToString("MM") &&
                Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour)
            {
                await ReplyAsync(arrResponse[new Random().Next(0, arrResponse.Length)],
                embed: new EmbedBuilder()
                .WithColor(Config.Hazuki.EmbedColor)
                .WithImageUrl(arrResponseImg[new Random().Next(0, arrResponseImg.Length)])
                .Build());
            }
            else
            {
                await ReplyAsync("I'm sorry, but it's not my birthday yet.",
                embed: new EmbedBuilder()
                .WithColor(Config.Hazuki.EmbedColor)
                .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/2/20/OD-EP16-05.png")
                .Build());
            }
        }

        [Command("hello"), Summary("Hello, I will greet you up")]
        public async Task hazukiHello()
        {
            List<string> listRandomRespond = new List<string>() {
                $"Hello there {MentionUtils.MentionUser(Context.User.Id)}. ",
                $"Hello, {MentionUtils.MentionUser(Context.User.Id)}. ",
            };

            int rndIndex = new Random().Next(0, listRandomRespond.Count);
            string tempReply = listRandomRespond[rndIndex] + Config.Hazuki.Status.currentActivityReply;

            await ReplyAsync(embed: new EmbedBuilder()
                .WithDescription(tempReply)
                .WithColor(Config.Hazuki.EmbedColor)
                .WithImageUrl("https://cdn.discordapp.com/attachments/706770454697738300/790613685885468692/d6581ed77ec875f739b61f13b5d597c48968ab08_00.png")
                .Build());
        }

        [Command("hug"), Summary("Give a hug for <username>")]
        public async Task HugUser([Remainder]string username)
        {
            List<string> listRandomImage = new List<string>() {
                "https://media.tenor.com/images/f51ff7041983283592e13e3e0c3b29b9/tenor.gif"
            };

            int rndIndex = new Random().Next(0, listRandomImage.Count);

            await ReplyAsync(embed: new EmbedBuilder()
                .WithDescription($"{MentionUtils.MentionUser(Context.User.Id)} has give a nice & friendly hug for {username}")
                .WithColor(Config.Hazuki.EmbedColor)
                .WithImageUrl(listRandomImage[rndIndex])
                .Build());
        }

        [Command("random"), Alias("moments"), Summary("Show any random Hazuki moments. " +
            "Fill <moments> with **random/first/sharp/motto/naisho/dokkan** for spesific moments.")]
        public async Task randomthing(string moments = "")
        {
            string finalUrl = ""; string footerUrl = "";
            JArray getDataObject = null; moments = moments.ToLower();
            if (moments == "")
            {
                int randomType = new Random().Next(0, 5);

                if(randomType != 3){
                    int randomMix = new Random().Next(0, 2); string path;
                    if (randomMix == 0)
                        path = "config/randomMoments/hazuki";
                    else
                        path = "config/randomMoments/hazuki/mix";

                    string randomPathFile = GlobalFunctions.getRandomFile(path, new string[] { ".png", ".jpg", ".gif", ".webm" });
                    await Context.Channel.SendFileAsync($"{randomPathFile}");
                    return;
                } else {
                    //select specific from wiki
                    var key = Config.Hazuki.jObjRandomMoments.Properties().ToList();
                    var randIndex = new Random().Next(0, key.Count);
                    moments = key[randIndex].Name;
                    getDataObject = (JArray)Config.Hazuki.jObjRandomMoments[moments];
                    finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
                }
            }
            else
            {
                if (Config.Hazuki.jObjRandomMoments.ContainsKey(moments))
                {
                    getDataObject = (JArray)Config.Hazuki.jObjRandomMoments[moments];
                    finalUrl = getDataObject[new Random().Next(0, getDataObject.Count)].ToString();
                }
                else
                {
                    await ReplyAsync($"Oops, I can't found the specified moments. " +
                        $"See `{Config.Hazuki.PrefixParent[0]}help random` for commands help.");
                    return;
                }
            }

            footerUrl = finalUrl;
            if (finalUrl.Contains("wikia")) footerUrl = "https://ojamajowitchling.fandom.com/";

            await ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Hazuki.EmbedColor)
            .WithImageUrl(finalUrl)
            .WithFooter(footerUrl)
            .Build());
            
        }

        [Command("stats"), Alias("bio"), Summary("I will show you my biography info")]
        public async Task showStats()
        {
            await ReplyAsync("Paipai Ponpoi Puwapuwa Puu! Show my biography info!",
            embed: new EmbedBuilder()
            .WithAuthor("Hazuki Fujiwara", Config.Hazuki.EmbedAvatarUrl)
            .WithDescription("Hazuki Fujiwara (藤原はづき, Fujiwara Hazuki) is one of the main characters in Ojamajo Doremi. " +
            "She has been Doremi Harukaze's friend since childhood and became an apprentice witch after Doremi, along with Aiko Senoo in order to help keep the secret.")
            .AddField("Full Name", "藤原 はづき Fujiwara Hazuki", true)
            .AddField("Gender", "female", true)
            .AddField("Blood Type", "A", true)
            .AddField("Birthday", "February 14th, 1991", true)
            .AddField("Instrument", "Violin", true)
            .AddField("Favorite Food", "Chiffon Cake", true)
            .AddField("Debut", "[I'm Doremi! Becoming a Witch Apprentice!](https://ojamajowitchling.fandom.com/wiki/I%27m_Doremi!_Becoming_a_Witch_Apprentice!)", true)
            .WithColor(Config.Hazuki.EmbedColor)
            .WithImageUrl("https://static.wikia.nocookie.net/ojamajowitchling/images/4/47/O.D_LFMD%27_Hazuki_Fujiwara.png")
            .WithFooter("Source: [Ojamajo Doremi Wiki](https://ojamajowitchling.fandom.com/wiki/Hazuki_Fujiwara)")
            .Build());
        }

        [Command("thank you"), Alias("thanks", "arigatou"), Summary("Say thank you to Hazuki Bot")]
        public async Task thankYou([Remainder] string query = "")
        {
            await ReplyAsync($"Your welcome, {MentionUtils.MentionUser(Context.User.Id)}. I'm glad that you're happy with it :smile:");
        }

        [Command("wheezuki"), Alias("laughzuki"), Summary("\uD83C\uDF2C I will give you some random woosh jokes \uD83E\uDD76 \n" +
            "Include the `unfunny` or `woosh` parameter on <jokes_type> to make the jokes very bad.")]
        public async Task randomcoldjokes(string jokes_type = "sos")
        {
            if (jokes_type == "sos")
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://official-joke-api.appspot.com/jokes/general/random");
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string jsonResp = reader.ReadToEnd().ToString();
                    JArray jarray = JArray.Parse(jsonResp);

                    var setup = jarray[0]["setup"];
                    var punchline = jarray[0]["punchline"];

                    await ReplyAsync(embed: new EmbedBuilder()
                    .WithAuthor("SOS Trio", "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcTWj56dpMHiFcKv0Gz_cBQPZTZRNZoaUskA_OuamYo8pTy4CaoJ")
                    .WithColor(Config.Hazuki.EmbedColor)
                    .WithDescription($"{setup}\n{punchline}")
                    .WithImageUrl("https://cdn.discordapp.com/attachments/706770454697738300/790606330217496626/unknown.png")
                    .WithFooter("Art by: Letter Three")
                    .Build());

                }
                catch (Exception e)
                {
                    Console.Write(e.ToString());
                }


            }
            else if (jokes_type.ToLower() == "unfunny" || jokes_type.ToLower() == "woosh")
            {
                string[] arrRandom =
                {
                    "https://i.pinimg.com/originals/65/39/3a/65393a36c2e67d0b63d377025337b81a.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcS0tDvPfOU7in7_1Ky5itHTCqn829Oao0qRj1d1IPSKQFekGflV",
                    "https://i.pinimg.com/originals/bc/7c/d0/bc7cd03a6ecfbc855b19013d273bcd0e.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcTY7rMoVM0ESvZbaIOfRiu4WscgtTLA_MUgxFCtf5RZvleGy7bN",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcRF8mURS1BA9VqzWc_yDNMXBipLziCp5N7yoe2m2a4dwkYXGUXB",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcSj8LBX7zfutMwpH5dZZ5ohY_gUf39IjNmvQeLIa0AXhRcRv_ed",
                    "http://1.bp.blogspot.com/-eME4OlMZ8wU/Uzh5F9sn7NI/AAAAAAAAB5Y/Nl1Jfdk625k/s1600/sales+c+2.jpg",
                    "https://www.jokejive.com/images/jokejive/7d/7d2130c5106a9e94fb37969f1b63853d.jpeg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcTjjZbeXkQF25_zcK9lsL3CARltyNqG9VHrLzWSTLfFORAs00Zf",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcQqEjXgLaMtxRI0pxmurfV4FMFc2KNB7wEGu6NwUur73pPNxLgR",
                    "https://i0.wp.com/silverleafwriters.com/wp-content/uploads/2018/02/Bear-trip.jpg?fit=675%2C332&ssl=1",
                    "http://2.bp.blogspot.com/-HbBPgXsN9tc/Ts0ne1MA2AI/AAAAAAAAA_I/zwfWScH2bV0/s1600/Bear-Hiking-Pack.jpg",
                    "https://i.pinimg.com/originals/fd/7e/23/fd7e231a12350d4cad043660bbb8b48f.jpg",
                    "https://danielamurphydotcom.files.wordpress.com/2013/06/hate.jpg?w=374&h=357",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcQqEjXgLaMtxRI0pxmurfV4FMFc2KNB7wEGu6NwUur73pPNxLgR",
                    "https://feathertale.com/wp-content/uploads/2014/10/07.19.15_Pedersen.gif",
                    "http://content.invisioncic.com/r266882/monthly_2019_02/knock.jpg.771cc56efd82396a87f4f632343d0dbd.jpg"
                };
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithAuthor("SOS Trio", "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcTWj56dpMHiFcKv0Gz_cBQPZTZRNZoaUskA_OuamYo8pTy4CaoJ")
                    .WithColor(Config.Hazuki.EmbedColor)
                    .WithImageUrl(arrRandom[new Random().Next(0, arrRandom.GetLength(0))])
                    .Build());
            }
            else
            {
                await ReplyAsync($"Sorry, I can't seems to understand your `wheezuki` jokes type. See `{Config.Hazuki.PrefixParent[0]}help wheezuki` for more info.");
            }

            //http://api.icndb.com/jokes/random?firstName=John&amp;lastName=Doe
        }

        [Command("wish"), Summary("I will grant you a <wishes>")]
        public async Task Wish([Remainder] string wishes)
        {
            await base.ReplyAsync($"Paipai Ponpoi Puwapuwa Puu! {wishes}",
            embed: new EmbedBuilder()
            .WithColor(Config.Hazuki.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/b/bc/Hazu-spell.gif")
            .Build());
        }


        [Command("giveaway"), Summary("Host a giveaway. Can only be executed by the bot owner.")]
        public async Task giveawayAssignment()
        {
            if (Context.User.Id != 145584315839938561)
            {
                await ReplyAsync($"Sorry, this command can only executed by someone who create me.");
                return;
            }
            else
            {
                var everyoneRole = Context.Guild.EveryoneRole.Mention;
                await Context.Message.DeleteAsync();
                var eb = new EmbedBuilder()
                    .WithColor(Config.Hazuki.EmbedColor)
                    .WithThumbnailUrl("https://cdn.discordapp.com/attachments/706770454697738300/728659482913144872/hazuki_happy5.jpg")
                    .WithTitle($"🎁 {Context.User.Username} has start a giveway event!")
                    .WithDescription("Pretty witchy Hazuki is here~\n" +
                    "Allow me to make a brief announcement that our friends: " +
                    $"{MentionUtils.MentionUser(Context.User.Id)} are hosting " +
                    $"**1 month discord classic** giveaway events for **5 lucky people**, starting from July 19'th until July 24'th (server time)!\n" +
                    $"You can see the terms and condition written below if you wish to participate the giveaway.")
                    .AddField("Terms and condition:",
                    "-You can participate the event by simply click the 🎁 reaction that has been provided by me.\n" +
                    "-You cannot participate the giveaway anymore if: " +
                    "**you already win the giveaway**, **the giveaway codes are running out**, **the giveaway has come to an end**.\n" +
                    "-Winner giveaway results will be announced on the group. " +
                    "Should you **win** the giveaway event, you'll be given the nitro code and " +
                    "you're not allowed to participate the giveaway anymore.\n" +
                    "-Didn't win the giveaway yet? " +
                    "Don't worry, you can still participate again on the next day as long the giveaway codes are still available and the event are still running.\n" +
                    $"-If the codes are not working, you can try to ask {MentionUtils.MentionUser(Context.User.Id)}.\n" +
                    $"-If there's still some code available and the giveaway has ended, " +
                    $"{MentionUtils.MentionUser(Context.User.Id)} will host and announce a last run giveaway on the group.");
                var sentMessage = await ReplyAsync(embed:eb.Build());
                await sentMessage.AddReactionAsync(new Discord.Emoji("\uD83C\uDF81")); //💎 discord nitro classic 1 month
            }
        }

    }


        //RANDOM OLD JOKES:
        /*string[] arrRandom =
                    {
                        "Q. Why was King Arthur’s army too tired to fight?\nA. It had too many sleepless knights.",
                        "Q. Which country’s capital has the fastest-growing population?\nA. Ireland. Every day it’s Dublin.",
                        "I asked my French friend if she likes to play video games. She said, 'Wii.'",
                        "Yesterday, a clown held the door open for me. It was such a nice jester!",
                        "The machine at the coin factory just suddenly stopped working, with no explanation. It doesn’t make any cents!",
                        "I was going to make myself a belt made out of watches, but then I realized it would be a waist of time.",
                        "Did you hear about the auto body shop that just opened? It comes highly wreck-a-mended.",
                        "Q. What’s the difference between a hippo and a Zippo?\nA. A hippo is really heavy, and a Zippo is a little lighter.",
                        "All these sea monster jokes are just Kraken me up.",
                        "Q. Why can’t you run through a campground?\nA. You can only ran, because it’s past tents.",
                        "Shout out to the people who ask what the opposite of “in” is.",
                        "I’m only friends with 25 letters of the alphabet. I don’t know Y.",
                        "Q. What sound does a sleeping T-Rex make?\nA. A dino-snore.",
                        "Q. Why can’t Harry Potter tell the difference between the pot he uses to make potions and his best friend?\n" +
                        "A. They’re both cauld ron.",
                        "Two windmills are standing in a wind farm. One asks, “What’s your favorite kind of music?” The other says, “I’m a big metal fan.”",
                        "Want to hear something terrible? Paper.",
                        "Last night, I dreamed I was swimming in an ocean of orange soda. But it was just a Fanta sea.",
                        "My boss yelled at me the other day, “You’ve got to be the worst train driver in history. How many trains did you derail last year?” I said, “Can’t say…",
                        "A man sued an airline company after it lost his luggage. Sadly, he lost his case.",
                        "Atoms are untrustworthy little critters. They make up everything!",
                        "The past, the present, and the future walk into a bar…\nIt was tense.",
                        "An atom loses an electron… it says, “Man, I really gotta keep an ion them.”",
                        "Did you hear about the man who was accidentally buried alive?  It was a grave mistake.",
                        "I had to clean out my spice rack and found everything was too old and had to be thrown out.  What a waste of thyme.",
                        "6:30 is the best time on a clock… hands down.",
                        "I hate how funerals are always at 9 a.m.  I’m not really a mourning person.",
                        "I lost my job at the bank on my very first day.  A woman asked me to check her balance, so I pushed her over.",
                        "Ray’s friends claim he’s a baseball nut. He says they’re way off base.",
                        "The public safety officer came up to a large mob of people outside a department store and asked, “What’s happening?” A mall officer replied, “These people are waiting to get…",
                        "Why not go out on a limb? Isn’t that where all the fruit is?",
                        "My ex used to hit me with stringed instruments. If only I had known about her history of violins.",
                        "Did you hear about the 2 silk worms in a race? It ended in a tie!",
                        "Someone stole my toilet and the police have nothing to go on.",
                        "Last time I got caught stealing a calendar I got 12 months.",
                        "What do you call a laughing motorcycle? A Yamahahaha.",
                        "A friend of mine tried to annoy me with bird puns, but I soon realized that toucan play at that game.",
                        "Did you hear about the guy who got hit in the head with a can of soda? He was lucky it was a soft drink.",
                        "I wasn’t originally going to get a brain transplant, but then I changed my mind.",
                        "I can’t believe I got fired from the calendar factory. All I did was take a day off.",
                        "A termite walks into a bar and says, “Where is the bar tender?”",
                        "I saw an ad for burial plots, and thought to myself this is the last thing I need.",
                        "What’s the difference between a poorly dressed man on a bicycle and a nicely dressed man on a tricycle? A tire.",
                        "What do you call a fish with no eyes? A fsh.",
                        "What do you call a can opener that doesn’t work? A can’t opener!",
                        "What do you get when you combine a rhetorical question and a joke?\n…\nGet it? Bad jokes don’t even need a punch line to be funny!",
                        "Did you hear about the Italian chef who died? He pasta-way.",
                        "Two muffins were sitting in an oven. One turned to the other and said, “Wow, it’s pretty hot in here.” The other one shouted, “Wow, a talking muffin!”",
                        "I sold my vacuum the other day. All it was doing was collecting dust."
                    };
                    */

    [Summary("hidden")]
    public class HazukiMagicalStageModule : ModuleBase
    {
        //magical stage section
        [Command("Pirika pirilala, Nobiyaka ni!")] //magical stage from doremi
        public async Task magicalStage()
        {
            if (Context.User.Id == Config.Doremi.Id)
            {
                await ReplyAsync($"{MentionUtils.MentionUser(Config.Aiko.Id)} Paipai Ponpoi, Shinyaka ni! \n",
                    embed: new EmbedBuilder()
                    .WithColor(Config.Hazuki.EmbedColor)
                    .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/9/91/MagicalStageMottoHazuki.png")
                    .Build());
            }
        }

        [Command("Magical Stage!")]//Final magical stage: from doremi
        public async Task magicalStagefinal([Remainder] string query)
        {
            if (Context.User.Id == Config.Doremi.Id)
                await ReplyAsync($"{MentionUtils.MentionUser(Config.Aiko.Id)} Magical Stage! {query}\n");
        }
    }

    [Summary("hidden")]
    class HazukiRandomEventModule : ModuleBase<SocketCommandContext>
    {
        List<string> listRespondDefault = new List<string>() {
            $":pensive: I'm afraid I can't right now {MentionUtils.MentionUser(Config.Doremi.Id)}-chan, I have violin lesson to attend",
            $":pensive: I'm sorry {MentionUtils.MentionUser(Config.Doremi.Id)}-chan, I have ballet lesson to attend"
        };

        [Remarks("go to the shop event")]
        [Command("let's go to maho dou")]
        public async Task eventmahoudou()
        {
            if (Context.User.Id == Config.Doremi.Id)
            {
                List<string> listRespond = new List<string>() { $":smile: Sure thing {MentionUtils.MentionUser(Config.Doremi.Id)}-chan, let's go to the shop." };

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
                List<string> listRespond = new List<string>() { $":smile: Sure {MentionUtils.MentionUser(Config.Doremi.Id)}-chan, let's go to your house." };

                for (int i = 0; i < listRespondDefault.Count - 1; i++)
                    listRespond.Add(listRespondDefault[i]);

                Random rnd = new Random();
                int rndIndex = rnd.Next(0, listRespond.Count); //random the list value
                await ReplyAsync($"{listRespond[rndIndex]}");
            }
        }

    }

    [Name("minigame"), Group("minigame"), Summary("This category contains all Hazuki minigame interactive commands.")]
    public class HazukiMinigameInteractive : InteractiveBase
    {
        [Command("score"), Summary("Show your minigame score points.")]
        public async Task Show_Minigame_Score()
        {
            await ReplyAsync(embed: MinigameCore.printScore(Context, Config.Hazuki.EmbedColor).Build());
        }

        [Command("leaderboard"), Summary("Show the top 10 player with the highest score points.")]
        public async Task Show_Minigame_Leaderboard()
        {//show top 10 player score
            await ReplyAsync(embed: MinigameCore.printLeaderboard(Context, Config.Hazuki.EmbedColor).Build());
        }

        [Command("jankenpon", RunMode = RunMode.Async), Alias("rps", "rockpaperscissors"), Summary("Play the Rock Paper Scissors minigame with Hazuki. " +
            "Reward: 20 minigame score points & 1 magic seeds.")]
        public async Task RockPaperScissors(string guess = "")
        {
            if (guess == "")
            {
                await ReplyAsync($"Please enter the valid parameter: **rock** or **paper** or **scissors**");
                return;
            }
            else if (guess.ToLower() != "rock" && guess.ToLower() != "paper" && guess.ToLower() != "scissors")
            {
                await ReplyAsync($"Sorry **{Context.User.Username}**. " +
                    $"Please enter the valid parameter: **rock** or **paper** or **scissors**");
                return;
            }

            guess = guess.ToLower();//lower the text
            int randomGuess = new Random().Next(0, 3);//generate random

            string[] arrWinReaction = { $"I'm sorry {Context.User.Username}, but I win the game this time." };//bot win
            string[] arrLoseReaction = { "Oh no, I loss from the game." };//bot lose
            string[] arrDrawReaction = { "We both landed a draw." };//bot draw

            Tuple<string, EmbedBuilder,Boolean> result = MinigameCore.rockPaperScissors.rpsResults(Config.Hazuki.EmbedColor, Config.Hazuki.EmbedAvatarUrl, randomGuess, guess, "hazuki", Context.User.Username,
                arrWinReaction, arrLoseReaction, arrDrawReaction,
                Context.Guild.Id, Context.User.Id);

            await Context.Channel.SendFileAsync(result.Item1, embed: result.Item2.Build());
        }

    }

    [Name("Card"), Group("card"), Summary("This category contains all Hazuki Trading card command.")]
    public class HazukiTradingCardInteractive : InteractiveBase
    {
        [Command("capture", RunMode = RunMode.Async), Alias("catch"), Summary("Capture spawned card with Hazuki.")]
        public async Task<RuntimeResult> trading_card_hazuki_capture(string boost = "")
        {
            //reference: https://www.newtonsoft.com/json/help/html/ModifyJson.htm
            var guildId = Context.Guild.Id;
            var clientId = Context.User.Id;

            var guildSpawnData = TradingCardGuildCore.getGuildData(guildId);
            if (Convert.ToInt32(guildSpawnData[DBM_Trading_Card_Guild.Columns.spawn_is_zone]) == 1)
            {
                var userTradingCardData = UserTradingCardDataCore.getUserData(clientId);
                string userCardZone = userTradingCardData[DBM_User_Trading_Card_Data.Columns.card_zone].ToString();
                if (!userCardZone.Contains("hazuki"))
                {
                    await ReplyAsync(":x: Sorry, you are not on the correct card zone. " +
                        $"Please assign yourself on the correct card zone with **{Config.Hazuki.PrefixParent[0]}card zone set <category>** command.");
                    return Ok();
                }
            }

            //var cardCaptureReturn = TradingCardCore.cardCapture(Config.Hazuki.EmbedColor, Context.Client.CurrentUser.GetAvatarUrl(), guildId, clientId, Context.User.Username,
            //TradingCardCore.Hazuki.emojiError, "hazuki", boost, Config.Hazuki.PrefixParent[0], "ha",
            //TradingCardCore.Hazuki.maxNormal, TradingCardCore.Hazuki.maxPlatinum, TradingCardCore.Hazuki.maxMetal, TradingCardCore.Hazuki.maxOjamajos);

            var cardCaptureReturn = TradingCardCore.cardCapture(Context, Config.Hazuki.EmbedColor,
                TradingCardCore.Hazuki.emojiError, "hazuki", boost, "ha");

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
                    .WithColor(Config.Hazuki.EmbedColor)
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
                    .SendFileAsync(TradingCardCore.Onpu.imgCompleteAllCard, null, embed: TradingCardCore
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
                    .SendFileAsync(TradingCardCore.Momoko.imgCompleteAllCard, null, embed: TradingCardCore
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

                await Bot.Hazuki.client
                .GetGuild(Context.Guild.Id)
                .GetTextChannel(Context.Channel.Id)
                .SendFileAsync(TradingCardCore.imgCompleteAllCardSpecial, null, embed: TradingCardCore
                .userCompleteTheirList(Context, Config.Hazuki.EmbedColor, Config.Hazuki.EmbedAvatarUrl, "other",
                TradingCardCore.imgCompleteAllCardSpecial, TradingCardCore.roleCompletionistSpecial)
                .Build());

            }

            return Ok();
        }

        [Command("pureleine", RunMode = RunMode.Async), Alias("oyajide"), Summary("Detect the bad card with the help from oyajide & pureleine computer. " +
            "Insert the answer as parameter to remove the bad cards if it's existed. Example: ha!card pureleine 10")]
        public async Task trading_card_pureleine(string answer = "")
        {
            await ReplyAsync(embed: TradingCardCore.activatePureleine(Context, answer).Build());
        }

        [Command("status", RunMode = RunMode.Async), Summary("Show your Trading Card Progression/Status. " +
            "You can put the mentioned username to see the card status of that user.")]
        public async Task trading_card_status([Remainder]SocketGuildUser otherUser = null)
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
                        printStatusTemplate(Context, Config.Hazuki.EmbedColor, otherUser)
                        .Build());
        }

        [Command("status complete", RunMode = RunMode.Async), Summary("Show your trading card completion date status. " +
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
                        printStatusComplete(Context, Config.Hazuki.EmbedColor, otherUser)
                        .Build());
        }

        [Command("inventory", RunMode = RunMode.Async), Summary("Show the inventory of **Hazuki** card category. " +
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
                        TradingCardCore.printInventory(Context, Config.Hazuki.EmbedColor, "hazuki", category, TradingCardCore.Hazuki.maxNormal));
                }

                //platinum category
                if (showAllInventory || category.ToLower() == "platinum")
                {
                    category = "platinum";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Hazuki.EmbedColor, "hazuki", category, TradingCardCore.Hazuki.maxPlatinum));
                }

                //metal category
                if (showAllInventory || category.ToLower() == "metal")
                {
                    category = "metal";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Hazuki.EmbedColor, "hazuki", category, TradingCardCore.Hazuki.maxMetal));
                }

                //ojamajos category
                if (showAllInventory || category.ToLower() == "ojamajos")
                {
                    category = "ojamajos";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Hazuki.EmbedColor, "hazuki", category, TradingCardCore.Hazuki.maxOjamajos));
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

        [Command("inventory", RunMode = RunMode.Async), Summary("Show the inventory of **Hazuki** card category. " +
            "You can put optional parameter with this format: <bot>!card inventory <category> <username>.")]
        public async Task trading_card_inventory_other([Remainder]SocketGuildUser username = null)
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
                    .WithColor(Config.Hazuki.EmbedColor)
                    .WithDescription($"Sorry, I can't find that username. Please enter the correct username.")
                    .WithThumbnailUrl(TradingCardCore.Hazuki.emojiError).Build());
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
                        TradingCardCore.printInventory(Context, Config.Hazuki.EmbedColor, "hazuki", category, TradingCardCore.Hazuki.maxNormal, username));
                }

                //platinum category
                if (showAllInventory || category.ToLower() == "platinum")
                {
                    category = "platinum";
                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Hazuki.EmbedColor, "hazuki", category, TradingCardCore.Hazuki.maxPlatinum, username));
                }

                //metal category
                if (showAllInventory || category.ToLower() == "metal")
                {
                    category = "metal";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Hazuki.EmbedColor, "hazuki", category, TradingCardCore.Hazuki.maxMetal, username));
                }

                //ojamajos category
                if (showAllInventory || category.ToLower() == "ojamajos")
                {
                    category = "ojamajos";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Hazuki.EmbedColor, "hazuki", category, TradingCardCore.Hazuki.maxOjamajos, username));
                }

                //special category
                if (showAllInventory || category.ToLower() == "special")
                {
                    category = "special";
                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Hazuki.EmbedColor, "other", category, TradingCardCore.maxSpecial, username));
                }

            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        [Command("inventory", RunMode = RunMode.Async), Summary("Show the inventory of **Hazuki** card category. " +
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
                    .WithColor(Config.Hazuki.EmbedColor)
                    .WithDescription($"Sorry, I can't find that username. Please mention the correct username.")
                    .WithThumbnailUrl(TradingCardCore.Hazuki.emojiError).Build());
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
                        TradingCardCore.printInventory(Context, Config.Hazuki.EmbedColor, "hazuki", category, TradingCardCore.Hazuki.maxNormal, username));
                }

                //platinum category
                if (showAllInventory || category.ToLower() == "platinum")
                {
                    category = "platinum";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Hazuki.EmbedColor, "hazuki", category, TradingCardCore.Hazuki.maxPlatinum, username));
                }

                //metal category
                if (showAllInventory || category.ToLower() == "metal")
                {
                    category = "metal";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Hazuki.EmbedColor, "hazuki", category, TradingCardCore.Hazuki.maxMetal, username));
                }

                //ojamajos category
                if (showAllInventory || category.ToLower() == "ojamajos")
                {
                    category = "ojamajos";

                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Hazuki.EmbedColor, "hazuki", category, TradingCardCore.Hazuki.maxOjamajos, username));
                }

                //special category
                if (showAllInventory || category.ToLower() == "special")
                {
                    category = "special";
                    await PagedReplyAsync(
                        TradingCardCore.printInventory(Context, Config.Hazuki.EmbedColor, "other", category, TradingCardCore.maxSpecial, username));
                }

            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        [Command("verify", RunMode = RunMode.Async), Summary("Verify the hazuki card pack to get the card completion role & badge on this server " +
            " if you have completed it.")]
        public async Task verify_card_completion()
        {
            var guildId = Context.Guild.Id;
            var userId = Context.User.Id;
            string userAvatarUrl = Context.User.GetAvatarUrl();
            string username = Context.User.Username;

            string cardPack = "hazuki";

            if (UserTradingCardDataCore.checkCardCompletion(userId, cardPack))
            {
                try
                {
                    if (Context.Guild.Roles.Where(x => x.Name == TradingCardCore.Hazuki.roleCompletionist).ToList().Count >= 1)
                    {
                        await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(
                            Context.Guild.Roles.First(x => x.Name == TradingCardCore.Hazuki.roleCompletionist)
                        );
                    }
                } catch(Exception e) { }
                

                EmbedBuilder embedReturn = TradingCardCore
                .userCompleteTheirList(Context, Config.Hazuki.EmbedColor, Config.Hazuki.EmbedAvatarUrl, cardPack,
                TradingCardCore.Hazuki.imgCompleteAllCard, TradingCardCore.Hazuki.roleCompletionist);

                if (embedReturn != null)
                {
                    await Bot.Hazuki.client
                    .GetGuild(Context.Guild.Id)
                    .GetTextChannel(Context.Channel.Id)
                    .SendFileAsync(TradingCardCore.Hazuki.imgCompleteAllCard, null, embed: embedReturn
                    .Build());
                } else
                {
                    await ReplyAsync(":white_check_mark: Your **hazuki** card completion status has been verified");
                }

                

            }
        }

        [Command("detail", RunMode = RunMode.Async), Alias("info","look"), Summary("See the detail of Hazuki card information from the <card_id>.")]
        public async Task trading_card_look(string card_id)
        {

            await ReplyAsync(null, embed: TradingCardCore.printCardDetailTemplate(Context, Config.Doremi.EmbedColor, card_id, TradingCardCore.Doremi.emojiError)
                    .Build());
        }

        [Command("boost", RunMode = RunMode.Async), Summary("Show card boost status.")]
        public async Task showCardBoostStatus()
        {
            await ReplyAsync(embed: TradingCardCore
                    .printCardBoostStatus(Context, Config.Hazuki.EmbedColor)
                    .Build());
        }

        [Command("zone set"), Alias("region set"), Summary("Set your card zone at **hazuki** and the entered category. " +
            "Example: **ha!card zone platinum**.")]
        public async Task setCardZone(string category = "")
        {
            await ReplyAsync(embed: TradingCardCore.assignZone(Context, "hazuki", category, Config.Hazuki.EmbedColor)
                .Build());
        }

        [Command("zone where"), Alias("region where"), Summary("Get your assigned card zone.")]
        public async Task lookCardZone()
        {
            await ReplyAsync(embed: TradingCardCore.lookZone(Context, Config.Hazuki.EmbedColor)
                .Build());
        }

        //show top 5 that capture each card pack
        [Command("leaderboard", RunMode = RunMode.Async), Summary("Show top 5 hazuki trading card leaderboard status.")]
        public async Task trading_card_leaderboard()
        {
            await ReplyAsync(embed: TradingCardCore.
                    printLeaderboardTemplate(Context, Config.Hazuki.EmbedColor, "hazuki")
                    .Build());
        }

    }
}
