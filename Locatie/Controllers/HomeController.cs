using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Locatie.Models;
using Locatie.Repositories.Core;

namespace Locatie.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPingRepository pingRepository;
        private readonly IDayRepository dayRepository;

        public HomeController(
            ILogger<HomeController> logger,
            IPingRepository pingRepository,
            IDayRepository dayRepository
        )
        {
            _logger = logger;
            this.pingRepository = pingRepository;
            this.dayRepository = dayRepository;
        }

        public async Task<IActionResult> Index()
        {
            var date = DateTime.Now.Date.AddDays(-7);
            var days = await dayRepository.GetDays(date, date.AddDays(1).AddMinutes(-1));
            ViewBag.Date = date;
            return View(days);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
