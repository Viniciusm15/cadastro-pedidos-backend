using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Domain.Models.Entities
{
    public class ApplicationUser : IdentityUser, ISoftDeletable
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime? DeletedAt { get; set; }

        public virtual Client? Client { get; set; }
    }
}
