using System;
using System.Collections.Generic;

namespace LocationTracker.Models
{
    public class Run
    {
        public Run()
        {
            Laps = new List<Ride>();
            DistanceInMeters = 0;
        }

        public DateTime TimeFrom { get; set; }
        public DateTime TimeTo { get; set; }
        public List<Ride> Laps { get; set; }
        public int DistanceInMeters { get; set; }
        public double TotalMinutes { get; set; }
        public double MinutesMoving { get; set; }
    }
}
