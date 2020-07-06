using System;
using System.Net;
using System.Text;
using System.Net.NetworkInformation;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage;
using Microsoft.Azure;

namespace PingService.Function
{
    public static class PingService
    {
        [FunctionName("PingService")]
        public static void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {

            string connectionString = "<connection string>";
            
            CloudTable table = InitCloudTable(connectionString);

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string targetHost = "bing.com";
            string data = "a quick brown fox jumped over the lazy dog";

            Ping pingSender = new Ping();
            PingOptions options = new PingOptions
            {
                DontFragment = true
            };
            
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 1024;

            PingReply reply = pingSender.Send(targetHost, timeout, buffer, options);

            if (reply.Status == IPStatus.Success)
            {
                AddEntity(true,targetHost,table);
            }
            else{
                AddEntity(false,targetHost,table);
            }

            
        }
        
        public static CloudTable InitCloudTable(string connectionString){

            //Initiate CloudTableClient
            var account = Microsoft.Azure.Cosmos.Table.CloudStorageAccount.Parse(connectionString); 
            CloudTableClient client = account.CreateCloudTableClient();
            CloudTable table = client.GetTableReference("PingData");
            table.CreateIfNotExists();

            return table;            
        }

        public static void AddEntity(bool isRunning, string targetHost, CloudTable table){

            StatusEntity entity;

            if(isRunning == false){

                entity = new StatusEntity {
                    PartitionKey = "Ping",
                    RowKey = targetHost,
                    RoundtripTime = null,
                    isRunning = false
                };
            }
            else{

                entity = new StatusEntity {
                    PartitionKey = "Ping",
                    RowKey = targetHost,
                    RoundtripTime = null,
                    isRunning = false
                };
            }            

            TableOperation insertOperation = TableOperation.Insert(entity);
            table.Execute(insertOperation);
        }

    }

    public class StatusEntity : TableEntity{
        public string RoundtripTime { get; set; }
        public bool isRunning { get; set; }
    }
}
