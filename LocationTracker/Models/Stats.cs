using System;
using System.Collections.Generic;

namespace LocationTracker.Models
{
    public class Stats
    {
        public Stats()
        {
            SummaryRow = false;
            Walking = new Movement();
            Running = new Movement();
            Biking = new Movement();
            All = new Movement();
        }

        public bool SummaryRow { get; set; }
        public string Label { get; set; }
        public double Speed { get; set; }
        public Dictionary<string, int> Locations { get; set; }
        public Movement Walking { get; set; }
        public Movement Running { get; set; }
        public Movement Biking { get; set; }
        public Movement All { get; set; }
        public double TotalHours { get; set; }
    }

    public class Movement
    {
        public Movement()
        {
            Minutes = 0;
            Meters = 0;
        }

        public int Minutes { get; set; }
        public int Meters { get; set; }
    }
}
