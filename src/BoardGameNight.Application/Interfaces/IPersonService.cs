using BoardGameNight.Application.DTOs;
using BoardGameNight.Domain.Enums;

namespace BoardGameNight.Application.Interfaces;

/// <summary>
/// Application service interface for person operations.
/// </summary>
public interface IPersonService
{
    Task<PersonDto?> GetByIdAsync(int id);
    Task<PersonDto?> GetByEmailAsync(string email);
    Task<PersonDto?> GetByIdentityUserIdAsync(string identityUserId);
    Task<IEnumerable<PersonDto>> GetAllAsync();
    
    Task<PersonDto> CreateAsync(CreatePersonDto dto);
    Task<PersonDto> UpdateAsync(int id, CreatePersonDto dto);
    Task LinkToIdentityUserAsync(int personId, string identityUserId);
    Task UpdateDietaryPreferencesAsync(int personId, DietaryPreference preferences);
    
    // Statistics for US_08 and US_09
    Task<int> GetShowCountAsync(int personId);
    Task<int> GetNoShowCountAsync(int personId);
    Task<double?> GetAverageOrganizerRatingAsync(int personId);
    Task<int> GetOrganizedGameNightCountAsync(int personId);
}
