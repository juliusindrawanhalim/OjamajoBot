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
                $"You can either tell me what to do by mentioning me <@{Config.Hazuki.Id}> or **hazuki!** or **ha!** as the starting command prefix.")
                .AddField("Basic Commands",
                "**hello** : I will greet you up\n" +
                "**change** or **henshin** : I will change into the ojamajo form\n" +
                "**transform <username> <wishes>** : Transform mentioned <username> into <wishes>\n" +
                "**wish <wishes>** : Give the user some <wishes>\n" +
                "**random** : I will do anything random\n" +
                $"**dab** or **dabzuki** : I will give you some dab {Config.Emoji.dabzuki}\n"+
                "**wheezuki** or **laughzuki** : :wind_blowing_face: I will give you some random woosh jokes :cold_face:")
                .Build());
        }

        [Command("hello")]
        public async Task hazukiHello()
        {
            string tempReply = "";
            List<string> listRandomRespond = new List<string>() {
                    $"Hello there {MentionUtils.MentionUser(Context.User.Id)}. ",
                    $"Hello, {MentionUtils.MentionUser(Context.User.Id)}. ",
            };

            int rndIndex = new Random().Next(0, listRandomRespond.Count);
            tempReply = listRandomRespond[rndIndex] + Config.Hazuki.arrRandomActivity[Config.Hazuki.indexCurrentActivity, 1];

            await ReplyAsync(tempReply);
        }

        [Command("change"), Alias("henshin")]
        public async Task transform()
        {
            await ReplyAsync("Pretty Witchy Hazuki Chi~\n");
            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Hazuki.EmbedColor)
                .WithImageUrl("https://66.media.tumblr.com/568483395f30aa59ca42082291156214/tumblr_p6ksbbhwau1x776xto4_250.gif")
                .Build());
        }

        [Command("transform")]
        public async Task spells(IUser iuser, [Remainder] string query)
        {
            //await Context.Message.DeleteAsync();
            await ReplyAsync("Paipai Ponpoi Puwapuwa Puu! Transform " + iuser.Mention + " into " + query);
            await base.ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Hazuki.EmbedColor)
            .WithImageUrl("https://i.ytimg.com/vi/iOkN602s-JQ/hqdefault.jpg")
            .Build());
        }

        [Command("wish")]
        public async Task wish([Remainder] string query)
        {
            await ReplyAsync($"Paipai Ponpoi Puwapuwa Puu! {query}");
            await base.ReplyAsync(embed: new EmbedBuilder()
            .WithColor(Config.Hazuki.EmbedColor)
            .WithImageUrl("https://i.ytimg.com/vi/iOkN602s-JQ/hqdefault.jpg")
            .Build());
        }

        [Command("dab"), Alias("dabzuki")]
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
            {"*Blushing intestifies*","http://i.4pcdn.org/s4s/1518104531115.gif"},
            {"Thank you, thank you :smiley:","https://66.media.tumblr.com/beb5b047c9b499ed49275928de28a77f/tumblr_inline_mgcb5dAQQ51r4lv3u.gif" } };

            Random rnd = new Random();
            int rndIndex = rnd.Next(0, arrRandom.GetLength(0));

            await ReplyAsync(arrRandom[rndIndex, 0]);
            await base.ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Config.Hazuki.EmbedColor)
                .WithImageUrl(arrRandom[rndIndex, 1])
                .Build());
        }

        [Command("wheezuki"),Alias("laughzuki")]
        public async Task randomcoldjokes()
        {
            int randomType = new Random().Next(0, 2);
            if (randomType == 0)
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
                    .WithColor(Config.Hazuki.EmbedColor)
                    .WithImageUrl(arrRandom[new Random().Next(0, arrRandom.GetLength(0))])
                    .Build());
            } else {
                string[] arrRandom =
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
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Config.Hazuki.EmbedColor)
                    .WithDescription(arrRandom[new Random().Next(0, arrRandom.GetLength(0))])
                    .WithImageUrl("https://cdn.discordapp.com/attachments/644383823286763544/665777255640989749/Wheezuki.png")
                    .Build());
            }
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

    }

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
                List<string> listRespond = new List<string>() {$":smile: Sure thing {MentionUtils.MentionUser(Config.Doremi.Id)}-chan, let's go to the shop." };

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
                List<string> listRespond = new List<string>() {$":smile: Sure {MentionUtils.MentionUser(Config.Doremi.Id)}-chan, let's go to your house." };

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
