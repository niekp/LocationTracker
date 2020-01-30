using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Hangfire;
using Locatie.Data;
using Locatie.Models;
using Locatie.Repositories.Core;
using Locatie.Utils;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace Locatie.Jobs
{
    public class Import
    {
        public readonly ILocationRepository locationRepository;
        public readonly IPingRepository pingRepository;
        public readonly IDayRepository dayRepository;
        public readonly IRideRepository rideRepository;
        public readonly LocatieContext locatieContext;
        public readonly ICache cache;

        public Import(
            ILocationRepository locationRepository,
            IPingRepository pingRepository,
            IDayRepository dayRepository,
            IRideRepository rideRepository,
            LocatieContext locatieContext,
            ICache cache
        )
        {
            this.locationRepository = locationRepository;
            this.pingRepository = pingRepository;
            this.dayRepository = dayRepository;
            this.rideRepository = rideRepository;
            this.locatieContext = locatieContext;
            this.cache = cache;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 60 * 5)]
        public void ImportWaypoints(string file)
        {
            XmlDocument gpxDoc = new XmlDocument();
            gpxDoc.Load(file);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(gpxDoc.NameTable);
            nsmgr.AddNamespace("x", "http://www.topografix.com/GPX/1/1");
            XmlNodeList wps = gpxDoc.SelectNodes("//x:wpt", nsmgr);

            foreach (XmlNode wp in wps)
            {
                var latitude = double.Parse(wp.Attributes["lat"].Value);
                var longitude = double.Parse(wp.Attributes["lon"].Value);
                var name = wp["name"].InnerText;

                var location = new Location()
                {
                    Label = string.Format("Import WP{0} ({1})", name, DateTime.Now.ToShortDateString()),
                    Latitude = latitude,
                    Longitude = longitude
                };

                locationRepository.Insert(location);
            }

            locationRepository.SaveAsync();
            cache.ClearCache();
        }

        [DisableConcurrentExecution(timeoutInSeconds: 60 * 5)]
        public async Task ImportTrack(string file)
        {
            XmlDocument gpxDoc = new XmlDocument();
            gpxDoc.Load(file);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(gpxDoc.NameTable);
            nsmgr.AddNamespace("x", "http://www.topografix.com/GPX/1/1");
            XmlNodeList trksegments = gpxDoc.SelectNodes("//x:trkseg", nsmgr);

            var resetFrom = DateTime.Now;

            foreach (XmlNode trkseg in trksegments)
            {
                var pings = trkseg.ChildNodes;
                if (pings.Count <= 1)
                {
                    continue;
                }

                var firstPing = GetDateFromGpx(pings[0]["time"].InnerText);
                var lastPing = GetDateFromGpx(pings[pings.Count - 1]["time"].InnerText);
                if (firstPing < resetFrom)
                {
                    resetFrom = firstPing;
                }

                await pingRepository.DeleteBetweenDates(firstPing, lastPing);

                foreach (XmlNode pingData in pings)
                {
                    var ping = new Ping()
                    {
                        Latitude = double.Parse(pingData.Attributes["lat"].Value),
                        Longitude = double.Parse(pingData.Attributes["lon"].Value),
                        Accuracy = 5,
                        Time = GetDateFromGpx(pingData["time"].InnerText)
                    };

                    pingRepository.Insert(ping);
                }

                await pingRepository.SaveAsync();
            }
            cache.ClearCache();
            await Reset(resetFrom, DateTime.Now);

            BackgroundJob.Enqueue<ProcessPings>(x => x.Process());
        }

        public async Task Reset(DateTime from, DateTime to)
        {
            // Reset based on day
            var days = await dayRepository.GetDays(from, to);
            var pings = new List<Ping>();

            foreach (var day in days)
            {
                pings = await pingRepository.GetPings(day);
                await ResetPings(pings);

                if (day.RideId != null)
                {
                    var ride = await rideRepository.GetByIdFull((int)day.RideId);
                    await ResetPings(ride.Pings.ToList());

                    rideRepository.Delete(day.RideId);
                    await rideRepository.SaveAsync();
                }

                dayRepository.Delete(day.Id);
            }

            await dayRepository.SaveAsync();

            // Reset the remaining pings
            pings = await pingRepository.GetBetweenDates(from, to);
            await ResetPings(pings);
            cache.ClearCache();
        }

        private async Task ResetPings(List<Ping> pings)
        {
            if (pings.Count == 0)
            {
                return;
            }

            var timeoutOld = locatieContext.Database.GetCommandTimeout();
            locatieContext.Database.SetCommandTimeout(86400);
            var fromTime = pings.OrderBy(p => p.Time).FirstOrDefault().Time;

            await locatieContext.Database.ExecuteSqlCommandAsync(
                "UPDATE ping SET rit_id = null, locatie_id = null, verwerkt = 0 WHERE tijd >= @time AND FIND_IN_SET(id, @ids) != 0",
                new MySqlParameter("@time", fromTime),
                new MySqlParameter("@ids", string.Join(',', pings.Select(p => p.Id).ToList()))
            );

            await locatieContext.SaveChangesAsync();

            locatieContext.Database.SetCommandTimeout(timeoutOld);
        }

        private DateTime GetDateFromGpx(string date)
        {
            if (!DateTime.TryParseExact(date, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
            {
                throw new Exception("Couln't extract date");
            }

            return dt;
        }
    }
}
