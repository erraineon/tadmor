using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.Commands.Formatters;
using Tadmor.Core.Commands.Models;
using Tadmor.Core.Extensions;
using Tadmor.Core.Formatting.Interfaces;
using Tadmor.Core.Notifications.Interfaces;
using Tadmor.Core.Rules.Interfaces;
using Tadmor.Core.Rules.Models;
using Tadmor.Core.Rules.Modules;
using Tadmor.Core.Rules.Notifications;
using Tadmor.Core.Rules.Services;

namespace Tadmor.Core.Rules.Extensions
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

        private static IHostBuilder UseScheduledTasks(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services => services
                    .AddHostedService<TimeRuleCheckNotificationPump>()
                    .AddTransient<INotificationHandler<TimeRuleCheckNotification>, TimeRuleMonitor>()
                    .AddTransient<ITimeRuleProvider, TimeRuleProvider>()
                    .AddTransient<ITimeRulePreferencesWriter, TimeRulePreferencesUpdater>()
                    .AddTransient<INotificationHandler<RuleExecutedNotification>, TimeRuleUpdater>()
                    .AddTransient<ITimeRuleNextRunDateCalculator, TimeRuleNextRunDateCalculator>())
                .UseModule<RulePreferencesModule>();
        }
    }
}