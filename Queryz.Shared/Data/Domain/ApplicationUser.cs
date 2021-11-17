using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Queryz.Data.Domain
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<ApplicationRole> Roles { get; set; }
    }
}