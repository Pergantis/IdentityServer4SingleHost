using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Services;
using IdentityServer4SingleHost.Domain.IdentityAndAccess.Models;
using IdentityServer4SingleHost.Web.IdentityAndAccess.Services;
using MediatR;

namespace IdentityServer4SingleHost.Web.IdentityAndAccess.Login
{

    public class SignInUserAndReturnToCallerHandler: IRequestHandler<SignInUserAndReturnToCaller, string>
    {
        private readonly ILoginService<ApplicationUser> _loginService;
        private readonly IIdentityServerInteractionService _interaction;

        public SignInUserAndReturnToCallerHandler(ILoginService<ApplicationUser> loginService, IIdentityServerInteractionService interaction)
        {
            _loginService = loginService;
            _interaction = interaction;
        }

        public async Task<string> Handle(SignInUserAndReturnToCaller request, CancellationToken cancellationToken)
        {
            // Find the user from his/her email
            var user = await _loginService.FindByUsername(request.Email);

            if (await _loginService.ValidateCredentials(user, request.Password))
            {
                // Setup the default login properties
                var props = _loginService.SetupDefaultProperties(request.ReturnUrl, true);

                // And sign in him/her
                await _loginService.SignInAsync(user, props);

                // If the return Url is still valid, return to authorize endpoint (mobile app)
                if (_interaction.IsValidReturnUrl(request.ReturnUrl))
                {
                    return request.ReturnUrl;
                }

                return "~/";
            }

            return null;
        }
    }
}
