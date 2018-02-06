namespace RepositoryLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateOn08082017 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AuthenticationDetails",
                c => new
                    {
                        AuthenticationDetailId = c.Long(nullable: false, identity: true),
                        UserMembershipId = c.Long(nullable: false),
                        RefreshToken = c.String(nullable: false),
                        Provider = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedBy = c.Long(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedBy = c.Long(),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedBy = c.Long(),
                        DeletedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.AuthenticationDetailId)
                .ForeignKey("dbo.UserMemberships", t => t.UserMembershipId, cascadeDelete: true)
                .Index(t => t.UserMembershipId);
            
            CreateTable(
                "dbo.AuthenticationTokens",
                c => new
                    {
                        AuthenticationTokenId = c.Long(nullable: false, identity: true),
                        AuthenticationDetailId = c.Long(nullable: false),
                        AccessToken = c.String(nullable: false),
                        CreatedBy = c.Long(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedBy = c.Long(),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedBy = c.Long(),
                        DeletedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.AuthenticationTokenId)
                .ForeignKey("dbo.AuthenticationDetails", t => t.AuthenticationDetailId, cascadeDelete: true)
                .Index(t => t.AuthenticationDetailId);
            
            CreateTable(
                "dbo.UserMemberships",
                c => new
                    {
                        UserMembershipId = c.Long(nullable: false, identity: true),
                        FirstName = c.String(nullable: false),
                        LastName = c.String(),
                        Email = c.String(nullable: false),
                        Password = c.String(),
                        Provider = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedBy = c.Long(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedBy = c.Long(),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedBy = c.Long(),
                        DeletedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.UserMembershipId);
            
            CreateTable(
                "dbo.Contacts",
                c => new
                    {
                        ContactId = c.Long(nullable: false, identity: true),
                        UserMembershipId = c.Long(nullable: false),
                        Name = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        PhoneNo = c.String(),
                        Company = c.String(),
                        Address = c.String(),
                        Website = c.String(),
                        InternetCall = c.String(),
                        IM = c.String(),
                        JobTittle = c.String(),
                        Notes = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        CreatedBy = c.Int(nullable: false),
                        ModifiedBy = c.Long(),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedBy = c.Long(),
                        DeletedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.ContactId);
            
            CreateTable(
                "dbo.EmailDetails",
                c => new
                    {
                        EmailDetailsId = c.Long(nullable: false, identity: true),
                        EmailTempletId = c.Long(nullable: false),
                        EmailFrom = c.String(),
                        EmailTo = c.String(nullable: false),
                        EmployeCode = c.String(),
                        FirstName = c.String(),
                        IsBounce = c.Boolean(nullable: false),
                        InProcess = c.Boolean(nullable: false),
                        BounceStatus = c.Int(nullable: false),
                        Notification = c.String(),
                        UserMembershipId = c.Long(nullable: false),
                        MessageId = c.String(),
                        FileName = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        CreatedBy = c.Long(nullable: false),
                        ModifiedBy = c.Long(),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedBy = c.Long(),
                        DeletedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.EmailDetailsId)
                .ForeignKey("dbo.EmailTemplets", t => t.EmailTempletId, cascadeDelete: true)
                .Index(t => t.EmailTempletId);
            
            CreateTable(
                "dbo.EmailTemplets",
                c => new
                    {
                        EmailTempletId = c.Long(nullable: false, identity: true),
                        UserMembershipId = c.Long(nullable: false),
                        EmailFrom = c.String(nullable: false),
                        EmailBody = c.String(nullable: false),
                        EmailSubject = c.String(),
                        FileName = c.String(),
                        FilePath = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        CreatedBy = c.Long(nullable: false),
                        ModifiedBy = c.Long(),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedBy = c.Long(),
                        DeletedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.EmailTempletId)
                .ForeignKey("dbo.UserMemberships", t => t.UserMembershipId, cascadeDelete: true)
                .Index(t => t.UserMembershipId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EmailTemplets", "UserMembershipId", "dbo.UserMemberships");
            DropForeignKey("dbo.EmailDetails", "EmailTempletId", "dbo.EmailTemplets");
            DropForeignKey("dbo.AuthenticationDetails", "UserMembershipId", "dbo.UserMemberships");
            DropForeignKey("dbo.AuthenticationTokens", "AuthenticationDetailId", "dbo.AuthenticationDetails");
            DropIndex("dbo.EmailTemplets", new[] { "UserMembershipId" });
            DropIndex("dbo.EmailDetails", new[] { "EmailTempletId" });
            DropIndex("dbo.AuthenticationTokens", new[] { "AuthenticationDetailId" });
            DropIndex("dbo.AuthenticationDetails", new[] { "UserMembershipId" });
            DropTable("dbo.EmailTemplets");
            DropTable("dbo.EmailDetails");
            DropTable("dbo.Contacts");
            DropTable("dbo.UserMemberships");
            DropTable("dbo.AuthenticationTokens");
            DropTable("dbo.AuthenticationDetails");
        }
    }
}
