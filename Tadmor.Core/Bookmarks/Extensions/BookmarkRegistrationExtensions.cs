using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tadmor.Core.Bookmarks.Interfaces;
using Tadmor.Core.Bookmarks.Services;

namespace Tadmor.Core.Bookmarks.Extensions
{
    public static class BookmarkRegistrationExtensions
    {
        public static IHostBuilder UseBookmarks(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services => services
                    .AddTransient<IBookmarkRepository, BookmarkRepository>());
        }
    }
}