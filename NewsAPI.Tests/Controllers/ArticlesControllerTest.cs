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

namespace NewsAPI.Tests.Controllers
{
    [TestClass]
    public class ArticlesControllerTest
    {
        [TestMethod]
        public void Get()
        {
            // Arrange
            ArticlesController controller = new ArticlesController(new TestNewsAPIContext());

            // Act
            IEnumerable<Article> result = controller.GetArticles();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Interesting title", result.ElementAt(0).Title);
        }

        [TestMethod]
        public void GetById()
        {
            // Arrange
            ArticlesController controller = new ArticlesController(new TestNewsAPIContext());

            // Act
            var result = controller.GetArticle(0);
            OkNegotiatedContentResult<Article> contentResult = result as OkNegotiatedContentResult<Article>;
            
            // Assert
            Assert.IsNotNull(contentResult);
            var article = contentResult.Content as Article;
            Assert.AreEqual("Interesting title", article.Title);
        }

        [TestMethod]
        public void Post()
        {
            // Arrange
            ArticlesController controller = new ArticlesController(new TestNewsAPIContext());

            // Act
            var article1rsp = controller.PostArticle(
                new Article() { Title = "google", PermLink = "www.google.com/articles" } 
            ) as CreatedAtRouteNegotiatedContentResult<Article>;

            // Assert
            Assert.IsNotNull(article1rsp);
        }

        [TestMethod]
        public void Put()
        {
            // Arrange
            ArticlesController controller = new ArticlesController(new TestNewsAPIContext());

            // Act
            var article1rsp = controller.PutArticle(0,
                new Article() { Title = "google", PermLink = "www.google.com/articles" } 
             ) as StatusCodeResult;

            // Assert
            Assert.AreEqual(HttpStatusCode.NoContent, article1rsp.StatusCode);
        }

        [TestMethod]
        public void Delete()
        {
            // Arrange
            ArticlesController controller = new ArticlesController(new TestNewsAPIContext());

            // Act
            var article1rsp = controller.DeleteArticle(0) as OkNegotiatedContentResult<Article>;

            // Assert
            Assert.IsNotNull(article1rsp);
        }

    }
}
