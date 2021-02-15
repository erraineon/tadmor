using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using E621;
using Microsoft.AspNetCore.WebUtilities;
using Tadmor.Core.Bookmarks.Interfaces;
using Tadmor.Core.Commands.Models;
using Tadmor.Furry.Services;

namespace Tadmor.Furry.Modules
{
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

        public async Task<RuntimeResult> SearchRandomAsync(string tags)
        {
            var post = await _e621SearchEngine.SearchRandomAsync(tags);
            var result = post?.File.Url ?? throw GetNoResultsException(tags);
            return CommandResult.FromSuccess(result);
        }

        public async Task SearchLatestAsync(string tags)
        {
            var lastSeenValue = await _bookmarkRepository.GetLastSeenValueAsync(tags);
            var previouslySearched = long.TryParse(lastSeenValue, out var lastSeenId);
            var newPosts = previouslySearched
                ? await GetNewPostsAsync(tags, lastSeenId)
                : await GetLatestPostsAsync(tags);

            foreach (var post in newPosts)
            {
                await ReplyAsync(post.File.Url);
            }
        }

        private async Task<List<E621Post>> GetLatestPostsAsync(string tags)
        {
            var posts = (await _e621SearchEngine.SearchLatestAsync(tags, null)).Take(1).ToList();
            if (posts.SingleOrDefault() is { } latestPost)
                await _bookmarkRepository.UpdateLastSeenAsync(tags, latestPost.Id.ToString());
            else throw GetNoResultsException(tags);
            return posts;
        }

        private async Task<IEnumerable<E621Post>> GetNewPostsAsync(string tags, long lastSeenId)
        {
            return (await _e621SearchEngine.SearchLatestAsync(tags, lastSeenId)).Reverse();
        }

        private static ModuleException GetNoResultsException(string tags)
        {
            return new($"no post matching the tags {tags} was found");
        }
    }
}
