using BoardGameNight.Domain.Enums;
using BoardGameNight.Domain.Exceptions;

namespace BoardGameNight.Domain.Entities;

/// <summary>
/// Represents a person who can organize or participate in board game nights.
/// A person can be both an organizer and a player.
/// </summary>
public class Person
{
    public int Id { get; private set; }
    
    public string Name { get; private set; } = string.Empty;
    
    public string Email { get; private set; } = string.Empty;
    
    public Gender Gender { get; private set; }
    
    public DateTime DateOfBirth { get; private set; }
    
    // Address
    public string Street { get; private set; } = string.Empty;
    
    public string HouseNumber { get; private set; } = string.Empty;
    
    public string City { get; private set; } = string.Empty;
    
    public DietaryPreference DietaryPreferences { get; private set; }
    
    // Navigation properties
    public ICollection<GameNight> OrganizedGameNights { get; private set; } = new List<GameNight>();
    
    public ICollection<GameNightParticipation> Participations { get; private set; } = new List<GameNightParticipation>();
    
    // Identity link
    public string? IdentityUserId { get; private set; }

    // For EF Core
    private Person() { }

    public Person(
        string name,
        string email,
        Gender gender,
        DateTime dateOfBirth,
        string street,
        string houseNumber,
        string city,
        DietaryPreference dietaryPreferences = DietaryPreference.None)
    {
        ValidateDateOfBirth(dateOfBirth);
        ValidateMinimumAge(dateOfBirth, 16, "Je moet minimaal 16 jaar oud zijn om een account aan te maken.");
        
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Gender = gender;
        DateOfBirth = dateOfBirth;
        Street = street ?? throw new ArgumentNullException(nameof(street));
        HouseNumber = houseNumber ?? throw new ArgumentNullException(nameof(houseNumber));
        City = city ?? throw new ArgumentNullException(nameof(city));
        DietaryPreferences = dietaryPreferences;
    }

    /// <summary>
    /// Calculates the current age of the person.
    /// </summary>
    public int Age
    {
        get
        {
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > today.AddYears(-age))
                age--;
            return age;
        }
    }

    /// <summary>
    /// Checks if the person is 18 years or older.
    /// </summary>
    public bool IsAdult => Age >= 18;

    /// <summary>
    /// Checks if the person can organize game nights (must be 18+).
    /// </summary>
    public bool CanOrganize => IsAdult;

    /// <summary>
    /// Links this person to an Identity user account.
    /// </summary>
    public void LinkToIdentityUser(string identityUserId)
    {
        IdentityUserId = identityUserId ?? throw new ArgumentNullException(nameof(identityUserId));
    }

    /// <summary>
    /// Updates the dietary preferences of the person.
    /// </summary>
    public void UpdateDietaryPreferences(DietaryPreference preferences)
    {
        DietaryPreferences = preferences;
    }

    /// <summary>
    /// Updates the person's profile information.
    /// </summary>
    public void UpdateProfile(string name, string street, string houseNumber, string city)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Street = street ?? throw new ArgumentNullException(nameof(street));
        HouseNumber = houseNumber ?? throw new ArgumentNullException(nameof(houseNumber));
        City = city ?? throw new ArgumentNullException(nameof(city));
    }

    /// <summary>
    /// Gets the full address as a formatted string.
    /// </summary>
    public string FullAddress => $"{Street} {HouseNumber}, {City}";

    /// <summary>
    /// Checks if this person's dietary needs are met by the given food options.
    /// </summary>
    public bool AreDietaryNeedsMet(DietaryPreference availableOptions)
    {
        // If person has no preferences, they're always satisfied
        if (DietaryPreferences == DietaryPreference.None)
            return true;

        // Check if all person's preferences are available
        return (DietaryPreferences & availableOptions) == DietaryPreferences;
    }

    private static void ValidateDateOfBirth(DateTime dateOfBirth)
    {
        if (dateOfBirth > DateTime.Today)
        {
            throw new DomainValidationException("Geboortedatum mag niet in de toekomst liggen.");
        }
    }

    private static void ValidateMinimumAge(DateTime dateOfBirth, int minimumAge, string errorMessage)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age))
            age--;

        if (age < minimumAge)
        {
            throw new DomainValidationException(errorMessage);
        }
    }
}
