using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.AdaptiveCard.Prompt;
using HDPagerSkill.Cards;
using HDPagerSkill.Models;
using HDPagerSkill.Services;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HDPagerSkill.Dialogs
{
    public class PagerEscalationPolicyDiaolog : SkillDialogBase
    {
        private IConfiguration _configuration { get; set; }
        static string AdaptivePromptId = "adaptive";
        public PagerEscalationPolicyDiaolog(IServiceProvider serviceProvider) : base(nameof(PagerEscalationPolicyDiaolog), serviceProvider)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();
            _configuration = builder.Build();

            var steps = new WaterfallStep[]
        {
            GetEscalationPolicyAsync,
           ShowMembersFormAsync

        };
            AddDialog(new WaterfallDialog(nameof(PagerEscalationPolicyDiaolog), steps));
            AddDialog(new AdaptiveCardPrompt(AdaptivePromptId));

            InitialDialogId = nameof(PagerEscalationPolicyDiaolog);
        }

        private async Task<DialogTurnResult> GetEscalationPolicyAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                var state = await StateAccessor.GetAsync(stepContext.Context, () => new SkillState(), cancellationToken: cancellationToken);
                
                if (state.IsAction)
                {
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
                var _pagerservice = new HDPagerService();
                
                string policiesData;
                policiesData = await _pagerservice.ListEscalationPolicies(_configuration);
                dynamic data = new { pagerdata = policiesData };
                
                string respAttachment = await AdaptiveCardResources.SendescalationpolicyDetailResponseCardAsync(_configuration, policiesData);
                List<Attachment> attachs = new List<Attachment>();
                attachs.Add(new Attachment
                {
                
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(respAttachment)
                });
                var opts = new PromptOptions
                {
                    Prompt = new Activity
                    {
                        Attachments = attachs,
                        Type = ActivityTypes.Message

                    }
                };
                return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private async Task<DialogTurnResult> ShowMembersFormAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var inputvalues = stepContext.Result.ToString();
            var _pagerservice = new HDPagerService();
            JObject data1 = JObject.Parse(inputvalues);
            string policyid = data1.SelectToken("inputchoice").ToString();
            string policiesData;
            policiesData = await _pagerservice.GetEscalationPoliciesMembersforPolicyName(_configuration,policyid);
            dynamic data = new { pagerdata = policiesData };

            string respAttachment = await AdaptiveCardResources.SendshowmembersResponseCardAsync(_configuration, policiesData);
            List<Attachment> attachs = new List<Attachment>();
            attachs.Add(new Attachment
            {

                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(respAttachment)
            });
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(attachs), cancellationToken);
            return await stepContext.NextAsync(cancellationToken: cancellationToken);

        }


    }
}
