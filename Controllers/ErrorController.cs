using Microsoft.AspNetCore.Mvc;

namespace untitled1.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            // Preserve the original status code in the response
            Response.StatusCode = statusCode;

            return statusCode switch
            {
                404 => View("NotFound"),
                403 => View("Forbidden"),
                _   => View("General")
            };
        }
    }
}
