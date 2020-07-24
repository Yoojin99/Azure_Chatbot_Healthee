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
using AdaptiveCards;
using System.IO;

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
            //QnAMaker �ҷ�����
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

            //��� Ȯ�ο�
            await turnContext.SendActivityAsync(MessageFactory.Text("���� ���� " + ModeManager.mode + "�Դϴ�,"), cancellationToken);

            //����� �Է� �ؽ�Ʈ
            string msg_from_user = turnContext.Activity.Text;
            msg_from_user = msg_from_user.Replace(" ","");

            //���� Dialog running ���� �ƴҶ�
            if (MainDialog.is_running_dialog == 0)
            {
                if (msg_from_user.Contains("���õ"))
                {
                    ModeManager.mode = (int)ModeManager.Modes.RecommendExercise;
                    await turnContext.SendActivityAsync(MessageFactory.Text("� ��õ ��� ����"), cancellationToken);
                }
                else if (msg_from_user.Contains("����"))
                {
                    ModeManager.mode = (int)ModeManager.Modes.Record;
                    await turnContext.SendActivityAsync(MessageFactory.Text("� ��� ����"), cancellationToken);
                }
                else if (msg_from_user.Contains("������õ"))
                {
                    ModeManager.mode = (int)ModeManager.Modes.RecommendFood;
                    await turnContext.SendActivityAsync(MessageFactory.Text("���� ��õ ����"), cancellationToken);
                }
                else if (msg_from_user.Contains("��ⱸ��õ"))
                {
                    ModeManager.mode = (int)ModeManager.Modes.RecommendEquipment;
                    await turnContext.SendActivityAsync(MessageFactory.Text("�ⱸ ��õ ����"), cancellationToken);
                }
                else if (msg_from_user.Contains("ĳ����"))
                {
                    ModeManager.mode = (int)ModeManager.Modes.CheckCharacterState;
                    await turnContext.SendActivityAsync(MessageFactory.Text("ĳ����"), cancellationToken);
                }
                else if (msg_from_user.Contains("��Ϻ���"))
                {
                    ModeManager.mode = (int)ModeManager.Modes.SeeMyRecord;
                    await turnContext.SendActivityAsync(MessageFactory.Text("��� ����"), cancellationToken);
                }
                else
                {
                    //ModeManager.mode = (int)ModeManager.Modes.ShowFunction; //�ӽ� Ʃ�丮��
                }
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);

            }
            else //���̾�α� �������϶�
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("EchoBot - ���̾�α� �������Դϴ�."), cancellationToken);
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
            }

            /*
            if (MainDialog.tutorial == 0)
                //����
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
            else {
                var options = new QnAMakerOptions { Top = 1 };

                // The actual call to the QnA Maker service.
                var response = await qnaMaker.GetAnswersAsync(turnContext, options);


                if (response != null && response.Length > 0)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(response[0].Answer), cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("No QnA Maker answers were found."), cancellationToken);
                }

            }*/

        }

    }
}
