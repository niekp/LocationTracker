using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public int DistanceInMeters { get; set; }

        public ICollection<RideTag> Tags { get; set; }
    }
}
