using System.ComponentModel.DataAnnotations;

namespace UserSyncAPI_Tomcat.Models
{
    public class LoginRequest
    {
        [Required, StringLength(100)]
        public string SourceSystem { get; set; } // nvarchar(100)
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}