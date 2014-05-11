namespace NewsAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FeedsReferenceStorageTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Articles", "Feed_FeedId", "dbo.Feeds");
            DropIndex("dbo.Articles", new[] { "Feed_FeedId" });
            DropTable("dbo.Articles");
        }
        
        public override void Down()
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
                .PrimaryKey(t => t.ArticleId);
            
            CreateIndex("dbo.Articles", "Feed_FeedId");
            AddForeignKey("dbo.Articles", "Feed_FeedId", "dbo.Feeds", "FeedId");
        }
    }
}
