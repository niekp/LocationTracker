using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocationTracker.Models;
using LocationTracker.Utils;

namespace LocationTracker.Repositories.Core
{
    public interface IRunRepository
    {
        Task<Run> GetRun(DateTime date, string Tag = Constants.RUNNING_TAG);
        Task<List<Run>> GetRuns(string Tag = Constants.RUNNING_TAG);
    }
}
