using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NewsAPI;
using NewsAPI.Controllers;
using NewsAPI.Models;
using System.Net;
using System.Web.Http.Results;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace NewsAPI.Tests.Controllers
{
    [TestClass]
    public class ArticlesControllerTest
    {
        CloudTable table = null;

        [TestMethod]
        public void Get()
        {
            // Arrange
            ArticlesController controller = new ArticlesController(new TestNewsAPIContext(), InitStorage());
            var article1rsp = controller.PostArticle(
               new Article() { Title = "googleGet", PermLink = "www.google.com/articles", Published = DateTime.Now, Summary = "text" }
           ) as CreatedAtRouteNegotiatedContentResult<Article>;

            // Act
            IEnumerable<Article> result = controller.GetArticles();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("googleGet", result.ElementAt(0).Title);

            controller.DeleteArticle("googleGet".GetHashCode());
        }

        [TestMethod]
        public void GetById()
        {
            // Arrange
            ArticlesController controller = new ArticlesController(new TestNewsAPIContext(), InitStorage());
            var article1rsp = controller.PostArticle(
               new Article() { Title = "googleGetById", PermLink = "www.google.com/articles", Published = DateTime.Now, Summary = "text" }
           ) as CreatedAtRouteNegotiatedContentResult<Article>;

            // Act
            var result = controller.GetArticle("googleGetById".GetHashCode());
            OkNegotiatedContentResult<Article> contentResult = result as OkNegotiatedContentResult<Article>;
            
            // Assert
            Assert.IsNotNull(contentResult);
            var article = contentResult.Content as Article;
            Assert.AreEqual("googleGetById", article.Title);
            controller.DeleteArticle("googleGetById".GetHashCode());
        }

        [TestMethod]
        public void Post()
        {
            // Arrange
            ArticlesController controller = new ArticlesController(new TestNewsAPIContext(), InitStorage());

            // Act
            var article1rsp = controller.PostArticle(
                new Article() { Title = "google2", PermLink = "www.google.com/articles", Published = DateTime.Now, Summary="text" } 
            ) as CreatedAtRouteNegotiatedContentResult<Article>;

            // Assert
            Assert.IsNotNull(article1rsp);

            // Revert
            controller.DeleteArticle(article1rsp.Content.ArticleId);
        }

        [TestMethod]
        public void Put()
        {
            // Arrange
            ArticlesController controller = new ArticlesController(new TestNewsAPIContext(), InitStorage());

            var article1rsp = controller.PostArticle(
               new Article() { Title = "googlePut", PermLink = "www.google.com/articles", Published = DateTime.Now, Summary = "text" }
           ) as CreatedAtRouteNegotiatedContentResult<Article>;

            // Act
            Article toPut = article1rsp.Content;
            toPut.Summary = "Other text.";

            var article2rsp = controller.PutArticle(toPut.ArticleId, toPut) as StatusCodeResult;

            // Assert
            Assert.AreEqual(HttpStatusCode.NoContent, article2rsp.StatusCode);
          
            // Revert
            controller.DeleteArticle(toPut.ArticleId);
        }

        [TestMethod]
        public void Delete()
        {
            // Arrange
            ArticlesController controller = new ArticlesController(new TestNewsAPIContext(), InitStorage());
            controller.PostArticle(
               new Article() { Title = "googleDelete", PermLink = "www.google.com/articles", Published = DateTime.Now, Summary = "text" }
           );

            // Act
            var article1rsp = controller.DeleteArticle("googleDelete".GetHashCode()) as OkNegotiatedContentResult<Article>;

            // Assert
            Assert.IsNotNull(article1rsp);
        }

        private CloudTable InitStorage()
        {
            if (table == null)
            {
                // Retrieve the storage account from the connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=pereda;AccountKey=yyqFWfdwifi014C5B6BTdo3ABfWGzLl9N5ReE3NcJlirVnV1yt3GvG6doj3MtBkmH+Aupz+zz3OFfv0mVX/riQ==");

                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                //Create the CloudTable object that represents the "articles" table.
                table = tableClient.GetTableReference("articlesTest");
                table.CreateIfNotExists();
            }

            return table;
        }
    }
}
