using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using System.IO;
using Microsoft.Extensions.Configuration;
using AdaptiveCards.Templating;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace HDO365HealthStatus.Services
{
    public class AdaptiveCardResponse
    {
        public static async Task<List<Attachment>> SendDeptDetailResponseCardAsync(IConfiguration configuration, string data)
        {
            string rawJson = string.Empty;
            string azureBlobConnStr = configuration.GetSection("blobConnectionString").Value;
            string blobContainerName = configuration.GetSection("blobContainerName").Value;
            string blobAdaptiveCardName = configuration.GetSection("blobAdaptiveCardName").Value;            
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
                        //Console.WriteLine(rawJson);
                        //rawJson = rawJson.Replace("\r\n", string.Empty).Replace(" ", string.Empty);                        
                        //rawJson = rawJson.Replace("${Response}", data);
                    }
                }

            }
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(rawJson);
            string cardJson = template.Expand(data);
            var cardAttachment = CreateAdaptiveCardAttachment(cardJson);
            return cardAttachment;
        }

        public static async Task<List<Attachment>> SendIncidentDetailResponseCardAsync(IConfiguration configuration, string data)
        {
            string rawJson = string.Empty;
            string azureBlobConnStr = configuration.GetSection("blobConnectionString").Value;
            string blobContainerName = configuration.GetSection("blobContainerName").Value;
            string blobAdaptiveCardName = configuration.GetSection("blobIncidentAdaptiveCardName").Value;
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
                        //Console.WriteLine(rawJson);
                        //rawJson = rawJson.Replace("\r\n", string.Empty).Replace(" ", string.Empty);                        
                        //rawJson = rawJson.Replace("${Response}", data);
                    }
                }

            }
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(rawJson);
            string cardJson = template.Expand(data);
            var cardAttachment = CreateAdaptiveCardAttachment(cardJson);
            return cardAttachment;
        }

        private static List<Attachment> CreateAdaptiveCardAttachment(string _card)
        {
            List<Attachment> adaptiveCardAttachment = new List<Attachment>();
            adaptiveCardAttachment.Add(new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(_card)
            });
            
            return adaptiveCardAttachment;
        }
    }
}
