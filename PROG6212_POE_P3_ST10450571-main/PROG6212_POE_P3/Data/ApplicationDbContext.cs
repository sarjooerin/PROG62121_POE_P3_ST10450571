using Microsoft.EntityFrameworkCore;
using PROG6212_POE_P3.Models;

namespace PROG6212_POE_P3.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets for your entities
        public DbSet<User> Users { get; set; }
        public DbSet<Claim> Claim { get; set; }

        // Optional: Seed initial data
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "lecturer1", Password = "pass123", Role = "Lecturer", Name = "Lec 1", HourlyRate = 150 },
                new User { Id = 2, Username = "hr1", Password = "hr123", Role = "HR", Name = "HR", HourlyRate = 0 },
                new User { Id = 3, Username = "coordinator1", Password = "coord123", Role = "Coordinator", Name = "Coord 1", HourlyRate = 0 },
                new User { Id = 4, Username = "manager1", Password = "manager123", Role = "Manager", Name = "Manager 1", HourlyRate = 0 }
            );

            // Optional: Seed Claims (remove if not needed)
            modelBuilder.Entity<Claim>().HasData(
                new Claim { Id = 1, LecturerName = "Lec 1", DateSubmitted = DateTime.Now.AddDays(-5), HoursWorked = 10, HourlyRate = 150, Status = ClaimStatus.Pending, UploadedFileName = "doc1.pdf", CoordinatorRemarks = "" }
            );
        }
    }
}
