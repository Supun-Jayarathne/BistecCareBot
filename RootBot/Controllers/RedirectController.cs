using Microsoft.AspNetCore.Mvc;

namespace OpenUrlRedirectBot.Controllers
{
    [Route("redirect")]
    [ApiController]
    public class RedirectController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(string url)
        {
            // TODO: Log url here
            return Redirect(url);
        }
    }
}
