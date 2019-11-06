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

        public static async IAsyncEnumerable<Image> GetAllImagesAsync(ICommandContext context, ICollection<string> linkedUrls)
        {
            // obtain the url of the image after discord uploads it to its proxy to avoid leaking bot ip info
            async ValueTask<string?> GetProxyImageUrl(string linkedUrl)
            {
                bool TryGetProxyUrl(IMessage message, out string? url)
                {
                    url = message.Embeds.FirstOrDefault(e => e.Thumbnail?.Url == linkedUrl)?.Thumbnail?.ProxyUrl;
                    return url != null;
                }

                if (!TryGetProxyUrl(context.Message, out var proxyUrl))
                {
                    var client = (DiscordSocketClient)context.Client;
                    var proxyImageLoadTask = new TaskCompletionSource<string>();

                    Task OnMessageUpdated(
                        Cacheable<IMessage, ulong> _,
                        SocketMessage updatedMessage,
                        ISocketMessageChannel __)
                    {
                        if (updatedMessage.Id == context.Message.Id && TryGetProxyUrl(updatedMessage, out var newUrl))
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

            IAsyncEnumerable<Image> GetAllDiscordImagesAsync()
            {
                var linkedProxyUrls = linkedUrls
                    .Where(linkedUrl => Uri.TryCreate(linkedUrl, UriKind.Absolute, out _))
                    .ToAsyncEnumerable()
                    .SelectAwait(GetProxyImageUrl);
                var attachmentsAndEmbeds = context.Channel
                    .GetMessagesAsync()
                    .Flatten()
                    .SelectMany(m => m.Embeds
                        .Select(e => e.Thumbnail?.ProxyUrl)
                        .Concat(m.Attachments.Select(a => a.Url))
                        .ToAsyncEnumerable());
                return linkedProxyUrls
                    .Concat(attachmentsAndEmbeds)
                    .Where(url => url != null)
                    .Select(url => new DiscordImage(url!));
            }

            async Task<IAsyncEnumerable<Image>> GetAllTelegramImagesAsync(TelegramClient telegram)
            {
                // HACK: messages with multiple photos are split in messages in which only the first one contains the command caption
                // the line below allows the following messages to be cached before the list of messages is retrieved
                await Task.Delay(10);
                var allMessages = context.Channel
                    .GetMessagesAsync()
                    .Flatten()
                    .SelectMany(m => m.Attachments
                        .Select(a => new TelegramImage(telegram, a.Filename))
                        .ToAsyncEnumerable());
                return allMessages;
            }

            var images = context.Client switch
            {
                TelegramClient telegram => await GetAllTelegramImagesAsync(telegram),
                DiscordSocketClient _ => GetAllDiscordImagesAsync(),
                _ => throw new NotSupportedException($"{context.Client.GetType()} image retrieval is not supported")
            };
            await foreach (var image in images)
            {
                yield return image;
            }
        }
    }
}