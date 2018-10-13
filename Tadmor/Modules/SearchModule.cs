using System;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Services.CustomSearch;

namespace Tadmor.Modules
{
    [Summary("search engine")]
    public class SearchModule : ModuleBase<ICommandContext>
    {
        private readonly CustomSearchService _search;

        public SearchModule(CustomSearchService search)
        {
            _search = search;
        }

        private async Task<string> Search(string query, bool image)
        {
            return await _search.SearchFirst(query, image) ?? throw new Exception("no results");
        }

        [Summary("search on google")]
        [Command("g")]
        public async Task Search([Remainder] string query)
        {
            await ReplyAsync(await Search(query, false));
        }

        [Summary("search on google images")]
        [Command("gi")]
        public async Task SearchImage([Remainder] string query)
        {
            await ReplyAsync(await Search(query, true));
        }
    }
}