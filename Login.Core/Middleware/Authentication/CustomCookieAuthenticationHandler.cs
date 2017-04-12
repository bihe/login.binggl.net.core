﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Jose;
using Login.Core.Configuration;
using Login.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Login.Core.Middleware.Authentication
{
    internal class CustomCookieAuthenticationHandler : AuthenticationHandler<CustomCookieAuthenticationOptions>
    {
        private const string HeaderValueNoCache = "no-cache";
        private const string HeaderValueMinusOne = "-1";
        private const string SessionIdClaim = "Microsoft.AspNetCore.Authentication.Cookies-SessionId";

        private bool _shouldRefresh;
        private DateTimeOffset? _refreshIssuedUtc;
        private DateTimeOffset? _refreshExpiresUtc;
        private string _sessionKey;
        private Task<AuthenticateResult> _readCookieTask;

        private Task<AuthenticateResult> EnsureCookieTicket()
        {
            // We only need to read the ticket once
            if (_readCookieTask == null)
            {
                _readCookieTask = ReadCookieTicket();
            }
            return _readCookieTask;
        }

        private void CheckForRefresh(AuthenticationTicket ticket)
        {
            var currentUtc = Options.SystemClock.UtcNow;
            var issuedUtc = ticket.Properties.IssuedUtc;
            var expiresUtc = ticket.Properties.ExpiresUtc;
            var allowRefresh = ticket.Properties.AllowRefresh ?? true;
            if (issuedUtc != null && expiresUtc != null && Options.SlidingExpiration && allowRefresh)
            {
                var timeElapsed = currentUtc.Subtract(issuedUtc.Value);
                var timeRemaining = expiresUtc.Value.Subtract(currentUtc);

                if (timeRemaining < timeElapsed)
                {
                    RequestRefresh(ticket);
                }
            }
        }

        private void RequestRefresh(AuthenticationTicket ticket)
        {
            var issuedUtc = ticket.Properties.IssuedUtc;
            var expiresUtc = ticket.Properties.ExpiresUtc;

            if (issuedUtc != null && expiresUtc != null)
            {
                _shouldRefresh = true;
                var currentUtc = Options.SystemClock.UtcNow;
                _refreshIssuedUtc = currentUtc;
                var timeSpan = expiresUtc.Value.Subtract(issuedUtc.Value);
                _refreshExpiresUtc = currentUtc.Add(timeSpan);
            }
        }

        private async Task<AuthenticateResult> ReadCookieTicket()
        {
            AuthenticationTicket ticket = null;

            var cookie = Options.CookieManager.GetRequestCookie(Context, Options.CookieName);
            if (string.IsNullOrEmpty(cookie))
            {
                // alternative: read the token cookie!
                var jwtCookie = Options.CookieManager.GetRequestCookie(Context, Options.JwtCookieName);
                if(string.IsNullOrEmpty(jwtCookie))
                {
                    return AuthenticateResult.Skip();
                }

                try
                {
                    // dehydrate the authentication-ticket
                    var keyBytes = System.Text.Encoding.UTF8.GetBytes(Options.JwtTokenSecret);
                    var payload = JWT.Decode(jwtCookie, keyBytes, JwsAlgorithm.HS256);
                    var payloadObject = JsonConvert.DeserializeObject<JwtPayload>(payload);

                    var authProperties = new AuthenticationProperties();

                    IList<Claim> claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name,  payloadObject.UserName),
                        new Claim(ClaimTypes.Sid,  payloadObject.UserId),
                        new Claim(ClaimTypes.GivenName,  payloadObject.GivenName),
                        new Claim(ClaimTypes.Surname, payloadObject.Surname),
                        new Claim(ClaimTypes.Email, payloadObject.Email),
                        new Claim(ClaimTypes.Role, Constants.ROLE_USER)
                    };

                    var identity = new ClaimsIdentity(claims, Options.ClaimsIssuer);
                    var principal = new ClaimsPrincipal(identity);

                    authProperties.IssuedUtc = payloadObject.Issued;
                    authProperties.ExpiresUtc = authProperties.IssuedUtc.Value.Add(Options.ExpireTimeSpan);

                    ticket = new AuthenticationTicket(principal, authProperties, Options.AuthenticationScheme);

                    var cookieValue = Options.TicketDataFormat.Protect(ticket, GetTlsTokenBinding());

                    Options.CookieManager.AppendResponseCookie(
                        Context,
                        Options.CookieName,
                        cookieValue,
                        BuildCookieOptions());
                }
                catch (Exception)
                {
                    // there was an error dehydrating the user by cookie - remove the cookie
                    Options.CookieManager.DeleteCookie(Context, Options.CookieName, BuildCookieOptions());

                    return AuthenticateResult.Skip();
                }
            }
            else
            {
                ticket = Options.TicketDataFormat.Unprotect(cookie, GetTlsTokenBinding());
            }
            
            if (ticket == null)
            {
                return AuthenticateResult.Fail("Unprotect ticket failed");
            }

            if (Options.SessionStore != null)
            {
                var claim = ticket.Principal.Claims.FirstOrDefault(c => c.Type.Equals(SessionIdClaim));
                if (claim == null)
                {
                    return AuthenticateResult.Fail("SessionId missing");
                }
                _sessionKey = claim.Value;
                ticket = await Options.SessionStore.RetrieveAsync(_sessionKey);
                if (ticket == null)
                {
                    return AuthenticateResult.Fail("Identity missing in session store");
                }
            }

            var currentUtc = Options.SystemClock.UtcNow;
            var issuedUtc = ticket.Properties.IssuedUtc;
            var expiresUtc = ticket.Properties.ExpiresUtc;

            if (expiresUtc != null && expiresUtc.Value < currentUtc)
            {
                if (Options.SessionStore != null)
                {
                    await Options.SessionStore.RemoveAsync(_sessionKey);
                }
                return AuthenticateResult.Fail("Ticket expired");
            }

            CheckForRefresh(ticket);

            // Finally we have a valid ticket
            return AuthenticateResult.Success(ticket);
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var result = await EnsureCookieTicket();
            if (!result.Succeeded)
            {
                return result;
            }

            var context = new CookieValidatePrincipalContext(Context, result.Ticket, Options);
            await Options.Events.ValidatePrincipal(context);

            if (context.Principal == null)
            {
                return AuthenticateResult.Fail("No principal.");
            }

            if (context.ShouldRenew)
            {
                RequestRefresh(result.Ticket);
            }

            return AuthenticateResult.Success(new AuthenticationTicket(context.Principal, context.Properties, Options.AuthenticationScheme));
        }

        private CookieOptions BuildCookieOptions()
        {
            var cookieOptions = new CookieOptions
            {
                Domain = Options.CookieDomain,
                HttpOnly = Options.CookieHttpOnly,
                Path = Options.CookiePath ?? (OriginalPathBase.HasValue ? OriginalPathBase.ToString() : "/"),
            };
            if (Options.CookieSecure == CookieSecurePolicy.SameAsRequest)
            {
                cookieOptions.Secure = Request.IsHttps;
            }
            else
            {
                cookieOptions.Secure = Options.CookieSecure == CookieSecurePolicy.Always;
            }
            return cookieOptions;
        }

        protected override async Task FinishResponseAsync()
        {
            // Only renew if requested, and neither sign in or sign out was called
            if (!_shouldRefresh || SignInAccepted || SignOutAccepted)
            {
                return;
            }

            var ticket = (await HandleAuthenticateOnceSafeAsync())?.Ticket;
            if (ticket != null)
            {
                if (_refreshIssuedUtc.HasValue)
                {
                    ticket.Properties.IssuedUtc = _refreshIssuedUtc;
                }
                if (_refreshExpiresUtc.HasValue)
                {
                    ticket.Properties.ExpiresUtc = _refreshExpiresUtc;
                }

                if (Options.SessionStore != null && _sessionKey != null)
                {
                    await Options.SessionStore.RenewAsync(_sessionKey, ticket);
                    var principal = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new[] { new Claim(SessionIdClaim, _sessionKey, ClaimValueTypes.String, Options.ClaimsIssuer) },
                            Options.AuthenticationScheme));
                    ticket = new AuthenticationTicket(principal, null, Options.AuthenticationScheme);
                }

                var cookieValue = Options.TicketDataFormat.Protect(ticket, GetTlsTokenBinding());

                var cookieOptions = BuildCookieOptions();
                if (ticket.Properties.IsPersistent && _refreshExpiresUtc.HasValue)
                {
                    cookieOptions.Expires = _refreshExpiresUtc.Value.ToUniversalTime();
                }

                Options.CookieManager.AppendResponseCookie(
                    Context,
                    Options.CookieName,
                    cookieValue,
                    cookieOptions);

                await ApplyHeaders(shouldRedirectToReturnUrl: false, properties: ticket.Properties);
            }
        }

        protected override async Task HandleSignInAsync(SignInContext signin)
        {
            // Process the request cookie to initialize members like _sessionKey.
            var result = await EnsureCookieTicket();
            var cookieOptions = BuildCookieOptions();

            var signInContext = new CookieSigningInContext(
                Context,
                Options,
                Options.AuthenticationScheme,
                signin.Principal,
                new AuthenticationProperties(signin.Properties),
                cookieOptions);

            DateTimeOffset issuedUtc;
            if (signInContext.Properties.IssuedUtc.HasValue)
            {
                issuedUtc = signInContext.Properties.IssuedUtc.Value;
            }
            else
            {
                issuedUtc = Options.SystemClock.UtcNow;
                signInContext.Properties.IssuedUtc = issuedUtc;
            }

            if (!signInContext.Properties.ExpiresUtc.HasValue)
            {
                signInContext.Properties.ExpiresUtc = issuedUtc.Add(Options.ExpireTimeSpan);
            }

            await Options.Events.SigningIn(signInContext);

            if (signInContext.Properties.IsPersistent)
            {
                var expiresUtc = signInContext.Properties.ExpiresUtc ?? issuedUtc.Add(Options.ExpireTimeSpan);
                signInContext.CookieOptions.Expires = expiresUtc.ToUniversalTime();
            }

            var ticket = new AuthenticationTicket(signInContext.Principal, signInContext.Properties, signInContext.AuthenticationScheme);
            if (Options.SessionStore != null)
            {
                if (_sessionKey != null)
                {
                    await Options.SessionStore.RemoveAsync(_sessionKey);
                }
                _sessionKey = await Options.SessionStore.StoreAsync(ticket);
                var principal = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[] { new Claim(SessionIdClaim, _sessionKey, ClaimValueTypes.String, Options.ClaimsIssuer) },
                        Options.ClaimsIssuer));
                ticket = new AuthenticationTicket(principal, null, Options.AuthenticationScheme);
            }
            var cookieValue = Options.TicketDataFormat.Protect(ticket, GetTlsTokenBinding());

            Options.CookieManager.AppendResponseCookie(
                Context,
                Options.CookieName,
                cookieValue,
                signInContext.CookieOptions);

            var signedInContext = new CookieSignedInContext(
                Context,
                Options,
                Options.AuthenticationScheme,
                signInContext.Principal,
                signInContext.Properties);

            await Options.Events.SignedIn(signedInContext);

            // Only redirect on the login path
            var shouldRedirect = Options.LoginPath.HasValue && OriginalPath == Options.LoginPath;
            await ApplyHeaders(shouldRedirect, signedInContext.Properties);
        }

        protected override async Task HandleSignOutAsync(SignOutContext signOutContext)
        {
            // Process the request cookie to initialize members like _sessionKey.
            var ticket = await EnsureCookieTicket();
            var cookieOptions = BuildCookieOptions();
            if (Options.SessionStore != null && _sessionKey != null)
            {
                await Options.SessionStore.RemoveAsync(_sessionKey);
            }

            var context = new CookieSigningOutContext(
                Context,
                Options,
                new AuthenticationProperties(signOutContext.Properties),
                cookieOptions);

            await Options.Events.SigningOut(context);

            Options.CookieManager.DeleteCookie(
                Context,
                Options.CookieName,
                context.CookieOptions);

            // Only redirect on the logout path
            var shouldRedirect = Options.LogoutPath.HasValue && OriginalPath == Options.LogoutPath;
            await ApplyHeaders(shouldRedirect, context.Properties);
        }

        private async Task ApplyHeaders(bool shouldRedirectToReturnUrl, AuthenticationProperties properties)
        {
            Response.Headers[HeaderNames.CacheControl] = HeaderValueNoCache;
            Response.Headers[HeaderNames.Pragma] = HeaderValueNoCache;
            Response.Headers[HeaderNames.Expires] = HeaderValueMinusOne;

            if (shouldRedirectToReturnUrl && Response.StatusCode == 200)
            {
                // set redirect uri in order:
                // 1. properties.RedirectUri
                // 2. query parameter ReturnUrlParameter
                //
                // Absolute uri is not allowed if it is from query string as query string is not
                // a trusted source.
                var redirectUri = properties.RedirectUri;
                if (string.IsNullOrEmpty(redirectUri))
                {
                    redirectUri = Request.Query[Options.ReturnUrlParameter];
                    if (string.IsNullOrEmpty(redirectUri) || !IsHostRelative(redirectUri))
                    {
                        redirectUri = null;
                    }
                }

                if (redirectUri != null)
                {
                    await Options.Events.RedirectToReturnUrl(
                        new CookieRedirectContext(Context, Options, redirectUri, properties));
                }
            }
        }

        private static bool IsHostRelative(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            if (path.Length == 1)
            {
                return path[0] == '/';
            }
            return path[0] == '/' && path[1] != '/' && path[1] != '\\';
        }

        protected override async Task<bool> HandleForbiddenAsync(ChallengeContext context)
        {
            var properties = new AuthenticationProperties(context.Properties);
            var returnUrl = properties.RedirectUri;
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = OriginalPathBase + Request.Path + Request.QueryString;
            }
            var accessDeniedUri = Options.AccessDeniedPath + QueryString.Create(Options.ReturnUrlParameter, returnUrl);
            var redirectContext = new CookieRedirectContext(Context, Options, BuildRedirectUri(accessDeniedUri), properties);
            await Options.Events.RedirectToAccessDenied(redirectContext);
            return true;
        }

        protected override async Task<bool> HandleUnauthorizedAsync(ChallengeContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var properties = new AuthenticationProperties(context.Properties);
            var redirectUri = properties.RedirectUri;
            if (string.IsNullOrEmpty(redirectUri))
            {
                redirectUri = OriginalPathBase + Request.Path + Request.QueryString;
            }

            var loginUri = Options.LoginPath + QueryString.Create(Options.ReturnUrlParameter, redirectUri);
            var redirectContext = new CookieRedirectContext(Context, Options, BuildRedirectUri(loginUri), properties);
            await Options.Events.RedirectToLogin(redirectContext);
            return true;

        }

        private string GetTlsTokenBinding()
        {
            var binding = Context.Features.Get<ITlsTokenBindingFeature>()?.GetProvidedTokenBindingId();
            return binding == null ? null : Convert.ToBase64String(binding);
        }
    }
}