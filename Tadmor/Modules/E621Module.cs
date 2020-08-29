using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using E621;
using Humanizer;
using Microsoft.Extensions.Logging;
using MoreLinq;
using Tadmor.Preconditions;
using Tadmor.Services.Abstractions;
using Tadmor.Services.Commands;
using Tadmor.Services.E621;

namespace Tadmor.Modules
{
    [Summary("e621")]
    public class E621Module : ModuleBase<ICommandContext>
    {
        private readonly E621Service _e621;
        private readonly ChatService _chatService;
        private readonly ILogger<E621Module> _logger;

        public E621Module(E621Service e621, ChatService chatService, ILogger<E621Module> logger)
        {
            _e621 = e621;
            _chatService = chatService;
            _logger = logger;
        }

        [Summary("search on e621")]
        [Command("e621")]
        [RequireNoGoodBoyMode(Group = "admin")]
        [RequireServiceUser(Group = "admin")]
        public async Task SearchRandom([Remainder] string tags)
        {
            var post = await _e621.SearchRandom(tags);
            await ReplyAsync(post.File.Url);
        }

        [Summary("starts a pokemon recognition game")]
        [Command("pokemongame")]
        [RequireNoGoodBoyMode(Group = "admin")]
        [RequireServiceUser(Group = "admin")]
        [RequireWhitelist]
        public async Task PokemonGame()
        {
            var channelId = Context.Channel.Id;
            var existingSession = _e621.GetSession(channelId);
            if (existingSession == null)
            {
                var newSession = _e621.CreateSession(channelId, "pokemon -young -gore -scat");
                RunSession(newSession, Context);
            }
            else
            {
                StopSession(existingSession);
            }
        }

        private void StopSession(PokemonGameSession existingSession)
        {
            existingSession.CancellationTokenSource.Cancel();
            ReplyAsync("the pokemon game was stopped");
        }

        private async void RunSession(PokemonGameSession session, ICommandContext commandContext)
        {
            var gameChannel = commandContext.Channel;
            while (!session.CancellationTokenSource.IsCancellationRequested)
            {
                var timeoutSource = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                try
                {
                    E621Post post;
                    do post = await _e621.SearchRandom(session.Tags);
                    while (post?.File?.Url == null);
                    await gameChannel.SendMessageAsync(post.File.Url);
                    var tag = post.Tags.Species
                        .Intersect(E621Service.PokemonNameTags)
                        .RandomSubset(1)
                        .Single();
                    var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
                        timeoutSource.Token,
                        session.CancellationTokenSource.Token);
                    var guessingMessage = await _chatService
                        .Next(um => um.Content != null &&
                                um.Channel.Id == gameChannel.Id &&
                                new[] {tag.Humanize(), tag}
                                    .Any(s => um.Content.Equals(s, StringComparison.OrdinalIgnoreCase)),
                            linkedSource.Token);
                    var guesserId = guessingMessage.Author.Id;
                    session.GuildUserScores[guesserId] =
                        session.GuildUserScores.TryGetValue(guesserId, out var currentScore)
                            ? currentScore + 1
                            : 1;
                    await gameChannel.SendMessageAsync($"the correct answer was {tag.Humanize()}");
                }
                catch (TaskCanceledException)
                {
                    if (timeoutSource.IsCancellationRequested)
                        await gameChannel.SendMessageAsync($"the pokemon game timed out due to inactivity");
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                }
            }
        }
    }
}