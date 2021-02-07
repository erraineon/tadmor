using System.Linq;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.ChatClients.Abstractions.Extensions;
using Tadmor.ChatClients.Abstractions.Models;
using Tadmor.Commands.Formatters;
using Tadmor.Commands.Interfaces;
using Tadmor.Commands.Models;
using Tadmor.Commands.Services;
using Tadmor.Extensions;
using Tadmor.Formatting.Interfaces;
using Tadmor.Notifications.Extensions;
using Tadmor.Notifications.Interfaces;
using Tadmor.Notifications.Services;
using Tadmor.Preference.Extensions;
using Tadmor.Preference.Interfaces;
using Tadmor.Preference.Models;
using Tadmor.Preference.Services;
using Tadmor.Rules.Extensions;
using Tadmor.Rules.Interfaces;
using Tadmor.Rules.Models;
using Tadmor.Rules.Services;

namespace Tadmor.Commands.Extensions
{
    public static class CommandRegistrationExtensions
    {
        public static IHostBuilder UseCommands(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .UseChatClientEvents()
                .UseNotifications()
                .UseRules()
                .UseTadmorDbContext()
                .UsePreferences()
                .ConfigureServices(services => services.AddMemoryCache()
                    // commands
                    .AddSingleton(new CommandService())
                    .AddTransient<ICommandService, CommandServiceWrapper>()
                    .AddHostedService<CommandsModuleRegistrar>()
                    .AddTransient<ICommandExecutor, CommandExecutor>()
                    .Decorate<ICommandExecutor, LoggingCommandExecutor>()

                    // command context
                    .AddScoped<ICommandContextResolver, CommandContextResolver>()
                    .AddTransient<ICommandContext>(s =>
                        s.GetRequiredService<ICommandContextResolver>().CurrentCommandContext)
                    .AddTransient<ICommandServiceScopeFactory, CommandServiceScopeFactory>()
                    .AddTransient<ICommandContextFactory, CommandContextFactory>()

                    // chat command execution pipeline
                    .AddTransient<INotificationHandler<MessageReceivedNotification>, ReceivedMessageValidator>()
                    .AddTransient<INotificationHandler<MessageValidatedNotification>, ChatCommandMonitor>()
                    .AddTransient<ICommandPrefixValidator, CommandPrefixValidator>()
                    .AddTransient<ICommandPermissionValidator, CommandPermissionValidator>()
                    .AddTransient<ICommandResultPublisher, CommandResultPublisher>());
        }
    }
}