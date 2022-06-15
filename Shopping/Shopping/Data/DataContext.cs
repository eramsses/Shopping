using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shopping.Data.Entities;

namespace Shopping.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> options) :base(options)
        {
        }

        //Registrar las entidades (Tablas DB)
        public DbSet<Category> Categories { get; set; }

        public DbSet<City> Cities { get; set; }

        public DbSet<Country> Countries { get; set; }

        public DbSet<State> States { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductCategory> ProductCategories { get; set; }

        public DbSet<ProductImage> ProductImages { get; set; }

        public DbSet<TemporalSale> TemporalSales { get; set; }

        public DbSet<Sale> Sales { get; set; }

        public DbSet<SaleDetail> SalesDetail { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //Indica que el nombre es único para la tabla Country
            modelBuilder.Entity<Country>().HasIndex(c => c.Name).IsUnique();
            //Indica que el nombre es único para la tabla Category
            modelBuilder.Entity<Category>().HasIndex(c => c.Name).IsUnique();
            //Indica que el nombre es único para la tabla Product
            modelBuilder.Entity<Product>().HasIndex(p => p.Name).IsUnique();


            //Indices compuestos
            modelBuilder.Entity<State>().HasIndex("Name", "CountryId").IsUnique();
            modelBuilder.Entity<City>().HasIndex("Name", "StateId").IsUnique();
            modelBuilder.Entity<ProductCategory>().HasIndex("ProductId", "CategoryId").IsUnique();
        }
    }
}
