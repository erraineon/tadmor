﻿using System;
using NCrontab;
using Tadmor.Core.Rules.Interfaces;
using Tadmor.Core.Rules.Models;

namespace Tadmor.Core.Rules.Services
{
    public class TimeRuleNextRunDateCalculator : ITimeRuleNextRunDateCalculator
    {
        public DateTime GetNextRunDate(TimeRule timeRule)
        {
            var now = DateTime.Now;
            var nextRunDate = timeRule switch
            {
                CronRule scheduledRule => CrontabSchedule.Parse(scheduledRule.CronSchedule).GetNextOccurrence(now),
                RecurringRule recurringRule => timeRule.LastRunDate + recurringRule.Interval is
                    { } originalNextRunDate && originalNextRunDate > now
                    ? originalNextRunDate
                    : now + recurringRule.Interval,
                OneTimeRule oneTimeRule => now + oneTimeRule.Delay,
                _ => throw new ArgumentOutOfRangeException(nameof(timeRule), timeRule, null)
            };
            return nextRunDate;
        }
    }
}