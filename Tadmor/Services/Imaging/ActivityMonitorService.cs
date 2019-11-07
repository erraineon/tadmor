using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Tadmor.Services.Abstractions;

namespace Tadmor.Services.Imaging
{
    [SingletonService]
    public class ActivityMonitorService : IMessageListener
    {
        private static readonly TimeSpan CutoffTime = TimeSpan.FromDays(3);

        private readonly ConcurrentDictionary<(ulong guildId, ulong userId), IUserMessage> _cachedUserIds =
            new ConcurrentDictionary<(ulong guildId, ulong userId), IUserMessage>();

        public Task OnMessageReceivedAsync(IDiscordClient client, IMessage message)
        {
            if (message.Channel is IGuildChannel channel &&
                message is IUserMessage userMessage &&
                !userMessage.Author.IsWebhook)
                _cachedUserIds[(channel.Guild.Id, userMessage.Author.Id)] = userMessage;
            return Task.CompletedTask;
        }

        public async IAsyncEnumerable<IGuildUser> GetActiveUsers(IGuild guild)
        {
            var inactiveKeys = _cachedUserIds
                .Where(p => p.Value.Timestamp.DateTime < DateTime.Now - CutoffTime)
                .Select(p => p.Key);
            foreach (var inactiveUser in inactiveKeys) _cachedUserIds.TryRemove(inactiveUser, out _);
            var cachedUsers = _cachedUserIds
                .Where(p => p.Key.guildId == guild.Id)
                .OrderByDescending(p => p.Value.Timestamp)
                .ToAsyncEnumerable()
                .SelectAwait(async p => (p.Key.userId, user: await guild.GetUserAsync(p.Key.userId)));

            await foreach (var (userId, user) in cachedUsers)
            {
                // user may no longer be in the guild: remove them
                if (user == null) _cachedUserIds.TryRemove((guild.Id, userId), out _);
                else yield return user;
            }
        }

        public Task<IUserMessage?> GetLastMessageAsync(IGuildUser user)
        {
            _cachedUserIds.TryGetValue((user.GuildId, user.Id), out var message);
            return Task.FromResult(message);
        }
    }
}