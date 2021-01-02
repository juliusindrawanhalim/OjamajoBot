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
using OjamajoBot.Database;
using OjamajoBot.Database.Model;
using System.Data;
using OjamajoBot.Core;
using System.Text.RegularExpressions;

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
        public static Stopwatch stopwatchWeather = new Stopwatch();

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
                }
                else if (randomWeather <= 40)
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

                for (int i = 0; i <= 4; i++)
                {
                    GardenCore.weather[i] = GardenCore.arrRandomWeather[selectedWeatherIndex, i];
                }

                stopwatchWeather.Start();

                if (stopwatchWeather.IsRunning)
                    stopwatchWeather.Restart();

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
            Console.WriteLine($"Doremi bot leaving from: {guild.Name}");
            //Config.Guild.removeGuildConfigFile(guild.Id.ToString());
        }

        public async Task JoinedGuild(SocketGuild guild)
        {
            var systemChannel = client.GetChannel(guild.SystemChannel.Id) as SocketTextChannel; // Gets the channel to send the message in
            await systemChannel.SendMessageAsync(
            embed: new EmbedBuilder()
            .WithColor(Config.Doremi.EmbedColor)
            .WithTitle($"Pretty witchy Doremi chi!")
            .WithDescription($"Thank you everyone for inviting me to {guild.Name}, I'm very happy to meet you all. " +
                $"You can call me with `{Config.Doremi.PrefixParent[0]}` as my default prefix or " +
                $"ask me with `{Config.Doremi.PrefixParent[0]}help` for all command list that I have.")
            .WithImageUrl("https://cdn.discordapp.com/attachments/706812034544697404/706815811406266478/dokkan.gif")
            .Build());

            Console.WriteLine($"Doremi bot joined into: {guild.Name}");
            Config.Guild.init(guild.Id);
        }

        public async Task GuildAvailable(SocketGuild guild)
        {
            Config.Guild.init(guild.Id);

            ulong guildId = guild.Id;
            var guildData = Config.Guild.getGuildData(guildId);
            string guildBirthdayLastAnnouncement = "";
            if (guildData[DBM_Guild.Columns.birthday_announcement_date_last].ToString() != "")
                guildBirthdayLastAnnouncement = guildData[DBM_Guild.Columns.birthday_announcement_date_last].ToString();
            else
                guildBirthdayLastAnnouncement = "1";

            if (guildData[DBM_Guild.Columns.id_channel_birthday_announcement].ToString() != "")
            {
                Config.Doremi._timerBirthdayAnnouncement[guild.Id.ToString()] = new Timer(async _ =>
                {
                guildData = Config.Guild.getGuildData(guildId);
                if (guildData[DBM_Guild.Columns.birthday_announcement_date_last].ToString() != "")
                    guildBirthdayLastAnnouncement = guildData[DBM_Guild.Columns.birthday_announcement_date_last].ToString();
                else
                    guildBirthdayLastAnnouncement = "1";

                EmbedBuilder eb = new EmbedBuilder()
                .WithColor(Config.Doremi.EmbedColor);

                //set birthday announcement timer
                if (guildBirthdayLastAnnouncement != DateTime.Now.ToString("dd") &&
                Convert.ToInt32(DateTime.Now.ToString("HH")) >= Config.Core.minGlobalTimeHour)
                {
                    Boolean birthdayExisted = false;

                    //announce hazuki birthday
                    if (Config.Hazuki.Status.isBirthday() &&
                    Convert.ToInt32(guildData[DBM_Guild.Columns.birthday_announcement_ojamajo]) == 1)
                    {
                        eb = eb.WithTitle($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy Birthday, Hazuki Chan!")
                        .WithDescription($"Happy birthday to Hazuki chan today. " +
                        $"She has turned into {Config.Hazuki.birthdayCalculatedYear} on this year. Let's give wonderful birthday wishes for her.")
                        .WithImageUrl("https://cdn.discordapp.com/attachments/706770454697738300/791510757778784306/hazuki_birthday.jpg");
                        await client
                        .GetGuild(guildId)
                        .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement]))
                        .SendMessageAsync(embed: eb.Build());
                        birthdayExisted = true;
                    }

                    //announce aiko birthday
                    if (Config.Aiko.Status.isBirthday() &&
                    Convert.ToInt32(guildData[DBM_Guild.Columns.birthday_announcement_ojamajo]) == 1)
                    {
                        eb = eb.WithTitle($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy Birthday, Aiko Chan!")
                        .WithDescription($"Happy birthday to our dear osakan friend: Aiko chan. " +
                        $"She has turned into {Config.Aiko.birthdayCalculatedYear} on this year. Let's give some takoyaki and wonderful birthday wishes for her.")
                        .WithImageUrl("https://cdn.discordapp.com/attachments/706770454697738300/791579373324992512/happy_birthday_aiko_chan.jpg")
                        .WithFooter("Art By: Letter Three");

                        await client
                        .GetGuild(guildId)
                        .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement]))
                        .SendMessageAsync(embed: eb.Build());
                        birthdayExisted = true;
                    }

                    //announce onpu birthday
                    if (Config.Onpu.Status.isBirthday() &&
                    Convert.ToInt32(guildData[DBM_Guild.Columns.birthday_announcement_ojamajo]) == 1)
                    {
                        eb = eb.WithTitle("{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy Birthday, Onpu Chan!")
                        .WithDescription($"Happy birthday to our wonderful idol friend: Onpu chan. " +
                        $"She has turned into {Config.Onpu.birthdayCalculatedYear} on this year. Let's give some wonderful birthday wishes for her.")
                        .WithImageUrl("https://cdn.discordapp.com/attachments/706770454697738300/790803480482021377/Onpu__Nintendo_Switch_Birthday_Pic.png")
                        .WithFooter("Art By: Letter Three");

                        await client
                        .GetGuild(guildId)
                        .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement]))
                        .SendMessageAsync(embed: eb.Build());
                        birthdayExisted = true;
                    }

                    //announce momoko birthday
                    if (Config.Momoko.Status.isBirthday() &&
                    Convert.ToInt32(guildData[DBM_Guild.Columns.birthday_announcement_ojamajo]) == 1)
                    {
                        eb = eb.WithTitle($"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy Birthday, Momoko Chan!")
                        .WithDescription($"Happy birthday to our wonderful friend: Momoko chan. " +
                        $"She has turned into {Config.Momoko.birthdayCalculatedYear} on this year. Let's give some wonderful birthday wishes for her.")
                        .WithImageUrl("https://cdn.discordapp.com/attachments/706770454697738300/790803547209203722/Momoko_Birthday_Pic.png")
                        .WithFooter("Art By: Letter Three");

                        await client
                        .GetGuild(guildId)
                        .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement]))
                        .SendMessageAsync(embed: eb.Build());
                        birthdayExisted = true;
                    }

                    //announce member birthday
                    DBC db = new DBC();
                    string query = @$"select * 
                        from {DBM_Guild_User_Birthday.tableName} 
                        where {DBM_Guild_User_Birthday.Columns.id_guild}=@{DBM_Guild_User_Birthday.Columns.id_guild} and 
                        month({DBM_Guild_User_Birthday.Columns.birthday_date}) = month(curdate()) and 
                        day({DBM_Guild_User_Birthday.Columns.birthday_date}) = day(curdate())";

                    Dictionary<string, object> colSelect = new Dictionary<string, object>();
                    colSelect[DBM_Guild_User_Birthday.Columns.id_guild] = guildId.ToString();

                    var result = db.selectAll(query, colSelect);
                    string birthdayPeople = "";

                        foreach (DataRow row in result.Rows)
                        {
                            //check if user exists/not on the server
                            try
                            {
                                SocketUser masterUser =
                                guild.Users.FirstOrDefault(x => x.Id ==
                                Convert.ToUInt64(row[DBM_Guild_User_Birthday.Columns.id_user].ToString()));
                                if (masterUser != null)
                                {
                                    birthdayPeople += $"{MentionUtils.MentionUser(Convert.ToUInt64(row[DBM_Guild_User_Birthday.Columns.id_user].ToString()))} ";
                                        birthdayExisted = true;
                                }
                            }
                                catch (Exception e) { }
                        }

                        if (birthdayExisted)
                        {
                            if (birthdayPeople != "")
                            {
                                //check if custom birthday message exists/not
                                query = $"SELECT * " +
                                $" FROM {DBM_Guild_Custom_Birthday.tableName} " +
                                $" WHERE {DBM_Guild_Custom_Birthday.Columns.id_guild}=@{DBM_Guild_Custom_Command.Columns.id_guild} " +
                                $" ORDER BY RAND() " +
                                $" LIMIT 1";
                                colSelect = new Dictionary<string, object>();
                                colSelect[DBM_Guild_Custom_Birthday.Columns.id_guild] = guildId.ToString();
                                var resultCustomBirthday = new DBC().selectAll(query, colSelect);
                                if (resultCustomBirthday.Rows.Count >= 1)
                                {
                                    foreach (DataRow row in resultCustomBirthday.Rows)
                                    {
                                        var custBirthdayMessage = row[DBM_Guild_Custom_Birthday.Columns.message].ToString();
                                        var custBirthdayImage = row[DBM_Guild_Custom_Birthday.Columns.img_url].ToString();

                                        await client
                                        .GetGuild(guildId)
                                        .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement].ToString()))
                                        .SendMessageAsync(
                                            $"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday to: {birthdayPeople}",
                                            embed: new EmbedBuilder()
                                            .WithColor(Config.Doremi.EmbedColor)
                                            .WithDescription(custBirthdayMessage)
                                            .WithImageUrl(custBirthdayImage)
                                            .WithFooter($"Best wishes from: {guild.Name} & friends")
                                            .Build());
                                    }
                                }
                                else
                                {
                                    string[] arrRandomedMessage = {
                                        $"Everyone, please give some wonderful birthday wishes. ",
                                        $"Wishing you all the best and hapiness always."
                                    };

                                    await client
                                    .GetGuild(guildId)
                                    .GetTextChannel(Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_birthday_announcement].ToString()))
                                    .SendMessageAsync(
                                        $"{Config.Emoji.partyPopper}{Config.Emoji.birthdayCake} Happy birthday to: {birthdayPeople}",
                                        embed: new EmbedBuilder()
                                        .WithColor(Config.Doremi.EmbedColor)
                                        .WithDescription(arrRandomedMessage[new Random().Next(0, arrRandomedMessage.Length)])
                                        .WithImageUrl("https://media.discordapp.net/attachments/706770454697738300/745492527070576670/1508005628768.png")
                                        .WithFooter($"Best wishes from: {guild.Name} & friends")
                                        .Build());
                                }
                            }
                            
                            query = @$"UPDATE {DBM_Guild.tableName} 
                                SET {DBM_Guild.Columns.birthday_announcement_date_last}=@{DBM_Guild.Columns.birthday_announcement_date_last} 
                                WHERE {DBM_Guild.Columns.id_guild}=@{DBM_Guild.Columns.id_guild}";
                            Dictionary<string, object> columnsFilter = new Dictionary<string, object>();
                            columnsFilter[DBM_Guild.Columns.birthday_announcement_date_last] = DateTime.Now.ToString("dd");
                            columnsFilter[DBM_Guild.Columns.id_guild] = guildId.ToString();
                            new DBC().update(query, columnsFilter);
                        }
                    }
                },
                null,
                TimeSpan.FromSeconds(10), //time to wait before executing the timer for the first time
                TimeSpan.FromMinutes(40) //time to wait before executing the timer again
            );
        }

                //set random event timer
                //if (Config.Guild.hasPropertyValues(guild.Id.ToString(), "id_random_event"))
                //{
                //    //start rotates random event
                //    Config.Doremi._timerRandomEvent[$"{guild.Id.ToString()}"] = new Timer(async _ =>
                //    {
                //        Random rnd = new Random();
                //        int rndIndex = rnd.Next(0, Config.Doremi.listRandomEvent.Count); //random the list value
                //        Console.WriteLine("Doremi Random Event : " + Config.Doremi.listRandomEvent[rndIndex]);
                //        try
                //        {
                //            await client
                //            .GetGuild(guild.Id)
                //            .GetTextChannel(Convert.ToUInt64(Config.Guild.getPropertyValue(guild.Id, "id_random_event")))
                //            .SendMessageAsync(Config.Doremi.listRandomEvent[rndIndex]);
                //        }
                //        catch
                //        {
                //            Console.WriteLine($"Doremi Random Event Exception: Send message permissions has been missing on {guild.Name}");
                //        }
                //    },
                //    null,
                //    TimeSpan.FromHours(Config.Doremi.Randomeventinterval), //time to wait before executing the timer for the first time
                //    TimeSpan.FromHours(Config.Doremi.Randomeventinterval) //time to wait before executing the timer again
                //    );
                //}

                //trading card timer
                var guildCardSpawnData = TradingCardGuildCore.getGuildData(guildId);
                if (guildCardSpawnData[DBM_Trading_Card_Guild.Columns.id_channel_spawn].ToString() != "")
                {
                    Config.Doremi._stopwatchCardSpawn[guild.Id.ToString()] = new Stopwatch();
                    Config.Doremi._stopwatchCardSpawn[guild.Id.ToString()].Start();

                    Config.Doremi._timerTradingCardSpawn[guild.Id.ToString()] = new Timer(async _ =>
                    {
                        if (Config.Doremi._stopwatchCardSpawn.ContainsKey(guild.Id.ToString()))
                            if (Config.Doremi._stopwatchCardSpawn[guild.Id.ToString()].IsRunning)
                                Config.Doremi._stopwatchCardSpawn[guild.Id.ToString()].Restart();

                        await TradingCardCore.generateCardSpawn(guildId);
                    },
                    null,
                    TimeSpan.FromMinutes(Convert.ToInt32(guildCardSpawnData[DBM_Trading_Card_Guild.Columns.spawn_interval])), //time to wait before executing the timer for the first time
                    TimeSpan.FromMinutes(Convert.ToInt32(guildCardSpawnData[DBM_Trading_Card_Guild.Columns.spawn_interval])) //time to wait before executing the timer again
                    );
                }
            


            //var channel = client.GetChannel(guild.SystemChannel.Id) as SocketTextChannel;
            //await channel.SendMessageAsync(guild.SystemChannel.Id.ToString());

            //Config.Music.storedLavaTrack[guild.Id.ToString()] = new List<LavaTrack>();
        }

        /// <summary>
        ///     Unregisters the events attached to the discord client.
        /// </summary>
        public void Dispose() => client.MessageReceived -= HandleCommandAsync;

        public async Task AnnounceJoinedUser(SocketGuildUser user) //Welcomes the new user
        {
            var guildData = Config.Guild.getGuildData(user.Guild.Id);
            
            //check welcome channel
            if (guildData[DBM_Guild.Columns.id_channel_notification_user_welcome].ToString() != "")
            {
                var channel = client.GetChannel(
                    Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_notification_user_welcome].ToString())) 
                    as SocketTextChannel;
                //channel = client.GetChannel(user.Guild.SystemChannel.Id) as SocketTextChannel;

                string welcomeTitle = guildData[DBM_Guild.Columns.welcome_title].ToString();
                string welcomeMessage = guildData[DBM_Guild.Columns.welcome_message].ToString();
                string welcomeImage = guildData[DBM_Guild.Columns.welcome_image].ToString();

                //add auto role if exists
                try
                {
                    if (guildData[DBM_Guild.Columns.id_autorole_user_join].ToString() != "")
                    {
                        var roleMaster = channel.Guild.Roles.FirstOrDefault(x => x.Id ==
                        Convert.ToUInt64(guildData[DBM_Guild.Columns.id_autorole_user_join].ToString()));
                        if (roleMaster != null) await user.AddRoleAsync(roleMaster);
                    }
                } catch(Exception e) { }
                

                if (user.Id != Bot.Hazuki.client.CurrentUser.Id ||
                 user.Id != Bot.Aiko.client.CurrentUser.Id ||
                 user.Id != Bot.Onpu.client.CurrentUser.Id ||
                 user.Id != Bot.Momoko.client.CurrentUser.Id){
                    //for initialization purpose
                    var userData = GuildUserAvatarCore.getUserData(user.Guild.Id, user.Id);
                    if (welcomeMessage == "")
                    {
                        string[] arrRandomWelcomeMessage = {
                            $"We hope you enjoy and happy with all of us, {MentionUtils.MentionUser(user.Id)}.",
                            $"We're really happy that you join our group, {MentionUtils.MentionUser(user.Id)}.",
                            $"We're expecting you to come and hopefully you're enjoying your stay on the group, {MentionUtils.MentionUser(user.Id)}.",
                            $"Hope you enjoy your stay, {MentionUtils.MentionUser(user.Id)}."
                        };

                        string[] arrRandomPictures =
                        {"https://66.media.tumblr.com/c8f9c5455355f8e522d52bacb8155ab0/tumblr_mswho8nWx11r98a5go1_400.gif"};

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
                    }
                    else
                    {
                        EmbedBuilder eb = new EmbedBuilder();
                        if (guildData[DBM_Guild.Columns.welcome_color].ToString() != "")
                        {
                            string[] welcomeColor = guildData[DBM_Guild.Columns.welcome_color].ToString().Split(",");
                            int colorR = Convert.ToInt32(welcomeColor[0]);
                            int colorG = Convert.ToInt32(welcomeColor[1]);
                            int colorB = Convert.ToInt32(welcomeColor[2]);
                            eb = eb.WithColor(colorR, colorG, colorB);
                        } else
                        {
                            eb = eb.WithColor(Config.Doremi.EmbedColor);
                        }

                        if (welcomeTitle != "")
                        {
                            welcomeTitle = welcomeTitle.Replace("$user$", user.Username);
                            welcomeTitle = welcomeTitle.Replace("$servername$", channel.Guild.Name);
                            eb = eb.WithTitle(welcomeTitle);
                        }

                        if (welcomeImage != "")
                        {
                            eb = eb.WithImageUrl(welcomeImage);
                        }

                        welcomeMessage = welcomeMessage.Replace("#", "<#");
                        welcomeMessage = welcomeMessage.Replace("$user$", MentionUtils.MentionUser(user.Id));
                        welcomeMessage = welcomeMessage.Replace("$servername$", channel.Guild.Name);

                        await channel.SendMessageAsync(embed: eb
                            .WithThumbnailUrl(user.GetAvatarUrl())
                            .WithDescription(welcomeMessage)
                            .Build());
                    }
                }

                //send dm to the joined user
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
            var guildData = Config.Guild.getGuildData(user.Guild.Id);
            
            try
            {
                string username = user.Username;
                var channel = client.GetChannel(Convert.ToUInt64(
                guildData[DBM_Guild.Columns.id_channel_user_leaving_log])) as SocketTextChannel; // Gets the channel to send the message in
                EmbedBuilder eb = new EmbedBuilder()
                    .WithColor(Config.Doremi.EmbedColor)
                    .WithTitle("User leaving")
                    .WithDescription($":wave: Good bye, {user.Username}.")
                    .WithThumbnailUrl(user.GetAvatarUrl())
                    .AddField("Username:",user.Mention)
                    .WithFooter($"User ID: {user.Id}");

                await channel.SendMessageAsync(embed: eb.Build());

            } catch(Exception e)
            {

            }
            //if (guildConfig.GetValue("user_leaving_notification").ToString() == "1")
            //{
            //    string[] arrRandomLeavingMessage = {
            //       $":sob: Oh no, {user.Mention} has leave the {channel.Guild.Name} and we are really sad.",
            //       $":sob: It's nice to have {user.Mention} on {channel.Guild.Name}. We wish that you can stay more longer on our groups."
            //    };

            //    string[] arrRandomPictures = {"https://vignette.wikia.nocookie.net/ojamajowitchling/images/d/d7/ODN-EP1-013.png",
            //    "https://vignette.wikia.nocookie.net/ojamajowitchling/images/e/e1/Linesticker39.png"};

            //    await channel.SendMessageAsync(arrRandomLeavingMessage[new Random().Next(0, arrRandomLeavingMessage.GetLength(0))],
            //        embed: new EmbedBuilder()
            //                .WithColor(Config.Doremi.EmbedColor)
            //                .WithImageUrl(arrRandomPictures[new Random().Next(0, arrRandomPictures.GetLength(0))])
            //                .Build()); //say goodbye to user
            //}
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
            SocketGuildUser guildUser = (SocketGuildUser)reaction.User;
            var channel = client.GetChannel(originChannel.Id) as SocketTextChannel;
            var guildId = channel.Guild.Id;
            if (message != null && reaction.User.IsSpecified)
            {
                //custom reaction
                if (!guildUser.IsBot)
                {
                    try
                    {
                        //non custom role emoticon
                        Dictionary<string, object> columns = new Dictionary<string, object>();
                        string query = $"SELECT * " +
                        $" FROM {DBM_Guild_Role_React.tableName} " +
                        $" WHERE {DBM_Guild_Role_React.Columns.id_guild}=@{DBM_Guild_Role_React.Columns.id_guild} AND " +
                        $" {DBM_Guild_Role_React.Columns.id_message}=@{DBM_Guild_Role_React.Columns.id_message} AND " +
                        $" { DBM_Guild_Role_React.Columns.emoji}=@{ DBM_Guild_Role_React.Columns.emoji}";
                            columns[DBM_Guild_Role_React.Columns.id_guild] = guildId.ToString();
                            columns[DBM_Guild_Role_React.Columns.id_message] = messageId.ToString();
                        if (reaction.Emote.ToString().Substring(0,1)=="<")
                        {//custom emote
                            columns[DBM_Guild_Role_React.Columns.emoji] = reaction.Emote.ToString();
                        } else
                        {
                            string hexValue = "";
                            for (var i = 0; i < reaction.Emote.ToString().Length; i += char.IsSurrogatePair(reaction.Emote.ToString(), i) ? 2 : 1)
                            {
                                var decValue = char.ConvertToUtf32(reaction.Emote.ToString(), i);
                                hexValue += "+" + decValue.ToString("X");
                            }

                            columns[DBM_Guild_Role_React.Columns.emoji] = hexValue;
                        }
                            
                        var result = new DBC().selectAll(query, columns);

                        if (result.Rows.Count>=1)
                        {
                            var roleId  = "";
                            foreach (DataRow row in result.Rows)
                            {
                                roleId = row[DBM_Guild_Role_React.Columns.id_role].ToString();
                            }
                            if (roleId != "")
                            {
                                var roleMaster = channel.Guild.Roles.FirstOrDefault(x => x.Id == Convert.ToUInt64(roleId));
                                //var roleSearch = guildUser.Roles.FirstOrDefault(x => x.Id == Convert.ToUInt64(roleId));
                                if (roleMaster != null)
                                {
                                    await guildUser.AddRoleAsync(roleMaster);

                                    //sending dm notification
                                    var dmchannel = await guildUser.GetOrCreateDMChannelAsync();
                                    await dmchannel.SendMessageAsync(embed: new EmbedBuilder()
                                                .WithColor(Config.Doremi.EmbedColor)
                                                .WithTitle("Your role has been set!")
                                                .WithDescription($":white_check_mark: You have been assigned with the new role: **{roleMaster.Name}**")
                                                .WithThumbnailUrl(TradingCardCore.Doremi.emojiOk)
                                                .WithFooter($"From: {channel.Guild.Name}")
                                                .Build());
                                }
                            }

                            //await message.RemoveReactionAsync(reaction.Emote, guildUser);

                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
                else if (reaction.Emote.Equals(new Discord.Emoji("\u2B50")))
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
            var message = await cachedMessage.GetOrDownloadAsync();
            var messageId = message.GetJumpUrl().Split('/').Last();
            SocketGuildUser guildUser = (SocketGuildUser)reaction.User;
            var channel = client.GetChannel(originChannel.Id) as SocketTextChannel;
            var guildId = channel.Guild.Id;
            if (message != null && reaction.User.IsSpecified)
            {
                //custom reaction
                if (!guildUser.IsBot)
                {
                    try
                    {
                        //non custom role emoticon
                        Dictionary<string, object> columns = new Dictionary<string, object>();
                        string query = $"SELECT * " +
                        $" FROM {DBM_Guild_Role_React.tableName} " +
                        $" WHERE {DBM_Guild_Role_React.Columns.id_guild}=@{DBM_Guild_Role_React.Columns.id_guild} AND " +
                        $" {DBM_Guild_Role_React.Columns.id_message}=@{DBM_Guild_Role_React.Columns.id_message} AND " +
                        $" {DBM_Guild_Role_React.Columns.emoji}=@{DBM_Guild_Role_React.Columns.emoji} ";
                        columns[DBM_Guild_Role_React.Columns.id_guild] = guildId.ToString();
                        columns[DBM_Guild_Role_React.Columns.id_message] = messageId.ToString();

                        if (reaction.Emote.ToString().Substring(0, 1) == "<")
                        {//custom emote
                            columns[DBM_Guild_Role_React.Columns.emoji] = reaction.Emote.ToString();
                        }
                        else
                        {
                            string hexValue = "";
                            for (var i = 0; i < reaction.Emote.ToString().Length; i += char.IsSurrogatePair(reaction.Emote.ToString(), i) ? 2 : 1)
                            {
                                var decValue = char.ConvertToUtf32(reaction.Emote.ToString(), i);
                                hexValue += "+" + decValue.ToString("X");
                            }

                            columns[DBM_Guild_Role_React.Columns.emoji] = hexValue;
                        }

                        var result = new DBC().selectAll(query, columns);

                        if (result.Rows.Count >= 1)
                        {
                            var roleId = "";
                            foreach (DataRow row in result.Rows)
                            {
                                roleId = row[DBM_Guild_Role_React.Columns.id_role].ToString();
                            }
                            //if (!Config.Doremi._imReactionRole.ContainsKey(guildId))
                            //    Config.Doremi._imReactionRole.Add(guildId, new List<IMessage>());
                            if (roleId != "")
                            {
                                var roleMaster = channel.Guild.Roles.FirstOrDefault(x => x.Id == Convert.ToUInt64(roleId));
                                if (roleMaster != null)
                                {
                                    await guildUser.RemoveRoleAsync(roleMaster);

                                    var dmchannel = await guildUser.GetOrCreateDMChannelAsync();
                                    await dmchannel.SendMessageAsync(embed: new EmbedBuilder()
                                    .WithColor(Config.Doremi.EmbedColor)
                                    .WithTitle("Your role has been removed.")
                                    .WithDescription($":x: You have been removed from the role: **{roleMaster.Name}**")
                                    .WithThumbnailUrl(TradingCardCore.Doremi.emojiOk)
                                    .WithFooter($"From: {channel.Guild.Name}")
                                    .Build());
                                }
                            }
                            

                            //await message.RemoveReactionAsync(reaction.Emote, guildUser);

                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
        }

        private async Task MessageReceived(SocketMessage message)
        {
            if (message.Author.IsBot) return;
            // check if the message is a user message as opposed to a system message (e.g. Clyde, pins, etc.)
            //if (message.Author.Id == Config.Doremi.Id) return;
            //if (message.Author.Id == Config.Hazuki.Id) return;
            //if (message.Author.Id == Config.Aiko.Id) return;
            //if (message.Author.Id == Config.Onpu.Id) return;
            //if (message.Author.Id == Config.Momoko.Id) return;
            if (!(message is SocketUserMessage userMessage)) return;
            //original:
            //if (!(message is SocketUserMessage userMessage)) return Task.CompletedTask;
            // check if the message origin is a guild message channel
            if (!(userMessage.Channel is SocketTextChannel textChannel)) return;

            int argPos = 0;

            SocketUserMessage msg = (SocketUserMessage)message;
            if (!msg.HasStringPrefix(Config.Doremi.PrefixParent[0], ref argPos) &&
                !msg.HasStringPrefix(Config.Hazuki.PrefixParent[0], ref argPos) &&
                !msg.HasStringPrefix(Config.Aiko.PrefixParent[0], ref argPos) &&
                !msg.HasStringPrefix(Config.Onpu.PrefixParent[0], ref argPos) &&
                !msg.HasStringPrefix(Config.Momoko.PrefixParent[0], ref argPos))
            {
                SocketTextChannel textchannel = (SocketTextChannel)message.Channel;
                SocketUserMessage usrmsg = (SocketUserMessage)message;

                try
                {
                    ulong guildId = textchannel.Guild.Id;
                    SocketUser user = usrmsg.Author;

                    using (StreamWriter sw = (File.Exists($"logs/{textchannel.Guild.Id}/{DateTime.Now.ToString("yyyy_MM_dd")}.txt")) ? File.AppendText($"logs/{textchannel.Guild.Id}/{DateTime.Now.ToString("yyyy_MM_dd")}.txt") :
                        File.CreateText($"logs/{textchannel.Guild.Id}/{DateTime.Now.ToString("yyyy_MM_dd")}.txt"))
                        sw.WriteLine($"[{DateTime.Now.ToString("yyyy/MM/dd HH:mm")}] {usrmsg.Author.Mention}{usrmsg.Author.Username} : {message}");

                    int expRandomChance = new Random().Next(0, 7);
                    if (expRandomChance <= 5)
                    {
                        UserDataCore.updateChatExp(user, 1);
                        await GuildUserAvatarCore.updateChatExp(client, textchannel, guildId, user, 1);
                        //if (guildData[DBM_Guild.Columns.id_channel_notification_chat_level_up].ToString() != "")
                        //{
                        //    //level up notification
                        //    ulong channelNotificationId = Convert.ToUInt64(guildData[DBM_Guild.Columns.id_channel_notification_chat_level_up]);
                        //    var systemChannel = client.GetChannel(channelNotificationId) as SocketTextChannel; // Gets the channel to send the message in
                        //    await systemChannel.SendMessageAsync($"",
                        //    embed: new EmbedBuilder()
                        //    .WithColor(Config.Doremi.EmbedColor)
                        //    .WithDescription("Has just leveled up!")
                        //    .Build());
                        //}
                    }
                }
                catch { }
            }

            //return Task.CompletedTask;
        }

        private Task MessageUpdated(Cacheable<IMessage, ulong> before,
            SocketMessage after, ISocketMessageChannel channel)
        {

            // check if the message is a user message as opposed to a system message (e.g. Clyde, pins, etc.)
            //if (!(after is SocketUserMessage userMessage)) return Task.CompletedTask;
            //// check if the message origin is a guild message channel
            //if (!(after.Channel is SocketTextChannel textChannel)) return Task.CompletedTask;

            //SocketTextChannel textchannel = (SocketTextChannel)after.Channel;
            //SocketUserMessage usrmsg = (SocketUserMessage)after;

            //// If the message was not in the cache, downloading it will result in getting a copy of `after`.
            //try
            //{
            //    var message = before.GetOrDownloadAsync();

            //    using (StreamWriter sw = (File.Exists($"logs/{textchannel.Guild.Id}/{DateTime.Now.ToString("yyyy_MM_dd")}.txt")) ? File.AppendText($"logs/{textchannel.Guild.Id}/{DateTime.Now.ToString("yyyy_MM_dd")}.txt") :
            //        File.CreateText($"logs/{textchannel.Guild.Id}/{DateTime.Now.ToString("yyyy_MM_dd")}.txt"))
            //        sw.WriteLine($"[{DateTime.Now.ToString("yyyy/MM/dd HH:mm")}] {usrmsg.Author.Mention}{usrmsg.Author.Username} : [update]{after}");
            //}
            //catch { }
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
            //await commands.AddModuleAsync(typeof(DoremiRoles), services);
            await commands.AddModuleAsync(typeof(DoremiMinigameInteractive), services);
            await commands.AddModuleAsync(typeof(DoremiWiki), services);
            await commands.AddModuleAsync(typeof(DoremiModerator), services);
            await commands.AddModuleAsync(typeof(DoremiUserAvatarInteractive), services);
            //await commands.AddModuleAsync(typeof(DoremiModeratorChannels), services);
            await commands.AddModuleAsync(typeof(DoremiMagicalStageModule), services);
            await commands.AddModuleAsync(typeof(DoremiGardenInteractive), services);
            await commands.AddModuleAsync(typeof(DoremiTradingCardInteractive), services);
            //await commands.AddModuleAsync(typeof(DoremiTradingCardEvent), services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(client, message);
            ulong guildId = context.Guild.Id;

            if (message.Author.Id == Config.Doremi.Id) return;
            //if (message.Author.IsBot) return; //prevent any bot from sending the commands

            //for copied and pasted if mentioned
            int argPos = 0;
            //if (Config.Guild.getPropertyValue(context.Guild.Id, "doremi_role_id") != "" &&
            //    message.HasStringPrefix($"<@&{Config.Guild.getPropertyValue(context.Guild.Id, "doremi_role_id")}>", ref argPos))
            //{
            //    await message.Channel.SendMessageAsync(embed: new EmbedBuilder()
            //    .WithAuthor(Config.Doremi.EmbedNameError)
            //    .WithDescription($"Sorry {context.User.Username}, it seems you're calling me with the role prefix. " +
            //                "Please use the non role prefix.")
            //    .WithColor(Config.Doremi.EmbedColor)
            //    .WithThumbnailUrl("https://vignette.wikia.nocookie.net/ojamajowitchling/images/d/d2/ODN-EP1-011.png")
            //    .Build());
            //}
            //else 

            if (Config.Core.customPrefix.ContainsKey(guildId.ToString()) && (Config.Core.customPrefix[guildId.ToString()] != ""))
            {
                //custom command
                try
                {
                    if (Config.Core.customPrefix[guildId.ToString()]!="" && 
                        message.HasStringPrefix(Config.Core.customPrefix[guildId.ToString()], ref argPos))
                    {
                        var newMessage = message.Content.Replace(Config.Core.customPrefix[guildId.ToString()], "");
                        foreach (string splitted in newMessage.Split(" "))
                        {
                            string query = $"SELECT * " +
                            $" FROM {DBM_Guild_Custom_Command.tableName} " +
                            $" WHERE {DBM_Guild_Custom_Command.Columns.id_guild}=@{DBM_Guild_Custom_Command.Columns.id_guild} AND " +
                            $" {DBM_Guild_Custom_Command.Columns.command} = @{DBM_Guild_Custom_Command.Columns.command} " +
                            $" order by rand() " +
                            $" limit 1";
                            Dictionary<string, object> columns = new Dictionary<string, object>();
                            columns[DBM_Guild_Custom_Command.Columns.id_guild] = guildId.ToString();
                            columns[DBM_Guild_Custom_Command.Columns.command] = $"{splitted.ToString()}";
                            var results = new DBC().selectAll(query, columns);
                            foreach (DataRow row in results.Rows)
                            {
                                await context.Channel.SendMessageAsync(
                                    row[DBM_Guild_Custom_Command.Columns.content].ToString());
                            }
                            return;
                        }
                    }
                }
                catch (Exception e) { }
            }
            
            if (message.HasStringPrefix(Config.Doremi.PrefixParent[0], ref argPos) ||
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