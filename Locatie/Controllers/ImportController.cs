using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Locatie.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Locatie.Controllers
{
    [Authorize]
    public class ImportController : Controller
    {
        public IActionResult Index()
        {
            return View(new ImportModel());
        }

        [HttpPost]
        public async Task<IActionResult> Upload(ImportModel importModel)
        {
            if (importModel.WayPoints != null && importModel.WayPoints.Length > 0)
            {
                var waypointsFile = Path.GetTempFileName();

                using var stream = System.IO.File.Create(waypointsFile);
                await importModel.WayPoints.CopyToAsync(stream);

                BackgroundJob.Enqueue<Jobs.Import>(x => x.ImportWaypoints(waypointsFile));
            }

            if (importModel.Track != null && importModel.Track.Length > 0)
            {
                var trackFile = Path.GetTempFileName();

                using var stream = System.IO.File.Create(trackFile);
                await importModel.Track.CopyToAsync(stream);

                BackgroundJob.Enqueue<Jobs.Import>(x => x.ImportTrack(trackFile));
            }

            return RedirectToAction("Index");
        }

        public IActionResult Reset()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Reset(string date)
        {
            if (string.IsNullOrEmpty(date) ||
                !(DateTime.TryParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _date)))
            {
                _date = DateTime.Now.Date;
            }

            BackgroundJob.Enqueue<Jobs.Import>(x => x.Reset(_date, DateTime.Now));

            return RedirectToAction("Index", "Home");
        }

        public IActionResult DisableProcessing()
        {
            RecurringJob.RemoveIfExists("ProcessPings");
            return RedirectToAction("Index", "Home");
        }
        
    }
}
