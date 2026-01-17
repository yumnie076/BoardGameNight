using BoardGameNight.Domain.Enums;

namespace BoardGameNight.Domain.Entities;

/// <summary>
/// Represents a board game that can be played at game nights.
/// </summary>
public class BoardGame
{
    public int Id { get; private set; }
    
    public string Name { get; private set; } = string.Empty;
    
    public string Description { get; private set; } = string.Empty;
    
    public GameGenre Genre { get; private set; }
    
    public GameType GameType { get; private set; }
    
    public bool IsAdultOnly { get; private set; }
    
    public string? PhotoUrl { get; private set; }
    
    public int MinPlayers { get; private set; }
    
    public int MaxPlayers { get; private set; }
    
    public int EstimatedDurationMinutes { get; private set; }

    // Navigation property for many-to-many relationship
    public ICollection<GameNightBoardGame> GameNights { get; private set; } = new List<GameNightBoardGame>();

    // For EF Core
    private BoardGame() { }

    public BoardGame(
        string name,
        string description,
        GameGenre genre,
        GameType gameType,
        bool isAdultOnly,
        int minPlayers = 2,
        int maxPlayers = 6,
        int estimatedDurationMinutes = 60,
        string? photoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Naam is verplicht.", nameof(name));

        if (minPlayers < 1)
            throw new ArgumentException("Minimaal aantal spelers moet minstens 1 zijn.", nameof(minPlayers));

        if (maxPlayers < minPlayers)
            throw new ArgumentException("Maximaal aantal spelers moet groter of gelijk zijn aan minimaal aantal.", nameof(maxPlayers));

        Name = name;
        Description = description ?? string.Empty;
        Genre = genre;
        GameType = gameType;
        IsAdultOnly = isAdultOnly;
        MinPlayers = minPlayers;
        MaxPlayers = maxPlayers;
        EstimatedDurationMinutes = estimatedDurationMinutes;
        PhotoUrl = photoUrl;
    }

    /// <summary>
    /// Updates the board game information.
    /// </summary>
    public void Update(
        string name,
        string description,
        GameGenre genre,
        GameType gameType,
        bool isAdultOnly,
        int minPlayers,
        int maxPlayers,
        int estimatedDurationMinutes,
        string? photoUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Naam is verplicht.", nameof(name));

        Name = name;
        Description = description ?? string.Empty;
        Genre = genre;
        GameType = gameType;
        IsAdultOnly = isAdultOnly;
        MinPlayers = minPlayers;
        MaxPlayers = maxPlayers;
        EstimatedDurationMinutes = estimatedDurationMinutes;
        PhotoUrl = photoUrl;
    }

    /// <summary>
    /// Sets the photo URL for the board game.
    /// </summary>
    public void SetPhotoUrl(string? photoUrl)
    {
        PhotoUrl = photoUrl;
    }

    /// <summary>
    /// Gets a display string showing the player count range.
    /// </summary>
    public string PlayerCountDisplay => MinPlayers == MaxPlayers 
        ? $"{MinPlayers} spelers" 
        : $"{MinPlayers}-{MaxPlayers} spelers";

    /// <summary>
    /// Gets a display string showing the estimated duration.
    /// </summary>
    public string DurationDisplay => EstimatedDurationMinutes >= 60
        ? $"{EstimatedDurationMinutes / 60}u {EstimatedDurationMinutes % 60}min"
        : $"{EstimatedDurationMinutes} min";
}
