using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;
using Locatie.Models;
using Locatie.Repositories.Core;

namespace Locatie.Jobs
{
    public class Import
    {
        public readonly ILocationRepository locationRepository;
        public readonly IPingRepository pingRepository;

        public Import(
            ILocationRepository locationRepository,
            IPingRepository pingRepository
        )
        {
            this.locationRepository = locationRepository;
            this.pingRepository = pingRepository;
        }

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
        }

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
