// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using static Microsoft.Bot.Builder.Dialogs.Choices.Channel;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples.Dialog
{
    /// <summary>
    /// This is an example root dialog. Replace this with your applications.
    /// </summary>
    public class RootDialog : ComponentDialog
    {
        /// <summary>
        /// QnA Maker initial dialog
        /// </summary>
        private const string InitialDialog = "initial-dialog";
        private readonly UserState _userState;
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;
        /// <summary>
        /// Initializes a new instance of the <see cref="RootDialog"/> class.
        /// </summary>
        /// <param name="services">Bot Services.</param>
        
        
        public RootDialog(IBotServices services, UserState userState)
        //public RootDialog(IBotServices services)
            : base("root")
        {
            _userState = userState;

            AddDialog(new QnAMakerBaseDialog(services));


            // userprofile.cs �� �ִ� �ڵ带 rootdialog �� �������� �� ����. 
           // AddDialog(new UserProfileDialog(userState));



            AddDialog(new WaterfallDialog(InitialDialog)
              .AddStep(InitialStepAsync));
           

           // �� �ڵ����ϸ� QnAMaker�� ���� ����
           /* AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync,
            }));
             */



            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
               NameStepAsync,
               TargetStepAsync,
               KnowHowStepAsync,
               PreWeightStepAsync,
               PostWeightStepAsync,
               DateStepAsync,
               // avatar profile ����
               ShowAvatarStepAsync,
               AvatarNameStepAsync,
               AlarmStepAsync,
               AvatarSummaryStepAsync,

            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
           // AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), WeightPromptValidatorAsync));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

       

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(QnAMakerDialog), null, cancellationToken);

        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userInfo = (UserProfile)stepContext.Result;

            string status = "You are signed up to review "
                + (userInfo.CompaniesToReview.Count is 0 ? "no companies" : string.Join(" and ", userInfo.CompaniesToReview))
                + ".";

            await stepContext.Context.SendActivityAsync(status);

            var accessor = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            await accessor.SetAsync(stepContext.Context, userInfo, cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }


        // -----
        private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the user's response is received.

            return await stepContext.PromptAsync(nameof(TextPrompt),
               new PromptOptions
               {
                   Prompt = MessageFactory.Text("�ȳ��ϼ���! �����а� �Բ� ��� Healthee�Դϴ�! �̸��� �˷��ּ���!"),
               }, cancellationToken);
        }

        private async Task<DialogTurnResult> TargetStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["UserName"] = ((string)stepContext.Result);

            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks {stepContext.Result}."), cancellationToken);


            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("��� ��ǥ�� �����? ���̾�Ʈ, �ٷ�, ������ �� �ϳ��� ����ּ���!"),
                Choices = ChoiceFactory.ToChoices(new List<string> { "���̾�Ʈ", "�ٷ� �", "������" }),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> KnowHowStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["Target"] = ((FoundChoice)stepContext.Result).Value;

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
               new PromptOptions
               {
                   Prompt = MessageFactory.Text("��� ���� �غ��̳���? �� �� �� �� �ϳ��� ����ּ���! "),
                   Choices = ChoiceFactory.ToChoices(new List<string> { "��", "��", "��" }),
               }, cancellationToken);
        }

        private async Task<DialogTurnResult> PreWeightStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["KnowHow"] = ((FoundChoice)stepContext.Result).Value;

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("���� ü���� �˷��ּ���"),
                RetryPrompt = MessageFactory.Text("0���� ũ�� 300���� ���� ��ġ�� �����ּ���."),
            };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);

        }
        private async Task<DialogTurnResult> PostWeightStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["PreWeight"] = (string)stepContext.Result;

            //  var msg = (int)stepContext.Values["age"] == -1 ? "No age given." : $"I have your age as {stepContext.Values["age"]}.";

            // We can send messages to the user at any point in the WaterfallStep.
            //  await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("��ǥ ü���� �˷��ּ���"),
                RetryPrompt = MessageFactory.Text("0���� ũ�� 300���� ���� ��ġ�� �����ּ���."),
            };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> DateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["PostWeight"] = (string)stepContext.Result;

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("��ǥ �Ⱓ�� �˷��ּ���"),
                RetryPrompt = MessageFactory.Text("��¥�������� �ۼ����ּ���."), // �Է¹��� ��¥�� ����κ��� D-day ���
            };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);

        }

        private async Task<DialogTurnResult> ShowAvatarStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["Date"] = (string)stepContext.Result;

            // Get the current profile object from user state.
            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("{UserProfile.UserName}�԰� �Բ� ��� ģ������!") }, cancellationToken);
            // {UserProfile.Username}�� �Է��� ���� �ҷ����� �����ϱ� 

        }

        private async Task<DialogTurnResult> AvatarNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["seenAvatar"] = ((string)stepContext.Result); // or  (bool)stepContext.Result)


            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("�׷�����. ĳ������ �̸��� �������ּ���!"),
                RetryPrompt = MessageFactory.Text("The value entered must be greater than 0 and less than 150."),
            };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> AlarmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["AvatarName"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("��ÿ� {UsrProfile.AvatarName}�� ���� ��Ͻǰǰ���?") }, cancellationToken);
            // {UserProfile.Username}�� �Է��� ���� �ҷ����� �����ϱ� 
            // �˶� �����ϴ� ��? 
        }

        private async Task<DialogTurnResult> AvatarSummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["AlarmSetup"] = (string)stepContext.Result;

            // Get the current profile object from user state.
            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            userProfile.UserName = (string)stepContext.Values["UserName"];
            userProfile.Target = (string)stepContext.Values["Target"];
            userProfile.KnowHow = (string)stepContext.Values["KnowHow"];
            userProfile.PreWeight = (string)stepContext.Values["PreWeight"];
            userProfile.PostWeight = (string)stepContext.Values["PostWeight"];
            userProfile.Date = (string)stepContext.Values["Date"];
            userProfile.AvatarName = (string)stepContext.Values["AvatarName"];

            var msg = $"{userProfile.UserName}�� {userProfile.Date}�� �Ⱓ���� Healthee�� {userProfile.Target} ��ǥ�� �����ϰ� �����غ��ƿ�! "
                        + $"{ userProfile.PreWeight} ���� { userProfile.PostWeight}���� ü�߰����� �̷�����ſ���!"
                        + $"{ userProfile.UserName}���� ĳ���� { userProfile.AvatarName} ��ȭ�� �����ܺ��ּ���!";

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);

        }

        private static Task<bool> AgePromptValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            // This condition is our validation rule. You can also change the value at this point.
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 0 && promptContext.Recognized.Value < 300);
        }

    }
}
