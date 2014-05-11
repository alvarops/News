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
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;

namespace NewsAPI.Tests.Controllers
{
    [TestClass]
    public class UsersControllerTest
    {
        //private NewsAPIContext db = new NewsAPIContext();
        private CloudTable table;

        [TestMethod]
        public void PostAndGetOne()
        {
            // Set up
            //foreach (User user in db.Users)
            //{
            //    db.Users.Remove(user);
            //}
            //db.SaveChanges();

            // Arrange
            UsersController controller = new UsersController(new TestNewsAPIContext());

            // Act
            var user1rsp = controller.PostUser(new User() { Name = "Alvaro" }) as CreatedAtRouteNegotiatedContentResult<User>;
            var user2rsp = controller.PostUser(new User() { Name = "Jennifer" }) as CreatedAtRouteNegotiatedContentResult<User>;
           
            // Assert
            Assert.IsNotNull(user1rsp);
            Assert.IsNotNull(user2rsp);

            User user1 = user1rsp.Content;
            User user2 = user2rsp.Content;

            Assert.AreEqual(user1.Name, "Alvaro");
            Assert.AreEqual(user2.Name, "Jennifer");

            // Act
            IEnumerable<User> result = controller.GetUsers();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
            Assert.AreEqual("Alvaro", result.ElementAt(1).Name);
            Assert.AreEqual("Jennifer", result.ElementAt(2).Name);

            //// Tear Down
            //foreach (User user in db.Users)
            //{
            //    db.Users.Remove(user);
            //}
            //db.SaveChanges();
        }

        [TestMethod]
        public void Get()
        {
            // Arrange
            UsersController controller = new UsersController(new TestNewsAPIContext());

            // Act
            IEnumerable<User> result = controller.GetUsers();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Alvaro", result.ElementAt(0).Name);
            Assert.AreEqual("Huffington Post", result.ElementAt(0).Feeds.ElementAt(0).Name);
            Assert.AreEqual("AOL dot com", result.ElementAt(0).Feeds.ElementAt(1).Name);
        }

        [TestMethod]
        public void GetById()
        {
            // Arrange
            UsersController controller = new UsersController(new TestNewsAPIContext());

            // Act
            var result = controller.GetUser(0);
            OkNegotiatedContentResult<User> contentResult = result as OkNegotiatedContentResult<User>;
            
            // Assert
            Assert.IsNotNull(contentResult);
            var user = contentResult.Content as User;
            Assert.AreEqual("Alvaro", user.Name);
        }

        [TestMethod]
        public void GetArticles()
        {
            // Arrange
            UsersController controller = new UsersController(new TestNewsAPIContext(), InitStorage());

            // Act
            IEnumerable < Article > result = controller.GetArticles(0, null);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Post()
        {
            // Arrange
            UsersController controller = new UsersController(new TestNewsAPIContext());

            // Act
            var user1rsp = controller.PostUser(
                new User() { Name = "John", 
                    Feeds = new List<Feed>() { 
                        new Feed() { Name = "google", Url = "www.google.com/feeds" } 
                    } 
                }) as CreatedAtRouteNegotiatedContentResult<User>;

            // Assert
            Assert.IsNotNull(user1rsp);
        }

        [TestMethod]
        public void Put()
        {
            // Arrange
            UsersController controller = new UsersController(new TestNewsAPIContext());

            // Act
           
            var response = controller.PutUser(0, new User()
            {
                Name = "John",
                Feeds = new List<Feed>() { 
                        new Feed() { Name = "google", Url = "www.google.com/feeds" } 
                    }
            }) as OkNegotiatedContentResult<User>;
            User modUser = response.Content;
            // Assert
            Assert.AreEqual("John", modUser.Name);

            // Revert
            controller.DeleteUser(modUser.UserId);
        }

        [TestMethod]
        public void Delete()
        {
            // Arrange
            UsersController controller = new UsersController(new TestNewsAPIContext());

            // Act
            var user1rsp = controller.DeleteUser(0) as OkNegotiatedContentResult<User>;

            // Assert
            Assert.IsNotNull(user1rsp);
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
                table = tableClient.GetTableReference("articles");
                table.CreateIfNotExists();
            }

            return table;
        }
    }

}
