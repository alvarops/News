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
using System.Data.Entity;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table.Protocol;

namespace WorkerService
{
    public class WorkerRole : RoleEntryPoint
    {
       // private INewsAPIContext db = new NewsAPIContext();
        private CloudTable table;

        public override void Run()
        {
            ExecuteIt();
        }

        private void ExecuteIt()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("WorkerService entry point called", "Information");
            CleanupDatabase();
            while (true)
            {
                UpdateArticles();
                Thread.Sleep(10000);
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

        private void CleanupDatabase()
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("NewsAPIConnection"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the table if it doesn't exist.
            table = tableClient.GetTableReference("articles");
           // await table.DeleteIfExistsAsync();
            table.SafeCreateIfNotExists();
        }

        private void UpdateArticles()
        {
            Trace.TraceInformation("Saving Articles", "Information");
            ArrayList feeds = CreateListOfFeeds();

            ParseArticles(feeds);
        }

        private void ParseArticles(ArrayList feeds)
        {
            foreach (Feed currFeed in feeds)
            {
                ArrayList articles = new ArrayList();
           
                ParseOneFeed(articles, currFeed);
                SaveArticles(articles);
            }
        }

        private static void ParseOneFeed(ArrayList articles, Feed currFeed)
        {
            string url = currFeed.Url.ToString();
            Trace.TraceInformation("Parsing " + url, "Information");
            try
            {
                SyndicationFeed feed = ParseXml(url);

                foreach (SyndicationItem item in feed.Items)
                {
                    if (ItemIsNotValid(item))
                        continue;
                    Article article = BuildArticle(currFeed, item);

                    articles.Add(article);
                }
            }
            catch (WebException)
            {
                Trace.TraceError(url + " Not found");
            }
        }

        private static bool ItemIsNotValid(SyndicationItem item)
        {
            return item.Title == null || item.Summary == null || item.Links.Count == 0 || item.PublishDate == null;
        }

        private static Article BuildArticle(Feed currFeed, SyndicationItem item)
        {
            String subject = item.Title.Text;
            String summary = item.Summary == null ? "" : item.Summary.Text;
            String permaLink = item.Links == null || item.Links.Count == 0 ? "" : item.Links.ElementAt(0).GetAbsoluteUri().ToString();
            DateTime published = item.PublishDate.DateTime;
            if (published.Year < 2000)
                published = DateTime.Now;

            Article article = new Article()
            {
                ArticleId = subject.GetHashCode(),
                Title = subject,
                Summary = summary,
                PermLink = permaLink,
                Published = published,
                Feed = currFeed
            };
            return article;
        }

        private void SaveArticles(ArrayList articles)
        {
            if (articles.Count == 0)
                return;
            // Create the batch operation.
            TableBatchOperation batchOperation = new TableBatchOperation();

            foreach (Article article in articles)
            {
                AddArticleIfNew(batchOperation, article);
            }
            if (batchOperation.Count > 0)
            {
                table.ExecuteBatch(batchOperation);
            }
        }

        private void AddArticleIfNew(TableBatchOperation batchOperation, Article article)
        {
            try
            {
                // Create the table query.
                TableQuery<ArticleEntity> rangeQuery = new TableQuery<ArticleEntity>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, article.Feed.Name),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, article.Title.GetHashCode().ToString())));
                var oldArticle = table.ExecuteQuery(rangeQuery).FirstOrDefault();
                if (oldArticle == null)
                {
                    batchOperation.Insert(article.toArticleEntity());
                }
                else
                {
                    Console.WriteLine("{0}, {1}\t{2}\t{3}", oldArticle.PartitionKey, oldArticle.RowKey,
                    oldArticle.PermLink, oldArticle.Published);
                }
            }
            catch (StorageException)
            {
                batchOperation.Insert(article.toArticleEntity());
            }
        }

        private static SyndicationFeed ParseXml(string url)
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
            return feed;
        }

        private static ArrayList CreateListOfFeeds()
        {
            ArrayList feeds = new ArrayList();
            using (var db = new NewsAPIContext())
            {
                feeds.AddRange(db.Feeds.ToList());
            }
            return feeds;
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

    public static class StorageExtensions
    {
        public static bool SafeCreateIfNotExists(this CloudTable table, TableRequestOptions requestOptions = null, OperationContext operationContext = null)
        {
            do
            {
                try
                {
                    return table.CreateIfNotExists(requestOptions, operationContext);
                }
                catch (StorageException e)
                {
                    if ((e.RequestInformation.HttpStatusCode == 409) && (e.RequestInformation.ExtendedErrorInformation.ErrorCode.Equals(TableErrorCodeStrings.TableBeingDeleted)))
                        Thread.Sleep(1000);// The table is currently being deleted. Try again until it works.
                    else
                        throw;
                }
            } while (true);
        }
    }
}
