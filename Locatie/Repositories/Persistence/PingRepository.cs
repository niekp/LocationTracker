using System;
using System.Linq;
using Locatie.Data;
using Locatie.Models;
using Locatie.Repositories.Core;

namespace Locatie.Repositories.Persistence
{
    public class PingRepository : Repository<Ping>, IPingRepository
    {
        public PingRepository(LocatieContext locatieContext) : base(locatieContext)
        {
        }

        public Ping GetLastPing()
        {
            var p = dbSet.Take(1).OrderByDescending(p => p.Time).FirstOrDefault();
            db.Entry(p).Reference(x => x.Location).Load();
            db.Entry(p).Collection(x => x.Days).Load();
            db.Entry(p.Days.FirstOrDefault()).Reference(x => x.Day).Load();

            return p;
        }
    }
}
