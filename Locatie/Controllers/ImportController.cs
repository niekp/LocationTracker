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
    }
}
