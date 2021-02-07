using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.Extensions;
using Tadmor.GuildManager.Interfaces;
using Tadmor.GuildManager.Modules;
using Tadmor.GuildManager.Services;

namespace Tadmor.GuildManager.Extensions
{
    public static class GuildManagerRegistrationExtensions
    {
        public static IHostBuilder UseGuildManager(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services => services.AddTransient<IChannelCopier, ChannelCopier>())
                .UseModule<GuildManagerModule>();
        }
    }
}