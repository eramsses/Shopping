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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //Indica que el nombre es único para la tabla Country
            modelBuilder.Entity<Country>().HasIndex(c => c.Name).IsUnique();
            //Indica que el nombre es único para la tabla Category
            modelBuilder.Entity<Category>().HasIndex(c => c.Name).IsUnique();

            //Indices compuestos
            modelBuilder.Entity<State>().HasIndex("Name", "CountryId").IsUnique();
            modelBuilder.Entity<City>().HasIndex("Name", "StateId").IsUnique();
        }
    }
}
