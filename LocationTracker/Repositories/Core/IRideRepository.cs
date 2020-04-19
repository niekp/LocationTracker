using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocationTracker.Models;

namespace LocationTracker.Repositories.Core
{
    public interface IRideRepository : IRepository<Ride>
    {
        Task<Ride> GetByIdFull(int id);
        Task SetTags(int rideId, string tags);
        Task<List<Ride>> GetByTag(Tag Tag);
        Task SplitRide(int rideId, long timestamp);
    }
}
