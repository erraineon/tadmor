using Discord.Commands;

namespace Tadmor.Core.Commands.Models
{
    public class CommandResult : RuntimeResult
    {
        public bool Reply { get; }

        private CommandResult(CommandError? error, string reason, bool reply) : base(error, reason)
        {
            Reply = reply;
        }

        public static RuntimeResult FromError(string reason, bool reply = false)
        {
            return new CommandResult(CommandError.Unsuccessful, reason, reply);
        }

        public static RuntimeResult FromSuccess(string value, bool reply = false)
        {
            return new CommandResult(null, value, reply);
        }

        public static RuntimeResult FromSuccess(string[] values, bool reply = false)
        {
            return FromSuccess(string.Join('\n', values));
        }
    }
}