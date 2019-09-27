using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Tadmor.Services.Telegram
{
    public class TelegramWrapper : IDiscordClient
    {
        public static readonly Dictionary<long, TelegramGuild> TelegramGuildsByChatId =
            new Dictionary<long, TelegramGuild>();

        public async Task<TelegramGuild> GetTelegramGuild(Chat chat)
        {
            var chatId = chat.Id;
            if (!TelegramGuildsByChatId.TryGetValue(chatId, out var guild))
            {
                var administrators = await Client.GetChatAdministratorsAsync(new ChatId(chatId));
                TelegramGuildsByChatId[chatId] = guild = new TelegramGuild(this, chat, administrators
                    .Select(a => (ulong)a.User.Id)
                    .Concat(new[] { (ulong)Client.BotId }).ToHashSet());
            }
            return guild;
        }

        public TelegramWrapper(TelegramOptions options, TelegramBotClient client)
        {
            Options = options;
            Client = client;
            CurrentUser = new TelegramSelfUser(client.BotId);
        }

        public TelegramOptions Options { get; }

        public TelegramBotClient Client { get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task StartAsync()
        {
            throw new NotImplementedException();
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IApplication> GetApplicationInfoAsync(RequestOptions options = null)
        {
            var application = new TelegramApplication(Options.BotOwnerId);
            return Task.FromResult((IApplication) application);
        }

        public async Task<IChannel> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            var chat = await Client.GetChatAsync(new ChatId((long) id));
            var guild = await GetTelegramGuild(chat);
            return guild;
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
        public ISelfUser CurrentUser { get; }
        public TokenType TokenType => TokenType.Bot;
    }
}