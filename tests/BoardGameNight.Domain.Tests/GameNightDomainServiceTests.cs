using BoardGameNight.Domain.Entities;
using BoardGameNight.Domain.Enums;
using BoardGameNight.Domain.Exceptions;
using BoardGameNight.Domain.Interfaces;
using BoardGameNight.Domain.Services;
using Moq;
using System.Reflection;
using Xunit;

namespace BoardGameNight.Domain.Tests;

/// <summary>
/// Unit tests for GameNightDomainService with mocking.
/// Tests business rules that span multiple entities (US_04).
/// </summary>
public class GameNightDomainServiceTests
{
    private readonly Mock<IGameNightRepository> _mockGameNightRepository;
    private readonly GameNightDomainService _service;
    private int _idCounter = 100; // Start higher to distinguish from other tests

    public GameNightDomainServiceTests()
    {
        _mockGameNightRepository = new Mock<IGameNightRepository>();
        _service = new GameNightDomainService(_mockGameNightRepository.Object);
    }

    private Person CreateAdultPerson(string name = "Test", int age = 25)
    {
        var person = new Person(name, $"{name.ToLower()}{_idCounter}@example.com", Gender.M,
            DateTime.Today.AddYears(-age), "Street", "1", "City");
        SetPersonId(person, _idCounter++);
        return person;
    }

    private void SetPersonId(Person person, int id)
    {
        var idProperty = typeof(Person).GetProperty("Id");
        if (idProperty != null && idProperty.CanWrite)
        {
            idProperty.SetValue(person, id);
        }
        else
        {
            var field = typeof(Person).GetField("<Id>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(person, id);
        }
    }

    private GameNight CreateGameNight(Person organizer, DateTime? dateTime = null)
    {
        return new GameNight(
            organizer,
            dateTime ?? DateTime.Now.AddDays(7),
            "Street", "1", "City",
            6, false, false, DietaryPreference.None);
    }

    #region CanPersonRegisterAsync Tests

    [Fact]
    public async Task CanPersonRegisterAsync_ValidPerson_ReturnsTrue()
    {
        // Arrange
        var organizer = CreateAdultPerson("Organizer");
        var participant = CreateAdultPerson("Participant");
        var gameNight = CreateGameNight(organizer);

        _mockGameNightRepository
            .Setup(r => r.HasRegistrationOnDateAsync(participant.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        // Act
        var (canRegister, reason) = await _service.CanPersonRegisterAsync(participant, gameNight);

        // Assert
        Assert.True(canRegister);
        Assert.Empty(reason);
    }

    [Fact]
    public async Task CanPersonRegisterAsync_AlreadyRegisteredOnSameDate_ReturnsFalse()
    {
        // Arrange
        var organizer = CreateAdultPerson("Organizer");
        var participant = CreateAdultPerson("Participant");
        var gameNight = CreateGameNight(organizer, DateTime.Now.AddDays(7));

        // Person already has a registration on the same date
        _mockGameNightRepository
            .Setup(r => r.HasRegistrationOnDateAsync(participant.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        // Act
        var (canRegister, reason) = await _service.CanPersonRegisterAsync(participant, gameNight);

        // Assert
        Assert.False(canRegister);
        Assert.Equal("Je kunt je maar voor één spelavond per dag aanmelden.", reason);
    }

    [Fact]
    public async Task CanPersonRegisterAsync_GameNightFull_ReturnsFalse()
    {
        // Arrange
        var organizer = CreateAdultPerson("Organizer");
        var gameNight = new GameNight(
            organizer,
            DateTime.Now.AddDays(7),
            "Street", "1", "City",
            2); // Max 2 players

        // Add 2 participants to make it full
        var p1 = CreateAdultPerson("P1");
        var p2 = CreateAdultPerson("P2");
        gameNight.AddParticipant(p1);
        gameNight.AddParticipant(p2);

        var newParticipant = CreateAdultPerson("NewPerson");

        _mockGameNightRepository
            .Setup(r => r.HasRegistrationOnDateAsync(newParticipant.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        // Act
        var (canRegister, reason) = await _service.CanPersonRegisterAsync(newParticipant, gameNight);

        // Assert
        Assert.False(canRegister);
        Assert.Equal("De bordspellenavond is vol.", reason);
    }

    [Fact]
    public async Task CanPersonRegisterAsync_MinorTo18PlusEvent_ReturnsFalse()
    {
        // Arrange
        var organizer = CreateAdultPerson("Organizer");
        var minor = new Person("Minor", $"minor{_idCounter}@example.com", Gender.M,
            DateTime.Today.AddYears(-17), "Street", "1", "City");
        SetPersonId(minor, _idCounter++);
        
        var gameNight = new GameNight(
            organizer,
            DateTime.Now.AddDays(7),
            "Street", "1", "City",
            6, isAdultOnly: true);

        _mockGameNightRepository
            .Setup(r => r.HasRegistrationOnDateAsync(minor.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        // Act
        var (canRegister, reason) = await _service.CanPersonRegisterAsync(minor, gameNight);

        // Assert
        Assert.False(canRegister);
        Assert.Equal("Deze bordspellenavond is alleen voor volwassenen (18+).", reason);
    }

    [Fact]
    public async Task CanPersonRegisterAsync_Organizer_ReturnsFalse()
    {
        // Arrange
        var organizer = CreateAdultPerson("Organizer");
        var gameNight = CreateGameNight(organizer);

        _mockGameNightRepository
            .Setup(r => r.HasRegistrationOnDateAsync(organizer.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        // Act
        var (canRegister, reason) = await _service.CanPersonRegisterAsync(organizer, gameNight);

        // Assert
        Assert.False(canRegister);
        Assert.Equal("Je bent de organisator van deze bordspellenavond.", reason);
    }

    #endregion

    #region RegisterPersonAsync Tests

    [Fact]
    public async Task RegisterPersonAsync_ValidPerson_ReturnsParticipation()
    {
        // Arrange
        var organizer = CreateAdultPerson("Organizer");
        var participant = CreateAdultPerson("Participant");
        var gameNight = CreateGameNight(organizer);

        _mockGameNightRepository
            .Setup(r => r.HasRegistrationOnDateAsync(participant.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        // Act
        var participation = await _service.RegisterPersonAsync(participant, gameNight);

        // Assert
        Assert.NotNull(participation);
        Assert.True(gameNight.IsParticipant(participant));
    }

    [Fact]
    public async Task RegisterPersonAsync_InvalidPerson_ThrowsException()
    {
        // Arrange
        var organizer = CreateAdultPerson("Organizer");
        var participant = CreateAdultPerson("Participant");
        var gameNight = CreateGameNight(organizer);

        // Person already registered on same date
        _mockGameNightRepository
            .Setup(r => r.HasRegistrationOnDateAsync(participant.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainValidationException>(
            () => _service.RegisterPersonAsync(participant, gameNight));

        Assert.Equal("Je kunt je maar voor één spelavond per dag aanmelden.", exception.Message);
    }

    #endregion

    #region GetDietaryWarnings Tests

    [Fact]
    public void GetDietaryWarnings_NoDietaryNeeds_ReturnsEmptyList()
    {
        // Arrange
        var organizer = CreateAdultPerson("Organizer");
        var participant = new Person("Test", $"test{_idCounter}@example.com", Gender.M,
            DateTime.Today.AddYears(-25), "Street", "1", "City",
            DietaryPreference.None);
        SetPersonId(participant, _idCounter++);
        var gameNight = CreateGameNight(organizer);

        // Act
        var warnings = _service.GetDietaryWarnings(participant, gameNight);

        // Assert
        Assert.Empty(warnings);
    }

    [Fact]
    public void GetDietaryWarnings_UnmetDietaryNeeds_ReturnsWarning()
    {
        // Arrange
        var organizer = CreateAdultPerson("Organizer");
        var participant = new Person("Test", $"test{_idCounter}@example.com", Gender.M,
            DateTime.Today.AddYears(-25), "Street", "1", "City",
            DietaryPreference.Vegetarian | DietaryPreference.LactoseFree);
        SetPersonId(participant, _idCounter++);
        
        // GameNight only offers Vegetarian, not LactoseFree
        var gameNight = new GameNight(
            organizer,
            DateTime.Now.AddDays(7),
            "Street", "1", "City",
            6, false, false,
            DietaryPreference.Vegetarian);

        // Act
        var warnings = _service.GetDietaryWarnings(participant, gameNight);

        // Assert
        Assert.Single(warnings);
        Assert.Contains("dieetwensen", warnings.First());
    }

    [Fact]
    public void GetDietaryWarnings_MetDietaryNeeds_ReturnsEmptyList()
    {
        // Arrange
        var organizer = CreateAdultPerson("Organizer");
        var participant = new Person("Test", $"test{_idCounter}@example.com", Gender.M,
            DateTime.Today.AddYears(-25), "Street", "1", "City",
            DietaryPreference.Vegetarian);
        SetPersonId(participant, _idCounter++);
        
        // GameNight offers what participant needs
        var gameNight = new GameNight(
            organizer,
            DateTime.Now.AddDays(7),
            "Street", "1", "City",
            6, false, false,
            DietaryPreference.Vegetarian | DietaryPreference.LactoseFree);

        // Act
        var warnings = _service.GetDietaryWarnings(participant, gameNight);

        // Assert
        Assert.Empty(warnings);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task CanPersonRegisterAsync_CallsRepositoryWithCorrectPersonId()
    {
        // Arrange
        var organizer = CreateAdultPerson("Organizer");
        var participant = CreateAdultPerson("Participant");
        var gameNight = CreateGameNight(organizer, DateTime.Now.AddDays(7));

        _mockGameNightRepository
            .Setup(r => r.HasRegistrationOnDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        // Act
        await _service.CanPersonRegisterAsync(participant, gameNight);

        // Assert - verify repository was called with the correct person ID
        _mockGameNightRepository.Verify(
            r => r.HasRegistrationOnDateAsync(participant.Id, It.IsAny<DateTime>()),
            Times.Once);
    }

    #endregion
}
