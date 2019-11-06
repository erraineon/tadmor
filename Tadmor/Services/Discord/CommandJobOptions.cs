using Discord;
using Tadmor.Services.Hangfire;

namespace Tadmor.Services.Discord
{
    public class CommandJobOptions : HangfireJobOptions
    {
        public string? Command { get; set; }

        public override string ToString(string jobId, string scheduleDescription, ITextChannel channel)
        {
            return $"{jobId}: execute '{Command}' in {channel.Mention} {scheduleDescription}";
        }
    }
}