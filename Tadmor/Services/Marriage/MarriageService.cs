using System;
using System.Collections.Generic;
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

        public async Task Marry(IUser partner1, IUser partner2, IMessageChannel channel, AppDbContext dbContext)
        {
            if (_channelsWithMarriages.Contains(channel.Id)) throw new Exception("someone's already getting married");
            if (partner1.Id == partner2.Id) throw new Exception("you cant marry yourself");
            await AssertNotMarried(partner1, dbContext);
            await AssertNotMarried(partner2, dbContext);
            try
            {
                _channelsWithMarriages.Add(channel.Id);
                var partner1Name = partner1.Username;
                var partner2Name = partner2.Username;
                var invocationMsg = $"we are here today to celebrate with {partner1Name} and {partner2Name} " +
                                    "as they proclaim their love and commitment etc to the world. we here to rejoice, with " +
                                    "and for them, in the new life they are about to take on together";
                await channel.SendMessageAsync(invocationMsg);
                await Task.Delay(TimeSpan.FromSeconds(10));
                var intentMsg = $"{partner1Name} and {partner2Name}, the relationship you enter into today has to be " +
                                "grounded in the strength of your love and the power of your faith in each other. " +
                                "to make your relationship succeed it will take a lot of love. it will take trust, " +
                                "to know in your hearts blah blah blah yadda yadda yadda";
                await channel.SendMessageAsync(intentMsg);
                await Task.Delay(TimeSpan.FromSeconds(10));
                var intentMsg2 = "if you both understand the effort involved to make your relationship thrive, " +
                                 "and are committed to it, please reply: ok";
                await channel.SendMessageAsync(intentMsg2);
                await EnsureBothSay(partner1, partner2, "ok");
                await Task.Delay(TimeSpan.FromSeconds(1));
                var ringMsgExpectedReply = "with this ring, i promise to stand with you as we share this life, " +
                                           "and cherish the memories we make together";
                var ringMsg = "now place the ring on each others hands and repeat after me: " + ringMsgExpectedReply;
                await channel.SendMessageAsync(ringMsg);
                await EnsureBothSay(partner1, partner2, ringMsgExpectedReply);
                await Task.Delay(TimeSpan.FromSeconds(1));
                var pronouncementMsg1 = $"{partner1Name} and {partner2Name}, prior to this moment you each walked a " +
                                        "separate path. now, you embark together on a shared path. it is the strength " +
                                        "of your love that shall nourish you all together as a family";
                await channel.SendMessageAsync(pronouncementMsg1);
                await Task.Delay(TimeSpan.FromSeconds(10));
                var pronouncementMsg2 = $"{partner1Name} and {partner2Name}, today you have stood before these " +
                                        "witnesses and declared your intent to commit your lives to each other " +
                                        "in marriage. you have made promises to each other. i hope you will never " +
                                        "forget the fight and perseverance it has taken to get to this moment. and " +
                                        "I hope you will never forget the love and joy you feel today, because these " +
                                        "are the values that will keep your marriage and bond to one another strong";
                await channel.SendMessageAsync(pronouncementMsg2);
                await Task.Delay(TimeSpan.FromSeconds(12));
                var pronouncementMsg3 = "and so, by the power vested in me by the Ministry of Smoothies, " +
                                        "i now pronounce you husband and wife";
                await channel.SendMessageAsync(pronouncementMsg3);
                await Task.Delay(TimeSpan.FromSeconds(5));
                await dbContext.MarriedCouples.AddAsync(new MarriedCouple
                {
                    Partner1Id = partner1.Id,
                    Partner2Id = partner2.Id,
                    TimeStamp = DateTime.Now
                });
                await dbContext.SaveChangesAsync();
            }
            finally
            {
                _channelsWithMarriages.Remove(channel.Id);
            }
        }


        private async Task AssertNotMarried(IUser partner, AppDbContext dbContext)
        {
            var existingMarriage = await dbContext.MarriedCouples
                .FirstOrDefaultAsync(c => c.Partner1Id == partner.Id || c.Partner2Id == partner.Id);
            if (existingMarriage != null) throw new AlreadyMarriedException(existingMarriage);
        }

        private async Task EnsureBothSay(IUser partner1, IUser partner2, string expected)
        {
            async Task EnsureSays(ulong partnerId, CancellationToken token)
            {
                var response = await _chatService.Next(m => m.Author.Id == partnerId, token);
                var trimmedContent = response.Content?.Trim(' ', '\'', '"');
                if (!string.Equals(trimmedContent, expected, StringComparison.OrdinalIgnoreCase))
                {
                    throw new TaskCanceledException();
                }
            }

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
                await Task.WhenAll(EnsureSays(partner1.Id, cts.Token), EnsureSays(partner2.Id, cts.Token));
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
            if (now - marriage.LastKissed < TimeSpan.FromDays(1)) throw new Exception("you can only kiss once a day");
            marriage.Kisses++;
            marriage.LastKissed = now;
            await dbContext.SaveChangesAsync();
            return marriage.Kisses;
        }

        private async Task<MarriedCouple> GetMarriage(IUser partner1, IUser partner2, AppDbContext dbContext)
        {
            var marriage = await dbContext.MarriedCouples
                .FirstOrDefaultAsync(c => c.Partner1Id == partner1.Id && c.Partner2Id == partner2.Id ||
                                          c.Partner2Id == partner1.Id && c.Partner1Id == partner2.Id);
            return marriage;
        }
    }
}
