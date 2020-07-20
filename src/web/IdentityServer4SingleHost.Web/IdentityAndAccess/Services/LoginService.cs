using System;
using System.Threading.Tasks;
using IdentityServer4SingleHost.Domain.IdentityAndAccess.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace IdentityServer4SingleHost.Web.IdentityAndAccess.Services
{
    public class LoginService: ILoginService<ApplicationUser>
    {
        private readonly  UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public LoginService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<ApplicationUser> FindByUsername(string user)
        {
            return await _userManager.FindByEmailAsync(user);
        }

        public async Task<bool> ValidateCredentials(ApplicationUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public Task SignIn(ApplicationUser user)
        {
            return _signInManager.SignInAsync(user, true);
        }

        public Task SignInAsync(ApplicationUser user, AuthenticationProperties properties, string authenticationMethod = null)
        {
            return _signInManager.SignInAsync(user, properties, authenticationMethod);
        }

        public AuthenticationProperties SetupDefaultProperties(string returnUrl, bool hasPermanentToken = true)
        {
            var tokenLifetime = _configuration.GetValue("TokenLifetimeMinutes", 120);
            var permanentTokenLifetime = _configuration.GetValue("PermanentTokenLifetimeDays", 365);

            var props = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(tokenLifetime),
                AllowRefresh = true,
                RedirectUri = returnUrl
            };

            if (hasPermanentToken)
            {
                props.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(permanentTokenLifetime);
                props.IsPersistent = true;
            };

            return props;
        }
    }
}
