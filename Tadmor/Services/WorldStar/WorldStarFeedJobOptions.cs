using Discord.WebSocket;
using Hangfire.Storage;
using Tadmor.Extensions;
using Tadmor.Services.Cron;

namespace Tadmor.Services.WorldStar
{
    public class WorldStarFeedJobOptions : CronJobOptions
    {
        public string LastLink { get; set; }

        public override string ToString(RecurringJobDto job, SocketTextChannel channel)
        {
            return $"{job.Id}: post new worldstar videos into {channel.Mention} {job.Cron.ToCronDescription()}";
        }
    }
}