﻿using Microsoft.AspNetCore.Identity;

namespace HomeBrewery.Domain.Entities;

public class HBRole : IdentityRole<int>
{
    public HBRole()
    {
        UserRoles = new HashSet<HBUserRole>();
    }

    public virtual ICollection<HBUserRole> UserRoles { get; set; }
}