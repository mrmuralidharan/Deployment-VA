using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos.Table;
using DepartmentSkill.Models;
using Microsoft.Azure.Cosmos;

namespace DepartmentSkill.Services
{
    public class AzureBlobTableSearch
    {
        /// <summary>
        /// Fetches the department details from Azure Blob table
        /// </summary>
        /// <param name="getData"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public async Task<BuildEntity[]> FetchData(DepartmentLookUpData getData, IConfiguration configuration)
        {
            var entities = new List<BuildEntity>();
            try
            {
                string storageAccountName = configuration.GetSection("blobStorageAccountName").Value;
                string tableName = configuration.GetSection("blobStorageTableName").Value;
                string accountKey = configuration.GetSection("blobStorageKey").Value;
                BuildEntity[] filterResult; var qryCondition = string.Empty;
                var acc = new Microsoft.Azure.Cosmos.Table.CloudStorageAccount(new Microsoft.Azure.Cosmos.Table.StorageCredentials(storageAccountName, accountKey), true);

                if (!string.IsNullOrEmpty(getData.DeptName))
                {
                    qryCondition = Microsoft.Azure.Cosmos.Table.TableQuery.GenerateFilterCondition("PartitionKey", Microsoft.Azure.Cosmos.Table.QueryComparisons.Equal, getData.DeptName);
                }
                else if (!string.IsNullOrEmpty(getData.DeptNumber))
                {
                    qryCondition = Microsoft.Azure.Cosmos.Table.TableQuery.GenerateFilterCondition("RowKey", Microsoft.Azure.Cosmos.Table.QueryComparisons.Equal, getData.DeptNumber);
                }


                var tableClient = acc.CreateCloudTableClient();
                var table = tableClient.GetTableReference(tableName);
                Microsoft.Azure.Cosmos.Table.TableContinuationToken token = null;
                do
                {
                    var queryResult = await table.ExecuteQuerySegmentedAsync(new Microsoft.Azure.Cosmos.Table.TableQuery<BuildEntity>().Where(qryCondition), token);
                    entities.AddRange(queryResult.Results);
                    Console.WriteLine(queryResult.Results);

                    if (!string.IsNullOrEmpty(getData.DeptName))
                    {
                        filterResult = entities.Where(x => x.DeptName == getData.DeptName).ToArray();
                        Console.WriteLine(filterResult);
                        break;
                    }
                    else if (!string.IsNullOrEmpty(getData.DeptNumber))
                    {
                        filterResult = entities.Where(x => x.DeptNumber == getData.DeptNumber).ToArray();
                        Console.WriteLine(filterResult);
                        break;
                    }
                } while (true);

               return filterResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

    }

    /// <summary>
    /// Entities for the Azure blob table
    /// </summary>
    public class BuildEntity : Microsoft.Azure.Cosmos.Table.TableEntity
    {
        public BuildEntity() { }
        public BuildEntity(string partKey, string rowKey)
        {
            PartitionKey = partKey;
            RowKey = rowKey;
        }
        public string DeptName { get; set; }
        public string DeptNumber { get; set; }

    }
}
