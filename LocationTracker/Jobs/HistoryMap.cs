using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using LocationTracker.Repositories.Core;
using Microsoft.AspNetCore.Hosting;

namespace LocationTracker.Jobs
{
    public class HistoryMap
    {
        private readonly IWebHostEnvironment env;
        private readonly IPingRepository pingRepository;

        public HistoryMap(IWebHostEnvironment env, IPingRepository pingRepository)
        {
            this.env = env;
            this.pingRepository = pingRepository;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 60 * 30)]
        public async Task DrawMap()
        {
            var coordinates = await pingRepository
                .GetUniqueLocationsBetweenDates(
                new DateTime(1900, 1, 1),
                DateTime.Now
            );

            var multiplier = 10000;
            var padding = 50;
            var y_min = (coordinates.Min(c => c.Latitude) * multiplier) - padding;
            var y_max = (coordinates.Max(c => c.Latitude) * multiplier) - padding;
            var x_min = (coordinates.Min(c => c.Longitude) * multiplier) + padding;
            var x_max = (coordinates.Max(c => c.Longitude) * multiplier) + padding;
            int width = Convert.ToInt32(Math.Ceiling(x_max - x_min));
            int height = Convert.ToInt32(Math.Ceiling(y_max - y_min));

            Image image = new Bitmap(width + (padding), height);
            Graphics graph = Graphics.FromImage(image);
            graph.Clear(Color.Black);

            Pen pen = new Pen(Brushes.White);

            foreach (var coord in coordinates)
            {
                graph.DrawEllipse(pen, (int)((coord.Longitude * multiplier) - x_min), (int)((coord.Latitude * multiplier) - y_min), 1, 1);
            }

            var webRoot = env.WebRootPath;
            var file = System.IO.Path.Combine(webRoot, "map.png");
            image.Save(file, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
