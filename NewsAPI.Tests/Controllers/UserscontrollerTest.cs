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

namespace NewsAPI.Tests.Controllers
{
    [TestClass]
    public class UsersControllerTest
    {
        [TestMethod]
        public void PostAndGetOne()
        {
            // Arrange
            UsersController controller = new UsersController();

            // Act
            controller.PostUser(new User() { Name = "Alvaro" });
            controller.PostUser(new User() { Name = "Jennifer" });
            IEnumerable<User> result = controller.GetUsers();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("Alvaro", result.ElementAt(0).Name);
            Assert.AreEqual("Jennifer", result.ElementAt(1).Name);
        }
    }
}
