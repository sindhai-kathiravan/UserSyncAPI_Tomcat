using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UserSyncAPI_Tomcat.Models
{
    public class DeleteUserRequest
    {
        [Required, StringLength(100)]
        public string SourceSystem { get; set; } // nvarchar(100)
        [Required]
        public int UserId { get; set; }
        [Required]
        public int ModifiedByUserId { get; set; }

    }
}