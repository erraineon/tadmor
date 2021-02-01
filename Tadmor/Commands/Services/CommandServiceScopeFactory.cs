using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Tadmor.Commands.Interfaces;

namespace Tadmor.Commands.Services
{
    public class CommandServiceScopeFactory : ICommandServiceScopeFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CommandServiceScopeFactory(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<IServiceScope> CreateScopeAsync(ICommandContext commandContext)
        {
            var serviceScope = _serviceScopeFactory.CreateScope();
            var commandContextResolver = serviceScope.ServiceProvider.GetRequiredService<ICommandContextResolver>();
            commandContextResolver.CurrentCommandContext = commandContext;
            return serviceScope;
        }
    }
}