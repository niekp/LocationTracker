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
    }
}
