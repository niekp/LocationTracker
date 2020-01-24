using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Locatie.Models
{
    [Table("dag")]
    public class Day
    {
        public int Id { get; set; }

        [Column("tijd_van")]
        public DateTime TimeFrom { get; set; }

        [Column("tijd_tot")]
        public DateTime TimeTo { get; set; }

        [Column("locatie_id")]
        public int? LocationId { get; set; }

        public Location Location { get; set; }

        [Column("rit_id")]
        public int? RideId { get; set; }

        public Ride Ride { get; set; }

        public ICollection<DayPing> Pings { get; set; }
    }
}
