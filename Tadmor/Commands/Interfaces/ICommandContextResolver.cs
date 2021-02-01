using Discord.Commands;

namespace Tadmor.Commands.Interfaces
{
    public interface ICommandContextResolver
    {
        ICommandContext CurrentCommandContext { get; set; }
    }
}