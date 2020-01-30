using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Locatie.Data;
using Locatie.Models;
using Locatie.Repositories.Core;
using Locatie.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Locatie.Repositories.Persistence
{
    public class PingRepository : Repository<Ping>, IPingRepository
    {
        private readonly ICache cache;

        public PingRepository(
            LocatieContext locatieContext,
            ICache cache
        ) : base(locatieContext)
        {
            this.cache = cache;
        }

        public Task<List<Ping>> GetBetweenDates(DateTime from, DateTime to)
        {
            return dbSet.Where(p => p.Time >= from && p.Time <= to).ToListAsync();
        }

        public async Task DeleteBetweenDates(DateTime from, DateTime to)
        {
            var pings = await GetBetweenDates(from, to);
            foreach (var ping in pings)
            {
                Delete(ping.Id);
            }

            await db.SaveChangesAsync();
        }

        public Task<Ping> GetLastPing(bool onlyProcessed = false)
        {
            return dbSet
                .Where(p => !onlyProcessed || p.Processed == 1)
                .OrderByDescending(p => p.Time)
                .Include(p => p.Ride)
                .Include(p => p.Location)
                .Take(1)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Ping>> GetLastPings(bool onlyProcessed = false, int amount = 1)
        {
            return await dbSet
                .Where(p => !onlyProcessed || p.Processed == 1)
                .OrderByDescending(p => p.Time)
                .Include(p => p.Ride)
                .Include(p => p.Location)
                .Take(amount)
                .ToListAsync();
        }

        public Task<List<Ping>> GetPings(Day day)
        {
            return cache.GetOrCreate(string.Format("PingRepository_GetPings_d_{0}", day.Id), async cache_item =>
            {
                cache_item.SetOptions(cache.GetCacheOption());
                await db.Entry(day).Collection(x => x.Pings).LoadAsync();
                return day.Pings.ToList();
            });
        }

        public Task<List<Ping>> GetPings(Ride ride)
        {
            return cache.GetOrCreate(string.Format("PingRepository_GetPings_r_{0}", ride.Id), async cache_item =>
            {
                cache_item.SetOptions(cache.GetCacheOption());
                await db.Entry(ride).Collection(x => x.Pings).LoadAsync();
                return ride.Pings.ToList();
            });
            
        }

        public Task<List<Ping>> GetUnprocessed()
        {
            return dbSet.Where(p => p.Processed == 0).OrderBy(p => p.Time).ToListAsync();
        }

        public async Task MergeLocation(int fromId, int toId)
        {
            var pings = await dbSet.Where(p => p.LocationId == fromId).ToListAsync();

            foreach (var ping in pings)
            {
                ping.LocationId = toId;
            }

            await SaveAsync();
        }
    }
}
