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
            return dbSet.Take(1).OrderByDescending(p => p.Tijd).FirstOrDefault();
        }
    }
}
