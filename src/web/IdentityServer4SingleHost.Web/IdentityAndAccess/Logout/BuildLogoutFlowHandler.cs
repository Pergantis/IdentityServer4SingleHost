using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Services;
using IdentityServer4SingleHost.Web.Models.AccountViewModels;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace IdentityServer4SingleHost.Web.IdentityAndAccess.Logout
{
    public class BuildLogoutFlowHandler: IRequestHandler<BuildLogoutFlow, LogoutViewModel>
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IIdentityServerInteractionService _interaction;

        public BuildLogoutFlowHandler(IHttpContextAccessor accessor, IIdentityServerInteractionService interaction)
        {
            _accessor = accessor;
            _interaction = interaction;
        }

        public async Task<LogoutViewModel> Handle(BuildLogoutFlow request, CancellationToken cancellationToken)
        {
            var logoutId = request.LogoutId;

            var user = _accessor.HttpContext.User.Identity;

            var vm = new LogoutViewModel {LogoutId = logoutId};

            // Xamarin Case. It's safe to sign out because i come from a registered mobile app
            var context = await _interaction.GetLogoutContextAsync(logoutId);

            // If the user is not authenticated, just show the logout page.
            // Prevent the auto sign out to malicious web page
            vm.ShowLogoutPrompt = (user.IsAuthenticated != false && context?.ShowSignoutPrompt != false);

            return vm;
        }
    }
}
