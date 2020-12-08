using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Tadmor.Adapters.Telegram;
using Tadmor.Services.Abstractions;
using Image = Tadmor.Services.Abstractions.Image;

namespace Tadmor.Extensions
{
    public static class ImageRetrievalExtensions
    {
        public static async ValueTask<Image?> GetAvatarAsync(this IGuildUser user)
        {
            static Image? GetDiscordAvatar(SocketGuildUser discordUser)
            {
                return discordUser.GetAvatarUrl() is {} avatarUrl
                    ? new DiscordImage(avatarUrl)
                    : default;
            }

            var avatar = user switch
            {
                TelegramGuildUser telegramUser => await telegramUser.GetAvatarAsync(),
                SocketGuildUser discordUser => GetDiscordAvatar(discordUser),
                _ => throw new NotSupportedException($"{user.GetType()} avatar retrieval is not supported")
            };

            return avatar;
        }


        // obtain the url of the image after discord uploads it to its proxy to avoid leaking bot ip info
        private static async ValueTask<string?> GetProxyImageUrl(IUserMessage userMessage, DiscordSocketClient client, string linkedUrl)
        {
            bool TryGetProxyUrl(IMessage message, out string? url)
            {
                url = message.Embeds.FirstOrDefault(e => e.Thumbnail?.Url == linkedUrl)?.Thumbnail?.ProxyUrl;
                return url != null;
            }

            if (!TryGetProxyUrl(userMessage, out var proxyUrl))
            {
                var proxyImageLoadTask = new TaskCompletionSource<string>();

                Task OnMessageUpdated(
                    Cacheable<IMessage, ulong> _,
                    SocketMessage updatedMessage,
                    ISocketMessageChannel __)
                {
                    if (updatedMessage.Id == userMessage.Id && TryGetProxyUrl(updatedMessage, out var newUrl))
                        proxyImageLoadTask.TrySetResult(newUrl!);

                    return Task.CompletedTask;
                }

                var cts = new CancellationTokenSource();
                cts.Token.Register(() => proxyImageLoadTask.TrySetCanceled(cts.Token));
                client.MessageUpdated += OnMessageUpdated;
                // give discord a maximum of 5 seconds to load the image
                cts.CancelAfter(TimeSpan.FromSeconds(5));
                try
                {
                    proxyUrl = await proxyImageLoadTask.Task;
                }
                catch (TaskCanceledException)
                {
                    proxyUrl = default;
                }
                finally
                {
                    client.MessageUpdated -= OnMessageUpdated;
                }
            }

            return proxyUrl;
        }

        public static async IAsyncEnumerable<Image> GetAllImagesAsync(this ICommandContext context, ICollection<string> linkedUrls, bool scanOnlyOwnMessages)
        {
            // HACK: telegram messages with multiple photos are split in messages in which only the first one contains the command caption
            // the line below allows the following messages to be cached before the list of messages is retrieved
            await Task.Delay(10);
            var images = context.Channel
                .GetMessagesAsync()
                .Flatten()
                .Where(m => !scanOnlyOwnMessages || m.Author.Id == context.User.Id)
                .OfType<IUserMessage>()
                .SelectMany(m => GetAllImagesAsync(m, context.Client, linkedUrls));
            await foreach (var image in images)
            {
                yield return image;
            }
        }

        public static async IAsyncEnumerable<Image> GetAllImagesAsync(this IUserMessage userMessage, IDiscordClient client, ICollection<string> linkedUrls)
        {

            IAsyncEnumerable<Image> GetAllDiscordImages(DiscordSocketClient discord)
            {
                var attachmentsAndEmbeds = userMessage.Embeds
                    .Select(e => e.Thumbnail?.ProxyUrl)
                    .Concat(userMessage.Attachments.Select(a => a.Url))
                    .ToAsyncEnumerable()
                    .Concat(linkedUrls
                        .Where(linkedUrl => userMessage.Content.Contains(linkedUrl) &&
                                            Uri.TryCreate(linkedUrl, UriKind.Absolute, out _))
                        .ToAsyncEnumerable()
                        .SelectAwait(u => GetProxyImageUrl(userMessage, discord, u)))
                    .Where(url => url != null)
                    .Select(url => new DiscordImage(url!));
                return attachmentsAndEmbeds;
            }

            IAsyncEnumerable <Image> GetAllTelegramImages(TelegramClient telegram)
            {
                var attachments = userMessage.Attachments
                        .Select(a => new TelegramImage(telegram, a.Filename))
                        .ToAsyncEnumerable();
                return attachments;
            }

            var images = client switch
            {
                TelegramClient telegram => GetAllTelegramImages(telegram),
                DiscordSocketClient discord => GetAllDiscordImages(discord),
                _ => throw new NotSupportedException($"{client.GetType()} image retrieval is not supported")
            };
            await foreach (var image in images)
            {
                yield return image;
            }
        }
    }
}