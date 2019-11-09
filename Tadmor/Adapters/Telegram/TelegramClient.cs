using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Tadmor.Services.Abstractions;
using Tadmor.Utils;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Image = Tadmor.Services.Abstractions.Image;

namespace Tadmor.Adapters.Telegram
{
    public class TelegramClient : IDiscordClient
    {
        private readonly AsyncConcurrentDictionary<long, TelegramGuild> _guildsByChatId = new AsyncConcurrentDictionary<long, TelegramGuild>();
        private TelegramBotClient? _api;
        private IApplication? _application;
        private readonly IDictionary<string, byte[]> _imagesCache = new Dictionary<string, byte[]>();

        public TelegramClient(TelegramClientConfig configuration)
        {
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

        public Task<IGuild> GetGuildAsync(ulong id, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IGuild>> GetGuildsAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            return Task.FromResult((IReadOnlyCollection<IGuild>) _guildsByChatId.Values.ToList().AsReadOnly());
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
            return await _guildsByChatId.GetOrAddAsync(chat.Id, async chatId =>
            {
                var api = Api;
                var administrators = await api.GetChatAdministratorsAsync(new ChatId(chatId));
                var administratorIds = administrators
                    .Select(a => (ulong) a.User.Id)
                    .Concat(new[] {(ulong) api.BotId})
                    .ToHashSet();
                return new TelegramGuild(this, api, chat, administratorIds);
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
                var telegramMessage = guild.ProcessInboundMessage(apiMessage);
                await MessageReceived(telegramMessage);
            }
        }

        public async Task<Image?> GetAvatarAsync(ulong userId)
        {
            var userPhotos = (await Api.GetUserProfilePhotosAsync((int) userId, 0, 1)).Photos;
            var photo = userPhotos.FirstOrDefault()?.FirstOrDefault();
            return photo != null ? new TelegramImage(this, photo.FileId) : default;
        }

        public async Task<byte[]> GetImageAsync(string fileId)
        {
            var memoryStream = new MemoryStream();
            await Api.GetInfoAndDownloadFileAsync(fileId, memoryStream);
            var data = memoryStream.ToArray();
            return data;
        }
    }
}