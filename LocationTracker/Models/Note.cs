using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationTracker.Models
{
    [Table("note")]
    public class Note
    {
        public Note() { }

        public Note(Day day, string note)
        {
            Date = day.TimeFrom.Date;
            Text = note;
        }

        public Note(DateTime date, string note)
        {
            Date = date.Date;
            Text = note;
        }

        [Key]
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string Text { get; set; }
    }
}
