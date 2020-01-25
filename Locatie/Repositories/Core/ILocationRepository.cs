using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Locatie.Models;

namespace Locatie.Repositories.Core
{
    public interface ILocationRepository : IRepository<Location>
    {
        Task<Location> GetByIdWithHistory(int id);
    }
}
