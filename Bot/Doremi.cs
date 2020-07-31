using System;
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
using System.Globalization;
using Spectacles.NET.Types;

namespace OjamajoBot.Bot
{
    public class Doremi
    {
        public CommandService commands;
        private IServiceProvider services;

        public static DiscordSocketClient client;

        //set timer for random event
        //private static Timer randomEventTimer;
        //private int minRandomEventInterval;

        //private AudioService audioservice;
        //private Lavalink4netService lavalink4netservice;

        //private VictoriaService victoriaservice;

        //init lavanode
        //private LavaNode _lavaNode;

        //timer to rotates activity
        private Timer _timerStatus, _rotatesWeather;

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
            //audioservice = new AudioService();
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
            //.AddSingleton(new ReliabilityService(client))
            //.AddSingleton(audioservice)
            //victoria
            //.AddSingleton<LavaConfig>()
            //.AddSingleton<LavaNode>()
            // Request Caching for Lavalink
            //.AddSingleton<ILavalinkCache, LavalinkCache>()
            .BuildServiceProvider();

            client.Log += client_log;

            // do something .. don't forget disposing serviceProvider!
            Dispose();

            await RegisterCommandsAsync();
            await client.LoginAsync(TokenType.Bot, Config.Doremi.Token);
            await client.StartAsync();

            //start rotates weather
            _rotatesWeather = new Timer(async _ =>
                {
                    int randomWeather = new Random().Next(0, 51);
                    int selectedWeatherIndex = 0;
                    //Console.WriteLine(randomWeather);
                    if (randomWeather <= 30)
                    {
                        selectedWeatherIndex = 0;
                        //GardenCore.weather = new string[]{ $"☀️", "sunny","A perfect time to water the plant~","5"};
                        
                    } else if (randomWeather <= 40)
                    {
                        selectedWeatherIndex = 1;
                        //GardenCore.weather = new string[] { $"☁️", "cloudy","There might be a chance to rain soon...","4"};
                    }
                    else if (randomWeather <= 45)
                    {
                        selectedWeatherIndex = 2;
                        //GardenCore.weather = new string[] { $"🌧️", "raining","Not sure if it's a good time to water the plant.","3"};
                    }
                    else if (randomWeather <= 50)
                    {
                        selectedWeatherIndex = 3;
                        //GardenCore.weather = new string[] { $"⛈️", "thunder storm","I don't think it's the best time to water the plant now...","2"};
                    }

                    for(int i = 0; i<=4; i++)
                    {
                        GardenCore.weather[i] = GardenCore.arrRandomWeather[selectedWeatherIndex, i];
                    }
                    
                    //GardenCore.weather[1] = GardenCore.arrRandomWeather[selectedWeatherIndex,1];
                    //GardenCore.weather[2] = GardenCore.arrRandomWeather[selectedWeatherIndex,2];
                    //GardenCore.weather[3] = GardenCore.arrRandomWeather[selectedWeatherIndex,3];
                    //GardenCore.weather[4] = GardenCore.arrRandomWeather[selectedWeatherIndex,4];

                    //GardenCore.weather = new string[] { GardenCore.arrRandomWeather[selectedWeatherIndex, 0] };
                },
                null,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromHours(2)
            );

            //start rotates activity
            _timerStatus = new Timer(async _ =>
            {

                var returnStatusActivity = Config.Core.BotStatus.checkStatusActivity(Config.Core.BotClass.Doremi,
                    Config.Doremi.Status.arrRandomActivity);

                var returnObjectActivity = returnStatusActivity.Item1;
                Config.Doremi.Status.currentActivity = returnObjectActivity[0].ToString();
                Config.Doremi.Status.currentActivityReply = returnObjectActivity[1].ToString();
                await client.SetGameAsync(Config.Doremi.Status.currentActivity);
                await client.SetStatusAsync((UserStatus)returnObjectActivity[2]);
                
                //if (!forceStatusChange && returnDailyRoutine.Item1)
                //{
                //    //Config.Doremi.Status.userStatus = returnDailyRoutine.Item2[
                //    await client.SetStatusAsync(Config.Doremi.Status.userStatus);
                //} else
                //{
                //    Random rnd = new Random();
                //    int rndIndex = rnd.Next(0, Config.Doremi.Status.arrRandomActivity.GetLength(0)); //random the list value
                //    await client.SetGameAsync(Config.Doremi.Status.arrRandomActivity[rndIndex, 0].ToString(), type: Discord.ActivityType.Playing); //set activity to current index position
                //    Config.Doremi.Status.currentActivityReply = Config.Doremi.Status.arrRandomActivity[rndIndex, 1].ToString();
                    
                //    string updLog = $"Updated Doremi Activity - Playing: {Config.Doremi.Status.currentActivity}";
                //    Console.WriteLine(updLog);
                //}

            },
            null,
            TimeSpan.FromSeconds(1), //time to wait before executing the timer for the first time (set first status)
            TimeSpan.FromMinutes(10) //time to wait before executing the timer again (set new status - repeats indifinitely every 10 seconds)
            );
            //end block

            //_lavaNode = services.GetRequiredService<LavaNode>();
            //victoriaservice = new VictoriaService(_lavaNode, client);
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

                //_lavaNode.ConnectAsync();
                //new Hazuki().RunBotAsync().GetAwaiter().GetResult();
                return Task.CompletedTask;
            };

            //// Block this task until the program is closed.

            await Task.Delay(1000);
        }


        /// <summary>
        ///     Stops the bot asynchronously.
        /// </summary>
        /// <returns>a task that represents the asynchronous operation</returns>
        public async Task StopAsync() => await client.StopAsync();

        public async Task LeftGuild(SocketGuild guild)
        {
            Console.WriteLine($"Bot lefted from: {guild.Name}");
            Config.Guild.removeGuildConfigFile(guild.Id.ToString());
        }

        public async Task JoinedGuild(SocketGuild guild)
        {
            //Config.Music.storedLavaTrack[guild.Id.ToString()] = new List<LavaTrack>();
            var systemChannel = client.GetChannel(guild.SystemChannel.Id) as SocketTextChannel; // Gets the channel to send the message in
            await systemChannel.SendMessageAsync($"Pretty witchy {MentionUtils.MentionUser(Config.Doremi.Id)} chi~ has arrived to the {guild.Name}. " +
                $"Thank you everyone for inviting me up, I'm very happy to meet you all. " +
                $"You can ask me with `{Config.Doremi.PrefixParent[0]}help` for all commands list.",
            embed: new EmbedBuilder()
            .WithColor(Config.Doremi.EmbedColor)
            .WithImageUrl("https://i.pinimg.com/474x/e7/1c/ce/e71cce7499e4ea9f9520c6143c9672e7.jpg")
            .Build());

            Console.WriteLine($"Doremi Bot joined into: {guild.Name}");
            Config.Guild.init(guild.Id);
        }

        public async Task GuildAvailable(SocketGuild guild)
        {
            Config.Guild.init(guild.Id);
            //set birthday announcement timer
            if (Config.Guild.hasPropertyValues(guild.Id.ToString(), "id_birthday_announcement"))
            {
                Config.Doremi._timerBirthdayAnnouncement[guild.Id.ToString()] = new Timer(async _ =>
                {
                    var guildId = guild.Id; DateTime date; Boolean birthdayExisted = false;

                    //announce hazuki birthday
                    if (Config.Hazuki.Status.isBirthday())
                    {
                        await client
                        .GetGuild(guild.Id)
                        .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guild.Id, "id_birthday_announcement")))
                        .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday to you, {MentionUtils.MentionUser(Config.Hazuki.Id)} chan. " +
                        $"She has turned into {Config.Hazuki.birthdayCalculatedYear} on this year. Let's give wonderful birthday wishes for her.");
                        birthdayExisted = true;
                    }

                    //announce aiko birthday
                    if (Config.Aiko.Status.isBirthday())
                    {
                        await client
                        .GetGuild(guild.Id)
                        .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guild.Id, "id_birthday_announcement")))
                        .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday to our dear osakan friend: {MentionUtils.MentionUser(Config.Aiko.Id)} chan. " +
                        $"She has turned into {Config.Aiko.birthdayCalculatedYear} on this year. Let's give some takoyaki and wonderful birthday wishes for her.");
                        birthdayExisted = true;
                    }

                    //announce onpu birthday
                    if (Config.Onpu.Status.isBirthday())
                    {
                        await client
                        .GetGuild(guild.Id)
                        .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guild.Id, "id_birthday_announcement")))
                        .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday to our wonderful idol friend: {MentionUtils.MentionUser(Config.Onpu.Id)} chan. " +
                        $"She has turned into {Config.Onpu.birthdayCalculatedYear} on this year. Let's give some wonderful birthday wishes for her.");
                        birthdayExisted = true;
                    }

                    //announce momoko birthday
                    if (Config.Momoko.Status.isBirthday())
                    {
                        await client
                        .GetGuild(guild.Id)
                        .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guild.Id, "id_birthday_announcement")))
                        .SendMessageAsync($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday to our wonderful friend: {MentionUtils.MentionUser(Config.Momoko.Id)} chan. " +
                        $"She has turned into {Config.Momoko.birthdayCalculatedYear} on this year. Let's give some wonderful birthday wishes for her.");
                        birthdayExisted = true;
                    }

                    var guildJsonFile = (JObject)JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json")).GetValue("user_birthday");
                    var jobjbirthday = guildJsonFile.Properties().ToList();

                    for (int i = 0; i < jobjbirthday.Count; i++)
                    {
                        var key = jobjbirthday[i].Name; var val = jobjbirthday[i].Value.ToString();
                        try
                        {
                            var user = guild.GetUser(Convert.ToUInt64(key));

                            if ((DateTime.TryParseExact(val, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date) ||
                                DateTime.TryParseExact(val, "dd/MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) &&
                                (Int32.Parse(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour &&
                                Int32.Parse(DateTime.Now.ToString("HH")) <= Config.Core.maxGlobalTimeHour))
                            {
                                if (date.ToString("dd/MM") == DateTime.Now.ToString("dd/MM"))
                                {

                                    string[] arrRandomedMessage = {
                                    $"{Config.Emoji.birthdayCake} Everyone, let's give a wonderful birthday wishes for: {MentionUtils.MentionUser(user.Id)} ",
                                    $"{Config.Emoji.birthdayCake} Happy birthday to our wonderful friend: {MentionUtils.MentionUser(user.Id)} . " +
                                    $"Please give the wonderful birthday wishes for {MentionUtils.MentionUser(user.Id)}.",
                                    $"{Config.Emoji.birthdayCake} Everyone, we have important birthday announcement! Please give some wonderful birthday wishes for {MentionUtils.MentionUser(user.Id)}."
                                };
                                    var birthdayMessage = arrRandomedMessage[new Random().Next(0, arrRandomedMessage.Length)];

                                    try
                                    {
                                        await client
                                        .GetGuild(guild.Id)
                                        .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guild.Id, "id_birthday_announcement")))
                                        .SendMessageAsync(birthdayMessage);
                                    }
                                    catch
                                    {
                                        Console.WriteLine($"Doremi Birthday Announcement Exception: Send message permissions has been missing on {guild.Name}");
                                    }
                                    birthdayExisted = true;
                                }
                            }
                        }
                        catch
                        {
                            //guildJsonFile.Property(key).Remove();
                            //File.WriteAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json", guildJsonFile.ToString());
                        }

                    }

                    if (birthdayExisted)
                    {
                        EmbedBuilder builder = new EmbedBuilder();
                        builder.ThumbnailUrl = "https://i.4pcdn.org/s4s/1508005628768.jpg";
                        builder.Color = Config.Doremi.EmbedColor;

                        await client
                        .GetGuild(guild.Id)
                        .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guild.Id, "id_birthday_announcement")))
                        .SendMessageAsync(embed: builder.Build());
                    }

                },
                null,
                TimeSpan.FromSeconds(10), //time to wait before executing the timer for the first time
                TimeSpan.FromHours(24) //time to wait before executing the timer again
                );
            }

            //set random event timer
            if (Config.Guild.hasPropertyValues(guild.Id.ToString(), "id_random_event"))
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
                        .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guild.Id, "id_random_event")))
                        .SendMessageAsync(Config.Doremi.listRandomEvent[rndIndex]);
                    }
                    catch
                    {
                        Console.WriteLine($"Doremi Random Event Exception: Send message permissions has been missing on {guild.Name}");
                    }
                },
                null,
                TimeSpan.FromHours(Config.Doremi.Randomeventinterval), //time to wait before executing the timer for the first time
                TimeSpan.FromHours(Config.Doremi.Randomeventinterval) //time to wait before executing the timer again
                );
            }

            //init variable
            //Config.Doremi._tradingCardSpawnedId[guild.Id.ToString()] = "";
            //Config.Doremi._tradingCardSpawnedCategory[guild.Id.ToString()] = "";

            //set random card spawn timer
            if (Config.Guild.hasPropertyValues(guild.Id.ToString(), "trading_card_spawn"))
            {
                Config.Doremi._stopwatchCardSpawn[guild.Id.ToString()] = new Stopwatch();
                Config.Doremi._stopwatchCardSpawn[guild.Id.ToString()].Start();

                Config.Doremi._timerTradingCardSpawn[guild.Id.ToString()] = new Timer(async _ =>
                {
                    if (Config.Doremi._stopwatchCardSpawn.ContainsKey(guild.Id.ToString()))
                        if (Config.Doremi._stopwatchCardSpawn[guild.Id.ToString()].IsRunning)
                            Config.Doremi._stopwatchCardSpawn[guild.Id.ToString()].Restart();
                    
                    await TradingCardCore.generateCardSpawn(guild.Id);
                },
                null,
                TimeSpan.FromMinutes(Convert.ToInt32(Config.Guild.getPropertyValue(guild.Id, "trading_card_spawn_interval"))), //time to wait before executing the timer for the first time
                TimeSpan.FromMinutes(Convert.ToInt32(Config.Guild.getPropertyValue(guild.Id, "trading_card_spawn_interval"))) //time to wait before executing the timer again
                );
            }

            //var channel = client.GetChannel(guild.SystemChannel.Id) as SocketTextChannel;
            //await channel.SendMessageAsync(guild.SystemChannel.Id.ToString());


            Config.Guild.init(guild.Id);
            //Config.Music.storedLavaTrack[guild.Id.ToString()] = new List<LavaTrack>();
        }

        /// <summary>
        ///     Unregisters the events attached to the discord client.
        /// </summary>
        public void Dispose() => client.MessageReceived -= HandleCommandAsync;

        public async Task AnnounceJoinedUser(SocketGuildUser user) //Welcomes the new user
        {
            var channel = client.GetChannel(user.Guild.SystemChannel.Id) as SocketTextChannel; // Gets the channel to send the message in
            //var uses = invites.First(x => x.Code == inviteID).Uses.Value;

            if (user.Id != Config.Hazuki.Id && user.Id != Config.Aiko.Id &&
                user.Id != Config.Onpu.Id && user.Id != Config.Momoko.Id)
            {
                string[] arrRandomWelcomeMessage = {
                    $"We hope you enjoy and happy with all of us, {MentionUtils.MentionUser(user.Id)}.",
                    $"We're really happy that you join our group, {MentionUtils.MentionUser(user.Id)}.",
                    $"We're expecting you to come and hopefully you're enjoying your stay on the group, {MentionUtils.MentionUser(user.Id)}.",
                    $"Hope you enjoy your stay, {MentionUtils.MentionUser(user.Id)}."
                };

                string[] arrRandomPictures =
                {"https://66.media.tumblr.com/c8f9c5455355f8e522d52bacb8155ab0/tumblr_mswho8nWx11r98a5go1_400.gif",
                "https://thumbs.gfycat.com/DamagedGrouchyBarracuda-small.gif",
                "https://data.whicdn.com/images/39976659/original.gif",
                "https://cdn140.picsart.com/316023608328211.png?type=webp&to=min&r=640",
                "https://cdn.discordapp.com/attachments/706770454697738300/706770708147208323/the-ojamajos.png",
                "https://cdn.discordapp.com/attachments/706770454697738300/706770837558263928/TW361403.png"};

                int rndIndexWelcomeMessage = new Random().Next(0, arrRandomWelcomeMessage.GetLength(0));
                int rndIndexRandomPictures = new Random().Next(0, arrRandomPictures.GetLength(0));

                await channel.SendMessageAsync(embed: new EmbedBuilder()
                    .WithTitle($"Welcome to {channel.Guild.Name}, {user.Username}")
                    .WithDescription($"{arrRandomWelcomeMessage[rndIndexWelcomeMessage]} " +
                    $"Please introduce yourself and don't forget to always follow & read the rule guidelines.")
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithThumbnailUrl(user.GetAvatarUrl())
                    .WithImageUrl(arrRandomPictures[rndIndexRandomPictures])
                    .Build());

                //sending dm to the joined user
                //var dmchannel = await user.GetOrCreateDMChannelAsync();
                //await dmchannel.SendMessageAsync(arrRandomWelcomeMessage[rndIndexWelcomeMessage] +
                //    " Please introduce yourself on the group, also don't forget to always follow and read the rule guidelines :smile:",
                //    embed: new EmbedBuilder()
                //            .WithColor(Config.Doremi.EmbedColor)
                //            .WithImageUrl(arrRandomPictures[rndIndexRandomPictures])
                //            .Build());
            }


        }

        public async Task AnnounceLeavingUser(SocketGuildUser user) //Send a leaving user notifications
        {
            var channel = client.GetChannel(user.Guild.SystemChannel.Id) as SocketTextChannel; // Gets the channel to send the message in

            JObject guildConfig = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{user.Guild.Id}/{user.Guild.Id}.json"));

            //remove user birthday
            if (((JObject)guildConfig["user_birthday"]).ContainsKey(user.Id.ToString()))
            {
                ((JObject)guildConfig.GetValue("user_birthday")).Remove(user.Id.ToString());
                File.WriteAllText($"{Config.Core.headConfigGuildFolder}{user.Guild.Id}/{user.Guild.Id}.json", guildConfig.ToString());
            }

            if (guildConfig.GetValue("user_leaving_notification").ToString() == "1")
            {
                string[] arrRandomLeavingMessage = {
                   $":sob: Oh no, {user.Mention} has leave the {channel.Guild.Name} and we are really sad.",
                   $":sob: It's nice to have {user.Mention} on {channel.Guild.Name}. We wish that you can stay more longer on our groups."
                };

                string[] arrRandomPictures = {"https://vignette.wikia.nocookie.net/ojamajowitchling/images/d/d7/ODN-EP1-013.png",
                "https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e1/Linesticker39.png"};

                await channel.SendMessageAsync(arrRandomLeavingMessage[new Random().Next(0, arrRandomLeavingMessage.GetLength(0))],
                    embed: new EmbedBuilder()
                            .WithColor(Config.Doremi.EmbedColor)
                            .WithImageUrl(arrRandomPictures[new Random().Next(0, arrRandomPictures.GetLength(0))])
                            .Build()); //say goodbye to user
            }
        }

        public void HookReactionAdded(BaseSocketClient client) => client.ReactionAdded += HandleReactionAddedAsync;
        public void HookReactionRemoved(BaseSocketClient client) => client.ReactionRemoved += HandleReactionRemovedAsync;

        public async Task HandleReactionAddedAsync(Cacheable<IUserMessage, ulong> cachedMessage,
        ISocketMessageChannel originChannel, SocketReaction reaction)
        {
            //https://discordapp.com/channels/646244365928497162/651069058556362753/719763968171966545

            var message = await cachedMessage.GetOrDownloadAsync();
            //var context = new SocketCommandContext(client, cachedMessage);
            var messageId = message.GetJumpUrl().Split('/').Last();
            var guildId = message.GetJumpUrl().Split('/')[4];
            SocketGuildUser guildUser = (SocketGuildUser)reaction.User;
            var channel = client.GetChannel(originChannel.Id) as SocketTextChannel;
            if (message != null && reaction.User.IsSpecified)
            {
                JObject guildConfig = JObject.Parse(File.ReadAllText($"{Config.Core.headConfigGuildFolder}{guildId}/{guildId}.json"));
                //custom reaction
                if (!guildUser.IsBot)
                {
                    try
                    {
                        //non custom role emoticon
                        var dataReaction = (JObject)guildConfig["roles_react"][messageId]["data"];
                        if (dataReaction.ContainsKey(reaction.Emote.ToString()))
                        {
                            //if (!Config.Doremi._imReactionRole.ContainsKey(guildId))
                            //    Config.Doremi._imReactionRole.Add(guildId, new List<IMessage>());

                            var roleId = guildConfig["roles_react"][messageId]["data"][reaction.Emote.ToString()];

                            var roleMaster = channel.Guild.Roles.FirstOrDefault(x => x.Id == Convert.ToUInt64(roleId));
                            var roleSearch = guildUser.Roles.FirstOrDefault(x => x.Id == Convert.ToUInt64(roleId));
                            if (roleSearch == null)
                            {
                                await guildUser.AddRoleAsync(roleMaster);

                                //IMessage im = await originChannel.SendMessageAsync(embed: new EmbedBuilder()
                                //.WithColor(Config.Doremi.EmbedColor)
                                //.WithDescription($":white_check_mark: {MentionUtils.MentionUser(guildUser.Id)} " +
                                //$"have been assigned with new role: " +
                                //$"{MentionUtils.MentionRole(roleMaster.Id)}")
                                //.Build());

                                //sending dm notification
                                var dmchannel = await guildUser.GetOrCreateDMChannelAsync();
                                await dmchannel.SendMessageAsync(embed: new EmbedBuilder()
                                            .WithColor(Config.Doremi.EmbedColor)
                                            .WithTitle("Your role has been set!")
                                            .WithDescription($":white_check_mark: You have been assigned with the new role: **{roleMaster.Name}**")
                                            .WithThumbnailUrl(TradingCardCore.Doremi.emojiOk)
                                            .WithFooter($"From: {channel.Guild.Name}")
                                            .Build());

                                //new Timer(async _ =>
                                //{
                                //    if (im == default || im == null) return;
                                //    else
                                //    {
                                //        await Task.Run(async () =>
                                //        {
                                //            await im.DeleteAsync();
                                //        });
                                //    }
                                //},
                                //null,
                                //20000,
                                //Timeout.Infinite //time to wait before executing the timer again
                                //);

                                //IMessage im = await originChannel.SendMessageAsync(embed: new EmbedBuilder()
                                //.WithColor(Config.Doremi.EmbedColor)
                                //.WithDescription($":white_check_mark: {MentionUtils.MentionUser(guildUser.Id)} now have new role: " +
                                //$"{MentionUtils.MentionRole(roleMaster.Id)}")
                                //.Build());
                                //Config.Doremi._imReactionRole[guildId].Add(im);

                                //new Timer(async _ =>
                                //{
                                //    for (int i = 0; i < Config.Doremi._imReactionRole[guildId].Count(); i++)
                                //    {
                                //        await Config.Doremi._imReactionRole[guildId][i].DeleteAsync();
                                //    }
                                //    Config.Doremi._imReactionRole[guildId].Clear();
                                //},
                                //null,
                                //20000,
                                //Timeout.Infinite //time to wait before executing the timer again
                                //);
                            }
                            else
                            {
                                await guildUser.RemoveRoleAsync(roleMaster);

                                //IMessage im = await originChannel.SendMessageAsync(embed: new EmbedBuilder()
                                //.WithColor(Config.Doremi.EmbedColor)
                                //.WithDescription($":x: {MentionUtils.MentionUser(guildUser.Id)} removed from the role: " +
                                //$"**{roleMaster.Name}**")
                                //.Build());

                                //send dm notification
                                var dmchannel = await guildUser.GetOrCreateDMChannelAsync();
                                await dmchannel.SendMessageAsync(embed: new EmbedBuilder()
                                .WithColor(Config.Doremi.EmbedColor)
                                .WithTitle("Your role has been removed.")
                                .WithDescription($":x: You have been removed from the role: **{roleMaster.Name}**")
                                .WithThumbnailUrl(TradingCardCore.Doremi.emojiOk)
                                .WithFooter($"From: {channel.Guild.Name}")
                                .Build());

                                //IMessage im = await originChannel.SendMessageAsync(embed: new EmbedBuilder()
                                //    .WithColor(Config.Doremi.EmbedColor)
                                //    .WithDescription($":white_check_mark: {MentionUtils.MentionUser(guildUser.Id)} have been removed from the role: " +
                                //    $"{MentionUtils.MentionRole(roleMaster.Id)}")
                                //    .Build());
                                //Config.Doremi._imReactionRole[guildId].Add(im);

                                //try
                                //{
                                //    if (Config.Doremi._imReactionRole[guildId].Count() >= 1)
                                //    {
                                //        new Timer(async _ =>
                                //        {
                                //            for (int i = 0; i < Config.Doremi._imReactionRole[guildId].Count(); i++)
                                //            {
                                //                await Config.Doremi._imReactionRole[guildId][i].DeleteAsync();
                                //            }

                                //            Config.Doremi._imReactionRole[guildId].Clear();

                                //            //await im.DeleteAsync();
                                //            //await originChannel.DeleteMessageAsync(im.Id);
                                //            //await im.DeleteAsync();
                                //        },
                                //        null,
                                //        30000,
                                //        Timeout.Infinite //time to wait before executing the timer again
                                //        );
                                //    }

                                //}
                                //catch { }
                            }

                            await message.RemoveReactionAsync(reaction.Emote, guildUser);

                        }
                    }
                    catch (Exception e)
                    {
                    }
                } else if (reaction.Emote.Equals(new Discord.Emoji("\u2B50")))
                {
                    //star react
                    if (message.Reactions.TryGetValue(new Discord.Emoji("\u2B50"), out var metadata))
                    {
                        if (message.Author.Id == Config.Doremi.Id && metadata.ReactionCount >= 5 && !message.IsPinned)
                        {
                            string splittedMentionedId = (message.Content.ToString().Split(">")[0]).Replace("<@!", "");

                            var getMentionedUserId = Int64.Parse(splittedMentionedId);

                            await client_log(new LogMessage(0, "Reaction pinned:", $"{message.Author}'s : {message.Id} has enough reactions."));
                            await originChannel.SendMessageAsync(embed: new EmbedBuilder()
                            .WithColor(Config.Doremi.EmbedColor)
                            .WithDescription($"{Config.Emoji.clap} Congratulations, I will now pin " +
                                $"{MentionUtils.MentionUser((ulong)getMentionedUserId)}'s message.")
                            .WithThumbnailUrl("https://static.zerochan.net/Harukaze.Doremi.full.2494232.gif")
                            .Build());
                            await message.PinAsync();
                        }
                    }

                    //Console.WriteLine(reactionstars);

                    //Console.WriteLine($"{reaction.User.Value} just added a reaction '{reaction.Emote}' " +
                    //$"to {message.Author}'s message ({message.Id}).");
                }
            }
        }

        public async Task HandleReactionRemovedAsync(Cacheable<IUserMessage, ulong> cachedMessage,
        ISocketMessageChannel originChannel, SocketReaction reaction)
        {
            //var message = await cachedMessage.GetOrDownloadAsync();
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
            SocketUserMessage usrmsg = (SocketUserMessage)message;

            try
            {
                using (StreamWriter sw = (File.Exists($"logs/{textchannel.Guild.Id}/{DateTime.Now.ToString("yyyy_MM_dd")}.txt")) ? File.AppendText($"logs/{textchannel.Guild.Id}/{DateTime.Now.ToString("yyyy_MM_dd")}.txt") :
                    File.CreateText($"logs/{textchannel.Guild.Id}/{DateTime.Now.ToString("yyyy_MM_dd")}.txt"))
                    sw.WriteLine($"[{DateTime.Now.ToString("yyyy/MM/dd HH:mm")}] {usrmsg.Author.Mention}{usrmsg.Author.Username} : {message}");
            }
            catch { }

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
            try
            {
                var message = before.GetOrDownloadAsync();

                using (StreamWriter sw = (File.Exists($"logs/{textchannel.Guild.Id}/{DateTime.Now.ToString("yyyy_MM_dd")}.txt")) ? File.AppendText($"logs/{textchannel.Guild.Id}/{DateTime.Now.ToString("yyyy_MM_dd")}.txt") :
                    File.CreateText($"logs/{textchannel.Guild.Id}/{DateTime.Now.ToString("yyyy_MM_dd")}.txt"))
                    sw.WriteLine($"[{DateTime.Now.ToString("yyyy/MM/dd HH:mm")}] {usrmsg.Author.Mention}{usrmsg.Author.Username} : [update]{after}");
            }
            catch { }
            return Task.CompletedTask;
            //using (StreamWriter w = File.AppendText($"attachments/{Context.Guild.Id}/feedback_{Context.Guild.Id}.txt"))
            //    w.WriteLine($"[{DateTime.Now.ToString("MM/dd/yyyy HH:mm")}]{message}:{feedback_message}");
        }

        public async Task RegisterCommandsAsync()
        {
            //commands.CommandExecuted += OnCommandExecutedAsync;
            client.MessageReceived += HandleCommandAsync;

            await commands.AddModuleAsync(typeof(DoremiModule), services);
            await commands.AddModuleAsync(typeof(DorememesModule), services);
            await commands.AddModuleAsync(typeof(DoremiRoles), services);
            await commands.AddModuleAsync(typeof(DoremiBirthdayModule), services);
            await commands.AddModuleAsync(typeof(DoremiMinigameInteractive), services);
            await commands.AddModuleAsync(typeof(DoremiWiki), services);
            await commands.AddModuleAsync(typeof(DoremiModerator), services);
            //await commands.AddModuleAsync(typeof(DoremiModeratorChannels), services);
            await commands.AddModuleAsync(typeof(DoremiMagicalStageModule), services);
            await commands.AddModuleAsync(typeof(DoremiGardenInteractive), services);
            await commands.AddModuleAsync(typeof(DoremiTradingCardInteractive), services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(client, message);

            if (message.Author.Id == Config.Doremi.Id) return;
            //if (message.Author.IsBot) return; //prevent any bot from sending the commands

            //for copied and pasted if mentioned
            int argPos = 0;
            if (Config.Guild.getPropertyValue(context.Guild.Id, "doremi_role_id") != "" &&
                message.HasStringPrefix($"<@&{Config.Guild.getPropertyValue(context.Guild.Id, "doremi_role_id")}>", ref argPos))
            {
                await message.Channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithAuthor(Config.Doremi.EmbedNameError)
                .WithDescription($"Sorry {context.User.Username}, it seems you're calling me with the role prefix. " +
                            "Please use the non role prefix.")
                .WithColor(Config.Doremi.EmbedColor)
                .WithThumbnailUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/d/d2/ODN-EP1-011.png")
                .Build());
            }
            else if (message.HasStringPrefix(Config.Doremi.PrefixParent[0], ref argPos) ||
              message.HasStringPrefix(Config.Doremi.PrefixParent[1], ref argPos) ||
              message.HasMentionPrefix(client.CurrentUser, ref argPos))
            {
                var result = await commands.ExecuteAsync(context, argPos, services);

                switch (result.Error)
                {
                    case CommandError.BadArgCount:
                        await context.Channel.SendMessageAsync("Oops, looks like you have missing/too much parameter. " +
                            $"See `{Config.Doremi.PrefixParent[0]}help <commands or category>` for commands help.");
                        break;
                    case CommandError.UnknownCommand:
                        await message.Channel.SendMessageAsync(embed: new EmbedBuilder()
                            .WithAuthor(Config.Doremi.EmbedNameError)
                            .WithDescription("Sorry, I can't find that commands. " +
                            $"See `{Config.Doremi.PrefixParent[0]}help <commands or category>` for commands help.")
                            .WithColor(Config.Doremi.EmbedColor)
                            .WithThumbnailUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/c/ca/ODN-EP11-041.png")
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
                        //Console.WriteLine(result.ErrorReason);
                        break;
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