using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.Extensions;
using Tadmor.Twitter.Interfaces;
using Tadmor.Twitter.Models;
using Tadmor.Twitter.Modules;
using Tadmor.Twitter.Services;

namespace Tadmor.Twitter.Extensions
{
    public static class TwitterRegistrationExtensions
    {
        public static IHostBuilder UseTwitter(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices((hostingContext, services) =>
                {
                    var config = services.BindConfigurationSection<TwitterOptions>(hostingContext.Configuration);
                    AssertConfigurationValid(config);
                    services.AddTransient<ITweetProvider, TweetProvider>();
                    services.Decorate<ITweetProvider, CachedTweetProviderDecorator>();
                    services.AddTransient<ITwitterContextFactory, TwitterContextFactory>();
                    services.Decorate<ITwitterContextFactory, CachedTwitterContextFactoryDecorator>();
                    services.AddTransient<IRandomTweetProvider, RandomTweetProvider>();
                    services.AddTransient<IImageTweetSender, ImageTweetSender>();
                })
                .UseModule<TwitterModule>();
        }

        private static void AssertConfigurationValid(TwitterOptions configuration)
        {
            if (configuration.ConsumerKey == default ||
                configuration.ConsumerSecret == default ||
                configuration.OAuthToken == default ||
                configuration.OAuthTokenSecret == default)
            {
                const string msg = "to use the twitter module you must specify " +
                                   "the service address in the options";
                throw new Exception(msg);
            }
        }
    }
}