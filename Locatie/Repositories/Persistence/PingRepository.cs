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
    public class PingRepository : Repository<Ping>, IPingRepository
    {
        public PingRepository(LocatieContext locatieContext) : base(locatieContext)
        {
        }

        public async Task<List<Ping>> GetPings(Day day)
        {
            await db.Entry(day).Collection(x => x.Pings).LoadAsync();
            return day.Pings.ToList();
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
