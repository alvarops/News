using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using NewsAPI.Models;
using System.Collections;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace NewsAPI.Controllers
{
    public class UsersController : ApiController
    {
        private INewsAPIContext db = new NewsAPIContext();
        private CloudTable table;

        public UsersController ()
        {
           // db.Configuration.ProxyCreationEnabled = false;
            InitStorage();
        }

        public UsersController(INewsAPIContext context)
        {
            db = context;
            // db.Configuration.ProxyCreationEnabled = false;
        }

        public UsersController(INewsAPIContext context, CloudTable table)
        {
            db = context;
            this.table = table;
            // db.Configuration.ProxyCreationEnabled = false;
        }

        private void InitStorage()
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                Microsoft.WindowsAzure.CloudConfigurationManager.GetSetting("NewsAPIConnection"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            //Create the CloudTable object that represents the "articles" table.
            table = tableClient.GetTableReference("articles");
            table.CreateIfNotExists();
        }

        // GET api/Users
        public IQueryable<User> GetUsers()
        {
            return db.Users.Include(u => u.Feeds);
        }

        // GET api/Users/5
        [ResponseType(typeof(User))]
        public IHttpActionResult GetUser(int id)
        {
            User user = db.Users.Include(u => u.Feeds).Where(u => u.UserId == id).First<User>();
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // GET api/Users/5/articles
        public IQueryable<Article> GetArticles(int id, string full)
        {
            User user = db.Users.Include(u => u.Feeds).Where(u => u.UserId == id).FirstOrDefault<User>();
            // Retrieve the storage account from the connection string.
            
            List<Article> articles = new List<Article>();
            if (user == null)
            {
                return articles.AsQueryable();
            } 
          
            foreach (Feed feed in user.Feeds)
            {
                TableQuery<ArticleEntity> query = new TableQuery<ArticleEntity>()
                  .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, feed.Name));

                // Print the fields for each article.
                foreach (ArticleEntity entity in table.ExecuteQuery(query))
                {
                    articles.Add(entity.ToArticle());
                }
            }
            return articles.AsQueryable();
        }

        [ResponseType(typeof(User))]
        // PUT api/Users/5
        public IHttpActionResult PutUser(int id, User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.UserId)
            {
                return BadRequest();
            }
            foreach (Feed feed in user.Feeds)
            {
                Feed existingFeed = db.Feeds.SingleOrDefault(f => f.Url == feed.Url);
                if (existingFeed == null)
                {
                    db.Feeds.Add(feed);
                    db.MarkAsModified(user);
                }
                else
                {
                    feed.FeedId = existingFeed.FeedId;
                    db.Feeds.Where(f => f.FeedId == feed.FeedId).First().Users.Add(user);
                }
                    
            }
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(user);
        }

        // POST api/Users
        [ResponseType(typeof(User))]
        public IHttpActionResult PostUser(User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Users.Add(user);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = user.UserId }, user);
        }

        // DELETE api/Users/5
        [ResponseType(typeof(User))]
        public IHttpActionResult DeleteUser(int id)
        {
            User user = db.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            db.Users.Remove(user);
            db.SaveChanges();

            return Ok(user);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserExists(int id)
        {
            return db.Users.Count(e => e.UserId == id) > 0;
        }


    }
}