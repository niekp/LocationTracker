using System;
using Locatie.Models;
using Microsoft.EntityFrameworkCore;

namespace Locatie.Data
{
    public class LocatieContext : DbContext
    {
        public LocatieContext(DbContextOptions<LocatieContext> options) : base(options)
        {
        }

        public DbSet<Ping> Ping { get; set; }
        public DbSet<Location> Location { get; set; }
        public DbSet<Ride> Ride { get; set; }
        public DbSet<Day> Day { get; set; }
        public DbSet<DayPing> DayPing { get; set; }
        public DbSet<Tag> Tag { get; set; }
        public DbSet<RideTag> RideTag { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Day Ping
            modelBuilder.Entity<DayPing>()
                .HasKey(dp => new { dp.DayId, dp.PingId });
            modelBuilder.Entity<DayPing>()
                .HasOne(dp => dp.Day)
                .WithMany(d => d.Pings)
                .HasForeignKey(dp => dp.DayId);
            modelBuilder.Entity<DayPing>()
                .HasOne(dp => dp.Ping)
                .WithMany(p => p.Days)
                .HasForeignKey(dp => dp.PingId);

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
