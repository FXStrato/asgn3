using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace asgn3
{
    public class Spider
    {
        
        private ThreadManager threadManager { get; set; }

        private Queue<Url> URLQueue { get; set; }

        private CloudQueue queue = Cloud.getQueue();

        private HashSet<String> UrlsSeen { get; set; }

        public List<string> inserturls = new List<string>();
        public int queuesize = 0;
        public int urlscount = 0;

        public static List<string> errors = new List<string>();

        public Spider()
        {
            this.threadManager = new ThreadManager();

            this.URLQueue = new Queue<Url>();
            this.UrlsSeen = new HashSet<String>();
        }

        //Need two different types. One where you pass a URL in, and another where you can set it to go and it pulls
        //a url from the queue and runs that. 
        public void Go()
        {
            //This goes through each seedurl and launches a new thread for each url. Since only two is gonna be done for now, 
            //It should launch two threads and go through two urls. 
            foreach (string seed in SpiderController.SeedURLs)
            {
                Url url = new Url(seed, -1);
                this.UrlsSeen.Add(seed);
                threadManager.LaunchThread(FetchNewPage, url);
            }

            if (SpiderController.SeedURLs.Count == 0)
                Console.WriteLine("Need at least one seed URL.");
        }

        public void Go(string seed)
        {
                Url url = new Url(seed, -1);
                this.UrlsSeen.Add(seed);
                threadManager.LaunchThread(FetchNewPage, url);
                inserturls.Add(seed);
        }

        public void End()
        {
            threadManager.KillAll();
            inserturls.Clear();
            urlscount = 0; 
        }

        private void FetchNewPage(Url url)
        {
            //Log.WriteToLog("Fetching page...", url.uri.AbsoluteUri);

            NetworkConnection connection = new NetworkConnection();
            Page page = new Page(url, connection.Go(url));

            if (!String.IsNullOrEmpty(page.source))
            {
                //Log.LoadSuccess(url.uri.AbsoluteUri);
                Crawl(page);
            }
            LoadNextURL();
        }

        private void Crawl(Page page)
        {
            
            page.FetchAllUrls(page.url.depth);

            if (page.UrlList.Count > 0)
            {
                foreach (Url url in page.UrlList)
                {
                    
                    //Getting title from source
                    Regex regex = new Regex("<title>(.*)</title>");
                    var v = regex.Match(page.source);
                    string title = v.Groups[1].ToString();

                    //Getting date from source
                    Regex regex2 = new Regex("property=\"og:pubdate\"><meta content=\"(.*) itemprop=\"dateCreated\">");
                    var t = regex2.Match(page.source);
                    string date = t.Groups[1].ToString();
                    date = date.Replace("\"", "");
                    date = date.Trim();
                    Url temp = new Url(url.uri.AbsoluteUri, title, date, 0);
                    HandleURL(temp);
                    urlscount++;
                }
            }
            else
                Console.WriteLine("No links found.");

            Console.WriteLine("Finished crawling page.");
        }

        private void LoadNextURL()
        {
            CloudQueue queue = Cloud.getQueue();
            while (queue.GetMessage() != null)
            {
                if (threadManager.ThreadList.Count >= SpiderController.MaxThreads)
                    break;

                Url url = new Url();

                lock (this.queue)
                {
                    if (this.queue.PeekMessage() != null)
                    {
                        CloudQueueMessage message = this.queue.GetMessage();
                        url.uri = new Uri(message.AsString);
                        this.queue.DeleteMessageAsync(message);
                        if(queuesize > 0)
                        queuesize--;
                    }
                }

                if (SpiderController.ShouldContinue(url.depth))
                {
                    Thread.Sleep(SpiderController.IdleTime());
                    threadManager.LaunchThread(FetchNewPage, url);
                }
            }
            threadManager.KillThread();
        }

        private void HandleURL(Url url)
        {
            string link = url.uri.AbsoluteUri.ToLower();

            if (this.UrlsSeen.Contains(link)){ }
                //Log.SkippedThisQueuedURL(link);
            else if (SpiderController.UseWhiteList == true && !SpiderController.IsWhiteListedDomain(url.uri.Authority)) { }
            // Log.WriteToLog("URL domain not on whitelist", link);
            else if (SpiderController.IsExcludedDomain(link)) { }
            // Log.SkippedThisExcludedURL(link);
            else if (SpiderController.IsExcludedFileType(link)) { }
            // Log.SkippedThisExcludedFileType(link);
            else if (link.Contains("bleacherreport") && !link.Contains("nba"))
            {
                // Log.SkippedThisExcludedURL(link);
            }
           else if (link.Contains(".js") || link.Contains("file://"))
            {

            }
            /*else if (url.date != "" && !url.date.Contains("2015") || !url.date.Contains("2014"))
            {

            }*/
            else
            {
                lock (this.queue)
                {
                    CloudQueueMessage message = new CloudQueueMessage(link);
                    this.UrlsSeen.Add(link);
                    this.queue.AddMessageAsync(message);
                    url.Download();
                    queuesize++;
                    if (inserturls.Count == 10)
                    {
                        inserturls.RemoveAt(0);
                        inserturls.Add(link);
                    } else
                    inserturls.Add(link);
                }
            }
        }
    }
}
