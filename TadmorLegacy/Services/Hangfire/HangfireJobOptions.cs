using Discord;
using Tadmor.Services.Abstractions;

namespace Tadmor.Services.Hangfire
{
    public abstract class HangfireJobOptions
    {
        public ChatClientType ContextType { get; set; }
        public ulong ChannelId { get; set; }
        public ulong OwnerId { get; set; }
        public abstract string ToString(string jobId, string scheduleDescription, ITextChannel channel);
    }
}