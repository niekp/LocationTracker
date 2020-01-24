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

        public HomeController(
            ILogger<HomeController> logger,
            IPingRepository pingRepository
        )
        {
            _logger = logger;
            this.pingRepository = pingRepository;
        }

        public IActionResult Index()
        {
            ViewBag.Ping = pingRepository.GetLastPing();
            return View();
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
