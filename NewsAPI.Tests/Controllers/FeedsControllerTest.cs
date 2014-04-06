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
    public class FeedsControllerTest
    {
        [TestMethod]
        public void Get()
        {
            // Arrange
            FeedsController controller = new FeedsController(new TestNewsAPIContext());

            // Act
            IEnumerable<Feed> result = controller.GetFeeds();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("Huffington Post", result.ElementAt(0).Name);
            Assert.AreEqual("AOL dot com", result.ElementAt(1).Name);
        }

        [TestMethod]
        public void GetById()
        {
            // Arrange
            FeedsController controller = new FeedsController(new TestNewsAPIContext());

            // Act
            var result = controller.GetFeed(0);
            OkNegotiatedContentResult<Feed> contentResult = result as OkNegotiatedContentResult<Feed>;
            
            // Assert
            Assert.IsNotNull(contentResult);
            var feed = contentResult.Content as Feed;
            Assert.AreEqual("Huffington Post", feed.Name);
        }

        [TestMethod]
        public void Post()
        {
            // Arrange
            FeedsController controller = new FeedsController(new TestNewsAPIContext());

            // Act
            var feed1rsp = controller.PostFeed(
                new Feed() { Name = "google", Url = "www.google.com/feeds" } 
            ) as CreatedAtRouteNegotiatedContentResult<Feed>;

            // Assert
            Assert.IsNotNull(feed1rsp);
        }

        [TestMethod]
        public void Put()
        {
            // Arrange
            FeedsController controller = new FeedsController(new TestNewsAPIContext());

            // Act
            var feed1rsp = controller.PutFeed(0,
                new Feed() { Name = "google", Url = "www.google.com/feeds" } 
             ) as StatusCodeResult;

            // Assert
            Assert.AreEqual(HttpStatusCode.NoContent, feed1rsp.StatusCode);
        }

        [TestMethod]
        public void Delete()
        {
            // Arrange
            FeedsController controller = new FeedsController(new TestNewsAPIContext());

            // Act
            var feed1rsp = controller.DeleteFeed(0) as OkNegotiatedContentResult<Feed>;

            // Assert
            Assert.IsNotNull(feed1rsp);
        }

    }
}
