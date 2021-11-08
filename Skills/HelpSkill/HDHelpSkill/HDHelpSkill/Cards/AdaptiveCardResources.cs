
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Azure.Storage.Blobs;
using AdaptiveCards.Templating;
using HDHelpSkill.Services;
using HDHelpSkill.Models;

namespace HDHelpSkill.Cards
{
    public class AdaptiveCardResources
    {


        public static Activity ShowDeptDetailCard(ITurnContext context, LocaleTemplateManager localTemplateEngineManager)
        {
            var response = context.Activity.CreateReply();
            return response;
        }

        public static async Task<string> SendhelpDetailResponseCardAsync(IConfiguration configuration, string changetext)
        {
            string rawJson = string.Empty;
            string azureBlobConnStr = configuration.GetSection("blobConnetionString").Value;
            string blobContainerName = configuration.GetSection("blobContainerName").Value;
            string blobAdaptiveCardName = configuration.GetSection("blobAdaptiveCardName").Value;
            string cardJson = "";

            BlobContainerClient container = new BlobContainerClient(azureBlobConnStr, blobContainerName);

            BlobClient blobAdaptiveCardClient = container.GetBlobClient(blobAdaptiveCardName);

            if (await blobAdaptiveCardClient.ExistsAsync())
            {
                var blobResponse = blobAdaptiveCardClient.DownloadAsync();

                using (var streamRead = new StreamReader(blobResponse.Result.Value.Content))
                {

                    while (!streamRead.EndOfStream)
                    {
                        rawJson = await streamRead.ReadToEndAsync();
                        AdaptiveCardTemplate template = new AdaptiveCardTemplate(rawJson);
                        string apiResponseString = changetext;

                        cardJson = template.Expand(apiResponseString);


                    }
                }
            }
            return cardJson;
        }
    }
}
