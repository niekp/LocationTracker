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

        public string GetTimeDisplay()
        {
            if (TimeFrom.Date == DateTime.Now.Date)
                return TimeFrom.Date.ToString("HH:mm");
            if (TimeFrom.Year != DateTime.Now.Year)
                return TimeTo.ToString("dd-MM-yyyy HH:mm");
            else
                return TimeTo.ToString("HH:mm");
        }

        public string GetTimeSpendDisplay()
        {
            var timeSpend = (TimeTo - TimeFrom);
            return String.Format("{0:00}:{1:00}",
                Convert.ToInt32(
                    timeSpend.Hours + (Math.Floor(timeSpend.TotalDays) * 24)
                ),
                timeSpend.Minutes
            );
        }

        public double GetHours()
        {
            return (TimeTo - TimeFrom).TotalHours;
        }

    }
}
