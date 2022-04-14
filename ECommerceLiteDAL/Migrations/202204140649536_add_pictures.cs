namespace ECommerceLiteDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_pictures : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Tables", newName: "ProductPictures");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.ProductPictures", newName: "Tables");
        }
    }
}
