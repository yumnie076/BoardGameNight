using BoardGameNight.Application.DTOs;
using BoardGameNight.Application.Interfaces;
using BoardGameNight.Domain.Entities;
using BoardGameNight.Domain.Enums;
using BoardGameNight.Domain.Exceptions;
using BoardGameNight.Domain.Interfaces;

namespace BoardGameNight.Application.Services;

/// <summary>
/// Application service for board game operations.
/// </summary>
public class BoardGameService : IBoardGameService
{
    private readonly IBoardGameRepository _boardGameRepository;

    public BoardGameService(IBoardGameRepository boardGameRepository)
    {
        _boardGameRepository = boardGameRepository;
    }

    public async Task<BoardGameDto?> GetByIdAsync(int id)
    {
        var boardGame = await _boardGameRepository.GetByIdAsync(id);
        return boardGame == null ? null : MapToDto(boardGame);
    }

    public async Task<IEnumerable<BoardGameDto>> GetAllAsync()
    {
        var boardGames = await _boardGameRepository.GetAllAsync();
        return boardGames.Select(MapToDto);
    }

    public async Task<IEnumerable<BoardGameDto>> GetByGenreAsync(GameGenre genre)
    {
        var boardGames = await _boardGameRepository.GetByGenreAsync(genre);
        return boardGames.Select(MapToDto);
    }

    public async Task<IEnumerable<BoardGameDto>> GetByTypeAsync(GameType gameType)
    {
        var boardGames = await _boardGameRepository.GetByTypeAsync(gameType);
        return boardGames.Select(MapToDto);
    }

    public async Task<IEnumerable<BoardGameDto>> GetFamilyFriendlyAsync()
    {
        var boardGames = await _boardGameRepository.GetFamilyFriendlyAsync();
        return boardGames.Select(MapToDto);
    }

    public async Task<IEnumerable<BoardGameDto>> SearchAsync(string searchTerm)
    {
        var boardGames = await _boardGameRepository.SearchByNameAsync(searchTerm);
        return boardGames.Select(MapToDto);
    }

    public async Task<BoardGameDto> CreateAsync(CreateBoardGameDto dto)
    {
        var boardGame = new BoardGame(
            dto.Name,
            dto.Description,
            dto.Genre,
            dto.GameType,
            dto.IsAdultOnly,
            dto.MinPlayers,
            dto.MaxPlayers,
            dto.EstimatedDurationMinutes,
            dto.PhotoUrl);

        await _boardGameRepository.AddAsync(boardGame);
        return MapToDto(boardGame);
    }

    public async Task<BoardGameDto> UpdateAsync(int id, CreateBoardGameDto dto)
    {
        var boardGame = await _boardGameRepository.GetByIdAsync(id)
            ?? throw new DomainValidationException("Bordspel niet gevonden.");

        boardGame.Update(
            dto.Name,
            dto.Description,
            dto.Genre,
            dto.GameType,
            dto.IsAdultOnly,
            dto.MinPlayers,
            dto.MaxPlayers,
            dto.EstimatedDurationMinutes,
            dto.PhotoUrl);

        await _boardGameRepository.UpdateAsync(boardGame);
        return MapToDto(boardGame);
    }

    public async Task DeleteAsync(int id)
    {
        var boardGame = await _boardGameRepository.GetByIdAsync(id)
            ?? throw new DomainValidationException("Bordspel niet gevonden.");

        await _boardGameRepository.DeleteAsync(boardGame);
    }

    private static BoardGameDto MapToDto(BoardGame boardGame)
    {
        return new BoardGameDto
        {
            Id = boardGame.Id,
            Name = boardGame.Name,
            Description = boardGame.Description,
            Genre = boardGame.Genre,
            GameType = boardGame.GameType,
            IsAdultOnly = boardGame.IsAdultOnly,
            PhotoUrl = boardGame.PhotoUrl,
            MinPlayers = boardGame.MinPlayers,
            MaxPlayers = boardGame.MaxPlayers,
            EstimatedDurationMinutes = boardGame.EstimatedDurationMinutes,
            PlayerCountDisplay = boardGame.PlayerCountDisplay,
            DurationDisplay = boardGame.DurationDisplay
        };
    }
}
