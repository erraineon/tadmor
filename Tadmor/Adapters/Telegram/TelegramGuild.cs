using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace Tadmor.Adapters.Telegram
{
    public class TelegramGuild : IGuild, ITextChannel
    {
        private readonly HashSet<ulong> _administratorIds;
        private readonly TelegramBotClient _api;
        private readonly Chat _chat;
        private readonly FixedSizedQueue<IMessage> _messageCache;
        private readonly TelegramClient _telegram;
        private readonly IDictionary<ulong, TelegramGuildUser> _usersCache = new Dictionary<ulong, TelegramGuildUser>();

        public TelegramGuild(TelegramClient telegram, TelegramBotClient api, Chat chat, HashSet<ulong> administratorIds)
        {
            _telegram = telegram;
            _api = api;
            _chat = chat;
            _administratorIds = administratorIds;
            _messageCache = new FixedSizedQueue<IMessage>(telegram.Configuration.MessageCacheSize);
        }

        public Task DeleteAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public ulong Id => (ulong) _chat.Id;

        public Task ModifyAsync(Action<GuildProperties> func, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task ModifyEmbedAsync(Action<GuildEmbedProperties> func, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task ReorderChannelsAsync(IEnumerable<ReorderChannelProperties> args, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task ReorderRolesAsync(IEnumerable<ReorderRoleProperties> args, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public async Task LeaveAsync(RequestOptions? options = null)
        {
            await _api.LeaveChatAsync(_chat.Id, options?.CancelToken ?? default);
        }

        public Task<IReadOnlyCollection<IBan>> GetBansAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IBan> GetBanAsync(IUser user, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IBan> GetBanAsync(ulong userId, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task AddBanAsync(IUser user, int pruneDays = 0, string? reason = null, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task AddBanAsync(ulong userId, int pruneDays = 0, string? reason = null, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveBanAsync(IUser user, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveBanAsync(ulong userId, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IGuildChannel>> GetChannelsAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IGuildChannel> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<ITextChannel>> GetTextChannelsAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            return Task.FromResult((IReadOnlyCollection<ITextChannel>) new ITextChannel[] {this});
        }

        public Task<ITextChannel> GetTextChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            return Task.FromResult((ITextChannel) this);
        }

        public Task<IReadOnlyCollection<IVoiceChannel>> GetVoiceChannelsAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<ICategoryChannel>> GetCategoriesAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IVoiceChannel> GetVoiceChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IVoiceChannel> GetAFKChannelAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<ITextChannel> GetSystemChannelAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<ITextChannel> GetDefaultChannelAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IGuildChannel> GetEmbedChannelAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<ITextChannel> CreateTextChannelAsync(string name, Action<TextChannelProperties>? func = null,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IVoiceChannel> CreateVoiceChannelAsync(string name, Action<VoiceChannelProperties>? func = null,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<ICategoryChannel> CreateCategoryAsync(string name, Action<GuildChannelProperties>? func = null,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IVoiceRegion>> GetVoiceRegionsAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IGuildIntegration>> GetIntegrationsAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IGuildIntegration> CreateIntegrationAsync(ulong id, string type, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IInviteMetadata>> GetInvitesAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IInviteMetadata> GetVanityInviteAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public IRole GetRole(ulong id)
        {
            throw new NotImplementedException();
        }

        public Task<IRole> CreateRoleAsync(string name, GuildPermissions? permissions = null, Color? color = null,
            bool isHoisted = false,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IGuildUser> AddGuildUserAsync(ulong userId, string accessToken,
            Action<AddGuildUserProperties>? func = null, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IGuildUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            return Task.FromResult((IReadOnlyCollection<IGuildUser>) new IGuildUser[0]);
        }

        public async Task<IGuildUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            var chatMember = await _api.GetChatMemberAsync(new ChatId(_chat.Id), (int)id,
                options?.CancelToken ?? default);
            return new TelegramGuildUser(_telegram, this, chatMember.User);
        }

        public async Task<IGuildUser> GetCurrentUserAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            var me = await _api.GetMeAsync(options?.CancelToken ?? default);
            return new TelegramGuildUser(_telegram, this, me);
        }

        public Task<IGuildUser> GetOwnerAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task DownloadUsersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> PruneUsersAsync(int days = 30, bool simulate = false, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IAuditLogEntry>> GetAuditLogsAsync(int limit = 100,
            CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null,
            ulong? beforeId = null, ulong? userId = null, ActionType? actionType = null)
        {
            throw new NotImplementedException();
        }

        public Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IWebhook>> GetWebhooksAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<GuildEmote> GetEmoteAsync(ulong id, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<GuildEmote> CreateEmoteAsync(string name, Image image,
            Optional<IEnumerable<IRole>> roles = new Optional<IEnumerable<IRole>>(), RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<GuildEmote> ModifyEmoteAsync(GuildEmote emote, Action<EmoteProperties> func,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task DeleteEmoteAsync(GuildEmote emote, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public string Name => _chat.Title;

        public int AFKTimeout => throw new NotImplementedException();

        public bool IsEmbeddable => throw new NotImplementedException();

        public DefaultMessageNotifications DefaultMessageNotifications => throw new NotImplementedException();

        public MfaLevel MfaLevel => throw new NotImplementedException();

        public VerificationLevel VerificationLevel => throw new NotImplementedException();

        public ExplicitContentFilterLevel ExplicitContentFilter => throw new NotImplementedException();

        public string IconId => throw new NotImplementedException();

        public string IconUrl => throw new NotImplementedException();

        public string SplashId => throw new NotImplementedException();

        public string SplashUrl => throw new NotImplementedException();

        public bool Available => throw new NotImplementedException();

        public ulong? AFKChannelId => throw new NotImplementedException();

        public ulong DefaultChannelId => throw new NotImplementedException();

        public ulong? EmbedChannelId => throw new NotImplementedException();

        public ulong? SystemChannelId => throw new NotImplementedException();

        public ulong OwnerId => throw new NotImplementedException();

        public ulong? ApplicationId => throw new NotImplementedException();

        public string VoiceRegionId => throw new NotImplementedException();

        public IAudioClient AudioClient => throw new NotImplementedException();

        public IRole EveryoneRole => throw new NotImplementedException();

        public IReadOnlyCollection<GuildEmote> Emotes => throw new NotImplementedException();

        public IReadOnlyCollection<string> Features => throw new NotImplementedException();

        public IReadOnlyCollection<IRole> Roles => throw new NotImplementedException();

        public PremiumTier PremiumTier => throw new NotImplementedException();

        public string BannerId => throw new NotImplementedException();

        public string BannerUrl => throw new NotImplementedException();

        public string VanityURLCode => throw new NotImplementedException();

        public SystemChannelMessageDeny SystemChannelFlags => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public int PremiumSubscriptionCount => throw new NotImplementedException();

        public string PreferredLocale => throw new NotImplementedException();

        public CultureInfo PreferredCulture => throw new NotImplementedException();

        public DateTimeOffset CreatedAt => throw new NotImplementedException();

        public int Position => throw new NotImplementedException();

        public Task<ICategoryChannel> GetCategoryAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task SyncPermissionsAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IInviteMetadata> CreateInviteAsync(int? maxAge, int? maxUses = null, bool isTemporary = false,
            bool isUnique = false,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public ulong? CategoryId => throw new NotImplementedException();

        public Task ModifyAsync(Action<GuildChannelProperties> func, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public OverwritePermissions? GetPermissionOverwrite(IRole role)
        {
            throw new NotImplementedException();
        }

        public OverwritePermissions? GetPermissionOverwrite(IUser user)
        {
            throw new NotImplementedException();
        }

        public Task RemovePermissionOverwriteAsync(IRole role, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemovePermissionOverwriteAsync(IUser user, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task AddPermissionOverwriteAsync(IRole role, OverwritePermissions permissions,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task AddPermissionOverwriteAsync(IUser user, OverwritePermissions permissions,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> IGuildChannel.GetUsersAsync(CacheMode mode,
            RequestOptions? options)
        {
            throw new NotImplementedException();
        }

        async Task<IUser> IChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions? options)
        {
            return await GetUserAsync(id, mode, options);
        }

        public IGuild Guild => this;
        public ulong GuildId => Id;

        public IReadOnlyCollection<Overwrite> PermissionOverwrites => throw new NotImplementedException();

        IAsyncEnumerable<IReadOnlyCollection<IUser>> IChannel.GetUsersAsync(CacheMode mode, RequestOptions? options)
        {
            return ToAsyncEnumerableReadOnlyCollection(_usersCache.Values.Cast<IUser>());
        }

        public Task DeleteMessagesAsync(IEnumerable<IMessage> messages, RequestOptions? options = null)
        {
            return DeleteMessagesAsync(messages.Select(message => message.Id), options);
        }

        public async Task DeleteMessagesAsync(IEnumerable<ulong> messageIds, RequestOptions? options = null)
        {
            foreach (var messageId in messageIds) await DeleteMessageAsync(messageId);
        }

        public Task ModifyAsync(Action<TextChannelProperties> func, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IWebhook> CreateWebhookAsync(string name, Stream? avatar = null, RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public bool IsNsfw => false;

        public string Topic => throw new NotImplementedException();

        public int SlowModeInterval => throw new NotImplementedException();

        public async Task<IUserMessage> SendMessageAsync(string? text = null, bool isTts = false, Embed? embed = null,
            RequestOptions? options = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                if (embed == null) throw new Exception("either text or embed must be not null");
                text = ToText(embed);
            }

            // escape telegram markdown tokens out of urls
            text = Regex.Replace(text, @"http.+?(?=\s|$)", m => Regex.Replace(m.Value, @"[*_~]", @"\$&"));
            // replace regular markdown with the kind that telegram expects
            text = Regex.Replace(text, @"([*_])\1(.*?)\1\1", @"$1$2$1");
            var message = await _api.SendTextMessageAsync(_chat.Id, text, ParseMode.Markdown);
            return ProcessInboundMessage(message);
        }

        private static string ToText(IEmbed embed)
        {
            var builder = new StringBuilder();
            if (embed.Image != null) builder.AppendLine(embed.Image.Value.Url);
            if (embed.Author != null) builder.AppendLine($"**{embed.Author.Value.Name.TrimEnd('#')}**");
            if (embed.Title is { } title)
            {
                if (embed.Url is { } url) title = $"[{title}]({url})";
                builder.AppendLine(title);
            }
            builder.AppendLine(embed.Description);
            foreach (var embedField in embed.Fields)
            {
                builder.AppendLine($"**{embedField.Name}**");
                builder.AppendLine(embedField.Value);
            }

            return builder.ToString();
        }

        public Task<IUserMessage> SendFileAsync(string filePath, string? text = null, bool isTts = false,
            Embed? embed = null,
            RequestOptions? options = null, bool isSpoiler = false)
        {
            throw new NotImplementedException();
        }

        public async Task<IUserMessage> SendFileAsync(Stream stream, string filename, string? text = null,
            bool isTts = false, Embed? embed = null,
            RequestOptions? options = null, bool isSpoiler = false)
        {
            var videoExtensions = new[] {".gif", ".mp4"};
            var message = videoExtensions.Any(filename.EndsWith)
                ? await _api.SendVideoAsync(_chat.Id, new InputOnlineFile(stream), caption: text)
                : await _api.SendPhotoAsync(_chat.Id, new InputOnlineFile(stream), text);
            return ProcessInboundMessage(message);
        }

        public Task<IMessage> GetMessageAsync(ulong id, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100,
            CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
        {
            return ToAsyncEnumerableReadOnlyCollection(_messageCache
                .Reverse()
                .Take(limit));
        }

        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir,
            int limit = 100, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            return ToAsyncEnumerableReadOnlyCollection(_messageCache
                .Reverse()
                .SkipWhile(m => m.Id != fromMessageId)
                .Take(limit));
        }

        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir,
            int limit = 100, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IMessage>> GetPinnedMessagesAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task DeleteMessageAsync(ulong messageId, RequestOptions? options = null)
        {
            return _api.DeleteMessageAsync(_chat.Id, (int) messageId, options?.CancelToken ?? default);
        }

        public Task DeleteMessageAsync(IMessage message, RequestOptions? options = null)
        {
            return DeleteMessageAsync(message.Id, options);
        }

        public Task TriggerTypingAsync(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public IDisposable EnterTypingState(RequestOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public string Mention => _chat.Title;

        private static IAsyncEnumerable<IReadOnlyCollection<T>> ToAsyncEnumerableReadOnlyCollection<T>(IEnumerable<T> values)
        {
            return new[] {new ReadOnlyCollection<T>(values.ToList())}
                .ToAsyncEnumerable();
        }

        public TelegramUserMessage ProcessInboundMessage(Message msg)
        {
            var user = new TelegramGuildUser(_telegram, this, msg.From);
            var message = new TelegramUserMessage(this, user, msg);
            _messageCache.Enqueue(message);
            _usersCache[user.Id] = user;
            return message;
        }

        public bool IsAdmin(TelegramGuildUser telegramGuildUser)
        {
            return _administratorIds.Contains(telegramGuildUser.Id);
        }
    }
}