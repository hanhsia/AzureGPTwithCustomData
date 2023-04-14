// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.18.1

using Azure.Search.Documents.Models;
using Backend.Model;
using Backend.Service;
using Bot.Builder.Community.Cards.Management;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.LanguageGeneration;

namespace Backend.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly AIService _aiService;
        private readonly CardManager _cardManager;
        private readonly UserState _userState;
        private readonly BotState _conversationState;
        private readonly ILogger _logger;
        private readonly Templates _lgEngine;

        public MainDialog(
            AIService aiService,
            CardManager cardManager,
            UserState userState,
            ConversationState conversationState,
            IConfiguration configuration,
            ILogger<MainDialog> logger)
        {
            _aiService=aiService;
            _cardManager=cardManager;
            _userState = userState;
            _conversationState = conversationState;
            _logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));

            var waterfallSteps = new WaterfallStep[]
            {
                    ProcessStepAsync,
            };
            var loopDialog =
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);

            _lgEngine = Templates.ParseFile("./Cards/Cards.lg");
        }


        private async Task<DialogTurnResult> ProcessStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserData>(nameof(UserData));
            var userData = await userStateAccessors.GetAsync(stepContext.Context, () => new UserData());
            _aiService.CurrentProvider=userData.ServiceProvider;

            var conversationStateAccessors = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            var conversationData = await conversationStateAccessors.GetAsync(stepContext.Context, () => new ConversationData());

            var question = string.Empty;

            //get current question
            var switchAiService = CheckServiceProvider(stepContext);
            if (switchAiService)
            {
                question=conversationData.ConversationHistory[conversationData.ConversationHistory.Count-1].User;
            }
            else
            {
                question =stepContext.Context.Activity.Text;
            }

            //generate answer using azure search and gpt
            var answer = await GenerateAnswerAsync(question, conversationData.ConversationHistory);
            if (!string.IsNullOrEmpty(answer))
            {
                //send response to the user
                if (switchAiService)
                {
                    await SendTextResponseAsync(stepContext, answer, cancellationToken);
                    _aiService.CurrentProvider=_aiService.CurrentProvider==ProviderType.OpenAi ? ProviderType.Azure : ProviderType.OpenAi;
                }
                else
                {
                    await SendOneButtonResponseAsync(stepContext, conversationData, answer, cancellationToken);
                }
            }
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<string> GenerateAnswerAsync(string question, SynchronizedCollection<ChatTurn> history)
        {
            string answer = string.Empty;
            string internalEnglishQuestion = question;
            string internalChineseQuestion = question;


            //get full context question usge gpt summary
            (internalEnglishQuestion,internalChineseQuestion) = await _aiService.GetFullContextQuestionAsync(question, history);

            //get refrence content from azure search
            string refContent = await _aiService.GetReferenceContentAsync(internalEnglishQuestion,QueryLanguage.EnUs);
            if(string.IsNullOrEmpty(refContent))
            {
                refContent = await _aiService.GetReferenceContentAsync(internalChineseQuestion, QueryLanguage.ZhCn);
            }
            //gerenater answer from chat gpt
            answer=await _aiService.GetChatGPTAnswerAsync(question, history, refContent);
            
            _logger.LogInformation($"Bot:{answer}");

            //to escapt back slash and quota. otherwise generating adaptive card will fail.
            return answer.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }


        private bool CheckServiceProvider(WaterfallStepContext stepContext)
        {
            dynamic value = stepContext.Context.Activity.Value;
            if (value !=null)
            {
                var action = value["action"].ToString();

                if (action =="SwitchServiceProvider")
                {

                    _aiService.CurrentProvider=_aiService.CurrentProvider==ProviderType.OpenAi ? ProviderType.Azure : ProviderType.OpenAi;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        private async Task<string> SendTextResponseAsync(WaterfallStepContext stepContext, string? answer, CancellationToken cancellationToken)
        {
            var source = _aiService.CurrentProvider==ProviderType.OpenAi ? "OpenAI" : "Azure";
            var answerWithSource = $"This is the answer from {source}:\\n\\n{answer}";

            var cardText = _lgEngine.Evaluate("TextResponseCard", new { text = answerWithSource });
            var answerActivity = ActivityFactory.FromObject(cardText);
            return (await stepContext.Context.SendActivityAsync(answerActivity, cancellationToken)).Id;
        }

        private async Task<string?> SendOneButtonResponseAsync(WaterfallStepContext stepContext, ConversationData conversationData, string answer, CancellationToken cancellationToken)
        {
            var cardData = new
            {
                text = answer,
                title = _aiService.CurrentProvider==ProviderType.OpenAi ? "Peek Azure's answer" : "Peek OpenAI's answer",
                action = "SwitchServiceProvider"
            };
            var cardText = _lgEngine.Evaluate("OneButtonResponseCard", cardData);
            var answerActivity = ActivityFactory.FromObject(cardText);

            if (answerActivity != null)
            {
                object adaptiveCard = answerActivity.Attachments[0].Content;
                DataId.SetInAdaptiveCard(ref adaptiveCard, new DataIdOptions(DataIdScopes.Card));
                answerActivity.Attachments[0].Content = adaptiveCard;
                var response = await stepContext.Context.SendActivityAsync(answerActivity, cancellationToken);

                //save user question and bot response to the conversation data.
                var pair = new ChatTurn() { 
                    User=stepContext.Context.Activity.Text, 
                    Assistant=answer };
                conversationData.ConversationHistory.Add(pair);
                var count = _aiService.GetTokenCount(string.Join("\n",conversationData.ConversationHistory.Select(x => x.User+"\n"+x.Assistant)));
                while (count>2500)
                {
                    conversationData.ConversationHistory.RemoveAt(0);
                    count = _aiService.GetTokenCount(string.Join("\n", conversationData.ConversationHistory.Select(x => x.User+"\n"+x.Assistant)));
                }
                return response.Id;
            }
            return null;
        }

    }
}