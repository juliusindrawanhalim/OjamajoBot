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
using Discord.Addons.Interactive;

namespace OjamajoBot.Bot
{
    class Momoko
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
                .AddSingleton(new InteractiveService(client))
                .AddSingleton(audioservice)
                .BuildServiceProvider();

            client.Log += client_log;

            await RegisterCommandsAsync();
            await client.LoginAsync(TokenType.Bot, Config.Momoko.Token);
            await client.StartAsync();

            client.JoinedGuild += JoinedGuild;

            //start rotates random activity
            _timerStatus = new Timer(async _ =>
            {
                Random rnd = new Random();
                int rndIndex = rnd.Next(0, Config.Momoko.arrRandomActivity.GetLength(0)); //random the list value
                //if (rndIndex > 0) rndIndex -= 1;
                string updLog = "Updated Momoko Activity - Playing: " + Config.Momoko.arrRandomActivity[rndIndex, 0];
                Config.Momoko.indexCurrentActivity = rndIndex;
                await client.SetGameAsync(Config.Momoko.arrRandomActivity[rndIndex, 0], type: ActivityType.Playing); //set activity to current index position

                Console.WriteLine(updLog);
            },
            null,
            TimeSpan.FromSeconds(1), //time to wait before executing the timer for the first time (set first status)
            TimeSpan.FromMinutes(10) //time to wait before executing the timer again (set new status - repeats indifinitely every 10 seconds)
            );
            //end block

            client.Ready += () =>
            {
                Console.WriteLine("Momoko Connected!");
                return Task.CompletedTask;
            };


            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        public async Task JoinedGuild(SocketGuild guild)
        {
            var systemChannel = client.GetChannel(guild.SystemChannel.Id) as SocketTextChannel; // Gets the channel to send the message in
            await systemChannel.SendMessageAsync($"Pretty witchy {MentionUtils.MentionUser(Config.Momoko.Id)} chi~ has arrived to the {guild.Name}. Hello everyone, please to meet you all. Thank you for inviting me up. " +
                $"You can ask me with `{Config.Momoko.PrefixParent[0]}help` for all commands list.",
            embed: new EmbedBuilder()
            .WithColor(Config.Momoko.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/3/3d/09.07.JPG")
            .Build());
        }

        public async Task RegisterCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;
            await commands.AddModuleAsync(typeof(MomokoModule), services);
            await commands.AddModuleAsync(typeof(MomokoMagicalStageModule), services);
            await commands.AddModuleAsync(typeof(MomokoRandomEventModule), services);
            await commands.AddModuleAsync(typeof(MomokoBakery), services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message.Author.Id == Config.Momoko.Id) return;
            //if (message.Author.IsBot) return; //prevent any bot from sending the commands

            int argPos = 0;
            if (message.HasStringPrefix(Config.Momoko.PrefixParent[0], ref argPos) ||
                message.HasStringPrefix(Config.Momoko.PrefixParent[1], ref argPos) ||
                message.HasMentionPrefix(client.CurrentUser, ref argPos))
            {
                var context = new SocketCommandContext(client, message);
                var result = await commands.ExecuteAsync(context, argPos, services);
                switch (result.Error)
                {
                    case CommandError.BadArgCount:
                        await context.Channel.SendMessageAsync($"Oops, looks like you have missing/too much parameter. " +
                            $"See `{Config.Momoko.PrefixParent[0]}help` for command help.");
                        break;
                    case CommandError.UnknownCommand:
                        await message.Channel.SendMessageAsync($"Sorry, I can't seems to understand your commands. " +
                            $"See `{Config.Momoko.PrefixParent[0]}help` for command help.",
                        embed: new EmbedBuilder()
                        .WithColor(Config.Momoko.EmbedColor)
                        .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/52/ODN-EP9-025.png")
                        .Build());
                        Console.WriteLine(result.ErrorReason);
                        break;
                    case CommandError.ObjectNotFound:
                        await message.Channel.SendMessageAsync($"Oops, {result.ErrorReason} " +
                            $"See `{Config.Momoko.PrefixParent[0]}help` for command help.");
                        break;
                    case CommandError.ParseFailed:
                        await message.Channel.SendMessageAsync($"Oops, {result.ErrorReason} " +
                            $"See `{Config.Momoko.PrefixParent[0]}help` for command help.");
                        break;
                }

            }
        }

        private Task client_log(LogMessage msg)
        {
            Console.WriteLine("Momoko: " + msg.ToString());
            return Task.CompletedTask;
        }

    }
}
