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
                graph.DrawEllipse(pen, (int)((coord.Longitude * multiplier) - x_min), (int)((coord.Latitude * multiplier) - y_min), 1, 1);
            }
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

            // Add all pings as light grey
            DrawCoordinates(graph, new Pen(Brushes.LightGray), coordinates, multiplier, x_min, y_min);

            // Redraw the past 6 months with white
            coordinates = await pingRepository
                .GetUniqueLocationsBetweenDates(
                DateTime.Now.AddMonths(-6),
                DateTime.Now
            );

            DrawCoordinates(graph, new Pen(Brushes.White), coordinates, multiplier, x_min, y_min);

            // Redraw the past week with cyan
            coordinates = await pingRepository
                .GetUniqueLocationsBetweenDates(
                DateTime.Now.AddDays(-7),
                DateTime.Now
            );

            DrawCoordinates(graph, new Pen(Brushes.Cyan), coordinates, multiplier, x_min, y_min);

            // Save the image
            image.Save(Constants.BASE_MAP_PNG, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
