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
    public class DayRepository : Repository<Day>, IDayRepository
    {
        public DayRepository(LocatieContext locatieContext) : base(locatieContext)
        {
        }

        public Task<List<Day>> GetDays(DateTime From, DateTime To)
        {
            return dbSet.Where(d =>
                (d.TimeFrom >= From && d.TimeFrom <= To)
                ||
                (d.TimeTo >= From && d.TimeTo <= To)
            )
            .Include(d => d.Location)
            .Include(d => d.Ride)
            .Include("Ride.Pings")
            .OrderBy(d => d.TimeFrom)
            .ToListAsync();
        }
    }
}
