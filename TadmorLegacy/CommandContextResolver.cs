using Discord.Commands;

namespace Tadmor
{
    public class CommandContextResolver
    {
        public ICommandContext? CurrentCommandContext { get; set; }
        public ICommandContext? GetCommandContext()
        {
            return CurrentCommandContext;
        }
    }
}