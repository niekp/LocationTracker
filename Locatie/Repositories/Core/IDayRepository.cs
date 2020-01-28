using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Locatie.Models;

namespace Locatie.Repositories.Core
{
    public interface IDayRepository : IRepository<Day>
    {
        Task<List<Day>> GetDays(DateTime From, DateTime To);
        Task<Day> GetPrevious(Day day);
        Task<Day> GetNext(Day day);
        Task MergeLocation(int fromId, int toId);
        Task DeleteDay(int id, bool removePings = false);
        Task<Day> GetByRide(Ride ride);
    }
}
