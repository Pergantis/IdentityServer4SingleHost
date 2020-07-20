using IdentityServer4SingleHost.Web.Models.AccountViewModels;
using MediatR;

namespace IdentityServer4SingleHost.Web.IdentityAndAccess.Logout
{
    public class SignOutUserBasedOnProvider: IRequest<LoggedOutViewModel>
    {
        public string LogoutId { get; set; }

        public SignOutUserBasedOnProvider(string logoutId)
        {
            LogoutId = logoutId;
        }
    }
}
