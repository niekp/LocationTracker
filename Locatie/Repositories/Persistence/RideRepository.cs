using System;
using System.Linq;
using Locatie.Data;
using Locatie.Models;
using Locatie.Repositories.Core;

namespace Locatie.Repositories.Persistence
{
    public class RideRepository : Repository<Ride>, IRideRepository
    {
        public RideRepository(LocatieContext locatieContext) : base(locatieContext)
        {
        }
    }
}
