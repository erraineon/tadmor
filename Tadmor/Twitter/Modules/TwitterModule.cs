using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Commands.Models;
using Tadmor.Twitter.Interfaces;

namespace Tadmor.Twitter.Modules
{
    [Summary("twitter")]
    public class TwitterModule : ModuleBase<ICommandContext>
    {
        private readonly ITwitterService _twitterService;

        public TwitterModule(ITwitterService twitterService)
        {
            _twitterService = twitterService;
        }

        [Command("twitter")]
        public async Task<RuntimeResult> GetRandomTweetAsync(string displayName, [Remainder] string? filter = default)
        {
            return await GetRandomTweetAsync(displayName, filter, false);
        }

        [Command("twitter media")]
        [Priority(1)]
        public async Task<RuntimeResult> GetRandomMediaTweetAsync(string displayName, [Remainder] string? filter = default)
        {
            return await GetRandomTweetAsync(displayName, filter, true);
        }

        private async Task<RuntimeResult> GetRandomTweetAsync(string displayName, string? filter, bool onlyMedia)
        {
            var tweet = await _twitterService.GetRandomTweetAsync(displayName, onlyMedia, filter) ??
                        throw new ModuleException("no tweets that matched the filter were found");
            return CommandResult.FromSuccess($"https://twitter.com/{tweet.AuthorName}/status/{tweet.Id}");
        }
    }
}