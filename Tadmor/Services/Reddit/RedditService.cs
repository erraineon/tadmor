using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Tadmor.Services.Data;

namespace Tadmor.Services.Reddit
{
    [ScopedService]
    public class RedditService
    {
        private readonly AppDbContext _context;

        public RedditService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Upvote(IGuildUser user)
        {
            var upvote = await _context.Upvotes
                .AsQueryable()
                .SingleOrDefaultAsync(u => u.GuildId == user.GuildId && u.UserId == user.Id);
            if (upvote == null)
            {
                upvote = new Upvote { GuildId = user.GuildId, UserId = user.Id };
                _context.Upvotes.Add(upvote);
            }

            upvote.UpvotesCount++;
            await _context.SaveChangesAsync();
            return upvote.UpvotesCount;
        }

        public async Task<IList<Upvote>> GetUpvotes(ulong guildId)
        {
            return await _context.Upvotes.AsQueryable().Where(u => u.GuildId == guildId).ToListAsync();
        }
    }
}