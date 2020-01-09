using System;
using System.Collections.Generic;
using System.Text;

using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;

namespace OjamajoBot.Module
{
    /// <summary>
    ///     Presents some of the main features of the Lavalink4NET-Library.
    /// </summary>
    /// 

    [Name("Music")]
    [RequireContext(ContextType.Guild)]
    public sealed class MusicModule : ModuleBase<SocketCommandContext>
    {
        private readonly Lavalink4NET.IAudioService _audioService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MusicModule"/> class.
        /// </summary>
        /// <param name="audioService">the audio service</param>
        /// <exception cref="ArgumentNullException">
        ///     thrown if the specified <paramref name="audioService"/> is <see langword="null"/>.
        /// </exception>
        public MusicModule(Lavalink4NET.IAudioService audioService)
            => _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));

        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinChannel(IVoiceChannel channel = null)
        {
            // Get the audio channel
            //channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            //if (channel == null) { await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

            // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
            var audioClient = await channel.ConnectAsync();

            // get player
            //var player = _audioService.GetPlayer<LavalinkPlayer>(IGuild)
            //    ?? await _audioService.JoinAsync(channel);

            //await Play("music/music.mp3");


        }

        /// <summary>
        ///     Disconnects from the current voice channel connected to asynchronously.
        /// </summary>
        /// <returns>a task that represents the asynchronous operation</returns>
        [Command("disconnect", RunMode = RunMode.Async)]
        public async Task Disconnect()
        {
            var player = await GetPlayerAsync();

            if (player == null)
            {
                return;
            }

            // when using StopAsync(true) the player also disconnects and clears the track queue.
            // DisconnectAsync only disconnects from the channel.
            await player.StopAsync(true);
            await ReplyAsync("Disconnected.");
        }

        /// <summary>
        ///     Plays music from YouTube asynchronously.
        /// </summary>
        /// <param name="query">the search query</param>
        /// <returns>a task that represents the asynchronous operation</returns>
        [Command("play", RunMode = RunMode.Async)]
        public async Task Play([Remainder]string query)
        {
            Console.WriteLine("Init play");
            var player = await GetPlayerAsync();

            if (player == null)
            {
                return;
            }

            var track = await _audioService.GetTrackAsync(query, SearchMode.YouTube);

            if (track == null)
            {
                await ReplyAsync("😖 No results.");
                return;
            }

            var position = await player.PlayAsync(track, enqueue: true);

            if (position == 0)
            {
                await ReplyAsync("🔈 Playing: " + track.Source);
            }
            else
            {
                await ReplyAsync("🔈 Added to queue: " + track.Source);
            }
        }

        /// <summary>
        ///     Shows the track position asynchronously.
        /// </summary>
        /// <returns>a task that represents the asynchronous operation</returns>
        [Command("position", RunMode = RunMode.Async)]
        public async Task Position()
        {
            var player = await GetPlayerAsync();

            if (player == null)
            {
                return;
            }

            if (player.CurrentTrack == null)
            {
                await ReplyAsync("Nothing playing!");
                return;
            }

            await ReplyAsync($"Position: {player.TrackPosition} / {player.CurrentTrack.Duration}.");
        }

        /// <summary>
        ///     Stops the current track asynchronously.
        /// </summary>
        /// <returns>a task that represents the asynchronous operation</returns>
        [Command("stop", RunMode = RunMode.Async)]
        public async Task Stop()
        {
            var player = await GetPlayerAsync();

            if (player == null)
            {
                return;
            }

            if (player.CurrentTrack == null)
            {
                await ReplyAsync("Nothing playing!");
                return;
            }

            await player.StopAsync();
            await ReplyAsync("Stopped playing.");
        }

        /// <summary>
        ///     Updates the player volume asynchronously.
        /// </summary>
        /// <param name="volume">the volume (1 - 1000)</param>
        /// <returns>a task that represents the asynchronous operation</returns>
        [Command("volume", RunMode = RunMode.Async)]
        public async Task Volume(int volume = 100)
        {
            if (volume > 1000 || volume < 0)
            {
                await ReplyAsync("Volume out of range: 0% - 1000%!");
                return;
            }

            var player = await GetPlayerAsync();

            if (player == null)
            {
                return;
            }

            await player.SetVolumeAsync(volume / 100f);
            await ReplyAsync($"Volume updated: {volume}%");
        }

        /// <summary>
        ///     Gets the guild player asynchronously.
        /// </summary>
        /// <param name="connectToVoiceChannel">
        ///     a value indicating whether to connect to a voice channel
        /// </param>
        /// <returns>
        ///     a task that represents the asynchronous operation. The task result is the lavalink player.
        /// </returns>
        private async Task<VoteLavalinkPlayer> GetPlayerAsync(bool connectToVoiceChannel = true)
        {
            var player = _audioService.GetPlayer<VoteLavalinkPlayer>(Context.Guild);
            if (player != null
                && player.State != PlayerState.NotConnected
                && player.State != PlayerState.Destroyed)
            {
                Console.WriteLine("No player");
                return player;
            }

            var user = Context.Guild.GetUser(Context.User.Id);

            if (!user.VoiceState.HasValue)
            {
                await ReplyAsync("You must be in a voice channel!");
                return null;
            }

            if (!connectToVoiceChannel)
            {
                await ReplyAsync("The bot is not in a voice channel!");
                return null;
            }

            return await _audioService.JoinAsync<VoteLavalinkPlayer>(user.VoiceChannel);
        }
    }
}
