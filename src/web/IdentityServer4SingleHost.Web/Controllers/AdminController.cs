using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer4SingleHost.Web.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        [Authorize(Policy = "IsAuthenticatedAdministrator")]
        public IActionResult Index()
        {
            return View();
        }
    }
}