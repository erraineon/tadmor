using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Core.Commands.Attributes;
using Tadmor.Impersonation.Interfaces;

namespace Tadmor.Impersonation
{
    public class ImpersonationModule : ModuleBase<ICommandContext>
    {
        private readonly IImpersonator _impersonator;

        public ImpersonationModule(IImpersonator impersonator)
        {
            _impersonator = impersonator;
        }

        [Command("mimic")]
        [RequireWhitelist]
        public async Task Mimic(IUser user, [Remainder] string message)
        {
            await _impersonator.WhileImpersonating(Context.Client, user,
                () => Context.Channel.SendMessageAsync(message));
        }
    }
}
