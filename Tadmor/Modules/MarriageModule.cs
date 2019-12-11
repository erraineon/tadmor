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

        [Summary("admin marry")]
        [Command("adminmarry")]
        [RequireOwner]
        public async Task AdminMarry(IGuildUser user)
        {
            await _marriageService.Marry(Context.User, user, _dbContext);
            await ReplyAsync("ok");
        }

        [Summary("admin kiss")]
        [Command("adminkiss")]
        [RequireOwner]
        public async Task AdminKisses(IGuildUser user, int kisses)
        {
            await _marriageService.SetKisses(Context.User, user, kisses, _dbContext);
            await ReplyAsync("ok");
        }

        [Summary("admin reset cd")]
        [Command("adminresetcd")]
        [RequireOwner]
        public async Task AdminKisses(IGuildUser user)
        {
            await _marriageService.ResetCooldown(Context.User, user, _dbContext);
            await ReplyAsync("ok");
        }

        [Summary("marries the sender with another user")]
        [Command("marry")]
        public async Task Marry(IGuildUser user)
        {
            try
            {
                if (user.Id == Context.Client.CurrentUser.Id) throw new Exception("😳");
                await _marriageService.DoWedding(Context.User, user, Context.Channel, _dbContext);
                await ReplyAsync("now kiss plz");
            }
            catch (AlreadyMarriedException e)
            {
                var marriage = e.ExistingMarriage;
                var formattableString = await GetStringDescription(marriage);
                throw new Exception(formattableString);
            }
        }

        [Summary("lists existing marriages")]
        [Command("marriages")]
        public async Task Marriages()
        {
            var marriages = await _marriageService.GetMarriages(Context.Guild, _dbContext);
            var marriageStrings = await Task.WhenAll(marriages
                .OrderByDescending(m => m.Kisses)
                .Select(GetStringDescription));
            await ReplyAsync(marriageStrings.Any()
                ? string.Join(Environment.NewLine, marriageStrings)
                : "no users are married");
        }

        [Summary("shows you your marriage")]
        [Command("marriage")]
        public async Task Marriage(IGuildUser user)
        {
            var marriage = await _marriageService.GetMarriage(Context.User, user, _dbContext);
            await ReplyAsync(await GetStringDescription(marriage));
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
            var kissLog = await _marriageService.Kiss(Context.User, user, _dbContext);
            await ReplyAsync(kissLog);
        }

        [Summary("makes a baby with another user")]
        [Command("baby")]
        public async Task CreateBaby(IGuildUser user, [Remainder]string babyName)
        {
            var babyLog = await _marriageService.CreateBaby(Context.User, user, babyName, _dbContext);
            await ReplyAsync(babyLog);
        }

        [Summary("get a list of babies")]
        [Command("babies")]
        public async Task Babies(IGuildUser user)
        {
            var babiesInfo = await _marriageService.GetBabiesInfo(Context.User, user, _dbContext);
            await ReplyAsync(babiesInfo);
        }

        [Summary("releases a baby")]
        [Command("release")]
        public async Task ReleaseBaby(IGuildUser user, [Remainder]string babyName)
        {
            await _marriageService.ReleaseBaby(Context.User, user, babyName, _dbContext);
            await ReplyAsync($"bye bye {babyName}!");
        }

        private async Task<string> GetStringDescription(MarriedCouple marriage)
        {
            var partner1 = await Context.Guild.GetUserAsync(marriage.Partner1Id);
            var partner2 = await Context.Guild.GetUserAsync(marriage.Partner2Id);
            var timeMarried = (DateTime.Now - marriage.TimeStamp).Humanize();
            var result = $"{partner1.Nickname ?? partner1.Username} has been married " +
                         $"to {partner2.Nickname ?? partner2.Username} " +
                         $"for {timeMarried} with {Math.Floor(marriage.Kisses)} kisses";
            return result;
        }
    }
}