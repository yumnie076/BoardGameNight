using BoardGameNight.Application.DTOs;
using BoardGameNight.Application.Interfaces;
using BoardGameNight.Domain.Entities;
using BoardGameNight.Domain.Exceptions;
using BoardGameNight.Domain.Interfaces;
using BoardGameNight.Domain.Services;

namespace BoardGameNight.Application.Services;

/// <summary>
/// Application service for game night operations.
/// </summary>
public class GameNightService : IGameNightService
{
    private readonly IGameNightRepository _gameNightRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IBoardGameRepository _boardGameRepository;
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly GameNightDomainService _domainService;

    public GameNightService(
        IGameNightRepository gameNightRepository,
        IPersonRepository personRepository,
        IBoardGameRepository boardGameRepository,
        IFoodItemRepository foodItemRepository,
        IReviewRepository reviewRepository,
        GameNightDomainService domainService)
    {
        _gameNightRepository = gameNightRepository;
        _personRepository = personRepository;
        _boardGameRepository = boardGameRepository;
        _foodItemRepository = foodItemRepository;
        _reviewRepository = reviewRepository;
        _domainService = domainService;
    }

    public async Task<IEnumerable<GameNightListDto>> GetAllUpcomingAsync()
    {
        var gameNights = await _gameNightRepository.GetUpcomingAsync();
        return gameNights.Select(MapToListDto);
    }

    public async Task<IEnumerable<GameNightListDto>> GetByOrganizerAsync(int organizerId)
    {
        var gameNights = await _gameNightRepository.GetByOrganizerAsync(organizerId);
        return gameNights.Select(MapToListDto);
    }

    public async Task<IEnumerable<GameNightListDto>> GetByParticipantAsync(int personId)
    {
        var gameNights = await _gameNightRepository.GetByParticipantAsync(personId);
        return gameNights.Select(MapToListDto);
    }

    public async Task<GameNightDto?> GetByIdAsync(int id)
    {
        var gameNight = await _gameNightRepository.GetByIdAsync(id);
        return gameNight == null ? null : await MapToDtoAsync(gameNight);
    }

    public async Task<GameNightDto?> GetWithDetailsAsync(int id)
    {
        var gameNight = await _gameNightRepository.GetWithDetailsAsync(id);
        return gameNight == null ? null : await MapToDtoAsync(gameNight);
    }

    public async Task<GameNightDto> CreateAsync(int organizerId, CreateGameNightDto dto)
    {
        var organizer = await _personRepository.GetByIdAsync(organizerId)
            ?? throw new DomainValidationException("Organisator niet gevonden.");

        var gameNight = new GameNight(
            organizer,
            dto.DateTime,
            dto.Street,
            dto.HouseNumber,
            dto.City,
            dto.MaxPlayers,
            dto.IsAdultOnly,
            dto.IsPotluck,
            dto.AvailableDietaryOptions);

        // Add board games
        foreach (var gameId in dto.BoardGameIds)
        {
            var boardGame = await _boardGameRepository.GetByIdAsync(gameId);
            if (boardGame != null)
            {
                gameNight.AddBoardGame(boardGame);
            }
        }

        await _gameNightRepository.AddAsync(gameNight);
        return await MapToDtoAsync(gameNight);
    }

    public async Task<GameNightDto> UpdateAsync(int organizerId, UpdateGameNightDto dto)
    {
        var gameNight = await _gameNightRepository.GetWithDetailsAsync(dto.Id)
            ?? throw new DomainValidationException("Bordspellenavond niet gevonden.");

        if (gameNight.OrganizerId != organizerId)
        {
            throw new DomainValidationException("Je bent niet de organisator van deze bordspellenavond.");
        }

        gameNight.Update(
            dto.DateTime,
            dto.Street,
            dto.HouseNumber,
            dto.City,
            dto.MaxPlayers,
            dto.IsAdultOnly,
            dto.IsPotluck,
            dto.AvailableDietaryOptions);

        // Update board games - remove all and add new ones
        var currentGames = gameNight.BoardGames.ToList();
        foreach (var game in currentGames)
        {
            var boardGame = await _boardGameRepository.GetByIdAsync(game.BoardGameId);
            if (boardGame != null)
            {
                gameNight.RemoveBoardGame(boardGame);
            }
        }

        foreach (var gameId in dto.BoardGameIds)
        {
            var boardGame = await _boardGameRepository.GetByIdAsync(gameId);
            if (boardGame != null)
            {
                gameNight.AddBoardGame(boardGame);
            }
        }

        await _gameNightRepository.UpdateAsync(gameNight);
        return await MapToDtoAsync(gameNight);
    }

    public async Task DeleteAsync(int organizerId, int gameNightId)
    {
        var gameNight = await _gameNightRepository.GetWithDetailsAsync(gameNightId)
            ?? throw new DomainValidationException("Bordspellenavond niet gevonden.");

        if (gameNight.OrganizerId != organizerId)
        {
            throw new DomainValidationException("Je bent niet de organisator van deze bordspellenavond.");
        }

        if (!gameNight.CanBeDeleted)
        {
            throw new DomainValidationException(
                "Kan de bordspellenavond niet verwijderen omdat er al spelers zijn ingeschreven.");
        }

        await _gameNightRepository.DeleteAsync(gameNight);
    }

    public async Task<(bool Success, string Message, IEnumerable<string> Warnings)> RegisterParticipantAsync(
        int personId, int gameNightId)
    {
        var person = await _personRepository.GetByIdAsync(personId)
            ?? throw new DomainValidationException("Persoon niet gevonden.");

        var gameNight = await _gameNightRepository.GetWithDetailsAsync(gameNightId)
            ?? throw new DomainValidationException("Bordspellenavond niet gevonden.");

        var (canRegister, reason) = await _domainService.CanPersonRegisterAsync(person, gameNight);

        if (!canRegister)
        {
            return (false, reason, Enumerable.Empty<string>());
        }

        var warnings = _domainService.GetDietaryWarnings(person, gameNight);

        await _domainService.RegisterPersonAsync(person, gameNight);
        await _gameNightRepository.UpdateAsync(gameNight);

        return (true, "Je bent succesvol ingeschreven voor de bordspellenavond.", warnings);
    }

    public async Task UnregisterParticipantAsync(int personId, int gameNightId)
    {
        var person = await _personRepository.GetByIdAsync(personId)
            ?? throw new DomainValidationException("Persoon niet gevonden.");

        var gameNight = await _gameNightRepository.GetWithDetailsAsync(gameNightId)
            ?? throw new DomainValidationException("Bordspellenavond niet gevonden.");

        gameNight.RemoveParticipant(person);
        await _gameNightRepository.UpdateAsync(gameNight);
    }

    public async Task<bool> CanPersonJoinAsync(int personId, int gameNightId)
    {
        var person = await _personRepository.GetByIdAsync(personId);
        var gameNight = await _gameNightRepository.GetWithDetailsAsync(gameNightId);

        if (person == null || gameNight == null)
            return false;

        var (canRegister, _) = await _domainService.CanPersonRegisterAsync(person, gameNight);
        return canRegister;
    }

    public async Task AddBoardGameAsync(int gameNightId, int boardGameId)
    {
        var gameNight = await _gameNightRepository.GetWithDetailsAsync(gameNightId)
            ?? throw new DomainValidationException("Bordspellenavond niet gevonden.");

        var boardGame = await _boardGameRepository.GetByIdAsync(boardGameId)
            ?? throw new DomainValidationException("Bordspel niet gevonden.");

        gameNight.AddBoardGame(boardGame);
        await _gameNightRepository.UpdateAsync(gameNight);
    }

    public async Task RemoveBoardGameAsync(int gameNightId, int boardGameId)
    {
        var gameNight = await _gameNightRepository.GetWithDetailsAsync(gameNightId)
            ?? throw new DomainValidationException("Bordspellenavond niet gevonden.");

        var boardGame = await _boardGameRepository.GetByIdAsync(boardGameId)
            ?? throw new DomainValidationException("Bordspel niet gevonden.");

        gameNight.RemoveBoardGame(boardGame);
        await _gameNightRepository.UpdateAsync(gameNight);
    }

    public async Task<FoodItemDto> AddFoodItemAsync(int personId, CreateFoodItemDto dto)
    {
        var person = await _personRepository.GetByIdAsync(personId)
            ?? throw new DomainValidationException("Persoon niet gevonden.");

        var gameNight = await _gameNightRepository.GetWithDetailsAsync(dto.GameNightId)
            ?? throw new DomainValidationException("Bordspellenavond niet gevonden.");

        if (!gameNight.IsPotluck)
        {
            throw new DomainValidationException("Dit is geen potluck bordspellenavond.");
        }

        var foodItem = new FoodItem(
            dto.Name,
            gameNight,
            dto.DietaryOptions,
            person,
            dto.Description);

        gameNight.AddFoodItem(foodItem);
        await _gameNightRepository.UpdateAsync(gameNight);

        return MapToFoodItemDto(foodItem);
    }

    public async Task<IEnumerable<FoodItemDto>> GetFoodItemsAsync(int gameNightId)
    {
        var foodItems = await _foodItemRepository.GetByGameNightAsync(gameNightId);
        return foodItems.Select(MapToFoodItemDto);
    }

    public async Task<ReviewDto> AddReviewAsync(int reviewerId, CreateReviewDto dto)
    {
        var reviewer = await _personRepository.GetByIdAsync(reviewerId)
            ?? throw new DomainValidationException("Persoon niet gevonden.");

        var gameNight = await _gameNightRepository.GetWithDetailsAsync(dto.GameNightId)
            ?? throw new DomainValidationException("Bordspellenavond niet gevonden.");

        var review = gameNight.AddReview(reviewer, dto.Rating, dto.ReviewText);
        await _gameNightRepository.UpdateAsync(gameNight);

        return MapToReviewDto(review);
    }

    public async Task<IEnumerable<ReviewDto>> GetReviewsAsync(int gameNightId)
    {
        var reviews = await _reviewRepository.GetByGameNightAsync(gameNightId);
        return reviews.Select(MapToReviewDto);
    }

    public async Task RecordAttendanceAsync(int organizerId, RecordAttendanceDto dto)
    {
        var gameNight = await _gameNightRepository.GetWithDetailsAsync(dto.GameNightId)
            ?? throw new DomainValidationException("Bordspellenavond niet gevonden.");

        if (gameNight.OrganizerId != organizerId)
        {
            throw new DomainValidationException("Je bent niet de organisator van deze bordspellenavond.");
        }

        var participation = gameNight.Participants.FirstOrDefault(p => p.PersonId == dto.PersonId)
            ?? throw new DomainValidationException("Deelnemer niet gevonden.");

        participation.RecordAttendance(dto.DidAttend);
        await _gameNightRepository.UpdateAsync(gameNight);
    }

    // Mapping methods
    private GameNightListDto MapToListDto(GameNight gameNight)
    {
        return new GameNightListDto
        {
            Id = gameNight.Id,
            DateTime = gameNight.DateTime,
            City = gameNight.City,
            MaxPlayers = gameNight.MaxPlayers,
            CurrentPlayerCount = gameNight.CurrentPlayerCount,
            IsFull = gameNight.IsFull,
            IsAdultOnly = gameNight.IsAdultOnly,
            OrganizerName = gameNight.Organizer?.Name ?? "Onbekend",
            BoardGameCount = gameNight.BoardGames.Count
        };
    }

    private async Task<GameNightDto> MapToDtoAsync(GameNight gameNight)
    {
        var organizerRating = await _personRepository.GetAverageOrganizerRatingAsync(gameNight.OrganizerId);
        var organizerCount = await _personRepository.GetOrganizedGameNightCountAsync(gameNight.OrganizerId);

        var participantDtos = new List<ParticipantDto>();
        foreach (var p in gameNight.Participants)
        {
            var shows = await _personRepository.GetShowCountAsync(p.PersonId);
            var noShows = await _personRepository.GetNoShowCountAsync(p.PersonId);
            participantDtos.Add(new ParticipantDto
            {
                PersonId = p.PersonId,
                Name = p.Person?.Name ?? "Onbekend",
                RegistrationDate = p.RegistrationDate,
                DidAttend = p.DidAttend,
                TotalShows = shows,
                TotalNoShows = noShows
            });
        }

        return new GameNightDto
        {
            Id = gameNight.Id,
            DateTime = gameNight.DateTime,
            Street = gameNight.Street,
            HouseNumber = gameNight.HouseNumber,
            City = gameNight.City,
            FullAddress = gameNight.FullAddress,
            MaxPlayers = gameNight.MaxPlayers,
            CurrentPlayerCount = gameNight.CurrentPlayerCount,
            IsFull = gameNight.IsFull,
            IsAdultOnly = gameNight.IsAdultOnly,
            IsPotluck = gameNight.IsPotluck,
            HasPassed = gameNight.HasPassed,
            AvailableDietaryOptions = gameNight.AvailableDietaryOptions,
            OrganizerId = gameNight.OrganizerId,
            OrganizerName = gameNight.Organizer?.Name ?? "Onbekend",
            OrganizerAverageRating = organizerRating,
            OrganizerGameNightCount = organizerCount,
            BoardGames = gameNight.BoardGames.Select(bg => MapToBoardGameDto(bg.BoardGame!)).ToList(),
            Participants = participantDtos,
            FoodItems = gameNight.FoodItems.Select(MapToFoodItemDto).ToList(),
            Reviews = gameNight.Reviews.Select(MapToReviewDto).ToList()
        };
    }

    private static BoardGameDto MapToBoardGameDto(BoardGame boardGame)
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

    private static FoodItemDto MapToFoodItemDto(FoodItem foodItem)
    {
        return new FoodItemDto
        {
            Id = foodItem.Id,
            Name = foodItem.Name,
            Description = foodItem.Description,
            DietaryOptions = foodItem.DietaryOptions,
            BroughtByPersonName = foodItem.BroughtByPerson?.Name
        };
    }

    private static ReviewDto MapToReviewDto(Review review)
    {
        return new ReviewDto
        {
            Id = review.Id,
            Rating = review.Rating,
            ReviewText = review.ReviewText,
            CreatedAt = review.CreatedAt,
            ReviewerName = review.Reviewer?.Name ?? "Anoniem"
        };
    }
}
