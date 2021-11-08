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
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HDPagerSkill.Dialogs
{
    public class PagerCreateIncidentDiaolog : SkillDialogBase
    {
        static string AdaptivePromptId = "adaptive";
      
        private IConfiguration _configuration { get; set; }
        private  LocaleTemplateManager _templateEngine;
        public PagerCreateIncidentDiaolog(IServiceProvider serviceProvider) : base(nameof(PagerEscalationPolicyDiaolog), serviceProvider)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();
            _configuration = builder.Build();
           

            var steps = new WaterfallStep[]
        {
            EscalationPromptSync,
            ResultUserFormAsync


        };
            AddDialog(new WaterfallDialog(nameof(PagerEscalationPolicyDiaolog), steps));
            AddDialog(new AdaptiveCardPrompt(AdaptivePromptId));


            InitialDialogId = nameof(PagerEscalationPolicyDiaolog);
        }

        private async Task<DialogTurnResult> EscalationPromptSync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
           
                try
                {
                string policyname = string.Empty;
                if (stepContext.Context.TurnState.Get<Luis.HdPagerSkillLuis>(StateProperties.SkillLuisResult).Entities.PolicyName != null)
                {
                    policyname = (stepContext.Context.TurnState.Get<Luis.HdPagerSkillLuis>(StateProperties.SkillLuisResult)).Entities.PolicyName[0];
                }
                else
                {
                    var policynametxt = stepContext.Context.Activity.Text;
                    policynametxt = policynametxt.ToLower();
                    var pol = policynametxt.Split("page:");
                    policyname = pol[1];
                    policyname = policyname.TrimStart();
                }


                var state = await StateAccessor.GetAsync(stepContext.Context, () => new SkillState(), cancellationToken: cancellationToken);
                    
                    if (state.IsAction)
                    {
                        return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                    }
                    var _pagerservice = new HDPagerService();
                    string policiesData;

                policiesData = "";
                EscalationPolicy isvalidpolicy = new EscalationPolicy();
                     isvalidpolicy =policyname != "" ? await _pagerservice.IsValidEscalationPolicy(_configuration,policyname) :isvalidpolicy;
                
                
                {
                    if (isvalidpolicy.id != null)
                    {
                        SelectedEscalationPolicy selectedesc = new SelectedEscalationPolicy();
                        selectedesc.Policies = isvalidpolicy;
                        var jsonnew = JsonConvert.SerializeObject(selectedesc);
                        string respAttachment = await AdaptiveCardResources.SendincidentcreationDetailResponseCardAsync(_configuration, jsonnew);
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
                                Type = ActivityTypes.Message,
                                Value = isvalidpolicy

                            }
                        };
                        return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(TemplateEngine.GenerateActivityForLocale("HelpingText"), cancellationToken);
                        return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);

                    }
                }
            }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
        private  async Task<DialogTurnResult> ResultUserFormAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            var inputvalues = stepContext.Result.ToString();
            var _pagerservice = new HDPagerService();
            JObject data1 = JObject.Parse(inputvalues);
            string policyname = data1.SelectToken("txtesc").ToString().Split('-').Last().Trim();
            string incidenttitle = data1.SelectToken("txttitle").ToString();
            string incidentdescription = data1.SelectToken("txtdesc").ToString();
            string msg = await _pagerservice.createincidents(_configuration,incidenttitle, policyname, incidentdescription);
            dynamic data = new { number = msg };
            var response = TemplateEngine.GenerateActivityForLocale("IncidentCreation", data);
            await stepContext.Context.SendActivityAsync(response, cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
           
        }





    }
}
    
