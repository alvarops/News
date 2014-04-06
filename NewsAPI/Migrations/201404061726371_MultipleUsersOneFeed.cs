namespace NewsAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MultipleUsersOneFeed : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Feeds", "User_UserId", "dbo.Users");
            DropIndex("dbo.Feeds", new[] { "User_UserId" });
            CreateTable(
                "dbo.UserFeeds",
                c => new
                    {
                        User_UserId = c.Int(nullable: false),
                        Feed_FeedId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_UserId, t.Feed_FeedId })
                .ForeignKey("dbo.Users", t => t.User_UserId, cascadeDelete: true)
                .ForeignKey("dbo.Feeds", t => t.Feed_FeedId, cascadeDelete: true)
                .Index(t => t.User_UserId)
                .Index(t => t.Feed_FeedId);
            
            DropColumn("dbo.Feeds", "User_UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Feeds", "User_UserId", c => c.Int());
            DropForeignKey("dbo.UserFeeds", "Feed_FeedId", "dbo.Feeds");
            DropForeignKey("dbo.UserFeeds", "User_UserId", "dbo.Users");
            DropIndex("dbo.UserFeeds", new[] { "Feed_FeedId" });
            DropIndex("dbo.UserFeeds", new[] { "User_UserId" });
            DropTable("dbo.UserFeeds");
            CreateIndex("dbo.Feeds", "User_UserId");
            AddForeignKey("dbo.Feeds", "User_UserId", "dbo.Users", "UserId");
        }
    }
}
