using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Tadmor.Services.Abstractions;
using Tadmor.Services.Data;

namespace Tadmor.Services.Marriage
{
    [SingletonService]
    public class MarriageService
    {
        private readonly ChatService _chatService;
        private readonly List<ulong> _channelsWithMarriages = new List<ulong>();

        public MarriageService(ChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task Marry(IUser partner1, IGuildUser partner2, IMessageChannel channel, AppDbContext dbContext)
        {
            if (_channelsWithMarriages.Contains(channel.Id)) throw new Exception("someone's already getting married");
            if (partner1.Id == partner2.Id) throw new Exception("you cant marry yourself");
            var guildId = partner2.GuildId;
            await AssertNotMarried(partner1, guildId, dbContext);
            await AssertNotMarried(partner2, guildId, dbContext);
            try
            {
                _channelsWithMarriages.Add(channel.Id);
                var partner1Name = partner1.Username;
                var partner2Name = partner2.Username;
                var invocationMsg = $"we are here today to celebrate with {partner1Name} and {partner2Name} " +
                                    "as they proclaim their love and commitment etc etc etc. if youre both " +
                                    "committed to it, please reply: ok";
                await channel.SendMessageAsync(invocationMsg);
                await EnsureBothSay(partner1, partner2, "ok");
                await Task.Delay(TimeSpan.FromSeconds(1));
                var pronouncementMsg3 = "nice job youre married";
                await channel.SendMessageAsync(pronouncementMsg3);
                await Task.Delay(TimeSpan.FromSeconds(1));
                await dbContext.MarriedCouples.AddAsync(new MarriedCouple
                {
                    Partner1Id = partner1.Id,
                    Partner2Id = partner2.Id,
                    TimeStamp = DateTime.Now,
                    GuildId = guildId
                });
                await dbContext.SaveChangesAsync();
            }
            finally
            {
                _channelsWithMarriages.Remove(channel.Id);
            }
        }


        private async Task AssertNotMarried(IUser partner, ulong guildId, AppDbContext dbContext)
        {
            var existingMarriage = await dbContext.MarriedCouples
                .AsQueryable()
                .FirstOrDefaultAsync(c => c.GuildId == guildId &&
                                          (c.Partner1Id == partner.Id || c.Partner2Id == partner.Id));
            if (existingMarriage != null) throw new AlreadyMarriedException(existingMarriage);
        }

        private async Task EnsureBothSay(IUser partner1, IUser partner2, string expected)
        {
            async Task EnsureSays(ulong partnerId, CancellationTokenSource cts)
            {
                var response = await _chatService.Next(m => m.Author.Id == partnerId, cts.Token);
                var trimmedContent = response.Content?.Trim(' ', '\'', '"');
                if (!string.Equals(trimmedContent, expected, StringComparison.OrdinalIgnoreCase))
                {
                    cts.Cancel();
                    throw new TaskCanceledException();
                }
            }

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
                await Task.WhenAll(EnsureSays(partner1.Id, cts), EnsureSays(partner2.Id, cts));
            }
            catch (TaskCanceledException)
            {
                throw new Exception("ok nvm then");
            }
        }

        public async Task Divorce(IUser partner1, IUser partner2, AppDbContext dbContext)
        {
            var marriage = await GetMarriage(partner1, partner2, dbContext);
            if (marriage == null) throw new Exception($"{partner1.Username} is not married to {partner2.Username}");
            dbContext.MarriedCouples.Remove(marriage);
            await dbContext.SaveChangesAsync();
        }

        public async Task<int> Kiss(IUser partner1, IUser partner2, AppDbContext dbContext)
        {
            var marriage = await GetMarriage(partner1, partner2, dbContext);
            if (marriage == null) throw new Exception($"you can only kiss your partner");
            var now = DateTime.Now;
            if (now - marriage.LastKissed < TimeSpan.FromHours(1)) throw new Exception("you can only kiss once an hour");
            marriage.Kisses++;
            marriage.LastKissed = now;
            await dbContext.SaveChangesAsync();
            return marriage.Kisses;
        }

        private async Task<MarriedCouple> GetMarriage(IUser partner1, IUser partner2, AppDbContext dbContext)
        {
            var marriage = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(dbContext.MarriedCouples, c => c.Partner1Id == partner1.Id && c.Partner2Id == partner2.Id ||
                                          c.Partner2Id == partner1.Id && c.Partner1Id == partner2.Id);
            return marriage;
        }

        public async Task<IList<MarriedCouple>> GetMarriages(IGuild guild, AppDbContext dbContext)
        {
            var marriedCouples = await dbContext.MarriedCouples
                .AsQueryable()
                .Where(c => c.GuildId == guild.Id)
                .ToListAsync();
            return marriedCouples;
        }
    }
}
