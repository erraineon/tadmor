using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Flurl.Http;
using Microsoft.Extensions.Caching.Memory;
using Tadmor.Core.ChatClients.Discord.Interfaces;
using Tadmor.Core.ChatClients.Telegram.Interfaces;
using Tadmor.Core.Extensions;
using Tadmor.MessageRendering.Interfaces;

namespace Tadmor.MessageRendering.Services
{
    public class ImageProvider : IImageProvider
    {
        private readonly ICommandContext _commandContext;
        private readonly IMemoryCache _memoryCache;

        public ImageProvider(
            ICommandContext commandContext, 
            IMemoryCache memoryCache)
        {
            _commandContext = commandContext;
            _memoryCache = memoryCache;
        }

        public async Task<byte[]?> GetAvatarAsync(IUser user)
        {
            return _commandContext.Client switch
            {
                ITelegramChatClient telegramClient => await GetTelegramAvatar(user, telegramClient),
                IDiscordChatClient => await GetDiscordAvatar(user),
                _ => throw new NotSupportedException("can only retrieve images for telegram and discord")
            };
        }

        private async Task<byte[]?> GetDiscordAvatar(IUser user)
        {
            var avatarUrl = user.GetAvatarUrl();
            var avatar = avatarUrl != null
                ? await _memoryCache.GetOrCreateAsyncLock($"avatar-{avatarUrl}", async e =>
                {
                    CacheEntryExtensions.SetSlidingExpiration(e, TimeSpan.FromDays(1));
                    return await avatarUrl.GetBytesAsync();
                })
                : null;
            return avatar;
        }

        private async Task<byte[]?> GetTelegramAvatar(IUser user, ITelegramChatClient telegramClient)
        {
            var avatarId = await telegramClient.GetAvatarIdAsync(user);
            var avatar = avatarId != null
                ? await _memoryCache.GetOrCreateAsyncLock($"avatar-{avatarId}", async e =>
                {
                    e.SetSlidingExpiration(TimeSpan.FromDays(1));
                    return await telegramClient.DownloadFileAsync(avatarId);
                })
                : null;
            return avatar;
        }

        public async Task<IList<byte[]>> GetImagesAsync(IMessage message)
        {
            return _commandContext.Client switch
            {
                ITelegramChatClient telegramClient => await GetTelegramImages(message, telegramClient),
                IDiscordChatClient => await GetDiscordImages(message),
                _ => throw new NotSupportedException("can only retrieve images for telegram and discord")
            };
        }

        private async Task<IList<byte[]>> GetTelegramImages(IMessage message, ITelegramChatClient telegramClient)
        {
            var fileIds = message.Embeds
                .Select(e => e.Image?.ProxyUrl)
                .Concat(message.Attachments
                    .Select(a => a.Filename))
                .Where(fileId => fileId != default);
            var images = await Task.WhenAll(fileIds
                .Select(fileId => _memoryCache.GetOrCreateAsyncLock($"file-{fileId}", async e =>
                {
                    e.SetSlidingExpiration(TimeSpan.FromMinutes(10));
                    var file = await telegramClient.DownloadFileAsync(fileId!);
                    return file;
                })));
            return images;
        }

        private async Task<IList<byte[]>> GetDiscordImages(IMessage message)
        {
            var fileUrls = message.Embeds
                .Select(e => e.Thumbnail?.ProxyUrl)
                .Concat(message.Attachments.Select(a => a.Url))
                .Where(url => url != null);
            var images = await Task.WhenAll(fileUrls
                .Select(fileUrl => _memoryCache.GetOrCreateAsyncLock($"file-{fileUrl}", async e =>
                {
                    e.SetSlidingExpiration(TimeSpan.FromMinutes(10));
                    var file = await fileUrl.GetBytesAsync();
                    return file;
                })));
            return images;
        }
    }
}