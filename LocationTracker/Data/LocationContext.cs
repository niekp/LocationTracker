using System;
using LocationTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace LocationTracker.Data
{
    public class LocationContext : DbContext
    {
        public LocationContext(DbContextOptions<LocationContext> options) : base(options)
        {
        }

        public DbSet<Ping> Ping { get; set; }
        public DbSet<Location> Location { get; set; }
        public DbSet<Ride> Ride { get; set; }
        public DbSet<Day> Day { get; set; }
        public DbSet<Tag> Tag { get; set; }
        public DbSet<RideTag> RideTag { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<UserSession> UserSession { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Ride Tag
            modelBuilder.Entity<RideTag>()
                .HasKey(rt => new { rt.RideId, rt.TagId });
            modelBuilder.Entity<RideTag>()
                .HasOne(rt => rt.Ride)
                .WithMany(r => r.Tags)
                .HasForeignKey(rt => rt.RideId);
            modelBuilder.Entity<RideTag>()
                .HasOne(rt => rt.Tag)
                .WithMany(t => t.Rides)
                .HasForeignKey(bc => bc.TagId);
        }

    }
}
