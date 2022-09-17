﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using HomeBrewery.Application.Common.Exceptions;
using HomeBrewery.Application.Common.Settings;
using HomeBrewery.Application.Services.Auth.Models;
using HomeBrewery.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HomeBrewery.Application.Services.Auth;

public class AuthService : IAuthService
{
    private readonly JwtBearerTokenSettings _jwtSettings;
    private readonly IMapper _mapper;
    private readonly UserManager<HBUser> _userManager;

    public AuthService(
        IMapper mapper,
        UserManager<HBUser> userManager,
        IOptionsSnapshot<JwtBearerTokenSettings> jwtSettings)
    {
        _mapper = mapper;
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<LoginOutputModel> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new NotFoundException(nameof(HBUser), email);
        }

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            throw new Exception("Incorrect password");
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var userRole in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
        }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

        var token = new JwtSecurityToken(
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            expires: DateTime.Now.AddSeconds(_jwtSettings.ExpiryTimeInSeconds),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new LoginOutputModel
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = token.ValidTo
        };
    }

    public async Task<int> RegisterAsync(UserRegisterModel registerModel)
    {
        var userExists = await _userManager.FindByEmailAsync(registerModel.Email);
        if (userExists != null)
        {
            throw new Exception($"User ({registerModel.Email}) has already been registered.");
        }

        var user = _mapper.Map<HBUser>(registerModel);

        var result = await _userManager.CreateAsync(user, registerModel.Password);
        if (!result.Succeeded)
        {
            throw new DbUpdateException("DataBase conflict.",
                new Exception(string.Join("\n", result.Errors)));
        }

        return user.Id;
    }
}