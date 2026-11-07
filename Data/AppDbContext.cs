
using Microsoft.EntityFrameworkCore;
using Practicum.Models;

namespace Practicum.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        public DbSet<Device> Devices => Set<Device>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            b.Entity<Device>().HasData(
                new Device
                {
                    DeviceId = "DEV-001",
                    FieldName = "North Field",
                    Status = "Idle",
                    LastUpdated = new DateTime(2026, 1, 1)
                },
                new Device
                {
                    DeviceId = "DEV-002",
                    FieldName = "South Field",
                    Status = "Idle",
                    LastUpdated = new DateTime(2026, 1, 1)
                },
                new Device
                {
                    DeviceId = "DEV-003",
                    FieldName = "East Field",
                    Status = "Idle",
                    LastUpdated = new DateTime(2026, 1, 1)
                }
            );
        }
    }
}
