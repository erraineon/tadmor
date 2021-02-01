using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Commands.Builders;

namespace Tadmor.Commands.Interfaces
{
    public interface ICommandService
    {
        void Dispose();
        Task<ModuleInfo> CreateModuleAsync(string primaryAlias, Action<ModuleBuilder> buildFunc);
        Task<ModuleInfo> AddModuleAsync<T>(IServiceProvider services);
        Task<ModuleInfo> AddModuleAsync(Type type, IServiceProvider services);
        Task<IEnumerable<ModuleInfo>> AddModulesAsync(Assembly assembly, IServiceProvider services);
        Task<bool> RemoveModuleAsync(ModuleInfo module);
        Task<bool> RemoveModuleAsync<T>();
        Task<bool> RemoveModuleAsync(Type type);
        void AddTypeReader<T>(TypeReader reader);
        void AddTypeReader(Type type, TypeReader reader);
        void AddTypeReader<T>(TypeReader reader, bool replaceDefault);
        void AddTypeReader(Type type, TypeReader reader, bool replaceDefault);
        SearchResult Search(ICommandContext context, int argPos);
        SearchResult Search(ICommandContext context, string input);
        SearchResult Search(string input);

        Task<IResult> ExecuteAsync(ICommandContext context, int argPos, IServiceProvider services,
            MultiMatchHandling multiMatchHandling = MultiMatchHandling.Exception);

        Task<IResult> ExecuteAsync(ICommandContext context, string input, IServiceProvider services,
            MultiMatchHandling multiMatchHandling = MultiMatchHandling.Exception);

        IEnumerable<ModuleInfo> Modules { get; }
        IEnumerable<CommandInfo> Commands { get; }
        ILookup<Type, TypeReader> TypeReaders { get; }
        event Func<LogMessage, Task> Log;
        event Func<Optional<CommandInfo>, ICommandContext, IResult, Task> CommandExecuted;
    }
}