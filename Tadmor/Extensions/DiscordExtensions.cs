using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using E621;
using Humanizer;
using Tadmor.Services.WorldStar;

namespace Tadmor.Extensions
{
    public static class DiscordExtensions
    {
        public static Embed ToEmbed(this E621Post post)
        {
            var author = post.Artists.Any() ? post.Artists.Humanize() : post.Author;
            var builder = new EmbedBuilder()
                .WithTitle($"id: {post.Id} • score: {post.Score}")
                .WithAuthor(author)
                .WithDescription(post.Description.Truncate(EmbedBuilder.MaxDescriptionLength))
                .WithImageUrl(post.FileUrl)
                .WithUrl($"https://e621.net/post/show/{post.Id}");
            return builder.Build();
        }

        public static async Task<IList<string>> GetAllImageUrls(this ICommandContext context, ICollection<string> linkedUrls)
        {
            var urlAttachments = context.Message.Attachments.Where(a => a.Width != null).Select(a => a.Url);
            var linkedProxyUrls = await Task.WhenAll(linkedUrls
                .Where(linkedUrl => Uri.TryCreate(linkedUrl, UriKind.Absolute, out _))
                .Select(context.GetProxyImageUrl));
            var oldMessages = await context.Channel
                .GetMessagesAsync(context.Message, Direction.Before, mode: CacheMode.CacheOnly).FlattenAsync();
            var oldEmbeds = oldMessages
                .SelectMany(m => m.Embeds
                    .Select(e => e.Thumbnail?.ProxyUrl)
                    .Concat(m.Attachments
                        .Select(a => a.Url)))
                .Where(t => t != null);
            return urlAttachments.Concat(linkedProxyUrls).Concat(oldEmbeds).ToList();
        }

        public static async Task<string> GetProxyImageUrl(this ICommandContext context, string linkedUrl)
        {
            if (TryGetProxyUrl(context.Message, out var proxyUrl)) return proxyUrl;
            var client = (DiscordSocketClient)context.Client;
            var tcs = new TaskCompletionSource<string>();
            var cts = new CancellationTokenSource();

            bool TryGetProxyUrl(IMessage message, out string url)
            {
                url = message.Embeds
                    .Where(e => e.Thumbnail?.Url == linkedUrl)
                    .Select(e => e.Thumbnail?.ProxyUrl).FirstOrDefault();
                return url != null;
            }

            Task OnClientOnMessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage updatedMessage,
                ISocketMessageChannel arg3)
            {
                if (updatedMessage.Id == context.Message.Id && TryGetProxyUrl(updatedMessage, out var u))
                {
                    client.MessageUpdated -= OnClientOnMessageUpdated;
                    tcs.TrySetResult(u);
                }

                return Task.CompletedTask;
            }

            cts.Token.Register(() =>
            {
                client.MessageUpdated -= OnClientOnMessageUpdated;
                tcs.TrySetCanceled(cts.Token);
            });
            client.MessageUpdated += OnClientOnMessageUpdated;
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            try
            {
                return await tcs.Task;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }
    }
}