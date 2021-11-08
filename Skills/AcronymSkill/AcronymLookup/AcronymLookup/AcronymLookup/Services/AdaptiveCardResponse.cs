using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdaptiveCards.Templating;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.Bot.Schema;

namespace AcronymLookup.Services
{
    public static class AdaptiveCardResponse
    {
        public static async Task<List<Attachment>> AddItemResponseCardAsync(IConfiguration configuration, string data)
        {
            string rawJson = string.Empty;
            string azureBlobConnStr = configuration.GetSection("blobConnectionString").Value;
            string blobContainerName = configuration.GetSection("blobContainerName").Value;
            string blobAdaptiveCardName = configuration.GetSection("newAcronymCardName").Value;
            BlobContainerClient container = new BlobContainerClient(azureBlobConnStr, blobContainerName);

            BlobClient blobAdaptiveCardClient = container.GetBlobClient(blobAdaptiveCardName);
            if (await blobAdaptiveCardClient.ExistsAsync())
            {
                var blobResponse = blobAdaptiveCardClient.DownloadAsync();
                using var streamRead = new StreamReader(blobResponse.Result.Value.Content);
                while (!streamRead.EndOfStream)
                {
                    rawJson = await streamRead.ReadToEndAsync();
                }

            }
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(rawJson);
            string cardJson = template.Expand(data);
            var cardAttachment = CreateAdaptiveCardAttachment(cardJson);
            return cardAttachment;
        }

        public static async Task<List<Attachment>> UpdateItemResponseCardAsync(IConfiguration configuration, string data)
        {
            string rawJson = string.Empty;
            string azureBlobConnStr = configuration.GetSection("blobConnectionString").Value;
            string blobContainerName = configuration.GetSection("blobContainerName").Value;
            string blobAdaptiveCardName = configuration.GetSection("editAcronymCardName").Value;
            BlobContainerClient container = new BlobContainerClient(azureBlobConnStr, blobContainerName);

            BlobClient blobAdaptiveCardClient = container.GetBlobClient(blobAdaptiveCardName);
            if (await blobAdaptiveCardClient.ExistsAsync())
            {
                var blobResponse = blobAdaptiveCardClient.DownloadAsync();
                using var streamRead = new StreamReader(blobResponse.Result.Value.Content);
                while (!streamRead.EndOfStream)
                {
                    rawJson = await streamRead.ReadToEndAsync();
                }

            }
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(rawJson);
            string cardJson = template.Expand(data);
            var cardAttachment = CreateAdaptiveCardAttachment(cardJson);
            return cardAttachment;
        }

        public static async Task<List<Attachment>> ViewItemResponseCardAsync(IConfiguration configuration, string data)
        {
            string rawJson = string.Empty;
            string azureBlobConnStr = configuration.GetSection("blobConnectionString").Value;
            string blobContainerName = configuration.GetSection("blobContainerName").Value;
            string blobAdaptiveCardName = configuration.GetSection("viewAcronymCardName").Value;
            BlobContainerClient container = new BlobContainerClient(azureBlobConnStr, blobContainerName);

            BlobClient blobAdaptiveCardClient = container.GetBlobClient(blobAdaptiveCardName);
            if (await blobAdaptiveCardClient.ExistsAsync())
            {
                var blobResponse = blobAdaptiveCardClient.DownloadAsync();
                using var streamRead = new StreamReader(blobResponse.Result.Value.Content);
                while (!streamRead.EndOfStream)
                {
                    rawJson = await streamRead.ReadToEndAsync();
                }

            }
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(rawJson);
            string cardJson = template.Expand(data);
            var cardAttachment = CreateAdaptiveCardAttachment(cardJson);
            return cardAttachment;
        }

        public static async Task<List<Attachment>> CountResponseCardAsync(IConfiguration configuration, string data)
        {
            string rawJson = string.Empty;
            string azureBlobConnStr = configuration.GetSection("blobConnectionString").Value;
            string blobContainerName = configuration.GetSection("blobContainerName").Value;
            string blobAdaptiveCardName = configuration.GetSection("generalAcronymCardName").Value;
            BlobContainerClient container = new BlobContainerClient(azureBlobConnStr, blobContainerName);

            BlobClient blobAdaptiveCardClient = container.GetBlobClient(blobAdaptiveCardName);
            if (await blobAdaptiveCardClient.ExistsAsync())
            {
                var blobResponse = blobAdaptiveCardClient.DownloadAsync();
                using var streamRead = new StreamReader(blobResponse.Result.Value.Content);
                while (!streamRead.EndOfStream)
                {
                    rawJson = await streamRead.ReadToEndAsync();
                }

            }
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(rawJson);
            string cardJson = template.Expand(data);
            var cardAttachment = CreateAdaptiveCardAttachment(cardJson);
            return cardAttachment;
        }

        private static List<Attachment> CreateAdaptiveCardAttachment(string _card)
        {
            List<Attachment> adaptiveCardAttachment = new List<Attachment>
            {
                new Attachment
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(_card)
                }
            };

            return adaptiveCardAttachment;
        }
    }
}
