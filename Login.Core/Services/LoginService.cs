using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Login.Core.Data;
using Login.Core.Models;
using Login.Core.Exceptions;

namespace Login.Core.Services
{
  public class LoginService : ILoginService
    {
        private LoginContext context;
        private IMemoryCache _cache;
        private readonly ILogger logger;
        const int CacheTime = 60;

        public LoginService(LoginContext context, IMemoryCache cache, ILogger<LoginService> logger)
        {
            this.context = context;
            _cache = cache;
            this.logger = logger;
        }

        public async Task<User> GetUserByEmail(string email, bool noCache)
        {
            var query = from u in context.Users.Include(s => s.Sites) /* eager load the dependant entities */
                        where email.ToLower() == u.Email.ToLower() select u;
            
            if (_cache == null || noCache)
            {
                return await query.FirstOrDefaultAsync();
            }

            var user = await _cache.GetOrCreateAsync<User>(email, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromSeconds(CacheTime);
                return query.FirstOrDefaultAsync();
            });
            return user;
        }

        public async Task<UserSite> GetSiteByName(string siteName, bool noCache)
        {
            var query = from s in context.UserSites where siteName.ToLower() == s.Name.ToLower() select s;

            if (_cache == null || noCache)
            {
                return await query.FirstOrDefaultAsync();
            }

            var site = await _cache.GetOrCreateAsync<UserSite>(siteName, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromSeconds(CacheTime);
                return query.FirstOrDefaultAsync();
            });
            return site;
        }

        public async Task SaveLoginSession(string username, string displayname, Enums.LoginType loginType)
        {
            using(var tx = context.Database.BeginTransaction())
            {
                try
                {
                    context.Logins.Add(new Models.Login
                    {
                        Created = DateTime.Now,
                        Modified = DateTime.Now,
                        UserDisplayName = displayname,
                        UserName = username,
                        Type = loginType
                    });

                    await context.SaveChangesAsync();

                    tx.Commit();
                }
                catch(Exception EX)
                {
                    logger.LogError($"Could not save the login operation {EX}!");
                    tx.Rollback();

                    throw new Exceptions.ApplicationException("Could not save the login.", EX);
                }
            }
        }

        public bool IsValidRedirectUrl(User user, String siteName, String redirectUrl)
        {
            bool result = false;

            logger.LogDebug($"Find the site {siteName} and check the redirect-url {redirectUrl}");

            // find the permissions of the given user
            var query = from s in user.Sites where s.Name.ToLower() == siteName.ToLower() select s;
            if(query.Any())
            {
                var entry = query.FirstOrDefault();

                // match the given URL
                // the redirect-URL must start with the url defined for the given site
                Uri site = new Uri(entry.Url);
                Uri redirect = new Uri(redirectUrl);

                if (site.Scheme == redirect.Scheme
                        && site.Host == redirect.Host
                        && site.Port == redirect.Port)
                {

                    logger.LogDebug($"Matching of url succeeded for protocol/host/port site: {site}, redirect: {redirect}");

                    // specifically check the path
                    string sitePath = site.AbsolutePath;
                    if (string.IsNullOrEmpty(sitePath))
                    {
                        return true;
                    }
                    string redirectPath = redirect.AbsolutePath;

                    if (redirectPath.StartsWith(sitePath))
                    {
                        logger.LogDebug($"The redirect url starts with the same path as the site-url. site: {sitePath}, redirect: {redirectPath}");
                        return true;
                    }
                }
            }

            logger.LogDebug("Could not find a site with the given name or the redirect url did not match!");

            return result;
        }
    }
}
