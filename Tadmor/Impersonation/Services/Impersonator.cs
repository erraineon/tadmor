using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Flurl.Http;
using Tadmor.Impersonation.Interfaces;

namespace Tadmor.Impersonation.Services
{
    public class Impersonator : IImpersonator
    {
        private readonly ICommandContext _commandContext;

        public Impersonator(ICommandContext commandContext)
        {
            _commandContext = commandContext;
        }

        public async Task WhileImpersonating(IDiscordClient chatClient, IUser target, Func<Task> asScope)
        {
            // doesn't really work, rate limit prevents any more than two switches every 10 minutes
            var currentUser = chatClient.CurrentUser;
            var currentGuildUser = await _commandContext.Guild.GetCurrentUserAsync();

            var originalIcon = await GetIconAsync(currentUser);
            var originalNickname = currentGuildUser.Nickname;

            var targetIcon = await GetIconAsync(target);
            var targetNickname = target is IGuildUser { Nickname: var nickname and not null } ? nickname : target.Username;
            await WhileImpersonating(originalIcon, targetIcon, originalNickname, targetNickname, currentUser, currentGuildUser, asScope);
        }

        private static async Task WhileImpersonating(
            Optional<Image?> originalIcon, 
            Optional<Image?> targetIcon, 
            string originalNickname,
            string targetNickname, 
            ISelfUser currentUser, 
            IGuildUser currentGuildUser, 
            Func<Task> asScope)
        {
            var iconDiffers = !Equals(targetIcon, originalIcon);
            var nicknameDiffers = originalNickname != targetNickname;
            if (iconDiffers)
            {
                await currentUser.ModifyAsync(properties => { properties.Avatar = targetIcon; });
            }

            if (nicknameDiffers)
            {
                await currentGuildUser.ModifyAsync(u => u.Nickname = targetNickname);
            }

            try
            {
                await asScope();
            }
            finally
            {
                if (iconDiffers)
                {
                    await currentUser.ModifyAsync(properties => { properties.Avatar = originalIcon; });
                }

                if (nicknameDiffers)
                {
                    await currentGuildUser.ModifyAsync(u => u.Nickname = originalNickname);
                }
            }
        }

        private static async Task<Optional<Image?>> GetIconAsync(IUser user)
        {
            var originalIconUrl = user.GetAvatarUrl();
            var originalIcon = originalIconUrl != null
                ? Optional.Create<Image?>(new Image(new MemoryStream(await originalIconUrl.GetBytesAsync())))
                : Optional<Image?>.Unspecified;
            return originalIcon;
        }
    }
}
