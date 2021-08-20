using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using MoreLinq.Extensions;
using Tadmor.Core.Commands.Models;
using Tadmor.Core.Extensions;
using Tadmor.Raffles.Interfaces;

namespace Tadmor.Raffles.Modules
{
    [RequireUserPermission(GuildPermission.Administrator)]
    public class RaffleModule : ModuleBase<ICommandContext>
    {
        private readonly IRaffleDrawingService _raffleDrawingService;
        private readonly IEnumerable<IRaffleBiasStrategy> _raffleBiasStrategies;
        private readonly IRaffleWinnersRepository _raffleWinnersRepository;

        public RaffleModule(IRaffleDrawingService raffleDrawingService, 
            IEnumerable<IRaffleBiasStrategy> raffleBiasStrategies,
            IRaffleWinnersRepository raffleWinnersRepository)
        {
            _raffleDrawingService = raffleDrawingService;
            _raffleBiasStrategies = raffleBiasStrategies;
            _raffleWinnersRepository = raffleWinnersRepository;
        }

        [Command("raffle winners")]
        public async Task<RuntimeResult> AddWinnersManually(DateTime extractionTime, params IUser[] users)
        {
            await _raffleWinnersRepository.AddWinnersAsync(users, GetRaffleId(), extractionTime);
            return CommandResult.FromSuccess(
                $"added raffle victory at {extractionTime} for {users.Humanize(u => u.Username)}", true);
        }

        [Command("raffle ls")]
        public async Task<RuntimeResult> ListAllExtractionsAsync()
        {
            var extractions = await _raffleWinnersRepository.GetAllExtractionsAsync(GetRaffleId());
            var extractionsByName = (await Task.WhenAll(extractions
                    .GroupBy(e => e.UserId)
                    .Select(async g =>
                    {
                        var user = await Context.Guild.GetUserAsync(g.Key);
                        var username = user?.Username ?? $"missing user {g.Key}";
                        var extractionTimes = g
                            .Select(e => e.ExtractionTime)
                            .OrderBy(d => d)
                            .Humanize(d => d.ToShortDateString());
                        return $"{username}: {extractionTimes}";
                    })))
                .OrderBy(s => s)
                .ToList();
            return CommandResult.FromSuccess(extractionsByName);
        }

        [Command("raffle")]
        public async Task<RuntimeResult> Draw(int winnersCount, string? raffleType = default)
        {
            if (winnersCount < 1) throw new ModuleException("there must be one or more winners");
            var targetMessage = await Context.GetSelectedMessageAsync();
            var participants = await GetUsersWhoReactedAsync(targetMessage);
            if (participants.Count < winnersCount) throw new ModuleException("there are not enough participants");
            var raffleStrategy = raffleType == default ? null : GetRaffleStrategy(raffleType);
            var raffleId = GetRaffleId();
            var winners = await _raffleDrawingService.DrawAsync(participants, winnersCount, raffleId, raffleStrategy);
            return CommandResult.FromSuccess(winners.Select(u => u.Mention).Humanize(), true);
        }

        private string GetRaffleId() => $"{Context.Guild.Id}-{Context.User.Id}";

        private IRaffleBiasStrategy GetRaffleStrategy(string raffleType)
        {
            return _raffleBiasStrategies
                    .FirstOrDefault(s => string.Equals(s.Name, raffleType, StringComparison.OrdinalIgnoreCase)) ??
                throw new ModuleException(
                    $"no raffle with type {raffleType} was found. " +
                    $"available raffle types: {_raffleBiasStrategies.Humanize()}");
        }

        private async Task<ICollection<IUser>> GetUsersWhoReactedAsync(IMessage targetMessage)
        {
            var participants = (await targetMessage.Reactions.Keys
                    .Select(e => targetMessage.GetReactionUsersAsync(e, int.MaxValue))
                    .ToAsyncEnumerable()
                    .SelectMany(g => g)
                    .FlattenAsync())
                .DistinctBy(u => u.Id)
                .Where(u => u.Id != Context.User.Id)
                .ToList();
            if (!participants.Any()) throw new ModuleException("no users reacted to the message");
            return participants;
        }
    }
}