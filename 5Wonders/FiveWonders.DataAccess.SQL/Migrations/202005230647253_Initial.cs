namespace FiveWonders.DataAccess.SQL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        mID = c.String(nullable: false, maxLength: 128),
                        mCategoryName = c.String(nullable: false),
                        mTimeEntered = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.mID);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        mID = c.String(nullable: false, maxLength: 128),
                        mName = c.String(nullable: false),
                        mDesc = c.String(),
                        mCategory = c.String(),
                        mImage = c.String(),
                        mPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        mSubCategories = c.String(),
                        mSizeChart = c.String(),
                        mTimeEntered = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.mID);
            
            CreateTable(
                "dbo.SizeCharts",
                c => new
                    {
                        mID = c.String(nullable: false, maxLength: 128),
                        mChartName = c.String(nullable: false),
                        mImageChartUrl = c.String(nullable: false),
                        mSizesToDisplay = c.String(),
                        mTimeEntered = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.mID);
            
            CreateTable(
                "dbo.SubCategories",
                c => new
                    {
                        mID = c.String(nullable: false, maxLength: 128),
                        mSubCategoryName = c.String(nullable: false),
                        isEventOrTheme = c.Boolean(nullable: false),
                        mTimeEntered = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.mID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SubCategories");
            DropTable("dbo.SizeCharts");
            DropTable("dbo.Products");
            DropTable("dbo.Categories");
        }
    }
}
