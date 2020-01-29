using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Locatie.Models;

namespace Locatie.Repositories.Core
{
    public interface IPingRepository : IRepository<Ping>
    {
        Task<List<Ping>> GetPings(Day day);
        Task<List<Ping>> GetPings(Ride day);
        Task<List<Ping>> GetUnprocessed();
        Task<Ping> GetLastPing(bool onlyProcessed = false);
        Task<List<Ping>> GetLastPings(bool onlyProcessed = false, int amount = 1);

        Task MergeLocation(int fromId, int toId);

        Task<List<Ping>> GetBetweenDates(DateTime from, DateTime to);
        Task DeleteBetweenDates(DateTime from, DateTime to);
    }
}
