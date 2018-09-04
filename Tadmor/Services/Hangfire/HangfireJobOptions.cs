using Discord.WebSocket;
using Hangfire.Storage;

namespace Tadmor.Services.Hangfire
{
    public abstract class HangfireJobOptions
    {
        public ulong ChannelId { get; set; }
        public abstract string ToString(RecurringJobDto job, SocketTextChannel channel);
        public ulong OwnerId { get; set; }
    }
}