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
using Victoria;

namespace OjamajoBot.Bot
{
    class Onpu
    {
        private CommandService commands;
        private IServiceProvider services;

        public static DiscordSocketClient client;

        //timer to rotates activity
        private Timer _timerStatus;

        private AudioService audioservice;
        private VictoriaService victoriaservice;
        private LavaNode _lavaNode;

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
                .AddSingleton(new ReliabilityService(client))
                .AddSingleton(audioservice)
                //victoria
                .AddSingleton<LavaNode>()
                .AddSingleton<LavaConfig>()
                .BuildServiceProvider();

            client.Log += client_log;

            // do something .. don't forget disposing serviceProvider!
            Dispose();

            await RegisterCommandsAsync();
            await client.LoginAsync(TokenType.Bot, Config.Onpu.Token);
            await client.StartAsync();

            _lavaNode = services.GetRequiredService<LavaNode>();
            victoriaservice = new VictoriaService(_lavaNode, client);
            client.JoinedGuild += JoinedGuild;
            client.GuildAvailable += GuildAvailable;

            //start rotates random activity
            _timerStatus = new Timer(async _ =>
            {
                Boolean birthdayExisted = false;

                //override if there's bot birthday
                if (DateTime.Now.ToString("dd") == Config.Doremi.birthdayDate.ToString("dd") &&
                DateTime.Now.ToString("MM") == Config.Doremi.birthdayDate.ToString("MM") &&
                Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour)
                {
                    await client.SetGameAsync($"with Doremi birthday {Config.Emoji.birthdayCake}", type: ActivityType.Playing); //set activity to current index position
                    birthdayExisted = true;
                }

                //announce hazuki birthday
                if (DateTime.Now.ToString("dd") == Config.Hazuki.birthdayDate.ToString("dd") &&
                DateTime.Now.ToString("MM") == Config.Hazuki.birthdayDate.ToString("MM") &&
                Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour)
                {
                    await client.SetGameAsync($"with Hazuki birthday {Config.Emoji.birthdayCake}", type: ActivityType.Playing); //set activity to current index position
                    birthdayExisted = true;
                }

                //announce aiko birthday
                if (DateTime.Now.ToString("dd") == Config.Aiko.birthdayDate.ToString("dd") &&
                DateTime.Now.ToString("MM") == Config.Aiko.birthdayDate.ToString("MM") &&
                Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour)
                {
                    await client.SetGameAsync($"with Aiko birthday {Config.Emoji.birthdayCake}", type: ActivityType.Playing); //set activity to current index position
                    birthdayExisted = true;
                }

                //announce onpu birthday
                if (DateTime.Now.ToString("dd") == Config.Onpu.birthdayDate.ToString("dd") &&
                DateTime.Now.ToString("MM") == Config.Onpu.birthdayDate.ToString("MM") &&
                Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour)
                {
                    await client.SetGameAsync($"with Onpu birthday {Config.Emoji.birthdayCake}", type: ActivityType.Playing); //set activity to current index position
                    birthdayExisted = true;
                }

                //announce momoko birthday
                if (DateTime.Now.ToString("dd") == Config.Momoko.birthdayDate.ToString("dd") &&
                DateTime.Now.ToString("MM") == Config.Momoko.birthdayDate.ToString("MM") &&
                Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour)
                {
                    await client.SetGameAsync($"with Momoko birthday {Config.Emoji.birthdayCake}", type: ActivityType.Playing); //set activity to current index position
                    birthdayExisted = true;
                }

                if (!birthdayExisted)
                {
                    Random rnd = new Random();
                    int rndIndex = rnd.Next(0, Config.Onpu.arrRandomActivity.GetLength(0)); //random the list value
                                                                                            //if (rndIndex > 0) rndIndex -= 1;
                    string updLog = "Updated Onpu Activity - Playing: " + Config.Onpu.arrRandomActivity[rndIndex, 0];
                    Config.Onpu.indexCurrentActivity = rndIndex;
                    await client.SetGameAsync(Config.Onpu.arrRandomActivity[rndIndex, 0], type: ActivityType.Playing); //set activity to current index position

                    Console.WriteLine(updLog);
                }
            },
            null,
            TimeSpan.FromSeconds(1), //time to wait before executing the timer for the first time (set first status)
            TimeSpan.FromMinutes(10) //time to wait before executing the timer again (set new status - repeats indifinitely every 10 seconds)
            );
            //end block

            client.Ready += () =>
            {
                Console.WriteLine("Onpu Connected!");
                //new Momoko().RunBotAsync().GetAwaiter().GetResult();
                if (!_lavaNode.IsConnected)
                    _lavaNode.ConnectAsync();

                return Task.CompletedTask;
            };


            //// Block this task until the program is closed.
            await Task.Delay(4000);

        }

        public async Task JoinedGuild(SocketGuild guild)
        {
            var systemChannel = client.GetChannel(guild.SystemChannel.Id) as SocketTextChannel; // Gets the channel to send the message in
            await systemChannel.SendMessageAsync($"Pretty witchy {MentionUtils.MentionUser(Config.Onpu.Id)} chi~ has arrived to the {guild.Name}. Hello, this is Onpu, thank you for inviting me to the group. " +
                $"As an idol, I will make all of you happy. You can ask me with `{Config.Onpu.PrefixParent[0]}help` for all commands list.",
            embed: new EmbedBuilder()
            .WithColor(Config.Onpu.EmbedColor)
            .WithImageUrl("https://i.imgur.com/mgTmDgF.png")
            .Build());
        }

        public async Task GuildAvailable(SocketGuild guild)
        {
            //set Onpu birthday announcement timer
            if (Config.Guild.hasPropertyValues(guild.Id.ToString(), "id_birthday_announcement"))
            {
                Config.Onpu._timerBirthdayAnnouncement[guild.Id.ToString()] = new Timer(async _ =>
                {
                    //announce doremi birthday
                    if (DateTime.Now.ToString("dd") == Config.Doremi.birthdayDate.ToString("dd") &&
                    DateTime.Now.ToString("MM") == Config.Doremi.birthdayDate.ToString("MM") &&
                    (Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour &&
                    Int32.Parse(DateTime.Now.ToString("HH")) <= Config.Core.maxGlobalTimeHour))
                    {
                        await client
                        .GetGuild(guild.Id)
                        .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guild.Id, "id_birthday_announcement")))
                        .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday, {MentionUtils.MentionUser(Config.Doremi.Id)} chan. " +
                        $"She has turned into {Config.Doremi.birthdayCalculatedYear} on this year. Let's give some big steak and wonderful birthday wishes for her.");
                    }

                },
                null,
                TimeSpan.FromSeconds(10), //time to wait before executing the timer for the first time
                TimeSpan.FromHours(24) //time to wait before executing the timer again
                );
            }
        }

        /// <summary>
        ///     Unregisters the events attached to the discord client.
        /// </summary>
        public void Dispose() => client.MessageReceived -= HandleCommandAsync;

        public async Task RegisterCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;
            await commands.AddModuleAsync(typeof(OnpuModule), services);
            await commands.AddModuleAsync(typeof(OnpuVictoriaMusic), services);
            await commands.AddModuleAsync(typeof(OnpuMagicalStageModule), services);
            await commands.AddModuleAsync(typeof(OnpuMinigameInteractive), services);
            await commands.AddModuleAsync(typeof(OnpuTradingCardInteractive), services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(client, message);
            if (message.Author.Id == Config.Onpu.Id) return;
            //if (message.Author.IsBot) return; //prevent any bot from sending the commands

            int argPos = 0;
            if (Config.Guild.getPropertyValue(context.Guild.Id, "onpu_role_id") != "" &&
            message.HasStringPrefix($"<@&{Config.Guild.getPropertyValue(context.Guild.Id, "onpu_role_id")}>", ref argPos)){
                await message.Channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithAuthor(Config.Onpu.EmbedNameError)
                .WithDescription($"Sorry {context.User.Username}, it seems you're calling me with the role prefix. " +
                "Please try to use the non role prefix.")
                .WithColor(Config.Onpu.EmbedColor)
                .WithThumbnailUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/55/ODN-EP11-084.png")
                .Build());
            } else if (message.HasStringPrefix(Config.Onpu.PrefixParent[0], ref argPos) ||
                message.HasStringPrefix(Config.Onpu.PrefixParent[1], ref argPos) ||
                message.HasMentionPrefix(client.CurrentUser, ref argPos)) {
                var result = await commands.ExecuteAsync(context, argPos, services);
                switch (result.Error)
                {
                    case CommandError.BadArgCount:
                        await context.Channel.SendMessageAsync("Onpu sense that you have missing/many parameters. " +
                            $"See `{Config.Onpu.PrefixParent[0]}help <commands or category>` for command help.");
                        break;
                    case CommandError.UnknownCommand:
                        await message.Channel.SendMessageAsync(embed: new EmbedBuilder()
                        .WithDescription("Onpu can't seems to understand your commands. " +
                        $"See `{Config.Onpu.PrefixParent[0]}help <commands or category>` for command help.")
                        .WithAuthor(Config.Onpu.EmbedNameError)
                        .WithColor(Config.Onpu.EmbedColor)
                        .WithThumbnailUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/7/7e/ODN-EP11-028.png")
                        .Build());
                        break;
                    case CommandError.ObjectNotFound:
                        await message.Channel.SendMessageAsync($"Onpu has noticed an error: {result.ErrorReason} " +
                            $"See `{Config.Onpu.PrefixParent[0]}help <commands or category>` for command help.");
                        break;
                    case CommandError.ParseFailed:
                        await message.Channel.SendMessageAsync($"Onpu has noticed an error: {result.ErrorReason} " +
                            $"See `{Config.Onpu.PrefixParent[0]}help <commands or category> `for command help.");
                        break;
                }

            }
        }

        private Task client_log(LogMessage msg)
        {
            Console.WriteLine("Onpu: " + msg.ToString());
            return Task.CompletedTask;
        }

    }
}
