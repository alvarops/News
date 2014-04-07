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
            this.Articles = new TestArticleDbSet();


            Feed huffpo = new Feed { Name = "Huffington Post", Url = "http://www.huffingtonpost.co.uk/feeds/index.xml" };
            Feed aolcom = new Feed { Name = "AOL dot com", Url = "http://www.aol.com/feeds/index.xml" };
            var feeds = new List<Feed>() { huffpo, aolcom };
            feeds.ForEach(f => Feeds.Add(f));
            Users.Add(new User { Name = "Alvaro", Feeds = new List<Feed>() { huffpo, aolcom } });
            Article article =
                new Article
                {
                    Title = "Interesting title",
                    PermLink = "http://www.huffingtonpost.co.uk/feeds/article7",
                    Summary = "Text that should be longer than the title",
                    Feed = huffpo
                };
            Articles.Add(article);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Feed> Feeds { get; set; }
        public DbSet<Article> Articles { get; set; }

        public int SaveChanges()
        {
            return 0;
        }

        public void MarkAsModified(Object item) { }
        public void Dispose() { }
    }
}
