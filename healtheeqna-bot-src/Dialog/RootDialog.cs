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
        private object turnContext;

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


            // userprofile.cs 에 있던 코드를 rootdialog 로 가져오는 게 낫다. 
           // AddDialog(new UserProfileDialog(userState));



            AddDialog(new WaterfallDialog(InitialDialog)
              .AddStep(InitialStepAsync));
           

           // 이 코드사용하면 QnAMaker도 시작 안함
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
               // avatar profile 설정
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
            AddDialog(new DateTimePrompt(nameof(DateTimePrompt)));
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
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"안녕하세요! \n\n 여러분과 함께 운동할 Healthee입니다!"), cancellationToken);
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the user's response is received.
            return await stepContext.PromptAsync(nameof(TextPrompt),
               new PromptOptions
               {
                   Prompt = MessageFactory.Text("이름을 알려주세요!"),
               }, cancellationToken);
        }

        private async Task<DialogTurnResult> TargetStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["UserName"] = ((string)stepContext.Result);

            // We can send messages to the user at any point in the WaterfallStep.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text($"{stepContext.Result}님의 운동의 목표가 뭘까요? \n\n 다이어트, 근력, 유연성 중 하나를 골라주세요!"),
                Choices = ChoiceFactory.ToChoices(new List<string> { "다이어트", "근력 운동", "유연성" }),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> KnowHowStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["Target"] = ((FoundChoice)stepContext.Result).Value;

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
               new PromptOptions
               {
                   Prompt = MessageFactory.Text("운동을 많이 해보셨나요? 상 중 하 중 하나를 골라주세요! "),
                   Choices = ChoiceFactory.ToChoices(new List<string> { "상", "중", "하" }),
               }, cancellationToken);
        }

        private async Task<DialogTurnResult> PreWeightStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["KnowHow"] = ((FoundChoice)stepContext.Result).Value;

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("현재 체중을 알려주세요"),
                RetryPrompt = MessageFactory.Text("0보다 크고 300보다 작은 수치로 적어주세요."),
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
                Prompt = MessageFactory.Text("목표 체중을 알려주세요"),
                RetryPrompt = MessageFactory.Text("0보다 크고 300보다 작은 수치로 적어주세요."),
            };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> DateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["PostWeight"] = (string)stepContext.Result;

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("목표 기간을 알려주세요"),
                RetryPrompt = MessageFactory.Text("날짜형식으로 작성해주세요."), // 입력받은 날짜는 현재로부터 D-day 기능
            };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken); // DateTimePrompt 형식 설정?

        }

        // This value will be a TIMEX. And we are only interested in a Date so grab the first result and drop the Time part.
        // TIMEX is a format that represents DateTime expressions that include some ambiguity. e.g. missing a Year.
        //public string TravelDate
        //=> Entities.datetime?.FirstOrDefault()?.Expressions.FirstOrDefault()?.Split('T')[0];


        private async Task<DialogTurnResult> ShowAvatarStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["Date"] = (string)stepContext.Result;

            // Get the current profile object from user state.
            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            userProfile.UserName = (string)stepContext.Values["UserName"];
            /*
                    var card = new HeroCard
                    {
                        Images = new List<CardImage> { new CardImage("https://images.app.goo.gl/WWS6igGQQ6C36Qsh6") }

                    };

                    var reply = MessageFactory.Attachment(card.ToAttachment());
                    await stepContext.Context.SendActivityAsync(reply, cancellationToken);
                    */
           return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"{userProfile.UserName}님과 함께 운동할 친구예요! 어떠신가요?") }, cancellationToken);


        }

        private async Task<DialogTurnResult> AvatarNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["seenAvatar"] = ((string)stepContext.Result); // or  (bool)stepContext.Result)


            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("그렇군요. 캐릭터의 이름을 설정해주세요!"),
                RetryPrompt = MessageFactory.Text("The value entered must be greater than 0 and less than 150."),
            };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> AlarmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            stepContext.Values["AvatarName"] = (string)stepContext.Result;
            
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"몇 시에  {stepContext.Result}과 같이 운동하실건가요?") }, cancellationToken);
            // {UserProfile.Username}이 입력한 값을 불러오록 수정하기 
            // 알람 설정하는 법? 
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

            var msg = $"{userProfile.UserName}님 {userProfile.Date}일 기간동안 Healthee와 {userProfile.Target} 목표를 성실하게 수행해보아요!\n "
                        + $"{ userProfile.PreWeight}에서 { userProfile.PostWeight}으로 체중감량이 이루어질거에요!\n"
                        + $"{ userProfile.UserName}님의 캐릭터 { userProfile.AvatarName} 변화도 눈여겨봐주세요!";

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
