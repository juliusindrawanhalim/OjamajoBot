using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using OjamajoBot.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace OjamajoBot.Module
{
    class DoremiModule : ModuleBase<SocketCommandContext>
    {
        [Command("Help")]
        public async Task showHelpUsers()
        {
            await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithAuthor($"Doremi Bot","https://cdn.discordapp.com/emojis/651062436866293760.png?v=1")
                .WithTitle("Command List:")
                .WithDescription($"Pretty Witchy Doremi Chi~ " +
                $"You can either tell me what to do by mentioning me <@{Config.Doremi.Id}> or **doremi!** or **do!** as the starting command prefix.")
                .AddField("Basic Commands",
                "**hello** : Hello, I will greet you up\n" +
                "**change** or **henshin** : I will change into the ojamajo form\n" +
                "**transform <username> <wishes>** : Transform mentioned <username> into <wishes>\n" +
                "**wish <wishes>** : Give the user some <wishes>\n" +
                $"**steak**: Random steak moments will appeared {Config.Emoji.drool}{Config.Emoji.steak}\n" +
                "**quotes** : I will mention any random quotes\n" +
                "**random** : I will do anything random.\n" +
                "**quiz** : I will give you some quiz. Think you can answer them all?\n" +
                "**magicalstage** or **magical stage** **<wishes>**: I will perform magical stage along with the other and make a <wishes> [This requires Hazuki & Aiko Bot]\n" +
                "**dorememe** or **dorememes** : I will give you some random memes :zany_face:\n" +
                "**hug** <username> : I will give warm hug for mentioned <username>\n" +
                "**invite** : I will generate the invitation links for related ojamajo bot")
                //"**musicrepeat** or **murep** <Off/One/All> : Toggle the Music Repeat State based on the <parameter>\n" +
                .AddField("Musical Commands (On Beta, there might be bug on some feature)",
                "**join** : I will enter the voice channel based on where you connected\n" +
                "**musiclist** or **mulist** : Show the doremi music list\n" +
                "**queue** or **muq** : Show all the music that's currently on queue list\n" +
                "**youtube** or **yt** <keyword or url> : Play the youtube music either it's from keyword/url\n" +
                "**play <track nunmber or title>** : Play the music with the given <title> parameter\n" +
                "**playall** : Play all the music that's available on doremi music list\n" +
                "**skip** : Play the music with the given <title> parameter\n" +
                "**seek** <timespan> : Seek the music into the given <timespan>[Format: hh:mm::ss] \n" +
                "**stop** : Stop playing the music. This will also clear the queue list")
                .AddField("Moderator Commands [``Manage Channels`` permissions])",
                "**mod help** : This will display all basic moderator commands list")
                .Build());
        }

        [Command("hello")]
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

        [Command("change"), Alias("henshin")]
        public async Task transform()
        {
            await ReplyAsync("Pretty Witchy Doremi Chi~\n");
            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl("https://media1.tenor.com/images/b99530648de9200b2cfaec83426f5482/tenor.gif")
                .Build());
        }

        [Command("transform")]
        public async Task spells(IUser iuser,[Remainder] string query)
        {
            //await Context.Message.DeleteAsync();
            await ReplyAsync("Pirika pirilala poporina peperuto! Transform " + iuser.Mention + " into " + query);
            await base.ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Doremi.EmbedColor)
            .WithImageUrl("https://i.makeagif.com/media/10-05-2015/rEFQz2.gif")
            .Build());
        }

        [Command("wish")]
        public async Task wish([Remainder] string query)
        {
            await ReplyAsync($"Pirika pirilala poporina peperuto! {query}");
            await base.ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Doremi.EmbedColor)
            .WithImageUrl("https://i.makeagif.com/media/10-05-2015/rEFQz2.gif")
            .Build());
        }

        [Command("quotes")]
        public async Task quotes()
        {
            String[] arrQuotes = {
                "I'm the world's most unluckiest pretty girl!",
                "Happy! Lucky! For all of you!",
                "I like Steak very much"
            };

            await ReplyAsync(arrQuotes[new Random().Next(0, arrQuotes.Length)]);
        }

        [Command("random")]
        public async Task randomthing()
        {
            String[,] arrRandom =
            { {"Doremi has give you a smug looking" , "https://66.media.tumblr.com/bd4f75234f1180fa7fd99a5200ac3c8d/tumblr_nbhwuqEY6c1r98a5go1_500.gif"},
            {$"Doremi is very happy to meet you: {MentionUtils.MentionUser(Context.User.Id)}", "https://66.media.tumblr.com/e62f24b9645540f4fff4e6ebe8bd213e/tumblr_pco5qx9Mim1r98a5go1_500.gif"},
            {"Majo Rika! Majo Rika!", "https://media1.tenor.com/images/2bedf54ca06c5a3b073f3d9349db65b4/tenor.gif"},
            {"Doremi is cheering you up", "https://static.zerochan.net/Harukaze.Doremi.full.2494232.gif"},
            {$"Doremi was looking very happy at you: {MentionUtils.MentionUser(Context.User.Id)}", "https://i.4pcdn.org/s4s/1511988377651.gif"},
            {"Doremi is giving her best", "https://66.media.tumblr.com/68b432cf50e18a72b661ba952fcf778f/tumblr_pgohlgzfvY1xqvqxzo1_400.gif"},
            {":sob: *Crying loudly*","https://espressocomsaudade.files.wordpress.com/2014/07/6.gif"},
            {":scream:","https://cdn.discordapp.com/attachments/569409307100315651/646751194441842688/unknown.png" },
            {"Thank you, thank you :sweat_smile:","https://66.media.tumblr.com/b11f9dced4594739776b976ed66920fc/tumblr_inline_mgcb4zEbfV1r4lv3u.gif" } };

            Random rnd = new Random();
            int rndIndex = rnd.Next(0, arrRandom.GetLength(0));

            await ReplyAsync(arrRandom[rndIndex, 0]);
            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl(arrRandom[rndIndex, 1])
                .Build());
        }

        [Command("dorememe"), Alias("dorememes")]
        public async Task givememe()
        {
            String[] arrRandom =
            {"https://i.imgflip.com/1h9k61.jpg",
            "https://66.media.tumblr.com/4b8ae988116282b0fbb86156006977a7/tumblr_ndl02pfvej1thwu0wo1_1280.png",
            "https://66.media.tumblr.com/6143b1c1b6033c4cc068904909b68fbd/tumblr_n91u5yW35z1thwu0wo1_1280.png",
            "https://66.media.tumblr.com/df6d13c7abe1970b4bc9726e5c264252/tumblr_n8ypyaubZl1thwu0wo1_1280.png",
            "https://66.media.tumblr.com/1c00104523408517270a02f185208ff6/tumblr_n9iqy3L44d1thwu0wo1_1280.png",
            "https://66.media.tumblr.com/ffad930ddacf0964646700523e80fb81/tumblr_n906n643rG1thwu0wo1_1280.png",
            "https://img1.ak.crunchyroll.com/i/spire4/1cd32824fff0e3be86cbd9f6c5b4cb2b1326942608_full.jpg",
            "https://66.media.tumblr.com/9fdbbdc668507fa90c38bae8fa8d9f8a/tumblr_nvu6gqb6NE1thwu0wo1_1280.png",
            "https://images-wixmp-ed30a86b8c4ca887773594c2.wixmp.com/f/e90aeb60-6815-432d-bc4b-ad18ae885aaf/ddeph8m-bf0e2b8c-bc89-4e2f-b27c-f31d00d3c6cb.png/v1/fill/w_742,h_1077,q_70,strp/my_strawberry_shortcake_cast_meme__ojamajo_doremi__by_balloongal101_ddeph8m-pre.jpg?token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1cm46YXBwOjdlMGQxODg5ODIyNjQzNzNhNWYwZDQxNWVhMGQyNmUwIiwiaXNzIjoidXJuOmFwcDo3ZTBkMTg4OTgyMjY0MzczYTVmMGQ0MTVlYTBkMjZlMCIsIm9iaiI6W1t7ImhlaWdodCI6Ijw9MTQ4NSIsInBhdGgiOiJcL2ZcL2U5MGFlYjYwLTY4MTUtNDMyZC1iYzRiLWFkMThhZTg4NWFhZlwvZGRlcGg4bS1iZjBlMmI4Yy1iYzg5LTRlMmYtYjI3Yy1mMzFkMDBkM2M2Y2IucG5nIiwid2lkdGgiOiI8PTEwMjQifV1dLCJhdWQiOlsidXJuOnNlcnZpY2U6aW1hZ2Uub3BlcmF0aW9ucyJdfQ.YimkZYTzceBEJT3bYtwr3b0wsHrg2RGNou-a4uuLS6M",
            "https://pics.ballmemes.com/how-every-country-sees-magical-girl-anime-sailor-moon-ojamajo-44565702.png",
            "https://static.fjcdn.com/pictures/Ojamajo_a17764_528025.jpg",
            "https://cdn.discordapp.com/attachments/644383823286763544/663616227914154014/unknown.png",
            "https://media.discordapp.net/attachments/512825478512377877/660677566599790627/DO_THE_SWAG.gif",
            "https://cdn.discordapp.com/attachments/569409307100315651/653670970342506548/unknown.png",
            "https://media.discordapp.net/attachments/310544560164044801/398230870445785089/DSRxAB9VQAAI5Ja.png",
            "https://i.4pcdn.org/s4s/1537724473581.gif",
            "https://i.4pcdn.org/s4s/1508866828910.gif",
            "http://i.4pcdn.org/s4s/1500066705217.gif"};

            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl(arrRandom[new Random().Next(0, arrRandom.GetLength(0))])
                .Build());
        }

        [Command("steak")]
        public async Task randomsteakmoments()
        {
            String[,] arrRandom =
            { {$"Itadakimasu!{Config.Emoji.drool}" , "https://66.media.tumblr.com/5cea42347519a4f8159197ec6a064eb4/tumblr_olqtewoJDS1r809wso2_640.png"},
            {"How nice, I want to get proposed with steak too.", "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQIrd47iWprHNy350YW9GKT1E3CBWXekyF2Dk9KFzKcLWHwcltU1g&s"},
            {$"Itadakimasu~{Config.Emoji.drool}", "https://i.4pcdn.org/s4s/1507491838404.jpg"},
            {$"*Dreaming of steaks {Config.Emoji.drool}*", "https://i.4pcdn.org/s4s/1505677406037.png"},
            {$"Big Steak~{Config.Emoji.drool}", "https://images.plurk.com/vTMo-3u8rgOUOAP0RE2hrzIJHvs.jpg"},
            {$"I can't wait to taste this delicious steak~{Config.Emoji.drool}", "https://scontent-mia3-1.cdninstagram.com/vp/54447c3d0032fab0e92771612f457bc6/5E23392F/t51.2885-15/e35/68766038_126684885364883_7230690820171265153_n.jpg?_nc_ht=scontent-mia3-1.cdninstagram.com&_nc_cat=111&ig_cache_key=MjEzNDM1MTExMTk4MTM3NDA1NQ%3D%3D.2"},
            {$"A wild steak has appeared!","https://66.media.tumblr.com/85ac2417517a14300a8660a536b9e940/tumblr_oxy34aOH591tdnbbbo1_640.gif" },
            {$"Itadakimasu!{Config.Emoji.drool}" ,"https://i.4pcdn.org/s4s/1507491838404.jpg"},
            {$"Big steak{Config.Emoji.drool}","https://images.plurk.com/vTMo-3u8rgOUOAP0RE2hrzIJHvs.jpg" },
            {$"Itadakimasu!{Config.Emoji.drool}","https://66.media.tumblr.com/337aaf42d3fb0992c74f7f9e2a0bf4f6/tumblr_olqtewoJDS1r809wso1_500.png" },
            {$"*Pouting madly*","https://media1.tenor.com/images/c9d91a992a919d4c92e2d5d499f379d2/tenor.gif" } };

            Random rnd = new Random();
            int rndIndex = rnd.Next(0, arrRandom.GetLength(0));

            await ReplyAsync(arrRandom[rndIndex, 0]);
            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl(arrRandom[rndIndex, 1])
                .Build());
        }

        [Command("hug")]
        [RequireBotPermission(ChannelPermission.AddReactions, ErrorMessage = "I don't have permission to add a reaction.")]
        public async Task HugUser(SocketGuildUser user)
        {
            string message = $"Pirika pirilala poporina peperuto! Give a warm hug for {MentionUtils.MentionUser(user.Id)} :hugging:.";
            var sent = await Context.Channel.SendMessageAsync(message);
            await sent.AddReactionAsync(new Emoji("\uD83E\uDD17"));
            //to add custom emote:
            //if (Emote.TryParse("<:hugging:>", out var emote))
            //    await sent.AddReactionAsync(emoji);

        }

        [Command("invite")]
        public async Task invite()
        {
            await ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Doremi.EmbedColor)
            .WithAuthor("Doremi Bot", "https://cdn.discordapp.com/emojis/651062436866293760.png?v=1")
            .WithTitle("Bot Invitation Links")
            .WithDescription($"Pirika pirilala poporina peperuto! Generate the bot links!")
            .AddField("Doremi Bot", "[Click here to invite Doremi Bot](https://discordapp.com/api/oauth2/authorize?client_id=655668640502251530&permissions=2117532736&scope=bot)")
            .AddField("Hazuki Bot", "[Click here to invite Hazuki Bot](https://discordapp.com/api/oauth2/authorize?client_id=655307117128974346&permissions=238419008&scope=bot)")
            .AddField("Aiko Bot", "[Click here to invite Aiko Bot](https://discordapp.com/api/oauth2/authorize?client_id=663612449341046803&permissions=238419008&scope=bot)")
            .Build());
        }

        //magical stage section
        [Command("magical stage"), Alias("magicalstage")]
        public async Task magicalStage([Remainder] string query)
        {
            if (query != null)
            {
                Config.Doremi.MagicalStageWishes = query;
                await ReplyAsync($"<@{Config.Hazuki.Id}> Pirika pirilala, Nobiyaka ni!\n",
                embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/3d/Nobiyakanis1.2.png/revision/latest?cb=20190408124752")
                .Build());
            } else {
                await ReplyAsync($"Please enter your wishes.");
            }

        }

        [Command("Pameruku raruku, Takaraka ni!")]//from aiko
        public async Task magicalStagefinal()
        {
            if (Context.User.Id == Config.Aiko.Id)
            {
                await ReplyAsync($"<@{Config.Hazuki.Id}> Magical Stage! {Config.Doremi.MagicalStageWishes}\n");
                Config.Doremi.MagicalStageWishes = "";//erase the magical stage
            }
        }

        //todo/more upcoming commands: 10 star voting pinned message, easter egg/hidden commands, set daily message announcement, gacha,
        //witch seeds to cast a spells
        //10 stars pinned message 
        //mini hangman game, virtual maho-dou shop interactive
    }

    [Group("mod")]
    [RequireUserPermission(ChannelPermission.ManageChannels,
        ErrorMessage = "Oops, You need to have the ``manage channels`` permission to use this command",
        NotAGuildErrorMessage = "Oops, You need to have the ``manage channels`` permission to use this command")]
    public class DoremiModerator : ModuleBase<SocketCommandContext>
    {
        [Command("Help")]
        public async Task showHelpModerator()
        {
            await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithAuthor("Doremi Bot", "https://cdn.discordapp.com/emojis/651062436866293760.png?v=1")
                .WithTitle("Moderator Command List:")
                .WithDescription($"Require ``Manage Channels`` permission.\n" +
                $"Channel Moderator Prefix: **<@{Config.Doremi.Id}> mod channels** or **doremi!mod channels** or **do!mod channels**\n" +
                $"Basic Moderator Prefix: **<@{Config.Doremi.Id}> mod** or **doremi!mod** or **do!mod** followed with the <whitespace>")
                .AddField("Channel Moderator:",
                $"**randomevent** <channel_name> : Schedule <@{Config.Doremi.Id}> to do random event message on mentioned <channel_name> every 1 hour. \n" +
                $"**online** <channel_name> : Will make <@{Config.Doremi.Id}> to announce on <channel_name> whenever the bot goes online again.\n" +
                $"**remove** <randomevent|online> : Will remove the settings for given parameter: Randomevent or online")
                .AddField("Basic Moderator:",
                "**help** : You already execute this command\n" +
                "**guildid** : This will give you the Guild/Server Id\n" +
                "**channelid** <channel_name> : This will give you the Channel Id within the optional mentioned <channel_name> parameter\n")
                .Build());
        }

        [Command("guildid")]
        public async Task getGuildId(){
            await ReplyAsync($"{Context.Guild.Id}");
        }

        [Command("channelid")]
        public async Task getCurrentChannelId(){
            await ReplyAsync($"{Context.Channel.Id}");
        }

        [Command("channelid")]
        public async Task getChannelId(IGuildChannel guildChannel){
            await ReplyAsync($"{guildChannel.Id}");
        }

    }

    [Group("mod channels")]
    [RequireUserPermission(ChannelPermission.ManageChannels,
        ErrorMessage = "Oops, You need to have the ``manage channels`` permission to use this command",
        NotAGuildErrorMessage = "Oops, You need to have the ``manage channels`` permission to use this command")]
    public class DoremiChannelsModerator : ModuleBase<SocketCommandContext>
    {

        [Command("randomEvent")]
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
                    try{
                        await socketClient
                        .GetGuild(iguild.GuildId)
                        .GetTextChannel(Config.Guild.Id_random_event[iguild.GuildId.ToString()])
                        .SendMessageAsync(Config.Doremi.listRandomEvent[rndIndex]);
                    } catch {
                        Console.WriteLine($"Doremi Random Event Exception: Send message permissions has been missing {iguild.Guild.Name} : {iguild.Name}");
                    }
                    
                },
                null,
                TimeSpan.FromSeconds(Config.Doremi.Randomeventinterval), //time to wait before executing the timer for the first time
                TimeSpan.FromSeconds(Config.Doremi.Randomeventinterval) //time to wait before executing the timer again
            );

            await ReplyAsync($"**Random Event Channels** has been assigned into: {MentionUtils.MentionChannel(iguild.Id)}");
        }

        [Command("online")]
        public async Task assignNotifOnline(IGuildChannel iguild)
        {
            Config.Guild.assignId(iguild.GuildId, "id_notif_online", iguild.Id.ToString());
            await ReplyAsync($"**Bot Online Notification Channels** has been assigned into: {MentionUtils.MentionChannel(iguild.Id)}");
        }

        [Command("remove")]
        public async Task assignRandomEvent(string query)
        {
            String property = "";
            Boolean propertyExists = false;
            ulong channelId = 0;
            
            if (query.ToLower() == "randomevent"){
                property = "Random Event";
                
                if (Config.Guild.Id_random_event.ContainsKey(Context.Guild.Id.ToString())){
                    channelId = Config.Guild.Id_random_event[Context.Guild.Id.ToString()];
                    propertyExists = true;
                    Config.Guild.assignId(Context.Guild.Id, "id_random_event", "");
                    if (Config.Doremi._timerRandomEvent.ContainsKey(Context.Guild.Id.ToString()))
                        Config.Doremi._timerRandomEvent[Context.Guild.Id.ToString()].Change(Timeout.Infinite, Timeout.Infinite);
                }
                
                
            } else if (query.ToLower() == "notifonline") {
                property = "Bot Online Notification";

                if (Config.Guild.Id_notif_online.ContainsKey(Context.Guild.Id.ToString())){
                    channelId = Config.Guild.Id_notif_online[Context.Guild.Id.ToString()];
                    propertyExists = true;
                    Config.Guild.assignId(Context.Guild.Id, "id_notif_online", "");
                } 
            }

            if (propertyExists)
                await ReplyAsync($"**{property} Channels** settings has been removed.");
            else
                await ReplyAsync($"**{property} Channels** has no settings yet.");
        }
    }

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

        [Command("Join")]
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
                await ReplyAsync("You must be connected to a voice channel!");
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

        [Command("Leave")]
        public async Task LeaveAsync()
        {
            //await _lavaNode.LeaveAsync((Context.User as IVoiceState).VoiceChannel);
            //await ReplyAsync($"Left {(Context.User as IVoiceState).VoiceChannel} channel!");


            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to any voice channels!");
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

        [Command("Move")]
        public async Task MoveAsync()
        {
            await _lavaNode.MoveAsync((Context.User as IVoiceState).VoiceChannel);
            var player = _lavaNode.GetPlayer(Context.Guild);
            await ReplyAsync($"Moved from {player.VoiceChannel} to {(Context.User as IVoiceState).VoiceChannel}!");
        }

        [Command("Seek")]
        public async Task SeekAsync([Remainder] string timeSpan)
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
                await player.SeekAsync(TimeSpan.Parse(timeSpan));
                await ReplyAsync($"I've seeked `{player.Track.Title}` to {TimeSpan.Parse(timeSpan)}.");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        //https://www.youtube.com/watch?v=dQw4w9WgXcQ
        [Command("youtube"), Alias("yt")]
        public async Task PlayYoutubeAsync([Remainder] string query)
        {
            var search = await _lavaNode.SearchYouTubeAsync(query);
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

        [Command("playall")]
        public async Task PlayAll()
        {
            var player = _lavaNode.HasPlayer(Context.Guild)
                ? _lavaNode.GetPlayer(Context.Guild)
                : await _lavaNode.JoinAsync((Context.User as IVoiceState).VoiceChannel);

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

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

        [Command("play")]
        public async Task PlayLocal([Remainder] string query)
        {
            var player = _lavaNode.HasPlayer(Context.Guild)
                ? _lavaNode.GetPlayer(Context.Guild)
                : await _lavaNode.JoinAsync((Context.User as IVoiceState).VoiceChannel);

            if (string.IsNullOrWhiteSpace(query))
            {
                await ReplyAsync("Please provide search terms.");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            JObject jObj = Config.Music.jobjectfile;
            if (int.TryParse(query, out int n)) {
                
                if(n <= (jObj.GetValue("musiclist") as JObject).Count){
                    query = jObj.GetValue("musiclist")[n.ToString()]["filename"].ToString();
                } else {
                    await ReplyAsync($"I wasn't able to find anything for track number {query}. See the available doremi music list on ``doremi!mulist`` commands.");
                    return;
                }
                
            } else {
                for (int i = 0; i < (jObj.GetValue("musiclist") as JObject).Count; i++)
                {
                    String replacedFilename = jObj.GetValue("musiclist")[(i + 1).ToString()]["filename"].ToString().Replace(".mp3", "").Replace(".ogg", "");
                    if (replacedFilename == query)
                    {
                        query = jObj.GetValue("musiclist")[(i + 1).ToString()]["filename"].ToString();
                    }
                    
                }
            }

            var searchResponse = await _lavaNode.SearchAsync("music/"+query);
            if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                searchResponse.LoadStatus == LoadStatus.NoMatches)
            {
                await ReplyAsync($"I wasn't able to find anything for `{query}`. See the available doremi music list on ``doremi!mulist`` commands.");
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

        [Command("NowPlaying"), Alias("Np")]
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

        [Command("Pause")]
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

        [Command("Resume")]
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

        [Command("Stop")]
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

        [Command("Skip")]
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

        [Command("Volume")]
        public async Task SetVolume([Remainder] ushort query)
        {
            await _lavaNode.GetPlayer(Context.Guild).UpdateVolumeAsync(query);
            await ReplyAsync($":sound: Volume set to:{query}");
        }

        [Command("Musiclist"), Alias("mulist")]
        public async Task ShowMusicList()
        {
            JObject jObj = Config.Music.jobjectfile;
            String musiclist="";
            for (int i = 0; i < (jObj.GetValue("musiclist") as JObject).Count; i++)
            {
                String replacedFilename = jObj.GetValue("musiclist")[(i + 1).ToString()]["filename"].ToString().Replace(".mp3","").Replace(".ogg","");
                String title = jObj.GetValue("musiclist")[(i + 1).ToString()]["title"].ToString();
                musiclist += $"[**{i+1}**] **{replacedFilename}** : {title}\n";
            }
            //for (int i = 0; i < Config.MusicList.arrMusicList.Count; i++)
            //{
            //    String seperatedMusicTitle = Config.MusicList.arrMusicList[i].Replace(".mp3", "").Replace(".ogg", "");//erase format
            //    String musiclist = $"[**{i + 1}**] **ojamajocarnival** : Ojamajo Carnival\n";
            //}

            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor)
                .WithAuthor("Doremi Bot", "https://cdn.discordapp.com/emojis/651062436866293760.png?v=1")
                .WithTitle("Music List:")
                .WithDescription($"These are the music list that's available for me to play: " +
                $"You can use the **play** commands followed with the track number or title.\n" +
                $"Example: **doremi!play 1** or **doremi!play ojamajocarnival**")
                .AddField("[Track No] Title",
                musiclist)
                .Build());
        }

        [Command("queue"), Alias("muq")]
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
                    .AddField($"[Track No] Title",
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

        [Command("quiz", RunMode = RunMode.Async)]
        public async Task Interact_Quiz()
        {
            Random rnd = new Random();
            int rndQuiz = rnd.Next(0, 4);

            String question, replyCorrect, replyWrong, replyEmbed;
            List<string> answer = new List<string>();
            String replyTimeout = "Time's up. Sorry but it seems you haven't answered yet.";

            if (rndQuiz == 0){
                question = "What is my favorite food?";
                answer.Add("steak");
                replyCorrect = "Ding Dong, correct! I love steak very much";
                replyWrong = "Sorry but that's wrong.";
                replyTimeout = "Time's up. My favorite food is steak.";
                replyEmbed = "https://66.media.tumblr.com/337aaf42d3fb0992c74f7f9e2a0bf4f6/tumblr_olqtewoJDS1r809wso1_500.png";
            } else if (rndQuiz == 1) {
                question = "Where do I attend my school?";
                answer.Add("misora elementary school"); answer.Add("misora elementary"); answer.Add("misora school");
                replyCorrect = "Ding Dong, correct!";
                replyWrong = "Sorry but that's wrong.";
                replyTimeout = "Time's up. I went to Misora Elementary School.";
                replyEmbed = "https://vignette.wikia.nocookie.net/ojamajowitchling/images/d/df/E.JPG/revision/latest?cb=20160108002304";
            } else if (rndQuiz == 2) {
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

        [Command("getChannelName")]
        public async Task getChannelId()
        {
            await Context.Channel.SendMessageAsync($"{MentionUtils.MentionChannel(Context.Channel.Id)}");
        }

        //PagedReplyAsync will send a paginated message to the channel
        //You can customize the paginator by creating a PaginatedMessage object
        //You can customize the criteria for the paginator as well, which defaults to restricting to the source user
        // This method will not block.
        [Command("paginator")]
        public async Task Test_Paginator()
        {
            PaginatedMessage page = new PaginatedMessage();
            var pages = new[] { "Page 1", "Page 2", "Page 3", "aaaaaa", "Page 5" };

            await PagedReplyAsync(pages);
        }

    }
        


}
