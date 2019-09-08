using Discord;
using Discord.WebSocket;
using Hangfire.Storage;
using Tadmor.Extensions;
using Tadmor.Services.Hangfire;
using Tadmor.Utils;

namespace Tadmor.Services.Discord
{
    public class CommandJobOptions : HangfireJobOptions
    {
        public CommandJobContextType ContextType { get; set; }
        public string Command { get; set; }

        public override string ToString(string jobId, string scheduleDescription, ITextChannel channel)
        {
            return $"{jobId}: execute '{Command}' in {channel.Mention} {scheduleDescription}";
        }
    }

    public enum CommandJobContextType
    {
        Discord,
        Telegram
    }
}