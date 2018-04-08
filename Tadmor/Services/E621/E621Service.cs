using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using E621;
using Microsoft.Extensions.Options;

namespace Tadmor.Services.E621
{
    public class E621Service
    {
        private readonly E621Client _client;

        public E621Service(IOptions<E621Options> options)
        {
            _client = new E621Client(options.Value.UserAgent);
        }

        public async Task<E621Post> SearchRandom(string tags)
        {
            var options = new E621SearchOptions
            {
                Limit = 1,
                Tags = $"{tags} order:random",
                TypedTags = true
            };
            var posts = await _client.Search(options);
            return posts.FirstOrDefault();
        }

        public async Task<(List<E621Post>, long)> SearchAfter(string tags, long afterId)
        {
            var searchOptions = new E621SearchOptions
            {
                Limit = afterId == default ? 1 : 10,
                Tags = tags,
                TypedTags = true
            };
            var posts = await _client.Search(searchOptions);
            var newPosts = posts.TakeWhile(post => post.Id > afterId).Reverse().ToList();
            var newLastId = posts.Max(post => post.Id);
            return (newPosts, newLastId);
        }
    }
}