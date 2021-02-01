using Discord.Commands;

namespace Tadmor.Commands.Models
{
    public record ExecuteCommandRequest(ICommandContext CommandContext, string Input);
}