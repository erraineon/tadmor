﻿using Discord;
using Discord.Commands;
using Tadmor.ChatClients.Abstractions.Interfaces;

namespace Tadmor.Commands.Interfaces
{
    public interface ICommandContextFactory
    {
        ICommandContext Create(
            string command,
            IGuildChannel executeIn,
            IUser executeAs,
            IChatClient chatClient,
            IUserMessage? referencedMessage);
    }
}