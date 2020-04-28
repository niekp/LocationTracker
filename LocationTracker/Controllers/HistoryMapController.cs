using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using LocationTracker.Models;
using LocationTracker.Repositories.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LocationTracker.Controllers
{
    public class HistoryMapController : Controller
    {
        private readonly IWebHostEnvironment env;
        private readonly IPingRepository pingRepository;

        public HistoryMapController(IWebHostEnvironment env, IPingRepository pingRepository)
        {
            this.env = env;
            this.pingRepository = pingRepository;
        }

        public IActionResult Start()
        {
            BackgroundJob.Enqueue<Jobs.HistoryMap>(x => x.DrawMap());
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Map()
        {
            var webRoot = env.WebRootPath;
            var file = System.IO.Path.Combine(webRoot, "map.png");
            return File(file, "image/png", "Kaart");
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
