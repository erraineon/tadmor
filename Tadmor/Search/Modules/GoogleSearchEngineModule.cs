using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Commands.Models;
using Tadmor.Search.Interfaces;

namespace Tadmor.Search.Modules
{
    [Summary("search engine")]
    public class GoogleSearchEngineModule : ModuleBase<ICommandContext>
    {
        private readonly IGoogleSearchEngine _googleSearchEngine;

        public GoogleSearchEngineModule(IGoogleSearchEngine googleSearchEngine)
        {
            _googleSearchEngine = googleSearchEngine;
        }

        [Summary("search on google")]
        [Command("g")]
        public async Task<RuntimeResult> FindWebpageAsync([Remainder] string query)
        {
            var webpageLink = await _googleSearchEngine.FindWebpageLinkAsync(query) ??
                              throw new ModuleException("no results");
            return CommandResult.FromSuccess(webpageLink);
        }

        [Summary("search on google images")]
        [Command("gi")]
        public async Task<RuntimeResult> FindImageAsync([Remainder] string query)
        {
            var imageLink = await _googleSearchEngine.FindImageLinkAsync($"{query} -site:me.me") ??
                            throw new ModuleException("no results");
            return CommandResult.FromSuccess(imageLink);
        }
    }
}