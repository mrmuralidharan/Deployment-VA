using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using DepartmentSkill.Models;
using Microsoft.Extensions.Configuration;

namespace DepartmentSkill.Services
{
    public class AzureCosmosTableSearch
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
        public CosmosDepartmentLookupData[] QueryItems(DepartmentLookUpData getData, IConfiguration configuration)
        {
            try
            {
                string cosmosEndpointUri = configuration.GetSection("cosmosDb:cosmosDBEndpoint").Value;
                string cosmosPrimaryKey = configuration.GetSection("cosmosDb:authKey").Value;
                string cosmosDBId = configuration.GetSection("cosmosDB_Id").Value;
                string cosmosDBContainerId = configuration.GetSection("cosmosDB_ContainerId").Value;
                CosmosClient cosmosClient = new CosmosClient(cosmosEndpointUri, cosmosPrimaryKey);
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(cosmosDBId, cosmosDBContainerId);
                FeedOptions DefaultOptions = new FeedOptions { EnableCrossPartitionQuery = true };
                string qryTxt = "SELECT * FROM c where ";
                CosmosDepartmentLookupData[] resultArr;

                var client = new DocumentClient(new Uri(cosmosEndpointUri), cosmosPrimaryKey, ConnectionPolicy);

                if (!string.IsNullOrEmpty(getData.DeptName))
                {
                    qryTxt += "CONTAINS(c.deptName, '"+ getData.DeptName + "')";
                }
                else if (!string.IsNullOrEmpty(getData.DeptNumber))
                {
                    qryTxt += "c.deptNumber ='" + getData.DeptNumber + "'";
                }

                var qry = client.CreateDocumentQuery<CosmosDepartmentLookupData>(collectionUri, new SqlQuerySpec()
                {
                    QueryText = qryTxt
                }, DefaultOptions);

                resultArr = qry.ToArray();

                return resultArr;

            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}

