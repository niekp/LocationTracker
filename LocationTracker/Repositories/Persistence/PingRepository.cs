using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocationTracker.Data;
using LocationTracker.Models;
using LocationTracker.Repositories.Core;
using LocationTracker.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LocationTracker.Repositories.Persistence
{
    public class PingRepository : Repository<Ping>, IPingRepository
    {
        private readonly ICache cache;

        public PingRepository(
            LocationContext locatieContext,
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

        public async Task<List<Coordinate>> GetUniqueLocationsBetweenDates(DateTime dateFrom, DateTime dateTo)
        {
            var pings = await (from p in db.Ping
                    where p.Time >= dateFrom && p.Time <= dateTo
                    && p.DayId > 0
                    group p by new { p.Latitude, p.Longitude } into pu
                    select new
                    {
                        pu.Key.Latitude,
                        pu.Key.Longitude,
                        Count = pu.Count()
                    }).OrderBy(p => p.Latitude)
                    .ThenBy(p => p.Longitude)
                    .ToListAsync();

            return pings.Select(p => new Coordinate() {
                Latitude = p.Latitude,
                Longitude = p.Longitude
            }).ToList();
        }

        public async Task<(double x_min, double x_max, double y_min, double y_max, DateTime FirstPing)> GetMinMax(DateTime dateFrom, DateTime dateTo)
        {
            var x_min = await db.Ping.Where(p => p.Time >= dateFrom && p.Time <= dateTo).MinAsync(p => p.Latitude);
            var x_max = await db.Ping.Where(p => p.Time >= dateFrom && p.Time <= dateTo).MaxAsync(p => p.Latitude);

            var y_min = await db.Ping.Where(p => p.Time >= dateFrom && p.Time <= dateTo).MinAsync(p => p.Longitude);
            var y_max = await db.Ping.Where(p => p.Time >= dateFrom && p.Time <= dateTo).MaxAsync(p => p.Longitude);

            var FirstPing = await db.Ping.Where(p => p.Time >= dateFrom && p.Time <= dateTo).MinAsync(p => p.Time);

            return (x_min, x_max, y_min, y_max, FirstPing);
        }
    }
}
