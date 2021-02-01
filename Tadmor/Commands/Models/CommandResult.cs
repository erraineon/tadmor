using Discord.Commands;

namespace Tadmor.Commands.Models
{
    public class CommandResult : RuntimeResult
    {
        private CommandResult(CommandError? error, string reason) : base(error, reason)
        {
        }
        
        public static RuntimeResult FromError(string reason) => new CommandResult(CommandError.Unsuccessful, reason);
        public static RuntimeResult FromSuccess(string value) => new CommandResult(null, value);
        public static RuntimeResult FromSuccess(string[] values) => FromSuccess(string.Join('\n', values));
    }
}