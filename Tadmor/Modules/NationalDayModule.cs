using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Services.NationalDay;

namespace Tadmor.Modules
{
    [Summary("national day")]
    public class NationalDayModule : ModuleBase<ICommandContext>
    {
        private readonly NationalDayService _nationalDay;

        public NationalDayModule(NationalDayService nationalDay)
        {
            _nationalDay = nationalDay;
        }

        [Summary("gets today's national holidays")]
        [Command("holidays")]
        public async Task TodaysHolidays()
        {
            var holidays = await _nationalDay.GetTodaysHolidays();
            if (!holidays.Any()) throw new Exception("no national holidays for today");
            var embedBuilder = new EmbedBuilder();
            embedBuilder.Fields.AddRange(holidays
                .Select(h => new EmbedFieldBuilder()
                    .WithIsInline(true)
                    .WithName("\u200b")
                    .WithValue($"[{h.Name}]({h.Url})")));
            var embed = embedBuilder.Build();
            await ReplyAsync(string.Empty, embed: embed);
        }
    }
}