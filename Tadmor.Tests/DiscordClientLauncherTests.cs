using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tadmor.Core.ChatClients.Discord.Interfaces;
using Tadmor.Core.ChatClients.Discord.Models;
using Tadmor.Core.ChatClients.Discord.Services;

namespace Tadmor.Tests
{
    [TestClass]
    public class DiscordClientLauncherTests
    {
        private IDiscordChatClient _discordChatClient;
        private DiscordOptions _discordOptions;
        private DiscordClientLauncher _sut;

        [TestInitialize]
        public void Initialize()
        {
            _discordChatClient = Substitute.For<IDiscordChatClient>();
            _discordOptions = new DiscordOptions("foo", true);
            _sut = new DiscordClientLauncher(_discordChatClient, _discordOptions);
        }

        [TestMethod]
        public async Task If_Enabled_Works()
        {
            var startTask = _sut.StartAsync(CancellationToken.None);
            _discordChatClient.Ready += Raise.Event<Func<Task>>();
            await startTask;
            await _discordChatClient.Received().LoginAsync(TokenType.Bot, "foo", true);
            await _discordChatClient.Received().StartAsync();
            await _sut.StopAsync(CancellationToken.None);
            await _discordChatClient.Received().StopAsync();
        }
    }
}