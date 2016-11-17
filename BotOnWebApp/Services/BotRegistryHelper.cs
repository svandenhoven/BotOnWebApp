using BotOnWebApp.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Linq;

namespace BotOnWebApp.Services
{
    public class BotRegistryHelper
    {

        public static void MapUser(string userid, string accessToken)
        {
            MapUser(userid, accessToken, "");
        }
        public static void MapUser(string userid, string accessToken, string serviceUrl)
        {
            var UserMappings = new UserMappings(userid, accessToken, serviceUrl);

            var connectionString = CloudConfigurationManager.GetSetting("AzureWebJobsStorage");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("BotMappings");
            table.CreateIfNotExists();

            TableOperation insertOperation = TableOperation.InsertOrReplace(UserMappings);
            table.Execute(insertOperation);
        }

        public static void StoreUserMap(UserMappings userMap)
        {

            var connectionString = CloudConfigurationManager.GetSetting("AzureWebJobsStorage");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("BotMappings");
            table.CreateIfNotExists();

            TableOperation insertOperation = TableOperation.InsertOrReplace(userMap);
            table.Execute(insertOperation);
        }


        public static string GetUserToken(string key)
        {
            string connstring = CloudConfigurationManager.GetSetting("AzureWebJobsStorage");
            var storageAccount = CloudStorageAccount.Parse(connstring);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("BotMappings");

            // Create the table query.
            var rangeQuery = new TableQuery<UserMappings>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "MSFT"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("Key", QueryComparisons.Equal, key)));

            var userMap = table.ExecuteQuery(rangeQuery);

            return (userMap.Count() > 0) ? userMap.ElementAt<UserMappings>(0).AccessToken : null;
        }

        public static bool ForgetUserToken(string key)
        {
            string connstring = CloudConfigurationManager.GetSetting("AzureWebJobsStorage");
            var storageAccount = CloudStorageAccount.Parse(connstring);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("BotMappings");

            // Create the table query.
            var rangeQuery = new TableQuery<UserMappings>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "MSFT"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("Key", QueryComparisons.Equal, key)));

            var userMap = table.ExecuteQuery(rangeQuery);
            if (userMap.Count() > 0)
            {
                TableOperation deleteOperation = TableOperation.Delete(userMap.ElementAt<UserMappings>(0));
                var r = table.Execute(deleteOperation);
                return (r.Result != null);
            }

            return false;
        }

        public static UserMappings GetUserRow(string userid)
        {
            string connstring = CloudConfigurationManager.GetSetting("AzureWebJobsStorage");
            var storageAccount = CloudStorageAccount.Parse(connstring);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("BotMappings");

            // Create the table query.
            var rangeQuery = new TableQuery<UserMappings>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "MSFT"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, userid)));

            var userMap = table.ExecuteQuery(rangeQuery);

            return (userMap.Count() > 0) ? userMap.First() : null;
        }
    }
}