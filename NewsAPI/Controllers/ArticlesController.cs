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
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections;

namespace NewsAPI.Controllers
{
    public class ArticlesController : ApiController
    {
        private INewsAPIContext db = new NewsAPIContext();
        private CloudTable table;

        public ArticlesController()
        {
            InitStorage();
        }

        public ArticlesController(INewsAPIContext context, CloudTable table)
        {
            db = context;
            this.table = table;
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

        // GET api/Articles
        public IQueryable<Article> GetArticles()
        {
            // Create the table query.
            TableQuery<ArticleEntity> rangeQuery = new TableQuery<ArticleEntity>();
            List<Article> articles = new List<Article>();
            foreach (ArticleEntity entity in table.ExecuteQuery(rangeQuery))
            {
                articles.Add(entity.ToArticle());
            }
            return articles.AsQueryable();
        }

        // GET api/Articles/5
        [ResponseType(typeof(Article))]
        public IHttpActionResult GetArticle(int id)
        {
            ArticleEntity result = null;
            result = FindArticleWithId(id);

            if (result != null)
                return Ok(result.ToArticle());
            return NotFound();
        }

      
        // PUT api/Articles/5
        public IHttpActionResult PutArticle(int id, Article article)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Assign the result to a CustomerEntity object.
            ArticleEntity updateEntity = FindArticleWithId(id);

            if (updateEntity == null)
            {
                return NotFound();
            }

            if (updateEntity.RowKey != article.Title.GetHashCode().ToString())
            {
                return BadRequest();
            }

            // Change the fields.
            updateEntity.PermLink = article.PermLink;
            updateEntity.Published = article.Published;
            updateEntity.Summary = article.Summary;

            // Create the InsertOrReplace TableOperation
            TableOperation updateOperation = TableOperation.Replace(updateEntity);

            // Execute the operation.
            table.Execute(updateOperation);
            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/Articles
        [ResponseType(typeof(Article))]
        public IHttpActionResult PostArticle(Article article)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Create the TableOperation that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(article.toArticleEntity());
            table.Execute(insertOperation);
            article.ArticleId = article.Title.GetHashCode();
            
            return CreatedAtRoute("DefaultApi", new { id = article.ArticleId }, article);
        }

        // DELETE api/Articles/5
        [ResponseType(typeof(Article))]
        public IHttpActionResult DeleteArticle(int id)
        {
            ArticleEntity article = FindArticleWithId(id);
            if (article == null)
            {
                return NotFound();
            }
            TableOperation removeOperation = TableOperation.Delete(article);
            table.Execute(removeOperation);

            return Ok(article);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private ArticleEntity FindArticleWithId(int id)
        {
            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<ArticleEntity> query = new TableQuery<ArticleEntity>()
            .Where(TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual, ""),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id.ToString())
                ));

            var result = table.ExecuteQuery(query);
            return result.FirstOrDefault();
        }

       

    }
}