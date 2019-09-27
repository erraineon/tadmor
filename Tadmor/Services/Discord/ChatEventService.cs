using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Tadmor.Services.Commands;
using Tadmor.Services.Telegram;
using Tadmor.Utils;

namespace Tadmor.Services.Discord
{
    public class ChatEventService : IHostedService
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly ChatCommandsService _commands;
        private readonly TelegramService _telegramService;
        private readonly IServiceProvider _services;

        public ChatEventService(DiscordSocketClient discordClient, ChatCommandsService commands, TelegramService telegramService, IServiceProvider services)
        {
            _discordClient = discordClient;
            _commands = commands;
            _telegramService = telegramService;
            _services = services;
        }

        private async Task ProcessJoin(SocketGuildUser arg)
        {
            await ProcessEvent(null, null, arg.Guild.Id, arg, default, GuildEventTriggerType.GuildJoin, _discordClient);
        }

        private Task ProcessMessage(SocketMessage msg)
        {
            return ProcessMessage((IMessage) msg);
        }

        private async Task ProcessMessage(IMessage msg)
        {
            if (msg.Channel is IGuildChannel guildChannel)
            {
                var client = msg.Channel is TelegramGuild ? (IDiscordClient)_telegramService.Wrapper : _discordClient;
                await ProcessEvent(msg.Id, guildChannel.Id, guildChannel.Guild.Id, (IGuildUser) msg.Author, msg.Content, GuildEventTriggerType.RegexMatch, client);
            }
        }

        private async Task ProcessEvent(ulong? messageId, ulong? inputChannelId, ulong guildId, IGuildUser author,
            string input, GuildEventTriggerType triggerType, IDiscordClient client)
        {
            if (author.Id != client.CurrentUser.Id)
            {
                var guildOptions = GetGuildOptions(guildId);
                if (guildOptions != null)
                {
                    var events = GetEvents(inputChannelId, triggerType, input, guildOptions);
                    foreach (var guildEvent in events)
                        await ProcessEvent(messageId, inputChannelId, author, input, guildEvent, client);
                }
            }
        }

        private GuildOptions GetGuildOptions(ulong guildId)
        {
            GuildOptions guildOptions;
            using (var scope = _services.CreateScope())
            {
                var discordOptions = scope.ServiceProvider.GetService<IOptionsSnapshot<DiscordOptions>>().Value;
                guildOptions = discordOptions.GuildOptions.SingleOrDefault(options => options.Id == guildId);
            }

            return guildOptions;
        }

        private static IEnumerable<GuildEvent> GetEvents(ulong? channelId, GuildEventTriggerType triggerType,
            string input, GuildOptions guildOptions)
        {
            IEnumerable<GuildEvent> events;
            switch (triggerType)
            {
                case GuildEventTriggerType.GuildJoin:
                    events = guildOptions.Events
                        .Where(e => e.TriggerType == GuildEventTriggerType.GuildJoin);
                    break;
                case GuildEventTriggerType.RegexMatch:
                    events = guildOptions.Events
                        .Where(e => e.TriggerType == GuildEventTriggerType.RegexMatch &&
                                    (e.Scope == GuildEventScope.Guild || e.ChannelId == channelId) &&
                                    Regex.IsMatch(input, e.Trigger, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(triggerType), triggerType, null);
            }

            return events;
        }

        private async Task ProcessEvent(ulong? messageId, ulong? inputChannelId, IGuildUser author, string input,
            GuildEvent guildEvent, IDiscordClient client)
        {
            var responseChannel = await author.Guild.GetTextChannelAsync(inputChannelId ?? guildEvent.ChannelId);
            var variablesByName = new Dictionary<string, string>
            {
                ["{{user}}"] = author.Mention,
                ["{{time}}"] = DateTime.Now.ToString("g"),
                ["{{random}}"] = new Random().Next(10000).ToString(),
                ["{{input}}"] = input
            };
            if (guildEvent.TriggerType == GuildEventTriggerType.RegexMatch)
            {
                var match = Regex.Match(input, guildEvent.Trigger, RegexOptions.IgnoreCase);
                for (var i = 1; i < match.Groups.Count; i++)
                {
                    variablesByName[$"${i}"] = match.Groups[i].Value;
                }
            }
            var reaction = variablesByName
                .Aggregate(guildEvent.Reaction,
                    (output, variable) => output.Replace(variable.Key, variable.Value));
            var message = new ServiceUserMessage(responseChannel, author, reaction);
            var context = new CommandContext(client, message);
            await _commands.ExecuteCommand(context, string.Empty);
            if (guildEvent.DeleteTrigger && messageId.HasValue && !author.GuildPermissions.Administrator)
            {
                await responseChannel.DeleteMessageAsync((ulong) messageId);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _discordClient.UserJoined += ProcessJoin;
            _discordClient.MessageReceived += ProcessMessage;
            _telegramService.MessageReceived += ProcessMessage;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _discordClient.UserJoined -= ProcessJoin;
            _discordClient.MessageReceived -= ProcessMessage;
            _telegramService.MessageReceived -= ProcessMessage;
            return Task.CompletedTask;
        }

        public List<string> GetEventInfos(IGuild guild)
        {
            var guildOptions = GetGuildOptions(guild.Id);
            return guildOptions != null ? guildOptions.Events.Select(e => e.ToString()).ToList() : new List<string>();
        }

        public async Task AddInputEvent(ulong guildId, ulong channelId, string reaction, string trigger, bool deleteTrigger)
        {
            await AddEvent(guildId, channelId, reaction, trigger, deleteTrigger, GuildEventTriggerType.RegexMatch);
        }

        public async Task AddJoinEvent(ulong guildId, ulong channelId, string reaction)
        {
            await AddEvent(guildId, channelId, reaction, default, false, GuildEventTriggerType.GuildJoin);
        }

        private async Task AddEvent(ulong guildId, ulong channelId, string reaction, string? trigger,
            bool deleteTrigger, GuildEventTriggerType triggerType)
        {
            using var scope = _services.CreateScope();
            var discordOptions = scope.ServiceProvider.GetService<IWritableOptionsSnapshot<DiscordOptions>>();
            await discordOptions.UpdateAsync(options =>
            {
                var guildOptions = options.GetOrAddGuildOptions(guildId);
                guildOptions.Events.Add(new GuildEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    ChannelId = channelId,
                    Scope = GuildEventScope.Guild,
                    Reaction = reaction,
                    Trigger = trigger,
                    TriggerType = triggerType,
                    DeleteTrigger = deleteTrigger
                });
            });
        }

        public async Task<bool> TryRemoveEvent(ulong guildId, string eventId)
        {
            using var scope = _services.CreateScope();
            var discordOptions = scope.ServiceProvider.GetService<IWritableOptionsSnapshot<DiscordOptions>>();
            var eventExists = false;
            await discordOptions.UpdateAsync(options =>
            {
                var guildOptions = options.GetOrAddGuildOptions(guildId);
                eventExists = guildOptions.Events.RemoveAll(e => e.Id == eventId) > 0;
            });
            return eventExists;
        }
    }
}