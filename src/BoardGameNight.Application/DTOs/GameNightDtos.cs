using BoardGameNight.Domain.Enums;

namespace BoardGameNight.Application.DTOs;

/// <summary>
/// DTO for creating a new game night.
/// </summary>
public class CreateGameNightDto
{
    public DateTime DateTime { get; set; }
    public string Street { get; set; } = string.Empty;
    public string HouseNumber { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int MaxPlayers { get; set; }
    public bool IsAdultOnly { get; set; }
    public bool IsPotluck { get; set; }
    public DietaryPreference AvailableDietaryOptions { get; set; }
    public List<int> BoardGameIds { get; set; } = new();
}

/// <summary>
/// DTO for updating a game night.
/// </summary>
public class UpdateGameNightDto
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public string Street { get; set; } = string.Empty;
    public string HouseNumber { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int MaxPlayers { get; set; }
    public bool IsAdultOnly { get; set; }
    public bool IsPotluck { get; set; }
    public DietaryPreference AvailableDietaryOptions { get; set; }
    public List<int> BoardGameIds { get; set; } = new();
}

/// <summary>
/// DTO for displaying game night information.
/// </summary>
public class GameNightDto
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public string Street { get; set; } = string.Empty;
    public string HouseNumber { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public int MaxPlayers { get; set; }
    public int CurrentPlayerCount { get; set; }
    public bool IsFull { get; set; }
    public bool IsAdultOnly { get; set; }
    public bool IsPotluck { get; set; }
    public bool HasPassed { get; set; }
    public DietaryPreference AvailableDietaryOptions { get; set; }
    
    // Organizer info
    public int OrganizerId { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public double? OrganizerAverageRating { get; set; }
    public int OrganizerGameNightCount { get; set; }
    
    // Related data
    public List<BoardGameDto> BoardGames { get; set; } = new();
    public List<ParticipantDto> Participants { get; set; } = new();
    public List<FoodItemDto> FoodItems { get; set; } = new();
    public List<ReviewDto> Reviews { get; set; } = new();
}

/// <summary>
/// DTO for game night list items.
/// </summary>
public class GameNightListDto
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public string City { get; set; } = string.Empty;
    public int MaxPlayers { get; set; }
    public int CurrentPlayerCount { get; set; }
    public bool IsFull { get; set; }
    public bool IsAdultOnly { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public int BoardGameCount { get; set; }
}
