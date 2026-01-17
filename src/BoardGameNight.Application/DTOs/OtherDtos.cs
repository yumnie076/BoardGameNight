using BoardGameNight.Domain.Enums;

namespace BoardGameNight.Application.DTOs;

/// <summary>
/// DTO for displaying board game information.
/// </summary>
public class BoardGameDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GameGenre Genre { get; set; }
    public GameType GameType { get; set; }
    public bool IsAdultOnly { get; set; }
    public string? PhotoUrl { get; set; }
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public string PlayerCountDisplay { get; set; } = string.Empty;
    public string DurationDisplay { get; set; } = string.Empty;
}

/// <summary>
/// DTO for creating a new board game.
/// </summary>
public class CreateBoardGameDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GameGenre Genre { get; set; }
    public GameType GameType { get; set; }
    public bool IsAdultOnly { get; set; }
    public string? PhotoUrl { get; set; }
    public int MinPlayers { get; set; } = 2;
    public int MaxPlayers { get; set; } = 6;
    public int EstimatedDurationMinutes { get; set; } = 60;
}

/// <summary>
/// DTO for displaying person information.
/// </summary>
public class PersonDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int Age { get; set; }
    public bool IsAdult { get; set; }
    public string Street { get; set; } = string.Empty;
    public string HouseNumber { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public DietaryPreference DietaryPreferences { get; set; }
}

/// <summary>
/// DTO for creating/registering a new person.
/// </summary>
public class CreatePersonDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Street { get; set; } = string.Empty;
    public string HouseNumber { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public DietaryPreference DietaryPreferences { get; set; }
}

/// <summary>
/// DTO for participant information in a game night.
/// </summary>
public class ParticipantDto
{
    public int PersonId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public bool? DidAttend { get; set; }
    public int TotalShows { get; set; }
    public int TotalNoShows { get; set; }
}

/// <summary>
/// DTO for food item information.
/// </summary>
public class FoodItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DietaryPreference DietaryOptions { get; set; }
    public string? BroughtByPersonName { get; set; }
}

/// <summary>
/// DTO for creating a food item (potluck).
/// </summary>
public class CreateFoodItemDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DietaryPreference DietaryOptions { get; set; }
    public int GameNightId { get; set; }
}

/// <summary>
/// DTO for review information.
/// </summary>
public class ReviewDto
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string ReviewText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
}

/// <summary>
/// DTO for creating a review.
/// </summary>
public class CreateReviewDto
{
    public int GameNightId { get; set; }
    public int Rating { get; set; }
    public string ReviewText { get; set; } = string.Empty;
}

/// <summary>
/// DTO for recording attendance.
/// </summary>
public class RecordAttendanceDto
{
    public int GameNightId { get; set; }
    public int PersonId { get; set; }
    public bool DidAttend { get; set; }
}
