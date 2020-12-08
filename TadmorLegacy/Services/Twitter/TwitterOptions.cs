namespace Tadmor.Services.Twitter
{
    [Options]
    public class TwitterOptions
    {
        public string? ConsumerKey { get; set; }
        public string? ConsumerSecret { get; set; }
        public string? OAuthToken { get; set; }
        public string? OAuthTokenSecret { get; set; }
    }
}