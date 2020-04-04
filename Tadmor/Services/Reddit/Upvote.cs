namespace Tadmor.Services.Reddit
{
    public class Upvote
    {
        public ulong TargetUserId { get; set; }
        public ulong GuildId { get; set; }
        public ulong MessageId { get; set; }
        public ulong VoterId { get; set; }
    }
}