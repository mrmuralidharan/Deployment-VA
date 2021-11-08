using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HDHelpSkill.Cards;
using HDHelpSkill.Models;
using HDHelpSkill.Services;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace HDHelpSkill.Dialogs
{
    public class HelpDialog : SkillDialogBase
    {
        private IConfiguration _configuration { get; set; }
        public HelpDialog(IServiceProvider serviceProvider) : base(nameof(HelpDialog), serviceProvider)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();
            _configuration = builder.Build();

            var steps = new WaterfallStep[]
        {
            GetUserResponseAsync

        };
            AddDialog(new WaterfallDialog(nameof(HelpDialog), steps));


            InitialDialogId = nameof(HelpDialog);
        }
        private async Task<DialogTurnResult> GetUserResponseAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await StateAccessor.GetAsync(stepContext.Context, () => new SkillState(), cancellationToken: cancellationToken);
            if (state.IsAction)
            {
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            var Usertextresult = stepContext.Context.TurnState.Get<HdHelpSkillLuis>(StateProperties.SkillLuisResult);
            var userTxt = Usertextresult.Text;
            AzureBlobTableSearch tblSearch = new AzureBlobTableSearch();

            var res = tblSearch.fetchData(userTxt, _configuration);
        
           string respAttachment = await AdaptiveCardResources.SendhelpDetailResponseCardAsync(_configuration, res);
            List<Attachment> attachs = new List<Attachment>();
            attachs.Add(new Attachment
            {
                ContentType = AdaptiveCards.AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(respAttachment)
            });
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(attachs), cancellationToken);
            return await stepContext.NextAsync(cancellationToken: cancellationToken);


        }
    
        private static class DialogIds
        {
            public const string NamePrompt = "HelpDialog";
        }
    }
   }
