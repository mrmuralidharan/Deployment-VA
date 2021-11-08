using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

namespace HDPagerSkill.Dialogs
{
    public class PagerPolicyMembersDiaolog : SkillDialogBase
    {
        private IConfiguration _configuration { get; set; }
        
        public PagerPolicyMembersDiaolog(IServiceProvider serviceProvider) : base(nameof(PagerPolicyMembersDiaolog), serviceProvider)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();
            _configuration = builder.Build();

            var steps = new WaterfallStep[]
        {
            GetUserResponseAsync

        };
            AddDialog(new WaterfallDialog(nameof(PagerPolicyMembersDiaolog), steps));


            InitialDialogId = nameof(PagerPolicyMembersDiaolog);
        }

        private async Task<DialogTurnResult> GetUserResponseAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                string policynametxt = string.Empty;
                if (stepContext.Context.TurnState.Get<Luis.HdPagerSkillLuis>(StateProperties.SkillLuisResult).Entities.Members != null)
                {
                     policynametxt = (stepContext.Context.TurnState.Get<Luis.HdPagerSkillLuis>(StateProperties.SkillLuisResult)).Entities.Members[0];
                }
                else
                {
                    var policyname = stepContext.Context.Activity.Text;
                    policyname = policyname.ToLower();
                    var pol=policyname.Split("members of");
                    policynametxt = pol[1];
                    policynametxt = policyname.TrimStart();
                }
                    var state = await StateAccessor.GetAsync(stepContext.Context, () => new SkillState(), cancellationToken: cancellationToken);
             
                if (state.IsAction)
                {
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
                var _pagerservice = new HDPagerService();
              
                string policiesData;
                policiesData = await _pagerservice.GetEscalationPoliciesMembers(_configuration,policynametxt);
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
            catch(Exception ex)
            {
                throw ex;
            }

        }

       
        }
    }
