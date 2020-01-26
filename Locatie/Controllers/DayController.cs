using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Locatie.Models;
using Locatie.Repositories.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Locatie.Controllers
{
    public class DayController : Controller
    {
        private readonly IDayRepository dayRepository;
        private readonly IPingRepository pingRepository;
        private readonly IRideRepository rideRepository;
        private readonly ILocationRepository locationRepository;

        public DayController(
            IDayRepository dayRepository,
            IPingRepository pingRepository,
            IRideRepository rideRepository,
            ILocationRepository locationRepository
        )
        {
            this.dayRepository = dayRepository;
            this.pingRepository = pingRepository;
            this.rideRepository = rideRepository;
            this.locationRepository = locationRepository;
        }

        public async Task<IActionResult> Edit(int id)
        {
            var day = await dayRepository.GetByIdAsync(id);
            var locations = (await locationRepository.GetAllASync()).OrderBy(l => l.Label);
            ViewBag.LocationOptions = locations.Select(x => new SelectListItem { Text = x.Label, Value = x.Id.ToString(), Selected = x.Id == day.LocationId }).ToList();

            return View(day);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Day day)
        {
            var _day = await dayRepository.GetByIdAsync(day.Id);
            var oldLocationId = _day.LocationId;
            _day.LocationId = day.LocationId;
            dayRepository.Update(_day);
            dayRepository.Save();

            return RedirectToAction("Index", "Location", new { id = oldLocationId });
        }

        public async Task<IActionResult> Delete(int id, bool removePings = false)
        {
            var day = await dayRepository.GetByIdAsync(id);
            var returnLocationId = day.LocationId;
            var pings = new List<Ping>();

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
                dayRepository.Save();

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

            dayRepository.Delete(day.Id);
            dayRepository.Save();

            return RedirectToAction("Index", "Location", new { id = returnLocationId });
        }
    }
}
