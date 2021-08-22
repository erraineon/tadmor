using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;
using Tadmor.Core.Notifications.Interfaces;
using Tadmor.Core.Rules.Interfaces;
using Tadmor.Core.Rules.Models;

namespace Tadmor.Core.Rules.Services
{
    public class TimeRuleMonitor : INotificationHandler<TimeRuleCheckNotification>
    {
        private readonly IRuleExecutor _ruleExecutor;
        private readonly ITimeRuleProvider _timeRuleProvider;

        public TimeRuleMonitor(
            ITimeRuleProvider timeRuleProvider,
            IRuleExecutor ruleExecutor)
        {
            _timeRuleProvider = timeRuleProvider;
            _ruleExecutor = ruleExecutor;
        }

        public async Task HandleAsync(TimeRuleCheckNotification notification, CancellationToken cancellationToken)
        {
            var chatClient = notification.ChatClient;
            var guilds = await chatClient.GetGuildsAsync();
            await Task.WhenAll(
                guilds
                    .Select(async guild => { await RunDueTimeRulesAsync(guild, chatClient, cancellationToken); }));
        }

        private async Task RunDueTimeRulesAsync(
            IGuild guild,
            IChatClient chatClient,
            CancellationToken cancellationToken)
        {
            var dueTimeRulesByChannelId = await _timeRuleProvider.GetRulesByChannelId(guild.Id, DateTime.Now);
            await Task.WhenAll(
                dueTimeRulesByChannelId
                    .Select(
                        async rulesByChannel =>
                        {
                            if (await chatClient.GetChannelAsync(rulesByChannel.Key) is ITextChannel channel)
                                await RunDueTimeRulesAsync(chatClient, channel, rulesByChannel, cancellationToken);
                        }));
        }

        private async Task RunDueTimeRulesAsync(
            IChatClient chatClient,
            IGuildChannel channel,
            IEnumerable<TimeRule> rules,
            CancellationToken cancellationToken)
        {
            var timeRules = rules.ToList();
            await Task.WhenAll(
                timeRules.Select(
                    timeRule =>
                        RunDueRuleAsync(chatClient, channel, timeRule, cancellationToken)));
        }

        private async Task RunDueRuleAsync(
            IChatClient chatClient,
            IGuildChannel channel,
            TimeRule timeRule,
            CancellationToken cancellationToken)
        {
            // warning: by defaulting to the current user instead of the author of the rule,
            // we may be escalating permissions. for the time being, have the permissions for
            // deferred execution commands be the same as the current user's.
            // TODO #19: store snapshot of user upon creation, then retrieve upon execution
            var executeAs = 
                await channel.Guild.GetUserAsync(timeRule.AuthorUserId) ??
                await channel.Guild.GetUserAsync(chatClient.CurrentUser.Id);
            var timeRuleTriggerContext = new TimeRuleTriggerContext(
                timeRule,
                executeAs,
                channel,
                chatClient);
            await _ruleExecutor.ExecuteRuleAsync(timeRuleTriggerContext, cancellationToken);
        }
    }
}