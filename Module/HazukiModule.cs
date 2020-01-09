using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using OjamajoBot.Service;

namespace OjamajoBot.Module
{
    class HazukiModule : ModuleBase<SocketCommandContext>
    {
        [Command("Help")]
        public async Task showhelp()
        {
            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Hazuki.EmbedColor)
                .WithAuthor("Hazuki Bot", "https://cdn.discordapp.com/emojis/651062978854125589.png?v=1")
                .WithTitle("Command List:")
                .WithDescription($"Pretty Witchy Hazuki Chi~ " +
                $"You can either tell me what to do by mentioning me **<@{Config.Hazuki.Id}>** or **hazuki** or **ha** and followed with the <whitespace> as the starting command prefix.")
                .AddField("Basic Commands",
                "**transform** or **henshin** : I will transform into the ojamajo form\n" +
                "**spells <username>,<wishes>** : Transform mentioned <username> with the given <wishes> parameter.\n" +
                "**quotes** : I will mention any random quotes.\n" +
                "**random** : I will do anything random.\n" +
                $"**dab** : I will give you some dab {Config.Emoji.dabzuki}\n"+
                "**coldjokes** : :wind_blowing_face: I will give you some random cold jokes :cold_face:")
                .Build());
        }

        [Command("transform"), Alias("henshin")]
        public async Task transform()
        {
            await ReplyAsync("Pretty Witchy Hazuki Chi~ \n");
            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Hazuki.EmbedColor)
                .WithImageUrl("https://66.media.tumblr.com/568483395f30aa59ca42082291156214/tumblr_p6ksbbhwau1x776xto4_250.gif")
                .Build());
        }

        [Command("spells")]
        public async Task spells([Remainder] string query)
        {
            String[] splitted = query.Split(",");
            splitted[1].TrimStart();
            await Context.Message.DeleteAsync();
            await ReplyAsync("Paipai Ponpoi Puwapuwa Puu! Transform " + splitted[0] + " into " + splitted[1]);

            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Hazuki.EmbedColor)
                .WithImageUrl("https://i.ytimg.com/vi/iOkN602s-JQ/hqdefault.jpg")
                .Build());
        }

        [Command("dab")]
        public async Task dabzuki()
        {
            String[] arrRandom =
            {
                $":sunglasses: Let's do the dab with me, everyone! {Config.Emoji.dabzuki}",
                $":sunglasses: Please dab with me, <@{Context.User.Id}> {Config.Emoji.dabzuki}",
                $"Don't tell me to do the dab, <@{Context.User.Id}> {Config.Emoji.dabzuki}",
                $":regional_indicator_d:ab, dab and dab {Config.Emoji.dabzuki}",
                $":regional_indicator_d::regional_indicator_a::regional_indicator_b::regional_indicator_z::regional_indicator_u::regional_indicator_k::regional_indicator_i: in action! {Config.Emoji.dabzuki}"
            };

            await ReplyAsync(arrRandom[new Random().Next(0, arrRandom.GetLength(0))],
                    embed: new EmbedBuilder()
                    .WithColor(Config.Hazuki.EmbedColor)
                    .WithImageUrl("https://cdn.discordapp.com/attachments/663232256676069386/663603236099457035/Dabzuki.png")
                    .Build());

            //await ReplyAsync(arrRandom[rndIndex]);
        }

        [Command("random")]
        public async Task randomthing()
        {
            String[,] arrRandom =
            { {"*chuckling intestifies*" , "http://33.media.tumblr.com/28c2441a5655ecb1bd23df8275f3598f/tumblr_nfkjtbSQZg1r98a5go1_500.gif"},
            {"*casting magical stage*", "https://i.pinimg.com/originals/64/bf/74/64bf74df2f6e326a30865756933117cd.gif"},
            {"Happy Hazuki","https://media1.tenor.com/images/0e7fa48b017b6904fa2587729ec2e64d/tenor.gif" },
            {"Hazuki looks really sad, please cheer her up.","https://thumbs.gfycat.com/ApprehensiveLimpCardinal-size_restricted.gif" },
            {"*Screaming loudly*","https://i.pinimg.com/originals/71/c4/57/71c45767cfd3febf17cdea7aba96d06f.gif" },
            {"*Blushing intestifies*","http://i.4pcdn.org/s4s/1518104531115.gif"} };

            Random rnd = new Random();
            int rndIndex = rnd.Next(0, arrRandom.GetLength(0));

            await ReplyAsync(arrRandom[rndIndex, 0]);
            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Hazuki.EmbedColor)
                .WithImageUrl(arrRandom[rndIndex, 1])
                .Build());
        }

        //magical stage section
        [Command("Pirika pirilala, Nobiyaka ni!")] //magical stage from doremi
        public async Task magicalStage()
        {
            if (Context.User.Id == Config.Doremi.Id)
            {
                await ReplyAsync($"<@{Config.Aiko.Id}> Paipai Ponpoi, Shinyaka ni! \n",
                    embed: new EmbedBuilder()
                    .WithColor(Config.Hazuki.EmbedColor)
                    .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/4/4b/Shinayakanis1.2.png/revision/latest?cb=20190408124906")
                    .Build());
            }
        }

        [Command("Magical Stage!")]//Final magical stage: from doremi
        public async Task magicalStagefinal([Remainder] string query)
        {
            if (Context.User.Id == Config.Doremi.Id)
            {
                await ReplyAsync($"<@{Config.Aiko.Id}> Magical Stage! {query}\n");
            }
        }

        [Command("quotes")]
        public async Task quotes()
        {
            String[] arrQuotes = {
                "Majo Rika! Majo Rika!"
            };

            await ReplyAsync(arrQuotes[new Random().Next(0, arrQuotes.Length)]);
        }

        [Command("coldjokes")]
        public async Task randomcoldjokes()
        {

            String[] arrRandom =
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

            await ReplyAsync(arrRandom[new Random().Next(0, arrRandom.GetLength(0))],
                    embed: new EmbedBuilder()
                    .WithColor(Config.Hazuki.EmbedColor)
                    .WithImageUrl("https://cdn.discordapp.com/attachments/663232256676069386/663603236099457035/Dabzuki.png")
                    .Build());

        }

    }

    class HazukiRandomEventModule : ModuleBase<SocketCommandContext>
    {
        List<string> listRespondDefault = new List<string>() {
            ":pensive: I'm afraid I can't right now Doremi chan, I have violin lesson to attend",
            ":pensive: I'm afraid I can't right now Doremi chan, I have ballet lesson to attend"
        };

        [Remarks("go to the shop event")]
        [Command("let's go to maho dou")]
        public async Task eventmahoudou()
        {
            if (Context.User.Id == Config.Doremi.Id)
            {
                List<string> listRespond = new List<string>() { ":smile: Sure thing Doremi chan, let's go to the shop" };

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
                List<string> listRespond = new List<string>() { ":smile: Sure Doremi Chan, let's go to your house" };

                for (int i = 0; i < listRespondDefault.Count - 1; i++)
                    listRespond.Add(listRespondDefault[i]);

                Random rnd = new Random();
                int rndIndex = rnd.Next(0, listRespond.Count); //random the list value
                await ReplyAsync($"{listRespond[rndIndex]}");
            }
        }

        //[Command("please give me some big steak")]
        //public async Task eventgivemesteak()
        //{
        //    //if (Context.User.Id == Config.Doremi.Id)
        //    //{
        //    await ReplyAsync($"Sure thing Doremi chan, let's go to the shop now");
        //    //}
        //}
    }


        //public class HazukiMusic : ModuleBase<SocketCommandContext>
        //{
        //    //a modules stops existing when a command is done executing and services exist aslong we did not dispose them

        //    // Scroll down further for the AudioService.
        //    // Like, way down
        //    private readonly AudioService _service;

        //    // Remember to add an instance of the AudioService
        //    // to your IServiceCollection when you initialize your bot
        //    public HazukiMusic(AudioService service)
        //    {
        //        _service = service;
        //    }

        //    // You *MUST* mark these commands with 'RunMode.Async'
        //    // otherwise the bot will not respond until the Task times out.
        //    [Command("join", RunMode = RunMode.Async)]
        //    public async Task JoinCmd()
        //    {
        //        await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        //    }

        //    // Remember to add preconditions to your commands,
        //    // this is merely the minimal amount necessary.
        //    // Adding more commands of your own is also encouraged.
        //    [Command("leave", RunMode = RunMode.Async)]
        //    public async Task LeaveCmd()
        //    {
        //        await _service.LeaveAudio(Context.Guild);
        //    }

        //    [Command("play", RunMode = RunMode.Async)]
        //    public async Task PlayCmd([Remainder] string song)
        //    {
        //        await _service.SendAudioAsync(Context.Guild, Context.Channel, "music/" + song + ".mp3");
        //    }

        //}
    }
