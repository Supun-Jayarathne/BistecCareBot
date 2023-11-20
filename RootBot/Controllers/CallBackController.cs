﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Recognizers.Text.Config;
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
        private readonly Microsoft.Extensions.Configuration.IConfiguration configuration;
        public CallBackController(Microsoft.Extensions.Configuration.IConfiguration iconfig)
        {
            configuration = iconfig;
        }

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
            request.RequestUri = new Uri(configuration.GetSection("Authentication")["tokenUrl"]);

            using MultipartFormDataContent multipartContent = new();
            multipartContent.Add(new StringContent(code, Encoding.UTF8, MediaTypeNames.Text.Plain), "code");
            multipartContent.Add(new StringContent(configuration.GetSection("Authentication")["grant_type"], Encoding.UTF8, MediaTypeNames.Text.Plain), "grant_type");
            multipartContent.Add(new StringContent(configuration.GetSection("Authentication")["code_verifier"], Encoding.UTF8, MediaTypeNames.Text.Plain), "code_verifier");
            multipartContent.Add(new StringContent(configuration.GetSection("Authentication")["redirect_uri"], Encoding.UTF8, MediaTypeNames.Text.Plain), "redirect_uri");
            multipartContent.Add(new StringContent(configuration.GetSection("Authentication")["client_id"], Encoding.UTF8, MediaTypeNames.Text.Plain), "client_id");
            multipartContent.Add(new StringContent(configuration.GetSection("Authentication")["scope"], Encoding.UTF8, MediaTypeNames.Text.Plain), "scope");

            using var response = await client.PostAsync(configuration.GetSection("Authentication")["tokenUrl"], multipartContent);

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
