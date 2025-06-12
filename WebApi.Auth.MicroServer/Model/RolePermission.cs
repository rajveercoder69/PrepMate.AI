using System.ComponentModel.DataAnnotations;

namespace WebApi.Auth.MicroServer.Model
{
    public class RolePermission
    {
        [Key]
        public int Id { get; set; }
        public string Role { get; set; }               // Requested role
        public string? IssuedBy { get; set; }           // User/system issuing the role (e.g., "System" or "AdminUser")
        public string? AskedByUserName { get; set; }    // Who asked for the role
        public string Email { get; set; }              // Email of the requester

        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Rejected
        public DateTime? UpdatedDateTime { get; set; }
        public DateTime RequestedDateTime { get; set; } = DateTime.UtcNow;
        
    }
}
