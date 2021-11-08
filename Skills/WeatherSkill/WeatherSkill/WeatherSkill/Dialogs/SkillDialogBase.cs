// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions.Authentication;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Bot.Solutions.Skills;
using Microsoft.Bot.Solutions.Util;
using Microsoft.Extensions.DependencyInjection;
using WeatherSkill.Models;
using WeatherSkill.Responses.Shared;
using WeatherSkill.Services;
using WeatherSkill.Utilities;

namespace WeatherSkill.Dialogs
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
            TemplateManager = serviceProvider.GetService<LocaleTemplateManager>();
            ServiceManager = serviceProvider.GetService<IServiceManager>();

            var conversationState = serviceProvider.GetService<ConversationState>();
            StateAccessor = conversationState.CreateProperty<SkillState>(nameof(SkillState));
        }

        protected BotSettings Settings { get; }

        protected BotServices Services { get; }

        protected IStatePropertyAccessor<SkillState> StateAccessor { get; }

        protected LocaleTemplateManager TemplateManager { get; }

        protected IServiceManager ServiceManager { get; }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            await GetLuisResultAsync(innerDc, cancellationToken);
            await DigestLuisResultAsync(innerDc, cancellationToken);
            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            await GetLuisResultAsync(innerDc, cancellationToken);
            await DigestLuisResultAsync(innerDc, cancellationToken);
            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        protected async Task<DialogTurnResult> GetAuthTokenAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            try
            {
                return await sc.PromptAsync(nameof(MultiProviderAuthDialog), new PromptOptions());
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

        protected async Task<DialogTurnResult> AfterGetAuthTokenAsync(WaterfallStepContext sc, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // When the user authenticates interactively we pass on the tokens/Response event which surfaces as a JObject
                // When the token is cached we get a TokenResponse object.
                if (sc.Result is ProviderTokenResponse providerTokenResponse)
                {
                    var state = await StateAccessor.GetAsync(sc.Context, cancellationToken: cancellationToken);
                    state.Token = providerTokenResponse.TokenResponse.Token;
                }

                return await sc.NextAsync();
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

        protected Task<bool> AuthPromptValidatorAsync(PromptValidatorContext<TokenResponse> promptContext, CancellationToken cancellationToken)
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

        // Helpers
        protected async Task GetLuisResultAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            if (dc.Context.Activity.Type == ActivityTypes.Message)
            {
                var state = await StateAccessor.GetAsync(dc.Context, () => new SkillState(), cancellationToken: cancellationToken);

                // Get luis service for current locale
                var localeConfig = Services.GetCognitiveModels();
                var luisService = localeConfig.LuisServices["WeatherSkill"];

                // Get intent and entities for activity
                var result = await luisService.RecognizeAsync<WeatherSkillLuis>(dc.Context, CancellationToken.None);
                state.LuisResult = result;
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
            await sc.Context.SendActivityAsync(TemplateManager.GenerateActivity(SharedResponses.ErrorMessage), cancellationToken);

            // clear state
            var state = await StateAccessor.GetAsync(sc.Context, cancellationToken: cancellationToken);
            state.Clear();
        }

        protected async Task DigestLuisResultAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            try
            {
                List<string> regExpArr = new List<string> { @"^[0-9A-Za-z]{3}$", @"^[A-Za-z]{1}[0-9]{3}$", @"^[0-9]{4}$", @"^[0-9]{5}$", @"^[A-Za-z]{1}[0-9]{4}$", @"^[A-Za-z]{2}[-\s][0-9]{2}$", @"^[A-Za-z]{2}[0-9]{3}$", @"^[0-9]{2}[-\s][0-9]{3}$", @"^[0-9]{3}[-\s][0-9]{2}$", @"^[A-Za-z]{2}[0-9]{4}$", @"^[0-9]{6}$", @"^[0-9]{3}[-\s][0-9]{3}$", @"^[A-Za-z]{3}[0-9]{4}$", @"^[A-Za-z]{2}[-\s][0-9]{4}$", @"^[A-Za-z]{5}[0-9]{2}$", @"^([A-Za-z]{2}[0-9]{1})[-\s]([0-9]{1}[A-Za-z]{2})$", @"^[0-9]{7}$", @"^([A-Za-z]{1}[0-9]{1}[A-Za-z]{1})[-\s]([0-9]{1}[A-Za-z]{2}[0-9]{1})$", @"^[0-9]{4}[-\s][A-Za-z]{2}$", @"^[A-Za-z]{2}[0-9]{5}$", @"^[A-Za-z]{2}[-\s][0-9]{4}$", @"^[0-9]{4}[-\s][A-Za-z]{2}$", @"^([A-Za-z]{2}[0-9]{2})[-\s]([0-9]{1}[A-Za-z]{2})$", @"^[A-Za-z]{4}[-\s]([0-9]{1}[A-Za-z]{2})$", @"^[0-9]{4}[-\s][0-9]{3}$", @"^([A-Za-z]{3})[0-9]{1})[-\s]([0-9]{1}[A-Za-z]{2})$", @"^[A-Za-z]{1}[0-9]{7}$", @"^[A-Za-z]{3}[-\s][0-9]{4}$", @"^[0-9]{3}[-\s][0-9]{4}$", @"^([A-Za-z]{2}[0-9]{1})[-\s]([0-9]{4})$", @"^([A-Za-z]{2}[0-9]{1}[A-Za-z]{1})[-\s]([0-9]{1}[A-Za-z]{2}[0-9]{1})$", @"^([A-Za-z]{1})[0-9]{4}[A-Za-z]{3}$", @"^[0-9]{5}[-\s][0-9]{3}$", @"^[0-9]{5}[-\s][0-9]{4}$", @"^([A-Za-z]{2}[0-9]{1}[A-Za-z]{1})[-\s]([0-9]{1}[A-Za-z]{2})$", @"^([A-Za-z]{2}[0-9]{1}[A-Za-z]{1})[-\s]([A-Za-z]{3})$", @"^([A-Za-z]{2}[0-9]{1}[A-Za-z]{1})[-\s]([0-9]{1}[A-Za-z]{3})$", @"^[A-Za-z]{2}[0-9]{1}[A-Za-z]{1}[-\s][0-9]{1}[A-Za-z]{2}$" };

                List<string> regKeyword = new List<string> { "forecast weather at ", "weather at ", "forecast ", "weather" };

                var state = await StateAccessor.GetAsync(dc.Context, () => new SkillState(), cancellationToken: cancellationToken);

                if (state.LuisResult != null)
                {
                    var entities = state.LuisResult.Entities;
                    var text = state.LuisResult.Text;

                    if (entities.geographyV2 != null)
                    {
                        state.Geography = entities.geographyV2[0].Location;
                        if (entities.geographyV2.Length > 1)
                        {
                            state.Country = entities.geographyV2[1].Location;
                        }
                    }

                    if (entities.postalCode != null)
                    {
                        //state.zipCode = entities.postalCode[0].ToUpper();

                        //var t = regExpArr.Find(x => Regex.IsMatch(entities.postalCode[0], x));
                        //Console.WriteLine(t);

                        if (string.IsNullOrEmpty(state.zipCode) && entities.postalCode[0].Length <= 10)
                        {
                            state.zipCode = entities.postalCode[0].ToUpper();
                        }

                        bool isRegexMatch = false;
                        foreach (var item in regExpArr)
                        {
                            Regex rg = new Regex(item);
                            isRegexMatch = rg.IsMatch(entities.postalCode[0]);
                            if (isRegexMatch)
                            {
                                state.zipCode = entities.postalCode[0].ToUpper();
                            }
                        }
                    }

                    if (text != null)
                    {
                        string txtCountry = string.Empty;
                        string[] weatherTxt = text.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        if (text.Contains(","))
                        {
                            txtCountry = weatherTxt[weatherTxt.Length - 1].Trim();
                            state.Country = txtCountry;
                        }
                        //txtCountry = weatherTxt[weatherTxt.Length - 1].Trim();
                        //state.Country = txtCountry;

                    }

                    if (string.IsNullOrEmpty(state.Geography) && string.IsNullOrEmpty(state.Country) && string.IsNullOrEmpty(state.zipCode))
                    {

                        foreach (var item in regKeyword)
                        {
                            if (text.Contains(item))
                            {
                                var x = Regex.Split(text, item);
                                if (x.Length == 2 && string.IsNullOrEmpty(state.Geography))
                                {
                                    state.Geography = x[x.Length - 1];
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // put log here
            }
        }

        // Workaround until adaptive card renderer in teams is upgraded to v1.2
        protected string GetDivergedCardName(ITurnContext turnContext, string card)
        {
            if (Channel.GetChannelId(turnContext) == Channels.Msteams)
            {
                return card + ".1.0";
            }
            else
            {
                return card;
            }
        }
    }
}