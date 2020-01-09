using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using System.Timers;

using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

using OjamajoBot.Module;

using Victoria;
using Discord.Audio;
using System.Diagnostics;
using System.IO;
using OjamajoBot.Service;

namespace OjamajoBot.Bot
{
    public class Doremi
    {
        private CommandService commands;
        private IServiceProvider services;

        private DiscordSocketClient client;

        //set timer for random event
        //private static Timer randomEventTimer;
        //private int minRandomEventInterval;

        private AudioService audioservice;
        //private Lavalink4netService lavalink4netservice;

        private VictoriaService victoriaservice;

        //init lavanode
        private LavaNode _lavaNode;

        //timer to rotates activity
        private Timer _timerStatus;
        private Timer _timerRandomEvent;

        //private readonly List<string> listRandomListening = new List<string>() {
        //    "Otome wa Kyuu ni Tomarenai", "Kitto Chanto Onnanoko", "Ice Cream Child", "'Su' no Tsuku Koibito", "Merry-Go-Round"
        //};

        //bot console: https://discordapp.com/developers/applications/655668640502251530/information
        //https://docs.stillu.cc/guides/getting_started/terminology.html

        public async Task RunBotAsync()
        {
            client = new DiscordSocketClient(
                new DiscordSocketConfig(){ LogLevel = LogSeverity.Verbose }
            );
            commands = new CommandService();
            audioservice = new AudioService();
            //victoriaservice = new VictoriaService(new LavaNode(client,_lavaConfig),client);
            /// <summary>
            ///     Configures the application services.
            /// </summary>
            /// <returns>the service provider</returns>
            services = new ServiceCollection()
                // Discord
                .AddSingleton(client)
                .AddSingleton(commands)
                .AddSingleton(new InteractiveService(client))
                .AddSingleton(audioservice)
                //victoria
                .AddSingleton<LavaConfig>()
                .AddSingleton<LavaNode>()

                //// Request Caching for Lavalink
                //.AddSingleton<ILavalinkCache, LavalinkCache>()
                .BuildServiceProvider();

            client.Log += client_log;

            // do something .. don't forget disposing serviceProvider!
            Dispose();

            await RegisterCommandsAsync();
            await client.LoginAsync(TokenType.Bot, Config.Doremi.Token);
            await client.StartAsync();

            //start rotates activity
            _timerStatus = new Timer(async _ =>
            {
                List<string> listRandomPlaying = new List<string>() {
                    "at misora elementary school", "piano" , "at maho dou", "with steak" , 
                    "with friends", "with Hazuki", "with Aiko", "with Kotake",
                    "with Pop"
                };

                Random rnd = new Random();
                int rndIndex = rnd.Next(0, listRandomPlaying.Count); //random the list value
                String updLog = "Updated Doremi Activity - ";

                updLog += "Playing: " + listRandomPlaying.ElementAtOrDefault(rndIndex);
                await client.SetGameAsync(listRandomPlaying.ElementAtOrDefault(rndIndex), type: ActivityType.Playing); //set activity to current index position
                
                Console.WriteLine(updLog);
            },
            null,
            TimeSpan.FromSeconds(1), //time to wait before executing the timer for the first time (set first status)
            TimeSpan.FromHours(1) //time to wait before executing the timer again (set new status - repeats indifinitely every 10 seconds)
            );
            //end block

            _lavaNode = services.GetRequiredService<LavaNode>();
            victoriaservice = new VictoriaService(_lavaNode, client);
            client.UserJoined += AnnounceJoinedUser;
            client.UserLeft += AnnounceLeavingUser;
            client.MessageUpdated += MessageUpdated;
            

            client.Ready += () => {

                client.GetGuild(Config.Guild.Id).GetTextChannel(Config.Guild.Id_notif_online)
                .SendMessageAsync("Pretty Witchy Doremi Chi~");

                Console.WriteLine("Doremi Connected!");
                
                _lavaNode.ConnectAsync();

                //start rotates random event
                _timerStatus = new Timer(async _ =>
                {
                    List<string> listRandomEvent = new List<string>() {
                        $"<@{Config.Hazuki.Id}> let's go to maho dou",
                        $"<@{Config.Aiko.Id}> let's go to maho dou",
                        $"<@{Config.Hazuki.Id}> let's go to my house today",
                        $"<@{Config.Aiko.Id}> let's go to my house today",
                        $"Hii everyone, hope you all have a nice day and always be happy :smile:",
                        $"Hii everyone, please come visit our shop: maho dou :smile:",
                        $"Someone, please give me a big steak right now {Config.Emoji.drool}{Config.Emoji.steak}"
                    };

                    Random rnd = new Random();
                    int rndIndex = rnd.Next(0, listRandomEvent.Count); //random the list value
                    String updLog = "Doremi Random Event : " + listRandomEvent[rndIndex];
                    await client.GetGuild(Config.Guild.Id).GetTextChannel(Config.Guild.Id_notif_online)
                    .SendMessageAsync(listRandomEvent[rndIndex]);

                    Console.WriteLine(updLog);
                },
                null,
                TimeSpan.FromSeconds(Config.Doremi.Randomeventinterval), //time to wait before executing the timer for the first time
                TimeSpan.FromSeconds(Config.Doremi.Randomeventinterval) //time to wait before executing the timer again
                );

                return Task.CompletedTask;
            };

            //await _lavaNode.ConnectAsync();

            //SetTimer();

            //// Block this task until the program is closed.
            await Task.Delay(10);
        }

        
        /// <summary>
        ///     Stops the bot asynchronously.
        /// </summary>
        /// <returns>a task that represents the asynchronous operation</returns>
        public async Task StopAsync() => await client.StopAsync();

        /// <summary>
        ///     Unregisters the events attached to the discord client.
        /// </summary>
        public void Dispose() => client.MessageReceived -= HandleCommandAsync;

        //public async Task OnReadyAsync() => await _lavaNode.ConnectAsync();


        public async Task AnnounceJoinedUser(SocketGuildUser user) //Welcomes the new user
        {
            var channel = client.GetChannel(Config.Guild.Id_welcome) as SocketTextChannel; // Gets the channel to send the message in

            String[] arrRandomWelcomeMessage = {
               $"Hii there {user.Mention}, welcome to the {channel.Guild.Name}. We do hope you enjoying your stay.",
               $"Hello new friends: {user.Mention}, welcome to the {channel.Guild.Name}. We're expecting you to come and hopefully you're enjoying your stay."
            };

            String[] arrRandomPictures =
            {"https://66.media.tumblr.com/c8f9c5455355f8e522d52bacb8155ab0/tumblr_mswho8nWx11r98a5go1_400.gif",
            "https://thumbs.gfycat.com/DamagedGrouchyBarracuda-small.gif",
            "https://data.whicdn.com/images/39976659/original.gif"};

            Random rnd = new Random();
            int rndIndexWelcomeMessage = rnd.Next(0, arrRandomWelcomeMessage.GetLength(0));
            int rndIndexRandomPictures = rnd.Next(0, arrRandomPictures.GetLength(0));

            await channel.SendMessageAsync(arrRandomWelcomeMessage[rndIndexWelcomeMessage]+"\n"+
                "Don't forget to follow and read the rule guidelines.",
                embed: new EmbedBuilder()
                        .WithColor(Color.Red)
                        .WithImageUrl(arrRandomPictures[rndIndexRandomPictures])
                        .Build()); //Welcomes the new user

            //sending dm to the joined user
            var dmchannel = await user.GetOrCreateDMChannelAsync();
            await dmchannel.SendMessageAsync(arrRandomWelcomeMessage[rndIndexWelcomeMessage] + "\n" +
                "Don't forget to follow and read the rule guidelines.",
                embed: new EmbedBuilder()
                        .WithColor(Color.Red)
                        .WithImageUrl(arrRandomPictures[rndIndexRandomPictures])
                        .Build());
        }

        public async Task AnnounceLeavingUser(SocketGuildUser user)
        {
            String[] arrRandomLeavingMessage = {
               $":sob: Oh no, one of our friends: {user.Mention} has leaving the group and we are really sad about it.",
               $":sob: It's nice to have {user.Mention} on our group. I wish {user.Mention} can stay a bit longer."
            };

            String[] arrRandomPictures =
            {"https://media1.tenor.com/images/f51ff7041983283592e13e3e0c3b29b9/tenor.gif"};

            Random rnd = new Random();
            int rndIndexWelcomeMessage = rnd.Next(0, arrRandomLeavingMessage.GetLength(0));
            int rndIndexRandomPictures = rnd.Next(0, arrRandomPictures.GetLength(0));;

            var channel = client.GetChannel(Config.Guild.Id_welcome) as SocketTextChannel; // Gets the channel to send the message in

            await channel.SendMessageAsync(arrRandomLeavingMessage[rndIndexWelcomeMessage],
                embed: new EmbedBuilder()
                        .WithColor(Color.Red)
                        .WithImageUrl(arrRandomPictures[rndIndexRandomPictures])
                        .Build()); //Welcomes the new user

        }

        //private static void SetTimer()
        //{
        //    // Create a timer with a two second interval.
        //    randomEventTimer = new Timer(2000);
        //    // Hook up the Elapsed event for the timer. 
        //    randomEventTimer.Elapsed += OnTimedEvent;
        //    randomEventTimer.AutoReset = true;
        //    randomEventTimer.Enabled = true;
        //}

        //private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        //{
        //    Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}",
        //                      e.SignalTime);
        //}

        private async Task MessageUpdated(Cacheable<IMessage, ulong> before,
            SocketMessage after, ISocketMessageChannel channel)
        {
            // If the message was not in the cache, downloading it will result in getting a copy of `after`.
            var message = await before.GetOrDownloadAsync();
            Console.WriteLine($"{message} -> {after}");
        }

        public async Task RegisterCommandsAsync(){
            client.MessageReceived += HandleCommandAsync;
            await commands.AddModuleAsync(typeof(DoremiModule), services);
            await commands.AddModuleAsync(typeof(DoremiInteractive), services);
            await commands.AddModuleAsync(typeof(DoremiVictoriaMusic), services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message.Author.Id == Config.Doremi.Id) return;
            //if (message.Author.IsBot) return; //prevent any bot from sending the commands

            int argPos = 0;
            if (message.HasStringPrefix("doremi ", ref argPos) ||
                message.HasStringPrefix("do ", ref argPos) ||
                message.HasMentionPrefix(client.CurrentUser, ref argPos))
            {
                var context = new SocketCommandContext(client, message);
                var result = await commands.ExecuteAsync(context, argPos, services);
                switch (result.Error)
                {
                    case CommandError.BadArgCount:
                        await context.Channel.SendMessageAsync("Oops, looks like you have missing argument. See ``doremi help`` for more info.");
                        break;
                    case CommandError.UnknownCommand:
                        await message.Channel.SendMessageAsync($"Sorry, I can't seem to understand your commands. See ``doremi help`` for more info.",
                        embed: new EmbedBuilder()
                        .WithColor(Color.Red)
                        .WithImageUrl("https://media1.tenor.com/images/3ba7d829ec2fd5300b0f3a16a86a7af8/tenor.gif")
                        .Build());
                        Console.WriteLine(result.ErrorReason);
                        break;
                    case CommandError.Exception:
                        // This is what happens instead of the catch block.
                        await message.Channel.SendMessageAsync($"Sorry, I can't seem to understand your commands. See ``doremi help`` for more info.");
                        Console.WriteLine(result.ErrorReason);
                        break;
                }

                //if (!result.IsSuccess)
                //{
                //    
                //}
            }
        }

        public async Task ReactAsync(SocketUserMessage userMsg, string emoteName)
        {
            var emote = client.Guilds
                    .SelectMany(x => x.Emotes)
                    .FirstOrDefault(x => x.Name.IndexOf(
                        emoteName, StringComparison.OrdinalIgnoreCase) != -1);
            if (emote == null) return;
            await userMsg.AddReactionAsync(emote);

            // equivalent to "👌"
            var emoji = new Emoji("\uD83D\uDC4C");
            await userMsg.AddReactionAsync(emoji);
        }

        private Task client_log(LogMessage msg)
        {
            Console.WriteLine("Doremi: " + msg.ToString());
            return Task.CompletedTask;
        }
            

    }
}
