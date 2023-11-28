// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.RootBot.Objects;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples.SSORootBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        public static readonly string ActiveSkillPropertyName = $"{typeof(MainDialog).FullName}.ActiveSkillProperty";
        private readonly IStatePropertyAccessor<BotFrameworkSkill> _activeSkillProperty;
        private readonly BotFrameworkAuthentication _auth;
        private readonly string _connectionName;
        private readonly BotFrameworkSkill _ssoSkill;
        private readonly ITokenService _tokenService;

        public MainDialog(BotFrameworkAuthentication auth, ConversationState conversationState, SkillConversationIdFactoryBase conversationIdFactory, SkillsConfiguration skillsConfig, IConfiguration configuration, ITokenService tokenService)
            : base(nameof(MainDialog))
        {
            _tokenService = tokenService;
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));

            var botId = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
            if (string.IsNullOrWhiteSpace(botId))
            {
                throw new ArgumentException($"{MicrosoftAppCredentials.MicrosoftAppIdKey} is not set in configuration");
            }

            _connectionName = configuration.GetSection("ConnectionName")?.Value;
            if (string.IsNullOrWhiteSpace(_connectionName))
            {
                throw new ArgumentException("\"ConnectionName\" is not set in configuration");
            }

            // We use a single skill in this example.
            var targetSkillId = "SkillBot";
            if (!skillsConfig.Skills.TryGetValue(targetSkillId, out _ssoSkill))
            {
                throw new ArgumentException($"Skill with ID \"{targetSkillId}\" not found in configuration");
            }

            AddDialog(new ChoicePrompt("ActionStepPrompt"));
            AddDialog(new SsoSignInDialog(_connectionName));
            AddDialog(new SkillDialog(CreateSkillDialogOptions(skillsConfig, botId, conversationIdFactory, conversationState), nameof(SkillDialog)));

            var waterfallSteps = new WaterfallStep[]
            {
                CheckTokenAsync,
                ContinueStepAsync,
                PromptFinalStepAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            // Create state property to track the active skill.
            _activeSkillProperty = conversationState.CreateProperty<BotFrameworkSkill>(ActiveSkillPropertyName);

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        // Helper to create a SkillDialogOptions instance for the CLU skill.
        private SkillDialogOptions CreateSkillDialogOptions(SkillsConfiguration skillsConfig, string botId, SkillConversationIdFactoryBase conversationIdFactory, ConversationState conversationState)
        {
            return new SkillDialogOptions
            {
                BotId = botId,
                ConversationIdFactory = conversationIdFactory,
                SkillClient = _auth.CreateBotFrameworkClient(),
                SkillHostEndpoint = skillsConfig.SkillHostEndpoint,
                ConversationState = conversationState,
                Skill = _ssoSkill,
            };
        }


        // Check the token based on the current sign in status and skill redirection
        private async Task<DialogTurnResult> CheckTokenAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userId = stepContext.Context.Activity?.From?.Id;
            var userTokenClient = stepContext.Context.TurnState.Get<UserTokenClient>();

            // Show different options if the user is signed in on the parent or not.
            var tokens = await userTokenClient.GetUserTokenAsync(userId, _connectionName, stepContext.Context.Activity?.ChannelId, null, cancellationToken);
            // Retrieve the access token from user state


            var token = await _tokenService.GetToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                 var messageText = "First you need to login via bistec care system?";
                await stepContext.Context.SendActivityAsync(messageText, cancellationToken: cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(SsoSignInDialog), null, cancellationToken);

            }


            //else
            //{
            //    var beginSkillActivity = new Activity
            //    {
            //        Type = ActivityTypes.Event,
            //        Name = "CLU"
            //    };

            //    // Save active skill in state (this is use in case of errors in the AdapterWithErrorHandler).
            //    await _activeSkillProperty.SetAsync(stepContext.Context, _ssoSkill, cancellationToken);

            //    return await stepContext.BeginDialogAsync(nameof(SkillDialog), new BeginSkillDialogOptions { Activity = beginSkillActivity }, cancellationToken);
            //}

            return await stepContext.NextAsync(cancellationToken: cancellationToken); ;
        }

        private async Task<DialogTurnResult> ContinueStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userId = stepContext.Context.Activity?.From?.Id;
            var userTokenClient = stepContext.Context.TurnState.Get<UserTokenClient>();

            // Show different options if the user is signed in on the parent or not.
            var tokens = await userTokenClient.GetUserTokenAsync(userId, _connectionName, stepContext.Context.Activity?.ChannelId, null, cancellationToken);
            var token = await _tokenService.GetToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                //    var messageText = "First you need to login via bistec care system?";
                //    await stepContext.Context.SendActivityAsync(messageText, cancellationToken: cancellationToken);
                //    return await stepContext.BeginDialogAsync(nameof(SsoSignInDialog), null, cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);

            }
            else
            {
                var beginSkillActivity = new Activity
                {
                    Type = ActivityTypes.Event,
                    Name = "CLU"
                };

                // Save active skill in state (this is use in case of errors in the AdapterWithErrorHandler).
                await _activeSkillProperty.SetAsync(stepContext.Context, _ssoSkill, cancellationToken);

                return await stepContext.BeginDialogAsync(nameof(SkillDialog), new BeginSkillDialogOptions { Activity = beginSkillActivity }, cancellationToken);
            }
            //return await stepContext.NextAsync(cancellationToken: cancellationToken); ;
        }

        private async Task<DialogTurnResult> PromptFinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Clear active skill in state.
            await _activeSkillProperty.DeleteAsync(stepContext.Context, cancellationToken);

            // Restart the dialog (we will exit when the user says end)
            return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
        }
    }
}
