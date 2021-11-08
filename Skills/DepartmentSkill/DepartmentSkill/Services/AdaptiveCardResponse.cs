using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DepartmentSkill.Models;
using Azure.Storage.Blobs;
using System.IO;

namespace DepartmentSkill.Services
{
    public static class AdaptiveCardResponse
    {
        /// <summary>
        /// Gets the Adaptive card template from the Azure Blob storage and replaces the placeholder values
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<string> SendDeptDetailResponseCardAsync(IConfiguration configuration, string data)
        {
            string rawJson = string.Empty;
            string azureBlobConnStr = configuration.GetSection("blobConnectionString").Value;
            string blobContainerName = configuration.GetSection("blobContainerName").Value;
            string blobAdaptiveCardName = configuration.GetSection("blobAdaptiveCardName").Value;
            string blobIconName = configuration.GetSection("blobHomerIconName").Value;
            BlobContainerClient container = new BlobContainerClient(azureBlobConnStr, blobContainerName);
            BlobClient blobIconClient = container.GetBlobClient(blobIconName);
            var imgIconResponse = blobIconClient.Uri.AbsoluteUri;

            BlobClient blobAdaptiveCardClient = container.GetBlobClient(blobAdaptiveCardName);
            if (await blobAdaptiveCardClient.ExistsAsync())
            {
                var blobResponse = blobAdaptiveCardClient.DownloadAsync();
                using (var streamRead = new StreamReader(blobResponse.Result.Value.Content))
                {
                    while (!streamRead.EndOfStream)
                    {
                        rawJson = await streamRead.ReadToEndAsync();
                        Console.WriteLine(rawJson);
                        rawJson = rawJson.Replace("\r\n", string.Empty).Replace(" ", string.Empty);
                        //rawJson = rawJson.Replace("${imgURL}", imgIconResponse).Replace("${HomerTitle}", "Homer").Replace("${NameApp}", "\tAPP\t ");
                        rawJson = rawJson.Replace("${Response}", data);
                    }
                }

            }
            return rawJson;
        }
    }
}
