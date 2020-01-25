using System;
using System.Linq;
using System.Threading.Tasks;
using Locatie.Data;
using Locatie.Models;
using Locatie.Repositories.Core;
using Microsoft.EntityFrameworkCore;

namespace Locatie.Repositories.Persistence
{
    public class RideRepository : Repository<Ride>, IRideRepository
    {
        public RideRepository(LocatieContext locatieContext) : base(locatieContext)
        {
        }

        public Task<Ride> GetByIdWithPings(int id)
        {
            return dbSet.Where(r => r.Id == id)
                .Include(r => r.Pings)
                .FirstOrDefaultAsync();
        }
    }
}
