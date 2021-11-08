using DepartmentSkill.Models;
using DepartmentSkill.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DepartmentSkill.Dialogs
{
    public class DepartmentDialog : SkillDialogBase
    {
        private IConfiguration _configuration { get; set; }

        public DepartmentDialog(IServiceProvider serviceProvider):base(nameof(DepartmentDialog),serviceProvider)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();
            _configuration = builder.Build();

            var step = new WaterfallStep[]
            {
                ResponseAsync,
            };
            AddDialog(new WaterfallDialog(nameof(DepartmentDialog), step));
            AddDialog(new TextPrompt(DialogIds.DeptPrompt));

            InitialDialogId = nameof(DepartmentDialog);
        }

        private async Task<DialogTurnResult> ResponseAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string responseData = string.Empty;
            string deptName = string.Empty;
            string deptNumber = string.Empty;
            string deptSearch = string.Empty;
            char[] charsToTrim = { ',', ' ' };

            var state = await StateAccessor.GetAsync(stepContext.Context, () => new SkillState(), cancellationToken: cancellationToken);
            if (state.IsAction)
            {
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }

            var deptResult = stepContext.Context.TurnState.Get<Luis.DepartmentSkillLuis>(StateProperties.SkillLuisResult);

            if (deptResult.Entities.DeptDetail != null)
            {
                if (deptResult.Entities.DeptDetail.Length > 0)
                {
                    string[] deptDetail = deptResult.Entities.DeptDetail;
                    char trimChar = '?';
                    string deptNameNo = deptDetail[deptDetail.Length - 1].Trim(trimChar).TrimEnd().ToUpper();
                    DepartmentLookUpData deptLookUpData = new DepartmentLookUpData()
                    {
                        DeptName = deptNameNo.Length > 2 ? deptNameNo : string.Empty,
                        DeptNumber = deptNameNo.Length <= 2 ? deptNameNo : string.Empty
                    };

                    #region Azure Table
                    //AzureBlobTableSearch tblSearch = new AzureBlobTableSearch();
                    //BuildEntity[] res = await tblSearch.FetchData(deptLookUpData, _configuration);
                    //deptNumber = res.Length == 1 ? res[0].DeptNumber : string.Empty;
                    //if (res.Length > 0)
                    //{
                    //    foreach (var item in res)
                    //    {
                    //        deptName += item.DeptName + ", ";
                    //    }
                    //    deptName = deptName.TrimEnd(charsToTrim);
                    //    if (deptNameNo.Length > 2)
                    //    {
                    //        responseData = "The department number for " + deptLookUpData.DeptName + " is " + deptNumber;
                    //    }
                    //    else if (deptNameNo.Length <= 2)
                    //    {
                    //        if (res.Length > 1)
                    //        {
                    //            responseData = "Department " + deptLookUpData.DeptNumber + "'s are '" + deptName + "'";
                    //        }
                    //        else
                    //        {
                    //            responseData = "Department " + deptLookUpData.DeptNumber + " is " + deptName;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    responseData = "Department " + deptNameNo + " isn't available. Please try with different department.";
                    //}
                    #endregion

                    #region Cosmos
                    AzureCosmosTableSearch cosmosTableSearch = new AzureCosmosTableSearch();
                    //List<CosmosDepartmentLookupData> cosmosResult = await cosmosTableSearch.QueryItems(deptLookUpData, _configuration);
                    CosmosDepartmentLookupData[] cosmosResult = cosmosTableSearch.QueryItems(deptLookUpData, _configuration);

                    if (cosmosResult.Length > 0)
                    {
                        foreach (var item in cosmosResult)
                        {
                            deptName += item.deptName + ", ";
                            deptNumber = item.deptNumber;
                        }
                        deptName = deptName.TrimEnd(charsToTrim);

                        if (deptNameNo.Length > 2)
                        {
                            responseData = "The department number for " + deptLookUpData.DeptName + " is " + deptNumber;
                        }
                        else if (deptNameNo.Length <= 2)
                        {
                            if (deptName.Contains(','))
                            {
                                responseData = "Department " + deptLookUpData.DeptNumber + "'s are '" + deptName + "'";
                            }
                            else
                            {
                                responseData = "Department " + deptLookUpData.DeptNumber + " is " + deptName;
                            }
                            //if (cosmosResult.Length > 1)
                            //{
                            //    responseData = "Department " + deptLookUpData.DeptNumber + "'s are '" + deptName + "'";
                            //}
                            //else
                            //{
                            //    responseData = "Department " + deptLookUpData.DeptNumber + " is " + deptName;
                            //}
                        }
                    }
                    else
                    {
                        responseData = "Department " + deptNameNo + " isn't available. Please try with different department.";
                    }
                    #endregion
                }
                else
                {
                    responseData = "I am facing problem right now. Please try after sometime.";
                }
            }
            else
            {
                responseData = "I am not able to recognize the department which you have provided. Please try with format as 'what department is Millwork'";
            }


            string respAttachment = await AdaptiveCardResponse.SendDeptDetailResponseCardAsync(_configuration, responseData);

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
            public const string DeptPrompt = "DepartmentLookupMessage";
        }
    }
}
