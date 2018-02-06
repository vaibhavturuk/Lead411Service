namespace RepositoryLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AllowNullNameinContacttable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Contacts", "Name", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Contacts", "Name", c => c.String(nullable: false));
        }
    }
}
