using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LocationTracker.Models;
using LocationTracker.Repositories.Core;
using System.Globalization;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using LocationTracker.Utils;

namespace LocationTracker.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPingRepository pingRepository;
        private readonly IDayRepository dayRepository;
        private readonly INoteRepository noteRepository;

        public HomeController(
            ILogger<HomeController> logger,
            IPingRepository pingRepository,
            IDayRepository dayRepository,
            INoteRepository noteRepository
        )
        {
            _logger = logger;
            this.pingRepository = pingRepository;
            this.dayRepository = dayRepository;
            this.noteRepository = noteRepository;
        }


        public async Task<IActionResult> Index(string date = "")
        {
            var _date = DateFunctions.GetDate(date);
            var days = await dayRepository.GetDays(_date, _date.AddDays(1).AddMinutes(-1));
            ViewBag.Date = _date;
            ViewBag.Note = await noteRepository.GetNote(_date);

            return View(days);
        }

        public IActionResult Process()
        {
            BackgroundJob.Enqueue<Jobs.ProcessPings>(x => x.Process());
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SaveNote(string date, string note)
        {
            var _date = DateFunctions.GetDate(date);
            await noteRepository.SaveNote(_date, note);

            return RedirectToAction("Index", new { date });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
