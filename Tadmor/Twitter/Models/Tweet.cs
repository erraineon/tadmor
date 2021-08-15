namespace Tadmor.Twitter.Models
{
    public record Tweet
    {
        public ulong Id { get; set; }
        public string AuthorName { get; set; }
        public string Status { get; set; }
        public bool HasMedia { get; set; }
    }
}