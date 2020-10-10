using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer4SingleHost.Web.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("temp_data")]
        [Authorize(AuthenticationSchemes = "Bearer", Policy = "CanAccessPublicApiData")]
        public IActionResult GetTempData()
        {
            return Ok("Hello User");
        }
    }
}