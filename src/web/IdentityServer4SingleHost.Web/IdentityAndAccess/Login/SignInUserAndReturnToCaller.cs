using System.ComponentModel.DataAnnotations;
using MediatR;

namespace IdentityServer4SingleHost.Web.IdentityAndAccess.Login
{
    public class SignInUserAndReturnToCaller: IRequest<string>
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Range(0, 2)]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }

        public SignInUserAndReturnToCaller(string email, string password, string returnUrl)
        {
            Email = email;
            Password = password;
            ReturnUrl = returnUrl;
        }
    }
}
