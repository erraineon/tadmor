using System.Threading.Tasks;

namespace Tadmor.Twitter.Interfaces
{
    public interface IImageTweetSender
    {
        Task<string> TweetImageAsync(byte[] image);
    }
}