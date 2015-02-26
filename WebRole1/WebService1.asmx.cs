using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Queue;
using asgn3;
using System.Xml;
using System.Text.RegularExpressions;

namespace WebRole1
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {

        private PerformanceCounter cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private PerformanceCounter ram = new PerformanceCounter("Memory", "Available MBytes");


        [WebMethod]
        public string checkqueue()
        {
            CloudQueue queue = getQueue();
            if (queue.PeekMessage() != null)
            {
                CloudQueueMessage message = queue.PeekMessage();
                return message.AsString;
            }
            else
            {
                return "Nothing found in queue";
            }

        }

        [WebMethod]
        public string getFromTable(string url)
        {
            CloudTable table = getTable();
            string cleanurl = Regex.Replace(url, @"[^0-9a-zA-Z]+", "");
            string temp = "";
            TableOperation retrieveOperation = TableOperation.Retrieve<Node>("asgn", cleanurl);
            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);
            if (retrievedResult.Result != null)
            {
                temp += url + "|";
                temp += (((Node)retrievedResult.Result).title.ToString()) + "|";
                temp += (((Node)retrievedResult.Result).date.ToString());
            }
            else
                temp = url + " not found in Table. Please check the spelling and syntax of the query. An example is: http://www.cnn.com/";
            return temp;
        }

        [WebMethod]
        public string clearTable()
        {
            CloudTable table = getTable();
            table.DeleteIfExists();
            return "Deleted table. Please wait for at least 10 seconds before restarting crawler.";
        }

        [WebMethod]
        public string CrawlerStatus()
        {
            string temp = "";
            CloudTable table = getTable();
            TableOperation retrieveOperation = TableOperation.Retrieve<QCheck>("end", "spider1");
            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);
            if (retrievedResult.Result != null)
            {
                temp += ((((QCheck)retrievedResult.Result).status));
            }
            else
            {
                temp += ("Uninitialized");
            }
            return temp;
        }

        [WebMethod]
        public List<string> initstart()
        {
            CloudQueue queue = getQueue();
            queue.Clear();
            QCheck node = new QCheck("spider1", "", "idle");
            CloudTable table = getTable();
            TableOperation insertOperation = TableOperation.InsertOrReplace(node);
            table.ExecuteAsync(insertOperation);
            /*dashboard replace = new dashboard(0, "", 0, 0);
            TableOperation insertOperation2 = TableOperation.InsertOrReplace(replace);
            table.ExecuteAsync(insertOperation2);*/
            return getStats();
        }

        [WebMethod]
        public string initializeSpider(string url)
        {
            QCheck node = new QCheck("spider1", url, "start");
            CloudTable table = getTable();
            TableOperation insertOperation = TableOperation.InsertOrReplace(node);
            table.ExecuteAsync(insertOperation);
            return "Initialized crawler with " + url;
        }

        [WebMethod]
        public List<string> GetFromTable(string url)
        {
            List<string> temp = new List<string>();
            string cleanurl = Regex.Replace(url, @"[^0-9a-zA-Z]+", "");
            CloudTable table = getTable();
            TableOperation retrieveOperation = TableOperation.Retrieve<Node>("asgn", cleanurl);
            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);
            if (retrievedResult.Result != null)
            {
                temp.Add(url);
                temp.Add(((((Node)retrievedResult.Result).title)));
                temp.Add(((((Node)retrievedResult.Result).date)));
                return temp;
            }
            else
            {
                temp.Add("n");
                return temp;
            }

        }

        [WebMethod]
        public string endSpider()
        {
            QCheck node = new QCheck("spider1", "", "stop");
            CloudTable table = getTable();
            TableOperation insertOperation = TableOperation.InsertOrReplace(node);
            table.ExecuteAsync(insertOperation);
            return "idle";
        }

        [WebMethod]
        public string cpuram()
        {
            float temp = this.cpu.NextValue();
            Thread.Sleep(1000);
            temp = this.cpu.NextValue();
            return this.ram.NextValue() + "|" + temp;
        }

        [WebMethod]
        public List<string> getStats()
        {
            List<string> temp = new List<string>();
            CloudTable table = getTable();
            TableOperation retrieveOperation = TableOperation.Retrieve<dashboard>("dash", "stats");
            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);
            if (retrievedResult.Result != null)
            {
                dashboard board = ((dashboard)retrievedResult.Result);
                temp.Add(board.urlcount.ToString());
                temp.Add(board.urls);
                temp.Add(board.queuesize.ToString());
                temp.Add(board.tablesize.ToString());
                temp.Add(board.errors.ToString());
                return temp;
            }
            else
                return temp;
        }

        [WebMethod]
        public List<string> initdash()
        {
            CloudTable table = getTable();
            dashboard replace = new dashboard(0, "", 0, 0, "");
            TableOperation insertOperation = TableOperation.InsertOrReplace(replace);
            table.ExecuteAsync(insertOperation);
            return getStats();
        }

        [WebMethod]
        public string clearQueue()
        {
            CloudQueue queue = getQueue();
            queue.Clear();
            return "Queue Cleared";
        }


        public static CloudTable getTable()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.AppSettings["connectionstring"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("asgn3");
            table.CreateIfNotExists();
            return table;
        }
        public static CloudQueue getQueue()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.AppSettings["connectionstring"]);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("asgn3");
            queue.CreateIfNotExists();
            return queue;
        }
    }
}
