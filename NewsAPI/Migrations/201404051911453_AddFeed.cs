namespace NewsAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFeed : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Feeds",
                c => new
                    {
                        FeedId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Url = c.String(nullable: false),
                        User_UserId = c.Int(),
                    })
                .PrimaryKey(t => t.FeedId)
                .ForeignKey("dbo.Users", t => t.User_UserId)
                .Index(t => t.User_UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Feeds", "User_UserId", "dbo.Users");
            DropIndex("dbo.Feeds", new[] { "User_UserId" });
            DropTable("dbo.Feeds");
        }
    }
}
