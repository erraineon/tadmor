using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Commands.Formatters;
using Tadmor.Commands.Models;
using Tadmor.Extensions;
using Tadmor.Formatting.Interfaces;
using Tadmor.Notifications.Interfaces;
using Tadmor.Rules.Interfaces;
using Tadmor.Rules.Models;
using Tadmor.Rules.Modules;
using Tadmor.Rules.Notifications;
using Tadmor.Rules.Services;

namespace Tadmor.Rules.Extensions
{
    public static class RulesRegistrationExtensions
    {
        public static IHostBuilder UseRules(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services => services
                    .AddTransient<IRuleExecutor, RuleExecutor>()
                    .AddTransient<IRuleCommandParser, RecursiveRuleCommandParser>()
                    .AddTransient<IStringFormatter<RuleBase>, RuleFormatter>()
                    .AddTransient<INotificationHandler<MessageValidatedNotification>, ChatRuleMonitor>())
                .UseModule<RulePreferencesModule>()
                .UseScheduledTasks();
        }

        public static IHostBuilder UseScheduledTasks(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services => services
                    .AddHostedService<TimeRuleCheckNotificationPump>()
                    .AddTransient<INotificationHandler<TimeRuleCheckNotification>, TimeRuleMonitor>()
                    .AddTransient<ITimeRuleProvider, TimeRuleProvider>()
                    .AddTransient<ITimeRulePreferencesWriter, TimeRulePreferencesUpdater>()
                    .AddTransient<INotificationHandler<RuleExecutedNotification>, TimeRuleUpdater>()
                    .AddTransient<ITimeRuleNextRunDateCalculator, TimeRuleNextRunDateCalculator>())
                .UseModule<TimeRulePreferencesModule>();
        }
    }
}