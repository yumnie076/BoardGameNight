using BoardGameNight.Domain.Entities;
using BoardGameNight.Domain.Enums;

namespace BoardGameNight.Domain.Interfaces;

/// <summary>
/// Repository interface for BoardGame entities.
/// </summary>
public interface IBoardGameRepository : IRepository<BoardGame>
{
    /// <summary>
    /// Gets all board games of a specific genre.
    /// </summary>
    Task<IEnumerable<BoardGame>> GetByGenreAsync(GameGenre genre);
    
    /// <summary>
    /// Gets all board games of a specific type.
    /// </summary>
    Task<IEnumerable<BoardGame>> GetByTypeAsync(GameType gameType);
    
    /// <summary>
    /// Gets all non-adult board games.
    /// </summary>
    Task<IEnumerable<BoardGame>> GetFamilyFriendlyAsync();
    
    /// <summary>
    /// Searches board games by name.
    /// </summary>
    Task<IEnumerable<BoardGame>> SearchByNameAsync(string searchTerm);
}
