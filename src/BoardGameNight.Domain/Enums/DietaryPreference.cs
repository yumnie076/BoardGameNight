namespace BoardGameNight.Domain.Enums;

/// <summary>
/// Represents dietary preferences and allergies.
/// Used for both persons and food/drinks at events.
/// </summary>
[Flags]
public enum DietaryPreference
{
    None = 0,
    LactoseFree = 1,        // Lactosevrij
    NutFree = 2,            // Notenvrij
    Vegetarian = 4,         // Vegetarisch
    AlcoholFree = 8,        // Alcoholvrij
    GlutenFree = 16,        // Glutenvrij
    Vegan = 32,             // Veganistisch
    Halal = 64,             // Halal
    Kosher = 128            // Koosjer
}
