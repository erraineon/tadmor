using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Discord.TelegramAdapter
{
    public class TelegramClient : IDiscordClient
    {
        private static readonly Dictionary<long, TelegramGuild> TelegramGuildsByChatId =
            new Dictionary<long, TelegramGuild>();

        private TelegramBotClient? _api;
        private IApplication? _application;

        public TelegramClient(TelegramClientConfig configuration)
        {
            Configuration = configuration;
        }

        public TelegramClientConfig Configuration { get; }

        public void Dispose()
        {
            StopAsync();
        }

        private async Task WithInitializedApi(Func<TelegramBotClient, Task> action)
        {
            await WithInitializedApi<object?>(async api =>
            {
                await action(api);
                return default;
            });
        }

        private Task<T> WithInitializedApi<T>(Func<TelegramBotClient, Task<T>> action)
        {
            if (_api == null) throw new Exception($"call {nameof(LoginAsync)} first");
            return action(_api);
        }

        public Task StartAsync()
        {
            return WithInitializedApi(api =>
            {
                api.StartReceiving();
                return Task.CompletedTask;
            });
        }

        public Task StopAsync()
        {
            return WithInitializedApi(api =>
            {
                api.StopReceiving();
                return Task.CompletedTask;
            });
        }

        public Task<IApplication?> GetApplicationInfoAsync(RequestOptions? options = null)
        {
            return Task.FromResult(_application);
        }

        public async Task<IChannel?> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            return await WithInitializedApi(async api =>
            {
                var chat = await api.GetChatAsync(new ChatId((long)id));
                var guild = await GetTelegramGuild(chat);
                return guild;
            });
        }

        public Task<IReadOnlyCollection<IPrivateChannel>> GetPrivateChannelsAsync(
            CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IDMChannel>> GetDMChannelsAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IGroupChannel>> GetGroupChannelsAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IConnection>> GetConnectionsAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IGuild> GetGuildAsync(ulong id, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IGuild>> GetGuildsAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IGuild> CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon = null,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IInvite> GetInviteAsync(string inviteId, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IUser> GetUserAsync(string username, string discriminator, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetRecommendedShardCountAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public ConnectionState ConnectionState { get; }
        public ISelfUser CurrentUser { get; set; }
        public TokenType TokenType => TokenType.Bot;

        private async Task<TelegramGuild?> GetTelegramGuild(Chat chat)
        {
            return await WithInitializedApi(async api =>
            {
                var chatId = chat.Id;
                if (!TelegramGuildsByChatId.TryGetValue(chatId, out var guild))
                {
                    var administrators = await api.GetChatAdministratorsAsync(new ChatId(chatId));
                    var administratorIds = administrators
                        .Select(a => (ulong) a.User.Id)
                        .Concat(new[] {(ulong) api.BotId})
                        .ToHashSet();
                    TelegramGuildsByChatId[chatId] = guild = new TelegramGuild(this, api, chat, administratorIds);
                }
                return guild;
            });
        }

        public event Func<IUserMessage, Task> MessageReceived = _ => Task.CompletedTask;

        public async Task LoginAsync(string token, int botOwnerId)
        {
            if (!string.IsNullOrEmpty(token) && token != "your bot token")
            {
                var client = new TelegramBotClient(token);
                client.OnMessage += OnMessageReceived;
                _api = client;
                CurrentUser = new TelegramSelfUser(client);
                _application = new TelegramApplication(botOwnerId);
            }
        }

        private async void OnMessageReceived(object sender, MessageEventArgs e)
        {
            var msg = e.Message;
            if (msg.Chat.Type != ChatType.Private)
            {
                var guild = await GetTelegramGuild(msg.Chat);
                var message = guild.ProcessInboundMessage(msg);
                await MessageReceived(message);
            }
        }

        public async Task<(byte[] data, string avatarId)?> GetAvatarAsync(ulong userId)
        {
            var photos = await _api.GetUserProfilePhotosAsync((int)userId, 0, 1);
            if (photos.Photos.Length > 0)
            {
                var photo = photos.Photos[0].FirstOrDefault();
                if (photo != null)
                {
                    var memoryStream = new MemoryStream();
                    var file = await _api.GetFileAsync(photo.FileId);
                    await _api.DownloadFileAsync(file.FilePath, memoryStream);
                    return (memoryStream.ToArray(), photo.FileId);
                }
            }

            return default;
        }

        public async Task<> GetAllImagesAsync(ICommandContext context, ICollection<string> linkedUrls)
        {
            throw new NotImplementedException();
        }
    }

    public class TelegramClientConfig
    {
        public int MessageCacheSize { get; set; }
    }
}