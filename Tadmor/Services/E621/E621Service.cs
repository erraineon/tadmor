using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using E621;
using Microsoft.Extensions.Options;

namespace Tadmor.Services.E621
{
    [SingletonService]
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
            };
            var posts = await _client.Search(options);
            return posts.FirstOrDefault() ?? throw new Exception("no results");
        }

        public async Task<(List<E621Post>, long)> SearchAfter(string tags, long afterId)
        {
            var searchOptions = new E621SearchOptions
            {
                Tags = tags,
            };
            var posts = await _client.Search(searchOptions);
            var newPosts = posts
                .TakeWhile(post => post.Id > afterId)
                .Take(afterId == default ? 1 : 8)
                .ToList();
            var newLastId = posts.Max(post => post.Id);
            return (newPosts, newLastId);
        }
    }
}