using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using LocationTracker.Jobs;
using LocationTracker.Models;
using LocationTracker.Repositories.Core;
using LocationTracker.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LocationTracker.Controllers
{
    [Authorize]
    public class HistoryMapController : Controller
    {
        HistoryMap historyMapJob = null;
        public HistoryMapController(IPingRepository pingRepository)
        {
            historyMapJob = new HistoryMap(pingRepository);
        }

        public async Task<IActionResult> Start()
        {
            //await historyMapJob.DrawMap();
            BackgroundJob.Enqueue<HistoryMap>(x => x.DrawMap());
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

        public IActionResult Rotate()
        {
            using (var imageRotate = new Bitmap(Constants.BASE_MAP_PNG))
            {
                imageRotate.RotateFlip(RotateFlipType.Rotate270FlipNone);
                imageRotate.Save(Constants.BASE_MAP_PNG, System.Drawing.Imaging.ImageFormat.Png);
            }

            return RedirectToAction("Index");
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
    }
}
