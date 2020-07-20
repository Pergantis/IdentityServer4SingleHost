using System.Collections.Generic;
using MediatR;

namespace IdentityServer4SingleHost.Web.IdentityAndAccess.Subscribe
{
    public class RegisterUserAndRedirectHimToLogin: IRequest<(string,List<string>)>
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string ReturnUrl { get; set; }

        public RegisterUserAndRedirectHimToLogin(string email, string password, string returnUrl)
        {
            Email = email;
            Password = password;
            ReturnUrl = returnUrl;
        }
    }
}
