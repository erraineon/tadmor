namespace Tadmor.Core.Rules.Models
{
    public record CronRule(string CronSchedule, ulong AuthorUserId, string Reaction) : TimeRule(AuthorUserId, Reaction);
}