using BoardGameNight.Domain.Enums;
using BoardGameNight.Domain.Exceptions;

namespace BoardGameNight.Domain.Entities;

/// <summary>
/// Represents a board game night event.
/// </summary>
public class GameNight
{
    public int Id { get; private set; }
    
    public DateTime DateTime { get; private set; }
    
    // Address
    public string Street { get; private set; } = string.Empty;
    
    public string HouseNumber { get; private set; } = string.Empty;
    
    public string City { get; private set; } = string.Empty;
    
    public int MaxPlayers { get; private set; }
    
    public bool IsAdultOnly { get; private set; }
    
    /// <summary>
    /// Indicates if this is a potluck event where participants bring food.
    /// </summary>
    public bool IsPotluck { get; private set; }
    
    /// <summary>
    /// Dietary options available at this game night (set by organizer).
    /// </summary>
    public DietaryPreference AvailableDietaryOptions { get; private set; }

    // Foreign key
    public int OrganizerId { get; private set; }
    
    // Navigation properties
    public Person Organizer { get; private set; } = null!;
    
    public ICollection<GameNightBoardGame> BoardGames { get; private set; } = new List<GameNightBoardGame>();
    
    public ICollection<GameNightParticipation> Participants { get; private set; } = new List<GameNightParticipation>();
    
    public ICollection<FoodItem> FoodItems { get; private set; } = new List<FoodItem>();
    
    public ICollection<Review> Reviews { get; private set; } = new List<Review>();

    // For EF Core
    private GameNight() { }

    public GameNight(
        Person organizer,
        DateTime dateTime,
        string street,
        string houseNumber,
        string city,
        int maxPlayers,
        bool isAdultOnly = false,
        bool isPotluck = false,
        DietaryPreference availableDietaryOptions = DietaryPreference.None)
    {
        ValidateOrganizer(organizer);
        ValidateDateTime(dateTime);
        ValidateMaxPlayers(maxPlayers);

        Organizer = organizer;
        OrganizerId = organizer.Id;
        DateTime = dateTime;
        Street = street ?? throw new ArgumentNullException(nameof(street));
        HouseNumber = houseNumber ?? throw new ArgumentNullException(nameof(houseNumber));
        City = city ?? throw new ArgumentNullException(nameof(city));
        MaxPlayers = maxPlayers;
        IsAdultOnly = isAdultOnly;
        IsPotluck = isPotluck;
        AvailableDietaryOptions = availableDietaryOptions;
    }

    /// <summary>
    /// Gets the full address as a formatted string.
    /// </summary>
    public string FullAddress => $"{Street} {HouseNumber}, {City}";

    /// <summary>
    /// Gets the current number of participants (excluding organizer).
    /// </summary>
    public int CurrentPlayerCount => Participants.Count;

    /// <summary>
    /// Checks if the game night is full.
    /// </summary>
    public bool IsFull => CurrentPlayerCount >= MaxPlayers;

    /// <summary>
    /// Checks if the game night has already occurred.
    /// </summary>
    public bool HasPassed => DateTime < System.DateTime.Now;

    /// <summary>
    /// Checks if there are any participants registered.
    /// </summary>
    public bool HasParticipants => Participants.Any();

    /// <summary>
    /// Updates the game night details. Only allowed if no participants have registered.
    /// </summary>
    public void Update(
        DateTime dateTime,
        string street,
        string houseNumber,
        string city,
        int maxPlayers,
        bool isAdultOnly,
        bool isPotluck,
        DietaryPreference availableDietaryOptions)
    {
        if (HasParticipants)
        {
            throw new DomainValidationException(
                "Kan de bordspellenavond niet wijzigen omdat er al spelers zijn ingeschreven.");
        }

        ValidateDateTime(dateTime);
        ValidateMaxPlayers(maxPlayers);

        DateTime = dateTime;
        Street = street ?? throw new ArgumentNullException(nameof(street));
        HouseNumber = houseNumber ?? throw new ArgumentNullException(nameof(houseNumber));
        City = city ?? throw new ArgumentNullException(nameof(city));
        MaxPlayers = maxPlayers;
        IsAdultOnly = isAdultOnly;
        IsPotluck = isPotluck;
        AvailableDietaryOptions = availableDietaryOptions;

        // Recalculate adult-only status based on games
        RecalculateAdultOnlyStatus();
    }

    /// <summary>
    /// Adds a board game to this game night.
    /// </summary>
    public void AddBoardGame(BoardGame game)
    {
        if (game == null)
            throw new ArgumentNullException(nameof(game));

        if (BoardGames.Any(bg => bg.BoardGameId == game.Id))
            return; // Already added

        var gameNightBoardGame = new GameNightBoardGame(this, game);
        BoardGames.Add(gameNightBoardGame);

        // If an 18+ game is added, the night becomes 18+
        if (game.IsAdultOnly)
        {
            IsAdultOnly = true;
        }
    }

    /// <summary>
    /// Removes a board game from this game night.
    /// </summary>
    public void RemoveBoardGame(BoardGame game)
    {
        var existing = BoardGames.FirstOrDefault(bg => bg.BoardGameId == game.Id);
        if (existing != null)
        {
            BoardGames.Remove(existing);
            RecalculateAdultOnlyStatus();
        }
    }

    /// <summary>
    /// Registers a person as a participant for this game night.
    /// </summary>
    public GameNightParticipation AddParticipant(Person person)
    {
        ValidateParticipantCanJoin(person);

        var participation = new GameNightParticipation(this, person);
        Participants.Add(participation);
        return participation;
    }

    /// <summary>
    /// Removes a participant from this game night.
    /// </summary>
    public void RemoveParticipant(Person person)
    {
        var participation = Participants.FirstOrDefault(p => p.PersonId == person.Id);
        if (participation != null)
        {
            Participants.Remove(participation);
        }
    }

    /// <summary>
    /// Checks if a person is already registered as a participant.
    /// </summary>
    public bool IsParticipant(Person person)
    {
        return Participants.Any(p => p.PersonId == person.Id);
    }

    /// <summary>
    /// Checks if a person can join this game night.
    /// </summary>
    public bool CanPersonJoin(Person person, out string reason)
    {
        reason = string.Empty;

        if (IsFull)
        {
            reason = "De bordspellenavond is vol.";
            return false;
        }

        if (IsAdultOnly && !person.IsAdult)
        {
            reason = "Deze bordspellenavond is alleen voor volwassenen (18+).";
            return false;
        }

        if (IsParticipant(person))
        {
            reason = "Je bent al ingeschreven voor deze bordspellenavond.";
            return false;
        }

        if (person.Id == OrganizerId)
        {
            reason = "Je bent de organisator van deze bordspellenavond.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Adds a food item to this game night (for potluck events).
    /// </summary>
    public void AddFoodItem(FoodItem foodItem)
    {
        if (foodItem == null)
            throw new ArgumentNullException(nameof(foodItem));

        FoodItems.Add(foodItem);
        
        // Update available dietary options
        AvailableDietaryOptions |= foodItem.DietaryOptions;
    }

    /// <summary>
    /// Adds a review for this game night.
    /// </summary>
    public Review AddReview(Person reviewer, int rating, string reviewText)
    {
        if (!HasPassed)
        {
            throw new DomainValidationException("Je kunt alleen een review schrijven na de bordspellenavond.");
        }

        if (!Participants.Any(p => p.PersonId == reviewer.Id))
        {
            throw new DomainValidationException("Je kunt alleen een review schrijven als je hebt deelgenomen.");
        }

        if (Reviews.Any(r => r.ReviewerId == reviewer.Id))
        {
            throw new DomainValidationException("Je hebt al een review geschreven voor deze bordspellenavond.");
        }

        var review = new Review(this, reviewer, rating, reviewText);
        Reviews.Add(review);
        return review;
    }

    /// <summary>
    /// Checks if this game night can be deleted.
    /// </summary>
    public bool CanBeDeleted => !HasParticipants;

    private void ValidateOrganizer(Person organizer)
    {
        if (organizer == null)
            throw new ArgumentNullException(nameof(organizer));

        if (!organizer.CanOrganize)
        {
            throw new DomainValidationException(
                "Je moet minimaal 18 jaar oud zijn om een bordspellenavond te organiseren.");
        }
    }

    private static void ValidateDateTime(DateTime dateTime)
    {
        if (dateTime <= System.DateTime.Now)
        {
            throw new DomainValidationException(
                "De datum en tijd van de bordspellenavond moet in de toekomst liggen.");
        }
    }

    private static void ValidateMaxPlayers(int maxPlayers)
    {
        if (maxPlayers < 2)
        {
            throw new DomainValidationException(
                "Het maximaal aantal spelers moet minimaal 2 zijn.");
        }
    }

    private void ValidateParticipantCanJoin(Person person)
    {
        if (!CanPersonJoin(person, out string reason))
        {
            throw new DomainValidationException(reason);
        }
    }

    private void RecalculateAdultOnlyStatus()
    {
        // If any game is 18+, the night is 18+
        IsAdultOnly = BoardGames.Any(bg => bg.BoardGame?.IsAdultOnly == true);
    }
}
