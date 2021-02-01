using NCrontab;

namespace Tadmor.Rules.Models
{
    public record CronRule(string CronSchedule, string Reaction) : TimeRule(Reaction);
}