using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
namespace Inta.Inspectio.Model
{
    public class DatabaseContext :DbContext
    {
        
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Request> Requests { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.;Database=Validation_Db;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=False");
            base.OnConfiguring(optionsBuilder);
           
           
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Customer>().HasKey(c=>c.Id);
            modelBuilder.Entity<Customer>().Property(c => c.Id).ValueGeneratedNever();

            modelBuilder.Entity<Request>().HasKey(r=>r.Id);
            modelBuilder.Entity<Request>().Property(r=>r.Id).ValueGeneratedNever();
        }
    }
}
