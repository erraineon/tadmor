using System;
using System.Linq;
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
                if (user.Id == Context.Client.CurrentUser.Id) throw new Exception("😳");
                await _marriageService.Marry(Context.User, user, Context.Channel, _dbContext);
                await ReplyAsync("now kiss plz");
            }
            catch (AlreadyMarriedException e)
            {
                var marriage = e.ExistingMarriage;
                var formattableString = await GetStringDescription(marriage);
                throw new Exception(formattableString);
            }
        }

        private async Task<string> GetStringDescription(MarriedCouple marriage)
        {
            var partner1 = await Context.Guild.GetUserAsync(marriage.Partner1Id);
            var partner2 = await Context.Guild.GetUserAsync(marriage.Partner2Id);
            var timeMarried = (DateTime.Now - marriage.TimeStamp).Humanize();
            var result = $"{partner1.Username} has been married to {partner2.Username} " +
                         $"for {timeMarried} with {marriage.Kisses} kisses";
            return result;
        }

        [Summary("lists existing marriages")]
        [Command("marriages")]
        public async Task Marriages()
        {
            var marriages = await _marriageService.GetMarriages(Context.Guild, _dbContext);
            var marriageStrings = await Task.WhenAll(marriages.Select(GetStringDescription));
            await ReplyAsync(marriageStrings.Any()
                ? string.Join(Environment.NewLine, marriageStrings)
                : "no users are married");
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