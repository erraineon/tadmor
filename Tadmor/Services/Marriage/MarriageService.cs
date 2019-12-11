using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tadmor.Extensions;
using Tadmor.Services.Abstractions;
using Tadmor.Services.Data;
using Tadmor.Services.Marriage.Babies;

namespace Tadmor.Services.Marriage
{
    [SingletonService]
    public class MarriageService
    {
        private readonly ChatService _chatService;
        private readonly List<ulong> _channelsWithMarriages = new List<ulong>();
        private readonly IServiceProvider _babyFactory;

        public MarriageService(ChatService chatService, IServiceProvider babyFactory)
        {
            _chatService = chatService;
            _babyFactory = babyFactory;
        }

        public async Task DoWedding(IUser partner1, IGuildUser partner2, IMessageChannel channel, AppDbContext dbContext)
        {
            if (_channelsWithMarriages.Contains(channel.Id)) throw new Exception("someone's already getting married");
            if (partner1.Id == partner2.Id) throw new Exception("you cant marry yourself");
            var guildId = partner2.GuildId;
            await AssertNotMarried(partner2, guildId, dbContext);
            await AssertNotMarried(partner1, guildId, dbContext);
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
                var pronouncementMsg = "nice job youre married";
                await channel.SendMessageAsync(pronouncementMsg);
                await Task.Delay(TimeSpan.FromSeconds(1));
                await Marry(partner1, partner2, dbContext);
            }
            finally
            {
                _channelsWithMarriages.Remove(channel.Id);
            }
        }

        public async Task Marry(IUser partner1, IGuildUser partner2, AppDbContext dbContext)
        {
            await dbContext.MarriedCouples.AddAsync(new MarriedCouple
            {
                Partner1Id = partner1.Id,
                Partner2Id = partner2.Id,
                TimeStamp = DateTime.Now,
                GuildId = partner2.GuildId
            });
            await dbContext.SaveChangesAsync();
        }

        public async Task<string> CreateBaby(IUser partner1, IGuildUser partner2, string babyName, AppDbContext dbContext)
        {
            var marriage = await GetMarriage(partner1, partner2, dbContext);
            AssertBabyNameValid(babyName);
            var logger = new StringBuilderLogger();
            var babyCost = await CalculateBabyCost(marriage, logger);
            if (babyCost - marriage.Kisses is var missingKisses && missingKisses > 0) 
                throw new Exception($"you need {Math.Ceiling(missingKisses)} more kisses to make a baby");
            var maxBabiesCount = await CalculateMaxBabiesCount(marriage, logger);
            if (marriage.Babies.Count > maxBabiesCount)
                throw new Exception($"you can have at most {maxBabiesCount} babies");
            var baby = CreateRandomBaby();
            baby.Name = babyName;
            baby.BirthDate = DateTime.Now;
            baby.Rank = await CalculateBabyRank(marriage, logger);
            marriage.Babies.Add(baby);
            marriage.Kisses -= babyCost;
            await dbContext.SaveChangesAsync();
            logger.LogInformation($"you made {baby}");
            return logger.ToString();
        }

        public async Task<string> CombineBabies(IUser partner1, IGuildUser partner2, string babyName1, string babyName2,
            string newBabyName,
            AppDbContext dbContext)
        {
            var marriage = await GetMarriage(partner1, partner2, dbContext);
            var baby1 = GetBaby(babyName1, marriage);
            var baby2 = GetBaby(babyName2, marriage);
            AssertCanCombine(baby1);
            AssertCanCombine(baby2);
            AssertBabyNameValid(newBabyName);
            var logger = new StringBuilderLogger();
            var baby = CreateRandomBaby();
            baby.Name = newBabyName;
            baby.BirthDate = DateTime.Now;
            var minRank = (int)Math.Floor((baby1.Rank + baby2.Rank)/2f);
            baby.Rank = await CalculateBabyRank(marriage, logger, minRank);
            marriage.Babies.Add(baby);
            marriage.Babies.Remove(baby1);
            marriage.Babies.Remove(baby2);
            logger.LogInformation($"{baby1.Name} and {baby2.Name} were combined to create {baby}");
            await dbContext.SaveChangesAsync();
            return logger.ToString();
        }

        private void AssertCanCombine(Baby baby)
        {
            if (!baby.CanCombine) throw new Exception($"{baby.Name} can't be combined");
        }

        private static void AssertBabyNameValid(string babyName)
        {
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
            return (Baby) _babyFactory.GetService(babyType);
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
            dbContext.MarriedCouples.Remove(marriage);
            await dbContext.SaveChangesAsync();
        }

        public async Task<MarriedCouple> GetMarriage(IUser partner1, IUser partner2, AppDbContext dbContext)
        {
            var marriage = await GetMarriageOrNull(partner1, partner2, dbContext);
            if (marriage == null) throw new Exception($"{partner1.Username} is not married to {partner2.Username}");
            return marriage;
        }

        public async Task<string> Kiss(IUser partner1, IUser partner2, AppDbContext dbContext)
        {
            var marriage = await GetMarriageOrNull(partner1, partner2, dbContext);
            if (marriage == null) throw new Exception($"you can only kiss your partner");
            var logger = new StringBuilderLogger();
            var now = DateTime.Now;
            if (now - marriage.LastKissed is var timeSinceLastKiss && 
                marriage.KissCooldown - timeSinceLastKiss is var timeRemaining && 
                timeRemaining > TimeSpan.Zero) 
                throw new Exception($"you can kiss again in {timeRemaining.Humanize()}");
            marriage.Kisses = await CalculateKisses(marriage, logger);
            marriage.KissCooldown = await CalculateCooldown(marriage, logger);
            marriage.LastKissed = now;
            await dbContext.SaveChangesAsync();
            logger.LogInformation($"you have kissed your partner {Math.Floor(marriage.Kisses)} time(s) and " +
                                  $"you can again in {marriage.KissCooldown.Humanize()}");
            return logger.ToString();
        }

        public async Task SetKisses(IUser partner1, IUser partner2, int kisses, AppDbContext dbContext)
        {
            var marriage = await GetMarriage(partner1, partner2, dbContext);
            marriage.Kisses = kisses;
            await dbContext.SaveChangesAsync();
        }

        public async Task ResetCooldown(IUser partner1, IGuildUser partner2, AppDbContext dbContext)
        {
            var marriage = await GetMarriage(partner1, partner2, dbContext);
            marriage.KissCooldown = TimeSpan.Zero;
            await dbContext.SaveChangesAsync();
        }

        private async Task<int> CalculateMaxBabiesCount(MarriedCouple marriage, ILogger logger)
        {
            return 16;
        }

        private async Task<int> CalculateBabyRank(MarriedCouple marriage, ILogger logger, int minRank = 2)
        {
            var random = new Random();
            // a rank bonus of 1 ensures that the rank is always 10
            float rankBonus = 0;
            var cooldownAffectors = GetBabiesOfType<IBabyRankBonusAffector>(marriage);
            var currentRankBonus = rankBonus;
            foreach (var cooldownAffector in cooldownAffectors)
            {
                currentRankBonus = await cooldownAffector
                    .GetNewBabyRankBonus(currentRankBonus, rankBonus, marriage, cooldownAffectors, logger);
            }

            rankBonus = currentRankBonus;
            // logarithmic curve mapping 0..1 to 2..10
            return (int)Math.Round(-((10 - minRank) / 4f) * Math.Log(-80 * (random.NextDouble() - 1 + rankBonus) + 1, 3) + 10);
        }

        private async Task<TimeSpan> CalculateCooldown(MarriedCouple marriage, ILogger logger)
        {
            var baseCooldown = TimeSpan.FromHours(1);
            var cooldownAffectors = GetBabiesOfType<IKissCooldownAffector>(marriage);
            var currentCooldown = baseCooldown;
            foreach (var cooldownAffector in cooldownAffectors)
            {
                currentCooldown = await cooldownAffector
                    .GetNewCooldown(currentCooldown, baseCooldown, marriage, cooldownAffectors, logger);
            }

            return currentCooldown;
        }

        private async Task<float> CalculateKisses(MarriedCouple marriage, ILogger logger)
        {
            const float baseKissIncrement = 1f;
            var currentKisses = marriage.Kisses;
            var kissAffectors = GetBabiesOfType<IKissIncrementAffector>(marriage);
            var currentIncrement = baseKissIncrement;
            foreach (var kissAffector in kissAffectors)
            {
                currentIncrement = await kissAffector
                    .GetNewIncrement(currentIncrement, baseKissIncrement, marriage, kissAffectors, logger);
            }

            return currentKisses + currentIncrement;
        }

        private async Task<float> CalculateBabyCost(MarriedCouple marriage, ILogger logger)
        {
            var baseBabyCost = 10f;
            var costAffectors = GetBabiesOfType<IBabyCostAffector>(marriage);
            var currentCost = baseBabyCost;
            foreach (var costAffector in costAffectors)
            {
                currentCost = await costAffector
                    .GetNewCost(currentCost, baseBabyCost, marriage, costAffectors, logger);
            }

            return currentCost;
        }

        private static List<T> GetBabiesOfType<T>(MarriedCouple marriage)
        {
            return marriage.Babies
                .OfType<T>()
                .OrderBy(b => b!.GetType().GetCustomAttribute<BabyEffectOrderAttribute>()?.Order ?? 0)
                .ToList();
        }

        private async Task<MarriedCouple?> GetMarriageOrNull(IUser partner1, IUser partner2, AppDbContext dbContext)
        {
            var marriage = await dbContext.MarriedCouples
                .AsQueryable()
                .Include(c => c.Babies)
                .FirstOrDefaultAsync(c => c.Partner1Id == partner1.Id && c.Partner2Id == partner2.Id ||
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

        public async Task<string> GetBabiesInfo(IUser partner1, IGuildUser partner2, AppDbContext dbContext)
        {
            var marriage = await GetMarriage(partner1, partner2, dbContext);

            var babyStrings = marriage.Babies
                .OrderByDescending(b => b.Rank)
                .GroupBy(
                    b => $"{b.GetType().Name.Humanize()}: {b.GetDescription()}",
                    b => $"{Environment.NewLine}{b.Name} {b.GetStarRank()}",
                    (description, names) => $"**{description}**{string.Concat(names)}")
                .ToList();
            var result = babyStrings.Any() ? string.Join(Environment.NewLine, babyStrings) : "you have no babies";
            return result;
        }

        public async Task ReleaseBaby(IUser partner1, IGuildUser partner2, string babyName, AppDbContext dbContext)
        {
            var marriage = await GetMarriage(partner1, partner2, dbContext);
            var baby = GetBaby(babyName, marriage);
            await baby.Release(marriage);
            marriage.Babies.Remove(baby);
            await dbContext.SaveChangesAsync();
        }

        private static Baby GetBaby(string babyName, MarriedCouple marriage)
        {
            var baby = marriage.Babies
                           .FirstOrDefault(b => string.Equals(b.Name, babyName, StringComparison.OrdinalIgnoreCase)) ??
                       throw new Exception($"you have no baby named {babyName}");
            return baby;
        }

        private class StringBuilderLogger : ILogger
        {
            private readonly StringBuilder _builder = new StringBuilder();
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                _builder.AppendLine(state?.ToString());
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                throw new NotImplementedException();
            }

            public override string ToString()
            {
                return _builder.ToString();
            }
        }
    }
}
