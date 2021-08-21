namespace Tadmor.Core.Rules.Models
{
    public record CronRule(string CronSchedule, string Reaction) : TimeRule(Reaction);
}