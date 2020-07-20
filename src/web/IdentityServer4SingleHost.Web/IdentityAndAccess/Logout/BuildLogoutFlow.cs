using IdentityServer4SingleHost.Web.Models.AccountViewModels;
using MediatR;

namespace IdentityServer4SingleHost.Web.IdentityAndAccess.Logout
{
    public class BuildLogoutFlow: IRequest<LogoutViewModel>
    {
        public string LogoutId { get; set; }

        public BuildLogoutFlow(string logoutId)
        {
            LogoutId = logoutId;
        }
    }
}
