using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocationTracker.Models;
using LocationTracker.Repositories.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LocationTracker.Controllers
{
    [Authorize]
    public class RideController : Controller
    {
        private readonly IRideRepository rideRepository;
        private readonly ITagRepository tagRepository;
        private readonly IDayRepository dayRepository;

        public RideController(
            IRideRepository rideRepository,
            ITagRepository tagRepository,
            IDayRepository dayRepository
        )
        {
            this.rideRepository = rideRepository;
            this.tagRepository = tagRepository;
            this.dayRepository = dayRepository;
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

            if (ride is Ride)
            {
                await dayRepository.DeleteDay(ride.Day.Id, deletePings == "1");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> SaveRide(Ride _ride)
        {
            var ride = await rideRepository.GetByIdFull(_ride.Id);
            ride.AccuracyCutoff = _ride.AccuracyCutoff;
            ride.ResetDistance();
            rideRepository.Update(ride);
            await rideRepository.SaveAsync();

            return RedirectToAction("Index", "Ride", new { id = ride.Id });
        }
        

    }
}
