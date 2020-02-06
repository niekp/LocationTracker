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
    public class LocationRepository : Repository<Location>, ILocationRepository
    {
        private readonly ICache cache;
        private readonly Utility utility;

        public LocationRepository(
            LocationContext locatieContext,
            ICache cache
        ) : base(locatieContext)
        {
            this.cache = cache;
            utility = new Utility();
        }

        public async Task<Dictionary<Location, double>> GetByCoordinates(double latitude, double longitude)
        {
            // Cache rounded down to 4 characters. This creates a max of 10m deviation so good enough.
            return await cache.GetOrCreate(
                string.Format(
                    "LocationRepository_GetByCoordinates_{0}_{1}",
                    Math.Round(latitude, 4),
                    Math.Round(latitude, 4)
                ),
            async cache_item => {
                cache_item.SetOptions(cache.GetCacheOption());
                var locations = await GetAllASync();
                Dictionary<Location, double> locationDistances = new Dictionary<Location, double>();

                foreach (var location in locations)
                {
                    var distance = utility.GetDistanceInMeters(location.Latitude, location.Longitude, latitude, longitude);
                    if (distance <= Constants.KNOWN_LOCATION_RADIUS)
                    {
                        locationDistances.Add(location, distance);
                    }
                }

                return locationDistances.OrderBy(l => l.Value).ToDictionary(x => x.Key, x => x.Value);
            });
        }

        public override void Insert(Location obj)
        {
            cache.Remove("LocationRepository_GetAllAsync");
            base.Insert(obj);
        }

        public override async Task<IEnumerable<Location>> GetAllASync()
        {
            return await cache.GetOrCreate("LocationRepository_GetAllAsync", cache_item =>
            {
                cache_item.SetOptions(cache.GetCacheOption());
                return dbSet.ToListAsync();
            });
        }

        public Task<Location> GetByIdWithHistory(int id)
        {
            return dbSet.Where(l => l.Id == id)
                .Include(l => l.Days)
                .FirstOrDefaultAsync();
        }
    }
}
