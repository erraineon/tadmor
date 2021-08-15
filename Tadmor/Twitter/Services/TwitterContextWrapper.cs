using LinqToTwitter;
using LinqToTwitter.OAuth;
using Tadmor.Twitter.Interfaces;

namespace Tadmor.Twitter.Services
{
    public class TwitterContextWrapper : TwitterContext, ITwitterContext
    {
        public TwitterContextWrapper(IAuthorizer authorizer) : base(authorizer)
        {
        }
    }
}