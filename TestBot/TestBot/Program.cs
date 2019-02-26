// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Builder;
using System;

namespace TestBot
{
    public class WelcomeUserState
    {
        /// <summary>
        /// Gets or sets whether the user has been welcomed in the conversation.
        /// </summary>
        /// <value>The user has been welcomed in the conversation.</value>
        public bool DidBotWelcomeUser { get; set; } = false;
    }
    /// <summary>
    /// This class holds a set of accessors (to specific properties) that the bot uses to access
    /// specific data. These are created as singleton via DI.
    /// </summary>
    public class WelcomeUserStateAccessors
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomeUserStateAccessors"/> class.
        /// Contains the <see cref="UserState"/> and associated <see cref="IStatePropertyAccessor{T}"/>.
        /// </summary>
        /// <param name="userState">The state object that stores the counter.</param>
        public WelcomeUserStateAccessors(UserState userState)
        {
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
        }

        /// <summary>
        /// Gets the <see cref="IStatePropertyAccessor{T}"/> name used for the <see cref="BotBuilderSamples.WelcomeUserState"/> accessor.
        /// </summary>
        /// <remarks>Accessors require a unique name.</remarks>
        /// <value>The accessor name for the WelcomeUser state.</value>
        public static string WelcomeUserName { get; } = $"{nameof(WelcomeUserStateAccessors)}.WelcomeUserState";

        /// <summary>
        /// Gets or sets the <see cref="IStatePropertyAccessor{T}"/> for DidBotWelcome.
        /// </summary>
        /// <value>
        /// The accessor stores if the bot has welcomed the user or not.
        /// </value>
        public IStatePropertyAccessor<WelcomeUserState> WelcomeUserState { get; set; }

        /// <summary>
        /// Gets the <see cref="UserState"/> object for the conversation.
        /// </summary>
        /// <value>The <see cref="UserState"/> object.</value>
        public UserState UserState { get; }
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    // Add Azure Logging
                    logging.AddAzureWebAppDiagnostics();

                    // Logging Options.
                    // There are other logging options available:
                    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1
                    // logging.AddDebug();
                    // logging.AddConsole();
                })

                // Logging Options.
                // Consider using Application Insights for your logging and metrics needs.
                // https://azure.microsoft.com/en-us/services/application-insights/
                // .UseApplicationInsights()
                .UseStartup<Startup>()
                .Build();
    }
}
