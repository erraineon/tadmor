using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
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

            var alreadyExists = await _tadmorDbContext.Bookmarks
                .AsNoTracking()
                .AnyAsync(b =>
                    b.ChatClientId == bookmark.ChatClientId &&
                    b.GuildId == bookmark.GuildId &&
                    b.ChannelId == bookmark.ChannelId &&
                    b.Key == bookmark.Key, cancellationToken);

            if (alreadyExists) _tadmorDbContext.Update(bookmark);
            else await _tadmorDbContext.Bookmarks.AddAsync(bookmark, cancellationToken);

            await _tadmorDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<string?> GetLastSeenValueAsync(string key, CancellationToken cancellationToken = default)
        {
            var lastSeenValue = await _tadmorDbContext.Bookmarks
                .AsQueryable()
                .Where(b =>
                    b.ChatClientId == ((IChatClient)_commandContext.Client).Name &&
                    b.GuildId == _commandContext.Guild.Id &&
                    b.ChannelId == _commandContext.Channel.Id &&
                    b.Key == key)
                .Select(b => b.LastSeenValue)
                .SingleOrDefaultAsync(cancellationToken);

            return lastSeenValue;
        }
    }
}
