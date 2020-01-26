using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Locatie.Models
{
    [Table("rit")]
    public class Ride
    {
        public int Id { get; set; }

        [Column("tijd_van")]
        public DateTime TimeFrom { get; set; }

        [Column("tijd_tot")]
        public DateTime TimeTo { get; set; }

        [Column("afstand_meter")]
        public int? DistanceInMeters { get; set; }

        public ICollection<RideTag> Tags { get; set; }

        public ICollection<Ping> Pings { get; set; }

        public Day Day { get; set; }

        private double? _distance = null;
        private double GetDistanceInMeters()
        {
            if (_distance == null)
            {
                var u = new Utils.Utility();
                _distance = u.GetDistanceInMeters(Pings.OrderBy(p => p.Time).ToList());
            }

            return _distance ?? 0;
        }

        public double GetDistanceInKilometers()
        {
            return Math.Round(GetDistanceInMeters() / 1000, 2);
        }

        public double GetSpeed()
        {
            var km = GetDistanceInMeters() / 1000;
            var hours = (TimeTo - TimeFrom).TotalHours;

            return Math.Round(km / hours, 2);
        }
    }
}
