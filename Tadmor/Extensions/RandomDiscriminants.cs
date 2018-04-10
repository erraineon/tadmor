using System;

namespace Tadmor.Extensions
{
    [Flags]
    public enum RandomDiscriminants
    {
        UserId = 1 << 0,
        GuildId = 1 << 1,
        Nickname = 1 << 2,
        AvatarId = 1 << 3
    }
}