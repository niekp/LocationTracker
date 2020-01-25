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

        public IActionResult Index()
        {
            return View();
        }

            
            
        public async Task<IActionResult> History(int id, string from = "", string to = "")
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

    }
}
