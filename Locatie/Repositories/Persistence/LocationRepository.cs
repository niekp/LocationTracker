using System;
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
        public LocationRepository(LocatieContext locatieContext) : base(locatieContext)
        {
        }

        public Task<Location> GetByIdWithHistory(int id)
        {
            return dbSet.Where(l => l.Id == id)
                .Include(l => l.Days)
                .FirstOrDefaultAsync();
        }
    }
}
