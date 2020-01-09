using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Discord;
using Lavalink4NET.Events;

namespace OjamajoBot.Module
{
    public sealed class LavalinkModule : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;
        public IVoiceChannel joinedchannel;

        public LavalinkModule(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
        }

        [Command("Join", RunMode = RunMode.Async)]
        public async Task JoinAsync(IVoiceChannel channel = null)
        {
            // Get the audio channel
            joinedchannel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null) { await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

            // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
            await _lavaNode.JoinAsync(joinedchannel);
            await ReplyAsync($"Joined {joinedchannel} channel!");
        }

        [Command("Leave", RunMode = RunMode.Async)]
        public async Task LeaveAsyncIVoiceChannel()
        {
            //await _lavaNode.LeaveAsync(player);
            //await ReplyAsync($"Left {joinedchannel} channel!");
        }

        [Command("Move", RunMode = RunMode.Async)]
        public async Task MoveAsync(IVoiceChannel channel = null)
        {
            // Get the audio channel
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null) { await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

            await _lavaNode.MoveAsync(channel);
            var player = _lavaNode.GetPlayer(Context.Guild);
            await ReplyAsync($"Moved from {player.VoiceChannel} to {channel}!");
        }

        [Command("Play",RunMode = RunMode.Async)]
        public async Task PlayAsync([Remainder] string query)
        {
            var search = await _lavaNode.SearchYouTubeAsync(query);
            var track = search.Tracks.FirstOrDefault();

            var player = _lavaNode.HasPlayer(Context.Guild)
                ? _lavaNode.GetPlayer(Context.Guild)
                : await _lavaNode.JoinAsync(joinedchannel);

            if (player.PlayerState == PlayerState.Playing)
            {
                player.Queue.Enqueue(track);
                await ReplyAsync($"Enqeued {track.Title}.");
            }
            else
            {
                await player.PlayAsync(track);
                await ReplyAsync($"Playing {track.Title}.");
            }
        }

    }
}
