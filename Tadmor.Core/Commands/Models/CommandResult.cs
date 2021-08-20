using System.Collections.Generic;
using Discord.Commands;

namespace Tadmor.Core.Commands.Models
{
    public class CommandResult : RuntimeResult
    {
        public bool Reply { get; }
        public FileToUpload? File { get; }

        private CommandResult(CommandError? error, string reason, bool reply, FileToUpload? file = default) : base(error, reason)
        {
            Reply = reply;
            File = file;
        }

        public static RuntimeResult FromError(string reason, bool reply = false)
        {
            return new CommandResult(CommandError.Unsuccessful, reason, reply);
        }

        public static RuntimeResult FromSuccess(string value, bool reply = false)
        {
            return new CommandResult(null, value, reply);
        }

        public static RuntimeResult FromSuccess(byte[] file, string filename = "output.png", string? text = default, bool reply = false)
        {
            return new CommandResult(null, text ?? string.Empty, reply, new FileToUpload(file, filename));
        }

        public static RuntimeResult FromSuccess(IEnumerable<string> values, bool reply = false)
        {
            return FromSuccess(string.Join('\n', values), reply);
        }
    }
}