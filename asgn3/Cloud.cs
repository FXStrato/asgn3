using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asgn3
{
    class Cloud
    {
        public static CloudTable getTable()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            connection());
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("asgn3");
            table.CreateIfNotExists();
            return table;
        }
        public static CloudQueue getQueue()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            connection());
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("asgn3");
            queue.CreateIfNotExists();
            return queue;
        }

        private static string connection()
        {
            return "DefaultEndpointsProtocol=https;AccountName=jeffrz;AccountKey=K34J4gKknAejfRop72z0dOVDCwm3I5ptZFAP8069rxafwvK/H7NACRcyeh04c+W3hDXNDtkldukWSaUwQ+sGHw==";
        }
    }
}
