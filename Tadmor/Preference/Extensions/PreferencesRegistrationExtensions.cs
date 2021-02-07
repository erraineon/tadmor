using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.ChatClients.Abstractions.Models;
using Tadmor.Commands.Formatters;
using Tadmor.Commands.Modules;
using Tadmor.Extensions;
using Tadmor.Formatting.Interfaces;
using Tadmor.Notifications.Interfaces;
using Tadmor.Preference.Interfaces;
using Tadmor.Preference.Models;
using Tadmor.Preference.Services;

namespace Tadmor.Preference.Extensions
{
    public static class PreferencesRegistrationExtensions
    {
        public static IHostBuilder UsePreferences(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services => services
                    .AddTransient<IGuildPreferencesRepository, GuildPreferencesRepository>()
                    .AddTransient<IContextualPreferencesProvider, ContextualPreferencesProvider>()
                    .Decorate<IGuildPreferencesRepository, CachedGuildPreferencesRepositoryDecorator>()
                    .Decorate<IContextualPreferencesProvider, CachedContextualPreferencesProviderDecorator>()
                    .AddTransient<
                        INotificationHandler<GuildMemberUpdatedNotification>,
                        CachedContextualPreferencesProviderDecorator>()
                    .AddTransient<
                        INotificationHandler<GuildPreferencesUpdatedNotification>,
                        CachedContextualPreferencesProviderDecorator>()
                    .AddTransient<IStringFormatter<PreferencesScope>, PreferenceScopeFormatter>())
                .UseModule<CommandPreferencesModule>();
        }
    }
}