using System.ComponentModel.DataAnnotations.Schema;

namespace Tadmor.Services.Twitter
{
    public class TwitterMedia
    {
        public ulong TweetId { get; set; }
        public ulong MediaId { get; set; }

        public string Username { get; set; }
        public string Url { get; set; }
        public string StatusText { get; set; }
    }
}