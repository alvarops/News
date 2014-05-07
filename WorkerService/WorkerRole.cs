using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using System.Xml;
using System.ServiceModel.Syndication;
using NewsAPI.Models;
using System.Collections;
using System.IO;
using System.Text;
using System.Globalization;

namespace WorkerService
{
    public class WorkerRole : RoleEntryPoint
    {
       // private INewsAPIContext db = new NewsAPIContext();

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("WorkerService entry point called", "Information");
            using (var db = new NewsAPIContext())
            {
                foreach (Article a in db.Articles)
                    db.Articles.Remove(a);
                db.SaveChanges();
            }
            while (true)
            {
                Thread.Sleep(1000000);
                Trace.TraceInformation("Working", "Information");
                ArrayList feeds = new ArrayList();
                using (var db = new NewsAPIContext())
                {
                    feeds.AddRange(db.Feeds.ToList());
                    Trace.TraceInformation("Feeds Added", "Information");
                }

                ArrayList articles = new ArrayList();
                foreach (Feed currFeed in feeds)
                {
                    string url = currFeed.Url.ToString();
                    Trace.TraceInformation("Parsing " + url, "Information");
                    try
                    {
                       
                        string xml;
                        using (WebClient webClient = new WebClient())
                        {
                            xml = Encoding.UTF8.GetString(webClient.DownloadData(url));
                        }
                        xml = xml.Replace("+00:00", "");
                        byte[] bytes = System.Text.UTF8Encoding.ASCII.GetBytes(xml);
                        XmlReader reader = new MyXmlReader(new MemoryStream(bytes));
                        SyndicationFeed feed = SyndicationFeed.Load(reader);

                        reader.Close();

                        foreach (SyndicationItem item in feed.Items)
                        {
                            if (item.Title == null || item.Summary == null || item.Links.Count == 0 || item.PublishDate == null)
                                continue;
                            String subject = item.Title.Text;
                            String summary = item.Summary == null? "" : item.Summary.Text;
                            String permaLink = item.Links == null || item.Links.Count == 0? "" : item.Links.ElementAt(0).GetAbsoluteUri().ToString();
                            DateTime published = item.PublishDate.DateTime;
                            if (published.Year < 2000)
                                published = DateTime.Now;

                            Article article = new Article()
                            {
                                Title = subject,
                                Summary = summary,
                                PermLink = permaLink,
                                Published = published,
                                Feed = currFeed
                            };

                            articles.Add(article);
                        }
                    }
                    catch (WebException)
                    {
                        Trace.TraceError(url + " Not found");
                    }
                   

                   
                }
               // string url = "http://www.huffingtonpost.co.uk/feeds/index.xml";
               
                using (var db = new NewsAPIContext())
                {
                    foreach (Article article in articles)
                    {
                       
                        if (db.Articles.Where(a => a.Title == article.Title).SingleOrDefault() == null)
                        {
                            db.Articles.Add(article);
                            db.Feeds.Where(f => f.FeedId == article.Feed.FeedId).First().Articles.Add(article);
                        }
                    }
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }
    }

    class MyXmlReader : XmlTextReader
    {
        private bool readingDate = false;
        private bool readingTitle = false;

        string[] CustomUtcDateTimeFormat = { "ddd MMM dd HH:mm:ss Z yyyy", "ddd MMM dd HH:mm:ss EDT yyyy" }; // Wed Oct 07 08:00:07 GMT 2009

        public MyXmlReader(Stream s) : base(s) { }

        public MyXmlReader(string inputUri) : base(inputUri) { }

        public override void ReadStartElement(string localname, string ns)
        {
            base.ReadStartElement(localname, ns);
        }

        public override string ReadElementString()
        {
            if (string.Equals(base.Name, "title", StringComparison.InvariantCultureIgnoreCase))
            {
                readingTitle = true;
                string readTitle;
                try
                {
                    readTitle = base.ReadElementString();
                    return readTitle;
                }
                catch (XmlException)
                {
                    base.Skip();
                    try
                    {
                        return base.ReadString();
                    }
                    catch (XmlException)
                    {
                        base.Skip();
                        return "invalid title";
                    }
                   
                }
            }
            return base.ReadElementString();
        }

        public override void ReadStartElement()
        {
            if (string.Equals(base.NamespaceURI, string.Empty, StringComparison.InvariantCultureIgnoreCase) &&
                (string.Equals(base.LocalName, "lastBuildDate", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(base.LocalName, "pubDate", StringComparison.InvariantCultureIgnoreCase)))
            {
                readingDate = true;
            }
            base.ReadStartElement();
        }

        public override void ReadStartElement(string localname)
        {
            base.ReadStartElement(localname);
        }

        public override void ReadEndElement()
        {
            if (readingDate)
            {
                readingDate = false;
            }
            else if (readingTitle)
            {
                readingTitle = false;
            }
            try
            {
                base.ReadEndElement();
            }
            catch (XmlException)
            {

            }
        }

        public override string ReadString()
        {
            if (readingDate)
            {
                string dateString = base.ReadString();
                DateTime dt;
                if (!DateTime.TryParse(dateString, out dt))
                    DateTime.TryParseExact(dateString, CustomUtcDateTimeFormat,null, DateTimeStyles.None, out dt);
                return dt.ToUniversalTime().ToString("R", CultureInfo.InvariantCulture);
            }
            else if (readingTitle)
            {
                try
                {
                    return base.ReadString();
                }
                catch (XmlException)
                {
                    string titleString = base.ReadInnerXml();
                    return titleString;
                }
                
            }
            else
            {
                return base.ReadString();
            }
        }
    }
}
