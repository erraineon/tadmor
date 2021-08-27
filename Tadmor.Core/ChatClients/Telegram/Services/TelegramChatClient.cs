using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Tadmor.Core.ChatClients.Telegram.Interfaces;
using Tadmor.Core.ChatClients.Telegram.Models;

namespace Tadmor.Core.ChatClients.Telegram.Services
{
    public class TelegramChatClient : ITelegramChatClient
    {
        private readonly TelegramOptions _telegramOptions;
        private readonly ITelegramGuildFactory _telegramGuildFactory;
        private readonly ITelegramApiClient _api;
        private readonly ITelegramCachedChatRepository _telegramCachedChatRepository;
        private IApplication? _application;

        public TelegramChatClient(
            TelegramOptions telegramOptions,
            ITelegramGuildFactory telegramGuildFactory,
            ITelegramApiClient api,
            ITelegramCachedChatRepository telegramCachedChatRepository)
        {
            _telegramOptions = telegramOptions;
            _telegramGuildFactory = telegramGuildFactory;
            _api = api;
            _telegramCachedChatRepository = telegramCachedChatRepository;
        }

        public void Dispose()
        {
            StopAsync();
        }

        public Task StartAsync()
        {
            _api.StartReceiving(null, CancellationToken.None);
            CurrentUser = new TelegramSelfUser(_api.BotId.Value);
            _application = new TelegramApplication(_telegramOptions.BotOwnerId);
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            _api.StopReceiving();
            return Task.CompletedTask;
        }

        public Task<IApplication?> GetApplicationInfoAsync(RequestOptions? options = null)
        {
            return Task.FromResult(_application);
        }

        public async Task<IChannel?> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            var channel = await _telegramGuildFactory.CreateAsync((long) id);
            return channel;
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
            var guild = await _telegramGuildFactory.CreateAsync((long) id);
            return guild;
        }

        public async Task<IReadOnlyCollection<IGuild>> GetGuildsAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            var guilds = Task.WhenAll(_telegramCachedChatRepository.GetAllChatIds()
                .Select(_telegramGuildFactory.CreateAsync));
            return await guilds;
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

        public Task<BotGateway> GetBotGatewayAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public ConnectionState ConnectionState => throw new NotImplementedException();
        public ISelfUser? CurrentUser { get; private set; }
        public TokenType TokenType => TokenType.Bot;
        public string Name => "telegram";
        public async Task<byte[]> DownloadFileAsync(string fileId)
        {
            var memoryStream = new MemoryStream();
            await _api.GetInfoAndDownloadFileAsync(fileId, memoryStream);
            return memoryStream.ToArray();
        }

        public async Task<string?> GetAvatarIdAsync(IUser user)
        {
            var userPhotos = (await _api.GetUserProfilePhotosAsync((int)user.Id, 0, 1)).Photos;
            return userPhotos.FirstOrDefault()?.FirstOrDefault().FileId;
        }
    }
}