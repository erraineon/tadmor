using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Tadmor.Services.Hangfire;
using Tadmor.Utils;

namespace Tadmor.Services.Discord
{
    public class CommandJob : IHangfireJob<CommandJobOptions>
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly DiscordService _discordService;

        public CommandJob(DiscordSocketClient discordClient, DiscordService discordService)
        {
            _discordClient = discordClient;
            _discordService = discordService;
        }

        [CancelRecurrenceUponFailure]
        public async Task Do(CommandJobOptions options)
        {
            var channel = _discordClient.GetChannel(options.ChannelId) as IMessageChannel ??
                          throw new Exception("channel gone, delete schedule");
            var owner = _discordClient.GetUser(options.OwnerId);
            var message = new ServiceUserMessage(channel, owner, options.Command);
            var context = new CommandContext(_discordClient, message);
            await _discordService.ExecuteCommand(context, string.Empty);
        }
    }
}