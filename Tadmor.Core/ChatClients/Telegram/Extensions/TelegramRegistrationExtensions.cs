using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.ChatClients.Abstractions.Interfaces;
using Tadmor.Core.ChatClients.Telegram.Interfaces;
using Tadmor.Core.ChatClients.Telegram.Models;
using Tadmor.Core.ChatClients.Telegram.Services;
using Tadmor.Core.Extensions;
using Telegram.Bot;

namespace Tadmor.Core.ChatClients.Telegram.Extensions
{
    public static class TelegramRegistrationExtensions
    {
        public static IHostBuilder UseTelegram(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(
                    (hostingContext, services) =>
                    {
                        var telegramOptions =
                            services.BindConfigurationSection<TelegramOptions>(hostingContext.Configuration);
                        if (telegramOptions.Enabled)
                        {
                            services.AddMemoryCache();
                            services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(telegramOptions.Token));
                            services.AddSingleton<ITelegramApiClient, TelegramApiClient>();
                            services.AddSingleton<ITelegramChatClient, TelegramChatClient>();
                            services.AddSingleton<ITelegramApiListener, TelegramApiListener>();
                            services.AddSingleton<ITelegramEventProvider, TelegramEventProvider>();
                            services.AddSingleton<IChatEventProvider>(s => s.GetRequiredService<ITelegramEventProvider>());
                            services.AddHostedService<TelegramClientLauncher>();
                            services.AddTransient<IGuildUserCache, GuildUserCache>();
                            services.AddTransient<IUserMessageCache, UserMessageCache>();
                            services.AddTransient<ITelegramGuildFactory, TelegramGuildFactory>();
                            services.AddTransient<ITelegramGuildUserFactory, TelegramGuildUserFactory>();
                            services.AddTransient<ITelegramUserMessageFactory, TelegramUserMessageFactory>();
                        }
                    });
        }
    }
}