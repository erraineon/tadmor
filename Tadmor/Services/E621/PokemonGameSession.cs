using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Tadmor.Services.E621
{
    public class PokemonGameSession
    {
        public PokemonGameSession(ulong gameStartingUserId, string tags)
        {
            GameStartingUserId = gameStartingUserId;
            Tags = tags;
        }

        public ulong GameStartingUserId { get; }
        public IDictionary<ulong, int> GuildUserScores { get; } = new ConcurrentDictionary<ulong, int>();
        public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
        public string Tags { get; }
    }
}