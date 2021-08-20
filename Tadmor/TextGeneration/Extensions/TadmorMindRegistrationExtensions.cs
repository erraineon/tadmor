﻿using System;
using Flurl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.Extensions;
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
                    var config = services.BindConfigurationSection<TadmorMindOptions>(hostingContext.Configuration);
                    AssertConfigurationValid(config);
                    services.AddTransient<ITadmorMindClient, TadmorMindClient>();
                })
                .UseModule<TadmorMindModule>();
        }

        private static void AssertConfigurationValid(TadmorMindOptions configuration)
        {
            if (!Url.IsValid(configuration.ServiceAddress))
            {
                const string msg = "to use the tadmor mind module you must specify " +
                                   "the service address in the options";
                throw new Exception(msg);
            }
        }
    }
}