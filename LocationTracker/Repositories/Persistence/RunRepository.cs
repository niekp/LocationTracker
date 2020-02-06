using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocationTracker.Data;
using LocationTracker.Models;
using LocationTracker.Repositories.Core;
using LocationTracker.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LocationTracker.Repositories.Persistence
{
    public class RunRepository : IRunRepository
    {
        private readonly ITagRepository tagRepository;
        private readonly IRideRepository rideRepository;
        private readonly ICache cache;

        public RunRepository(
            ITagRepository tagRepository,
            IRideRepository rideRepository,
            ICache cache
        )
        {
            this.rideRepository = rideRepository;
            this.tagRepository = tagRepository;
            this.cache = cache;
        }

        private Run GetRunFromRides(List<Ride> rides)
        {
            var run = new Run();
            run.Laps = rides.OrderBy(r => r.TimeFrom).ToList();
            run.TimeFrom = run.Laps.First().TimeFrom;
            run.TimeTo = run.Laps.Last().TimeTo;
            run.TotalMinutes = (run.TimeTo - run.TimeFrom).TotalMinutes;

            run.MinutesMoving = 0;
            run.DistanceInMeters = 0;
            foreach (var r in rides)
            {
                run.MinutesMoving += (r.TimeTo - r.TimeFrom).TotalMinutes;
                run.DistanceInMeters += r.DistanceInMeters ?? 0;
            }

            return run;
        }

        public async Task<List<Run>> GetRuns()
        {
            var tag = await tagRepository.GetOrCreate(Constants.RUNNING_TAG);
            var rides = await rideRepository.GetByTag(tag);
            var runs = new List<Run>();

            foreach (var date in rides.Select(r => r.TimeFrom.Date).Distinct())
            {
                var runsOnDate = rides.Where(r => r.TimeFrom.Date == date);
                var run = GetRunFromRides(runsOnDate.ToList());

                runs.Add(run);
            }

            return runs.OrderByDescending(r => r.TimeFrom).ToList();
        }

        public async Task<Run> GetRun(DateTime date)
        {
            var tag = await tagRepository.GetOrCreate(Constants.RUNNING_TAG);
            var rides = (await rideRepository.GetByTag(tag)).Where(r => r.TimeFrom.Date == date);
            var run = GetRunFromRides(rides.ToList());

            return run;
        }
    }
}
