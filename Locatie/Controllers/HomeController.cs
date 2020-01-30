using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Locatie.Models;
using Locatie.Repositories.Core;
using System.Globalization;
using Hangfire;
using Microsoft.AspNetCore.Authorization;

namespace Locatie.Controllers
{
    [Authorize]
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

        public async Task<IActionResult> Index(string date = "")
        {
            if (string.IsNullOrEmpty(date) ||
                !(DateTime.TryParseExact(date, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _date)))
            {
                _date = DateTime.Now.Date;
            }   
            
            var days = await dayRepository.GetDays(_date, _date.AddDays(1).AddMinutes(-1));
            ViewBag.Date = _date;
            return View(days);
        }

        public string Process()
        {
            BackgroundJob.Enqueue<Jobs.ProcessPings>(x => x.Process());
            return "ok";
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
