using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.Extensions;
using Tadmor.Search.Interfaces;
using Tadmor.Search.Models;
using Tadmor.Search.Modules;
using Tadmor.Search.Services;

namespace Tadmor.Search.Extensions
{
    public static class GoogleRegistrationExtensions
    {
        public static IHostBuilder UseGoogleSearch(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices((hostingContext, services) =>
                {
                    var config = services.BindConfigurationSection<GoogleOptions>(hostingContext.Configuration);
                    AssertConfigurationValid(config);
                    services.AddTransient<IGoogleClient, GoogleClientWrapper>();
                    services.AddTransient<IGoogleSearchEngine, GoogleSearchEngine>();
                })
                .UseModule<GoogleSearchEngineModule>();
        }

        private static void AssertConfigurationValid(GoogleOptions configuration)
        {
            var configurationInvalid = string.IsNullOrWhiteSpace(configuration.ApiKey) ||
                                       string.IsNullOrWhiteSpace(configuration.SearchEngineId);
            if (configurationInvalid)
            {
                const string msg = "to use the google module you must specify " +
                                   "an api key and a search engine id in the options";
                throw new Exception(msg);
            }
        }
    }
}