using Discord.WebSocket;
using Hangfire.Storage;
using Tadmor.Services.Cron;

namespace Tadmor.Services.Discord
{
    public class CommandJobOptions : CronJobOptions
    {
        public string Command { get; set; }

        public override string ToString(RecurringJobDto job, SocketTextChannel channel)
        {
            return $"{job.Id}: execute '{Command}' in {channel.Mention} with cron {job.Cron}";
        }
    }
}