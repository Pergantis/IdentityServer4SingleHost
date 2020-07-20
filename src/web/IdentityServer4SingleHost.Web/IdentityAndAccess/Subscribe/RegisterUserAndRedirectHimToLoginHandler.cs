using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4SingleHost.Domain.IdentityAndAccess.Models;
using IdentityServer4SingleHost.Infrastructure.Emails;
using IdentityServer4SingleHost.Web.Extensions;
using IdentityServer4SingleHost.Web.IdentityAndAccess.Services;
using IdentityServer4SingleHost.Web.Models.EmailViewModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace IdentityServer4SingleHost.Web.IdentityAndAccess.Subscribe
{
    public class RegisterUserAndRedirectHimToLoginHandler : IRequestHandler<RegisterUserAndRedirectHimToLogin, (string,List<string>)>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRedirectService _redirect;
        private readonly IHttpContextAccessor _accessor;
        private readonly LinkGenerator _generator;
        private readonly IRazorViewToStringRenderer _renderer;
        private readonly IOptions<EmailSettings> _settings;

        public RegisterUserAndRedirectHimToLoginHandler(UserManager<ApplicationUser> userManager, IHttpContextAccessor accessor,
            LinkGenerator generator, IRazorViewToStringRenderer renderer, IOptions<EmailSettings> settings, IRedirectService redirect)
        {
            _userManager = userManager;
            _accessor = accessor;
            _generator = generator;
            _renderer = renderer;
            _settings = settings;
            _redirect = redirect;
        }

        public async Task<(string,List<string>)> Handle(RegisterUserAndRedirectHimToLogin request, CancellationToken cancellationToken)
        {
            var redirectUrl = String.Empty;
            var context = _accessor.HttpContext;
            var errors = new List<string>();

            var newUser = new ApplicationUser { UserName = request.Email, Email = request.Email};

            var result = await _userManager.CreateAsync(newUser, request.Password);

            if (result.Succeeded)
            {
                if (request.ReturnUrl != null)
                {
                    if (context.User.IsAuthenticated())
                    {
                        redirectUrl = request.ReturnUrl;
                    }
                    else
                    {
                        var user = await _userManager.FindByEmailAsync(request.Email);

                        await SendConfirmationUrl(context, user, request.ReturnUrl);

                        redirectUrl = await _redirect.ExtractOpenEmailAppRedirectUrl(request.ReturnUrl);

                    }
                }
            }
            else
            {
                errors = SubscriptionErrors(result);
            }

            return (redirectUrl, errors);

        }

        private List<string> SubscriptionErrors(IdentityResult result)
        {
            var errorList = new List<string>();

            if (result.Errors.Any(e => e.Code.Equals(nameof(IdentityErrorDescriber.InvalidUserName))))
                errorList.Add("Invalid username.");

            if (result.Errors.Any(e => e.Code.Equals(nameof(IdentityErrorDescriber.DuplicateEmail))))
                errorList.Add("Duplicate email.");


            if (result.Errors.Any(e => e.Code.Equals(nameof(IdentityErrorDescriber.DuplicateUserName))))
                errorList.Add("Duplicate username.");


            if (result.Errors.Any(e => e.Code.Equals(nameof(IdentityErrorDescriber.PasswordTooShort))))
                errorList.Add("Invalid password.");

            return errorList;
        }

        private async Task SendConfirmationUrl(HttpContext context, ApplicationUser user, string oidcRedirectUrl)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var callbackUrl = _generator.GetUriByAction(context, "ConfirmEmail", "Account",
                new {userId = user.Id, code, returnUrl = oidcRedirectUrl});

            var message = await CreateConfirmationEmailHtml(callbackUrl);

            var mailProvider = new MailProvider(_settings.Value);

            await mailProvider.SendEmailAsync(user.Email, "Confirm Email", message);
        }

        private async Task<string> CreateConfirmationEmailHtml(string confirmationUrl)
        {
            var emailConfirmationViewModel = new EmailRedirectViewModel(){ RedirectUrl = confirmationUrl };

            string message = await _renderer.RenderViewToStringAsync("/Views/Emails/UserEmailConfirmation.cshtml",
                emailConfirmationViewModel);

            return message;
        }
    }
}
