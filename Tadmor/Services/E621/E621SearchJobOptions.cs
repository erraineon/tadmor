using Discord.WebSocket;
using Hangfire.Storage;
using Tadmor.Services.Cron;
using Tadmor.Services.Discord;

namespace Tadmor.Services.E621
{
    public class E621SearchJobOptions : CronJobOptions
    {
        public string Tags { get; set; }
        public long AfterId { get; set; }

        public override string ToString(RecurringJobDto job, SocketTextChannel channel)
        {
            return $"{job.Id}: search '{Tags}' on e621 into {channel.Mention} with cron {job.Cron}";
        }
    }
}