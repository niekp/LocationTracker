using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationTracker.Models
{
    [Table("rit_tag")]
    public class RideTag
    {
        [Key, Column("rit_id")]
        public int RideId { get; set; }

        [Key, Column("tag_id")]
        public int TagId { get; set; }

        public Ride Ride { get; set; }
        public Tag Tag { get; set; }

    }
}
