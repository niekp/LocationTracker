using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocationTracker.Models;

namespace LocationTracker.Repositories.Core
{
    public interface IPingRepository : IRepository<Ping>
    {
        Task<List<Ping>> GetPings(Day day);
        Task<List<Ping>> GetPings(Ride day);
        Task<List<Ping>> GetUnprocessed();
        Task<Ping> GetLastPing(bool onlyProcessed = false);
        Task<List<Ping>> GetLastPings(bool onlyProcessed = false, int amount = 1);

        Task MergeLocation(int fromId, int toId);

        Task<List<Coordinate>> GetUniqueLocationsBetweenDates(DateTime from, DateTime to);
        Task<List<Ping>> GetBetweenDates(DateTime from, DateTime to);
        Task DeleteBetweenDates(DateTime from, DateTime to);
        Task<(double x_min, double x_max, double y_min, double y_max, DateTime FirstPing)> GetMinMax(DateTime from, DateTime to);
    }
}
