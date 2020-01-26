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
        private readonly ITagRepository tagRepository;

        public RideController(
            IRideRepository rideRepository,
            ITagRepository tagRepository
        )
        {
            this.rideRepository = rideRepository;
            this.tagRepository = tagRepository;
        }

        public async Task<IActionResult> Index(int id)
        {
            var ride = await rideRepository.GetByIdFull(id);
            if (!(ride is Ride))
            {
                return RedirectToAction("Index", "Home");
            }

            return View(ride);
        }

        [HttpPost]
        public async Task<IActionResult> SaveTags(Ride ride)
        {
            string tagLabels = Request.Form["tags"];
            await rideRepository.SetTags(ride.Id, tagLabels);

            return RedirectToAction("Index", "Ride", new { id = ride.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Ride ride)
        {
            var deletePings = Request.Form["deletePings"];
            ride = await rideRepository.GetByIdFull(ride.Id);

            // Delegate the deleting to the day controller.
            return RedirectToAction("Delete", "Day", new { id = ride.Day.Id, removePings = deletePings == "1" });
        }

    }
}
