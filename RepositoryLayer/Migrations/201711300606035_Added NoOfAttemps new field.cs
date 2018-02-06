namespace RepositoryLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedNoOfAttempsnewfield : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EmailDetails", "NoOfAttemp", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.EmailDetails", "NoOfAttemp");
        }
    }
}
