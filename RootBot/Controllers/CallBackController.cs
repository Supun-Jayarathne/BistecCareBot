using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OpenUrlRedirectBot.Controllers
{
    [Route("")]
    [ApiController]
    public class CallBackController : ControllerBase
    {
        [AllowAnonymous]
        [DisableRequestSizeLimit]
        [HttpGet("callback")]
        public async Task<IActionResult> AuthCallback([FromQuery] string code)
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(300);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri("https://bisteccareltddev.b2clogin.com/bisteccareltddev.onmicrosoft.com/b2c_1a_signup_signin/oauth2/v2.0/token");

            using MultipartFormDataContent multipartContent = new();
            multipartContent.Add(new StringContent(code, Encoding.UTF8, MediaTypeNames.Text.Plain), "code");
            multipartContent.Add(new StringContent("authorization_code", Encoding.UTF8, MediaTypeNames.Text.Plain), "grant_type");
            multipartContent.Add(new StringContent("047aRKq64Yt8vGLqTWWuDSC5kSvXJGIr7zXpZgDSR10", Encoding.UTF8, MediaTypeNames.Text.Plain), "code_verifier");
            multipartContent.Add(new StringContent("https://supunrootbotapp.azurewebsites.net/callback", Encoding.UTF8, MediaTypeNames.Text.Plain), "redirect_uri");
            multipartContent.Add(new StringContent("4521dfe1-0635-4a44-bd4a-f4b42fca2067", Encoding.UTF8, MediaTypeNames.Text.Plain), "client_id");
            multipartContent.Add(new StringContent("openid profile email offline_access https://bisteccareltddev.onmicrosoft.com/4521dfe1-0635-4a44-bd4a-f4b42fca2067/Calendar.Read", Encoding.UTF8, MediaTypeNames.Text.Plain), "scope");
            using var response = await client.PostAsync("https://bisteccareltddev.b2clogin.com/bisteccareltddev.onmicrosoft.com/b2c_1a_signup_signin/oauth2/v2.0/token", multipartContent);

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                dynamic dynamicObject = JsonConvert.DeserializeObject(result);
                // set the access token to inmemory storage here

                return Ok(result);
            }
            else
            {
                // Handle the error condition
                return StatusCode((int)response.StatusCode);
            }

        }
    }

}
