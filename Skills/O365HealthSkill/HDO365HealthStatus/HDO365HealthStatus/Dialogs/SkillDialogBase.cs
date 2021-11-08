// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using HDO365HealthStatus.Models;
using HDO365HealthStatus.Services;
using HDO365HealthStatus.Helpers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions.Authentication;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Bot.Solutions.Skills;
using Microsoft.Bot.Solutions.Util;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using HDO365HealthStatus.Utilities;

namespace HDO365HealthStatus.Dialogs
{
    public class SkillDialogBase : ComponentDialog
    {
        public SkillDialogBase(
             string dialogId,
             IServiceProvider serviceProvider)
             : base(dialogId)
        {
            Settings = serviceProvider.GetService<BotSettings>();
            Services = serviceProvider.GetService<BotServices>();
            TemplateEngine = serviceProvider.GetService<LocaleTemplateManager>();

            // Initialize skill state
            var conversationState = serviceProvider.GetService<ConversationState>();
            StateAccessor = conversationState.CreateProperty<SkillState>(nameof(SkillState));

            //if (!Settings.OAuthConnections.Any())
            //{
            //    throw new Exception("You must configure an authentication connection before using this component.");
            //}

            //AppCredentials oauthCredentials = null;
            //if (Settings.OAuthCredentials != null &&
            //    !string.IsNullOrWhiteSpace(Settings.OAuthCredentials.MicrosoftAppId) &&
            //    !string.IsNullOrWhiteSpace(Settings.OAuthCredentials.MicrosoftAppPassword))
            //{
            //    oauthCredentials = new MicrosoftAppCredentials(Settings.OAuthCredentials.MicrosoftAppId, Settings.OAuthCredentials.MicrosoftAppPassword);
            //}

            //AddDialog(new MultiProviderAuthDialog(Settings.OAuthConnections, null, oauthCredentials));
        }

        protected BotSettings Settings { get; }

        protected BotServices Services { get; }

        protected IStatePropertyAccessor<SkillState> StateAccessor { get; }

        protected LocaleTemplateManager TemplateEngine { get; }

        protected async Task<DialogTurnResult> GetAuthTokenAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            try
            {                
                return await sc.PromptAsync(nameof(MultiProviderAuthDialog), new PromptOptions(), cancellationToken);
            }
            catch (SkillException ex)
            {
                await HandleDialogExceptionsAsync(sc, ex, cancellationToken);
                return new DialogTurnResult(DialogTurnStatus.Cancelled, CommonUtil.DialogTurnResultCancelAllDialogs);
            }
            catch (Exception ex)
            {
                await HandleDialogExceptionsAsync(sc, ex, cancellationToken);
                return new DialogTurnResult(DialogTurnStatus.Cancelled, CommonUtil.DialogTurnResultCancelAllDialogs);
            }
        }

        protected async Task<DialogTurnResult> AfterGetAuthTokenAsync(WaterfallStepContext sc, CancellationToken cancellationToken = default)
        {
            try
            {
                // When the user authenticates interactively we pass on the tokens/Response event which surfaces as a JObject
                // When the token is cached we get a TokenResponse object.
                if (sc.Result is ProviderTokenResponse providerTokenResponse)
                {                    
                    var state = await StateAccessor.GetAsync(sc.Context, () => new SkillState(), cancellationToken: cancellationToken);
                    state.Token = providerTokenResponse.TokenResponse.Token; 
                } else
                {
                    string uri = Settings.MicrosoftLoginUrl + Settings.TenantId + "/oauth2/v2.0/token";
                    FormUrlEncodedContent data = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "Content-Type", "application/x-www-form-urlencoded" },
                        { "client_id", Settings.MicrosoftAppId },
                        { "scope", Settings.GraphDefaultScope },
                        { "client_secret", Settings.MicrosoftAppPassword },
                        { "grant_type", "client_credentials" },
                    });
                    string content = string.Empty;
                    using HttpClient httpClient = new HttpClient();
                    HttpResponseMessage response = await httpClient.PostAsync(uri, data);
                    response.EnsureSuccessStatusCode();
                    content = await response.Content.ReadAsStringAsync();
                    var state = await StateAccessor.GetAsync(sc.Context, () => new SkillState(), cancellationToken: cancellationToken);
                    state.Token = JsonConvert.DeserializeObject<Models.TokenResponse>(content).Access_token;                    
                }

                return await sc.NextAsync(cancellationToken: cancellationToken);
            }
            catch (SkillException ex)
            {
                await HandleDialogExceptionsAsync(sc, ex, cancellationToken);
                return new DialogTurnResult(DialogTurnStatus.Cancelled, CommonUtil.DialogTurnResultCancelAllDialogs);
            }
            catch (Exception ex)
            {
                await HandleDialogExceptionsAsync(sc, ex, cancellationToken);
                return new DialogTurnResult(DialogTurnStatus.Cancelled, CommonUtil.DialogTurnResultCancelAllDialogs);
            }
        }

        // Validators
        protected Task<bool> TokenResponseValidatorAsync(PromptValidatorContext<Activity> pc, CancellationToken cancellationToken)
        {
            var activity = pc.Recognized.Value;
            if (activity != null && activity.Type == ActivityTypes.Event)
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        protected Task<bool> AuthPromptValidatorAsync(PromptValidatorContext<Microsoft.Bot.Schema.TokenResponse> promptContext, CancellationToken cancellationToken)
        {
            var token = promptContext.Recognized.Value;
            if (token != null)
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        // This method is called by any waterfall step that throws an exception to ensure consistency
        protected async Task HandleDialogExceptionsAsync(WaterfallStepContext sc, Exception ex, CancellationToken cancellationToken)
        {
            // send trace back to emulator
            var trace = new Activity(type: ActivityTypes.Trace, text: $"DialogException: {ex.Message}, StackTrace: {ex.StackTrace}");
            await sc.Context.SendActivityAsync(trace, cancellationToken);

            // log exception
            TelemetryClient.TrackException(ex, new Dictionary<string, string> { { nameof(sc.ActiveDialog), sc.ActiveDialog?.Id } });

            // send error message to bot user
            await sc.Context.SendActivityAsync(TemplateEngine.GenerateActivityForLocale("ErrorMessage"), cancellationToken);

            // clear state
            var state = await StateAccessor.GetAsync(sc.Context, () => new SkillState(), cancellationToken: cancellationToken);
            state.Clear();
        }

        
    }
}