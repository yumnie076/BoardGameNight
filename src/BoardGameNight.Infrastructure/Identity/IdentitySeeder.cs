using BoardGameNight.Domain.Entities;
using BoardGameNight.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BoardGameNight.Infrastructure.Identity;

/// <summary>
/// Seeds Identity users and links them to Person entities.
/// Creates test accounts for the docent and demo purposes.
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        ApplicationDbContext appContext)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Create roles
        await CreateRolesAsync(roleManager);

        // Create test users and link to persons
        await CreateTestUsersAsync(userManager, appContext);
    }

    private static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames = { "Admin", "Organizer", "Player" };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private static async Task CreateTestUsersAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext appContext)
    {
        // Test users met hun wachtwoorden
        var testUsers = new List<(string Email, string Password, string Role, string PersonEmail)>
        {
            // Docent account (admin)
            ("docent@avans.nl", "Docent123!", "Admin", null!),
            
            // Test users gekoppeld aan seeded persons
            ("jan@example.com", "Test123!", "Organizer", "jan@example.com"),
            ("maria@example.com", "Test123!", "Organizer", "maria@example.com"),
            ("ahmed@example.com", "Test123!", "Player", "ahmed@example.com"),
            ("sophie@example.com", "Test123!", "Player", "sophie@example.com"),
            ("thomas@example.com", "Test123!", "Player", "thomas@example.com"),
            ("lisa@example.com", "Test123!", "Player", "lisa@example.com"),
        };

        foreach (var (email, password, role, personEmail) in testUsers)
        {
            // Check if user already exists
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser != null)
                continue;

            // Find linked person (if any)
            Person? linkedPerson = null;
            if (!string.IsNullOrEmpty(personEmail))
            {
                linkedPerson = await appContext.Persons
                    .FirstOrDefaultAsync(p => p.Email == personEmail);
            }

            // Create Identity user
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                PersonId = linkedPerson?.Id
            };

            var result = await userManager.CreateAsync(user, password);
            
            if (result.Succeeded)
            {
                // Add role
                await userManager.AddToRoleAsync(user, role);

                // Link person to identity user
                if (linkedPerson != null)
                {
                    linkedPerson.LinkToIdentityUser(user.Id);
                    appContext.Persons.Update(linkedPerson);
                }
            }
            else
            {
                // Log errors (in production, use proper logging)
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                Console.WriteLine($"Failed to create user {email}: {errors}");
            }
        }

        await appContext.SaveChangesAsync();
    }

    /// <summary>
    /// Creates the docent admin user if not exists.
    /// Call this separately if you need to ensure admin exists.
    /// </summary>
    public static async Task EnsureDocentUserAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        const string docentEmail = "docent@avans.nl";
        const string docentPassword = "Docent123!";

        // Ensure Admin role exists
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Check if docent exists
        var docent = await userManager.FindByEmailAsync(docentEmail);
        if (docent == null)
        {
            docent = new ApplicationUser
            {
                UserName = docentEmail,
                Email = docentEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(docent, docentPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(docent, "Admin");
            }
        }
    }
}
