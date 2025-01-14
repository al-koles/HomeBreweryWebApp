﻿using HomeBrewery.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeBrewery.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<HomeBreweryDbContext>(bld => bld.UseSqlServer(connectionString));

        services.AddScoped<IHomeBreweryDbContext>(provider => 
            provider.GetRequiredService<HomeBreweryDbContext>());

        return services;
    }
}