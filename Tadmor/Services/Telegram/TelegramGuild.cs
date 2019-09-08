using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Tadmor.Utils;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace Tadmor.Services.Telegram
{
    public class TelegramGuild : IGuild, ITextChannel
    {
        private readonly Chat _chat;
        private readonly TelegramWrapper _telegram;

        public TelegramGuild(TelegramWrapper telegram, Chat chat, HashSet<ulong> administratorIds)
        {
            _telegram = telegram;
            _chat = chat;
            AdministratorIds = administratorIds;
            MessageCache = new FixedSizedQueue<IMessage>(telegram.Options.CacheSize);
        }

        public FixedSizedQueue<IMessage> MessageCache { get; set; }

        public HashSet<ulong> AdministratorIds { get; set; }
        public HashSet<TelegramGuildUser> UsersCache { get; } = new HashSet<TelegramGuildUser>();

        public Task DeleteAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public ulong Id => (ulong) _chat.Id;
        public DateTimeOffset CreatedAt { get; }

        public Task ModifyAsync(Action<GuildProperties> func, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task ModifyEmbedAsync(Action<GuildEmbedProperties> func, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task ReorderChannelsAsync(IEnumerable<ReorderChannelProperties> args, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task ReorderRolesAsync(IEnumerable<ReorderRoleProperties> args, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task LeaveAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IBan>> GetBansAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IBan> GetBanAsync(IUser user, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IBan> GetBanAsync(ulong userId, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task AddBanAsync(IUser user, int pruneDays = 0, string reason = null, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task AddBanAsync(ulong userId, int pruneDays = 0, string reason = null, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveBanAsync(IUser user, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveBanAsync(ulong userId, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IGuildChannel>> GetChannelsAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IGuildChannel> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<ITextChannel>> GetTextChannelsAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            return Task.FromResult((IReadOnlyCollection<ITextChannel>) new ITextChannel[] {this});
        }

        public Task<ITextChannel> GetTextChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            return Task.FromResult((ITextChannel) this);
        }

        public Task<IReadOnlyCollection<IVoiceChannel>> GetVoiceChannelsAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<ICategoryChannel>> GetCategoriesAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IVoiceChannel> GetVoiceChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IVoiceChannel> GetAFKChannelAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<ITextChannel> GetSystemChannelAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<ITextChannel> GetDefaultChannelAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IGuildChannel> GetEmbedChannelAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<ITextChannel> CreateTextChannelAsync(string name, Action<TextChannelProperties> func = null,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IVoiceChannel> CreateVoiceChannelAsync(string name, Action<VoiceChannelProperties> func = null,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<ICategoryChannel> CreateCategoryAsync(string name, Action<GuildChannelProperties> func = null,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IGuildIntegration>> GetIntegrationsAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IGuildIntegration> CreateIntegrationAsync(ulong id, string type, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IInviteMetadata>> GetInvitesAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IInviteMetadata> GetVanityInviteAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public IRole GetRole(ulong id)
        {
            throw new NotImplementedException();
        }

        public Task<IRole> CreateRoleAsync(string name, GuildPermissions? permissions = null, Color? color = null,
            bool isHoisted = false,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IGuildUser> AddGuildUserAsync(ulong userId, string accessToken,
            Action<AddGuildUserProperties> func = null, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IGuildUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            return Task.FromResult((IReadOnlyCollection<IGuildUser>) new IGuildUser[0]);
        }

        public async Task<IGuildUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            if (mode == CacheMode.CacheOnly) return default;
            try
            {
                var chatMember = await _telegram.Client.GetChatMemberAsync(new ChatId(_chat.Id), (int) id,
                    options?.CancelToken ?? default);
                return new TelegramGuildUser(this, chatMember.User);
            }
            catch
            {
                return default;
            }
        }

        public async Task<IGuildUser> GetCurrentUserAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            var me = await _telegram.Client.GetMeAsync(options?.CancelToken ?? default);
            return new TelegramGuildUser(this, me);
        }

        public Task<IGuildUser> GetOwnerAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task DownloadUsersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> PruneUsersAsync(int days = 30, bool simulate = false, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IAuditLogEntry>> GetAuditLogsAsync(int limit = 100,
            CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IWebhook>> GetWebhooksAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<GuildEmote> GetEmoteAsync(ulong id, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<GuildEmote> CreateEmoteAsync(string name, Image image,
            Optional<IEnumerable<IRole>> roles = new Optional<IEnumerable<IRole>>(), RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<GuildEmote> ModifyEmoteAsync(GuildEmote emote, Action<EmoteProperties> func,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task DeleteEmoteAsync(GuildEmote emote, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public string Name => _chat.Title;
        public int AFKTimeout { get; }
        public bool IsEmbeddable { get; }
        public DefaultMessageNotifications DefaultMessageNotifications { get; }
        public MfaLevel MfaLevel { get; }
        public VerificationLevel VerificationLevel { get; }
        public ExplicitContentFilterLevel ExplicitContentFilter { get; }
        public string IconId { get; }
        public string IconUrl { get; }
        public string SplashId { get; }
        public string SplashUrl { get; }
        public bool Available { get; }
        public ulong? AFKChannelId { get; }
        public ulong DefaultChannelId { get; }
        public ulong? EmbedChannelId { get; }
        public ulong? SystemChannelId { get; }
        public ulong OwnerId { get; }
        public ulong? ApplicationId { get; }
        public string VoiceRegionId { get; }
        public IAudioClient AudioClient { get; }
        public IRole EveryoneRole { get; }
        public IReadOnlyCollection<GuildEmote> Emotes { get; }
        public IReadOnlyCollection<string> Features { get; }
        public IReadOnlyCollection<IRole> Roles { get; }

        public Task<ICategoryChannel> GetCategoryAsync(CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task SyncPermissionsAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IInviteMetadata> CreateInviteAsync(int? maxAge, int? maxUses = null, bool isTemporary = false,
            bool isUnique = false,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public ulong? CategoryId { get; }

        public Task ModifyAsync(Action<GuildChannelProperties> func, RequestOptions options = null)
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

        public Task RemovePermissionOverwriteAsync(IRole role, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemovePermissionOverwriteAsync(IUser user, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task AddPermissionOverwriteAsync(IRole role, OverwritePermissions permissions,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task AddPermissionOverwriteAsync(IUser user, OverwritePermissions permissions,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> IGuildChannel.GetUsersAsync(CacheMode mode,
            RequestOptions options)
        {
            throw new NotImplementedException();
        }

        async Task<IUser> IChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
        {
            return await GetUserAsync(id, mode, options);
        }

        public int Position { get; }
        public IGuild Guild => this;
        public ulong GuildId => Id;
        public IReadOnlyCollection<Overwrite> PermissionOverwrites { get; }

        IAsyncEnumerable<IReadOnlyCollection<IUser>> IChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
        {
            return ToAsyncEnumerableReadOnlyCollection(UsersCache.Cast<IUser>());
        }

        public Task DeleteMessagesAsync(IEnumerable<IMessage> messages, RequestOptions options = null)
        {
            return DeleteMessagesAsync(messages.Select(message => message.Id), options);
        }

        public async Task DeleteMessagesAsync(IEnumerable<ulong> messageIds, RequestOptions options = null)
        {
            foreach (var messageId in messageIds) await DeleteMessageAsync(messageId);
        }

        public Task ModifyAsync(Action<TextChannelProperties> func, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IWebhook> CreateWebhookAsync(string name, Stream avatar = null, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public bool IsNsfw => true;
        public string Topic { get; }
        public int SlowModeInterval { get; }

        public async Task<IUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null,
            RequestOptions options = null)
        {
            if (embed != null) text = ToString(embed);
            if (text != null)
            {
                const string urlRegex =
                    @"[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=*]*)";
                var urls = Regex.Matches(text, urlRegex);
                foreach (Match match in urls)
                    text = text.Replace(match.Value, Regex.Replace(match.Value, "([*_])", @"\$1"));
            }

            var message = await _telegram.Client.SendTextMessageAsync(_chat.Id, text, ParseMode.Markdown);
            return ProcessInboundMessage(message);
        }

        public Task<IUserMessage> SendFileAsync(string filePath, string text = null, bool isTTS = false,
            Embed embed = null,
            RequestOptions options = null, bool isSpoiler = false)
        {
            throw new NotImplementedException();
        }

        public async Task<IUserMessage> SendFileAsync(Stream stream, string filename, string text = null,
            bool isTTS = false, Embed embed = null,
            RequestOptions options = null, bool isSpoiler = false)
        {
            var videoExtensions = new[] {".gif", ".mp4"};
            var message = videoExtensions.Any(filename.EndsWith)
                ? await _telegram.Client.SendVideoAsync(_chat.Id, new InputOnlineFile(stream), caption: text)
                : await _telegram.Client.SendPhotoAsync(_chat.Id, new InputOnlineFile(stream), text);
            return ProcessInboundMessage(message);
        }

        public Task<IMessage> GetMessageAsync(ulong id, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100,
            CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            return ToAsyncEnumerableReadOnlyCollection(MessageCache
                .Reverse()
                .Take(limit));
        }

        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir,
            int limit = 100, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            return ToAsyncEnumerableReadOnlyCollection(MessageCache
                .Reverse()
                .SkipWhile(m => m.Id != fromMessageId)
                .Take(limit));
        }

        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir,
            int limit = 100, CacheMode mode = CacheMode.AllowDownload,
            RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IMessage>> GetPinnedMessagesAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task DeleteMessageAsync(ulong messageId, RequestOptions options = null)
        {
            return _telegram.Client.DeleteMessageAsync(_chat.Id, (int) messageId, options?.CancelToken ?? default);
        }

        public Task DeleteMessageAsync(IMessage message, RequestOptions options = null)
        {
            return DeleteMessageAsync(message.Id, options);
        }

        public Task TriggerTypingAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public IDisposable EnterTypingState(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public string Mention => _chat.Title;

        private IAsyncEnumerable<IReadOnlyCollection<T>> ToAsyncEnumerableReadOnlyCollection<T>(IEnumerable<T> values)
        {
            return values
                .GroupBy(m => true)
                .Select(g => g.ToList())
                .ToAsyncEnumerable();
        }

        private string ToString(Embed embed)
        {
            var builder = new StringBuilder();
            if (embed.Image != null) builder.AppendLine(embed.Image.Value.Url);
            if (embed.Author != null) builder.AppendLine($"*{embed.Author.Value.Name.TrimEnd('#')}*");
            if (embed.Title != null) builder.AppendLine($"*{embed.Title}*");
            builder.AppendLine(embed.Description);
            foreach (var embedField in embed.Fields)
            {
                builder.AppendLine($"*{embedField.Name}*");
                builder.AppendLine(embedField.Value);
            }

            return builder.ToString();
        }

        public TelegramUserMessage ProcessInboundMessage(Message msg)
        {
            var user = new TelegramGuildUser(this, msg.From);
            var message = new TelegramUserMessage(this, user, msg);
            MessageCache.Enqueue(message);
            UsersCache.Add(user);
            return message;
        }
    }
}