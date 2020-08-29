using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using E621;
using MoreLinq;
using Tadmor.Preconditions;
using Tadmor.Services.Abstractions;
using Tadmor.Services.Commands;
using Tadmor.Services.Discord;
using Tadmor.Services.E621;

namespace Tadmor.Modules
{
    [Summary("e621")]
    public class E621Module : ModuleBase<ICommandContext>
    {
        private readonly E621Service _e621;
        private readonly ChatService _chatService;

        public E621Module(E621Service e621, ChatService chatService)
        {
            _e621 = e621;
            _chatService = chatService;
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
                _e621.StopSession(existingSession);
            }
        }

        private async void RunSession(PokemonGameSession session, ICommandContext commandContext)
        {
            E621Post post;
            do post = await _e621.SearchRandom(session.Tags);
            while (post?.File?.Url == null);
            await commandContext.Channel.SendMessageAsync(post.File.Url);
            var tag = post.Tags.Species.RandomSubset(1).Single();
            var timeoutSource = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, session.CancellationTokenSource.Token);
            var guessingMessage = await _chatService
                .Next(um => um.Channel.Id == commandContext.Channel.Id &&
                            um.Content.Equals(tag, StringComparison.OrdinalIgnoreCase), linkedSource.Token);

        }
    }
}