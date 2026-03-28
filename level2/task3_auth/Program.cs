using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var jwtKey = "CodvedaInternship2026SuperSecretKey!@#";

builder.Services.AddControllers();
builder.Services.AddSingleton<UserService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization", Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer", BearerFormat = "JWT", In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {{
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Reference = new Microsoft.OpenApi.Models.OpenApiReference
            { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
        }, Array.Empty<string>()
    }});
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidIssuer = "codveda-auth",
            ValidateAudience = true, ValidAudience = "codveda-app",
            ValidateLifetime = true, ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();


public record RegisterRequest(string Username, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string AccessToken, string Username, string Role, DateTime ExpiresAt);

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "User";
}

public class UserService
{
    private static readonly List<User> _users = new()
    {
        new User { Id = 1, Username = "admin", Email = "admin@codveda.com", PasswordHash = Hash("Admin123!"), Role = "Admin" },
        new User { Id = 2, Username = "gildas", Email = "niygildas@gmail.com", PasswordHash = Hash("Gildas123!"), Role = "User" }
    };
    private static int _nextId = 3;

    public static string Hash(string password)
    {
        var salt = new byte[16];
        RandomNumberGenerator.Fill(salt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, 100_000, System.Security.Cryptography.HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string hash)
    {
        var parts = hash.Split(':');
        var salt = Convert.FromBase64String(parts[0]);
        var expected = Convert.FromBase64String(parts[1]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, 100_000, System.Security.Cryptography.HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    public (bool ok, string? error, AuthResponse? response) Register(RegisterRequest req, string secret)
    {
        if (_users.Any(u => u.Email == req.Email)) return (false, "Email already exists.", null);
        if (req.Password.Length < 8) return (false, "Password too short.", null);
        var user = new User { Id = _nextId++, Username = req.Username, Email = req.Email, PasswordHash = Hash(req.Password) };
        _users.Add(user);
        return (true, null, GenerateToken(user, secret));
    }

    public (bool ok, string? error, AuthResponse? response) Login(LoginRequest req, string secret)
    {
        var user = _users.FirstOrDefault(u => u.Email == req.Email);
        if (user == null || !Verify(req.Password, user.PasswordHash)) return (false, "Invalid credentials.", null);
        return (true, null, GenerateToken(user, secret));
    }

    private AuthResponse GenerateToken(User user, string secret)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("userId", user.Id.ToString())
        };
        var expires = DateTime.UtcNow.AddHours(1);
        var token = new JwtSecurityToken("codveda-auth", "codveda-app", claims, expires: expires, signingCredentials: creds);
        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), user.Username, user.Role, expires);
    }

    public IEnumerable<object> GetAll() => _users.Select(u => new { u.Id, u.Username, u.Email, u.Role });
}

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserService _svc;
    private readonly string _secret;
    public AuthController(UserService svc, IConfiguration config)
    {
        _svc = svc;
        _secret = config["Jwt:SecretKey"] ?? "CodvedaInternship2026SuperSecretKey!@#";
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest req)
    {
        var (ok, error, response) = _svc.Register(req, _secret);
        return ok ? Ok(response) : BadRequest(new { error });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        var (ok, error, response) = _svc.Login(req, _secret);
        return ok ? Ok(response) : Unauthorized(new { error });
    }

    [HttpGet("me"), Authorize]
    public IActionResult Me() => Ok(new
    {
        Username = User.FindFirst(ClaimTypes.Name)?.Value,
        Email = User.FindFirst(ClaimTypes.Email)?.Value,
        Role = User.FindFirst(ClaimTypes.Role)?.Value
    });
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResourcesController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult Public() => Ok(new { message = "Accessible to any logged-in user.", user = User.Identity?.Name });

    [HttpGet("admin"), Authorize(Roles = "Admin")]
    public IActionResult Admin() => Ok(new { message = "Admin only area." });

    [HttpGet("manager"), Authorize(Roles = "Admin,Manager")]
    public IActionResult Manager() => Ok(new { message = "Admin and Manager area." });
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly UserService _svc;
    public UsersController(UserService svc) => _svc = svc;

    [HttpGet]
    public IActionResult GetAll() => Ok(_svc.GetAll());
}

