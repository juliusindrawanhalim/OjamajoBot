using Discord.Audio;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace OjamajoBot.Service
{
    public class VictoriaService
    {
        private readonly LavaNode _lavaNode;

        public VictoriaService(LavaNode lavaNode, DiscordSocketClient socketClient)
        {
            //socketClient.Ready += OnReady;
            _lavaNode = lavaNode;
            _lavaNode.OnPlayerUpdated += OnPlayerUpdated;
            _lavaNode.OnStatsReceived += OnStatsReceived;
            _lavaNode.OnTrackEnded += OnTrackEnded;
            _lavaNode.OnTrackException += OnTrackException;
            _lavaNode.OnTrackStuck += OnTrackStuck;
            _lavaNode.OnWebSocketClosed += OnWebSocketClosed;
        }

        private async Task OnReady()
        {
            await _lavaNode.ConnectAsync();
        }

        private Task OnPlayerUpdated(PlayerUpdateEventArgs arg)
        {
            //Console.WriteLine(arg.Track.Title);
            //Console.WriteLine($"Player update received for {arg.Player.VoiceChannel.Name}.");
            return Task.CompletedTask;
        }

        private Task OnStatsReceived(StatsEventArgs arg)
        {
            Console.WriteLine($"Lavalink Uptime {arg.Uptime}.");
            return Task.CompletedTask;
        }

        private Task OnTrackException(TrackExceptionEventArgs arg)
        {
            Console.WriteLine($"Track exception received for {arg.Track.Title}.");
            return Task.CompletedTask;
        }

        private Task OnTrackStuck(TrackStuckEventArgs arg)
        {
            Console.WriteLine($"Track stuck received for {arg.Track.Title}.");
            return Task.CompletedTask;
        }

        private Task OnWebSocketClosed(WebSocketClosedEventArgs arg)
        {
            Console.WriteLine($"Discord WebSocket connection closed with following reason: {arg.Reason}");
            return Task.CompletedTask;
        }

        /// Handling track end event for auto play. (original)
        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {

            if (!args.Reason.ShouldPlayNext())
                return;

            var player = args.Player;

            if (!player.Queue.TryDequeue(out var queueable))
            {

                await player.TextChannel.SendMessageAsync(":stop_button: No more tracks to play.");
                return;
            }

            if (!(queueable is LavaTrack track))
            {
                await player.TextChannel.SendMessageAsync(":x: Next item in queue is not a track.");
                return;
            }

            //https://i.ytimg.com/vi/tPEE9ZwTmy0/hqdefault.jpg
            //https://i.ytimg.com/vi/tPEE9ZwTmy0/hqdefault.jpg
            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync(
                $"{args.Reason}: **{args.Track.Title}**.",
                embed: new EmbedBuilder()
                .WithAuthor("Now Playing")
                .WithTitle(track.Title)
                .WithColor(Config.Onpu.EmbedColor)
                .WithUrl(track.Url)
                .AddField("Duration", track.Duration, true)
                .AddField("Author", track.Author, true)
                .WithThumbnailUrl($"https://i.ytimg.com/vi/{track.Id}/hqdefault.jpg")
                .WithFooter("Onpu Musicbox", Config.Onpu.EmbedAvatarUrl)
                .Build());
        }

        //backup
        //private async Task OnTrackEnded(TrackEndedEventArgs args)
        //{

        //    if (!args.Reason.ShouldPlayNext())
        //        return;

        //    var player = args.Player;
        //    player.Queue.Enqueue(args.Track);

        //    if (!player.Queue.TryDequeue(out var queueable))
        //    {
        //        player.Queue.Enqueue(args.Track);
        //        await player.TextChannel.SendMessageAsync("No more tracks to play.");
        //        return;
        //    }

        //    if (!(queueable is LavaTrack track))
        //    {
        //        await player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
        //        return;
        //    }

        //    await args.Player.PlayAsync(track);
        //    await args.Player.TextChannel.SendMessageAsync(
        //        $"{args.Reason}: {args.Track.Title}\n" +
        //        $"\u25B6 Now playing: {track.Title}");
        //}
        //end backup

        //private async Task OnTrackEnded(TrackEndedEventArgs args)
        //{
        //    if (!args.Reason.ShouldPlayNext())
        //        return;

        //    LavaPlayer player = args.Player;

        //    LavaTrack repeatedtrack = args.Track;
        //    LavaTrack track = repeatedtrack;

        //    if (Config.Music.repeat == 2 || Config.Music.repeat == 0) //repeat
        //    {
        //        Config.Music.storedLavaTrack.RemoveAt(0);
        //        if (!player.Queue.TryDequeue(out var queueable))
        //        {
        //            Config.Music.storedLavaTrack.Clear();
        //            await args.Player.TextChannel.SendMessageAsync("I've no more music to play from the queue list.");
        //        }

        //        if (Config.Music.repeat == 2)
        //        {
        //            player.Queue.Enqueue(args.Track);
        //        }

        //        track = (LavaTrack)queueable;
        //    } else if(Config.Music.repeat == 1) {
        //        track = repeatedtrack;
        //    }

        //    await args.Player.PlayAsync(track);
        //    await player.TextChannel.SendMessageAsync(
        //            $"{args.Reason}: {args.Track.Title}\n" +
        //            $"Now playing: {track.Title}");
        //}

    }
}
