using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BoardGameNight.Infrastructure.Identity;

/// <summary>
/// Separate database context for Identity (authentication/authorization).
/// Uses a separate database for security.
/// </summary>
public class ApplicationIdentityDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationIdentityDbContext(DbContextOptions<ApplicationIdentityDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Customize Identity tables if needed
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.PersonId).IsRequired(false);
        });
    }
}

/// <summary>
/// Extended Identity user with link to Person entity.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Foreign key to the Person entity in the main database.
    /// </summary>
    public int? PersonId { get; set; }
}
