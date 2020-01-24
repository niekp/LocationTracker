using System;
using System.Linq;
using Locatie.Data;
using Locatie.Models;
using Locatie.Repositories.Core;

namespace Locatie.Repositories.Persistence
{
    public class LocationRepository : Repository<Location>, ILocationRepository
    {
        public LocationRepository(LocatieContext locatieContext) : base(locatieContext)
        {
        }
    }
}
