using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DontPanic.TumblrSharp;
using DontPanic.TumblrSharp.Client;
using Microsoft.Extensions.Options;
using MoreLinq;

namespace Tadmor.Services.Tumblr
{
    public class TumblrService
    {
        private static readonly Random Random = new Random();

        private readonly Dictionary<string, int> _maxPostNumbersCache = new Dictionary<string, int>();
        private TumblrClient _client;

        public TumblrService(IOptions<TumblrOptions> options)
        {
            _client = new TumblrClientFactory().Create<TumblrClient>(
                options.Value.ConsumerKey,
                options.Value.ConsumerSecret);
        }

        public async Task<string> GetRandomPost(string blogName)
        {
            if (!_maxPostNumbersCache.TryGetValue(blogName, out var postsCount))
            {
                var blogInfo = await _client.GetBlogInfoAsync(blogName);
                _maxPostNumbersCache[blogName] = postsCount = (int) blogInfo.PostsCount;
            }

            //the loop is necessary because blogInfo.PostsCount doesn't count only images
            PhotoPost post;
            do
            {
                var postNumber = Random.Next(postsCount);
                var posts = await _client.GetPostsAsync(blogName, postNumber, 20, PostType.Photo);
                post = posts.Result.OfType<PhotoPost>().FirstOrDefault();
                if (post == null) _maxPostNumbersCache[blogName] = postsCount = postNumber;
            } while (post == null && postsCount > 0);

            if (post == null) throw new Exception("no images on the blog");
            var imageUrl = post.PhotoSet.RandomSubset(1).Single().OriginalSize.ImageUrl;
            return imageUrl;
        }
    }
}