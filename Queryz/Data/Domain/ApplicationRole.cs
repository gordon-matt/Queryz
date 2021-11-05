using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Queryz.Data.Domain
{
    public class ApplicationRole : IdentityRole
    {
        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}