using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace asgn3
{
    public class QCheck : TableEntity
    {
        public QCheck(string spider, string url, string status)
        {
            this.PartitionKey = "end";
            this.RowKey = spider;
            this.url = url;
            this.status = status;
            this.spider = spider;

        }

        public QCheck() { }

        public string spider { get; set; }
        public string url { get; set; }

        public string status { get; set; }
    }
}