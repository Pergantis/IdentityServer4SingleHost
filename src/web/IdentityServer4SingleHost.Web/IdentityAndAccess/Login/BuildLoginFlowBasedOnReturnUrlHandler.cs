using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4SingleHost.Web.Models.AccountViewModels;
using MediatR;
using Microsoft.AspNetCore.Authentication;

namespace IdentityServer4SingleHost.Web.IdentityAndAccess.Login
{
    public class BuildLoginFlowBasedOnReturnUrlHandler: IRequestHandler<BuildLoginFlowBasedOnReturnUrl, LoginViewModel>
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;

        public BuildLoginFlowBasedOnReturnUrlHandler(IIdentityServerInteractionService interaction, IClientStore clientStore, IAuthenticationSchemeProvider schemeProvider)
        {
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
        }

        public async Task<LoginViewModel> Handle(BuildLoginFlowBasedOnReturnUrl request, CancellationToken cancellationToken)
        {
            var returnUrl = request.ReturnUrl;

            // Take the authorization context from the return url
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

            // Check if there is a try to connect using an identity provider like Google or Facebook
            // We should find this information from the request context and in the scheme provider of Identity Server
            // In order to bypass the login flow we should configure the idp in acr values (front channel of the request)
            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == IdentityServerConstants.LocalIdentityProvider;

                // Short cirquit the UI and trigger the external provider
                var vm = new LoginViewModel()
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl
                };

                if (!local)
                {
                    vm.ExternalProviders = new[] {new ExternalProvider {AuthenticationScheme = context.IdP}};
                }

                return vm;
            }

            // Case when we create a classic login flow with html
            // Find all the authentication schemes
            var schemes = await _schemeProvider.GetAllSchemesAsync();

            // find the list with the external providers
            var providers = schemes.Where(x => x.DisplayName != null)
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName ?? x.Name,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;

            // Is the request from a authenticated client πχ like our mobile client
            if (context?.Client.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;
                }
            }

            return new LoginViewModel
            {
                EnableLocalLogin = allowLocal,
                ReturnUrl = returnUrl,
                ExternalProviders = providers.ToArray()
            };
        }
    }
}
