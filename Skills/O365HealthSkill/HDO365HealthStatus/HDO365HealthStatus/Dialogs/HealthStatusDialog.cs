using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using HDO365HealthStatus.Services;
using HDO365HealthStatus.Models;
using HDO365HealthStatus.Utilities;
using Microsoft.Bot.Solutions.Responses;
using System.IO;
using AdaptiveCards.Templating;
using Newtonsoft.Json;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder;
using Newtonsoft.Json.Linq;
using HDO365HealthStatus.Helpers;
using System.Net.Http;
using System.Net.Http.Headers;

namespace HDO365HealthStatus.Dialogs
{
    public class HealthStatusDialog : SkillDialogBase
    {
        public HealthStatusDialog(
            IServiceProvider serviceProvider)
            : base(nameof(HealthStatusDialog), serviceProvider)
        {
            var healthStatus = new WaterfallStep[]
            {
                //GetAuthTokenAsync,
                AfterGetAuthTokenAsync,
                //PromptForNameAsync,
                GetHealthStatusResponseAsync,
                //OnTurnAsync,
                EndAsync
            };

            AddDialog(new WaterfallDialog(nameof(HealthStatusDialog), healthStatus));
            //AddDialog(new AdaptiveCardPrompt(AdaptivePromptId));
            AddDialog(new TextPrompt(DialogIds.NamePrompt));

            InitialDialogId = nameof(HealthStatusDialog);

            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();
            _configuration = builder.Build();

        }

        private readonly string _cards = Path.Combine(".", "Content", "HealthStatus.json");

        private IConfiguration _configuration { get; set; }

        static string AdaptivePromptId = "adaptive";

        private async Task<DialogTurnResult> PromptForNameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            var state = await StateAccessor.GetAsync(stepContext.Context, () => new SkillState(), cancellationToken: cancellationToken);
            var prompt = TemplateEngine.GenerateActivityForLocale("HealthCheckMessage");
            
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> GetHealthStatusResponseAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await StateAccessor.GetAsync(stepContext.Context, cancellationToken: cancellationToken);            
            var entityResult = stepContext.Context.TurnState.Get<Luis.Hdo365HealthStatusLuis>(StateProperties.SkillLuisResult);
            if (entityResult.Entities.IncidentDetail != null)
            {
                if (entityResult.Entities.IncidentDetail.Length <= 1)
                {
                    using HttpClient httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", state.Token);
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    var responseMessage = await httpClient.GetAsync(string.Format(Settings.GetIncidentApiUrl, entityResult.Entities.IncidentDetail));
                    responseMessage.EnsureSuccessStatusCode();
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    string replacedResponseString = responseContent.Replace("serviceDegradation", "Service degradation").Replace("serviceOperational", "Service Operational")
                .Replace("extendedRecovery", "Extended recovery").Replace("investigating", "Investigating").Replace("serviceInterruption", "Service interruption")
                .Replace("restoringService", "Restoring service").Replace("investigationSuspended", "Investigation suspended").Replace("serviceRestored", "Service restored")
                .Replace("falsePositive", "False positive");

                    var cardAttachment = await AdaptiveCardResponse.SendIncidentDetailResponseCardAsync(_configuration, replacedResponseString);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                } 
            }
            else
            {
                using HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", state.Token);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                var responseMessage = await httpClient.GetAsync(Settings.HealthStatusApiUrl);
                responseMessage.EnsureSuccessStatusCode();
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                string replacedResponseString = responseContent.Replace("serviceDegradation", "Service degradation").Replace("serviceOperational", "Service Operational")
               .Replace("extendedRecovery", "Extended recovery").Replace("investigating", "Investigating").Replace("serviceInterruption", "Service interruption")
               .Replace("restoringService", "Restoring service").Replace("investigationSuspended", "Investigation suspended").Replace("serviceRestored", "Service restored")
               .Replace("falsePositive", "False positive");
                var cardAttachment = await AdaptiveCardResponse.SendDeptDetailResponseCardAsync(_configuration, replacedResponseString);
                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
            }
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }
        
        private Task<DialogTurnResult> EndAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private static class DialogIds
        {
            public const string NamePrompt = "HealthCheckMessage";
        }

        
    }
}
