using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Caching.Distributed;
using Tadmor.Core.Bookmarks.Interfaces;
using Tadmor.Core.Bookmarks.Models;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;
using Tadmor.Core.Data.Interfaces;

namespace Tadmor.Core.Bookmarks.Services
{
    public class BookmarkRepository : IBookmarkRepository
    {
        private readonly ITadmorDbContext _tadmorDbContext;
        private readonly ICommandContext _commandContext;

        public BookmarkRepository(
            ITadmorDbContext tadmorDbContext,
            ICommandContext commandContext)
        {
            _tadmorDbContext = tadmorDbContext;
            _commandContext = commandContext;
        }

        public async Task UpdateLastSeenAsync(string key, string lastSeenId, CancellationToken cancellationToken = default)
        {
            var bookmark = new Bookmark
            {
                ChatClientId = ((IChatClient) _commandContext.Client).Name,
                GuildId = _commandContext.Guild.Id,
                ChannelId = _commandContext.Channel.Id,
                Key = key,
                LastSeenValue = lastSeenId
            };
            _tadmorDbContext.Update(bookmark);
            await _tadmorDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<string?> GetLastSeenValueAsync(string key, CancellationToken cancellationToken = default)
        {
            var keyValues = new object[]
            {
                ((IChatClient) _commandContext.Client).Name,
                _commandContext.Guild.Id,
                _commandContext.Channel.Id,
                key
            };
            var bookmark = await _tadmorDbContext.Bookmarks.FindAsync(keyValues, cancellationToken);
            var lastSeenValue = bookmark?.LastSeenValue;
            return lastSeenValue;
        }
    }
}
