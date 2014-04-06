using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewsAPI.Models;
using System.Data.Entity;

namespace NewsAPI.Tests
{
    public class TestNewsAPIContext : INewsAPIContext
    {
        public TestNewsAPIContext()
        {
            this.Users = new TestUserDbSet();
            this.Feeds = new TestFeedDbSet();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Feed> Feeds { get; set; }

        public int SaveChanges()
        {
            return 0;
        }

        public void MarkAsModified(Object item) { }
        public void Dispose() { }
    }
}
