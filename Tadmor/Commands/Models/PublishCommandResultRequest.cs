using Discord.Commands;

namespace Tadmor.Commands.Models
{
    public record PublishCommandResultRequest(ICommandContext CommandContext, IResult CommandResult);
}