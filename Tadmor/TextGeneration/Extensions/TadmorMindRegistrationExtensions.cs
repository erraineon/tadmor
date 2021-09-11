using System;
using Flurl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.Commands.Models;
using Tadmor.Core.Extensions;
using Tadmor.Core.Notifications.Interfaces;
using Tadmor.TextGeneration.Interfaces;
using Tadmor.TextGeneration.Models;
using Tadmor.TextGeneration.Modules;
using Tadmor.TextGeneration.Services;

namespace Tadmor.TextGeneration.Extensions
{
    public static class TadmorMindRegistrationExtensions
    {
        public static IHostBuilder UseTadmorMind(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices((hostingContext, services) =>
                {
                    var config = services.BindConfigurationSection<Gpt3TadmorMindOptions>(hostingContext.Configuration);
                    if (config.Enabled)
                    {
                        AssertConfigurationValid(config);
                        services.AddSingleton<ITadmorMindThoughtsRepository, TadmorMindThoughtsRepository>();
                        services.AddHostedService<TadmorMindThoughtProducer>();
                        services.AddTransient<ITadmorMindClient, Gpt3TadmorMindClient>();
                        services.AddTransient<INotificationHandler<MessageValidatedNotification>, GenerateWhenRepliedToBehaviour>();
                        services.UseModule<TadmorMindModule>();
                    }
                });
        }

        private static void AssertConfigurationValid(Gpt3TadmorMindOptions configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.ApiKey) || string.IsNullOrWhiteSpace(configuration.ModelName))
            {
                const string msg = "to use the tadmor mind module you must specify " +
                                   "the openai api key and model name in the options";
                throw new Exception(msg);
            }
        }
    }
}