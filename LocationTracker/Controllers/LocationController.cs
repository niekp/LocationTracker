using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LocationTracker.Models;
using LocationTracker.Repositories.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using LocationTracker.Utils;

namespace LocationTracker.Controllers
{
    [Authorize]
    public class LocationController : Controller
    {
        private readonly ILocationRepository locationRepository;
        private readonly IPingRepository pingRepository;
        private readonly IDayRepository dayRepository;

        public LocationController(
            IPingRepository pingRepository,
            IDayRepository dayRepository,
            ILocationRepository locationRepository
        )
        {
            this.locationRepository = locationRepository;
            this.pingRepository = pingRepository;
            this.dayRepository = dayRepository;
        }

        public async Task<IActionResult> Index()
        {
            var locations = await locationRepository.GetAllASync();
            return View(locations.OrderBy(l => l.Label).ToList());
        }
            
        public async Task<IActionResult> History(int id, string from = "", string to = "")
        {
            var location = await locationRepository.GetByIdWithHistory(id);

            var _from = DateFunctions.GetDate(from, DateTime.Now.Date.AddDays(-7));
            var _to = DateFunctions.GetDate(to, DateTime.Now.Date);

            _to = _to.AddHours(23.99);

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

            return RedirectToAction("History", "Location", new { id = location.Id });
        }

        public async Task<IActionResult> Merge(int id)
        {
            var location = await locationRepository.GetByIdAsync(id);
            var locations = (await locationRepository.GetAllASync()).OrderBy(l => l.Label);
            ViewBag.LocationOptions = locations.Select(x => new SelectListItem { Text = x.Label, Value = x.Id.ToString() }).ToList();
            return View(location);
        }

        [HttpPost]
        public async Task<IActionResult> Merge(Location location)
        {
            var newLocationId = int.Parse(Request.Form["locationId"]);

            await Task.WhenAll(
                pingRepository.MergeLocation(location.Id, newLocationId),
                dayRepository.MergeLocation(location.Id, newLocationId)
            );

            locationRepository.Delete(location.Id);
            await locationRepository.SaveAsync();

            return RedirectToAction("History", "Location", new { id = newLocationId });

        }

        public async Task<IActionResult> Delete(int Id)
        {
            var location = await locationRepository.GetByIdWithHistory(Id);
            if (location is Location)
            {
                locationRepository.Delete(Id);
                locationRepository.Save();
            }

            return RedirectToAction("Index", "Location");
        }

        public async Task<IActionResult> New()
        {
            var location = new Location();
            var lastPing = await pingRepository.GetLastPing();
            if(lastPing is Ping)
            {
                location.Latitude = lastPing.Latitude;
                location.Longitude = lastPing.Longitude;
            }

            return View(location);
        }

        [HttpPost]
        public async Task<IActionResult> New(Location location)
        {
            if (ModelState.IsValid)
            {
                locationRepository.Insert(location);
                await locationRepository.SaveAsync();
            }
            else
            {
                return View(location);
            }

            return RedirectToAction("History", "Location", new { id = location.Id });
        }

    }
}
