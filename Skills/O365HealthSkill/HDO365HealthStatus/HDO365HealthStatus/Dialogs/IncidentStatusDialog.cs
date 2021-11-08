using HDO365HealthStatus.Models;
using HDO365HealthStatus.Services;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HDO365HealthStatus.Dialogs
{
    public class IncidentStatusDialog:SkillDialogBase
    {
        public IncidentStatusDialog(
           IServiceProvider serviceProvider)
           : base(nameof(IncidentStatusDialog), serviceProvider)
        {
            var healthStatus = new WaterfallStep[]
            {
                //GetAuthTokenAsync,
                //AfterGetAuthTokenAsync,
                //PromptForNameAsync,
                GetIncidentStatusResponseAsync,
                //OnTurnAsync,
                EndAsync
            };

            AddDialog(new WaterfallDialog(nameof(IncidentStatusDialog), healthStatus));
            
            

            InitialDialogId = nameof(IncidentStatusDialog);

            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();
            _configuration = builder.Build();

        }        

        private IConfiguration _configuration { get; set; }

        private async Task<DialogTurnResult> GetIncidentStatusResponseAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = stepContext.Context.TurnState.Get<LuisResult>(StateProperties.SkillLuisResult);
            var state = await StateAccessor.GetAsync(stepContext.Context, cancellationToken: cancellationToken);
            string apiResponseString = state.apiResponseString;
            string replacedResponseString = apiResponseString.Replace("serviceDegradation", "Service degradation").Replace("serviceOperational", "Service Operational")
                .Replace("extendedRecovery", "Extended recovery").Replace("investigating", "Investigating").Replace("serviceInterruption", "Service interruption")
                .Replace("restoringService", "Restoring service").Replace("investigationSuspended", "Investigation suspended").Replace("serviceRestored", "Service restored")
                .Replace("falsePositive", "False positive");

            var cardAttachment = await AdaptiveCardResponse.SendDeptDetailResponseCardAsync(_configuration, replacedResponseString);



            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Attachments = cardAttachment,
                    Type = ActivityTypes.Message,
                }
            };

            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
            //return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
            return await stepContext.NextAsync(cancellationToken: cancellationToken);


        }

        private Task<DialogTurnResult> EndAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

    }
}
