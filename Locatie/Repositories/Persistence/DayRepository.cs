using System;
using System.Linq;
using Locatie.Data;
using Locatie.Models;
using Locatie.Repositories.Core;

namespace Locatie.Repositories.Persistence
{
    public class DayRepository : Repository<Day>, IDayRepository
    {
        public DayRepository(LocatieContext locatieContext) : base(locatieContext)
        {
        }
    }
}
