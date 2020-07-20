using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4SingleHost.Domain.IdentityAndAccess.Models;
using IdentityServer4SingleHost.Web.Filters;
using IdentityServer4SingleHost.Web.IdentityAndAccess.Login;
using IdentityServer4SingleHost.Web.IdentityAndAccess.Logout;
using IdentityServer4SingleHost.Web.IdentityAndAccess.Subscribe;
using IdentityServer4SingleHost.Web.Models.AccountViewModels;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer4SingleHost.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMediator _mediator;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(IMediator mediator, UserManager<ApplicationUser> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // We create the login model based on client's return url
            var loginFlow = new BuildLoginFlowBasedOnReturnUrl(returnUrl);

            var vm = await _mediator.Send(loginFlow);

            if (vm.IsExternalLoginOnly)
            {
                // We challenge the external provider (Google or Facebook in particular case)
                return RedirectToAction("Challenge", "External", new { provider = vm.ExternalLoginScheme, returnUrl });
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [GlobalModelStateValidator]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Create the request object
            var request = new SignInUserAndReturnToCaller(model.Email, model.Password, model.ReturnUrl);

            // Pass the request handler to the respective MediatR handler.
            // The handler returns the returnUrl, after user has signed in
            var returnUrl = await _mediator.Send(request);

            // Check the returnUrl. If it is valid, we redirect respectively
            if (!returnUrl.IsNullOrEmpty())
                return Redirect(returnUrl);
            else // or we have an invalid name or password
            {
                //TODO Return invalid name or password error at UI
                ViewData["ReturnUrl"] = model.ReturnUrl;

                ModelState.AddModelError(string.Empty, "Invalid name ή password");

                return View(model);
            }
        }


        public async Task<IActionResult> RejectAuthentication(string returnUrl)
        {

            var rejectAuthentication = new RejectAuthenticationOnLoginFlow(returnUrl);

            var redirectUrl = await _mediator.Send(rejectAuthentication);

            return Redirect(redirectUrl);

        }

        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            var logoutFlow = new BuildLogoutFlow(logoutId);

            var vm = await _mediator.Send(logoutFlow);

            // If the handler returns a false,
            // it means that a user wants to connect from a mobile device without a intermediate logout UI
            if (vm.ShowLogoutPrompt == false)
            {
                return await Logout(vm);
            }

            // Return the prompt view. (Protects the application in case of a malicious redirect attack)
            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            var signOutUser = new SignOutUserBasedOnProvider(model.LogoutId);

            // We perform the use log out flow and return the view model
            var vm = await _mediator.Send(signOutUser);

            // Return a Javascript View that registers an i-frame with the postLogoutUrl. The view returns in an blink of an eye for better UX
            return View("LoggedOut", vm);
        }

        public async Task<IActionResult> DeviceLogOut(string redirectUrl)
        {
            // Remove the authentication cookie
            await HttpContext.SignOutAsync();

            // And set a new anonymous user
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            return Redirect(redirectUrl);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [GlobalModelStateValidator]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var registerUser = new RegisterUserAndRedirectHimToLogin(model.Email, model.Password, returnUrl);

            var (redirectUrl, errors) = await _mediator.Send(registerUser);

            if (errors.Any())
            {
                //TODO Model State errors at View
                AddErrors(errors);

                return View(model);
            }

            if (redirectUrl != null)
            {
                return Redirect(redirectUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userid, string code, string returnUrl)
        {
            var confirmEmail = new ConfirmUserEmail(userid, code, returnUrl);

            var returnValidUrl = await _mediator.Send(confirmEmail);

            return Redirect(returnValidUrl);
        }


        [HttpGet]
        public IActionResult RedirectWithJavascriptCall(string returnUrl)
        {
            return View("Redirect", new RedirectViewModel { RedirectUrl = returnUrl });;
        }

        [HttpGet]
        public IActionResult Redirecting()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        private void AddErrors(List<string> errors)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }


    }
}