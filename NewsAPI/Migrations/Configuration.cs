namespace NewsAPI.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using NewsAPI.Models;
    using System.Collections.Generic;

    internal sealed class Configuration : DbMigrationsConfiguration<NewsAPI.Models.NewsAPIContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(NewsAPI.Models.NewsAPIContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            Feed huffpo = new Feed { Name = "Huffington Post", Url = "http://www.huffingtonpost.co.uk/feeds/index.xml" };
            Feed aolcom = new Feed { Name = "AOL dot com", Url = "http://www.aol.com/feeds/index.xml" };
            var feeds = new List<Feed>() { huffpo, aolcom };
            feeds.ForEach(f => context.Feeds.AddOrUpdate(f));
            context.Users.AddOrUpdate(u => u.Name, new User { Name = "Alvaro", Feeds = new List<Feed>() { huffpo, aolcom } });
        }
    }
}
