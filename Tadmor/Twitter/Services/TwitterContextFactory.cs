using System.Linq;
using System.Threading.Tasks;
using LinqToTwitter;
using LinqToTwitter.OAuth;
using Tadmor.Twitter.Interfaces;
using Tadmor.Twitter.Models;

namespace Tadmor.Twitter.Services
{
    public class TwitterContextFactory : ITwitterContextFactory
    {
        private readonly TwitterOptions _twitterOptions;

        public TwitterContextFactory(TwitterOptions twitterOptions)
        {
            _twitterOptions = twitterOptions;
        }

        public async Task<TwitterContext> CreateAsync()
        {
            var authorizer = new SingleUserAuthorizer
            {
                CredentialStore = new InMemoryCredentialStore
                {
                    ConsumerKey = _twitterOptions.ConsumerKey,
                    ConsumerSecret = _twitterOptions.ConsumerSecret,
                    OAuthToken = _twitterOptions.OAuthToken,
                    OAuthTokenSecret = _twitterOptions.OAuthTokenSecret,
                }
            };

            await authorizer.AuthorizeAsync();
            var context = new TwitterContext(authorizer);
            var verifyResponse = await context.Account
                .Where(acct => acct.Type == AccountType.VerifyCredentials)
                .SingleOrDefaultAsync();
            if (verifyResponse?.User is { } user)
            {
                authorizer.CredentialStore.ScreenName = user.ScreenNameResponse;
            }
            return context;
        }
    }
}