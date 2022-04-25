namespace ECommerceLiteDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedFK : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Categories", "BaseCategoryId", "dbo.Categories");
            DropIndex("dbo.Categories", new[] { "BaseCategoryId" });
            AlterColumn("dbo.Categories", "CategoryName", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Categories", "CategoryDescription", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Categories", "CategoryDescription", c => c.String(nullable: false, maxLength: 500));
            AlterColumn("dbo.Categories", "CategoryName", c => c.String(nullable: false, maxLength: 100));
            CreateIndex("dbo.Categories", "BaseCategoryId");
            AddForeignKey("dbo.Categories", "BaseCategoryId", "dbo.Categories", "Id");
        }
    }
}
