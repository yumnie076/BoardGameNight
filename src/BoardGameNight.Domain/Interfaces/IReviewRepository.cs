using BoardGameNight.Domain.Entities;

namespace BoardGameNight.Domain.Interfaces;

/// <summary>
/// Repository interface for Review entities.
/// </summary>
public interface IReviewRepository : IRepository<Review>
{
    /// <summary>
    /// Gets all reviews for a specific game night.
    /// </summary>
    Task<IEnumerable<Review>> GetByGameNightAsync(int gameNightId);
    
    /// <summary>
    /// Gets all reviews for game nights organized by a specific person.
    /// </summary>
    Task<IEnumerable<Review>> GetByOrganizerAsync(int organizerId);
    
    /// <summary>
    /// Gets a review by reviewer and game night.
    /// </summary>
    Task<Review?> GetByReviewerAndGameNightAsync(int reviewerId, int gameNightId);
}
