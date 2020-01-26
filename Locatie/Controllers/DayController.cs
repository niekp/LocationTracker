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

        public async Task<IActionResult> Delete(int id)
        {
            var day = await dayRepository.GetByIdAsync(id);
            var returnLocationId = day.LocationId;
            await dayRepository.DeleteDay(day.Id, false);

            return RedirectToAction("Index", "Location", new { id = returnLocationId });
        }
    }
}
