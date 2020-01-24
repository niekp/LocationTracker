using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Locatie.Models
{
    [Table("tag")]
    public class Tag
    {
        public int Id { get; set; }

        [Column("tag")]
        public string Label { get; set; }

        public ICollection<RideTag> Rides { get; set; }
    }
}
