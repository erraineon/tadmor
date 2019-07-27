using System;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Utils;

namespace Tadmor.Preconditions
{
    public class RequireFakeUserMessage : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return Task.FromResult(context.Message is FakeUserMessage
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("command must be executed through service account"));
        }
    }
}