﻿using System;
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
using OjamajoBot.Database.Model;
using OjamajoBot.Database;

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
                //.AddSingleton(new ReliabilityService(client))
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
                var returnStatusActivity = Config.Core.BotStatus.checkStatusActivity(Config.Core.BotClass.Onpu,
                    Config.Onpu.Status.arrRandomActivity);

                var returnObjectActivity = returnStatusActivity.Item1;
                Config.Onpu.Status.currentActivity = returnObjectActivity[0].ToString();
                Config.Onpu.Status.currentActivityReply = returnObjectActivity[1].ToString();
                await client.SetGameAsync(Config.Onpu.Status.currentActivity);
                await client.SetStatusAsync((UserStatus)returnObjectActivity[2]);
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
            await systemChannel.SendMessageAsync(embed: new EmbedBuilder()
            .WithColor(Config.Onpu.EmbedColor)
            .WithTitle($"Pretty witchy Onpu chi!")
            .WithDescription($"Hello everyone, Onpu Segawa is here. Thank you for inviting me to the {guild.Name}. " +
            $"You can call me with `{Config.Onpu.PrefixParent[0]}` as my default prefix or ask me with `{Config.Onpu.PrefixParent[0]}help` for all command list that I have.")
            .WithImageUrl("https://cdn.discordapp.com/attachments/706812100789403659/706819976409120788/dokkan.gif")
            .Build());
        }

        public async Task GuildAvailable(SocketGuild guild)
        {
            ulong guildId = guild.Id;
            var guildData = Config.Guild.getGuildData(guildId);
            string guildBirthdayLastAnnouncement = "";
            if (guildData[DBM_Guild.Columns.birthday_announcement_date_last].ToString() != "")
                guildBirthdayLastAnnouncement = guildData[DBM_Guild.Columns.birthday_announcement_date_last].ToString();
            else
                guildBirthdayLastAnnouncement = "1";

            //set Onpu birthday announcement timer
            if (guildData[DBM_Guild.Columns.id_channel_birthday_announcement].ToString() != "" &&
            Convert.ToInt32(guildData[DBM_Guild.Columns.birthday_announcement_ojamajo]) == 1 &&
            Convert.ToInt32(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour)
            {
                Config.Onpu._timerBirthdayAnnouncement[guild.Id.ToString()] = new Timer(async _ =>
                {
                    //announce doremi birthday
                    if (guildBirthdayLastAnnouncement != DateTime.Now.ToString("dd") && 
                        Config.Doremi.Status.isBirthday())
                    {
                        await client
                        .GetGuild(guild.Id)
                        .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement].ToString()))
                        .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday, {MentionUtils.MentionUser(Config.Doremi.Id)} chan. " +
                        $"She has turned into {Config.Doremi.birthdayCalculatedYear} on this year. Let's give some big steak and wonderful birthday wishes for her.");

                        //update last birthday announcement date
                        string query = @$"UPDATE {DBM_Guild.tableName} 
                        SET {DBM_Guild.Columns.birthday_announcement_date_last}=@{DBM_Guild.Columns.birthday_announcement_date_last} 
                        WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild}";
                        Dictionary<string, object> columnsFilter = new Dictionary<string, object>();
                        columnsFilter[DBM_Guild.Columns.birthday_announcement_date_last] = DateTime.Now.ToString("dd");
                        columnsFilter[DBM_Guild.Columns.id_guild] = guildId.ToString();
                        new DBC().update(query, columnsFilter);

                    }
                },
                null,
                TimeSpan.FromSeconds(10), //time to wait before executing the timer for the first time
                TimeSpan.FromMinutes(40) //time to wait before executing the timer again
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
            //await commands.AddModuleAsync(typeof(OnpuVictoriaMusic), services);
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
            if (message.HasStringPrefix(Config.Onpu.PrefixParent[0], ref argPos) ||
                message.HasStringPrefix(Config.Onpu.PrefixParent[1], ref argPos) ||
                message.HasMentionPrefix(client.CurrentUser, ref argPos))
            {
                var result = await commands.ExecuteAsync(context, argPos, services);
                switch (result.Error)
                {
                    case CommandError.BadArgCount:
                        await context.Channel.SendMessageAsync("Onpu sense that you have missing/many parameters. " +
                            $"See `{Config.Onpu.PrefixParent[0]}help <commands or category>` for command help.");
                        break;
                    case CommandError.UnknownCommand:
                        await message.Channel.SendMessageAsync(embed: new EmbedBuilder()
                        .WithDescription("Sorry, Onpu can't find that command. " +
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
