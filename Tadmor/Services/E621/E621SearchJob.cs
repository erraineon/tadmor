using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Tadmor.Extensions;
using Tadmor.Services.Hangfire;

namespace Tadmor.Services.E621
{
    public class E621SearchJob : IHangfireJob<E621SearchJobOptions>
    {
        private readonly DiscordSocketClient _discord;
        private readonly E621Service _e621;

        public E621SearchJob(DiscordSocketClient discord, E621Service e621)
        {
            _discord = discord;
            _e621 = e621;
        }

        [UpdateArguments]
        [CancelRecurrenceUponFailure]
        public async Task Do(E621SearchJobOptions options)
        {
            var destChannel = _discord.GetChannel(options.ChannelId) as IMessageChannel ??
                              throw new Exception("channel gone, delete schedule");
            var (newPosts, newAfterId) = await _e621.SearchAfter(options.Tags, options.AfterId);
            if (newPosts.Any())
            {
                options.AfterId = newAfterId;
                foreach (var e621Post in newPosts)
                    await destChannel.SendMessageAsync("new submission", embed: e621Post.ToEmbed());
            }
        }
    }
}