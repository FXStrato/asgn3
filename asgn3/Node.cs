using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace asgn3
{
    public class Node : TableEntity
    {
        public Node(string url, string title, string date)
        {
            this.PartitionKey = "asgn";
            this.RowKey = url;
            this.url = url;
            this.title = title;
            this.date = date;

        }

        public Node() { }

        public string url { get; set; }
        public string title { get; set; }
        public string date { get; set; }
    }
}