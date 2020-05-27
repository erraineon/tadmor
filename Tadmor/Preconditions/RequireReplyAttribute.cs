using System;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Utils;

namespace Tadmor.Preconditions
{
    public class RequireReplyAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            var result = context.Message is IReplyMessage rm && await rm.GetQuotedMessageAsync() != null
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("not a reply message");
            return result;
        }
    }
}