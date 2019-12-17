using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Tadmor.Extensions;
using Tadmor.Logging;
using Tadmor.Services.Abstractions;
using Tadmor.Services.Data;

namespace Tadmor.Services.Marriage
{
    [ScopedService]
    public class MarriageService
    {
        private readonly ChatService _chatService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _memoryCache;
        private readonly AppDbContext _dbContext;
        private readonly StringLogger _logger;
        private readonly ICommandContext _commandContext;

        public MarriageService(
            ChatService chatService, 
            IServiceProvider serviceProvider, 
            IMemoryCache memoryCache, 
            AppDbContext dbContext, 
            StringLogger logger,
            ICommandContext commandContext)
        {
            _chatService = chatService;
            _serviceProvider = serviceProvider;
            _memoryCache = memoryCache;
            _dbContext = dbContext;
            _logger = logger;
            _commandContext = commandContext;
        }

        public async Task DoWedding(IUser partner1, IUser partner2)
        {
            var channel = _commandContext.Channel;
            var cacheKey = $"marriagechannels-{channel.Id}";
            if (_memoryCache.TryGetValue(cacheKey, out _)) 
                throw new Exception("someone's already getting married");
            if (partner1.Id == partner2.Id) throw new Exception("you cant marry yourself");
            var guildId = _commandContext.Guild.Id;
            await AssertNotMarried(partner2, guildId);
            await AssertNotMarried(partner1, guildId);
            try
            {
                _memoryCache.CreateEntry(cacheKey);
                var partner1Name = partner1.Username;
                var partner2Name = partner2.Username;
                var invocationMsg = $"we are here today to celebrate with {partner1Name} and {partner2Name} " +
                                    "as they proclaim their love and commitment etc etc etc. if youre both " +
                                    "committed to it, please reply: ok";
                await channel.SendMessageAsync(invocationMsg);
                await EnsureBothSay(partner1, partner2, "ok");
                await Task.Delay(TimeSpan.FromSeconds(1));
                var pronouncementMsg = "nice job youre married";
                await channel.SendMessageAsync(pronouncementMsg);
                await Marry(partner1, partner2);
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            finally
            {
                _memoryCache.Remove(cacheKey);
            }
        }

        public async Task Marry(IUser partner1, IUser partner2)
        {
            await _dbContext.MarriedCouples.AddAsync(new MarriedCouple
            {
                Partner1Id = partner1.Id,
                Partner2Id = partner2.Id,
                TimeStamp = DateTime.Now,
                GuildId = _commandContext.Guild.Id,
                Kisses = 10
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task<string> CreateBaby(IUser partner1, IUser? partner2, string babyName)
        {
            var marriage = await GetMarriage(partner1, partner2);
            SanitizeBabyName(ref babyName);
            var babyCost = Aggregate<IBabyCostEffector, double>(marriage, 10.0);
            if (babyCost - marriage.Kisses is var missingKisses && missingKisses > 0) 
                throw new Exception($"you need {Math.Ceiling(missingKisses)} more kisses to make a baby");
            babyCost = Aggregate<ILateBabyCostEffector, double>(marriage, babyCost);
            var maxBabiesCount = CalculateMaxBabiesCount(marriage);
            if (marriage.Babies.Count >= maxBabiesCount)
                throw new Exception($"you can have at most {maxBabiesCount} babies");
            var baby = CreateRandomBaby();
            baby.Name = babyName;
            baby.BirthDate = DateTime.Now;
            baby.Rank = CalculateBabyRank(marriage);
            marriage.Babies.Add(baby);
            marriage.Kisses -= (float)babyCost;
            await _dbContext.SaveChangesAsync();
            _logger.Log($"you made {baby}");
            return _logger.ToString();
        }

        public async Task<string> CombineBabies(IUser partner1, IUser? partner2, string babyName1, string babyName2,
            string newBabyName)
        {
            var marriage = await GetMarriage(partner1, partner2);
            var baby1 = GetBaby(babyName1, marriage);
            var baby2 = GetBaby(babyName2, marriage);
            await baby1.ExecuteCombinePrecondition(marriage);
            await baby2.ExecuteCombinePrecondition(marriage);
            SanitizeBabyName(ref newBabyName);
            var baby = CreateRandomBaby();
            baby.Name = newBabyName;
            baby.BirthDate = DateTime.Now;
            var minRank = (int)Math.Round((baby1.Rank + baby2.Rank)/2f, MidpointRounding.ToPositiveInfinity);
            baby.Rank = CalculateBabyRank(marriage, minRank);
            marriage.Babies.Add(baby);
            marriage.Babies.Remove(baby1);
            marriage.Babies.Remove(baby2);
            _logger.Log($"{baby1.Name} and {baby2.Name} were combined to create {baby}");
            await _dbContext.SaveChangesAsync();
            return _logger.ToString();
        }

        private static void SanitizeBabyName(ref string babyName)
        {
            babyName = babyName.Trim('\"');
            const int maxBabyNameLength = 64;
            if (babyName.Length > maxBabyNameLength)
                throw new Exception($"baby names can't exceed {maxBabyNameLength} characters");
        }

        private Baby CreateRandomBaby()
        {
            var rng = new Random();
            var babyTypes = Assembly.GetExecutingAssembly().ExportedTypes
                .Where(type => typeof(Baby).IsAssignableFrom(type) && !type.IsAbstract)
                .ToList();
            var babyType = babyTypes
                .Random(t => t.GetCustomAttribute<BabyFrequencyAttribute>()?.Weight ?? 1, rng);
            return Activator.CreateInstance(babyType) as Baby ?? throw new Exception($"unable to create {babyType.Name}");
        }

        
        private async Task AssertNotMarried(IUser partner, ulong guildId)
        {
            var existingMarriage = await _dbContext.MarriedCouples
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
            var marriage = await GetMarriage(partner1, partner2);
            dbContext.MarriedCouples.Remove(marriage);
            await dbContext.SaveChangesAsync();
        }

        public async Task<MarriedCouple> GetMarriage(IUser partner1, IUser? partner2)
        {
            var marriage = await GetMarriageOrNull(partner1, partner2);
            if (marriage == null) throw new Exception($"{partner1.Username} is not married to {partner2!.Username}");
            return marriage;
        }

        public async Task<string> Kiss(IUser partner1, IUser partner2, AppDbContext dbContext)
        {
            var marriage = await GetMarriageOrNull(partner1, partner2);
            if (marriage == null) throw new Exception($"you can only kiss your partner");
            var now = DateTime.Now;
            if (now - marriage.LastKissed is var timeSinceLastKiss && 
                marriage.KissCooldown - timeSinceLastKiss is var timeRemaining && 
                timeRemaining > TimeSpan.Zero) 
                throw new Exception($"you can kiss again in {timeRemaining.Humanize()}");
            marriage.Kisses = (float)CalculateKisses(marriage);
            marriage.KissCooldown = CalculateCooldown(marriage);
            marriage.LastKissed = now;
            await dbContext.SaveChangesAsync();
            _logger.Log($"you have kissed your partner {Math.Floor(marriage.Kisses)} time(s) and " +
                                   $"you can again in {marriage.KissCooldown.Humanize()}");
            return _logger.ToString();
        }

        public async Task SetKisses(IUser partner1, IUser partner2, int kisses)
        {
            var marriage = await GetMarriage(partner1, partner2);
            marriage.Kisses = kisses;
            await _dbContext.SaveChangesAsync();
        }

        public async Task ResetCooldown(IUser partner1, IGuildUser partner2)
        {
            var marriage = await GetMarriage(partner1, partner2);
            marriage.KissCooldown = TimeSpan.Zero;
            await _dbContext.SaveChangesAsync();
        }

        private int CalculateMaxBabiesCount(MarriedCouple marriage)
        {
            return 16;
        }

        private int CalculateBabyRank(MarriedCouple marriage, int minRank = 2)
        {
            var random = new Random();
            // a rank bonus of 1 ensures that the rank is always 10
            var rankBonus = Aggregate<IBabyRankBonusEffector, double>(marriage, 0);
            // logarithmic curve mapping 0..1 to 2..10
            return (int)Math.Round(-((10 - minRank) / 4f) * Math.Log(-80 * (random.NextDouble() - 1 + rankBonus) + 1, 3) + 10);
        }

        private TimeSpan CalculateCooldown(MarriedCouple marriage)
        {
            var baseCooldown = TimeSpan.FromHours(1);
            var increment = Aggregate<IKissCooldownEffector, TimeSpan>(marriage, baseCooldown);
            return increment;
        }

        private double CalculateKisses(MarriedCouple marriage)
        {
            const float baseKissIncrement = 1f;
            var increment = Aggregate<IKissGainEffector, double>(marriage, baseKissIncrement);
            return marriage.Kisses + increment;
        }

        private TValue Aggregate<TEffector, TValue>(
            MarriedCouple couple,
            TValue seed) where TEffector: IMarriageEffector<TValue>
        {
            return _serviceProvider.GetServices<TEffector>()
                .OrderBy(b => b!.GetType().GetCustomAttribute<EffectorOrderAttribute>()?.Order ?? 0)
                .Aggregate(seed, (current, affector) => affector.GetNewValue(current, seed, couple));
        }

        private async Task<MarriedCouple?> GetMarriageOrNull(IUser partner1, IUser? partner2)
        {
            var guildId = _commandContext.Guild.Id;
            var queryable = _dbContext.MarriedCouples
                .AsQueryable()
                .Include(c => c.Babies)
                .Where(c => c.GuildId == guildId);
            var marriage = partner2 != null
                ? await queryable
                    .FirstOrDefaultAsync(c =>
                        c.Partner1Id == partner1.Id && c.Partner2Id == partner2.Id ||
                        c.Partner2Id == partner1.Id && c.Partner1Id == partner2.Id)
                : await queryable
                      .Where(c => c.Partner1Id == partner1.Id || c.Partner2Id == partner1.Id)
                      .ToListAsync() is var marriages && marriages.Count <= 1
                    ? marriages.SingleOrDefault()
                    : throw new Exception(
                        $"multiple marriages exist for {partner1.Username}, disambiguate by specifying the other partner's name");
            return marriage;
        }

        public async Task<IList<MarriedCouple>> GetMarriages(IGuild guild)
        {
            var marriedCouples = await _dbContext.MarriedCouples
                .AsQueryable()
                .Where(c => c.GuildId == guild.Id)
                .ToListAsync();
            return marriedCouples;
        }

        public async Task<string> GetMarriageInfo(IUser partner1, IUser? partner2)
        {
            var marriage = await GetMarriage(partner1, partner2);
            var nl = Environment.NewLine;
            var babyStrings = marriage.Babies
                .OrderByDescending(b => b.Rank)
                .GroupBy(
                    b => $"{b.GetType().Name.Humanize()}: {b.GetDescription()}",
                    (description, babies) =>
                    {
                        var babiesByRank = babies.GroupBy(
                            b => b.GetStarRank(),
                            b => b.Name,
                            (rank, names) => $"{nl}{rank}: {string.Join(", ", names)}");
                        return $"**{description}**{string.Concat(babiesByRank)}";
                    })
                .ToList();
            var maxBabiesCount = CalculateMaxBabiesCount(marriage);
            var result = babyStrings.Any()
                ? $"babies: {marriage.Babies.Count}/{maxBabiesCount}{nl}{string.Join(nl, babyStrings)}"
                : "you have no babies";
            return result;
        }

        public async Task ReleaseBaby(IUser partner1, IUser? partner2, string babyName)
        {
            var marriage = await GetMarriage(partner1, partner2);
            var baby = GetBaby(babyName, marriage);
            await baby.Release(marriage);
            marriage.Babies.Remove(baby);
            await _dbContext.SaveChangesAsync();
        }

        private static Baby GetBaby(string babyName, MarriedCouple marriage)
        {
            var baby = marriage.Babies
                           .FirstOrDefault(b => string.Equals(b.Name, babyName, StringComparison.OrdinalIgnoreCase)) ??
                       throw new Exception($"you have no baby named {babyName}");
            return baby;
        }
    }
}
