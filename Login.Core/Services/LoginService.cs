using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Login.Core.Data;
using Login.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace Login.Core.Services
{
  public class LoginService : ILoginService
    {
        private LoginContext context;
        private IMemoryCache _cache;
        const int CacheTime = 60;

        public LoginService(LoginContext context, IMemoryCache cache)
        {
            this.context = context;
            _cache = cache;
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
    }
}
