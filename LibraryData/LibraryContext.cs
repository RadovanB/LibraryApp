using LibraryData.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryData
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions opt) : base(opt) 
        {
        }

        public DbSet<Patron> Patrons { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<Checkout> Checkouts { get; set; }
        public DbSet<CheckoutHistory> CheckoutHistories { get; set; }
        public DbSet<LibraryBranch> LibraryBranches { get; set; }
        public DbSet<BranchHours> BranchHours { get; set; }
        public DbSet<LibraryCard> LibraryCards { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<LibraryAssets> LibraryAssets { get; set; }
        public DbSet<Hold> Holds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
             modelBuilder.Entity<LibraryAssets>()
               .Property(p => p.Cost)
               .HasPrecision(9, 2); // or whatever your schema specifies

            modelBuilder.Entity<LibraryCard>()
              .Property(p => p.Fees)
              .HasPrecision(9, 2);
        }
    }
}
