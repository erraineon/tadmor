using Discord.WebSocket;
using Hangfire.Storage;
using Tadmor.Extensions;
using Tadmor.Services.Hangfire;

namespace Tadmor.Services.Discord
{
    public class CommandJobOptions : HangfireJobOptions
    {
        public string Command { get; set; }

        public override string ToString(RecurringJobDto job, SocketTextChannel channel)
        {
            return $"{job.Id}: execute '{Command}' in {channel.Mention} {job.Cron.ToCronDescription()}";
        }
    }
}