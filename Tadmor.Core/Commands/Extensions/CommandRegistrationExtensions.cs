using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.ChatClients.Abstractions.Extensions;
using Tadmor.Core.ChatClients.Abstractions.Models;
using Tadmor.Core.Commands.Interfaces;
using Tadmor.Core.Commands.Models;
using Tadmor.Core.Commands.Modules;
using Tadmor.Core.Commands.Services;
using Tadmor.Core.Data.Extensions;
using Tadmor.Core.Extensions;
using Tadmor.Core.Notifications.Extensions;
using Tadmor.Core.Notifications.Interfaces;
using Tadmor.Core.Preference.Extensions;
using Tadmor.Core.Rules.Extensions;

namespace Tadmor.Core.Commands.Extensions
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
                    .Decorate<ICommandExecutor, LoggingCommandExecutorDecorator>()
                    .Decorate<ICommandExecutor, PermissionAwareCommandExecutorDecorator>()

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
                    .AddTransient<ICommandResultPublisher, CommandResultPublisher>()
                
                    // commands help and related
                    .AddTransient<ICommandsMetadataProvider, CommandsMetadataProvider>()
                )
                .UseModule<HelpModule>();
        }
    }
}