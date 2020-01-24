using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Locatie.Models
{
    [Table("dag_ping")]
    public class DayPing
    {
        [Key, Column("dag_id")]
        public int DayId { get; set; }

        [Key, Column("ping_id")]
        public int PingId { get; set; }

        public Day Day { get; set; }
        public Ping Ping { get; set; }
    }
}
