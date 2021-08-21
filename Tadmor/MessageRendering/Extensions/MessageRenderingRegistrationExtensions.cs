using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.Extensions;
using Tadmor.MessageRendering.Interfaces;
using Tadmor.MessageRendering.Modules;
using Tadmor.MessageRendering.Services;

namespace Tadmor.MessageRendering.Extensions
{
    public static class MessageRenderingRegistrationExtensions
    {
        public static IHostBuilder UseMessageRendering(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services =>
                {
                    services.AddTransient<IMessageRenderer, MessageRenderer>();
                    services.AddTransient<IDrawableMessageFactory, DrawableMessageFactory>();
                    services.AddTransient<IImageProvider, ImageProvider>();
                })
                .UseModule<MessageRendererModule>();
        }
    }
}