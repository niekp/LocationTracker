using System;
using System.Collections.Generic;
using System.Linq;
using Locatie.Models;
using Geodesy;

namespace Locatie.Utils
{
    public class Utility
    {
        public Utility()
        {
        }

        public double GetDistanceInMeters(List<Ping> pings)
	    {
			Ping previous = null;
			double totalDistance = 0;

            foreach (Ping ping in pings)
            {
                if (ping is Ping && previous is Ping)
                {
                    totalDistance += GetDistanceInMeters(previous, ping);
                }
                previous = ping;
            }

            return totalDistance;
	    }

        public double GetDistanceInMeters(Ping ping1, Ping ping2)
        {
            return new GeodeticCalculator(Ellipsoid.WGS84)
                .CalculateGeodeticMeasurement(
                    GetGlobalPosition(ping1),
                    GetGlobalPosition(ping2)
                ).PointToPointDistance;
        }

        private GlobalPosition GetGlobalPosition(Ping ping)
        {
            return new GlobalPosition(new GlobalCoordinates(ping.Latitude, ping.Longitude));
        }
    }
}
