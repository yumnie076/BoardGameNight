using BoardGameNight.Domain.Entities;
using BoardGameNight.Domain.Enums;
using BoardGameNight.Domain.Interfaces;
using BoardGameNight.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoardGameNight.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for BoardGame entities.
/// </summary>
public class BoardGameRepository : Repository<BoardGame>, IBoardGameRepository
{
    public BoardGameRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<BoardGame>> GetByGenreAsync(GameGenre genre)
    {
        return await _dbSet
            .Where(bg => bg.Genre == genre)
            .OrderBy(bg => bg.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<BoardGame>> GetByTypeAsync(GameType gameType)
    {
        return await _dbSet
            .Where(bg => bg.GameType == gameType)
            .OrderBy(bg => bg.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<BoardGame>> GetFamilyFriendlyAsync()
    {
        return await _dbSet
            .Where(bg => !bg.IsAdultOnly)
            .OrderBy(bg => bg.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<BoardGame>> SearchByNameAsync(string searchTerm)
    {
        return await _dbSet
            .Where(bg => bg.Name.Contains(searchTerm) || bg.Description.Contains(searchTerm))
            .OrderBy(bg => bg.Name)
            .ToListAsync();
    }
}
