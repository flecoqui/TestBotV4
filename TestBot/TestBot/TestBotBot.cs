// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;

namespace TestBot
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service. Transient lifetime services are created
    /// each time they're requested. Objects that are expensive to construct, or have a lifetime
    /// beyond a single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class TestBotBot : IBot
    {
        private const string WelcomeMessage = @"Hey there! I'm your ASH Music Festival bot. I'm here to guide you around the festival!";
        private const string WelcomeMessageQuestion  = @"How would you like to explore the event?";
        // The bot state accessor object. Use this to access specific state properties.
        private readonly WelcomeUserStateAccessors _welcomeUserStateAccessors;
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>                        
        public TestBotBot(WelcomeUserStateAccessors statePropertyAccessor)
        {
            _welcomeUserStateAccessors = statePropertyAccessor ?? throw new System.ArgumentNullException("state accessor can't be null");
        }
        /// <summary>
        /// Sends an adaptive card greeting.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private static async Task SendIntroCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = turnContext.Activity.CreateReply(WelcomeMessageQuestion);
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "FAQs", Type = ActionTypes.ImBack, Value = "FAQs" },
                    new CardAction() { Title = "Band Search", Type = ActionTypes.ImBack, Value = "Band Search" },
                    new CardAction() { Title = "Navigate", Type = ActionTypes.ImBack, Value = "Navigate" },
                },
            };
            await turnContext.SendActivityAsync(reply, cancellationToken);

        }
        /// <summary>
        /// Every conversation turn calls this method.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // use state accessor to extract the didBotWelcomeUser flag
            var didBotWelcomeUser = await _welcomeUserStateAccessors.WelcomeUserState.GetAsync(turnContext, () => new WelcomeUserState());

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Your bot should proactively send a welcome message to a personal chat the first time
                // (and only the first time) a user initiates a personal chat with your bot.
                if (didBotWelcomeUser.DidBotWelcomeUser == false)
                {
                    didBotWelcomeUser.DidBotWelcomeUser = true;
                    // Update user state flag to reflect bot handled first user interaction.
                    await _welcomeUserStateAccessors.WelcomeUserState.SetAsync(turnContext, didBotWelcomeUser);
                    await _welcomeUserStateAccessors.UserState.SaveChangesAsync(turnContext);

                    // the channel should sends the user name in the 'From' object
                    var userName = turnContext.Activity.From.Name;
                    var ID = turnContext.Activity.Recipient.Id;
                    await turnContext.SendActivityAsync($"Welcome {userName} - ID= {ID}.", cancellationToken: cancellationToken);
                    //                    await turnContext.SendActivityAsync($"It is a good practice to welcome the user and provide personal greeting. For example, welcome {userName}.", cancellationToken: cancellationToken);
                    await turnContext.SendActivityAsync(WelcomeMessage, cancellationToken: cancellationToken);
                    await SendIntroCardAsync(turnContext, cancellationToken);
                }
                else
                {
                    // This example hardcodes specific utterances. You should use LUIS or QnA for more advance language understanding.
                    var text = turnContext.Activity.Text;
                    switch (text)
                    {
                        case "FAQs":
                        case "Band Search":
                        case "Navigate":
                            await turnContext.SendActivityAsync($"You said {text}.", cancellationToken: cancellationToken);
                            break;
                        default:
                            await turnContext.SendActivityAsync($"Unexpected choice.", cancellationToken: cancellationToken);
                            await SendIntroCardAsync(turnContext, cancellationToken);
                            break;
                    }
                }
            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (turnContext.Activity.MembersAdded != null)
                {
                    // Iterate over all new members added to the conversation
                    foreach (var member in turnContext.Activity.MembersAdded)
                    {
                        // Greet anyone that was not the target (recipient) of this message
                        // the 'bot' is the recipient for events from the channel,
                        // turnContext.Activity.MembersAdded == turnContext.Activity.Recipient.Id indicates the
                        // bot was added to the conversation.
                        if (member.Id != turnContext.Activity.Recipient.Id)
                        {
                            // Iterate over all new members added to the conversation
                                didBotWelcomeUser.DidBotWelcomeUser = true;
                                // Update user state flag to reflect bot handled first user interaction.
                                await _welcomeUserStateAccessors.WelcomeUserState.SetAsync(turnContext, didBotWelcomeUser);
                                await _welcomeUserStateAccessors.UserState.SaveChangesAsync(turnContext);

                                // the channel should sends the user name in the 'From' object

                                var userName = turnContext.Activity.From.Name;
                                var ID = turnContext.Activity.Recipient.Id;
                                await turnContext.SendActivityAsync($"ConversationUpdate Welcome {userName} - ID= {ID}.", cancellationToken: cancellationToken);

                                //                    await turnContext.SendActivityAsync($"You are seeing this message because this was your first message ever to this bot.", cancellationToken: cancellationToken);
                                //                    await turnContext.SendActivityAsync($"It is a good practice to welcome the user and provide personal greeting. For example, welcome {userName}.", cancellationToken: cancellationToken);
                                await turnContext.SendActivityAsync(WelcomeMessage, cancellationToken: cancellationToken);
                                await SendIntroCardAsync(turnContext, cancellationToken);
                        }
                    }


                }
            }
            // Handle Message activity type, which is the main activity type for shown within a conversational interface
            // Message activities may contain text, speech, interactive cards, and binary or unknown attachments.
            // see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types
            //if (turnContext.Activity.Type == ActivityTypes.Message)
            //{
            //    // Echo back to the user whatever they typed.             
            //    await turnContext.SendActivityAsync("Hello World", cancellationToken: cancellationToken);
            //}
        }
    }
}
