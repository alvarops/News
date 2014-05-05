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

namespace WorkerService
{
    public class WorkerRole : RoleEntryPoint
    {
       // private INewsAPIContext db = new NewsAPIContext();

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("WorkerService entry point called", "Information");

            while (true)
            {
                Thread.Sleep(10000);
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
                        XmlReader reader = XmlReader.Create(url);
                        SyndicationFeed feed = SyndicationFeed.Load(reader);
                        reader.Close();

                        foreach (SyndicationItem item in feed.Items)
                        {
                            String subject = item.Title.Text;
                            String summary = item.Summary.Text;
                            String permaLink = item.Links.ElementAt(0).GetAbsoluteUri().ToString();
                            DateTime published = item.PublishDate.DateTime;

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
                            db.SaveChanges();
                        }
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
}
