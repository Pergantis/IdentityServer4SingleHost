using MediatR;

namespace IdentityServer4SingleHost.Web.IdentityAndAccess.Subscribe
{
    public class ConfirmUserEmail: IRequest<string>
    {
        public string UserId { get; set; }

        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        public ConfirmUserEmail(string userId, string code, string returnUrl)
        {
            UserId = userId;
            Code = code;
            ReturnUrl = returnUrl;
        }
    }
}
