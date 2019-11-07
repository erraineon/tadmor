using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Services.Abstractions;
using Tadmor.Services.Commands;
using Tadmor.Services.Options;
using Tadmor.Utils;

namespace Tadmor.Services.Discord
{
    [ScopedService]
    public class ChatEventService : IMessageListener, IJoinListener
    {
        private readonly ChatOptionsService _chatOptions;
        private readonly CommandsService _commands;

        public ChatEventService(CommandsService commands, ChatOptionsService chatOptions)
        {
            _commands = commands;
            _chatOptions = chatOptions;
        }

        public async Task OnUserJoinedAsync(IDiscordClient client, IGuildUser user)
        {
            await ProcessEvent(null, null, user.Guild.Id, user, default, GuildEventTriggerType.GuildJoin, client);
        }

        public async Task OnMessageReceivedAsync(IDiscordClient client, IMessage message)
        {
            if (message.Channel is IGuildChannel guildChannel)
                await ProcessEvent(message.Id, guildChannel.Id, guildChannel.Guild.Id, (IGuildUser) message.Author,
                    message.Content, GuildEventTriggerType.RegexMatch, client);
        }

        private async Task ProcessEvent(ulong? messageId, ulong? inputChannelId, ulong guildId, IGuildUser author,
            string? input, GuildEventTriggerType triggerType, IDiscordClient client)
        {
            if (author.Id != client.CurrentUser.Id)
            {
                using var writableOptions = _chatOptions.GetOptions();
                var guildOptions = _chatOptions.GetGuildOptions(guildId, writableOptions.Value);
                if (guildOptions != null)
                {
                    var events = GetEvents(inputChannelId, triggerType, author, input, guildOptions);
                    foreach (var guildEvent in events)
                        await ProcessEvent(messageId, inputChannelId, author, input, guildEvent, client);
                }
            }
        }

        private static IEnumerable<GuildEvent> GetEvents(ulong? channelId, GuildEventTriggerType triggerType,
            IGuildUser author, string input, GuildOptions guildOptions)
        {
            var events = triggerType switch
            {
                GuildEventTriggerType.GuildJoin => guildOptions.Events.Where(e =>
                    e.TriggerType == GuildEventTriggerType.GuildJoin),
                GuildEventTriggerType.RegexMatch => guildOptions.Events.Where(e =>
                    e.TriggerType == GuildEventTriggerType.RegexMatch &&
                    (e.SenderIdFilter == null || e.SenderIdFilter == author.Id) &&
                    (e.Scope == GuildEventScope.Guild || e.ChannelId == channelId) && Regex.IsMatch(input, e.Trigger,
                        RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100))),
                _ => throw new ArgumentOutOfRangeException(nameof(triggerType), triggerType, null)
            };

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

        public void AddInputEvent(ICommandContext context, IGuildUser? sender, string reaction, string trigger,
            bool deleteTrigger)
        {
            AddEvent(context, sender, reaction, trigger, deleteTrigger, GuildEventTriggerType.RegexMatch);
        }

        public void AddJoinEvent(ICommandContext context, string reaction)
        {
            AddEvent(context, default, reaction, default, false, GuildEventTriggerType.GuildJoin);
        }

        private void AddEvent(ICommandContext context, IGuildUser? sender, string reaction, string? trigger,
            bool deleteTrigger, GuildEventTriggerType triggerType)
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
                TriggerType = triggerType,
                DeleteTrigger = deleteTrigger,
                SenderIdFilter = sender.Id,
            });
        }

        public bool TryRemoveEvent(ulong guildId, string eventId)
        {
            using var writableOptions = _chatOptions.GetOptions();
            var guildOptions = _chatOptions.GetGuildOptions(guildId, writableOptions.Value);
            var eventExists = guildOptions.Events.RemoveAll(e => e.Id == eventId) > 0;
            return eventExists;
        }
    }
}