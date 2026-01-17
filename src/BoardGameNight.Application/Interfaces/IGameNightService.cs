using BoardGameNight.Application.DTOs;

namespace BoardGameNight.Application.Interfaces;

/// <summary>
/// Application service interface for game night operations.
/// </summary>
public interface IGameNightService
{
    // Query operations
    Task<IEnumerable<GameNightListDto>> GetAllUpcomingAsync();
    Task<IEnumerable<GameNightListDto>> GetByOrganizerAsync(int organizerId);
    Task<IEnumerable<GameNightListDto>> GetByParticipantAsync(int personId);
    Task<GameNightDto?> GetByIdAsync(int id);
    Task<GameNightDto?> GetWithDetailsAsync(int id);

    // Command operations
    Task<GameNightDto> CreateAsync(int organizerId, CreateGameNightDto dto);
    Task<GameNightDto> UpdateAsync(int organizerId, UpdateGameNightDto dto);
    Task DeleteAsync(int organizerId, int gameNightId);

    // Participation
    Task<(bool Success, string Message, IEnumerable<string> Warnings)> RegisterParticipantAsync(
        int personId, int gameNightId);
    Task UnregisterParticipantAsync(int personId, int gameNightId);
    Task<bool> CanPersonJoinAsync(int personId, int gameNightId);

    // Board games
    Task AddBoardGameAsync(int gameNightId, int boardGameId);
    Task RemoveBoardGameAsync(int gameNightId, int boardGameId);

    // Food items (potluck)
    Task<FoodItemDto> AddFoodItemAsync(int personId, CreateFoodItemDto dto);
    Task<IEnumerable<FoodItemDto>> GetFoodItemsAsync(int gameNightId);

    // Reviews
    Task<ReviewDto> AddReviewAsync(int reviewerId, CreateReviewDto dto);
    Task<IEnumerable<ReviewDto>> GetReviewsAsync(int gameNightId);

    // Attendance (US_09)
    Task RecordAttendanceAsync(int organizerId, RecordAttendanceDto dto);
}
