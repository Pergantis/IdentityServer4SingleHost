using MediatR;

namespace IdentityServer4SingleHost.Web.IdentityAndAccess.Login
{
    public class RejectAuthenticationOnLoginFlow: IRequest<string>
    {
        public string ReturnUrl { get; set; }

        public RejectAuthenticationOnLoginFlow(string returnUrl)
        {
            ReturnUrl = returnUrl;
        }
    }
}
