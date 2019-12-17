using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Tadmor.Services.Data;
using Tadmor.Services.Marriage;
using Tadmor.Utils;

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
            await _marriageService.Marry(Context.User, user);
            await ReplyAsync("ok");
        }

        [Summary("admin kiss")]
        [Command("adminkiss")]
        [RequireOwner]
        public async Task AdminKisses(IGuildUser user, int kisses)
        {
            await _marriageService.SetKisses(Context.User, user, kisses);
            await ReplyAsync("ok");
        }

        [Summary("admin reset cd")]
        [Command("adminresetcd")]
        [RequireOwner]
        public async Task AdminKisses(IGuildUser user)
        {
            await _marriageService.ResetCooldown(Context.User, user);
            await ReplyAsync("ok");
        }

        [Summary("marries the sender with another user")]
        [Command("marry")]
        public async Task Marry(IGuildUser user)
        {
            try
            {
                if (user.Id == Context.Client.CurrentUser.Id) throw new Exception("😳");
                await _marriageService.DoWedding(Context.User, user);
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
            var marriages = await _marriageService.GetMarriages(Context.Guild);
            var marriageStrings = await Task.WhenAll(marriages
                .OrderByDescending(m => m.Kisses)
                .Select(GetStringDescription));
            await ReplyAsync(marriageStrings.Any()
                ? string.Join(Environment.NewLine, marriageStrings)
                : "no users are married");
        }

        [Summary("shows you your marriage")]
        [Command("marriage")]
        public async Task Marriage([ShowAsOptional] IGuildUser? user)
        {
            var marriage = await _marriageService.GetMarriage(Context.User, user);
            await ReplyAsync(await GetStringDescription(marriage));
        }

        [Summary("shows you your marriage")]
        [Command("marriage")]
        [Browsable(false)]
        public async Task Marriage()
        {
            await Marriage(default);
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
        [Priority(1)]
        public async Task CreateBaby([ShowAsOptional] IGuildUser? user, [Remainder]string babyName)
        {
            var babyLog = await _marriageService.CreateBaby(Context.User, user, babyName);
            await ReplyAsync(babyLog);
        }

        [Summary("makes a baby with another user")]
        [Command("baby")]
        [Browsable(false)]
        public async Task CreateBaby([Remainder]string babyName)
        {
            await CreateBaby(default, babyName);
        }

        [Summary("get a list of babies")]
        [Command("babies")]
        public async Task Babies([ShowAsOptional] IGuildUser? user)
        {
            var babiesInfo = await _marriageService.GetMarriageInfo(Context.User, user);
            await ReplyAsync(babiesInfo);
        }

        [Summary("get a list of babies")]
        [Command("babies")]
        [Browsable(false)]
        public async Task Babies()
        {
            await Babies(null);
        }

        [Summary("releases a baby")]
        [Command("release")]
        [Priority(1)]
        public async Task ReleaseBaby([ShowAsOptional] IGuildUser? user, [Remainder]string babyName)
        {
            await _marriageService.ReleaseBaby(Context.User, user, babyName);
            await ReplyAsync($"bye bye {babyName}!");
        }

        [Summary("releases a baby")]
        [Command("release")]
        [Browsable(false)]
        public async Task ReleaseBaby([Remainder]string babyName)
        {
            await ReleaseBaby(default, babyName);
        }

        [Summary("combines two babies")]
        [Command("combine")]
        [Priority(1)]
        public async Task CombineBabies([ShowAsOptional] IGuildUser? user, string babyName1, string babyName2, [Remainder] string newBabyName)
        {
            var combineLog = await _marriageService.CombineBabies(Context.User, user, babyName1, babyName2, newBabyName);
            await ReplyAsync(combineLog);
        }

        [Summary("combines two babies")]
        [Command("combine")]
        [Browsable(false)]
        public async Task CombineBabies(string babyName1, string babyName2, [Remainder] string newBabyName)
        {
            await CombineBabies(default, babyName1, babyName2, newBabyName);
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