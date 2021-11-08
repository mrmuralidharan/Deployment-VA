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
    public class TestDiaolog : SkillDialogBase
    {
        private IConfiguration _configuration { get; set; }
       
        public TestDiaolog(IServiceProvider serviceProvider) : base(nameof(PagerEscalationPolicyDiaolog), serviceProvider)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();
            _configuration = builder.Build();

            var steps = new WaterfallStep[]
        {
            GetEscalationPolicyAsync
           

        };
            AddDialog(new WaterfallDialog(nameof(TestDiaolog), steps));
       

            InitialDialogId = nameof(TestDiaolog);
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
                
                string respAttachment = await AdaptiveCardResources.TestCardAsync(_configuration, policiesData);
                List<Attachment> attachs = new List<Attachment>();
                attachs.Add(new Attachment
                {
                
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(respAttachment)
                });
                 await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(attachs),cancellationToken);
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
               



            }
            catch (Exception ex)
            {
                throw ex;
            }

        }



    }
}
