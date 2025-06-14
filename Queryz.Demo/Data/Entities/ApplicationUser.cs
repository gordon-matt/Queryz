﻿using Microsoft.AspNetCore.Identity;

namespace Queryz.Demo.Data.Entities;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public virtual ICollection<ApplicationRole> Roles { get; set; }
}