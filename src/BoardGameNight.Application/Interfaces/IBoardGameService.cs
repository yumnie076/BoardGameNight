using BoardGameNight.Application.DTOs;
using BoardGameNight.Domain.Enums;

namespace BoardGameNight.Application.Interfaces;

/// <summary>
/// Application service interface for board game operations.
/// </summary>
public interface IBoardGameService
{
    Task<BoardGameDto?> GetByIdAsync(int id);
    Task<IEnumerable<BoardGameDto>> GetAllAsync();
    Task<IEnumerable<BoardGameDto>> GetByGenreAsync(GameGenre genre);
    Task<IEnumerable<BoardGameDto>> GetByTypeAsync(GameType gameType);
    Task<IEnumerable<BoardGameDto>> GetFamilyFriendlyAsync();
    Task<IEnumerable<BoardGameDto>> SearchAsync(string searchTerm);
    
    Task<BoardGameDto> CreateAsync(CreateBoardGameDto dto);
    Task<BoardGameDto> UpdateAsync(int id, CreateBoardGameDto dto);
    Task DeleteAsync(int id);
}
