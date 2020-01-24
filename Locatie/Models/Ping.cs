using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Locatie.Models
{
    [Table("ping")]
    public class Ping
    {
        [Key]
        public int Id { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public DateTime Tijd { get; set; }

    }
}
