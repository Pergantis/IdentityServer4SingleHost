using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4SingleHost.Web.IdentityAndAccess.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityServer4SingleHost.Web.IdentityAndAccess.Login
{
    public class RejectAuthenticationOnLoginFlowHandler: IRequestHandler<RejectAuthenticationOnLoginFlow, string>
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IHttpContextAccessor _accessor;
        private readonly LinkGenerator _generator;

        public RejectAuthenticationOnLoginFlowHandler(IIdentityServerInteractionService interaction, IClientStore clientStore, IHttpContextAccessor accessor, LinkGenerator generator)
        {
            _interaction = interaction;
            _clientStore = clientStore;
            _accessor = accessor;
            _generator = generator;
        }

        public async Task<string> Handle(RejectAuthenticationOnLoginFlow request, CancellationToken cancellationToken)
        {
            var returnUrl = request.ReturnUrl;

            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

            if (context != null)
            {
                // If the user clicks the cancel button, we return the consent denied response, even if the client doesn't negotiate a consent
                // This move sends an OIDC error access denied to the client app
                await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                // We can trust the return url because the GetAuthorizationContextAsync returned not null value
                if (await _clientStore.IsPkceClientAsync(context.ClientId))
                {
                    // If the client uses PKCE then it is native and we return with javascript for better UX
                    var redirectUrlUsingJS = _generator
                        .GetPathByAction(_accessor.HttpContext, "RedirectWithJavascriptCall", "Account", new {returnUrl});

                    return redirectUrlUsingJS;
                }

                // Just return the returnUrl
                return returnUrl;
            }
            else
            {
                // If the context is not valid, return the home page
                return "~/";
            }
        }
    }
}
