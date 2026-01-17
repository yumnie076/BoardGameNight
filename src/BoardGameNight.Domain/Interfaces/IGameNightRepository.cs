using BoardGameNight.Domain.Entities;

namespace BoardGameNight.Domain.Interfaces;

/// <summary>
/// Repository interface for GameNight entities.
/// </summary>
public interface IGameNightRepository : IRepository<GameNight>
{
    /// <summary>
    /// Gets all upcoming game nights (in the future).
    /// </summary>
    Task<IEnumerable<GameNight>> GetUpcomingAsync();
    
    /// <summary>
    /// Gets all game nights organized by a specific person.
    /// </summary>
    Task<IEnumerable<GameNight>> GetByOrganizerAsync(int organizerId);
    
    /// <summary>
    /// Gets all game nights a person is participating in.
    /// </summary>
    Task<IEnumerable<GameNight>> GetByParticipantAsync(int personId);
    
    /// <summary>
    /// Gets a game night with all related data (games, participants, food, reviews).
    /// </summary>
    Task<GameNight?> GetWithDetailsAsync(int gameNightId);
    
    /// <summary>
    /// Checks if a person already has a registration for a game night on the given date.
    /// </summary>
    Task<bool> HasRegistrationOnDateAsync(int personId, DateTime date);
    
    /// <summary>
    /// Gets all past game nights for an organizer.
    /// </summary>
    Task<IEnumerable<GameNight>> GetPastByOrganizerAsync(int organizerId);
}
