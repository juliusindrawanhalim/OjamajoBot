﻿using System;
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
using Newtonsoft.Json.Linq;

namespace OjamajoBot.Bot
{
    public class Doremi
    {
        public CommandService commands;
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

        //private readonly List<string> listRandomListening = new List<string>() {
        //    "Otome wa Kyuu ni Tomarenai", "Kitto Chanto Onnanoko", "Ice Cream Child", "'Su' no Tsuku Koibito", "Merry-Go-Round"
        //};

        //bot console: https://discordapp.com/developers/applications/655668640502251530/information
        //https://docs.stillu.cc/guides/getting_started/terminology.html

        public async Task RunBotAsync()
        {
            client = new DiscordSocketClient(
                new DiscordSocketConfig() { LogLevel = LogSeverity.Verbose }
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
                // Request Caching for Lavalink
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
                Random rnd = new Random();
                int rndIndex = rnd.Next(0, Config.Doremi.arrRandomActivity.GetLength(0)); //random the list value
                //if (rndIndex > 0) rndIndex -= 1;
                string updLog = "Updated Doremi Activity - Playing: " + Config.Doremi.arrRandomActivity[rndIndex, 0];
                Config.Doremi.indexCurrentActivity = rndIndex;
                await client.SetGameAsync(Config.Doremi.arrRandomActivity[rndIndex,0], type: ActivityType.Playing); //set activity to current index position
                Console.WriteLine(updLog);
            },
            null,
            TimeSpan.FromSeconds(1), //time to wait before executing the timer for the first time (set first status)
            TimeSpan.FromMinutes(10) //time to wait before executing the timer again (set new status - repeats indifinitely every 10 seconds)
            );
            //end block

            _lavaNode = services.GetRequiredService<LavaNode>();
            victoriaservice = new VictoriaService(_lavaNode, client);
            client.UserJoined += AnnounceJoinedUser;
            client.UserLeft += AnnounceLeavingUser;
            client.MessageReceived += MessageReceived;
            client.MessageUpdated += MessageUpdated;
            client.GuildAvailable += GuildAvailable;
            client.JoinedGuild += JoinedGuild;
            client.LeftGuild += LeftGuild;
            client.ReactionAdded += HandleReactionAddedAsync;
            client.ReactionRemoved += HandleReactionRemovedAsync;

            //Console.WriteLine(iguildchannel.Id);

            client.Ready += () => {

                Console.WriteLine("Doremi Connected!");
                _lavaNode.ConnectAsync();

                return Task.CompletedTask;
            };

            //// Block this task until the program is closed.
            await Task.Delay(0);
        }


        /// <summary>
        ///     Stops the bot asynchronously.
        /// </summary>
        /// <returns>a task that represents the asynchronous operation</returns>
        public async Task StopAsync() => await client.StopAsync();

        public async Task LeftGuild(SocketGuild guild)
        {
            Console.WriteLine($"Bot lefted from: {guild.Name}");
            Config.Guild.remove(guild.Id.ToString());
        }

        public async Task JoinedGuild(SocketGuild guild)
        {
            //Config.Music.storedLavaTrack[guild.Id.ToString()] = new List<LavaTrack>();
            Config.Music.queuedTrack[guild.Id.ToString()] = new List<string>();
            Console.WriteLine($"Doremi Bot joined into: {guild.Name}");
            Config.Guild.init(guild.Id);
        }

        public async Task GuildAvailable(SocketGuild guild)
        {
            Config.Guild.init(guild.Id);
            Config.Music.queuedTrack[guild.Id.ToString()] = new List<string>();
            //Config.Music.storedLavaTrack[guild.Id.ToString()] = new List<LavaTrack>();

            //if (Config.Guild.Id_notif_online.ContainsKey(guild.Id.ToString()))
            //{ //announce bot if online
            //    try{
            //        await client.GetGuild(guild.Id)
            //        .GetTextChannel(Config.Guild.Id_notif_online[guild.Id.ToString()])
            //        .SendMessageAsync("Pretty Witchy Doremi Chi~");
            //    } catch {
            //        Console.WriteLine($"Doremi Online Notification Exception: Send message permissions {guild.Name}");
            //    }
                
            //}
            

            if (Config.Guild.Id_random_event.ContainsKey(guild.Id.ToString()))
            { 
                //start rotates random event
                Config.Doremi._timerRandomEvent[$"{guild.Id.ToString()}"] = new Timer(async _ =>
                {
                    Random rnd = new Random();
                    int rndIndex = rnd.Next(0, Config.Doremi.listRandomEvent.Count); //random the list value
                    Console.WriteLine("Doremi Random Event : " + Config.Doremi.listRandomEvent[rndIndex]);

                    try
                    {
                        await client
                        .GetGuild(guild.Id)
                        .GetTextChannel(Config.Guild.Id_random_event[guild.Id.ToString()])
                        .SendMessageAsync(Config.Doremi.listRandomEvent[rndIndex]);
                    } catch {
                        Console.WriteLine($"Doremi Random Event Exception: Send message permissions has been missing from {guild.Name}");
                    }
                },
                null,
                TimeSpan.FromHours(Config.Doremi.Randomeventinterval), //time to wait before executing the timer for the first time
                TimeSpan.FromHours(Config.Doremi.Randomeventinterval) //time to wait before executing the timer again
                );
            }


            //var channel = client.GetChannel(guild.SystemChannel.Id) as SocketTextChannel;
            //await channel.SendMessageAsync(guild.SystemChannel.Id.ToString());
        }

        /// <summary>
        ///     Unregisters the events attached to the discord client.
        /// </summary>
        public void Dispose() => client.MessageReceived -= HandleCommandAsync;

        public async Task AnnounceJoinedUser(SocketGuildUser user) //Welcomes the new user
        {
            var channel = client.GetChannel(user.Guild.SystemChannel.Id) as SocketTextChannel; // Gets the channel to send the message in

            if(user.Id != Config.Hazuki.Id && user.Id != Config.Aiko.Id &&
                user.Id != Config.Onpu.Id && user.Id!= Config.Momoko.Id) { 
                string[] arrRandomWelcomeMessage = {
                    $"Hii there {user.Mention}, welcome to the {channel.Guild.Name}. We hope you enjoy and happy with all of us.",
                    $"Hello {user.Mention}, welcome to the {channel.Guild.Name}. We're really happy that you join our group.",
                    $"Hello new friends: {user.Mention}, welcome to the {channel.Guild.Name}. " +
                    $"We're expecting you to come and hopefully you're enjoying your stay on the group."
                };

                string[] arrRandomPictures =
                {"https://66.media.tumblr.com/c8f9c5455355f8e522d52bacb8155ab0/tumblr_mswho8nWx11r98a5go1_400.gif",
                "https://thumbs.gfycat.com/DamagedGrouchyBarracuda-small.gif",
                "https://data.whicdn.com/images/39976659/original.gif"};

                int rndIndexWelcomeMessage = new Random().Next(0, arrRandomWelcomeMessage.GetLength(0));
                int rndIndexRandomPictures = new Random().Next(0, arrRandomPictures.GetLength(0));

                await channel.SendMessageAsync(arrRandomWelcomeMessage[rndIndexWelcomeMessage] +
                    " Please do introduce yourself, also don't forget to always follow and read the rule guidelines :smile:",
                    embed: new EmbedBuilder()
                            .WithColor(Config.Doremi.EmbedColor)
                            .WithImageUrl(arrRandomPictures[rndIndexRandomPictures])
                            .Build());

                //sending dm to the joined user
                var dmchannel = await user.GetOrCreateDMChannelAsync();
                await dmchannel.SendMessageAsync(arrRandomWelcomeMessage[rndIndexWelcomeMessage] + 
                    " Please do introduce yourself on the group, also don't forget to always follow and read the rule guidelines :smile:",
                    embed: new EmbedBuilder()
                            .WithColor(Config.Doremi.EmbedColor)
                            .WithImageUrl(arrRandomPictures[rndIndexRandomPictures])
                            .Build());
            }

            
        }

        public async Task AnnounceLeavingUser(SocketGuildUser user) //Send a leaving user notifications
        {
            var channel = client.GetChannel(user.Guild.SystemChannel.Id) as SocketTextChannel; // Gets the channel to send the message in

            JObject guildConfig = JObject.Parse(File.ReadAllText($"config/{user.Guild.Id}.json"));
            if (guildConfig.GetValue("user_leaving_notification").ToString() == "1"){
                string[] arrRandomLeavingMessage = {
               $":sob: Oh no, {user.Mention} has leave the {channel.Guild.Name} and we are really sad.",
               $":sob: It's nice to have {user.Mention} on {channel.Guild.Name}. We wish that you can stay more longer on our groups."
            };

                string[] arrRandomPictures = {"https://vignette.wikia.nocookie.net/ojamajowitchling/images/d/d7/ODN-EP1-013.png",
            "https://vignette.wikia.nocookie.net/ojamajowitchling/images/d/d9/04.51.12.JPG"};

                
                await channel.SendMessageAsync(arrRandomLeavingMessage[new Random().Next(0, arrRandomLeavingMessage.GetLength(0))],
                    embed: new EmbedBuilder()
                            .WithColor(Config.Doremi.EmbedColor)
                            .WithImageUrl(arrRandomPictures[new Random().Next(0, arrRandomPictures.GetLength(0))])
                            .Build()); //Welcomes the new user
            }

            
        }

        public void HookReactionAdded(BaseSocketClient client) => client.ReactionAdded += HandleReactionAddedAsync;
        public void HookReactionRemoved(BaseSocketClient client) => client.ReactionRemoved += HandleReactionRemovedAsync;

        public async Task HandleReactionAddedAsync(Cacheable<IUserMessage, ulong> cachedMessage,
        ISocketMessageChannel originChannel, SocketReaction reaction)
        {
            //https://discordapp.com/channels/81384788765712384/381889909113225237/643100808916893716
            var message = await cachedMessage.GetOrDownloadAsync();
            //var context = new SocketCommandContext(client, cachedMessage);

            if (message != null && reaction.User.IsSpecified)
            if (reaction.Emote.Equals(new Emoji("\u2B50"))){
                if (message.Reactions.TryGetValue(new Emoji("\u2B50"), out var metadata))
                {
                    if (message.Author.Id == Config.Doremi.Id && metadata.ReactionCount >= 5 && !message.IsPinned)
                    {
                        string splittedMentionedId = (message.Content.ToString().Split(">")[0]).Replace("<@!", "");

                        var getMentionedUserId = Int64.Parse(splittedMentionedId);

                        await client_log(new LogMessage(0, "Reaction pinned:", $"{message.Author}'s : {message.Id} has enough reactions."));
                        var channel = client.GetChannel(originChannel.Id) as SocketTextChannel;
                        await originChannel.SendMessageAsync($"{Config.Emoji.clap} Congratulations, I will now pin " +
                            $"{MentionUtils.MentionUser((ulong) getMentionedUserId)}'s message.",
                        embed: new EmbedBuilder()
                        .WithColor(Config.Doremi.EmbedColor)
                        .WithImageUrl("https://static.zerochan.net/Harukaze.Doremi.full.2494232.gif")
                        .Build());
                        await message.PinAsync();
                    }
                }

                //Console.WriteLine(reactionstars);

                //Console.WriteLine($"{reaction.User.Value} just added a reaction '{reaction.Emote}' " +
                //$"to {message.Author}'s message ({message.Id}).");
                    
            }
                
        }
        
        public async Task HandleReactionRemovedAsync(Cacheable<IUserMessage, ulong> cachedMessage,
        ISocketMessageChannel originChannel, SocketReaction reaction)
        {
            var message = await cachedMessage.GetOrDownloadAsync();
            //if (message != null && reaction.User.IsSpecified)
            //    Console.WriteLine($"{reaction.User.Value} just remove a reaction '{reaction.Emote}' " +
            //                        $"to {message.Author}'s message ({message.Id}).");
        }

        private Task MessageReceived(SocketMessage message)
        {
            // check if the message is a user message as opposed to a system message (e.g. Clyde, pins, etc.)
            if (!(message is SocketUserMessage userMessage)) return Task.CompletedTask;
            // check if the message origin is a guild message channel
            if (!(userMessage.Channel is SocketTextChannel textChannel)) return Task.CompletedTask;

            SocketTextChannel textchannel = (SocketTextChannel)message.Channel;
            SocketUserMessage usrmsg = (SocketUserMessage) message;

            try
            {
                using (StreamWriter sw = (File.Exists($"logs/{textchannel.Guild.Id}/{DateTime.Now.ToString("yyyy_MM_dd")}.txt")) ? File.AppendText($"logs/{textchannel.Guild.Id}/{DateTime.Now.ToString("yyyy_MM_dd")}.txt") : 
                    File.CreateText($"logs/{textchannel.Guild.Id}/{DateTime.Now.ToString("yyyy_MM_dd")}.txt"))
                    sw.WriteLine($"[{DateTime.Now.ToString("yyyy/MM/dd HH:mm")}] {usrmsg.Author.Mention}{usrmsg.Author.Username} : {message}");
            } catch(Exception e) {}
            
            return Task.CompletedTask;
        }

        private Task MessageUpdated(Cacheable<IMessage, ulong> before,
            SocketMessage after, ISocketMessageChannel channel)
        {

            // check if the message is a user message as opposed to a system message (e.g. Clyde, pins, etc.)
            if (!(after is SocketUserMessage userMessage)) return Task.CompletedTask;
            // check if the message origin is a guild message channel
            if (!(after.Channel is SocketTextChannel textChannel)) return Task.CompletedTask;

            SocketTextChannel textchannel = (SocketTextChannel)after.Channel;
            SocketUserMessage usrmsg = (SocketUserMessage)after;

            // If the message was not in the cache, downloading it will result in getting a copy of `after`.
            try {
                var message = before.GetOrDownloadAsync();
                
                using (StreamWriter sw = (File.Exists($"logs/{textchannel.Guild.Id}/{DateTime.Now.ToString("yyyy_MM_dd")}.txt")) ? File.AppendText($"logs/{textchannel.Guild.Id}/{DateTime.Now.ToString("yyyy_MM_dd")}.txt") :
                    File.CreateText($"logs/{textchannel.Guild.Id}/{DateTime.Now.ToString("yyyy_MM_dd")}.txt"))
                    sw.WriteLine($"[{DateTime.Now.ToString("yyyy/MM/dd HH:mm")}] {usrmsg.Author.Mention}{usrmsg.Author.Username} : [update]{after}");
            } catch(Exception e) {}
            return Task.CompletedTask;
            //using (StreamWriter w = File.AppendText($"attachments/{Context.Guild.Id}/feedback_{Context.Guild.Id}.txt"))
            //    w.WriteLine($"[{DateTime.Now.ToString("MM/dd/yyyy HH:mm")}]{message}:{feedback_message}");
        }

        public async Task RegisterCommandsAsync(){
            //commands.CommandExecuted += OnCommandExecutedAsync;
            client.MessageReceived += HandleCommandAsync;
            
            await commands.AddModuleAsync(typeof(DoremiModule), services);
            await commands.AddModuleAsync(typeof(DoremiVictoriaMusic), services);
            await commands.AddModuleAsync(typeof(DoremiInteractive), services);
            await commands.AddModuleAsync(typeof(DoremiModerator), services);
            //await commands.AddModuleAsync(typeof(DoremiModeratorChannels), services);
            await commands.AddModuleAsync(typeof(DoremiMagicalStageModule), services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            SocketCommandContext context = context = new SocketCommandContext(client, message);

            if (message.Author.Id == Config.Doremi.Id) return;
            //if (message.Author.IsBot) return; //prevent any bot from sending the commands

            int argPos = 0;

            if (message.HasStringPrefix(Config.Doremi.PrefixParent[0], ref argPos) ||
                message.HasStringPrefix(Config.Doremi.PrefixParent[1], ref argPos) ||
                message.HasMentionPrefix(client.CurrentUser, ref argPos))
            {
                var result = await commands.ExecuteAsync(context, argPos, services);
                string[] splittedString = context.Message.Content.Split("!");
                //var removedFirstPrefix = string.Join("!",context.Message.Content.Split().Skip(1));

                if (message.ToString().Contains($"<@!{Config.Doremi.Id}> mod")  ||
                    message.ToString().Contains($"<@!{Config.Doremi.Id}> mod channels") ||
                    splittedString[1].StartsWith("mod ")||
                    splittedString[1].StartsWith("mod channels"))
                { //executed by moderator commands
                    switch (result.Error)
                    {
                        case CommandError.BadArgCount:
                            await context.Channel.SendMessageAsync($"Moderator Commands: you have missing/many parameter. " +
                                $"See `{Config.Doremi.PrefixParent[0]}help mod <commands or category>` for commands help.");
                            break;
                        case CommandError.UnknownCommand:
                            await message.Channel.SendMessageAsync($"Moderator Commands: Sorry, I can't seem to understand your commands. " +
                               $"See `{Config.Doremi.PrefixParent[0]}help mod <commands or category>` for commands help.");
                            Console.WriteLine(result.ErrorReason);
                            break;
                        case CommandError.ObjectNotFound:
                            await message.Channel.SendMessageAsync($"Moderator Commands: Oops, {result.ErrorReason} " +
                                $"See `{Config.Doremi.PrefixParent[0]}help mod <commands or category>` for commands help.");
                            break;
                        case CommandError.ParseFailed:
                            await message.Channel.SendMessageAsync(result.ErrorReason);
                            break;
                        case CommandError.UnmetPrecondition:
                            await message.Channel.SendMessageAsync(result.ErrorReason);
                            break;
                    }
                } else {
                    switch (result.Error)
                    {
                        case CommandError.BadArgCount:
                            await context.Channel.SendMessageAsync("Oops, looks like you have missing/too much parameter. " +
                                $"See `{Config.Doremi.PrefixParent[0]}help <commands or category>`for commands help.");
                            break;
                        case CommandError.UnknownCommand:
                            await message.Channel.SendMessageAsync("Ehh? I can't seem to understand your commands. " +
                                $"See `{Config.Doremi.PrefixParent[0]}help <commands or category>` for commands help.",
                            embed: new EmbedBuilder()
                            .WithColor(Config.Doremi.EmbedColor)
                            .WithImageUrl("https://media1.tenor.com/images/3ba7d829ec2fd5300b0f3a16a86a7af8/tenor.gif")
                            .Build());
                            break;
                        case CommandError.ObjectNotFound:
                            await message.Channel.SendMessageAsync($"Oops, {result.ErrorReason} " +
                                $"See `{Config.Doremi.PrefixParent[0]}help <commands or category>` for commands help.");
                            break;
                        case CommandError.ParseFailed:
                            await message.Channel.SendMessageAsync($"Oops, {result.ErrorReason} " +
                                $"See `{Config.Doremi.PrefixParent[0]}help <commands or category>` for commands help.");
                            break;
                        case CommandError.Exception:
                            // This is what happens instead of the catch block.
                            //await message.Channel.SendMessageAsync($"Sorry, I can't seem to understand your commands. See ``doremi help`` for more info.");
                            Console.WriteLine(result.ErrorReason);
                            break;
                    }
                }

                return;
            }
        }

        private Task client_log(LogMessage msg)
        {
            Console.WriteLine("Doremi: " + msg.ToString());
            return Task.CompletedTask;
        }
        
    }
}