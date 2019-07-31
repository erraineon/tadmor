using System;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Utils;

namespace Tadmor.Preconditions
{
    public class RequireServiceUser : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return Task.FromResult(context.Message is ServiceUserMessage
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("command must be executed through service user"));
        }
    }
}