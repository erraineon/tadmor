using Discord.WebSocket;
using Hangfire.Storage;
using Tadmor.Extensions;
using Tadmor.Services.Hangfire;
using Tadmor.Utils;

namespace Tadmor.Services.E621
{
    public class E621SearchJobOptions : HangfireJobOptions
    {
        public string Tags { get; set; }
        public long AfterId { get; set; }

        public override string ToString(string jobId, string scheduleDescription, SocketTextChannel channel)
        {
            return $"{jobId}: search '{Tags}' on e621 into {channel.Mention} {scheduleDescription}";
        }
    }
}