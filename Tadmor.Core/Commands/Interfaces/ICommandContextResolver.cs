using Discord.Commands;

namespace Tadmor.Core.Commands.Interfaces
{
    public interface ICommandContextResolver
    {
        ICommandContext CurrentCommandContext { get; set; }
    }
}