using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using E621;
using Microsoft.Extensions.Options;
using MoreLinq;
using Tadmor.Extensions;

namespace Tadmor.Services.E621
{
    [SingletonService]
    public class E621Service
    {
        private readonly E621Client _client;
        private readonly IDictionary<ulong, PokemonGameSession> _pokemonGameSession = new ConcurrentDictionary<ulong, PokemonGameSession>();

        public E621Service(IOptions<E621Options> options)
        {
            _client = new E621Client(options.Value.UserAgent);
        }

        public async Task<E621Post> SearchRandom(string tags)
        {
            var options = new E621SearchOptions
            {
                Tags = $"{tags} order:random",
            };
            var posts = await _client.Search(options);
            return posts.Any() ? posts.RandomSubset(1).Single() : throw new Exception("no results");
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

        public PokemonGameSession CreateSession(ulong channelId, string tags)
        {
            if (GetSession(channelId) != default)
                throw new Exception($"there is already a session for {channelId}");
            var session = new PokemonGameSession(tags);
            _pokemonGameSession[channelId] = session;
            return session;
        }

        public PokemonGameSession? GetSession(ulong channelId)
        {
            return _pokemonGameSession.TryGetValue(channelId, out var session) ? session : default;
        }
    }

    public class PokemonGameSession
    {
        public PokemonGameSession(string tags)
        {
            Tags = tags;
        }

        public IDictionary<ulong, int> GuildUserScores { get; } = new ConcurrentDictionary<ulong, int>();
        public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
        public string Tags { get; }
    }
}