using Discord;
using Tadmor.Services.Hangfire;

namespace Tadmor.Services.E621
{
    public class E621SearchJobOptions : HangfireJobOptions
    {
        public string Tags { get; set; }
        public long AfterId { get; set; }

        public override string ToString(string jobId, string scheduleDescription, ITextChannel channel)
        {
            return $"{jobId}: search '{Tags}' on e621 into {channel.Mention} {scheduleDescription}";
        }
    }
}