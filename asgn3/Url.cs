using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace asgn3
{
    public class Url
    {
        public Uri uri { get; set; }
        public int depth { get; set; }

        public string title { get; set; }
        public string date { get; set; }

        public Url()
        {
        }

        public Url(string path, int depth)
        {
            this.uri = new Uri(path);
            this.depth = depth;
        }

        public Url(Uri uri, int depth)
        {
            this.uri = uri;
            this.depth = depth;
        }

        public Url(string path, string title, string date, int depth)
        {
            this.uri = new Uri(path);
            this.title = title;
            this.date = date;
            this.depth = depth;
        }

        //Need to modify this to move data to table and also move stuff to queue.
        //Find a way to include title and also potentially date. 
        public void Download()
        {
            Thread.Sleep(SpiderController.IdleTime());
            string cleanurl = Regex.Replace(this.uri.AbsoluteUri, @"[^0-9a-zA-Z]+", "");
            Node node = new Node(cleanurl, this.title, this.date);
            CloudTable table = Cloud.getTable();
            TableOperation insertOperation = TableOperation.InsertOrReplace(node);
            table.ExecuteAsync(insertOperation);
            
        }
    }
}
