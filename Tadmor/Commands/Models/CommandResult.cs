using Discord.Commands;

namespace Tadmor.Commands.Models
{
    public class CommandResult : RuntimeResult
    {
        private CommandResult(CommandError? error, string reason) : base(error, reason)
        {
        }

        public static RuntimeResult FromError(string reason)
        {
            return new CommandResult(CommandError.Unsuccessful, reason);
        }

        public static RuntimeResult FromSuccess(string value)
        {
            return new CommandResult(null, value);
        }

        public static RuntimeResult FromSuccess(string[] values)
        {
            return FromSuccess(string.Join('\n', values));
        }
    }
}