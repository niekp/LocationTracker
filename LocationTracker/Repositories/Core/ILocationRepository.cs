using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocationTracker.Models;

namespace LocationTracker.Repositories.Core
{
    public interface ILocationRepository : IRepository<Location>
    {
        Task<Location> GetByIdWithHistory(int id);
        Task<Dictionary<Location, double>> GetByCoordinates(double latitude, double longitude);
    }
}
