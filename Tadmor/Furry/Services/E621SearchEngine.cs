using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using E621;
using Microsoft.Extensions.Caching.Distributed;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;
using Tadmor.Core.Data.Interfaces;
using Tadmor.Core.Extensions;

namespace Tadmor.Furry.Services
{
    
    public interface IE621Client
    {
        Task<IList<E621Post>> Search(E621SearchOptions options);
    }

    public class E621ClientWrapper : E621Client, IE621Client
    {
        public E621ClientWrapper() : base("tadmor/errai")
        {
        }
    }

    public interface IE621SearchEngine
    {
        Task<E621Post?> SearchRandomAsync(string tags);
        Task<IList<E621Post>> SearchLatestAsync(string tags, long? afterId);
    }

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
            return posts.Any() ? posts.Random() : null;
        }

        public async Task<IList<E621Post>> SearchLatestAsync(string tags, long? afterId)
        {
            var options = new E621SearchOptions
            {
                Tags = tags,
                AfterId = afterId,
            };
            var posts = await _e621Client.Search(options);
            return posts;
        }
    }
}
