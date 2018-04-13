using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Tadmor.Services.Imaging
{
    public class ActivityMonitorService
    {
        private static readonly TimeSpan CutoffTime = TimeSpan.FromDays(3);

        private readonly Dictionary<(ulong guildId, ulong userId), DateTime> _activeUsers =
            new Dictionary<(ulong guildId, ulong userId), DateTime>();

        private readonly DiscordSocketClient _discord;

        public ActivityMonitorService(DiscordSocketClient discord)
        {
            _discord = discord;
        }

        public async Task PopulateActivity()
        {
            var tasks = _discord.Guilds
                .SelectMany(guild => guild.TextChannels
                    .Where(channel => channel.GetUser(_discord.CurrentUser.Id) != null)
                    .Select(channel => channel.GetMessagesAsync()
                        .Flatten()
                        .Where(message => !message.Author.IsWebhook && guild.GetUser(message.Author.Id) != null)
                        .Select(message => (guildId: guild.Id, userId: message.Author.Id, message.Timestamp.DateTime))
                        .ToList()));
            var userActivityTuples = (await Task.WhenAll(tasks))
                .SelectMany(tuples => tuples)
                    .GroupBy(u => (u.guildId, u.userId))
                .Select(g => (g.Key.guildId, g.Key.userId, g.Max(t => t.DateTime)))
                .ToList();
            foreach (var (guildId, userId, dateTime) in userActivityTuples)
                _activeUsers[(guildId, userId)] = dateTime;
        }

        public Task UpdateUserActivity(SocketMessage socketMessage)
        {
            if (socketMessage.Channel is IGuildChannel channel &&
                socketMessage is SocketUserMessage message &&
                !message.Author.IsWebhook)
                _activeUsers[(channel.Guild.Id, message.Author.Id)] = DateTime.Now;
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<IGuildUser>> GetActiveUsers(IGuild guild)
        {
            var inactiveKeys = _activeUsers
                .Where(p => p.Value < DateTime.Now - CutoffTime)
                .Select(p => p.Key)
                .ToList();
            foreach (var inactiveUser in inactiveKeys) _activeUsers.Remove(inactiveUser);
            var activeUsers = await Task.WhenAll(_activeUsers
                .Where(p => p.Key.guildId == guild.Id)
                .OrderByDescending(p => p.Value)
                .Select(async p => (p.Key.userId, user: await guild.GetUserAsync(p.Key.userId))));
            var missingUsers = activeUsers.Where(t => t.user == null);
            foreach (var (missingUserId, _) in missingUsers)
                _activeUsers.Remove((guild.Id, missingUserId));
            return activeUsers
                .Select(t => t.user);
        }
    }
}