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
    public class RideController : Controller
    {
        private readonly IRideRepository rideRepository;

        public RideController(
            IRideRepository rideRepository
        )
        {
            this.rideRepository = rideRepository;
        }

        public async Task<IActionResult> Index(int id)
        {
            var ride = await rideRepository.GetByIdWithPings(id);
            if (!(ride is Ride))
            {
                return RedirectToAction("Index", "Home");
            }

            return View(ride);
        }
    }
}
