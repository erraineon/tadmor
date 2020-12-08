using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using FChan.Library;

namespace Tadmor.Services.FourChan
{
    [SingletonService]
    public class ChanService
    {
        public async Task<IEnumerable<ChanPost>> GetHotPosts(string boardName, int maxPosts)
        {
            var hotPosts = new List<ChanPost>();
            var boards = (await Chan.GetBoardAsync()).Boards;
            var board = boards.SingleOrDefault(b => b.BoardName == boardName) ?? throw new Exception("unknown board");
            var ops = Enumerable.Range(1, board.Pages)
                .ToAsyncEnumerable()
                .SelectAwait(page => new ValueTask<ThreadRootObject>(Chan.GetThreadPageAsync(boardName, page)))
                .Select(tro => tro.Threads)
                .Flatten()
                .Select(thread => thread.Posts.First(p => p.Replies != null))
                .OrderByDescending(op => op.Replies);

            await foreach (var op in ops)
            {
                var lowestReplyCount = hotPosts.Select(post => post.Replies).DefaultIfEmpty().Min();
                // don't care about threads that don't even have as many replies as the smallest "hot post"
                if (op.Replies > lowestReplyCount)
                {
                    var posts = (await Chan.GetThreadAsync(boardName, op.PostNumber)).Posts;
                    hotPosts = posts
                        .SelectMany(post => Regex.Matches(post.Comment ?? string.Empty, @"(?<=#p)\d+")
                            .Select(match => (post, repliedToPost: int.Parse(match.Value))))
                        .GroupBy(t => t.repliedToPost, t => t.post)
                        .Where(t => t.Key != op.PostNumber)
                        .Select(g => (post: posts.SingleOrDefault(p => p.PostNumber == g.Key), replies: g.Count()))
                        .Where(t => t.post != null)
                        .Select(t => new ChanPost(t.post, t.replies, op.PostNumber, op.Subject))
                        .Concat(hotPosts)
                        .OrderByDescending(p => p.Replies)
                        .Take(maxPosts)
                        .ToList();
                }
            }

            return hotPosts.OrderByDescending(post => post.Replies).Take(maxPosts);
        }
    }
}