using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Tadmor.Services.Abstractions;
using Tadmor.Services.Hangfire;

namespace Tadmor.Services.E621
{
    [SingletonService]
    public class E621SearchJob : IHangfireJob<E621SearchJobOptions>
    {
        private readonly ChatService _chatService;
        private readonly E621Service _e621;

        public E621SearchJob(ChatService chatService, E621Service e621)
        {
            _chatService = chatService;
            _e621 = e621;
        }

        [UpdateArguments]
        [CancelRecurrenceUponFailure]
        public async Task Do(E621SearchJobOptions options)
        {
            var client = _chatService.GetClient(options.ContextType);
            var channel = await client.GetChannelAsync(options.ChannelId) as IMessageChannel ??
                          throw new Exception("channel gone, delete schedule");
            var (newPosts, newAfterId) = await _e621.SearchAfter(options.Tags, options.AfterId);
            if (newPosts.Any())
            {
                options.AfterId = newAfterId;
                foreach (var e621Post in newPosts)
                    await channel.SendMessageAsync($"new submission: {e621Post.FileUrl}");
            }
        }
    }
}