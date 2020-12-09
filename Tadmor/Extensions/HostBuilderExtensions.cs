using System.Diagnostics.CodeAnalysis;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tadmor.Abstractions.Services;
using Tadmor.ChatClients.Discord.Interfaces;
using Tadmor.ChatClients.Discord.Models;
using Tadmor.ChatClients.Discord.Services;
using Tadmor.ChatClients.Interfaces;
using Tadmor.Commands.Interfaces;
using Tadmor.Commands.Models;
using Tadmor.Commands.Services;
using Tadmor.Data;
using Tadmor.Data.Services;
using Tadmor.Notifications.Interfaces;
using Tadmor.Notifications.Models;
using Tadmor.Notifications.Services;
using Tadmor.Preference.Interfaces;
using Tadmor.Preference.Services;
using Tadmor.Telegram;

namespace Tadmor.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseLogging(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services => services
                    .AddLogging(configLogging => configLogging.AddConsole()));
        }
        public static IHostBuilder UseDiscord(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices((hostingContext, services) =>
                {
                    services.BindConfigurationSection<DiscordOptions>(hostingContext.Configuration);
                    services.TryAddSingleton<DiscordClient>();
                    services.TryAddSingleton<IChatClient>(s => s.GetRequiredService<DiscordClient>());
                    services.TryAddSingleton<IDiscordChatClient>(s => s.GetRequiredService<DiscordClient>());
                    services.TryAddSingleton<IHostedService, DiscordClientLauncher>();
                });
        }

        public static IHostBuilder UseTelegram(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices((hostingContext, services) =>
                {
                    services.BindConfigurationSection<TelegramOptions>(hostingContext.Configuration);
                });
        }

        public static IHostBuilder UseSqlite(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddDbContext<TadmorDbContext>(optionsBuilder =>
                    {
                        var configuration = hostingContext.Configuration;
                        var sqliteConnectionString = configuration.GetConnectionString("SqliteConnectionString");
                        optionsBuilder.UseSqlite(sqliteConnectionString);
                    });
                    services.AddHostedService<DatabaseMigrator>();
                });
        }

        public static IHostBuilder UseCommands(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services =>
                {
                    // utilities
                    services.AddMemoryCache();
                    services.AddTransient<NotificationPublisherFactory, NotificationPublisherFactory>();
                    services.AddTransient<INotificationPublisher, NotificationPublisher>();

                    // logging
                    services.AddTransient<INotificationHandler<LogNotification>, ChatClientLogger>();
                    
                    // commands
                    services.AddHostedService<CommandsModuleRegistrar>();
                    services.AddHostedService<ChatClientEventDispatcher>();
                    services.TryAddSingleton(new CommandService(new CommandServiceConfig
                    {
                        DefaultRunMode = RunMode.Async
                    }));
                    services.TryAddTransient<ICommandService, CommandServiceWrapper>();
                    services.TryAddTransient<ICommandExecutor, CommandExecutor>();
                    services.TryAddTransient<INotificationHandler<MessageReceivedNotification>, CommandListener>();

                    // preferences
                    services.TryAddTransient<IGuildPreferencesRepository, GuildPreferencesRepository>();
                    services.TryAddTransient<IContextualPreferencesProvider, ContextualPreferencesProvider>();
                    services.Decorate<IGuildPreferencesRepository, CachedGuildPreferencesRepositoryDecorator>();
                    services.Decorate<IContextualPreferencesProvider, CachedContextualPreferencesProviderDecorator>();
                    services.TryAddTransient<INotificationHandler<GuildMemberUpdatedNotification>, CachedContextualPreferencesProviderDecorator>();
                    services.TryAddTransient<INotificationHandler<GuildPreferencesUpdatedNotification>, CachedContextualPreferencesProviderDecorator>();
                })
                .UseSqlite();
        }

        public static IHostBuilder UseModule<TModule>(this IHostBuilder hostBuilder) where TModule : ModuleBase<ICommandContext>
        {
            return hostBuilder.ConfigureServices(UseModule<TModule>);
        }

        private static void UseModule<TModule>(this IServiceCollection services) where TModule : ModuleBase<ICommandContext>
        {
            services.TryAddSingleton<IModuleRegistration>(new ModuleRegistration(typeof(TModule)));
        }
    }
}