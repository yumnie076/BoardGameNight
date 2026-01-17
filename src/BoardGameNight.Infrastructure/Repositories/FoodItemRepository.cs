using BoardGameNight.Domain.Entities;
using BoardGameNight.Domain.Enums;
using BoardGameNight.Domain.Interfaces;
using BoardGameNight.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoardGameNight.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for FoodItem entities.
/// </summary>
public class FoodItemRepository : Repository<FoodItem>, IFoodItemRepository
{
    public FoodItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<FoodItem>> GetByGameNightAsync(int gameNightId)
    {
        return await _dbSet
            .Include(fi => fi.BroughtByPerson)
            .Where(fi => fi.GameNightId == gameNightId)
            .OrderBy(fi => fi.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<FoodItem>> GetByPersonAndGameNightAsync(int personId, int gameNightId)
    {
        return await _dbSet
            .Where(fi => fi.GameNightId == gameNightId && fi.BroughtByPersonId == personId)
            .ToListAsync();
    }

    public async Task<IEnumerable<FoodItem>> GetSuitableForDietAsync(int gameNightId, DietaryPreference preferences)
    {
        return await _dbSet
            .Where(fi => fi.GameNightId == gameNightId && 
                        (fi.DietaryOptions & preferences) == preferences)
            .ToListAsync();
    }
}
