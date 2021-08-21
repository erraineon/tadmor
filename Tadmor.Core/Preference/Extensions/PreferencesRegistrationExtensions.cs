using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.ChatClients.Abstractions.Models;
using Tadmor.Core.Commands.Formatters;
using Tadmor.Core.Commands.Models;
using Tadmor.Core.Commands.Modules;
using Tadmor.Core.Extensions;
using Tadmor.Core.Formatting.Interfaces;
using Tadmor.Core.Notifications.Interfaces;
using Tadmor.Core.Preference.Interfaces;
using Tadmor.Core.Preference.Models;
using Tadmor.Core.Preference.Services;

namespace Tadmor.Core.Preference.Extensions
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
                    .AddTransient<IStringFormatter<PreferencesScope>, PreferenceScopeFormatter>()
                    .AddTransient<IStringFormatter<CommandPermission>, CommandPermissionFormatter>())
                .UseModule<CommandPreferencesModule>();
        }
    }
}