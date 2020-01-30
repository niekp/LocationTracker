using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Locatie.Models
{
    [Table("user_session")]
    public class UserSession
    {
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        public User User { get; set; }

        [Column("valid_till")]
        public DateTime ValidTill { get; set; }

        public string Token { get; set; }
    }
}
