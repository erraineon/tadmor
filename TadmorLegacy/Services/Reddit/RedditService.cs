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

        public async Task<int> Vote(IMessage message, IGuildUser targetUser, IUser voter, VoteType voteType)
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
                VoterId = voter.Id,
                VoteType = voteType
            };
            await _context.Upvotes.AddAsync(upvote);
            await _context.SaveChangesAsync();

            var userScore = (await GetUpvotes(targetUser.GuildId, targetUser.Id))
                .Sum(u => u.VoteType == VoteType.Downvote ? -1 : 1);
            return userScore;
        }

        public async Task<Dictionary<ulong, (int upvoteCount, int downvoteCount)>> GetUpvoteCounts(ulong guildId)
        {
            return (await _context.Upvotes
                    .AsQueryable()
                    .Where(u => u.GuildId == guildId)
                    .ToListAsync())
                .GroupBy(u => u.TargetUserId)
                .Select(g => new
                {
                    id = g.Key,
                    upvoteCount = g.Count(u => u.VoteType == VoteType.Upvote),
                    downvoteCount = g.Count(u => u.VoteType == VoteType.Downvote)
                })
                .ToDictionary(o => o.id, g => (g.upvoteCount, g.downvoteCount));
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