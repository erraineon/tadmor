using System;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Extensions;

namespace Tadmor.Preconditions
{
    public class RequireReplyAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            var result = await context.Message.IsReplyAsync()
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("not a reply message");
            return result;
        }
    }
}