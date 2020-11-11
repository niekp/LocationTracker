using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using LocationTracker.Models;
using LocationTracker.Repositories.Core;
using LocationTracker.Utils;
using Microsoft.AspNetCore.Hosting;

namespace LocationTracker.Jobs
{
    public class HistoryMap
    {
        private readonly IPingRepository pingRepository;

        public HistoryMap(IPingRepository pingRepository)
        {
            this.pingRepository = pingRepository;
        }

        private void DrawCoordinates(Graphics graph, Pen pen, List<Coordinate> coordinates, int multiplier, double x_min, double y_min)
        {
            foreach (var coord in coordinates)
            {
                var x = (int)((coord.Latitude * multiplier) - x_min);
                var y = (int)((coord.Longitude * multiplier) - y_min);
                graph.DrawEllipse(pen, x, y, 1, 1);
            }
        }

        [DisableConcurrentExecution(timeoutInSeconds: 60 * 30)]
        public async Task DrawMap()
        {
            var multiplier = 100;
            var padding = 50;
            var dateStart = DateTime.Now.AddMonths(-12);
            var bounds = await pingRepository.GetMinMax(dateStart, DateTime.Now);
            var x_min = (bounds.x_min * multiplier);
            var x_max = (bounds.x_max * multiplier);
            var y_min = (bounds.y_min * multiplier);
            var y_max = (bounds.y_max * multiplier);
            int width = Convert.ToInt32(Math.Ceiling(x_max - x_min));
            int height = Convert.ToInt32(Math.Ceiling(y_max - y_min));

            if (File.Exists(Constants.BASE_MAP_PNG))
            {
                File.Delete(Constants.BASE_MAP_PNG);
            }

            using Image image = new Bitmap(width + padding, height + padding);
            using Graphics graph = Graphics.FromImage(image);
            graph.Clear(Color.Black);

            // Add all pings as light grey
            DrawCoordinates(graph, new Pen(Brushes.DarkGray), await pingRepository
                .GetUniqueLocationsBetweenDates(
                dateStart,
                DateTime.Now
            ), multiplier, x_min, y_min);

            // Redraw the past 6 months with white
            DrawCoordinates(graph, new Pen(Brushes.White), await pingRepository
                .GetUniqueLocationsBetweenDates(
                DateTime.Now.AddMonths(-6),
                DateTime.Now
            ), multiplier, x_min, y_min);

            // Redraw the past week with cyan
            DrawCoordinates(graph, new Pen(Brushes.Cyan), await pingRepository
                .GetUniqueLocationsBetweenDates(
                DateTime.Now.AddDays(-7),
                DateTime.Now
            ), multiplier, x_min, y_min);

            // Save the map
            image.Save(Constants.BASE_MAP_PNG, System.Drawing.Imaging.ImageFormat.Png);
            /*
            // Reopen and rotate it. something weird is happening with the x, y, lat, long mapping.
            using (var imageRotate = new Bitmap(Constants.BASE_MAP_PNG))
            {
                imageRotate.RotateFlip(RotateFlipType.Rotate270FlipNone);
                imageRotate.Save("/tmp/location.net.map2.png", System.Drawing.Imaging.ImageFormat.Png);
            }
            */

        }
    }
}
