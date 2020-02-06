using System;
using System.Collections.Generic;
using System.Linq;
using LocationTracker.Models;
using Geodesy;

namespace LocationTracker.Utils
{
    public class Utility
    {
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

        public double GetDistanceInMeters(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            return new GeodeticCalculator(Ellipsoid.WGS84)
                .CalculateGeodeticMeasurement(
                    GetGlobalPosition(latitude1, longitude1),
                    GetGlobalPosition(latitude2, longitude2)
                ).PointToPointDistance;
        }

        public Coordinates GetCoordinates(List<Ping> pings, double accuracy = 50)
        {
            var coords = new Coordinates() { Latitude = 0, Longitude = 0 };
            var filteredPings = pings.Where(p => p.Accuracy <= accuracy).ToList();

            if (filteredPings.Count == 0 && accuracy < 200)
            {
                return GetCoordinates(pings, accuracy);
            }
            else if (filteredPings.Count == 0)
            {
                return coords;
            }

            foreach (var ping in filteredPings)
            {
                coords.Latitude += ping.Latitude;
                coords.Longitude += ping.Longitude;
            }

            coords.Latitude /= filteredPings.Count;
            coords.Longitude /= filteredPings.Count;

            return coords;
        }

        public double GetSpeed(DateTime from, DateTime to, double meters)
        {
            var km = meters / 1000;
            var hours = (to - from).TotalHours;

            return Math.Round(km / hours, 2);
        }

        private GlobalPosition GetGlobalPosition(Ping ping)
        {
            return GetGlobalPosition(ping.Latitude, ping.Longitude);
        }

        private GlobalPosition GetGlobalPosition(double latitude, double longitude)
        {
            return new GlobalPosition(new GlobalCoordinates(latitude, longitude));
        }

    }
}
