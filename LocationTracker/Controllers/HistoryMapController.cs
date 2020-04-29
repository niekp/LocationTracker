using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using LocationTracker.Models;
using LocationTracker.Repositories.Core;
using LocationTracker.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

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
            var folderParts = Constants.BASE_MAP_PNG.Split("/");
            var folder = string.Join("/", folderParts.Take(folderParts.Count() - 1));
            var file = folderParts.Last();

            PhysicalFileProvider provider = new PhysicalFileProvider(folder);
            IFileInfo fileInfo = provider.GetFileInfo(file);

            var readStream = fileInfo.CreateReadStream();
            var mimeType = "image/png";

            provider.Dispose();

            return File(readStream, mimeType, "kaart.png");
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
    }
}
