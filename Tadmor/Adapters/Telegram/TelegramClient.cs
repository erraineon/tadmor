using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Caching.Memory;
using Tadmor.Extensions;
using Tadmor.Services.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Image = Tadmor.Services.Abstractions.Image;

namespace Tadmor.Adapters.Telegram
{
    public class TelegramClient : IDiscordClient
    {
        private const string GuildsKeyPrefix = "telegram-guilds";
        private readonly IMemoryCache _cache;
        private TelegramBotClient? _api;
        private IApplication? _application;

        public TelegramClient(TelegramClientConfig configuration, IMemoryCache cache)
        {
            _cache = cache;
            Configuration = configuration;
        }

        public TelegramClientConfig Configuration { get; }
        private TelegramBotClient Api => _api ?? throw new Exception($"call {nameof(LoginAsync)} first");

        public void Dispose()
        {
            StopAsync();
        }

        public Task StartAsync()
        {
            Api.StartReceiving();
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            Api.StartReceiving();
            return Task.CompletedTask;
        }

        public Task<IApplication?> GetApplicationInfoAsync(RequestOptions? options = null)
        {
            return Task.FromResult(_application);
        }

        public async Task<IChannel?> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            var chat = await Api.GetChatAsync(new ChatId((long) id));
            var guild = await GetTelegramGuild(chat);
            return guild;
        }

        public Task<IReadOnlyCollection<IPrivateChannel>> GetPrivateChannelsAsync(
            CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IDMChannel>> GetDMChannelsAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IGroupChannel>> GetGroupChannelsAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IConnection>> GetConnectionsAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public async Task<IGuild> GetGuildAsync(ulong id, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            return (await GetGuildsAsync()).SingleOrDefault(g => g.Id == id);
        }

        public Task<IReadOnlyCollection<IGuild>> GetGuildsAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            return Task.FromResult((IReadOnlyCollection<IGuild>) _cache.GetKeys()
                .OfType<string>()
                .Where(k => k.StartsWith(GuildsKeyPrefix))
                .Select(k => _cache.Get<IGuild>(k))
                .ToArray());
        }

        public Task<IGuild> CreateGuildAsync(string name, IVoiceRegion region, Stream? jpegIcon = null,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IInvite> GetInviteAsync(string inviteId, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IUser> GetUserAsync(string username, string discriminator, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IVoiceRegion>> GetVoiceRegionsAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetRecommendedShardCountAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public ConnectionState ConnectionState => throw new NotImplementedException();
        public ISelfUser? CurrentUser { get; private set; }
        public TokenType TokenType => TokenType.Bot;

        private async Task<TelegramGuild> GetTelegramGuild(Chat chat)
        {
            return await _cache.GetOrCreateAsyncLock($"{GuildsKeyPrefix}-{chat.Id}", async _ =>
            {
                var api = Api;
                var administrators = await api.GetChatAdministratorsAsync(new ChatId(chat.Id));
                var administratorIds = administrators
                    .Select(a => (ulong) a.User.Id)
                    .Concat(new[] {(ulong) api.BotId})
                    .ToHashSet();
                var guild = new TelegramGuild(this, api, chat, _cache, administratorIds);
                guild.MessageReceived += message => MessageReceived(message);
                return guild;
            });
        }

        public event Func<IUserMessage, Task> MessageReceived = _ => Task.CompletedTask;

        public Task LoginAsync(string? token, int botOwnerId)
        {
            if (!string.IsNullOrEmpty(token) && token != "your bot token")
            {
                _api = new TelegramBotClient(token);
                _api.OnMessage += OnMessageReceived;
                CurrentUser = new TelegramSelfUser(_api);
                _application = new TelegramApplication(botOwnerId);
            }

            return Task.CompletedTask;
        }

        private async void OnMessageReceived(object? sender, MessageEventArgs e)
        {
            var apiMessage = e.Message;
            if (apiMessage.Chat.Type != ChatType.Private)
            {
                var guild = await GetTelegramGuild(apiMessage.Chat);
                await guild.ProcessInboundMessage(apiMessage);
            }
        }

        public async Task<Image?> GetAvatarAsync(ulong userId)
        {
            var photo = await _cache.GetOrCreateAsyncLock($"telegram-userphoto-{userId}", async entry =>
            {
                entry.SetSlidingExpiration(TimeSpan.FromHours(4));
                var userPhotos = (await Api.GetUserProfilePhotosAsync((int)userId, 0, 1)).Photos;
                return userPhotos.FirstOrDefault()?.FirstOrDefault();
            });
           
            return photo != null ? new TelegramImage(this, photo.FileId) : default;
        }

        public async Task<byte[]> GetImageAsync(string fileId)
        {
            var data = await _cache.GetOrCreateAsyncLock($"telegram-file-{fileId}", async entry =>
            {
                entry.SetSlidingExpiration(TimeSpan.FromDays(1));
                var memoryStream = new MemoryStream();
                await Api.GetInfoAndDownloadFileAsync(fileId, memoryStream);
                return memoryStream.ToArray();
            });
            return data;
        }
    }
}