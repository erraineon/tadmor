using FChan.Library;

namespace Tadmor.Services.FourChan
{
    public class ChanPost
    {
        private readonly Post _post;

        public ChanPost(Post post, int repliesCount, int threadNumber, string threadSubject)
        {
            _post = post;
            Replies = repliesCount;
            var url = $"https://boards.4chan.org/{post.Board}/thread/{threadNumber}";
            if (post.PostNumber != threadNumber) url += $"#p{post.PostNumber}";
            Url = url;
            Name = threadSubject;
        }

        public string? Name { get; }
        public string Url { get; }
        public int Replies { get; }
        public string? Comment => _post.Comment;
        public string OriginalFileName => _post.OriginalFileName;
        public string FileExtension => _post.FileExtension;
    }
}