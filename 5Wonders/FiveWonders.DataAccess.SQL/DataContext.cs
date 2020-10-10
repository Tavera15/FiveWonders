using FiveWonders.core.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.DataAccess.SQL
{
    public class DataContext : DbContext
    {
        public DataContext() : base("DefaultConnection") { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<SizeChart> SizeCharts { get; set; }
        public DbSet<Basket> Baskets { get; set; }
        public DbSet<BasketItem> BasketItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<FWonderOrder> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<HomePage> HomeData { get; set; }
        public DbSet<GalleryImg> GalleryImgs { get; set; }
        public DbSet<ServicePage> ServiceData { get; set; }
        public DbSet<SocialMedia> SocialMedia { get; set; }
        public DbSet<CustomOptionList> ColorSets { get; set; }
    }
}
