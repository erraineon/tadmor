using System;

namespace Tadmor.Core.Rules.Models
{
    public record Reminder(string Username, string Mention, TimeSpan Delay, string ReminderText, ulong AuthorUserId) :
        OneTimeRule(Delay, AuthorUserId,  $"echo {Mention}: {ReminderText}");
}