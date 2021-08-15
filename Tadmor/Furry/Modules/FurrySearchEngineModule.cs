using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using E621;
using Tadmor.Core.Bookmarks.Interfaces;
using Tadmor.Core.Commands.Models;
using Tadmor.Furry.Interfaces;

namespace Tadmor.Furry.Modules
{
    [RequireNsfw]
    [Summary("furry stuff")]
    public class FurrySearchEngineModule : ModuleBase<ICommandContext>
    {
        private readonly IE621SearchEngine _e621SearchEngine;
        private readonly IBookmarkRepository _bookmarkRepository;

        public FurrySearchEngineModule(
            IE621SearchEngine e621SearchEngine,
            IBookmarkRepository bookmarkRepository)
        {
            _e621SearchEngine = e621SearchEngine;
            _bookmarkRepository = bookmarkRepository;
        }

        [Command("e621")]
        public async Task<RuntimeResult> SearchRandomAsync([Remainder] string tags)
        {
            var post = await _e621SearchEngine.SearchRandomAsync(tags);
            var result = post?.File.Url ?? throw GetNoResultsException(tags);
            return CommandResult.FromSuccess(result);
        }

        [Command("e621 latest")]
        [Priority(1)]
        public async Task SearchLatestAsync([Remainder] string tags)
        {
            var lastSeenValue = await _bookmarkRepository.GetLastSeenValueAsync(tags);
            var previouslySearched = long.TryParse(lastSeenValue, out var lastSeenId);
            var posts = previouslySearched
                ? await GetNewPostsAsync(tags, lastSeenId)
                : await GetLatestPostsAsync(tags);
            if (posts.LastOrDefault() is { } latestPost)
                await _bookmarkRepository.UpdateLastSeenAsync(tags, latestPost.Id.ToString());

            foreach (var post in posts)
            {
                await ReplyAsync(post.File.Url);
            }
        }

        private async Task<IList<E621Post>> GetLatestPostsAsync(string tags)
        {
            var posts = (await _e621SearchEngine.SearchLatestAsync(tags, null)).Take(1).ToList();
            if (!posts.Any()) throw GetNoResultsException(tags);
            return posts;
        }

        private async Task<IList<E621Post>> GetNewPostsAsync(string tags, long lastSeenId)
        {
            var posts = (await _e621SearchEngine.SearchLatestAsync(tags, lastSeenId)).Reverse().ToList();
            return posts;
        }

        private static ModuleException GetNoResultsException(string tags)
        {
            return new($"no post matching the tags {tags} was found");
        }
    }
}
