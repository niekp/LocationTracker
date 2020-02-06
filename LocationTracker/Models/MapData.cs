using System;
using System.Collections.Generic;

namespace LocationTracker.Models
{
    public class MapData
    {
        public List<Ride> Rides = new List<Ride>();
        public List<Location> Locations = new List<Location>();
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
