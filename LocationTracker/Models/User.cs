using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocationTracker.Models
{
    [Table("user")]
    public class User
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        [Column("api_token")]
        public string ApiToken { get; set; }
    }
}
