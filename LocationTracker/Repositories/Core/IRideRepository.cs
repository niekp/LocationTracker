using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocationTracker.Models;

namespace LocationTracker.Repositories.Core
{
    public interface IRideRepository : IRepository<Ride>
    {
        public Task<Ride> GetByIdFull(int id);
        public Task SetTags(int rideId, string tags);
        public Task<List<Ride>> GetByTag(Tag Tag);
    }
}
