// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class CreateSessionDialog : CancelAndHelpDialog
    {
        private const string LocationStepMsgText = "Where would you like to create a session?";
        private const string StartTimeStepMsgText = "When would you like to start the session?";

        public CreateSessionDialog()
            : base(nameof(CreateSessionDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                LocationStepAsync,
                StartTimeStepAsync,
                EndTimeStepAsync,
                ConfirmStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> LocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var sessionDetails = (SessionDetails)stepContext.Options;

            if (sessionDetails.Location == null)
            {
                var promptMessage = MessageFactory.Text(LocationStepMsgText, LocationStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(sessionDetails.Location, cancellationToken);
        }

        private async Task<DialogTurnResult> StartTimeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var sessionDetails = (SessionDetails)stepContext.Options;

            //sessionDetails.StartTime = (string)stepContext.Result;

            if (sessionDetails.StartTime == null)
            {
                var promptMessage = MessageFactory.Text(StartTimeStepMsgText, StartTimeStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(sessionDetails.StartTime, cancellationToken);
        }

        private async Task<DialogTurnResult> EndTimeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var sessionDetails = (SessionDetails)stepContext.Options;

            //sessionDetails.EndTime = (string)stepContext.Result;

            if (sessionDetails.EndTime == null || IsAmbiguous(sessionDetails.EndTime))
            {
                return await stepContext.BeginDialogAsync(nameof(DateResolverDialog), sessionDetails.EndTime, cancellationToken);
            }

            return await stepContext.NextAsync(sessionDetails.EndTime, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var sessionDetails = (SessionDetails)stepContext.Options;

            //sessionDetails.TravelDate = (string)stepContext.Result;

            var messageText = $"Please confirm, I have you create a session at: {sessionDetails.Location} from: {sessionDetails.StartTime} to: {sessionDetails.EndTime}. Is this correct?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var sessionDetails = (SessionDetails)stepContext.Options;

                return await stepContext.EndDialogAsync(sessionDetails, cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static bool IsAmbiguous(string timex)
        {
            var timexProperty = new TimexProperty(timex);
            return !timexProperty.Types.Contains(Constants.TimexTypes.Definite);
        }
    }
}
