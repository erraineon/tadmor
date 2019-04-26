using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Tadmor.Services.Discord
{
    public class GreetingService : IHostedService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;

        public GreetingService(DiscordSocketClient discord, IServiceProvider services)
        {
            _discord = discord;
            _services = services;
        }

        private async Task GreetUser(SocketGuildUser arg)
        {
            using (var scope = _services.CreateScope())
            {
                var discordOptions = scope.ServiceProvider.GetService<IOptionsSnapshot<DiscordOptions>>().Value;
                var guildOptions = discordOptions.GuildOptions.SingleOrDefault(options => options.Id == arg.Guild.Id);
                var welcomeMessage = guildOptions?.WelcomeMessage;
                if (!string.IsNullOrWhiteSpace(welcomeMessage))
                    await arg.Guild.DefaultChannel.SendMessageAsync(welcomeMessage);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _discord.UserJoined += GreetUser;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _discord.UserJoined -= GreetUser;
            return Task.CompletedTask;
        }
    }
}