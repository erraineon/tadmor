using System.Diagnostics.CodeAnalysis;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tadmor.Abstractions.Interfaces;
using Tadmor.Abstractions.Models;
using Tadmor.Abstractions.Services;
using Tadmor.ChatClients.Discord.Interfaces;
using Tadmor.ChatClients.Discord.Models;
using Tadmor.ChatClients.Discord.Services;
using Tadmor.ChatClients.Interfaces;
using Tadmor.ChatClients.Telegram;
using Tadmor.Commands.Formatters;
using Tadmor.Commands.Interfaces;
using Tadmor.Commands.Models;
using Tadmor.Commands.Services;
using Tadmor.Data;
using Tadmor.Data.Services;
using Tadmor.Notifications.Interfaces;
using Tadmor.Notifications.Services;
using Tadmor.Preference.Interfaces;
using Tadmor.Preference.Models;
using Tadmor.Preference.Services;
using Tadmor.Rules.Interfaces;
using Tadmor.Rules.Models;
using Tadmor.Rules.Notifications;
using Tadmor.Rules.Services;

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
                    services.AddSingleton<DiscordClient>();
                    services.AddSingleton<IChatClient>(s => s.GetRequiredService<DiscordClient>());
                    services.AddSingleton<IDiscordChatClient>(s => s.GetRequiredService<DiscordClient>());
                    services.AddSingleton<IHostedService, DiscordClientLauncher>();
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

        public static IHostBuilder UseScheduledTasks(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddHostedService<TimeRuleCheckNotificationPump>();
                    services.AddTransient<INotificationHandler<TimeRuleCheckNotification>, DueTimeRulesBroker>();
                    services.AddTransient<ITimeRuleProvider, TimeRuleProvider>();
                    services.AddTransient<ITimeRulePreferencesWriter, TimeRulePreferencesUpdater>();
                    services.AddTransient<INotificationHandler<RuleExecutedNotification>, TimeRuleUpdater>();
                    services.AddTransient<ITimeRuleNextRunDateCalculator, TimeRuleNextRunDateCalculator>();
                });
        }

        public static IHostBuilder UseCommands(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services =>
                {
                    // utilities
                    services.AddMemoryCache();
                    services.AddTransient<INotificationPublisherFactory, NotificationPublisherFactory>();
                    services.AddTransient<INotificationPublisher>(p => p.GetRequiredService<INotificationPublisherFactory>().Create());

                    // logging
                    services.AddTransient<INotificationHandler<LogNotification>, ChatClientLogger>();
                    
                    // commands
                    services.AddHostedService<CommandsModuleRegistrar>();
                    services.AddHostedService<ChatClientEventDispatcher>();
                    services.AddSingleton(new CommandService(new CommandServiceConfig
                    {
                        DefaultRunMode = RunMode.Sync
                    }));
                    services.AddTransient<ICommandService, CommandServiceWrapper>();
                    services.AddTransient<ICommandExecutor, CommandExecutor>();
                    services.Decorate<ICommandExecutor, LoggingCommandExecutor>();
                    services.AddTransient<ICommandServiceScopeFactory, CommandServiceScopeFactory>();
                    services.AddTransient<ICommandContextFactory, CommandContextFactory>();
                    services.AddScoped<ICommandContextResolver, CommandContextResolver>();
                    services.AddTransient(s => s.GetRequiredService<ICommandContextResolver>().CurrentCommandContext);
                    services.AddTransient<INotificationHandler<MessageReceivedNotification>, ConsideredMessagesFilter>();

                    // if the order is changed, a rule may be triggered as it's being added through a command
                    services.AddTransient<INotificationHandler<MessageValidatedNotification>, TriggeredChatRulesBroker>();
                    services.AddTransient<INotificationHandler<MessageValidatedNotification>, MessageDrivenCommandExecutor>();

                    services.AddTransient<ICommandPrefixValidator, CommandPrefixValidator>();
                    services.AddTransient<ICommandPermissionValidator, CommandPermissionValidator>();
                    services.AddTransient<ICommandResultPublisher, CommandResultPublisher>();
                    services.AddTransient<IRuleExecutor, RuleExecutor>();
                    services.AddTransient<IRuleCommandParser, RecursiveRuleCommandParser>();

                    // formatters
                    services.AddTransient<IStringFormatter<RuleBase>, RuleFormatter>();
                    services.AddTransient<IStringFormatter<PreferencesScope>, PreferenceScopeFormatter>();

                    // preferences
                    services.AddTransient<IGuildPreferencesRepository, GuildPreferencesRepository>();
                    services.AddTransient<IContextualPreferencesProvider, ContextualPreferencesProvider>();
                    services.Decorate<IGuildPreferencesRepository, CachedGuildPreferencesRepositoryDecorator>();
                    services.Decorate<IContextualPreferencesProvider, CachedContextualPreferencesProviderDecorator>();
                    services.AddTransient<INotificationHandler<GuildMemberUpdatedNotification>, CachedContextualPreferencesProviderDecorator>();
                    services.AddTransient<INotificationHandler<GuildPreferencesUpdatedNotification>, CachedContextualPreferencesProviderDecorator>();
                })
                .UseSqlite();
        }

        public static IHostBuilder UseModule<TModule>(this IHostBuilder hostBuilder) where TModule : ModuleBase<ICommandContext>
        {
            return hostBuilder.ConfigureServices(UseModule<TModule>);
        }

        private static void UseModule<TModule>(this IServiceCollection services) where TModule : ModuleBase<ICommandContext>
        {
            services.AddSingleton<IModuleRegistration>(new ModuleRegistration(typeof(TModule)));
        }
    }
}