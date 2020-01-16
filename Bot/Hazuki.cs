using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using OjamajoBot.Module;
using OjamajoBot.Service;
using System.Threading;

namespace OjamajoBot.Bot
{
    class Hazuki
    {
        private CommandService commands;
        private IServiceProvider services;

        private DiscordSocketClient client;

        private AudioService audioservice;

        //timer to rotates activity
        private Timer _timerStatus;

        public async Task RunBotAsync()
        {
            client = new DiscordSocketClient(
                new DiscordSocketConfig() { LogLevel = LogSeverity.Verbose }
            );
            commands = new CommandService();
            audioservice = new AudioService();
            services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .AddSingleton(audioservice)
                .BuildServiceProvider();

            client.Log += client_log;

            await RegisterCommandsAsync();
            await client.LoginAsync(TokenType.Bot, Config.Hazuki.Token);
            await client.StartAsync();

            client.MessageUpdated += MessageUpdated;
            //client.GuildAvailable += GuildAvailable;

            //start rotates random activity
            _timerStatus = new Timer(async _ =>
            {
                Random rnd = new Random();
                int rndIndex = rnd.Next(0, Config.Hazuki.arrRandomActivity.GetLength(0)); //random the list value
                if (rndIndex > 0) rndIndex -= 1;
                String updLog = "Updated Hazuki Activity - Playing: " + Config.Hazuki.arrRandomActivity[rndIndex, 0];
                Config.Hazuki.indexCurrentActivity = rndIndex;
                await client.SetGameAsync(Config.Hazuki.arrRandomActivity[rndIndex, 0], type: ActivityType.Playing); //set activity to current index position

                Console.WriteLine(updLog);
            },
            null,
            TimeSpan.FromSeconds(1), //time to wait before executing the timer for the first time (set first status)
            TimeSpan.FromMinutes(10) //time to wait before executing the timer again (set new status - repeats indifinitely every 10 seconds)
            );
            //end block

            client.Ready += () =>
            {

                //client.GetGuild(Config.Guild.Id).GetTextChannel(Config.Guild.Id_notif_online)
                //.SendMessageAsync("Pretty Witchy Hazuki Chi~");

                Console.WriteLine("Hazuki Connected!");
                return Task.CompletedTask;
            };


            //// Block this task until the program is closed.
            await Task.Delay(0);

        }

        //private async Task GuildAvailable(SocketGuild guild)
        //{
        //    if (Config.Guild.Id_notif_online.ContainsKey(guild.Id.ToString()))
        //    { //announce bot if online
        //        try
        //        {
        //            await client.GetGuild(guild.Id)
        //            .GetTextChannel(Config.Guild.Id_notif_online[guild.Id.ToString()])
        //            .SendMessageAsync("Pretty Witchy Hazuki Chi~");
        //        }
        //        catch { }
        //    }
        //}


        private async Task MessageUpdated(Cacheable<IMessage, ulong> before,
            SocketMessage after, ISocketMessageChannel channel)
        {
            // If the message was not in the cache, downloading it will result in getting a copy of `after`.
            var message = await before.GetOrDownloadAsync();
            Console.WriteLine($"{message} -> {after}");
        }

        public async Task RegisterCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;
            await commands.AddModuleAsync(typeof(HazukiModule), services);
            await commands.AddModuleAsync(typeof(HazukiMagicalStageModule), services);
            await commands.AddModuleAsync(typeof(HazukiRandomEventModule), services);
            //await commands.AddModuleAsync(typeof(HazukiMusic), services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(client, message);
            if (message.Author.Id == Config.Hazuki.Id) return;
            //if (message.Author.IsBot) return; //prevent any bot from sending the commands

            int argPos = 0;
            if (message.HasStringPrefix(Config.Hazuki.PrefixParent[0], ref argPos) ||
                message.HasStringPrefix(Config.Hazuki.PrefixParent[1], ref argPos) ||
                message.HasMentionPrefix(client.CurrentUser, ref argPos)){
                
                var result = await commands.ExecuteAsync(context, argPos, services);
                switch (result.Error)
                {
                    case CommandError.BadArgCount:
                        await context.Channel.SendMessageAsync("I'm sorry, looks like you have missing/too much parameter. " +
                            $"See `{Config.Hazuki.PrefixParent[0]}help <commands or category>`for command help.");
                        break;
                    case CommandError.UnknownCommand:
                        await message.Channel.SendMessageAsync("I'm sorry, I can't seem to understand your commands. " +
                            $"See `{Config.Hazuki.PrefixParent[0]}help <commands or category>`for command help.",
                        embed: new EmbedBuilder()
                        .WithColor(Config.Hazuki.EmbedColor)
                        .WithImageUrl("https://33.media.tumblr.com/28c2441a5655ecb1bd23df8275f3598f/tumblr_nfkjtbSQZg1r98a5go1_500.gif")
                        .Build());
                        Console.WriteLine(result.ErrorReason);
                        break;
                    case CommandError.ObjectNotFound:
                        await message.Channel.SendMessageAsync($"Oops, {result.ErrorReason} " +
                            $"See `{Config.Hazuki.PrefixParent[0]}help <commands or category>`for command help.");
                        break;
                    case CommandError.ParseFailed:
                        await message.Channel.SendMessageAsync($"Oops, {result.ErrorReason} " +
                            $"See `{Config.Hazuki.PrefixParent[0]}help <commands or category>`for command help.");
                        break;
                }

            }
        }

        private Task client_log(LogMessage msg)
        {
            Console.WriteLine("Hazuki: " + msg.ToString());
            return Task.CompletedTask;
        }

    }
}
