// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BDO.Bot.BDOSkillBot.Objects;
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
        private readonly UserState _userState;
        public MainDialog(BotFrameworkAuthentication auth, ConversationState conversationState, SkillConversationIdFactoryBase conversationIdFactory, SkillsConfiguration skillsConfig, IConfiguration configuration, ITokenService tokenService, UserState userState)
            : base(nameof(MainDialog))
        {
            _tokenService = tokenService;
            _userState= userState;
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
            AddDialog(new SsoSignInDialog(_connectionName, _tokenService, _userState));
            AddDialog(new SkillDialog(CreateSkillDialogOptions(skillsConfig, botId, conversationIdFactory, conversationState), nameof(SkillDialog)));

            var waterfallSteps = new WaterfallStep[]
            {
              PromptActionStepAsync,
              ShowToken

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

        private async Task<DialogTurnResult> PromptActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            return await stepContext.BeginDialogAsync(nameof(SsoSignInDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> ShowToken(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var hasGreetedUser = await GetHasGreetedUserFlag(stepContext.Context, cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<bool> GetHasGreetedUserFlag(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<TokenStorage>("AccsessToekn");
            var userTokens = await userStateAccessors.GetAsync(turnContext, () => new TokenStorage(), cancellationToken);
            var userId = turnContext.Activity.From.Id;

            // Check if the user has a token
            if (userTokens.AccessTokens.TryGetValue(userId, out var userToken) && !string.IsNullOrEmpty(userToken))
            {
                // User has a token
                await turnContext.SendActivityAsync("User has a token: " + userToken, cancellationToken: cancellationToken);
                return true;
                // Continue with your logic here...
            }
            else
            {
                // User does not have a token
                await turnContext.SendActivityAsync("User does not have a token.", cancellationToken: cancellationToken);

                return false;

            }
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
