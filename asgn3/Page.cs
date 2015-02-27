using RobotsTxt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace asgn3
{
    //Add a check to see if page is an xml. If it is, then parse through it.
    public class Page
    {
        public Url url { get; private set; }
        public string source { get; private set; }
        public List<Url> UrlList { get; private set; }

        private static Regex URLPATTERN = new Regex(@"(href|src)=""[\d\w\/:#@%;$\(\)~_\?\+\-=\\\.&]*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public Page(Url url, string source)
        {
            this.url = url;
            this.source = source;
            this.UrlList = new List<Url>();
        }

        public void FetchAllUrls(int depth)
        {
            //Check to see if page is an xml document. If so, need to separately parse it compared to regular urls.
            if (this.url.uri.OriginalString.Contains(".xml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(this.source);
                XmlNode root = doc.DocumentElement;
                XmlNodeList list = root.SelectNodes("loc");
                foreach (XmlNode xnode in list)
                {
                    if (xnode.InnerText.Contains("2015") && xnode.InnerText.Contains(".xml"))
                    {
                        Uri uri = new Uri(xnode.InnerText);
                        Url url = new Url(uri, depth + 1);
                        UrlList.Add(url);
                        //Log.FoundURL(url.uri.AbsoluteUri);
                    }
                    else if (!xnode.InnerText.Contains(".xml"))
                    {
                        //FIX THIS. THIS ISNT UTILIZING ROBOTS.TXT FOR ANYTHING.
                        //First pull up robots.txt and check to see if the site can be accessed.
                        foreach (string seed in SpiderController.SeedURLs)
                        {
                            if (this.url.uri.AbsoluteUri.Contains(seed))
                            {
                                Url temp = new Url(seed + "/robots.txt", 0);
                                NetworkConnection connection = new NetworkConnection();
                                Page page = new Page(temp, connection.Go(temp));
                                Robots robot = Robots.Load(page.source);
                                bool allowed = robot.IsPathAllowed("Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.31 (KHTML, like Gecko) Chrome/26.0.1410.64 Safari/537.31", this.url.uri.AbsoluteUri);
                                //Can access, include in queue.
                                if (allowed)
                                {
                                    UrlList.Add(url);
                                    //Log.FoundURL(url.uri.AbsoluteUri);
                                }
                                break;
                            }
                        }
                    }
                }

            }
            else
            {
                MatchCollection matches = URLPATTERN.Matches(this.source);

                foreach (Match match in matches)
                {
                    string cleanUrl = CleanUrl(match.Value);

                    if (!String.IsNullOrEmpty(cleanUrl))
                    {
                        Uri uri = new Uri(cleanUrl);
                        Url url = new Url(uri, depth + 1);

                        UrlList.Add(url);
                        //Log.FoundURL(url.uri.AbsoluteUri);
                    }
                }
            }
        }

        private string CleanUrl(string url)
        {
            StringBuilder cleanUrl = new StringBuilder(String.Empty);

            if (!url.Contains("mailto:"))
            {
                try
                {
                    cleanUrl.Append(Regex.Replace(url, @"(?i)(href|src)=|""", ""));

                    Uri uri;

                    if (!IsAbsoluteUrl(cleanUrl.ToString()))
                    {
                        if (cleanUrl.ToString().StartsWith("/"))
                            uri = new Uri(GetParentUriString(this.url.uri), cleanUrl.ToString());
                        else
                            uri = new Uri(this.url.uri.AbsoluteUri + cleanUrl.ToString());
                    }
                    else
                        uri = new Uri(cleanUrl.ToString());

                    UriBuilder uriBuilder = new UriBuilder(uri);
                    uriBuilder.Fragment = String.Empty;

                    cleanUrl.Clear();
                    cleanUrl.Append(uriBuilder.Uri.AbsoluteUri);
                }
                catch (UriFormatException ex)
                {
                    Console.WriteLine(ex.Message, url);
                }
            }

            return cleanUrl.ToString();
        }

        private bool IsAbsoluteUrl(string url)
        {
            Uri result;
            return Uri.TryCreate(url, UriKind.Absolute, out result); 
        }

        private Uri GetParentUriString(Uri uri)
        {
            string path = uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments.Last().Length);
            return new Uri(path);
        }
    }
}
