namespace NewsAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddArticleTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Articles",
                c => new
                    {
                        ArticleId = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Summary = c.String(),
                        PermLink = c.String(),
                        Published = c.DateTime(nullable: false),
                        Feed_FeedId = c.Int(),
                    })
                .PrimaryKey(t => t.ArticleId)
                .ForeignKey("dbo.Feeds", t => t.Feed_FeedId)
                .Index(t => t.Feed_FeedId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Articles", "Feed_FeedId", "dbo.Feeds");
            DropIndex("dbo.Articles", new[] { "Feed_FeedId" });
            DropTable("dbo.Articles");
        }
    }
}
