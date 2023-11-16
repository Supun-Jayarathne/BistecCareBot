// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples.SSORootBot.Dialogs
{
    public class SsoSignInDialog : ComponentDialog
    {
        public SsoSignInDialog(string connectionName)
            : base(nameof(SsoSignInDialog))
        {
            //AddDialog(new OAuthPrompt(nameof(OAuthPrompt), new OAuthPromptSettings
            //{
            //    ConnectionName = connectionName,
            //    Text = "Sign in to the root bot using Azure AD for SSO",
            //    Title = "Sign In"
            //}));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] { SignInStepAsync, DisplayTokenAsync }));
            InitialDialogId = nameof(WaterfallDialog);
        }

        //HttpContext _currentContext;

        //public SsoSignInDialog(IHttpContextAccessor context)
        //{
        //    _currentContext = context.HttpContext;
        //}

        private async Task<DialogTurnResult> SignInStepAsync(WaterfallStepContext context, CancellationToken cancellationToken)
        {
            var card = CreateAdaptiveCardAttachment();
            var response = MessageFactory.Attachment(card);
            await context.Context.SendActivityAsync(response, cancellationToken);
            return await ContinueDialogAsync(context);
            //return await context.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayTokenAsync(WaterfallStepContext context, CancellationToken cancellationToken)
        {
            if (!(context.Result is TokenResponse result))
            {
                await context.Context.SendActivityAsync("No token was provided.", cancellationToken: cancellationToken);
            }
            else
            {
                await context.Context.SendActivityAsync($"Here is your token: {result.Token}", cancellationToken: cancellationToken);
            }

            //return await context.EndDialogAsync(null, cancellationToken);
            return await context.NextAsync(null, cancellationToken);
        }

        private Attachment CreateAdaptiveCardAttachment()
        {
            var cardResourcePath = GetType().Assembly.GetManifestResourceNames().First(name => name.EndsWith("openUrlCard.json")); //"OpenUrlRedirectBot.Resources.openUrlCard.json";

            using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    var adaptiveCard = reader.ReadToEnd();

                    //string baseUrl = $"https://bisteccareltddev.b2clogin.com/bisteccareltddev.onmicrosoft.com/b2c_1a_signup_signin/oauth2/v2.0/authorize?client_id=4521dfe1-0635-4a44-bd4a-f4b42fca2067&scope=openid%20profile%20email%20offline_access%20https%3A%2F%2Fbisteccareltddev.onmicrosoft.com%2F4521dfe1-0635-4a44-bd4a-f4b42fca2067%2FCalendar.Read&redirect_uri=https%3A%2F%2Fadmin.dev.bisteccare.com%2Fcallback&client-request-id=b828c56f-aa10-4f0c-8de8-e54dcb676113&response_mode=fragment&response_type=code&x-client-SKU=msal.js.browser&x-client-VER=2.24.0&client_info=1&code_challenge=EKl6HumzwG1NMnVuuD07B6bLbeHveFEGJb2TbcAho_M&code_challenge_method=S256&nonce=d417b619-d010-425e-a2cf-464f886a3b71&state=eyJpZCI6IjMxMTcxMzZhLThiZTEtNGM0Ny1iY2Y0LTk3MGI2YjVjMmM1ZiIsIm1ldGEiOnsiaW50ZXJhY3Rpb25UeXBlIjoicmVkaXJlY3QifX0%3D";
                    string baseUrl = $"https://bisteccareltddev.b2clogin.com/bisteccareltddev.onmicrosoft.com/b2c_1a_signup_signin/oauth2/v2.0/authorize?client_id=4521dfe1-0635-4a44-bd4a-f4b42fca2067&scope=openid%20profile%20email%20offline_access%20https%3A%2F%2Fbisteccareltddev.onmicrosoft.com%2F4521dfe1-0635-4a44-bd4a-f4b42fca2067%2FCalendar.Read&redirect_uri=https%3A%2F%2Fsupunrootbotapp.azurewebsites.net%2Fcallback&client-request-id=b7e4635f-1844-4421-adfb-e6d10e7ff2e7&response_mode=query&response_type=id_token&x-client-SKU=msal.js.browser&x-client-VER=2.24.0&client_info=1&code_challenge=Q5OP0xfiMXBR9UqvYKf79r4awtJIIGo0hHd8zyID1l4&code_challenge_method=S256&nonce=d417b619-d010-425e-a2cf-464f886a3b71&state=eyJpZCI6IjMxMTcxMzZhLThiZTEtNGM0Ny1iY2Y0LTk3MGI2YjVjMmM1ZiIsIm1ldGEiOnsiaW50ZXJhY3Rpb25UeXBlIjoicmVkaXJlY3QifX0%3D";
                    //string baseUrl = $"https://bisteccareltddev.b2clogin.com/bisteccareltddev.onmicrosoft.com/oauth2/v2.0/authorize?p=B2C_1A_SIGNUP&client_id=4521dfe1-0635-4a44-bd4a-f4b42fca2067&nonce=121212&redirect_uri=https://supunrootbotapp.azurewebsites.net/callback&scope=openid&response_type=id_token&prompt=login";
                    adaptiveCard = adaptiveCard.Replace("[loginUrl]", baseUrl);

                    return new Attachment()
                    {
                        ContentType = "application/vnd.microsoft.card.adaptive",
                        Content = JsonConvert.DeserializeObject(adaptiveCard),
                    };
                }
            }
        }
    }
}
