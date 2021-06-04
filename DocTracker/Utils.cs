using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocTracker {
   public class Utils {

      public static CloudTable CreateTable(string accountName, string tableName, string sasToken) {
         // Retrieve storage account information from sas token string.
         StorageCredentials accountSAS = new StorageCredentials(sasToken);
         CloudStorageAccount accountWithSAS = new CloudStorageAccount(accountSAS, accountName,
            endpointSuffix: null, useHttps: true);

         // Create a table client for interacting with the table service
         CloudTableClient tableClientWithSAS = accountWithSAS.CreateCloudTableClient();

         // Get table reference 
         CloudTable table = tableClientWithSAS.GetTableReference(tableName);

         return table;
      }



      public static async Task<MyEntity> RetrieveEntityUsingPointQueryAsync<MyEntity>(CloudTable table, string partitionKey, string rowKey) where MyEntity : TableEntity {
         try {
            TableOperation retrieveOperation = TableOperation.Retrieve<MyEntity>(partitionKey, rowKey);
            TableResult result = await table.ExecuteAsync(retrieveOperation);
            MyEntity record = result.Result as MyEntity;
            if (record != null) {
               ;// Console.WriteLine("\t{0}\t{1}", customer.PartitionKey, customer.RowKey);
            }

            // Get the request units consumed by the current operation. RequestCharge of a TableResult is only applied to Azure CosmoS DB 
            if (result.RequestCharge.HasValue) {
               ;// Console.WriteLine("Request Charge of Retrieve Operation: " + result.RequestCharge);
            }

            return record;
         }
         catch (Exception) {
            /*
            Console.WriteLine(e.Message);
            Console.ReadLine();
            throw;
            */
            return null;
         }
      }




   }

   public static class MyExtensions {
      public static string InverseTicks(this DateTime mytime) {
         return (DateTime.MaxValue.Ticks - mytime.Ticks).ToString();
      }
   }

}
