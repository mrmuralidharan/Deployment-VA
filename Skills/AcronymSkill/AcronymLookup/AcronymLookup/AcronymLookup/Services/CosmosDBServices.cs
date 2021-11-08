using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Configuration;
using AcronymLookup.Models;

namespace AcronymLookup.Services
{
    public class CosmosDBServices
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

        public IQueryable<dynamic> QueryItems(IConfiguration configuration)
        {
            try
            {
                string cosmosEndpointUri = configuration.GetSection("cosmosDb:cosmosDBEndpoint").Value;
                string cosmosPrimaryKey = configuration.GetSection("cosmosDb:authKey").Value;
                string cosmosDBId = configuration.GetSection("cosmosDB_Id").Value;
                string cosmosDBContainerId = configuration.GetSection("cosmosDB_ContainerId").Value;                
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(cosmosDBId, cosmosDBContainerId);
                FeedOptions DefaultOptions = new FeedOptions { EnableCrossPartitionQuery = true };
                string qryTxt = "SELECT * FROM c";             

                var client = new DocumentClient(new Uri(cosmosEndpointUri), cosmosPrimaryKey, ConnectionPolicy);
                var qry = client.CreateDocumentQuery(collectionUri, new SqlQuerySpec()
                {
                    QueryText = qryTxt
                }, DefaultOptions);

                return qry;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Document AddItems(AcronymModel addAcronymData, IConfiguration configuration)
        {
            try
            {
                string cosmosEndpointUri = configuration.GetSection("cosmosDb:cosmosDBEndpoint").Value;
                string cosmosPrimaryKey = configuration.GetSection("cosmosDb:authKey").Value;
                string cosmosDBId = configuration.GetSection("cosmosDB_Id").Value;
                string cosmosDBContainerId = configuration.GetSection("cosmosDB_ContainerId").Value;                
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(cosmosDBId, cosmosDBContainerId);
                var client = new DocumentClient(new Uri(cosmosEndpointUri), cosmosPrimaryKey, ConnectionPolicy);
                AcronymModel responseData = QueryDBWithAcronym(client, addAcronymData, configuration);
                Document response = new Document();
                if (responseData.Id == null)
                {
                    var addResponse = client.CreateDocumentAsync(collectionUri, addAcronymData);
                    response = addResponse.Result;
                }
                return response;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public string EditItems(AcronymModel addAcronymData, IConfiguration configuration)
        {
            try
            {
                string cosmosEndpointUri = configuration.GetSection("cosmosDb:cosmosDBEndpoint").Value;
                string cosmosPrimaryKey = configuration.GetSection("cosmosDb:authKey").Value;
                string cosmosDBId = configuration.GetSection("cosmosDB_Id").Value;
                string cosmosDBContainerId = configuration.GetSection("cosmosDB_ContainerId").Value;                
                var client = new DocumentClient(new Uri(cosmosEndpointUri), cosmosPrimaryKey, ConnectionPolicy);
                Microsoft.Azure.Documents.Client.RequestOptions options = new Microsoft.Azure.Documents.Client.RequestOptions
                {
                    PartitionKey = new Microsoft.Azure.Documents.PartitionKey(addAcronymData.Acronym),
                    ConsistencyLevel = Microsoft.Azure.Documents.ConsistencyLevel.Session
                };
                AcronymModel responseData = QueryDBWithAcronym(client, addAcronymData, configuration);
                string response = "";
                if (responseData.Id != null)
                {
                    addAcronymData.Id = responseData.Id;
                    var editResponse = client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(cosmosDBId, cosmosDBContainerId, responseData.Id), addAcronymData, options);
                    response = editResponse.Result.StatusCode.ToString();
                }

                return response;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public string ViewItems(AcronymModel addAcronymData, IConfiguration configuration)
        {
            try
            {
                string cosmosEndpointUri = configuration.GetSection("cosmosDb:cosmosDBEndpoint").Value;
                string cosmosPrimaryKey = configuration.GetSection("cosmosDb:authKey").Value;
                string cosmosDBId = configuration.GetSection("cosmosDB_Id").Value;
                string cosmosDBContainerId = configuration.GetSection("cosmosDB_ContainerId").Value;                
                var client = new DocumentClient(new Uri(cosmosEndpointUri), cosmosPrimaryKey, ConnectionPolicy);
                var qry = client.CreateDocumentQuery<AcronymModel>(
                UriFactory.CreateDocumentCollectionUri(cosmosDBId, cosmosDBContainerId))
                .Where(so => so.Acronym == addAcronymData.Acronym)
                .AsEnumerable()
                .First();
                return Newtonsoft.Json.JsonConvert.SerializeObject(qry);

            }
            catch (Exception)
            {
                return "";
            }
        }

        public async Task<string> DeleteItems(AcronymModel addAcronymData, IConfiguration configuration)
        {
            try
            {
                string cosmosEndpointUri = configuration.GetSection("cosmosDb:cosmosDBEndpoint").Value;
                string cosmosPrimaryKey = configuration.GetSection("cosmosDb:authKey").Value;
                string cosmosDBId = configuration.GetSection("cosmosDB_Id").Value;
                string cosmosDBContainerId = configuration.GetSection("cosmosDB_ContainerId").Value;   
                var client = new DocumentClient(new Uri(cosmosEndpointUri), cosmosPrimaryKey, ConnectionPolicy);
                var searchedDoc = QueryDBWithAcronym(client, addAcronymData, configuration);
                string response="";
                if (searchedDoc.Id != null) {
                    var deleteResponse = await client.DeleteDocumentAsync(
                     UriFactory.CreateDocumentUri(cosmosDBId, cosmosDBContainerId, searchedDoc.Id),
                     new Microsoft.Azure.Documents.Client.RequestOptions { PartitionKey = new Microsoft.Azure.Documents.PartitionKey(addAcronymData.Acronym) });                    
                    response=deleteResponse.StatusCode.ToString();
                }
                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string RandomAcronym(IConfiguration configuration)
        {
            var queryItems = this.QueryItems(configuration);
            // Create a Random object  
            Random rand = new Random();
            // Generate a random index less than the size of the array.  
            int index = rand.Next(queryItems.ToArray().Length);
            var acronymData = queryItems.ToArray()[index];
            return Newtonsoft.Json.JsonConvert.SerializeObject(acronymData);
        }

        public AcronymModel QueryDBWithAcronym(DocumentClient client, AcronymModel addAcronymData, IConfiguration configuration)
        {
            try
            {
                string cosmosDBId = configuration.GetSection("cosmosDB_Id").Value;
                string cosmosDBContainerId = configuration.GetSection("cosmosDB_ContainerId").Value;
                var qry = client.CreateDocumentQuery<AcronymModel>(
                UriFactory.CreateDocumentCollectionUri(cosmosDBId, cosmosDBContainerId))
                .Where(so => so.Acronym == addAcronymData.Acronym)
                .AsEnumerable()
                .First();
                return qry;
            }
            catch (Exception)
            {
                return new AcronymModel();
            }
        }
    }
}
