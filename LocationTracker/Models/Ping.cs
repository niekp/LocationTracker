﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationTracker.Models
{
    [Table("ping")]
    public class Ping
    {
        public int Id { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public double Accuracy { get; set; }

        [Column("Tijd")]
        public DateTime Time { get; set; }

        [Column("verwerkt")]
        public Int16 Processed { get; set; }

        [Column("locatie_id")]
        public int? LocationId { get; set; }

        public Location Location { get; set; }

        [Column("rit_id")]
        public int? RideId { get; set; }

        public Ride Ride { get; set; }

        [Column("dag_id")]
        public int? DayId { get; set; }

        public Day Day { get; set; }

    }
}
