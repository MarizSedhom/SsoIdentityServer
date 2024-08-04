using Client1.Models;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Diagnostics;

namespace Client1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public IActionResult Privacy()
        {Console.WriteLine("IN PRIVACY");
            foreach (var claim in User.Claims)
            {
                
                Console.WriteLine($"{claim.Type}: {claim.Value}");
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Clear the authentication cookies
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Construct the SSO server logout URL with the returnUrl
            var ssoLogoutUrl = "https://localhost:7025/Identity/Account/Logout";
            var clientAppUrl = "https://localhost:7093/Home"; // Your client app URL
            var returnUrl = Uri.EscapeDataString(clientAppUrl);

            // Redirect to the SSO server logout URL
            return Redirect($"{ssoLogoutUrl}?returnUrl={returnUrl}");
            /*var redirectUri = new UriBuilder("https://localhost:7025/Identity/Account/Login");
            redirectUri.Query = $"returnUrl={"https://localhost:7135" + returnUrl}";

            redirectContext.HttpContext.Response.Redirect(redirectUri.ToString());*/
        }
    }
}
