namespace Tadmor.Services.Reddit
{
    public class Upvote
    {
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public int UpvotesCount { get; set; }
    }
}