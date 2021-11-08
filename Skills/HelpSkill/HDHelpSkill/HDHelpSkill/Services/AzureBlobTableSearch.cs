using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using HDHelpSkill.Models;

namespace HDHelpSkill.Services
{
    public class AzureBlobTableSearch
    {
        private static readonly ConnectionPolicy ConnectionPolicy = new ConnectionPolicy
        {
            ConnectionMode = Microsoft.Azure.Documents.Client.ConnectionMode.Direct,
            ConnectionProtocol = Protocol.Tcp,
            RequestTimeout = new TimeSpan(1, 0, 0),
            MaxConnectionLimit = 1000,
            RetryOptions = new RetryOptions
            {
                MaxRetryAttemptsOnThrottledRequests = 10,
                MaxRetryWaitTimeInSeconds = 60
            }
        };
        
        public string fetchData(string getData, IConfiguration configuration)

        {
        
            try
            {
                string cosmosEndpointUri = configuration.GetSection("cosmosDb:cosmosDBEndpoint").Value;
                string cosmosPrimaryKey = configuration.GetSection("cosmosDb:authKey").Value;
                string cosmosDBId = configuration.GetSection("cosmosDb:HelpdatabaseId").Value;
                string cosmosDBContainerId = configuration.GetSection("cosmosDb:HelpcontainerId").Value;
                
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(cosmosDBId, cosmosDBContainerId);
                FeedOptions DefaultOptions = new FeedOptions { EnableCrossPartitionQuery = true };
                string qryTxt = "SELECT * FROM c";
                HelpCommands[] resultArr;

                var client = new DocumentClient(new Uri(cosmosEndpointUri), cosmosPrimaryKey, ConnectionPolicy);
                var qry = client.CreateDocumentQuery<HelpCommands>(collectionUri, new SqlQuerySpec()
                {
                    QueryText = qryTxt
                }, DefaultOptions);
                resultArr = qry.ToArray();
                var rawjson = JsonConvert.SerializeObject(resultArr[0]);
                rawjson = rawjson.Replace("#", System.Environment.NewLine);
                return rawjson;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
   
}

