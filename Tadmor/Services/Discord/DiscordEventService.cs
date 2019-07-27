using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Tadmor.Utils;

namespace Tadmor.Services.Discord
{
    public class DiscordEventService : IHostedService
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly DiscordService _discordService;
        private readonly IServiceProvider _services;

        public DiscordEventService(DiscordSocketClient discordClient, DiscordService discordService, IServiceProvider services)
        {
            _discordClient = discordClient;
            _discordService = discordService;
            _services = services;
        }

        private async Task ProcessJoin(SocketGuildUser arg)
        {
            await ProcessEvent(null, null, arg.Guild.Id, arg.Id, default, GuildEventTriggerType.GuildJoin);
        }

        private async Task ProcessMessage(SocketMessage msg)
        {
            if (msg.Channel is SocketGuildChannel guildChannel)
                await ProcessEvent(msg.Id, guildChannel.Id, guildChannel.Guild.Id, msg.Author.Id, msg.Content, GuildEventTriggerType.RegexMatch);
        }

        private async Task ProcessEvent(ulong? messageId, ulong? inputChannelId, ulong guildId, ulong authorId,
            string input,
            GuildEventTriggerType triggerType)
        {
            if (authorId != _discordClient.CurrentUser.Id)
            {
                var guildOptions = GetGuildOptions(guildId);
                if (guildOptions != null)
                {
                    var events = GetEvents(inputChannelId, triggerType, input, guildOptions);
                    foreach (var guildEvent in events)
                        await ProcessEvent(messageId, inputChannelId, authorId, input, guildEvent);
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
                                    Regex.IsMatch(input, e.Trigger, RegexOptions.None, TimeSpan.FromMilliseconds(100)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(triggerType), triggerType, null);
            }

            return events;
        }

        private async Task ProcessEvent(ulong? messageId, ulong? inputChannelId, ulong authorId, string input,
            GuildEvent guildEvent)
        {
            var responseChannel = (SocketTextChannel) _discordClient.GetChannel(inputChannelId ?? guildEvent.ChannelId);
            var author = _discordClient.GetUser(authorId);
            var variablesByName = new Dictionary<string, string>
            {
                ["user"] = author.Mention,
                ["time"] = DateTime.Now.ToString("g"),
                ["random"] = new Random().Next(10000).ToString(),
                ["input"] = input
            };
            var reaction = variablesByName
                .Aggregate(guildEvent.Reaction,
                    (output, variable) => output.Replace($"{{{{{variable.Key}}}}}", variable.Value));
            var message = new FakeUserMessage(responseChannel, author, reaction);
            var context = new CommandContext(_discordClient, message);
            await _discordService.ExecuteCommand(context, string.Empty);
            if (guildEvent.DeleteTrigger && messageId.HasValue)
            {
                await responseChannel.DeleteMessageAsync((ulong) messageId);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _discordClient.UserJoined += ProcessJoin;
            _discordClient.MessageReceived += ProcessMessage;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _discordClient.UserJoined -= ProcessJoin;
            _discordClient.MessageReceived -= ProcessMessage;
            return Task.CompletedTask;
        }

        public List<string> GetEventInfos(SocketGuild guild)
        {
            var guildOptions = GetGuildOptions(guild.Id);
            return guildOptions != null ? guildOptions.Events.Select(e => e.ToString(guild)).ToList() : new List<string>();
        }
    }
}