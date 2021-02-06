using System;
using Discord;
using Telegram.Bot.Types;

namespace Tadmor.ChatClients.Telegram.Models
{
    public class TelegramApplication : IApplication
    {
        public TelegramApplication(int ownerId)
        {
            Owner = new TelegramUser(new User {Id = ownerId});
        }

        public string Name => throw new NotImplementedException();
        public string Description => throw new NotImplementedException();
        public string[] RPCOrigins => throw new NotImplementedException();
        public ulong Flags => throw new NotImplementedException();
        public string IconUrl => throw new NotImplementedException();
        public bool IsBotPublic => throw new NotImplementedException();
        public bool BotRequiresCodeGrant => throw new NotImplementedException();
        public ITeam Team => throw new NotImplementedException();
        public IUser Owner { get; }
        public ulong Id => throw new NotImplementedException();
        public DateTimeOffset CreatedAt => throw new NotImplementedException();
    }
}