using BoardGameNight.Domain.Entities;
using BoardGameNight.Domain.Enums;
using BoardGameNight.Domain.Exceptions;
using System.Reflection;
using Xunit;

namespace BoardGameNight.Domain.Tests;

public class GameNightTests
{
    private int _idCounter = 1;

    private Person CreateAdultPerson(string name = "Test", int age = 25)
    {
        var person = new Person(name, $"{name.ToLower()}{_idCounter}@example.com", Gender.M,
            DateTime.Today.AddYears(-age), "Street", "1", "City");
        
        // Set unique ID via reflection for testing (normally set by EF)
        SetPersonId(person, _idCounter++);
        return person;
    }

    private Person CreateMinorPerson(string name = "Minor", int age = 17)
    {
        var person = new Person(name, $"{name.ToLower()}{_idCounter}@example.com", Gender.M,
            DateTime.Today.AddYears(-age), "Street", "1", "City");
        
        SetPersonId(person, _idCounter++);
        return person;
    }

    private void SetPersonId(Person person, int id)
    {
        // Use reflection to set the Id property for testing
        var idProperty = typeof(Person).GetProperty("Id");
        if (idProperty != null && idProperty.CanWrite)
        {
            idProperty.SetValue(person, id);
        }
        else
        {
            // Try setting via backing field
            var field = typeof(Person).GetField("<Id>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(person, id);
        }
    }

    [Fact]
    public void Constructor_WithAdultOrganizer_CreatesGameNightSuccessfully()
    {
        // Arrange
        var organizer = CreateAdultPerson();
        var dateTime = DateTime.Now.AddDays(7);

        // Act
        var gameNight = new GameNight(organizer, dateTime, "Street", "1", "City", 6);

        // Assert
        Assert.Equal(organizer.Id, gameNight.OrganizerId);
        Assert.Equal(6, gameNight.MaxPlayers);
        Assert.False(gameNight.IsAdultOnly);
    }

    [Fact]
    public void Constructor_WithMinorOrganizer_ThrowsException()
    {
        // Arrange
        var minor = CreateMinorPerson();
        var dateTime = DateTime.Now.AddDays(7);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            new GameNight(minor, dateTime, "Street", "1", "City", 6));

        Assert.Equal("Je moet minimaal 18 jaar oud zijn om een bordspellenavond te organiseren.", exception.Message);
    }

    [Fact]
    public void Constructor_WithPastDateTime_ThrowsException()
    {
        // Arrange
        var organizer = CreateAdultPerson();
        var pastDateTime = DateTime.Now.AddDays(-1);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            new GameNight(organizer, pastDateTime, "Street", "1", "City", 6));

        Assert.Equal("De datum en tijd van de bordspellenavond moet in de toekomst liggen.", exception.Message);
    }

    [Fact]
    public void Constructor_WithMaxPlayersLessThan2_ThrowsException()
    {
        // Arrange
        var organizer = CreateAdultPerson();
        var dateTime = DateTime.Now.AddDays(7);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            new GameNight(organizer, dateTime, "Street", "1", "City", 1));

        Assert.Equal("Het maximaal aantal spelers moet minimaal 2 zijn.", exception.Message);
    }

    [Fact]
    public void AddBoardGame_With18PlusGame_MakesGameNightAdultOnly()
    {
        // Arrange
        var organizer = CreateAdultPerson();
        var gameNight = new GameNight(organizer, DateTime.Now.AddDays(7), "Street", "1", "City", 6);
        var adultGame = new BoardGame("Adult Game", "Description", GameGenre.Party, GameType.CardGame, true);

        // Act
        gameNight.AddBoardGame(adultGame);

        // Assert
        Assert.True(gameNight.IsAdultOnly);
    }

    [Fact]
    public void AddParticipant_WhenNotFull_AddsSuccessfully()
    {
        // Arrange
        var organizer = CreateAdultPerson("Organizer");
        var participant = CreateAdultPerson("Participant");
        var gameNight = new GameNight(organizer, DateTime.Now.AddDays(7), "Street", "1", "City", 6);

        // Act
        var participation = gameNight.AddParticipant(participant);

        // Assert
        Assert.NotNull(participation);
        Assert.Equal(1, gameNight.CurrentPlayerCount);
        Assert.True(gameNight.IsParticipant(participant));
    }

    [Fact]
    public void AddParticipant_WhenFull_ThrowsException()
    {
        // Arrange
        var organizer = CreateAdultPerson("Organizer");
        var gameNight = new GameNight(organizer, DateTime.Now.AddDays(7), "Street", "1", "City", 2);
        
        gameNight.AddParticipant(CreateAdultPerson("P1"));
        gameNight.AddParticipant(CreateAdultPerson("P2"));

        var newParticipant = CreateAdultPerson("P3");

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            gameNight.AddParticipant(newParticipant));

        Assert.Equal("De bordspellenavond is vol.", exception.Message);
    }

    [Fact]
    public void AddParticipant_MinorTo18PlusEvent_ThrowsException()
    {
        // Arrange
        var organizer = CreateAdultPerson("Organizer");
        var gameNight = new GameNight(organizer, DateTime.Now.AddDays(7), "Street", "1", "City", 6, isAdultOnly: true);
        var minor = CreateMinorPerson();

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            gameNight.AddParticipant(minor));

        Assert.Equal("Deze bordspellenavond is alleen voor volwassenen (18+).", exception.Message);
    }

    [Fact]
    public void AddParticipant_AlreadyRegistered_ThrowsException()
    {
        // Arrange
        var organizer = CreateAdultPerson("Organizer");
        var participant = CreateAdultPerson("Participant");
        var gameNight = new GameNight(organizer, DateTime.Now.AddDays(7), "Street", "1", "City", 6);
        gameNight.AddParticipant(participant);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            gameNight.AddParticipant(participant));

        Assert.Equal("Je bent al ingeschreven voor deze bordspellenavond.", exception.Message);
    }

    [Fact]
    public void Update_WithParticipants_ThrowsException()
    {
        // Arrange
        var organizer = CreateAdultPerson("Organizer");
        var participant = CreateAdultPerson("Participant");
        var gameNight = new GameNight(organizer, DateTime.Now.AddDays(7), "Street", "1", "City", 6);
        gameNight.AddParticipant(participant);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            gameNight.Update(DateTime.Now.AddDays(14), "New Street", "2", "New City", 8, false, false, DietaryPreference.None));

        Assert.Equal("Kan de bordspellenavond niet wijzigen omdat er al spelers zijn ingeschreven.", exception.Message);
    }

    [Fact]
    public void CanBeDeleted_WithoutParticipants_ReturnsTrue()
    {
        // Arrange
        var organizer = CreateAdultPerson();
        var gameNight = new GameNight(organizer, DateTime.Now.AddDays(7), "Street", "1", "City", 6);

        // Assert
        Assert.True(gameNight.CanBeDeleted);
    }

    [Fact]
    public void CanBeDeleted_WithParticipants_ReturnsFalse()
    {
        // Arrange
        var organizer = CreateAdultPerson("Organizer");
        var participant = CreateAdultPerson("Participant");
        var gameNight = new GameNight(organizer, DateTime.Now.AddDays(7), "Street", "1", "City", 6);
        gameNight.AddParticipant(participant);

        // Assert
        Assert.False(gameNight.CanBeDeleted);
    }

    [Fact]
    public void IsFull_WhenMaxPlayersReached_ReturnsTrue()
    {
        // Arrange
        var organizer = CreateAdultPerson("Organizer");
        var gameNight = new GameNight(organizer, DateTime.Now.AddDays(7), "Street", "1", "City", 2);
        
        gameNight.AddParticipant(CreateAdultPerson("P1"));
        gameNight.AddParticipant(CreateAdultPerson("P2"));

        // Assert
        Assert.True(gameNight.IsFull);
    }

    [Fact]
    public void AddParticipant_Organizer_ThrowsException()
    {
        // Arrange
        var organizer = CreateAdultPerson("Organizer");
        var gameNight = new GameNight(organizer, DateTime.Now.AddDays(7), "Street", "1", "City", 6);

        // Act & Assert - Organizer cannot join their own event
        var exception = Assert.Throws<DomainValidationException>(() =>
            gameNight.AddParticipant(organizer));

        Assert.Equal("Je bent de organisator van deze bordspellenavond.", exception.Message);
    }
}
