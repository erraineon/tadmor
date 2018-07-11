using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FChan.Library;
using Newtonsoft.Json.Linq;

namespace Tadmor.Services.Chan
{
    public class ChanService
    {
        private readonly HttpClient _http = new HttpClient();
        private List<Board> _boards;

        public async Task<IList<Post>> GetHotPosts(string boardName, int maxPosts)
        {
            await AssertBoardExists(boardName);
            var postReplies = new List<Post>();
            var catalog = await _http.GetStringAsync($"http://a.4cdn.org/{boardName}/catalog.json");
            var catalogJson = JArray.Parse(catalog);
            var threads = catalogJson
                .SelectMany(page => page["threads"], (page, thread) => (thread, replies: (int) thread["replies"]))
                .OrderByDescending(t => t.replies)
                .ToList();
            while (threads.Any())
            {
                var tuple = threads.First();
                var threadNumber = (int) tuple.thread["no"];
                var posts = (await FChan.Library.Chan.GetThreadAsync(boardName, threadNumber))?.Posts;
                if (posts != null)
                {
                    var mostRepliedToPosts = posts
                        .SelectMany(GetReply)
                        .GroupBy(t => t.Item2, t => t.Item1)
                        .Where(t => t.Key != threadNumber)
                        .OrderByDescending(g => g.Count())
                        .Select(g => (post: posts.SingleOrDefault(p => p.PostNumber == g.Key), replies: g.Count()))
                        .Where(t => t.post != null)
                        .Take(maxPosts);
                    foreach (var (post, replies) in mostRepliedToPosts)
                    {
                        post.Replies = replies;
                        var toDiscard = postReplies.FirstOrDefault(p => p.Replies < post.Replies);
                        if (toDiscard == null && postReplies.Count >= maxPosts) break;
                        postReplies.Remove(toDiscard);
                        var url = $"https://boards.4chan.org/{boardName}/thread/{threadNumber}";
                        if (post.PostNumber != threadNumber) url = $"{url}#p{post.PostNumber}";
                        post.ThreadUrlSlug = url;
                        post.Name = tuple.thread["sub"] is JToken sub ? (string)sub : default;
                        postReplies.Add(post);
                    }

                    if (postReplies.Count >= maxPosts)
                    {
                        var minReplies = postReplies.Min(post => post.Replies);
                        threads.RemoveAll(t => t.replies <= minReplies);
                    }
                }

                threads.Remove(tuple);
            }

            return postReplies;
        }

        private async Task AssertBoardExists(string boardName)
        {
            var boards = _boards ?? (_boards = (await FChan.Library.Chan.GetBoardAsync()).Boards);
            if (boards.All(board => board.BoardName != boardName))
                throw new Exception("unknown board");
        }

        private static IEnumerable<(Post, int)> GetReply(Post arg)
        {
            if (arg.Comment == null) return Enumerable.Empty<(Post, int)>();
            return Regex.Matches(arg.Comment, @"(?<=#p)\d+")
                .Select(match => (arg, int.Parse(match.Value)));
        }
    }
}