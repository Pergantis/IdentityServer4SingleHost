using IdentityServer4SingleHost.Web.Models.AccountViewModels;
using MediatR;

namespace IdentityServer4SingleHost.Web.IdentityAndAccess.Login
{
    public class BuildLoginFlowBasedOnReturnUrl : IRequest<LoginViewModel>
    {
        public string ReturnUrl { get; set; }

        public BuildLoginFlowBasedOnReturnUrl(string returnUrl)
        {
            ReturnUrl = returnUrl;
        }
    }
}
