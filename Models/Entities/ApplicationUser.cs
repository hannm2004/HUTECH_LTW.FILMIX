using System;
using Microsoft.AspNetCore.Identity;

namespace untitled1.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime? PremiumStartDate { get; set; }
        public DateTime? PremiumEndDate { get; set; }
        public bool IsPremium => PremiumEndDate.HasValue && PremiumEndDate.Value > DateTime.Now;
    }
}
