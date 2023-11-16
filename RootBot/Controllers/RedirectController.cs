using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace OpenUrlRedirectBot.Controllers
{
    [Route("Redirect")]
    [ApiController]
    public class RedirectController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(string url)
        {
            // TODO: Log url here
            return Redirect(url);
        }


        //[AllowAnonymous]
        //[DisableRequestSizeLimit]
        //[HttpPost]
        //[HttpGet]
        ////[HttpGet("callback")]
        //public  IActionResult IamCallback(string id_token)
        //{
        //    //
        //    // Read external identity from the temporary cookie
        //    //
        //    //var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        //    //if (result?.Succeeded != true)
        //    //{
        //    //    throw new Exception("Nein");
        //    //}

        //    //var oauthUser = result.Principal;

        //    var result = "Hit the end point"+ id_token;


        //    return Ok(result);
        //}
    }

}
