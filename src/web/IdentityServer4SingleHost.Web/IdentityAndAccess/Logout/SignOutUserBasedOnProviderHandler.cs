using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using IdentityServer4SingleHost.Domain.IdentityAndAccess.Models;
using IdentityServer4SingleHost.Web.Models.AccountViewModels;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;

namespace IdentityServer4SingleHost.Web.IdentityAndAccess.Logout
{
    public class SignOutUserBasedOnProviderHandler : IRequestHandler<SignOutUserBasedOnProvider, LoggedOutViewModel>
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly LinkGenerator _generator;

        public SignOutUserBasedOnProviderHandler(IIdentityServerInteractionService interaction, IHttpContextAccessor accessor,
            SignInManager<ApplicationUser> signInManager, LinkGenerator generator)
        {
            _interaction = interaction;
            _accessor = accessor;
            _signInManager = signInManager;
            _generator = generator;
        }

        public async Task<LoggedOutViewModel> Handle(SignOutUserBasedOnProvider request, CancellationToken cancellationToken)
        {
            var logoutId = request.LogoutId;

            var context = _accessor.HttpContext;

            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = true,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
            };

            if (context.User?.Identity.IsAuthenticated == true)
            {
                var idp = context.User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;

                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    // Create the fedareted signout. Facebook and Google providers doesn't support a federated sign out.
                    var providerSupportsSignout = await context.GetSchemeSupportsSignOutAsync(idp);

                    if (providerSupportsSignout)
                    {
                        if (logoutId == null)
                        {
                            // If there isn't a logoutId, just create one. In that way, we can pass the data to the external provider
                            // in order to sign out the user
                            logoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        // Create the redirect url
                        var redirectUrl = _generator.GetPathByAction(context, "Logout", "Account", new {logoutId});

                        try
                        {
                            await context.SignOutAsync(idp, new AuthenticationProperties {RedirectUri = redirectUrl});
                        }
                        catch (Exception e)
                        {
                            // ignored
                        }
                    }
                }

                // Clean the cookies
                await _signInManager.SignOutAsync();

            }

            return vm;
        }
    }
}
