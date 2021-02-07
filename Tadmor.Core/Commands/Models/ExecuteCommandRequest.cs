using Discord.Commands;

namespace Tadmor.Core.Commands.Models
{
    public record ExecuteCommandRequest(ICommandContext CommandContext, string Input);
}