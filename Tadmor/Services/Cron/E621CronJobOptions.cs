using Discord.WebSocket;
using Hangfire.Storage;

namespace Tadmor.Services.Cron
{
    public class E621CronJobOptions : CronJobOptions
    {
        public string Tags { get; set; }
        public long AfterId { get; set; }

        public override string ToString(RecurringJobDto job, SocketTextChannel channel)
        {
            return $"{job.Id}: search '{Tags}' on e621 into {channel.Mention} with cron {job.Cron}";
        }
    }
}