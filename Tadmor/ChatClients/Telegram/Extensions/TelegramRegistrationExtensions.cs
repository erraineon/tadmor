using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.ChatClients.Abstractions.Interfaces;
using Tadmor.ChatClients.Telegram.Interfaces;
using Tadmor.ChatClients.Telegram.Models;
using Tadmor.ChatClients.Telegram.Services;
using Tadmor.Extensions;
using Telegram.Bot;

namespace Tadmor.ChatClients.Telegram.Extensions
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
                            services.AddSingleton<ITelegramBotClient>(
                                _ => new TelegramBotClient(telegramOptions.Token));
                            services.AddSingleton<ITelegramApiClient, TelegramApiClient>();
                            services.AddSingleton<ITelegramClient, TelegramClient>();
                            services.AddSingleton<ITelegramApiListener, TelegramApiListener>();
                            services.AddSingleton<ITelegramEventProvider, TelegramEventProvider>();
                            services.AddSingleton<IChatEventProvider>(
                                s => s.GetRequiredService<ITelegramEventProvider>());
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