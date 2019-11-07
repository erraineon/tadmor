using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Services.Abstractions;
using Tadmor.Services.Commands;
using Tadmor.Services.Imaging;
using Tadmor.Services.Options;
using Tadmor.Utils;

namespace Tadmor.Services.Discord
{
    [ScopedService]
    public class ChatEventService : IMessageListener, IJoinListener
    {
        private readonly ActivityMonitorService _activityMonitor;
        private readonly ChatOptionsService _chatOptions;
        private readonly CommandsService _commands;

        public ChatEventService(CommandsService commands, ChatOptionsService chatOptions,
            ActivityMonitorService activityMonitor)
        {
            _commands = commands;
            _chatOptions = chatOptions;
            _activityMonitor = activityMonitor;
        }

        public async Task OnUserJoinedAsync(IDiscordClient client, IGuildUser user)
        {
            await ProcessEvent(null, null, user.Guild.Id, user, default, GuildEventType.Join, client);
        }

        public async Task OnMessageReceivedAsync(IDiscordClient client, IMessage message)
        {
            if (message.Channel is IGuildChannel guildChannel)
                await ProcessEvent(message.Id, guildChannel.Id, guildChannel.Guild.Id, (IGuildUser) message.Author,
                    message.Content, GuildEventType.Message, client);
        }

        private async Task ProcessEvent(ulong? messageId, ulong? inputChannelId, ulong guildId, IGuildUser author,
            string? input, GuildEventType eventType, IDiscordClient client)
        {
            if (author.Id != client.CurrentUser.Id)
            {
                using var writableOptions = _chatOptions.GetOptions();
                var guildOptions = _chatOptions.GetGuildOptions(guildId, writableOptions.Value);
                if (guildOptions != null)
                {
                    var events = GetEvents(inputChannelId, eventType, author, input, guildOptions);
                    foreach (var guildEvent in events)
                        await ProcessEvent(messageId, inputChannelId, author, input, guildEvent, client);
                }
            }
        }

        private IEnumerable<GuildEvent> GetEvents(ulong? channelId, GuildEventType eventType,
            IGuildUser author, string? input, GuildOptions guildOptions)
        {
            bool MatchesRegex(GuildEvent e)
            {
                return (e.SenderIdFilter == null || e.SenderIdFilter == author.Id) &&
                       (e.Scope == GuildEventScope.Guild || e.ChannelId == channelId) &&
                       Regex.IsMatch(input, e.Trigger, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
            }

            bool MatchesEverySoOften(GuildEvent e)
            {
                return channelId.HasValue &&
                       _activityMonitor.GetMessagesCount(channelId.Value) % e.MessagesCount == 0;
            }

            var events = guildOptions.Events
                .Where(e => (e.TriggerType, eventType) switch
                {
                    (GuildEventTriggerType.GuildJoin, GuildEventType.Join) => true,
                    (GuildEventTriggerType.GuildJoin, _) => false,
                    (GuildEventTriggerType.RegexMatch, GuildEventType.Message) => MatchesRegex(e),
                    (GuildEventTriggerType.EverySoOften, GuildEventType.Message) => MatchesEverySoOften(e),
                    _ => throw new ArgumentOutOfRangeException(nameof(e.TriggerType), e.TriggerType, null)
                });

            return events;
        }

        private async Task ProcessEvent(ulong? messageId, ulong? inputChannelId, IGuildUser author, string? input,
            GuildEvent guildEvent, IDiscordClient client)
        {
            var responseChannel = await author.Guild.GetTextChannelAsync(inputChannelId ?? guildEvent.ChannelId);
            var variablesByName = new Dictionary<string, string?>
            {
                ["{{user}}"] = author.Mention,
                ["{{time}}"] = DateTime.Now.ToString("g"),
                ["{{random}}"] = new Random().Next(10000).ToString(),
                ["{{input}}"] = input
            };
            if (guildEvent.TriggerType == GuildEventTriggerType.RegexMatch)
            {
                var match = Regex.Match(input, guildEvent.Trigger, RegexOptions.IgnoreCase);
                for (var i = 1; i < match.Groups.Count; i++) variablesByName[$"${i}"] = match.Groups[i].Value;
            }

            var reaction = variablesByName
                .Aggregate(guildEvent.Reaction ?? throw new Exception("the event's reaction must be not null"),
                    (output, variable) => output.Replace(variable.Key, variable.Value));
            var message = new ServiceUserMessage(responseChannel, author, reaction);
            var context = new CommandContext(client, message);
            await _commands.ExecuteCommand(context, string.Empty);
            if (guildEvent.DeleteTrigger && messageId.HasValue && !author.GuildPermissions.Administrator)
                await responseChannel.DeleteMessageAsync((ulong) messageId);
        }

        public List<string> GetEventInfos(IGuild guild)
        {
            using var writableOptions = _chatOptions.GetOptions();
            var guildOptions = _chatOptions.GetGuildOptions(guild.Id, writableOptions.Value);
            return guildOptions != null ? guildOptions.Events.Select(e => e.ToString()).ToList() : new List<string>();
        }

        public void AddEveryEvent(ICommandContext context, string reaction, int messagesCount)
        {
            using var writableOptions = _chatOptions.GetOptions();
            var guildOptions = _chatOptions.GetGuildOptions(context.Guild.Id, writableOptions.Value);
            guildOptions.Events.Add(new GuildEvent
            {
                Id = Guid.NewGuid().ToString(),
                ChannelId = context.Channel.Id,
                Scope = GuildEventScope.Guild,
                Reaction = reaction,
                Trigger = default,
                TriggerType = GuildEventTriggerType.EverySoOften,
                DeleteTrigger = false,
                MessagesCount = messagesCount,
                SenderIdFilter = default
            });
        }

        public void AddInputEvent(ICommandContext context, IGuildUser? sender, string reaction, string trigger,
            bool deleteTrigger)
        {
            using var writableOptions = _chatOptions.GetOptions();
            var guildOptions = _chatOptions.GetGuildOptions(context.Guild.Id, writableOptions.Value);
            guildOptions.Events.Add(new GuildEvent
            {
                Id = Guid.NewGuid().ToString(),
                ChannelId = context.Channel.Id,
                Scope = GuildEventScope.Guild,
                Reaction = reaction,
                Trigger = trigger,
                TriggerType = GuildEventTriggerType.RegexMatch,
                DeleteTrigger = deleteTrigger,
                MessagesCount = default,
                SenderIdFilter = sender?.Id
            });
        }

        public void AddJoinEvent(ICommandContext context, string reaction)
        {
            using var writableOptions = _chatOptions.GetOptions();
            var guildOptions = _chatOptions.GetGuildOptions(context.Guild.Id, writableOptions.Value);
            guildOptions.Events.Add(new GuildEvent
            {
                Id = Guid.NewGuid().ToString(),
                ChannelId = context.Channel.Id,
                Scope = GuildEventScope.Guild,
                Reaction = reaction,
                Trigger = default,
                TriggerType = GuildEventTriggerType.GuildJoin,
                DeleteTrigger = false,
                MessagesCount = default,
                SenderIdFilter = default
            });
        }

        public bool TryRemoveEvent(ulong guildId, string eventId)
        {
            using var writableOptions = _chatOptions.GetOptions();
            var guildOptions = _chatOptions.GetGuildOptions(guildId, writableOptions.Value);
            var eventExists = guildOptions.Events.RemoveAll(e => e.Id == eventId) > 0;
            return eventExists;
        }

        private enum GuildEventType
        {
            Join,
            Message
        }
    }
}