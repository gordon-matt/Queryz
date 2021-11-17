using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Queryz.Data.Domain
{
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole()
        {
        }

        public ApplicationRole(string roleName)
            : base(roleName)
        {
        }

        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}