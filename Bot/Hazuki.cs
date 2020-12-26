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
using Newtonsoft.Json.Linq;
using System.IO;
using System.Drawing;
using OjamajoBot.Database.Model;
using OjamajoBot.Database;

namespace OjamajoBot.Bot
{
    class Hazuki
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
            await client.LoginAsync(TokenType.Bot, Config.Hazuki.Token);
            await client.StartAsync();

            client.ReactionAdded += HandleReactionAddedAsync;
            client.JoinedGuild += JoinedGuild;
            client.GuildAvailable += GuildAvailable;

            //start rotates random activity
            _timerStatus = new Timer(async _ =>
            {
                var returnStatusActivity = Config.Core.BotStatus.checkStatusActivity(Config.Core.BotClass.Hazuki,
                    Config.Hazuki.Status.arrRandomActivity);

                var returnObjectActivity = returnStatusActivity.Item1;
                Config.Hazuki.Status.currentActivity = returnObjectActivity[0].ToString();
                Config.Hazuki.Status.currentActivityReply = returnObjectActivity[1].ToString();
                await client.SetGameAsync(Config.Hazuki.Status.currentActivity);
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
                //.SendMessageAsync("Pretty Witchy Hazuki Chi~");

                Console.WriteLine("Hazuki Connected!");
                //new Aiko().RunBotAsync().GetAwaiter().GetResult();
                return Task.CompletedTask;
            };


            //// Block this task until the program is closed.
            await Task.Delay(2000);

        }

        public void Dispose() => client.MessageReceived -= HandleCommandAsync;

        public void HookReactionAdded(BaseSocketClient client) => client.ReactionAdded += HandleReactionAddedAsync;

        public async Task HandleReactionAddedAsync(Cacheable<IUserMessage, ulong> cachedMessage,
        ISocketMessageChannel originChannel, SocketReaction reaction)
        {
            
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

            if (guildData[DBM_Guild.Columns.id_channel_birthday_announcement].ToString() != "" &&
            Convert.ToInt32(guildData[DBM_Guild.Columns.birthday_announcement_ojamajo]) == 1 &&
            Convert.ToInt32(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour
            )
            {
                Config.Hazuki._timerBirthdayAnnouncement[guild.Id.ToString()] = new Timer(async _ =>
                {
                    //set birthday announcement timer
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

        public async Task JoinedGuild(SocketGuild guild)
        {
            var systemChannel = client.GetChannel(guild.SystemChannel.Id) as SocketTextChannel; // Gets the channel to send the message in
            await systemChannel.SendMessageAsync(
            embed: new EmbedBuilder()
            .WithColor(Config.Hazuki.EmbedColor)
            .WithTitle("Pretty Witchy Hazuki Chi!")
            .WithDescription($"Hello, Hazuki is here! Thank you for inviting me to the {guild.Name}, I'm very happy to meet you all. " +
                $"You can call me with `{Config.Hazuki.PrefixParent[0]}` as my default prefix or ask me with `{Config.Hazuki.PrefixParent[0]}help` for all the command list that I have.")
            .WithImageUrl("https://cdn.discordapp.com/attachments/706812058175406210/706814281701916712/dokkan.gif")
            .Build());
        }

        public async Task RegisterCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;
            await commands.AddModuleAsync(typeof(HazukiModule), services);
            await commands.AddModuleAsync(typeof(HazukiMagicalStageModule), services);
            await commands.AddModuleAsync(typeof(HazukiRandomEventModule), services);
            await commands.AddModuleAsync(typeof(HazukiMinigameInteractive), services);
            await commands.AddModuleAsync(typeof(HazukiTradingCardInteractive), services);
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
                message.HasMentionPrefix(client.CurrentUser, ref argPos))
            {
                var result = await commands.ExecuteAsync(context, argPos, services);
                switch (result.Error)
                {
                    case CommandError.BadArgCount:
                        await context.Channel.SendMessageAsync($"I'm sorry {context.User.Username}, looks like you have missing/too much parameter. " +
                            $"See `{Config.Hazuki.PrefixParent[0]}help <commands or category>`for commands help.");
                        break;
                    case CommandError.UnknownCommand:
                        await message.Channel.SendMessageAsync(embed: new EmbedBuilder()
                            .WithColor(Config.Hazuki.EmbedColor)
                            .WithDescription($"Sorry, I can't find that command. " +
                            $"See `{Config.Hazuki.PrefixParent[0]}help <commands or category>`for commands help.")
                            .WithThumbnailUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/82/ODN-EP6-078.png")
                            .Build());
                        break;
                    case CommandError.ObjectNotFound:
                        await message.Channel.SendMessageAsync($"Sorry {context.User.Username}, {result.ErrorReason} " +
                            $"See `{Config.Hazuki.PrefixParent[0]}help <commands or category>`for commands help.");
                        break;
                    case CommandError.ParseFailed:
                        await message.Channel.SendMessageAsync($"Sorry {context.User.Username}, {result.ErrorReason} " +
                            $"See `{Config.Hazuki.PrefixParent[0]}help <commands or category>`for commands help.");
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
