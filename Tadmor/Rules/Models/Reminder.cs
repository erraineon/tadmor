using System;

namespace Tadmor.Rules.Models
{
    public record Reminder(string Username, string Mention, TimeSpan Delay, string ReminderText) :
        OneTimeRule(Delay, $"echo {Mention}: {ReminderText}");
}