using BoardGameNight.Domain.Enums;

namespace BoardGameNight.Domain.Entities;

/// <summary>
/// Represents a food or drink item available at a game night.
/// Can be provided by the organizer or brought by participants (potluck).
/// </summary>
public class FoodItem
{
    public int Id { get; private set; }
    
    public string Name { get; private set; } = string.Empty;
    
    public string? Description { get; private set; }
    
    /// <summary>
    /// Dietary options this food item satisfies.
    /// </summary>
    public DietaryPreference DietaryOptions { get; private set; }
    
    public int GameNightId { get; private set; }
    
    public GameNight GameNight { get; private set; } = null!;
    
    /// <summary>
    /// The person bringing this item (for potluck events).
    /// Null if provided by organizer.
    /// </summary>
    public int? BroughtByPersonId { get; private set; }
    
    public Person? BroughtByPerson { get; private set; }

    // For EF Core
    private FoodItem() { }

    public FoodItem(
        string name,
        GameNight gameNight,
        DietaryPreference dietaryOptions,
        Person? broughtByPerson = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Naam van het eten/drinken is verplicht.", nameof(name));

        Name = name;
        GameNight = gameNight ?? throw new ArgumentNullException(nameof(gameNight));
        GameNightId = gameNight.Id;
        DietaryOptions = dietaryOptions;
        BroughtByPerson = broughtByPerson;
        BroughtByPersonId = broughtByPerson?.Id;
        Description = description;
    }

    /// <summary>
    /// Checks if this food item is suitable for a person with given dietary preferences.
    /// </summary>
    public bool IsSuitableFor(DietaryPreference personPreferences)
    {
        if (personPreferences == DietaryPreference.None)
            return true;

        return (DietaryOptions & personPreferences) == personPreferences;
    }

    /// <summary>
    /// Updates the food item details.
    /// </summary>
    public void Update(string name, string? description, DietaryPreference dietaryOptions)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Naam van het eten/drinken is verplicht.", nameof(name));

        Name = name;
        Description = description;
        DietaryOptions = dietaryOptions;
    }
}
