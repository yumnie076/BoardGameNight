using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BoardGameNight.Infrastructure.Identity;
using BoardGameNight.Application.Interfaces;
using BoardGameNight.Domain.Enums;

namespace BoardGameNight.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IPersonService _personService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IPersonService personService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _personService = personService;
    }

    /// <summary>
    /// Login and receive JWT token
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized(new { error = "Ongeldige inloggegevens." });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            return Unauthorized(new { error = "Ongeldige inloggegevens." });

        var token = await GenerateJwtTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new LoginResponse
        {
            Token = token,
            Email = user.Email!,
            PersonId = user.PersonId,
            Roles = roles.ToList(),
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        });
    }

    /// <summary>
    /// Register new user account
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return BadRequest(new { error = "Email is al in gebruik." });

        try
        {
            // Create Person entity first
            var personDto = new Application.DTOs.CreatePersonDto
            {
                Name = request.Name,
                Email = request.Email,
                Gender = request.Gender,
                DateOfBirth = request.DateOfBirth,
                Street = request.Street,
                HouseNumber = request.HouseNumber,
                City = request.City,
                DietaryPreferences = request.DietaryPreferences
            };

            var person = await _personService.CreateAsync(personDto);

            // Create Identity user
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true,
                PersonId = person.Id
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                // Rollback person creation would be needed in production
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { error = errors });
            }

            // Add default role
            await _userManager.AddToRoleAsync(user, "Player");

            // Link person to identity user
            await _personService.LinkToIdentityUserAsync(person.Id, user.Id);

            return CreatedAtAction(nameof(Login), new RegisterResponse
            {
                PersonId = person.Id,
                Email = user.Email,
                Message = "Account succesvol aangemaakt. Je kunt nu inloggen."
            });
        }
        catch (Domain.Exceptions.DomainValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get current user info from token
    /// </summary>
    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ProducesResponseType(typeof(UserInfoResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);
        var person = user.PersonId.HasValue 
            ? await _personService.GetByIdAsync(user.PersonId.Value) 
            : null;

        return Ok(new UserInfoResponse
        {
            Email = user.Email!,
            PersonId = user.PersonId,
            PersonName = person?.Name,
            Roles = roles.ToList()
        });
    }

    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.PersonId.HasValue)
        {
            claims.Add(new Claim("PersonId", user.PersonId.Value.ToString()));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "DefaultSecretKeyForDevelopment12345!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(24);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "BoardGameNight",
            audience: _configuration["Jwt:Audience"] ?? "BoardGameNight",
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

#region Request/Response DTOs

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? PersonId { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime ExpiresAt { get; set; }
}

public class RegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Street { get; set; } = string.Empty;
    public string HouseNumber { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public DietaryPreference DietaryPreferences { get; set; }
}

public class RegisterResponse
{
    public int PersonId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class UserInfoResponse
{
    public string Email { get; set; } = string.Empty;
    public int? PersonId { get; set; }
    public string? PersonName { get; set; }
    public List<string> Roles { get; set; } = new();
}

#endregion
