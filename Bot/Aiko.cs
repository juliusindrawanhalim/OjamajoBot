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
    class Aiko
    {
        private CommandService commands;
        private IServiceProvider services;

        public static DiscordSocketClient client;

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
                //.AddSingleton(new ReliabilityService(client))
                //.AddSingleton(audioservice)
                .BuildServiceProvider();

            client.Log += client_log;

            // do something .. don't forget disposing serviceProvider!
            Dispose();

            await RegisterCommandsAsync();
            await client.LoginAsync(TokenType.Bot, Config.Aiko.Token);
            await client.StartAsync();

            client.JoinedGuild += JoinedGuild;
            client.GuildAvailable += GuildAvailable;

            //start rotates random activity
            _timerStatus = new Timer(async _ =>
            {
                var returnStatusActivity = Config.Core.BotStatus.checkStatusActivity(Config.Core.BotClass.Aiko,
                    Config.Aiko.Status.arrRandomActivity);

                var returnObjectActivity = returnStatusActivity.Item1;
                Config.Aiko.Status.currentActivity = returnObjectActivity[0].ToString();
                Config.Aiko.Status.currentActivityReply = returnObjectActivity[1].ToString();
                await client.SetGameAsync(Config.Aiko.Status.currentActivity);
                await client.SetStatusAsync((UserStatus)returnObjectActivity[2]);

            },
            null,
            TimeSpan.FromSeconds(1), //time to wait before executing the timer for the first time (set first status)
            TimeSpan.FromMinutes(10) //time to wait before executing the timer again (set new status - repeats indifinitely every 10 seconds)
            );
            //end block

            client.Ready += () =>
            {
                //client.GetGuild(Config.Guild.Id).GetTextChannel(Config.Guild.Id_notif_online)
                //.SendMessageAsync("Pretty Witchy Aiko Chi~");

                Console.WriteLine("Aiko Connected!");
                //new Onpu().RunBotAsync().GetAwaiter().GetResult();
                return Task.CompletedTask;
            };


            //// Block this task until the program is closed.
            await Task.Delay(3000);
        }

        public void Dispose() => client.MessageReceived -= HandleCommandAsync;

        public async Task JoinedGuild(SocketGuild guild)
        {
            var systemChannel = client.GetChannel(guild.SystemChannel.Id) as SocketTextChannel; // Gets the channel to send the message in
            await systemChannel.SendMessageAsync($"Pretty witchy {MentionUtils.MentionUser(Config.Aiko.Id)} chi~ has arrived to the {guild.Name}. " +
                $"Yo everyone, thank you very much for inviting me up. " +
                $"You can ask me with `{Config.Aiko.PrefixParent[0]}help` for all commands list.",
            embed: new EmbedBuilder()
            .WithColor(Config.Aiko.EmbedColor)
            .WithImageUrl("https://i.pinimg.com/originals/5c/b4/8f/5cb48f3c6fb6477d0c1f423895547683.png")
            .Build());
        }

        public async Task GuildAvailable(SocketGuild guild)
        {
            //set hazuki birthday announcement timer
            if (Config.Guild.hasPropertyValues(guild.Id.ToString(), "id_birthday_announcement"))
            {
                //Config.Aiko._timerBirthdayAnnouncement[guild.Id.ToString()] = new Timer(async _ =>
                //{
                //    //announce doremi birthday
                //    if (Config.Doremi.Status.isBirthday(guild.Id))
                //    {
                //        await client
                //        .GetGuild(guild.Id)
                //        .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guild.Id, "id_birthday_announcement")))
                //        .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday, {MentionUtils.MentionUser(Config.Doremi.Id)} chan. " +
                //        $"She has turned into {Config.Doremi.birthdayCalculatedYear} on this year. Let's give some big steak and wonderful birthday wishes for her.",
                //        embed: new EmbedBuilder()
                //        .WithColor(Config.Aiko.EmbedColor)
                //        .WithImageUrl(Config.Doremi.DoremiBirthdayCakeImgSrc)
                //        .Build());
                //    }
                //},
                //null,
                //TimeSpan.FromSeconds(10), //time to wait before executing the timer for the first time
                //TimeSpan.FromHours(24) //time to wait before executing the timer again
                //);
            }
        }

        public async Task RegisterCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;
            await commands.AddModuleAsync(typeof(AikoModule), services);
            await commands.AddModuleAsync(typeof(AikoMagicalStageModule), services);
            await commands.AddModuleAsync(typeof(AikoRandomEventModule), services);
            await commands.AddModuleAsync(typeof(AikoMinigameInteractive), services);
            await commands.AddModuleAsync(typeof(AikoTradingCardInteractive), services);
            //await commands.AddModuleAsync(typeof(AkoMusic), services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(client, message);
            if (message.Author.Id == Config.Aiko.Id) return;
            //if (message.Author.IsBot) return; //prevent any bot from sending the commands
            int argPos = 0;
            if (message.HasStringPrefix(Config.Aiko.PrefixParent[0], ref argPos) ||
                message.HasStringPrefix(Config.Aiko.PrefixParent[1], ref argPos) ||
                message.HasMentionPrefix(client.CurrentUser, ref argPos))
            {
                var result = await commands.ExecuteAsync(context, argPos, services);
                switch (result.Error)
                {
                    case CommandError.BadArgCount:
                        await context.Channel.SendMessageAsync($"Gomen ne {context.User.Username}, looks like you have missing/too much parameter. " +
                            $"See `{Config.Aiko.PrefixParent[0]}help <commands or category>`for commands help.");
                        break;
                    case CommandError.UnknownCommand:
                        await message.Channel.SendMessageAsync(embed: new EmbedBuilder()
                        .WithDescription($"Gomen ne {context.User.Username}, I can't seem to understand your commands. " +
                            $"See `{Config.Aiko.PrefixParent[0]}help <commands or category>`for command help.")
                        .WithAuthor(Config.Aiko.EmbedNameError)
                        .WithColor(Config.Aiko.EmbedColor)
                        .WithThumbnailUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/6/63/ODN-EP3-006.png")
                        .Build());
                        break;
                    case CommandError.ObjectNotFound:
                        await message.Channel.SendMessageAsync($"Gomen ne {context.User.Username}, {result.ErrorReason} " +
                            $"See `{Config.Aiko.PrefixParent[0]}help <commands or category>`for command help.");
                        break;
                    case CommandError.ParseFailed:
                        await message.Channel.SendMessageAsync($"Gomen ne {context.User.Username}, {result.ErrorReason} " +
                            $"See `{Config.Aiko.PrefixParent[0]}help <commands or category>`for command help.");
                        break;
                }
            }
        }

        private Task client_log(LogMessage msg)
        {
            Console.WriteLine("Aiko: " + msg.ToString());
            return Task.CompletedTask;
        }

    }
}
