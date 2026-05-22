using Microsoft.AspNetCore.Identity;

namespace untitled1.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
