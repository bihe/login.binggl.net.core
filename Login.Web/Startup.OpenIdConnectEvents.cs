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
            context.Response.Redirect("/error?message=" + context.Failure.Message);
            return Task.FromResult(0);
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


            var scopeFactory = _serviceColletion
                    .BuildServiceProvider()
                    .GetRequiredService<IServiceScopeFactory>();

            User lookupUser = null;
            using (var scope = scopeFactory.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var repository = provider.GetRequiredService<ILoginService>();

                var awaiter = repository.GetUserByEmail(externalLookupEmail);
                awaiter.Wait();
                lookupUser = awaiter.Result;
            }

            logger.LogDebug("User from lookup: {0}", lookupUser);

            if (lookupUser == null)
            {
                logger.LogWarning("Could not find a user by email {0}", externalLookupEmail);
                return Task.FromException(new SecurityTokenValidationException("Supplied user is not allowed to access the system!"));
            }

            IList<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, lookupUser.DisplayName)
                , new Claim(ClaimTypes.Sid,  claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? "")
                , new Claim(ClaimTypes.GivenName,  claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value ?? "")
                , new Claim(ClaimTypes.Surname, claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value ?? "")
                , new Claim(ClaimTypes.Email, claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value ?? "")
                , new Claim(ClaimTypes.Role, Constants.ROLE_USER)
            };

            logger.LogDebug("User {0} has {1} sites assigned.", externalLookupEmail, lookupUser?.Sites?.Count ?? 0);

            var identity = new ClaimsIdentity(claims, OpenIdConnectDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);
            context.Principal = principal;
            return Task.FromResult(0);
        }
    }
}
