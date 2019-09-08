using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using MoreLinq;
using Tadmor.Services.Telegram;

namespace Tadmor.Services.Imaging
{
    public class ActivityMonitorService : IHostedService
    {
        private static readonly TimeSpan CutoffTime = TimeSpan.FromDays(3);

        private readonly ConcurrentDictionary<(ulong guildId, ulong userId), IMessage> _activeUsers =
            new ConcurrentDictionary<(ulong guildId, ulong userId), IMessage>();

        private readonly DiscordSocketClient _discord;
        private readonly TelegramService _telegram;

        public ActivityMonitorService(DiscordSocketClient discord, TelegramService telegram)
        {
            _discord = discord;
            _telegram = telegram;
        }

        public async Task PopulateActivity()
        {
            var tasks = _discord.Guilds
                .SelectMany(guild => guild.TextChannels
                    .Where(channel => channel.GetUser(_discord.CurrentUser.Id) != null)
                    .Select(channel => channel.GetMessagesAsync()
                        .Flatten()
                        .Where(message => !message.Author.IsWebhook && guild.GetUser(message.Author.Id) != null)
                        .Select(message => (guildId: guild.Id, message))
                        .ToList()));
            var userActivityTuples = (await Task.WhenAll(tasks))
                .SelectMany(tuples => tuples)
                .OrderByDescending(t => t.message.Timestamp.DateTime)
                .DistinctBy(u => (u.guildId, u.message.Author.Id))
                .ToList();
            foreach (var (guildId, message) in userActivityTuples)
                _activeUsers[(guildId, message.Author.Id)] = message;
        }

        public Task UpdateUserActivity(SocketMessage socketMessage)
        {
            return UpdateUserActivity((IMessage) socketMessage);
        }

        public async Task<IEnumerable<IGuildUser>> GetActiveUsers(IGuild guild)
        {
            var inactiveKeys = _activeUsers
                .Where(p => p.Value.Timestamp.DateTime < DateTime.Now - CutoffTime)
                .Select(p => p.Key)
                .ToList();
            foreach (var inactiveUser in inactiveKeys) _activeUsers.TryRemove(inactiveUser, out _);
            var activeUsers = await Task.WhenAll(_activeUsers
                .Where(p => p.Key.guildId == guild.Id)
                .OrderByDescending(p => p.Value.Timestamp)
                .Select(async p => (p.Key.userId, user: await guild.GetUserAsync(p.Key.userId))));
            var missingUsers = activeUsers.Where(t => t.user == null).ToList();
            foreach (var (missingUserId, _) in missingUsers)
                _activeUsers.TryRemove((guild.Id, missingUserId), out _);
            return activeUsers
                .Except(missingUsers)
                .Select(t => t.user);
        }

        public async Task<IMessage> GetLastMessage(IGuildUser user)
        {
            _activeUsers.TryGetValue((user.GuildId, user.Id), out var message);
            return message;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _discord.MessageReceived += UpdateUserActivity;
            _telegram.MessageReceived += UpdateUserActivity;
            return Task.CompletedTask;
        }

        private async Task UpdateUserActivity(IMessage message)
        {
            if (message.Channel is IGuildChannel channel &&
                message is IUserMessage userMessage &&
                !userMessage.Author.IsWebhook)
                _activeUsers[(channel.Guild.Id, userMessage.Author.Id)] = message;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _discord.MessageReceived -= UpdateUserActivity;
            return Task.CompletedTask;
        }
    }
}