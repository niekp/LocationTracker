using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Locatie.Models;
using Locatie.Repositories.Core;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Locatie.Controllers
{
    public class DayController : Controller
    {
        public readonly IDayRepository dayRepository;
        public readonly IPingRepository pingRepository;
        public readonly IRideRepository rideRepository;

        public DayController(
            IDayRepository dayRepository,
            IPingRepository pingRepository,
            IRideRepository rideRepository
        )
        {
            this.dayRepository = dayRepository;
            this.pingRepository = pingRepository;
            this.rideRepository = rideRepository;
        }

        public async Task<IActionResult> Delete(int id)
        {
            var day = await dayRepository.GetByIdAsync(id);
            var returnLocationId = day.LocationId;

            // Find the previous day
            var previousDay = await dayRepository.GetPrevious(day);

            // Merge with the previous day
            if (previousDay is Day)
            {
                // Up the end time of the previous record
                previousDay.TimeTo = day.TimeTo;
                dayRepository.Update(previousDay);

                var rideId = previousDay.RideId;
                var locationId = previousDay.LocationId;
                var dayId = previousDay.Id;

                // Update the pings of the to be deleted day to the location of the previous day
                var pings = await pingRepository.GetPings(day);
                foreach (var ping in pings)
                {
                    ping.RideId = rideId;
                    ping.LocationId = locationId;
                    ping.DayId = dayId;
                    pingRepository.Update(ping);
                }

                // Check if the previous record is a ride. This also needs to be extended
                if (previousDay.Ride is Ride)
                {
                    previousDay.Ride.TimeTo = day.TimeTo;
                    rideRepository.Update(previousDay.Ride);
                    var nextDay = await dayRepository.GetNext(day);

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
                        dayRepository.Delete(nextDay.Id);
                    }
                }

                dayRepository.Update(previousDay);
            }

            dayRepository.Delete(day.Id);
            dayRepository.Save();

            return RedirectToAction("Index", "Location", new { id = returnLocationId });
        }
    }
}
