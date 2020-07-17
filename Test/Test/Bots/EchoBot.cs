// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.9.2

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Builder.Dialogs;
using Test.Dialogs;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace Test.Bots
{
    public class EchoBot<T> : ActivityHandler where T : Dialog
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly ILogger Logger;

        public EchoBot(IConfiguration configuration, IHttpClientFactory httpClientFactory, ConversationState conversationState, UserState userState, T dialog, ILogger<EchoBot<T>> logger) {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;

            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default) {

            await base.OnTurnAsync(turnContext, cancellationToken);

            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);

        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            /*
            var httpClient = _httpClientFactory.CreateClient();

            var qnaMaker = new QnAMaker(new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _configuration["QnAKnowledgebaseId"],
                    EndpointKey = _configuration["QnAEndpointKey"],
                    Host = _configuration["QnAEndpointHostName"]
                },
            null,
            httpClient);

            Logger.LogInformation("Running dialog with Message Activity.");

            if (UserProfileDialog.tutorial == 0)
                //����
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
            else {
                var options = new QnAMakerOptions { Top = 1 };

                // The actual call to the QnA Maker service.
                var response = await qnaMaker.GetAnswersAsync(turnContext, options);

                var msg = "������� ok";
                await turnContext.SendActivityAsync(MessageFactory.Text(msg, msg), cancellationToken);

                if (response != null && response.Length > 0)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(response[0].Answer), cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("No QnA Maker answers were found."), cancellationToken);
                }

            }
            */
            var msg = "messageactivityasync";
            await turnContext.SendActivityAsync(MessageFactory.Text(msg, msg), cancellationToken);

            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

        /*
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                    //await DisplayOptionsAsync(turnContext, cancellationToken);
                    await DisplayAnimationCard(turnContext, cancellationToken);
                }
            }
        }
        */

        private static async Task DisplayOptionsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Create a HeroCard with options for the user to interact with the bot.
            var card = new HeroCard
            {
                Title = "Hi!",
                Images = new List<CardImage> { new CardImage("https://w7.pngwing.com/pngs/271/1006/png-transparent-power-strength-gym-fitness-centre-physical-fitness-bodybuilding-physical-strength-workout-boxing-glove-logo-fictional-character.png") } 

            };

            var reply = MessageFactory.Attachment(card.ToAttachment());
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private static async Task DisplayAnimationCard(ITurnContext turnContext, CancellationToken cancellationToken) {

            // Cards are sent as Attachments in the Bot Framework.
            // So we need to create a list of attachments for the reply activity.
            var attachments = new List<Attachment>();

            // Reply to the activity we received with an activity.
            var reply = MessageFactory.Attachment(attachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            var heroCard = new HeroCard
            {
                Title = "#### 1. � ��õ",
                Text = "� ����, �ⱸ ����, � ������ �����Ͽ� ���� ��� ��õ�޾ƺ�����! � ����� ��� ��Ʈ, �ð�, �ڼ� �� ������ ��� ������ �˷��帳�ϴ�.",
                Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "� ��õ�ޱ�", value: "� ��õ ������") },
            };
            reply.Attachments.Add(heroCard.ToAttachment());

            heroCard = new HeroCard
            {
                Title = "#### 2. � ���",
                Text = "��� ���� ����غ�����! � �̸�, �ð�, ��� ������ ����� �� �ֽ��ϴ�. ��� ���� �ϰ� ����� ���� ĳ���Ͱ� �����ؿ�!",
                Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "� ����ϱ�", value: "� ����ҷ�") },
            };
            reply.Attachments.Add(heroCard.ToAttachment());

            heroCard = new HeroCard
            {
                Title = "#### 3. ���� ��õ",
                Text = "��ϰ� ���� ������ ��� ������ ��� ���� ������ ��õ��������!",
                Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "���� ��õ�ޱ�", value: "���� ��õ����") },
            };
            reply.Attachments.Add(heroCard.ToAttachment());

            heroCard = new HeroCard
            {
                Title = "#### 4. � �ⱸ ��õ",
                Text = "��ϰ� ���� ������ � ������ ���� ���� � �ⱸ�� ��õ��������! � �ⱸ�� �� �� �ִ� ���� �˷��帳�ϴ�.",
                Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack,"� �ⱸ ��õ�ޱ�", value: "� �ⱸ ��õ����") },
            };
            reply.Attachments.Add(heroCard.ToAttachment());

            heroCard = new HeroCard
            {
                Title = "#### 5. �˸�",
                Text = "��� �ð��� �����Ͽ� ������ �ð��� �˸��� ��������.w",
                Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "�˸� �����ϱ�" ,value: "�˸� �����ҷ�") },
            };
            reply.Attachments.Add(heroCard.ToAttachment());


            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
