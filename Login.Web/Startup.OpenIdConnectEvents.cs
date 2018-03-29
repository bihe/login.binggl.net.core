using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Login.Core.Configuration;
using Login.Core.Models;
using Login.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Login.Web
{
    /// <summary>
    /// use a partial class to somehow organize the startup file
    /// </summary>
    public partial class Startup
    {
        Task OnAuthenticationFailed(RemoteFailureContext context)
        {
            context.HandleResponse();
            var message = context?.Failure?.Message ?? "Remote authentication error!";
            return Task.FromException(new SecurityTokenValidationException(message));
        }

        Task OnRedirectToIdentityProviderForSignOut(RedirectContext context)
        {
            context.HandleResponse();
            context.Response.Redirect("/logoff");
            return Task.FromResult(0);
        }

        Task PerformPostTokenValidationAuthorization(TokenValidatedContext context)
        {
            var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
            var externalLookupEmail = claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value;

            logger.LogDebug("Final phase of token-validation; check supplied identiy with database entries. Supplied: {0}", externalLookupEmail);

            User lookupUser = null;

            this.UseService((provider) =>
            {
                var loginService = provider.GetRequiredService<ILoginService>();
                var awaiter = loginService.GetUserByEmail(externalLookupEmail);
                awaiter.Wait();
                lookupUser = awaiter.Result;
            });

            logger.LogDebug("User from lookup: {0}", lookupUser);

            if (lookupUser == null)
            {
                logger.LogWarning("Could not find a user by email {0}", externalLookupEmail);
                return Task.FromException(new SecurityTokenValidationException("Supplied user is not allowed to access the system!"));
            }


            // a valid user has the role USER
            // if there is a permission for the `this` site - assign the ADMIN Role
            var appSiteUrl = this.Configuration["Application:Url"];
            var appSiteName = this.Configuration["Application:Name"];
            var userPermissionRole = Constants.ROLE_USER;
            var siteQuery = from s in lookupUser.Sites where s.Url == appSiteUrl &&
                s.Name == appSiteName && s.Permissions.IndexOf(Constants.ROLE_ADMIN) > -1 select s;
            if (siteQuery.Any())
            {
                userPermissionRole = Constants.ROLE_ADMIN;
            }

            IList<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, lookupUser.DisplayName)
                , new Claim(ClaimTypes.Sid,  claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? "")
                , new Claim(ClaimTypes.GivenName,  claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value ?? "")
                , new Claim(ClaimTypes.Surname, claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value ?? "")
                , new Claim(ClaimTypes.Email, claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value ?? "")
                , new Claim(ClaimTypes.Role, userPermissionRole)
            };

            logger.LogDebug("User {0} has {1} sites assigned.", externalLookupEmail, lookupUser?.Sites?.Count ?? 0);

            var identity = new ClaimsIdentity(claims, OpenIdConnectDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            context.Principal = principal;

            return Task.FromResult(0);
        }

        void UseService(Action<IServiceProvider> action)
        {
            using (var serviceProvider = _serviceColletion.BuildServiceProvider())
            {
                var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
                using (var scope = scopeFactory.CreateScope())
                {
                    var provider = scope.ServiceProvider;
                    action(provider);
                }
            }
        }

    }
}
