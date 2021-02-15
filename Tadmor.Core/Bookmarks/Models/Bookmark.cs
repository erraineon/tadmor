namespace Tadmor.Core.Bookmarks.Models
{
    public class Bookmark
    {
        public string ChatClientId { get; set; } = null!;
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public string Key { get; set; } = null!;
        public string LastSeenValue { get; set; } = null!;
    }
}