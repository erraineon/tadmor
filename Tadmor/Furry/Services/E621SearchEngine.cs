using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using E621;
using Tadmor.Core.Extensions;
using Tadmor.Furry.Interfaces;

namespace Tadmor.Furry.Services
{
    public class E621SearchEngine : IE621SearchEngine
    {
        private readonly IE621Client _e621Client;

        public E621SearchEngine(IE621Client e621Client)
        {
            _e621Client = e621Client;
        }

        public async Task<E621Post?> SearchRandomAsync(string tags)
        {
            var options = new E621SearchOptions
            {
                Tags = $"{tags} order:random"
            };
            var posts = await _e621Client.Search(options);
            return posts.RandomOrDefault();
        }

        public async Task<IList<E621Post>> SearchLatestAsync(string tags, long? afterId)
        {
            var options = new E621SearchOptions
            {
                Tags = tags,
                AfterId = afterId
            };
            var posts = await _e621Client.Search(options);
            return posts;
        }
    }
}