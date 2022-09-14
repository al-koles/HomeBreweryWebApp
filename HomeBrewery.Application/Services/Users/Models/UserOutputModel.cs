﻿using HomeBrewery.Application.Common.Mappings;
using HomeBrewery.Domain;
using HomeBrewery.Domain.Data;

namespace HomeBrewery.Application.Services.Users.Models;

public class UserOutputModel : IMapWith<HBUser>
{
    public UserOutputModel()
    {
        Roles = new List<Role>();
    }
    
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public List<Role> Roles { get; set; }
}