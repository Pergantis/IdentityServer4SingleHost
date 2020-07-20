using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer4SingleHost.Web.Controllers
{
    public class HomeController : Controller
    {
        [Authorize(Policy = "IsAuthenticatedAdministrator")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("tempdata")]
        [Authorize(AuthenticationSchemes = "Bearer", Policy = "CanAccessPublicApiData")]
        public IActionResult GetTempData()
        {
            return Ok("Hello admin user");
        }
    }
}