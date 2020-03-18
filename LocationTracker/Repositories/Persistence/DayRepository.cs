using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocationTracker.Data;
using LocationTracker.Models;
using LocationTracker.Repositories.Core;
using Microsoft.EntityFrameworkCore;

namespace LocationTracker.Repositories.Persistence
{
    public class DayRepository : Repository<Day>, IDayRepository
    {
        private readonly IPingRepository pingRepository;
        private readonly IRideRepository rideRepository;

        public DayRepository(
            LocationContext locatieContext,
            IPingRepository pingRepository,
            IRideRepository rideRepository
        ) : base(locatieContext)
        {
            this.pingRepository = pingRepository;
            this.rideRepository = rideRepository;
        }

        public override async Task<Day> GetByIdAsync(object id)
        {
            return await dbSet.Where(d => d.Id == (int)id)
                .Include(d => d.Location)
                .Include(d => d.Ride)
                .FirstOrDefaultAsync();
        }

        public Task<List<Day>> GetDays(DateTime From, DateTime To)
        {
            return dbSet.Where(d =>
                (d.TimeFrom >= From && d.TimeFrom <= To)
                ||
                (d.TimeTo >= From && d.TimeTo <= To)
            )
            .Include(d => d.Location)
            .Include(d => d.Ride)
            .Include("Ride.Pings")
            .OrderBy(d => d.TimeFrom)
            .ToListAsync();
        }

        public Task<Day> GetPrevious(Day day)
        {
            return dbSet.Where(d =>
                    d.TimeTo <= day.TimeFrom
                    && d.Id != day.Id
                ).OrderByDescending(d => d.TimeFrom)
                .Include(d => d.Location)
                .Include(d => d.Ride)
                .Take(1)
                .FirstOrDefaultAsync();
        }

        public Task<Day> GetNext(Day day)
        {
            return dbSet.Where(d =>
                    d.TimeFrom >= day.TimeTo
                    && d.Id != day.Id
                ).OrderBy(d => d.TimeFrom)
                .Include(d => d.Location)
                .Include(d => d.Ride)
                .Take(1)
                .FirstOrDefaultAsync();
        }

        public async Task MergeLocation(int fromId, int toId)
        {
            var days = await dbSet.Where(d => d.LocationId == fromId).ToListAsync();

            foreach (var day in days)
            {
                day.LocationId = toId;
            }

            await SaveAsync();
        }

        public async Task DeleteDay(int id, bool removePings = false)
        {
            var day = await GetByIdAsync(id);
            var pings = new List<Ping>();

            // Find the previous day
            var previousDay = await GetPrevious(day);

            // Merge with the previous day
            if (previousDay is Day)
            {
                // Up the end time of the previous record
                previousDay.TimeTo = day.TimeTo;
                Update(previousDay);

                var rideId = previousDay.RideId;
                var locationId = previousDay.LocationId;
                var dayId = previousDay.Id;

                // Update the pings of the to be deleted day to the location of the previous day
                pings = await pingRepository.GetPings(day);
                foreach (var ping in pings)
                {
                    if (!removePings)
                    {
                        ping.RideId = rideId;
                        ping.LocationId = locationId;
                        ping.DayId = dayId;
                        pingRepository.Update(ping);
                    }
                    else
                    {
                        pingRepository.Delete(ping.Id);
                    }
                }

                await SaveAsync();

                // Check if the previous record is a ride. This also needs to be extended
                if (previousDay.Ride is Ride)
                {
                    previousDay.Ride.TimeTo = day.TimeTo;
                    rideRepository.Update(previousDay.Ride);
                    var nextDay = await GetNext(day);

                    // Is the next record also a ride, Merge them together.
                    if (nextDay is Day && nextDay.Ride is Ride)
                    {
                        previousDay.TimeTo = nextDay.TimeTo;
                        previousDay.Ride.TimeTo = nextDay.TimeTo;
                        previousDay.Ride.DistanceInMeters = null;

                        pings = await pingRepository.GetPings(nextDay);
                        foreach (var ping in pings)
                        {
                            ping.RideId = rideId;
                            ping.LocationId = locationId;
                            ping.DayId = dayId;
                            pingRepository.Update(ping);
                        }

                        rideRepository.Delete(nextDay.Ride.Id);
                        Delete(nextDay.Id);
                    }

                    previousDay.Ride.ResetDistance();
                    rideRepository.Update(previousDay.Ride);
                }

                Update(previousDay);
                await SaveAsync();

                if (day.Ride is Ride)
                {
                    // The previous merge actions removed all pings.
                    // But in case any pings didn't get moved double check by selecting with the ride instead of the day.
                    pings = await pingRepository.GetPings(day.Ride);
                    foreach (var ping in pings)
                    {
                        if (!removePings)
                        {
                            ping.RideId = rideId;
                            ping.LocationId = locationId;
                            ping.DayId = dayId;
                            pingRepository.Update(ping);
                        }
                        else
                        {
                            pingRepository.Delete(ping.Id);
                        }
                    }

                    rideRepository.Delete(day.Ride.Id);
                }
            }

            Delete(day.Id);
            await SaveAsync();
        }

        public async Task<Day> GetByRide(Ride ride)
        {
            return await dbSet.Where(d => d.RideId == ride.Id).FirstOrDefaultAsync();
        }
    }
}
