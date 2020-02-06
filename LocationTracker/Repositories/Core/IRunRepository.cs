using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocationTracker.Models;

namespace LocationTracker.Repositories.Core
{
    public interface IRunRepository
    {
        Task<Run> GetRun(DateTime date);
        Task<List<Run>> GetRuns();
    }
}
