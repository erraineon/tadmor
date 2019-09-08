using System;
using Discord;

namespace Tadmor.Services.Telegram
{
    public class TelegramApplication : IApplication
    {
        public TelegramApplication(int ownerId)
        {
            Owner = new TelegramApplicationOwner(ownerId);
        }

        public ulong Id { get; }
        public DateTimeOffset CreatedAt { get; }
        public string Name { get; }
        public string Description { get; }
        public string[] RPCOrigins { get; }
        public ulong Flags { get; }
        public string IconUrl { get; }
        public IUser Owner { get; }
    }
}