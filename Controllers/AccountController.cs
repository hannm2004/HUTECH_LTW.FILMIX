using Microsoft.AspNetCore.Mvc;

namespace untitled1.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Auth()
        {
            return View();
        }
    }
}
