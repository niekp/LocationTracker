using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Locatie.Models;
using Locatie.Repositories.Core;
using Microsoft.AspNetCore.Mvc;

namespace Locatie.Controllers
{
    public class LocationController : Controller
    {
        private readonly ILocationRepository locationRepository;

        public LocationController(
            ILocationRepository locationRepository
        )
        {
            this.locationRepository = locationRepository;
        }
            
        public async Task<IActionResult> Index(int id, string from = "", string to = "")
        {
            var location = await locationRepository.GetByIdWithHistory(id);

            if (string.IsNullOrEmpty(from) ||
                !(DateTime.TryParseExact(from, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _from)))
            {
                _from = DateTime.Now.Date.AddDays(-7);
            }

            if (string.IsNullOrEmpty(to) ||
                !(DateTime.TryParseExact(to, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _to)))
            {
                _to = DateTime.Now.Date;
            }

            ViewBag.From = _from;
            ViewBag.To = _to;

            if (!(location is Location))
            {
                return RedirectToAction("Index", "Home");
            }

            return View(location);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var location = await locationRepository.GetByIdAsync(id);
            return View(location);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Location location)
        {
            var _location = await locationRepository.GetByIdAsync(location.Id);
            _location.Label = location.Label;
            _location.Latitude = location.Latitude;
            _location.Longitude = location.Longitude;
            locationRepository.Update(_location);
            locationRepository.Save();

            return RedirectToAction("Index", "Location", new { id = location.Id });
        }
    }
}
