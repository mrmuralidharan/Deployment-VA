using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcronymLookup.Services;
using AcronymLookup.Models;
using Microsoft.Bot.Builder;

namespace AcronymLookup.Dialogs
{
    public class AcronymDialog:SkillDialogBase
    {
        private IConfiguration _configuration { get; set; }

        public AcronymDialog(IServiceProvider serviceProvider) : base(nameof(AcronymDialog), serviceProvider)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();
            _configuration = builder.Build();

            var step = new WaterfallStep[]
            {
                ResponseAsync,
            };
            AddDialog(new WaterfallDialog(nameof(AcronymDialog), step));
            AddDialog(new TextPrompt(DialogIds.AcronymPrompt));

            InitialDialogId = nameof(AcronymDialog);
        }

        private async Task<DialogTurnResult> ResponseAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var intentResult = stepContext.Context.TurnState.Get<Luis.AcronymLookupLuis>(StateProperties.SkillLuisResult);
            CosmosDBServices dbService = new CosmosDBServices();
            if (intentResult.Entities.Operation!=null&& intentResult.Entities.Operation.Length==1)
            {
                if(intentResult.Entities.Acronym != null|| intentResult.Entities.Description!=null)
                {
                    switch (intentResult.Entities.Operation[intentResult.Entities.Operation.Length-1].ToUpper())
                    {
                        case "ADD":
                            {                                
                                AcronymModel addAcronymData = splitAcronymDescription(stepContext, stepContext.Context.Activity.Text.ToString());
                                if (addAcronymData.Acronym != "")
                                {
                                    var response = dbService.AddItems(addAcronymData, _configuration);
                                    if (response.Id != null)
                                    {
                                        var cardAttachment = await AdaptiveCardResponse.AddItemResponseCardAsync(_configuration, response.ToString());
                                        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                                    }
                                    else
                                    {
                                        ResponseModel responseData = new ResponseModel
                                        {
                                            ResponseMessage = "The acronym " + addAcronymData.Acronym + " is available in the database"
                                        };
                                        string formattedResponse = Newtonsoft.Json.JsonConvert.SerializeObject(responseData);
                                        var cardAttachment = await AdaptiveCardResponse.CountResponseCardAsync(_configuration, formattedResponse);
                                        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                                    }
                                } 
                                else
                                {
                                    ResponseModel responseData = new ResponseModel
                                    {
                                        ResponseMessage = "Please check the intent for adding the acronym. Use Help intent to get all the intents"
                                    };
                                    string formattedResponse = Newtonsoft.Json.JsonConvert.SerializeObject(responseData);
                                    var cardAttachment = await AdaptiveCardResponse.CountResponseCardAsync(_configuration, formattedResponse);
                                    await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                                }
                               
                                return await stepContext.NextAsync(cancellationToken: cancellationToken);
                            }
                        case "EDIT":
                            {                                
                                AcronymModel addAcronymData = splitAcronymDescription(stepContext, stepContext.Context.Activity.Text.ToString());
                                if (addAcronymData.Acronym != "")
                                {
                                    var response = dbService.EditItems(addAcronymData, _configuration);
                                    if (response.ToString().ToLower() == "ok" && response.ToString() != "")
                                    {
                                        string dataForAdaptiveCard = Newtonsoft.Json.JsonConvert.SerializeObject(addAcronymData);
                                        var cardAttachment = await AdaptiveCardResponse.UpdateItemResponseCardAsync(_configuration, dataForAdaptiveCard);
                                        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                                    }
                                    else
                                    {
                                        ResponseModel responseData = new ResponseModel
                                        {
                                            ResponseMessage = "The acronym " + addAcronymData.Acronym + " is not available in the database"
                                        };
                                        string formattedResponse = Newtonsoft.Json.JsonConvert.SerializeObject(responseData);
                                        var cardAttachment = await AdaptiveCardResponse.CountResponseCardAsync(_configuration, formattedResponse);
                                        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                                    }
                                }
                                else
                                {
                                    ResponseModel responseData = new ResponseModel
                                    {
                                        ResponseMessage = "Please check the intent for adding the acronym. Use Help intent to get all the intents"
                                    };
                                    string formattedResponse = Newtonsoft.Json.JsonConvert.SerializeObject(responseData);
                                    var cardAttachment = await AdaptiveCardResponse.CountResponseCardAsync(_configuration, formattedResponse);
                                    await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                                }
                                
                                return await stepContext.NextAsync(cancellationToken: cancellationToken);
                            }
                        case "LOOKUP":
                            {
                                AcronymModel addAcronymData = new AcronymModel
                                {
                                    Acronym = intentResult.Entities.Acronym[intentResult.Entities.Acronym.Length - 1].ToUpper()
                                };
                                var response = dbService.ViewItems(addAcronymData, _configuration);
                                if (response != "")
                                {
                                    var cardAttachment = await AdaptiveCardResponse.ViewItemResponseCardAsync(_configuration, response);
                                    await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                                } 
                                else
                                {
                                    ResponseModel responseData = new ResponseModel
                                    {
                                        ResponseMessage = "The acronym " + addAcronymData.Acronym + " is not available in the database"
                                    };
                                    string formattedResponse = Newtonsoft.Json.JsonConvert.SerializeObject(responseData);
                                    var cardAttachment = await AdaptiveCardResponse.CountResponseCardAsync(_configuration, formattedResponse);
                                    await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                                }
                                return await stepContext.NextAsync(cancellationToken: cancellationToken);
                            }
                        case "DELETE":
                            {
                                AcronymModel addAcronymData = new AcronymModel
                                {
                                    Acronym = intentResult.Entities.Acronym[intentResult.Entities.Acronym.Length - 1].ToUpper()
                                };
                                var response = await dbService.DeleteItems(addAcronymData, _configuration);
                                if (response.ToLower()=="nocontent"&&response!="")
                                {
                                    ResponseModel responseData = new ResponseModel
                                    {
                                        ResponseMessage = "The acronym " + addAcronymData.Acronym + " has been successfully deleted."
                                    };
                                    string formattedResponse=Newtonsoft.Json.JsonConvert.SerializeObject(responseData);
                                    var cardAttachment = await AdaptiveCardResponse.CountResponseCardAsync(_configuration, formattedResponse);
                                    await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                                } else
                                {
                                    ResponseModel responseData = new ResponseModel
                                    {
                                        ResponseMessage = "The acronym " + addAcronymData.Acronym + " is not available in the database"
                                    };
                                    string formattedResponse = Newtonsoft.Json.JsonConvert.SerializeObject(responseData);
                                    var cardAttachment = await AdaptiveCardResponse.CountResponseCardAsync(_configuration, formattedResponse);
                                    await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                                }
                                return await stepContext.NextAsync(cancellationToken: cancellationToken);
                            }
                        default:
                            {
                                // intent was identified but not yet implemented                                
                                return await stepContext.NextAsync(cancellationToken: cancellationToken);
                            }
                    }                    
                }
                else if(intentResult.Entities.Operation[0].ToUpper() == "COUNT")
                {
                    var queryItems = dbService.QueryItems(_configuration);
                    ResponseModel responseData = new ResponseModel
                    {
                        ResponseMessage = "The acronym database contains " + queryItems.ToArray().Length + " unique acronyms"
                    };
                    var response= Newtonsoft.Json.JsonConvert.SerializeObject(responseData);
                    var cardAttachment = await AdaptiveCardResponse.CountResponseCardAsync(_configuration, response);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                }
                if(intentResult.Entities.Operation[0].ToUpper() == "TEACH")
                {
                    string acronymData=dbService.RandomAcronym(_configuration);
                    var cardAttachment = await AdaptiveCardResponse.ViewItemResponseCardAsync(_configuration, acronymData);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                }
            }
            
            
            return await stepContext.NextAsync(cancellationToken: cancellationToken);

        }

        private static class DialogIds
        {
            public const string AcronymPrompt = "AcronymLookupMessage";
        }

        private AcronymModel splitAcronymDescription(WaterfallStepContext stepContext, string intentText)
        {
            string[] splitIntent;
           
            if (intentText.Contains("WITH") && intentText.Split("with").Length == 1)
            {
                splitIntent = intentText.Split("WITH");
            }
            else if (intentText.Contains("with") && intentText.Split("WITH").Length == 1)
            {
                splitIntent = intentText.Split("with");
            }
            else
            {
                AcronymModel emptyAcronymData = new AcronymModel
                {
                    Acronym = "",
                    Description = "",
                    Modified = "",
                    LastModified = ""
                };
                return emptyAcronymData;
            }
            string acronym = splitIntent[0].ToUpper().Split("ACRONYM")[1].TrimStart().TrimEnd();
            if (acronym.Split(" ").Length == 1)
            {
                string description = splitIntent[1].TrimStart().TrimEnd();
                string modifiedBy = stepContext.Context.Activity.From.Name;
                string lastModified = DateTime.UtcNow.ToString("MM-dd-yyyy HH:mm:ss");
                AcronymModel addAcronymData = new AcronymModel
                {
                    Acronym = acronym,
                    Description = description,
                    Modified = modifiedBy,
                    LastModified = lastModified
                };
                return addAcronymData;
            } 
            else
            {
                AcronymModel emptyAcronymData = new AcronymModel
                {
                    Acronym = "",
                    Description = "",
                    Modified = "",
                    LastModified = ""
                };
                return emptyAcronymData;
            }
        }
    }
}
