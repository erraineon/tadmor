using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Tadmor.Services.Data;
using Tadmor.Services.Marriage;

namespace Tadmor.Modules
{
    [Summary("marriage")]
    public class MarriageModule : ModuleBase<ICommandContext>
    {
        private readonly AppDbContext _dbContext;
        private readonly MarriageService _marriageService;

        public MarriageModule(MarriageService marriageService, AppDbContext dbContext)
        {
            _marriageService = marriageService;
            _dbContext = dbContext;
        }

        [Summary("marries the sender with another user")]
        [Command("marry")]
        public async Task Marry(IGuildUser user)
        {
            try
            {
                await _marriageService.Marry(Context.User, user, Context.Channel, _dbContext);
                await ReplyAsync("now kiss plz");
            }
            catch (AlreadyMarriedException e)
            {
                var partner1 = await Context.Guild.GetUserAsync(e.ExistingMarriage.Partner1Id);
                var partner2 = await Context.Guild.GetUserAsync(e.ExistingMarriage.Partner2Id);
                var timeMarried = (DateTime.Now - e.ExistingMarriage.TimeStamp).Humanize();
                throw new Exception($"{partner1.Username} has already been married to {partner2.Username} for {timeMarried}");
            }
        }

        [Summary("divorces the sender with another user")]
        [Command("divorce")]
        public async Task Divorce(IGuildUser user)
        {
            await _marriageService.Divorce(Context.User, user, _dbContext);
            await ReplyAsync("ok");
        }

        [Summary("kisses another married user")]
        [Command("kiss")]
        public async Task Kiss(IGuildUser user)
        {
            var kisses = await _marriageService.Kiss(Context.User, user, _dbContext);
            await ReplyAsync($"you have kissed your partner {kisses} time(s)");
        }
    }
}