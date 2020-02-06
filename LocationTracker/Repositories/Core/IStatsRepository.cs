using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocationTracker.Models;

namespace LocationTracker.Repositories.Core
{
    public interface IStatsRepository
    {
        Task<Stats> GetStats(DateTime from, DateTime to);
    }
}
