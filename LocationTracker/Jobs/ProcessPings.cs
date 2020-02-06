using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using LocationTracker.Models;
using LocationTracker.Repositories.Core;
using LocationTracker.Utils;

namespace LocationTracker.Jobs
{
    public class ProcessPings
    {
        private readonly IPingRepository pingRepository;
        private readonly ILocationRepository locationRepository;
        private readonly IDayRepository dayRepository;
        private readonly IRideRepository rideRepository;
        private readonly Utility utility;
        private readonly ICache cache;

        public ProcessPings(
            IPingRepository pingRepository,
            ILocationRepository locationRepository,
            IRideRepository rideRepository,
            IDayRepository dayRepository,
            ICache cache
        )
        {
            this.pingRepository = pingRepository;
            this.locationRepository = locationRepository;
            this.rideRepository = rideRepository;
            this.dayRepository = dayRepository;
            utility = new Utility();
            this.cache = cache;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 60 * 30)]
        public async Task Process()
        {
            var pings = await pingRepository.GetUnprocessed();
            Ping previousPing = await pingRepository.GetLastPing(true);
            
            List<Ping> ridePings = new List<Ping>();
            List<Ping> locationPings = new List<Ping>();

            foreach (var ping in pings)
            {
                if (!(previousPing is Ping))
                {
                    previousPing = ping;
                    continue;
                }

                var distance = utility.GetDistanceInMeters(previousPing, ping);
                var kmh = utility.GetSpeed(previousPing.Time, ping.Time, distance);

                // Moving
                if (kmh >= Constants.MINIMUM_MOVING_SPEED)
                {
                    // Finish the locationPings before starting to move.
                    if (await IsValidLocation(locationPings))
                    {
                        await SaveLocationPings(locationPings);
                        // Reset the location list. These functions can append pings 1 at a time if needed
                        locationPings = new List<Ping>();

                        // Reset the ride list because if i moved inside a location i don't need to map that to a ride
                        ridePings = new List<Ping>();
                    }

                    ridePings.Add(ping);
                }
                // Not moving
                else
                {
                    if (await IsValidRide(ridePings))
                    {
                        await SaveRidePings(ridePings);
                        // Reset the location list. These functions can append pings 1 at a time if needed
                        ridePings = new List<Ping>();

                        // Reset the location list because if i'm standing at a trafic light don't create a location.
                        locationPings = new List<Ping>();
                    }
                    // If there is a ridePing stuck while at a valid location delete it so speed the process up
                    else if (ridePings.Count > 0 && await IsValidLocation(locationPings))
                    {
                        ridePings = new List<Ping>();
                    }

                    locationPings.Add(ping);
                }

                previousPing = ping;
            }

            // Save the last batch
            if (await IsValidLocation(locationPings))
            {
                await SaveLocationPings(locationPings);
            }

            if (await IsValidRide(ridePings))
            {
                await SaveRidePings(ridePings);
            }

            // Mark all as done
            if (pings.Count > 0)
            {
                var lastPing = await pingRepository.GetLastPing(true);

                var remainingPings = (await pingRepository.GetBetweenDates(pings[0].Time, lastPing.Time))
                    .Where(p => p.Processed == 0);
                foreach (var ping in remainingPings)
                {
                    ping.Processed = 1;
                    pingRepository.Update(ping);
                }
                await pingRepository.SaveAsync();
            }

            // RecurringJob.AddOrUpdate<ProcessPings>("ProcessPings", x => x.Process(), Cron.Minutely);
        }

        private async Task<bool> IsValidLocation(List<Ping> pings)
        {
            if (pings.Count == 0)
            {
                return false;
            }

            var coords = utility.GetCoordinates(pings);
            if (coords.Latitude == 0 || coords.Longitude == 0)
            {
                return false;
            }

            var seconds = GetTotalSeconds(pings);
            var locations = await locationRepository.GetByCoordinates(coords.Latitude, coords.Longitude);
            var knownLocation = locations.Any();

            // Check the time at the current location against the margins.
            if (seconds >= Constants.MINIMUM_SECONDS_UNKNOWN_LOCATION
                || (knownLocation && seconds > Constants.MINIMUM_SECONDS_KNOWN_LOCATION)
            )
            {
                return true;
            }

            // Check if the previous ping was at the same location
            var lastPing = await pingRepository.GetLastPing(true);
            if (locations.Select(l => l.Key.Id).ToList().Contains(lastPing.LocationId ?? 0))
            {
                return true;
            }

            return false;
        }

        private double GetTotalSeconds(List<Ping> pings)
        {
            return (pings[pings.Count - 1].Time - pings[0].Time).TotalSeconds;
        }

        private async Task<bool> IsValidRide(List<Ping> pings)
        {
            if (pings.Count == 0)
            {
                return false;
            }

            // Currently riding
            var ride = await GetCurrentRide();
            if (ride is Ride && (pings[0].Time - ride.TimeTo).TotalMinutes < 5)
            {
                return true;
            }

            var distance = utility.GetDistanceInMeters(pings);

            // Minimum time, distance, speed check
            if (
                GetTotalSeconds(pings) < Constants.MINIMUM_SECONDS_RIDING
                || distance < 100
                || utility.GetSpeed(pings[0].Time, pings[pings.Count - 1].Time, distance) < 1
            )
            {
                return false;
            }

            return true;
        }

        private async Task<Ride> GetCurrentRide()
        {
            var lastPings = await pingRepository.GetLastPings(true, 50);
            foreach (var ping in lastPings)
            {
                if ((ping.Time - lastPings[0].Time).TotalMinutes > 5)
                {
                    break;
                }

                if (ping.LocationId != null)
                {
                    break;
                }

                if (ping.RideId != null)
                {
                    return ping.Ride;
                }
            }

            return null;
        }

        private async Task SaveLocationPings(List<Ping> pings)
        {
            var coords = utility.GetCoordinates(pings);
            var locations = await locationRepository.GetByCoordinates(coords.Latitude, coords.Longitude);
            var knownLocation = locations.Any();
            var lastPing = await pingRepository.GetLastPing(true);
            Day day = null;

            Location location = locations.Select(k => k.Key).FirstOrDefault(); ;
            if(!knownLocation || !(location is Location))
            {
                location = new Location()
                {
                    Label = string.Format("Onbekend {0}", DateTime.Now.ToShortDateString()),
                    Latitude = coords.Latitude,
                    Longitude = coords.Longitude
                };
                locationRepository.Insert(location);
                await locationRepository.SaveAsync();
            }

            // Lookup if this matches with the last day record
            if (lastPing.LocationId == location.Id && lastPing.DayId != null)
            {
                day = await dayRepository.GetByIdAsync(lastPing.DayId);
            }

            // If not create a new day
            if (!(day is Day))
            {
                day = new Day()
                {
                    TimeFrom = pings[0].Time,
                    TimeTo = pings[pings.Count - 1].Time,
                    LocationId = location.Id
                };
                dayRepository.Insert(day);
                await dayRepository.SaveAsync();
            }
            else
            {
                day.TimeTo = pings[pings.Count - 1].Time;
                dayRepository.Update(day);
                await dayRepository.SaveAsync();
            }

            // Bind the pings to the location & day record
            foreach (var ping in pings)
            {
                ping.LocationId = location.Id;
                ping.RideId = null;
                ping.Processed = 1;
                ping.DayId = day.Id;
                pingRepository.Update(ping);
            }

            await pingRepository.SaveAsync();
            cache.ClearCache();
        }

        private async Task SaveRidePings(List<Ping> pings)
        {
            var ride = await GetCurrentRide();
            
            if (!(ride is Ride))
            {
                ride = new Ride()
                {
                    TimeFrom = pings[0].Time,
                    TimeTo = pings[pings.Count - 1].Time
                };
                rideRepository.Insert(ride);
                await rideRepository.SaveAsync();
            }
            else
            {
                ride.TimeTo = pings[pings.Count - 1].Time;
                rideRepository.Update(ride);
                await rideRepository.SaveAsync();
            }

            // Try to get the day
            var day = await dayRepository.GetByRide(ride);
            if (!(day is Day))
            {
                day = new Day()
                {
                    TimeFrom = pings[0].Time,
                    TimeTo = pings[pings.Count - 1].Time,
                    RideId = ride.Id
                };
                dayRepository.Insert(day);
                await dayRepository.SaveAsync();
            }
            else
            {
                day.TimeTo = pings[pings.Count - 1].Time;
                dayRepository.Update(day);
                await dayRepository.SaveAsync();
            }

            foreach (var ping in pings)
            {
                ping.RideId = ride.Id;
                ping.LocationId = null;
                ping.Processed = 1;
                ping.DayId = day.Id;
                pingRepository.Update(ping);
            }

            await pingRepository.SaveAsync();
            cache.ClearCache();

            // Save the ride distance to speed up some statistics
            ride = await rideRepository.GetByIdFull(ride.Id);
            ride.ResetDistance();
            rideRepository.Update(ride);
            await rideRepository.SaveAsync();
        }
    }
}
