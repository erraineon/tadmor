using System;
using Tadmor.Core.Rules.Models;

namespace Tadmor.Core.Rules.Interfaces
{
    public interface ITimeRuleNextRunDateCalculator
    {
        DateTime GetNextRunDate(TimeRule timeRule);
    }
}