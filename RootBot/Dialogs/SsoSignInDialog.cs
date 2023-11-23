// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BDO.Bot.BDOSkillBot.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.RootBot.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.SSORootBot.Dialogs
{
    public class SsoSignInDialog : ComponentDialog
    {
        private readonly ITokenService _tokenService;
        private readonly UserState _userState;

        public SsoSignInDialog(string connectionName, ITokenService tokenService, UserState userState)
            : base(nameof(SsoSignInDialog))
        {
            _tokenService = tokenService;
            _userState = userState;
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] { SignInStepAsync, DisplayTokenAsync }));
            InitialDialogId = nameof(WaterfallDialog);
        }

        

        private async Task<DialogTurnResult> SignInStepAsync(WaterfallStepContext context, CancellationToken cancellationToken)
        {
            var card = CreateAdaptiveCardAttachment();
            var response = MessageFactory.Attachment(card);
            await context.Context.SendActivityAsync(response, cancellationToken);
           
            //await context.Context.SendActivityAsync(f, cancellationToken: cancellationToken);
            return new DialogTurnResult(DialogTurnStatus.Waiting);
            //return await context.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayTokenAsync(WaterfallStepContext context, CancellationToken cancellationToken)
        {
            var token = _tokenService.GetToken();

            // Save the token in the user state based on user ID
            var userId = context.Context.Activity?.From?.Id;
            var userState = _userState;
            var userProperty = userState.CreateProperty<TokenStorage>("AccsessToekn");

            // Get the user ID from the activity
            var userTokens = await userProperty.GetAsync(context.Context, () => new TokenStorage(), cancellationToken);

       
            // Store the token based on user ID
            userTokens.AccessTokens[userId] = token;

            await userState.SaveChangesAsync(context.Context, cancellationToken: cancellationToken);

           
            // Retrieve the access token from user state
           
            await context.Context.SendActivityAsync(token, cancellationToken: cancellationToken);
            return await context.EndDialogAsync(null, cancellationToken);
        }

        private Attachment CreateAdaptiveCardAttachment()
        {
            var cardResourcePath = GetType().Assembly.GetManifestResourceNames().First(name => name.EndsWith("openUrlCard.json")); //"OpenUrlRedirectBot.Resources.openUrlCard.json";

            using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    var adaptiveCard = reader.ReadToEnd();

                    
                    string baseUrl = $"https://bisteccareltddev.b2clogin.com/bisteccareltddev.onmicrosoft.com/b2c_1a_signup_signin/oauth2/v2.0/authorize?client_id=4521dfe1-0635-4a44-bd4a-f4b42fca2067&scope=openid%20profile%20email%20offline_access%20https%3A%2F%2Fbisteccareltddev.onmicrosoft.com%2F4521dfe1-0635-4a44-bd4a-f4b42fca2067%2FCalendar.Read&redirect_uri=http://localhost:4201%2Fcallback&client-request-id=b7e4635f-1844-4421-adfb-e6d10e7ff2e7&response_mode=query&response_type=code&x-client-SKU=msal.js.browser&x-client-VER=2.24.0&client_info=1&code_challenge=0Opx5Tt1Kd-UB3iGV6nIbKQjAoetrarSEaHV6lEVXcM&code_challenge_method=S256&nonce=d417b619-d010-425e-a2cf-464f886a3b71&state=eyJpZCI6IjMxMTcxMzZhLThiZTEtNGM0Ny1iY2Y0LTk3MGI2YjVjMmM1ZiIsIm1ldGEiOnsiaW50ZXJhY3Rpb25UeXBlIjoicmVkaXJlY3QifX0%3D";
                    
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
