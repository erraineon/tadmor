using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace Tadmor.Core.Commands.Interfaces
{
    public interface ICommandsMetadataProvider
    {
        Task<IEnumerable<IGrouping<ModuleInfo, CommandInfo>>> GetCommandsByModuleAsync(ICommandContext commandContext);
        Task<IEnumerable<CommandInfo>> GetCommandsAsync(ICommandContext commandContext);
    }
}