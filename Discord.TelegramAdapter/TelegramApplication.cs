using System;
using Telegram.Bot.Types;

namespace Discord.TelegramAdapter
{
    public class TelegramApplication : IApplication
    {
        public TelegramApplication(int ownerId)
        {
            Owner = new TelegramUser(new User {Id = ownerId});
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