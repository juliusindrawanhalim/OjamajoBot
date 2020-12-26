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
using OjamajoBot.Database.Model;
using OjamajoBot.Database;

namespace OjamajoBot.Bot
{
    class Momoko
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
            await client.LoginAsync(TokenType.Bot, Config.Momoko.Token);
            await client.StartAsync();
            

            client.JoinedGuild += JoinedGuild;
            client.GuildAvailable += GuildAvailable;

            //start rotates random activity
            _timerStatus = new Timer(async _ =>
            {
                var returnStatusActivity = Config.Core.BotStatus.checkStatusActivity(Config.Core.BotClass.Momoko,
                    Config.Momoko.Status.arrRandomActivity);

                var returnObjectActivity = returnStatusActivity.Item1;
                Config.Momoko.Status.currentActivity = returnObjectActivity[0].ToString();
                Config.Momoko.Status.currentActivityReply = returnObjectActivity[1].ToString();
                await client.SetGameAsync(Config.Momoko.Status.currentActivity);
                await client.SetStatusAsync((UserStatus)returnObjectActivity[2]);
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

        public void Dispose() => client.MessageReceived -= HandleCommandAsync;

        public async Task JoinedGuild(SocketGuild guild)
        {
            var systemChannel = client.GetChannel(guild.SystemChannel.Id) as SocketTextChannel; // Gets the channel to send the message in
            await systemChannel.SendMessageAsync(embed: new EmbedBuilder()
            .WithColor(Config.Momoko.EmbedColor)
            .WithTitle($"Pretty witchy Momoko chi!")
            .WithDescription($"Hello everyone, Momoko Asuka is here. Thank you for inviting me to the {guild.Name}. " +
            $"You can call me with `{Config.Momoko.PrefixParent[0]}` as my default prefix or ask me with `{Config.Momoko.PrefixParent[0]}help` for all command list that I have.")
            .WithImageUrl("https://cdn.discordapp.com/attachments/706812115482312754/706820390017564692/dokkan.gif")
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

                //set Momoko birthday announcement timer
            if (guildData[DBM_Guild.Columns.id_channel_birthday_announcement].ToString() != "" &&
            Convert.ToInt32(guildData[DBM_Guild.Columns.birthday_announcement_ojamajo]) == 1 &&
            Convert.ToInt32(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour)
            {
                Config.Momoko._timerBirthdayAnnouncement[guild.Id.ToString()] = new Timer(async _ =>
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

        public async Task RegisterCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;
            await commands.AddModuleAsync(typeof(MomokoModule), services);
            await commands.AddModuleAsync(typeof(MomokoMagicalStageModule), services);
            await commands.AddModuleAsync(typeof(MomokoRandomEventModule), services);
            //await commands.AddModuleAsync(typeof(MomokoPatissiere), services);
            await commands.AddModuleAsync(typeof(MomokoMinigameInteractive), services);
            await commands.AddModuleAsync(typeof(MomokoTradingCardInteractive), services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(client, message);
            if (message.Author.Id == Config.Momoko.Id) return;
            //if (message.Author.IsBot) return; //prevent any bot from sending the commands
            int argPos = 0;

            if (message.HasStringPrefix(Config.Momoko.PrefixParent[0], ref argPos) ||
                message.HasStringPrefix(Config.Momoko.PrefixParent[1], ref argPos) ||
                message.HasMentionPrefix(client.CurrentUser, ref argPos))
            {
                var result = await commands.ExecuteAsync(context, argPos, services);
                switch (result.Error)
                {
                    case CommandError.BadArgCount:
                        await context.Channel.SendMessageAsync($"I'm sorry {context.User.Username}, looks like you have missing/too much parameter. " +
                            $"Please see `{Config.Momoko.PrefixParent[0]}help` for command help.");
                        break;
                    case CommandError.UnknownCommand:
                        await message.Channel.SendMessageAsync(embed: new EmbedBuilder()
                        .WithDescription($"It's an error! " +
                        $"Please see `{Config.Momoko.PrefixParent[0]}help` for command help.")
                        .WithAuthor(Config.Momoko.EmbedNameError)
                        .WithColor(Config.Momoko.EmbedColor)
                        .WithThumbnailUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/5/52/ODN-EP9-025.png")
                        .Build());
                        break;
                    case CommandError.ObjectNotFound:
                        await message.Channel.SendMessageAsync($"I'm sorry {context.User.Username}, {result.ErrorReason} " +
                            $"See `{Config.Momoko.PrefixParent[0]}help` for command help.");
                        break;
                    case CommandError.ParseFailed:
                        await message.Channel.SendMessageAsync($"I'm sorry {context.User.Username}, {result.ErrorReason} " +
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
