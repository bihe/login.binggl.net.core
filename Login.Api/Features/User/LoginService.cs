using System;
using System.Linq;
using System.Threading.Tasks;
using Models = Login.Api.Features.Shared.Models;
using Login.Api.Features.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Login.Api.Features.User
{
    public class LoginService : ILoginService
    {
        private LoginContext _context;
        private IMemoryCache _cache;
        private readonly ILogger _logger;
        const int CacheTime = 60;
        private static readonly object _lock = new object();

        public LoginService(LoginContext context, IMemoryCache cache, ILogger<LoginService> logger)
        {
            this._context = context;
            this._cache = cache;
            this._logger = logger;
        }

        public async Task<Models.User> GetUserByEmail(string email, bool noCache)
        {
            var query = from u in _context.Users.Include(s => s.Sites) /* eager load the dependant entities */
                        where email.ToLower() == u.Email.ToLower() select u;

            if (_cache == null || noCache)
            {
                return await query.FirstOrDefaultAsync();
            }

            var user = await _cache.GetOrCreateAsync<Models.User>(email, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromSeconds(CacheTime);
                return query.FirstOrDefaultAsync();
            });
            return user;
        }

        public async Task<Models.UserSite> GetSiteById(string id, bool noCache)
        {
            var query = from s in _context.UserSites where s.Id == id select s;

            if (_cache == null || noCache)
            {
                return await query.FirstOrDefaultAsync();
            }

            var site = await _cache.GetOrCreateAsync<Models.UserSite>(id, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromSeconds(CacheTime);
                return query.FirstOrDefaultAsync();
            });
            return site;
        }

        public void SaveLoginSession(string username, string displayname, Models.LoginType loginType)
        {
            lock(_lock)
            {
                // sqlite is not that happy with concurrent processes
                using(var tx = _context.Database.BeginTransaction())
                {
                    try
                    {
                        _context.Logins.Add(new Models.Login
                        {
                            Created = DateTime.Now,
                            Modified = DateTime.Now,
                            UserDisplayName = displayname,
                            UserName = username,
                            Type = loginType
                        });

                        var awaiter = _context.SaveChangesAsync();
                        awaiter.Wait();
                        tx.Commit();
                    }
                    catch(Exception EX)
                    {
                        _logger.LogError($"Could not save the login operation {EX}!");
                        tx.Rollback();

                        throw new Shared.Exceptions.ApplicationException("Could not save the login.", EX);
                    }
                }
            }

        }

        public bool SaveSiteList(List<Models.UserSite> sitesToSave, List<Models.UserSite> sitesToDelete)
        {
            bool result = false;
            lock(_lock)
            {
                // sqlite is not that happy with concurrent processes
                using (var tx = _context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var site in sitesToSave)
                        {
                            if (string.IsNullOrEmpty(site.Id))
                            {
                                _context.UserSites.Add(site);
                            }
                        }

                        if (sitesToDelete != null)
                        {
                            foreach (var site in sitesToDelete)
                            {
                                _context.UserSites.RemoveRange(sitesToDelete);
                            }
                        }
                        var awaiter = _context.SaveChangesAsync();
                        awaiter.Wait();
                        tx.Commit();

                        // clear the cache for sites
                        if (this._cache != null)
                        {
                            sitesToSave.ForEach(s => this._cache.Remove(s.Id));
                            this._cache.Remove(sitesToSave[0].User.Email);
                        }
                        result = true;
                    }
                    catch (Exception EX)
                    {
                        _logger.LogError($"Could not save the site-list {EX}!");
                        tx.Rollback();

                        throw new Shared.Exceptions.ApplicationException("Could not save the site-list.", EX);
                    }
                }
            }

            return result;
        }

        public bool IsValidRedirectUrl(Models.User user, String siteName, String redirectUrl)
        {
            bool result = false;

            _logger.LogDebug($"Find the site {siteName} and check the redirect-url {redirectUrl}");

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

                    _logger.LogDebug($"Matching of url succeeded for protocol/host/port site: {site}, redirect: {redirect}");

                    // specifically check the path
                    string sitePath = site.AbsolutePath;
                    if (string.IsNullOrEmpty(sitePath))
                    {
                        return true;
                    }
                    string redirectPath = redirect.AbsolutePath;

                    if (redirectPath.StartsWith(sitePath))
                    {
                        _logger.LogDebug($"The redirect url starts with the same path as the site-url. site: {sitePath}, redirect: {redirectPath}");
                        return true;
                    }
                }
            }

            _logger.LogDebug("Could not find a site with the given name or the redirect url did not match!");

            return result;
        }
    }
}
