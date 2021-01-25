using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4SingleHost.Domain.IdentityAndAccess.Models;
using IdentityServer4SingleHost.Web.IdentityAndAccess.Extensions;
using IdentityServer4SingleHost.Web.IdentityAndAccess.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer4SingleHost.Web.Controllers
{
    //TODO Lean Controller with mediator pattern.
    public class ExternalController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILoginService<ApplicationUser> _loginService;

        public ExternalController(IIdentityServerInteractionService interaction, IClientStore clientStore,
            UserManager<ApplicationUser> userManager, ILoginService<ApplicationUser> loginService)
        {
            _interaction = interaction;
            _clientStore = clientStore;
            _userManager = userManager;
            _loginService = loginService;
        }

        [HttpGet]
        public IActionResult Challenge(string provider, string returnUrl)
        {
            // A user can subscribe in the web mvc app. In this case the returnUrl is null, so we replace it with the home mvc index.
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

            // We check if the returnUrl is local and valid (in redirect context)
            if (Url.IsLocalUrl(returnUrl) == false && _interaction.IsValidReturnUrl(returnUrl) == false)
            {
                // The user followed a malicious link. It's in our best interests to log this behavior
                throw new Exception("Invalid Url");
            }

            // Create the authentication properties and challenge the external provider
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback)),
                Items =
                {
                    { "returnUrl", returnUrl },
                    { "scheme", provider },
                }
            };

            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            // Take the authenticate result
            var result =
                await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            // Find the user from the external provider in your system
            var (user, provider, providerUserId, claims) =
                await FindUserFromExternalProvider(result);

            if (user == null)
            {
                // Create the new user
                user = new ApplicationUser
                {
                    UserName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName).Value,
                    Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value,
                    EmailConfirmed = true // Oι external providers has verified the user's emails before us
                };

                // And save him after
                var creationResult = await _userManager.CreateAsync(user);

                if (creationResult.Succeeded)
                {
                    // You can add any more claims if you want here

                    //await _userManager
                    //    .AddClaimAsync(user, new Claim("profile_picture",
                    //        claims.FirstOrDefault(c => c.Type == "urn:google:image").Value));
                }
            }

            // We perform the user login with the external data
            await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider));

            // We collect any extra claims or properties from the authentication result in order to use them in a logout case.
            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();
            ProcessLoginCallbackForOidc(result, additionalLocalClaims, localSignInProps);

            // We sign in the user by writing on the default cookie
            await _loginService.SignInAsync(user, localSignInProps, provider);

            // Delete the external cookie
            await HttpContext.SignOutAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);

            // Check if the return url is part of an OIDC request (Open Id Protocol)
            var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

            if (context != null)
            {
                //If client uses a PKCE
                if (await _clientStore.IsPkceClientAsync(context.Client.ClientId))
                {
                    // Then he is native and we change the flow in a single page for better UX
                    return this.LoadingPage("Redirect", returnUrl);
                    //return Redirect(returnUrl);
                }
            }

            return Redirect(returnUrl);
        }


        private async Task<(ApplicationUser user, string provider, string providerUserId, IEnumerable<Claim> claims)>
            FindUserFromExternalProvider(AuthenticateResult result)
        {
            // We instantiate an external user using his external cookie claims
            var externalUser = result.Principal;

            // Check his uniqueness from the sub claim or his NameIdentifier. Maybe it is not true for all external providers
            var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                              externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new Exception("Unknown userid");

            // Remove the user id cliam in order to not add it as an extra claim in his creation
            var claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);

            // Βρισκώ τον external χρήστη χρησιμοποιώντας τον provider και το id του.
            var provider = result.Properties.Items["scheme"];
            var providerUserId = userIdClaim.Value;

            var user = await _userManager.FindByLoginAsync(provider, providerUserId);

            return (user, provider, providerUserId, claims);

        }


        private void ProcessLoginCallbackForOidc(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
            // Εαν ο external provider επιστρέψει ενα session id, το αποθηκεύω ως extra claim με σκοπό να κάνω single sign-out
            // If the external provider returns a session id, we store it as an extra claim in order to perform a single sign-out
            var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);

            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // If the external provider returns an id_token, we use it for the sign-out
            var id_token = externalResult.Properties.GetTokenValue("id_token");

            if (id_token != null)
            {
                localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = id_token } });
            }

            // Προσθέτω και τα custom properties του identity server
            string returnUrl;
            externalResult.Properties.Items.TryGetValue("returnUrl", out returnUrl );
            localSignInProps.ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(120);
            localSignInProps.AllowRefresh = true;
            localSignInProps.RedirectUri = returnUrl;
            localSignInProps.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(365);
            localSignInProps.IsPersistent = true;

        }





    }
}