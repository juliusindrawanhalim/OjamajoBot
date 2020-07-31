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
            var message = await cachedMessage.GetOrDownloadAsync();
            //var context = new SocketCommandContext(client, cachedMessage);
            var messageId = message.GetJumpUrl().Split('/').Last();
            var guildId = message.GetJumpUrl().Split('/')[4];
            SocketGuildUser guildUser = (SocketGuildUser)reaction.User;
            var channel = client.GetChannel(originChannel.Id) as SocketTextChannel;

            if (message != null && reaction.User.IsSpecified)
            {
                if (!guildUser.IsBot)
                {
                    if (reaction.Emote.Equals(new Discord.Emoji("\uD83C\uDF81")))
                    {
                        try
                        {
                            //config/
                            var fileDirectory = $"{Config.Core.headConfigFolder}giveaway/giveaway.json";
                            var fileDirectoryData = $"{Config.Core.headConfigFolder}giveaway/userdata.json";
                            var val = JObject.Parse(File.ReadAllText(fileDirectory));
                            var valUserData = JObject.Parse(File.ReadAllText(fileDirectoryData));
                            JArray itemCode = (JArray)val["discord"];

                            var dmchannel = await guildUser.GetOrCreateDMChannelAsync();

                            var userId = guildUser.Id;
                            var randomWin = new Random().Next(0, 21);
                            var eb = new EmbedBuilder()
                            .WithColor(Config.Hazuki.EmbedColor);


                            var dataClaimed = (JObject)valUserData["claimed"];
                            var dataParticipant = (JObject)valUserData["participant"];

                            if (dataClaimed.ContainsKey(guildUser.Id.ToString()))
                            {
                                eb.WithDescription("Sorry, " +
                                    "you're not allowed to participate anymore because you have win the giveaway already.")
                                .WithThumbnailUrl("https://cdn.discordapp.com/attachments/706770454697738300/728651519750176788/lose3.jpg");
                                await dmchannel.SendMessageAsync(embed: eb.Build());
                            } else if (dataParticipant.ContainsKey(guildUser.Id.ToString())&&
                                dataParticipant[guildUser.Id.ToString()].ToString() == DateTime.Now.ToString("dd"))
                            {
                                var now = DateTime.Now;
                                var tomorrow = now.AddDays(1).Date;
                                double totalHours = (tomorrow - now).TotalHours;

                                eb.WithDescription("Sorry, you have participate the giveaway event today. " +
                                    $"Please wait again until next day at **{Math.Floor(totalHours)}** hour(s) " +
                                    $"**{Math.Ceiling(60 * (totalHours - Math.Floor(totalHours)))}** more minute(s).")
                                .WithThumbnailUrl("https://cdn.discordapp.com/attachments/706770454697738300/728651519750176788/lose3.jpg");
                                await dmchannel.SendMessageAsync(embed: eb.Build());
                            }
                            else if (Convert.ToInt32(DateTime.Now.ToString("dd")) >= 25)
                            {
                                eb.WithDescription("I'm sorry to tell that the giveaway event has ended.")
                                    .WithThumbnailUrl("https://cdn.discordapp.com/attachments/706770454697738300/728651519750176788/lose3.jpg");
                                await dmchannel.SendMessageAsync(embed: eb.Build());
                            } else if (val.Count <= 0)
                            {
                                eb.WithDescription("I'm sorry to tell that I'm running out of the giveaway codes now... " +
                                "I hope you can win next time~")
                                .WithThumbnailUrl("https://cdn.discordapp.com/attachments/706770454697738300/728651519750176788/lose3.jpg");
                                await dmchannel.SendMessageAsync(embed: eb.Build());
                            } else if (randomWin != 3)
                            {//not win
                                eb.WithTitle("You are participating the giveaway event and the result comes out:")
                                .WithDescription(":x:  I'm sorry to tell that you're not lucky enough this time... " +
                                "But don't worry, you can try again next day or until the giveaway event has ended.")
                                .WithThumbnailUrl("https://cdn.discordapp.com/attachments/706770454697738300/728655230719361177/draw0.jpg");
                                await dmchannel.SendMessageAsync(embed: eb.Build());

                                //add to data
                                if (!dataParticipant.ContainsKey(guildUser.Id.ToString()))
                                {
                                    dataParticipant.Add(new JProperty(guildUser.Id.ToString(), DateTime.Now.ToString("dd")));
                                } else
                                {
                                    dataParticipant[guildUser.Id.ToString()] = DateTime.Now.ToString("dd");
                                }
                                
                                File.WriteAllText(fileDirectoryData, valUserData.ToString());
                            }
                            else if (randomWin == 3)
                            {//win
                                string code = itemCode[0].ToString();

                                eb.WithTitle("You are participating the giveaway event and the result comes out:")
                                .WithDescription($":tada: " +
                                $"Oh, looks like you have win the giveaway event. Big congratulations to you!\n" +
                                "Thank you for participating this event and I hope you enjoy this gift~")
                                .AddField("1 Month Discord Nitro Code:", $"||{code}||")
                                .WithFooter("Sincerely, Hazuki & Digi~")
                                .WithThumbnailUrl("https://cdn.discordapp.com/attachments/706770454697738300/728655569799610438/hazuki_happy21.jpg");
                                await dmchannel.SendMessageAsync(embed: eb.Build());

                                //start remove
                                itemCode[0].Remove();
                                File.WriteAllText(fileDirectory, val.ToString());
                                //end remove

                                //add to data
                                dataClaimed.Add(new JProperty(guildUser.Id.ToString(), code));
                                File.WriteAllText(fileDirectoryData, valUserData.ToString());

                                await originChannel.SendMessageAsync($"Congratulations to our friend: {MentionUtils.MentionUser(guildUser.Id)} that has win the giveaway event!");
                                
                                if (itemCode.Count <= 0)
                                {
                                    var ebEventFinish = new EmbedBuilder()
                                        .WithColor(Config.Hazuki.EmbedColor)
                                        .WithThumbnailUrl("https://cdn.discordapp.com/attachments/706770454697738300/728655569799610438/hazuki_happy21.jpg")
                                        .WithDescription($"The giveaway event has come to an end! " +
                                        $"Thank you everyone for participating. " +
                                        $"For those who haven't got the chance to win I hope you can win on next time~. " +
                                        $"Until we meet again next time~");
                                    await originChannel.SendMessageAsync(embed:ebEventFinish.Build());
                                }
                            }

                            await message.RemoveReactionAsync(reaction.Emote, guildUser);

                            //if (message.Author.Id == Config.Doremi.Id)
                            //{
                            //    //user id: 145584315839938561
                            //    
                            //    await dmchannel.SendMessageAsync(embed: new EmbedBuilder()
                            //    .WithColor(Config.Doremi.EmbedColor)
                            //    .WithTitle("You have choose the Discord Nitro Giveaway!")
                            //    .WithDescription($":x: You have been removed from the role: **{roleMaster.Name}**")
                            //    .WithThumbnailUrl(TradingCardCore.Doremi.emojiOk)
                            //    .Build());
                            //}
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    }
                }
            }
        }
        public async Task GuildAvailable(SocketGuild guild)
        {
            //set hazuki birthday announcement timer
            if (Config.Guild.hasPropertyValues(guild.Id.ToString(), "id_birthday_announcement"))
            {
                Config.Hazuki._timerBirthdayAnnouncement[guild.Id.ToString()] = new Timer(async _ =>
                {
                    //announce doremi birthday
                    if (Config.Doremi.Status.isBirthday())
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

        public async Task JoinedGuild(SocketGuild guild)
        {
            var systemChannel = client.GetChannel(guild.SystemChannel.Id) as SocketTextChannel; // Gets the channel to send the message in
            await systemChannel.SendMessageAsync($"Pretty witchy {MentionUtils.MentionUser(Config.Hazuki.Id)} chi~ has arrived to the {guild.Name}. " +
                $"Thank you for very much inviting me, I'm very happy to meet you all. " +
                $"You can ask me with `{Config.Hazuki.PrefixParent[0]}help` for all commands list.",
            embed: new EmbedBuilder()
            .WithColor(Config.Hazuki.EmbedColor)
            .WithImageUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/f/fc/04.01.08.JPG")
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
            if (Config.Guild.getPropertyValue(context.Guild.Id, "hazuki_role_id") != "" &&
                message.HasStringPrefix($"<@&{Config.Guild.getPropertyValue(context.Guild.Id, "hazuki_role_id")}>", ref argPos)){
                await message.Channel.SendMessageAsync(embed: new EmbedBuilder()
                    .WithAuthor(Config.Hazuki.EmbedNameError)
                    .WithDescription($"I'm sorry {context.User.Username}, it seems you're calling me with the role prefix. " +
                            "Please use the non role prefix.")
                    .WithColor(Config.Hazuki.EmbedColor)
                    .WithThumbnailUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/8/82/ODN-EP6-078.png")
                    .Build());
            } else if (message.HasStringPrefix(Config.Hazuki.PrefixParent[0], ref argPos) ||
                message.HasStringPrefix(Config.Hazuki.PrefixParent[1], ref argPos) ||
                message.HasMentionPrefix(client.CurrentUser, ref argPos)){
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
                            .WithDescription($"I'm sorry {context.User.Username}, I can't seem to understand your commands. " +
                            $"See `{Config.Hazuki.PrefixParent[0]}help <commands or category>`for commands help.")
                            //.WithImageUrl("https://33.media.tumblr.com/28c2441a5655ecb1bd23df8275f3598f/tumblr_nfkjtbSQZg1r98a5go1_500.gif")
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
