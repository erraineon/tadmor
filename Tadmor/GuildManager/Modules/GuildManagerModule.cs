using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Core.Commands.Attributes;
using Tadmor.Core.Commands.Models;
using Tadmor.GuildManager.Interfaces;

namespace Tadmor.GuildManager.Modules
{
    [HideInHelp]
    public class GuildManagerModule : ModuleBase<ICommandContext>
    {
        private readonly IChannelCopier _channelCopier;

        public GuildManagerModule(IChannelCopier channelCopier)
        {
            _channelCopier = channelCopier;
        }

        [RequireOwner]
        [RequireBotPermission(GuildPermission.ReadMessageHistory)]
        [Command("copy")]
        public async Task<RuntimeResult> Copy(ITextChannel destination, params ITextChannel[] sources)
        {
            return await Copy(destination, sources, false);
        }

        [RequireOwner]
        [RequireBotPermission(GuildPermission.ReadMessageHistory)]
        [Command("copy images")]
        public async Task<RuntimeResult> CopyImages(ITextChannel destination, params ITextChannel[] sources)
        {
            return await Copy(destination, sources, true);
        }

        private async Task<RuntimeResult> Copy(ITextChannel destination, ITextChannel[] sources, bool onlyImages)
        {
            var copiedCount = await _channelCopier.CopyAsync(sources, destination, onlyImages);
            return CommandResult.FromSuccess($"successfully copied {copiedCount} messages", true);
        }
    }
}