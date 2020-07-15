// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class QnABot<T> : ActivityHandler where T : Microsoft.Bot.Builder.Dialogs.Dialog
    {
        protected readonly BotState ConversationState;
        protected readonly Microsoft.Bot.Builder.Dialogs.Dialog Dialog;
        protected readonly BotState UserState;
        

        private BotState _conversationState;
        private BotState _userState;

        public QnABot(ConversationState conversationState, UserState userState, T dialog)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
           // Run the Dialog with the new message Activity. QnA '������ ������?'�� ���� �亯 
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome!"), cancellationToken);
                    await DisplayOptionsAsync(turnContext, cancellationToken);
                }
            }
        }

        private static async Task DisplayOptionsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Create a HeroCard with options for the user to interact with the bot.
            var card = new HeroCard
            {
                Images = new List<CardImage> { new CardImage("https://w7.pngwing.com/pngs/271/1006/png-transparent-power-strength-gym-fitness-centre-physical-fitness-bodybuilding-physical-strength-workout-boxing-glove-logo-fictional-character.png") }

                Images = new List<CardImage> { new CardImage("https://1.bp.blogspot.com/-055dCu7uAU0/XROZCKyjPPI/AAAAAAAAgzU/jLWMbfTdJXIHxxFC_0XrURGd0DUI5-1aQCEwYBhgL/s1600/2.jpg") }

            };

            var reply = MessageFactory.Attachment(card.ToAttachment());
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}