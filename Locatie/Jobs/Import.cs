using System;
using System.Threading.Tasks;
using System.Xml;
using Locatie.Models;
using Locatie.Repositories.Core;

namespace Locatie.Jobs
{
    public class Import
    {
        public readonly ILocationRepository locationRepository;

        public Import(
            ILocationRepository locationRepository
        )
        {
            this.locationRepository = locationRepository;
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
                    Label = String.Format("Import WP{0} ({1})", name, DateTime.Now.ToShortDateString()),
                    Latitude = latitude,
                    Longitude = longitude
                };

                locationRepository.Insert(location);
            }

            locationRepository.SaveAsync();
        }
    }
}
