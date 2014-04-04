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
    public class UsersControllerTest
    {
        private NewsAPIContext db = new NewsAPIContext();

        [TestMethod]
        public void PostAndGetOne()
        {
            // Set up
            foreach (User user in db.Users)
            {
                db.Users.Remove(user);
            }
            db.SaveChanges();

            // Arrange
            UsersController controller = new UsersController();

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
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("Alvaro", result.ElementAt(0).Name);
            Assert.AreEqual("Jennifer", result.ElementAt(1).Name);

            // Tear Down
            foreach (User user in db.Users)
            {
                db.Users.Remove(user);
            }
            db.SaveChanges();
        }
    }
}
