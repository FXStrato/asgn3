using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace asgn3
{
    public class dashboard : TableEntity
    {
        //Gonna need count of urls, top 10 most recent urls, size of queue, and size of index.
        public dashboard(int urlcount, string urls, int queuesize, int tablesize, string errors)
        {
            this.PartitionKey = "dash";
            this.RowKey = "stats";
            this.urlcount = urlcount;
            this.urls = urls;
            this.queuesize = queuesize;
            this.tablesize = tablesize;
            this.errors = errors;

        }

        public dashboard() { }

        public int urlcount { get; set; }

        public string urls { get; set; }

        public int queuesize { get; set; }
        public int tablesize { get; set; }

        public string errors { get; set; }
    }
}