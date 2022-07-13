using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using UserAuthMinimalApi.Models;
using UserAuthMinimalApi.Repositories;
using UserAuthMinimalApi.Services;
using UserAuthMinimalApi.Settings;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<TokenService>();

var secretKey = ApiSettings.GenerateSecretByte();

builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("manager", policy => policy.RequireRole("manager"));
    options.AddPolicy("operator", policy => policy.RequireRole("operator"));
});

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/login", (User userModel, TokenService service) =>
{
    var user = UserRepository.Find(userModel.Username, userModel.Password);

    if (user is null)
        return Results.NotFound(new { message = "Invalid username or password" });

    var token = service.GenerateToken(user);

    user.Password = string.Empty;

    return Results.Ok(new { user = user, token = token });
});

app.MapGet("/operator", (ClaimsPrincipal user) =>
{
    Results.Ok(new { message = $"Authenticated as { user?.Identity?.Name }" });
}).RequireAuthorization("Operator");

app.MapGet("/manager", (ClaimsPrincipal user) =>
{
    Results.Ok(new { message = $"Authenticated as { user?.Identity?.Name }" });
}).RequireAuthorization("Manager");

app.Run();
