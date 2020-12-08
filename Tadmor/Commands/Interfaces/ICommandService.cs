using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Tadmor.Commands.Interfaces
{
    public interface ICommandService
    {
        Task<ModuleInfo> AddModuleAsync(Type type, IServiceProvider services);

        Task<IResult> ExecuteAsync(ICommandContext context, string input, IServiceProvider services,
            MultiMatchHandling multiMatchHandling = MultiMatchHandling.Exception);

        event Func<Optional<CommandInfo>, ICommandContext, IResult, Task> CommandExecuted;
        IEnumerable<ModuleInfo> Modules { get; }
        Task<bool> RemoveModuleAsync(ModuleInfo module);
    }
}