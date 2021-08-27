using System;
using System.Text.RegularExpressions;
using Discord;
using JetBrains.Annotations;
using Tadmor.Core.Commands.Models;

namespace Tadmor.Core.Rules.Models
{
    public class RegexRuleTriggerContext : RuleTriggerContextBase
    {
        private readonly Regex _regex;
        private readonly IUserMessage _trigger;

        public RegexRuleTriggerContext(
            RegexRule regexRule,
            MessageValidatedNotification notification) : base(
            notification.GuildUser,
            notification.GuildChannel,
            notification.ChatClient,
            notification.UserMessage,
            regexRule,
            true)
        {
            _trigger = notification.UserMessage;
            _regex = new Regex(regexRule.Trigger, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
        }

        public Match GetMatch()
        {
            var match = _regex.Match(_trigger.Content);
            return match;
        }

        public override bool ShouldExecute => GetMatch().Success;
    }
}