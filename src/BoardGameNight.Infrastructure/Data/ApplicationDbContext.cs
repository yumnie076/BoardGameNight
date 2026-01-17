using BoardGameNight.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoardGameNight.Infrastructure.Data;

/// <summary>
/// Main database context for the application data.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Person> Persons => Set<Person>();
    public DbSet<BoardGame> BoardGames => Set<BoardGame>();
    public DbSet<GameNight> GameNights => Set<GameNight>();
    public DbSet<GameNightBoardGame> GameNightBoardGames => Set<GameNightBoardGame>();
    public DbSet<GameNightParticipation> GameNightParticipations => Set<GameNightParticipation>();
    public DbSet<FoodItem> FoodItems => Set<FoodItem>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Person configuration
        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(p => p.Email).IsUnique();
            entity.Property(p => p.Street).IsRequired().HasMaxLength(200);
            entity.Property(p => p.HouseNumber).IsRequired().HasMaxLength(20);
            entity.Property(p => p.City).IsRequired().HasMaxLength(100);
            entity.Property(p => p.IdentityUserId).HasMaxLength(450);
            entity.HasIndex(p => p.IdentityUserId).IsUnique().HasFilter("[IdentityUserId] IS NOT NULL");
        });

        // BoardGame configuration
        modelBuilder.Entity<BoardGame>(entity =>
        {
            entity.HasKey(bg => bg.Id);
            entity.Property(bg => bg.Name).IsRequired().HasMaxLength(200);
            entity.Property(bg => bg.Description).HasMaxLength(2000);
            entity.Property(bg => bg.PhotoUrl).HasMaxLength(500);
        });

        // GameNight configuration
        modelBuilder.Entity<GameNight>(entity =>
        {
            entity.HasKey(gn => gn.Id);
            entity.Property(gn => gn.Street).IsRequired().HasMaxLength(200);
            entity.Property(gn => gn.HouseNumber).IsRequired().HasMaxLength(20);
            entity.Property(gn => gn.City).IsRequired().HasMaxLength(100);

            entity.HasOne(gn => gn.Organizer)
                .WithMany(p => p.OrganizedGameNights)
                .HasForeignKey(gn => gn.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // GameNightBoardGame (many-to-many junction table) configuration
        modelBuilder.Entity<GameNightBoardGame>(entity =>
        {
            entity.HasKey(gnbg => new { gnbg.GameNightId, gnbg.BoardGameId });

            entity.HasOne(gnbg => gnbg.GameNight)
                .WithMany(gn => gn.BoardGames)
                .HasForeignKey(gnbg => gnbg.GameNightId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gnbg => gnbg.BoardGame)
                .WithMany(bg => bg.GameNights)
                .HasForeignKey(gnbg => gnbg.BoardGameId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // GameNightParticipation configuration
        modelBuilder.Entity<GameNightParticipation>(entity =>
        {
            entity.HasKey(gnp => gnp.Id);
            entity.HasIndex(gnp => new { gnp.GameNightId, gnp.PersonId }).IsUnique();

            entity.HasOne(gnp => gnp.GameNight)
                .WithMany(gn => gn.Participants)
                .HasForeignKey(gnp => gnp.GameNightId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gnp => gnp.Person)
                .WithMany(p => p.Participations)
                .HasForeignKey(gnp => gnp.PersonId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // FoodItem configuration
        modelBuilder.Entity<FoodItem>(entity =>
        {
            entity.HasKey(fi => fi.Id);
            entity.Property(fi => fi.Name).IsRequired().HasMaxLength(200);
            entity.Property(fi => fi.Description).HasMaxLength(500);

            entity.HasOne(fi => fi.GameNight)
                .WithMany(gn => gn.FoodItems)
                .HasForeignKey(fi => fi.GameNightId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(fi => fi.BroughtByPerson)
                .WithMany()
                .HasForeignKey(fi => fi.BroughtByPersonId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Review configuration
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.ReviewText).HasMaxLength(2000);
            entity.HasIndex(r => new { r.GameNightId, r.ReviewerId }).IsUnique();

            entity.HasOne(r => r.GameNight)
                .WithMany(gn => gn.Reviews)
                .HasForeignKey(r => r.GameNightId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Reviewer)
                .WithMany()
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
