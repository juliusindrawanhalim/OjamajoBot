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

            //start rotates random activity
            _timerStatus = new Timer(async _ =>
            {
                List<string> listRandomPlaying = new List<string>() {
                    "at misora elementary school", "at maho dou", "violin" ,
                    "with friends", "with Doremi", "with Aiko", "with Masaru Yada"
                };

                Random rnd = new Random();
                int rndIndex = rnd.Next(0, listRandomPlaying.Count); //random the list value
                String updLog = "Updated Hazuki Activity - Playing: " + listRandomPlaying.ElementAtOrDefault(rndIndex);
                await client.SetGameAsync(listRandomPlaying.ElementAtOrDefault(rndIndex), type: ActivityType.Playing); //set activity to current index position
                
                Console.WriteLine(updLog);
            },
            null,
            TimeSpan.FromSeconds(1), //time to wait before executing the timer for the first time (set first status)
            TimeSpan.FromHours(1) //time to wait before executing the timer again (set new status - repeats indifinitely every 10 seconds)
            );
            //end block

            client.Ready += () =>
            {
                client.GetGuild(Config.Guild.Id).GetTextChannel(Config.Guild.Id_notif_online)
                .SendMessageAsync("Pretty Witchy Hazuki Chi~");

                Console.WriteLine("Hazuki Connected!");
                return Task.CompletedTask;
            };


            //// Block this task until the program is closed.
            await Task.Delay(10);

        }

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
            await commands.AddModuleAsync(typeof(HazukiRandomEventModule), services);
            //await commands.AddModuleAsync(typeof(HazukiMusic), services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message.Author.Id == Config.Hazuki.Id) return;
            //if (message.Author.IsBot) return; //prevent any bot from sending the commands

            int argPos = 0;
            if (message.HasStringPrefix("hazuki ", ref argPos) ||
                message.HasStringPrefix("ha ", ref argPos) ||
                message.HasMentionPrefix(client.CurrentUser, ref argPos))
            {
                var context = new SocketCommandContext(client, message);
                var result = await commands.ExecuteAsync(context, argPos, services);
                if (!result.IsSuccess)
                {
                    await message.Channel.SendMessageAsync("Sorry, I can't seem to understand your commands.");
                    Console.WriteLine(result.ErrorReason);
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
