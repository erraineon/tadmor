using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Commands.Builders;
using Tadmor.Commands.Interfaces;

namespace Tadmor.Commands.Services
{
    [ExcludeFromCodeCoverage]
    public class CommandServiceWrapper : ICommandService
    {
        private readonly CommandService _commandService;

        public CommandServiceWrapper(CommandService commandService)
        {
            _commandService = commandService;
        }

        public void Dispose()
        {
            ((IDisposable) _commandService).Dispose();
        }

        public Task<ModuleInfo> CreateModuleAsync(string primaryAlias, Action<ModuleBuilder> buildFunc)
        {
            return _commandService.CreateModuleAsync(primaryAlias, buildFunc);
        }

        public Task<ModuleInfo> AddModuleAsync<T>(IServiceProvider services)
        {
            return _commandService.AddModuleAsync<T>(services);
        }

        public Task<ModuleInfo> AddModuleAsync(Type type, IServiceProvider services)
        {
            return _commandService.AddModuleAsync(type, services);
        }

        public Task<IEnumerable<ModuleInfo>> AddModulesAsync(Assembly assembly, IServiceProvider services)
        {
            return _commandService.AddModulesAsync(assembly, services);
        }

        public Task<bool> RemoveModuleAsync(ModuleInfo module)
        {
            return _commandService.RemoveModuleAsync(module);
        }

        public Task<bool> RemoveModuleAsync<T>()
        {
            return _commandService.RemoveModuleAsync<T>();
        }

        public Task<bool> RemoveModuleAsync(Type type)
        {
            return _commandService.RemoveModuleAsync(type);
        }

        public void AddTypeReader<T>(TypeReader reader)
        {
            _commandService.AddTypeReader<T>(reader);
        }

        public void AddTypeReader(Type type, TypeReader reader)
        {
            _commandService.AddTypeReader(type, reader);
        }

        public void AddTypeReader<T>(TypeReader reader, bool replaceDefault)
        {
            _commandService.AddTypeReader<T>(reader, replaceDefault);
        }

        public void AddTypeReader(Type type, TypeReader reader, bool replaceDefault)
        {
            _commandService.AddTypeReader(type, reader, replaceDefault);
        }

        public SearchResult Search(ICommandContext context, int argPos)
        {
            return _commandService.Search(context, argPos);
        }

        public SearchResult Search(ICommandContext context, string input)
        {
            return _commandService.Search(context, input);
        }

        public SearchResult Search(string input)
        {
            return _commandService.Search(input);
        }

        public Task<IResult> ExecuteAsync(
            ICommandContext context,
            int argPos,
            IServiceProvider services,
            MultiMatchHandling multiMatchHandling = MultiMatchHandling.Exception)
        {
            return _commandService.ExecuteAsync(context, argPos, services, multiMatchHandling);
        }

        public Task<IResult> ExecuteAsync(
            ICommandContext context,
            string input,
            IServiceProvider services,
            MultiMatchHandling multiMatchHandling = MultiMatchHandling.Exception)
        {
            return _commandService.ExecuteAsync(context, input, services, multiMatchHandling);
        }

        public IEnumerable<ModuleInfo> Modules => _commandService.Modules;

        public IEnumerable<CommandInfo> Commands => _commandService.Commands;

        public ILookup<Type, TypeReader> TypeReaders => _commandService.TypeReaders;

        public event Func<LogMessage, Task> Log
        {
            add => _commandService.Log += value;
            remove => _commandService.Log -= value;
        }

        public event Func<Optional<CommandInfo>, ICommandContext, IResult, Task> CommandExecuted
        {
            add => _commandService.CommandExecuted += value;
            remove => _commandService.CommandExecuted -= value;
        }
    }
}