using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MoreLinq.Extensions;

namespace Tadmor.Modules
{
    public class RandomModule : ModuleBase<ICommandContext>
    {
        private static readonly Random Random = new Random();

        [Command("roll")]
        public Task Roll(int max = 2) => ReplyAsync((Random.Next(max) + 1).ToString());

        [Command("someone")]
        public async Task Someone(IRole role = default)
        {
            var users = (await Context.Channel.GetUsersAsync().FlattenAsync()).OfType<IGuildUser>();
            if (role != default) users = users.Where(u => u.RoleIds.Contains(role.Id));
            var user = users.RandomSubset(1).Single();
            await ReplyAsync(user.Mention);
        }
    }
}