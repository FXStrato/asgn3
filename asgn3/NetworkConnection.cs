using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace asgn3
{
    class NetworkConnection
    {
        public string Go(Url url)
        {
            if (url.uri != null)
                return ReadPage(GetResponse(url.uri.AbsoluteUri));

            else return "";
        }

        private HttpWebResponse GetResponse(string url)
        {
            HttpWebResponse response = null;
            //Log.LoadingNewPage(url);

            try
            {
                if (url != null)
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                    request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.31 (KHTML, like Gecko) Chrome/26.0.1410.64 Safari/537.31";

                    response = (HttpWebResponse)request.GetResponse();
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Accepted: break;
                        case HttpStatusCode.OK: break;
                        case HttpStatusCode.Found: break;
                        default:
                            throw new Exception(response.StatusCode.ToString());
                    }
                }
            }
            catch (WebException ex)
            {
                Spider.errors.Add("Network Error for " + url + ", " + ex.Message + " Status Code: " + ex.Status);
            }
            catch (ProtocolViolationException ex)
            {
                Console.WriteLine("Protocol Error: {0}", ex.Message);
                Spider.errors.Add("Protocol Error for " + url + ", " + ex.Message);
            }
            catch (UriFormatException ex)
            {
                Console.WriteLine("URI Format Error: {0}", ex.Message);
                Spider.errors.Add("URI Format Error for " + url + ", " + ex.Message);
            }
            catch (NotSupportedException ex)
            {
                Console.WriteLine("Unknown Protocol: " + ex.Message);
                Spider.errors.Add("Unknown Protocol Error for " + url + ", " + ex.Message);
            }
            catch (IOException ex)
            {
                Console.WriteLine("I/O Error: " + ex.Message);
                Spider.errors.Add("I/O Error for " + url + ", " + ex.Message);
            }
            
            return response;
        }

        private string ReadPage(HttpWebResponse response)
        {
            if (response != null)
            {
                string html = String.Empty;

                try
                {
                    using (Stream inputStream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(inputStream))
                    {
                        html = reader.ReadToEnd();
                    }
                    response.Dispose();
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine(ex.Message);
                }

                return html;
            }
            else return "";
        }
    }
}
