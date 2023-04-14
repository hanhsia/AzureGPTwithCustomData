// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.18.1

using Backend.Model;
using Bot.Builder.Community.Cards.Management;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.Threading;

namespace Backend.Bots
{
    // This IBot implementation can run any type of _dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The _conversationState is used by the _dialog system. The _userState isn't, however, it might have been used in a _dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class DialogBot<T> : ActivityHandler
        where T : Dialog
    {
        private readonly T _dialog;
        private readonly BotState _conversationState;
        private readonly BotState _userState;
        private readonly ILogger _logger;
        private static Templates _lgEngine = Templates.ParseFile("./Cards/Cards.lg");
        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
        {
            _conversationState = conversationState;
            _userState = userState;
            _dialog = dialog;
            _logger = logger;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogInformation($"User:{turnContext?.Activity?.Text}");

            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //handle setDefaultServiceProvider button.
            if(await SetDefaultServiceProvider(turnContext))
            {
                return;
            }
            //var replyText = $"Echo: {turnContext.Activity.Text}";
            //await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);

            await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
        }

        private async Task<bool> SetDefaultServiceProvider(ITurnContext<IMessageActivity> turnContext)
        {
            dynamic value = turnContext.Activity.Value;
            if (value !=null)
            {
                var action = value["action"].ToString();
                var userStateAccessors = _userState.CreateProperty<UserData>(nameof(UserData));
                var userData = await userStateAccessors.GetAsync(turnContext, () => new UserData());
                var currentType = userData.ServiceProvider;
                if (action =="SetAzureAsServiceProvider")
                {
                    currentType =ProviderType.Azure;
                }
                else if (action =="SetOpenAiAsServiceProvider")
                {
                    currentType =ProviderType.OpenAi;
                }
                else
                {
                    return false;
                }
                userData.ServiceProvider=currentType;
                
                var cardText = _lgEngine.Evaluate("TextResponseCard", new { text = $"Set Service provider to {currentType} successfully!" });
                var answerActivity = ActivityFactory.FromObject(cardText);
                await turnContext.SendActivityAsync(answerActivity);
                return true;
            }
            return false;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var userStateAccessors = _userState.CreateProperty<UserData>(nameof(UserData));
            var userData = await userStateAccessors.GetAsync(turnContext, () => new UserData());
            var currentType=userData.ServiceProvider;
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    string provider = currentType==ProviderType.Azure ? "Azure" : "OpenAI";
                    string newProvider = currentType==ProviderType.Azure ? "OpenAI" : "Azure";
                    var cardData = new
                    {
                        text = $"Welcome to chat with GPT bot!\r\n Current service provider is {provider}.You can click below buttons to set service provider.",
                        title1 = $"Azure",
                        action1 = "SetAzureAsServiceProvider",
                        title2 = $"OpenAI",
                        action2 = "SetOpenAiAsServiceProvider"
                    };
                    
                    var cardText = _lgEngine.Evaluate("TwoButtonResponseCard", cardData);
                    var answerActivity = ActivityFactory.FromObject(cardText);

                    if (answerActivity != null)
                    {
                        await turnContext.SendActivityAsync(answerActivity, cancellationToken);
                    }
                }
                
            }
        }

        // Load attachment from embedded resource.
        //private Attachment? CreateAdaptiveCardAttachment()
        //{
        //    var cardResourcePath = GetType().Assembly.GetManifestResourceNames().First(name => name.EndsWith("welcomeCard.json"));

        //    using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
        //    {
        //        if (stream != null)
        //        {
        //            using (var reader = new StreamReader(stream))
        //            {
        //                var adaptiveCard = reader.ReadToEnd();
        //                return new Attachment()
        //                {
        //                    ContentType = "application/vnd.microsoft.card.adaptive",
        //                    Content = JsonConvert.DeserializeObject(adaptiveCard, new JsonSerializerSettings { MaxDepth = null }),
        //                };
        //            }
        //        }
        //    }
        //    return null;
        //}
    }
}