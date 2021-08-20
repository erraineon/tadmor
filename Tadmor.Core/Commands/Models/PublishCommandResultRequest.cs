using Discord.Commands;

namespace Tadmor.Core.Commands.Models
{
    public record PublishCommandResultRequest(ICommandContext CommandContext, IResult Result);
}