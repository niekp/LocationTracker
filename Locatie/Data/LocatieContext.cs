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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DayPing>()
                .HasKey(bc => new { bc.DayId, bc.PingId });
            modelBuilder.Entity<DayPing>()
                .HasOne(bc => bc.Day)
                .WithMany(b => b.Pings)
                .HasForeignKey(bc => bc.DayId);
            modelBuilder.Entity<DayPing>()
                .HasOne(bc => bc.Ping)
                .WithMany(c => c.Days)
                .HasForeignKey(bc => bc.PingId);
        }

    }
}
