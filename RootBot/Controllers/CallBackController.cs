using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Web;

namespace OpenUrlRedirectBot.Controllers
{
    [Route("callback")]
    [ApiController]
    public class CallBackController : ControllerBase
    {
        [AllowAnonymous]
        [DisableRequestSizeLimit]
        //[HttpPost]
        [HttpGet()]
        public IActionResult IamCallback([FromQuery] string state)
        {
            var request = HttpContext.Request;
            //var absoluteUri = string.Concat(
            //            request.Scheme,
            //            "://",
            //            request.Host.ToUriComponent(),
            //            request.PathBase.ToUriComponent(),
            //            request.Path.ToUriComponent(),
            //            request.QueryString.ToUriComponent());
            //var _baseURL = $"{request.Scheme}://{request.Host}";
            //var fullUrl = _baseURL + HttpContext.Request.Path + HttpContext.Request.QueryString;
            //var baseUrl = $"{request.Scheme}://{request.Host}:{request.PathBase.ToUriComponent()}";
            //var requestFeature = HttpContext.Features.Get<IHttpRequestFeature>();

            //return new Uri(requestFeature.RawTarget);

            string id_token = HttpUtility.ParseQueryString(request.QueryString.ToUriComponent()).Get("id_token");
            var result = "Hit the end point!, This is id_token " + id_token;


            return Ok(result);
        }
    }

}
