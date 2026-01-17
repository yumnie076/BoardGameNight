using BoardGameNight.Domain.Entities;
using BoardGameNight.Domain.Enums;

namespace BoardGameNight.Domain.Interfaces;

/// <summary>
/// Repository interface for FoodItem entities.
/// </summary>
public interface IFoodItemRepository : IRepository<FoodItem>
{
    /// <summary>
    /// Gets all food items for a specific game night.
    /// </summary>
    Task<IEnumerable<FoodItem>> GetByGameNightAsync(int gameNightId);
    
    /// <summary>
    /// Gets food items brought by a specific person for a game night.
    /// </summary>
    Task<IEnumerable<FoodItem>> GetByPersonAndGameNightAsync(int personId, int gameNightId);
    
    /// <summary>
    /// Gets all food items suitable for specific dietary preferences.
    /// </summary>
    Task<IEnumerable<FoodItem>> GetSuitableForDietAsync(int gameNightId, DietaryPreference preferences);
}
