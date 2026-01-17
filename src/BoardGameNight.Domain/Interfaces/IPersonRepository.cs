using BoardGameNight.Domain.Entities;

namespace BoardGameNight.Domain.Interfaces;

/// <summary>
/// Repository interface for Person entities.
/// </summary>
public interface IPersonRepository : IRepository<Person>
{
    Task<Person?> GetByEmailAsync(string email);
    
    Task<Person?> GetByIdentityUserIdAsync(string identityUserId);
    
    Task<Person?> GetWithOrganizedGameNightsAsync(int personId);
    
    Task<Person?> GetWithParticipationsAsync(int personId);
    
    /// <summary>
    /// Gets the total number of shows (attended) for a person.
    /// </summary>
    Task<int> GetShowCountAsync(int personId);
    
    /// <summary>
    /// Gets the total number of no-shows for a person.
    /// </summary>
    Task<int> GetNoShowCountAsync(int personId);
    
    /// <summary>
    /// Gets the average rating of all game nights organized by this person.
    /// </summary>
    Task<double?> GetAverageOrganizerRatingAsync(int personId);
    
    /// <summary>
    /// Gets the total number of game nights organized by this person.
    /// </summary>
    Task<int> GetOrganizedGameNightCountAsync(int personId);
}
