using System;
using Tadmor.Rules.Models;

namespace Tadmor.Rules.Interfaces
{
    public interface ITimeRuleNextRunDateCalculator
    {
        DateTime GetNextRunDate(TimeRule timeRule);
    }
}