using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Locatie.Models
{
    [Table("locatie")]
    public class Location
    {
        public int ID { get; set; }

        public string Label { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }
    }
}
