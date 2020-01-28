using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Locatie.Data;
using Locatie.Models;
using Locatie.Repositories.Core;
using Microsoft.EntityFrameworkCore;

namespace Locatie.Repositories.Persistence
{
    public class LocationRepository : Repository<Location>, ILocationRepository
    {
        public LocationRepository(
            LocatieContext locatieContext
        ) : base(locatieContext)
        {
        }

        // TODO: Round down and cache
        public async Task<Dictionary<Location, double>> GetByCoordinates(double latitude, double longitude)
        {
            var utility = new Utils.Utility();

            var locations = await GetAllASync();
            Dictionary<Location, double> locationDistances = new Dictionary<Location, double>();
            foreach(var location in locations)
            {
                var distance = utility.GetDistanceInMeters(location.Latitude, location.Longitude, latitude, longitude);
                if (distance <= Utils.Constants.KNOWN_LOCATION_RADIUS)
                {
                    locationDistances.Add(location, distance);
                }
            }

            return locationDistances.OrderBy(l => l.Value).ToDictionary(x => x.Key, x => x.Value);
        }

        // TODO: Use normal caching
        private List<Location> _locations = null;
        public override async Task<IEnumerable<Location>> GetAllASync()
        {
            if (_locations == null)
            {
                _locations = await dbSet.ToListAsync();
            }
            return _locations;
        }

        public Task<Location> GetByIdWithHistory(int id)
        {
            return dbSet.Where(l => l.Id == id)
                .Include(l => l.Days)
                .FirstOrDefaultAsync();
        }
    }
}
