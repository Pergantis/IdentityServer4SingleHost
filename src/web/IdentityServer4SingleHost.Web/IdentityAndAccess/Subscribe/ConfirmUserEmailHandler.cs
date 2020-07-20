using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Services;
using IdentityServer4SingleHost.Domain.IdentityAndAccess.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer4SingleHost.Web.IdentityAndAccess.Subscribe
{
    public class ConfirmUserEmailHandler: IRequestHandler<ConfirmUserEmail,string>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUrlHelper _helper;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IHttpContextAccessor _accessor;

        public ConfirmUserEmailHandler(UserManager<ApplicationUser> userManager, IUrlHelper helper, IIdentityServerInteractionService interaction, IHttpContextAccessor accessor)
        {
            _userManager = userManager;
            _helper = helper;
            _interaction = interaction;
            _accessor = accessor;
        }

        public async Task<string> Handle(ConfirmUserEmail request, CancellationToken cancellationToken)
        {
            var user = _userManager.FindByIdAsync(request.UserId).Result;

            var result = await _userManager.ConfirmEmailAsync(user, request.Code);

            if(result.Errors.Any(e => e.Code.Equals(nameof(IdentityErrorDescriber.InvalidToken))))
            {
                // Do nothing
            }

            if (_helper.IsLocalUrl(request.ReturnUrl) == false && _interaction.IsValidReturnUrl(request.ReturnUrl) == false)
            {
                throw new Exception("Invalid Url");
            }

            return request.ReturnUrl;

        }
    }
}
