using System;
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

        public async Task<int> Upvote(IMessage message, IGuildUser targetUser, IUser voter)
        {
            var upvote = await _context.Upvotes
                .AsQueryable()
                .SingleOrDefaultAsync(u => u.GuildId == targetUser.GuildId && 
                                           u.TargetUserId == targetUser.Id && 
                                           u.MessageId == message.Id &&
                                           u.VoterId == voter.Id);
            if (upvote != null) throw new Exception($"you already upvoted {targetUser.Nickname}'s message");
            upvote = new Upvote
            {
                GuildId = targetUser.GuildId,
                TargetUserId = targetUser.Id,
                MessageId = message.Id,
                VoterId = voter.Id
            };
            _context.Upvotes.Add(upvote);

            await _context.SaveChangesAsync();

            var userUpvotes = (await GetUpvotes(targetUser.GuildId, targetUser.Id)).Count;
            return userUpvotes;
        }

        public async Task<Dictionary<ulong, int>> GetUpvoteCounts(ulong guildId)
        {
            return await _context.Upvotes
                .AsQueryable()
                .Where(u => u.GuildId == guildId)
                .GroupBy(u => u.TargetUserId)
                .Select(g => new {id = g.Key, count = g.Count()})
                .ToDictionaryAsync(o => o.id, g => g.count);
        }

        public async Task<IList<Upvote>> GetUpvotes(ulong guildId, ulong userId)
        {
            return await _context.Upvotes
                .AsQueryable()
                .Where(u =>  u.GuildId == guildId && u.TargetUserId == userId)
                .ToListAsync();
        }
    }
}