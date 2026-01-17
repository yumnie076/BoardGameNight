using BoardGameNight.Domain.Entities;
using BoardGameNight.Domain.Enums;
using BoardGameNight.Domain.Exceptions;
using Xunit;

namespace BoardGameNight.Domain.Tests;

public class PersonTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesPersonSuccessfully()
    {
        // Arrange
        var dateOfBirth = DateTime.Today.AddYears(-25);

        // Act
        var person = new Person(
            "Jan de Vries",
            "jan@example.com",
            Gender.M,
            dateOfBirth,
            "Hoofdstraat",
            "123",
            "Amsterdam");

        // Assert
        Assert.Equal("Jan de Vries", person.Name);
        Assert.Equal("jan@example.com", person.Email);
        Assert.Equal(Gender.M, person.Gender);
        Assert.Equal(25, person.Age);
        Assert.True(person.IsAdult);
    }

    [Fact]
    public void Constructor_WithFutureDateOfBirth_ThrowsException()
    {
        // Arrange
        var futureDateOfBirth = DateTime.Today.AddDays(1);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            new Person("Test", "test@example.com", Gender.M, futureDateOfBirth, "Street", "1", "City"));

        Assert.Equal("Geboortedatum mag niet in de toekomst liggen.", exception.Message);
    }

    [Fact]
    public void Constructor_WithAgeUnder16_ThrowsException()
    {
        // Arrange
        var dateOfBirth = DateTime.Today.AddYears(-15);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            new Person("Test", "test@example.com", Gender.M, dateOfBirth, "Street", "1", "City"));

        Assert.Equal("Je moet minimaal 16 jaar oud zijn om een account aan te maken.", exception.Message);
    }

    [Theory]
    [InlineData(16, false)]
    [InlineData(17, false)]
    [InlineData(18, true)]
    [InlineData(25, true)]
    public void IsAdult_ReturnsCorrectValue(int age, bool expectedIsAdult)
    {
        // Arrange
        var dateOfBirth = DateTime.Today.AddYears(-age);
        var person = new Person("Test", "test@example.com", Gender.M, dateOfBirth, "Street", "1", "City");

        // Act & Assert
        Assert.Equal(expectedIsAdult, person.IsAdult);
    }

    [Theory]
    [InlineData(17, false)]
    [InlineData(18, true)]
    [InlineData(30, true)]
    public void CanOrganize_ReturnsCorrectValue(int age, bool expectedCanOrganize)
    {
        // Arrange
        var dateOfBirth = DateTime.Today.AddYears(-age);
        var person = new Person("Test", "test@example.com", Gender.M, dateOfBirth, "Street", "1", "City");

        // Act & Assert
        Assert.Equal(expectedCanOrganize, person.CanOrganize);
    }

    [Fact]
    public void FullAddress_ReturnsFormattedAddress()
    {
        // Arrange
        var person = new Person("Test", "test@example.com", Gender.M, 
            DateTime.Today.AddYears(-20), "Hoofdstraat", "123", "Amsterdam");

        // Act & Assert
        Assert.Equal("Hoofdstraat 123, Amsterdam", person.FullAddress);
    }

    [Fact]
    public void AreDietaryNeedsMet_WithNoPreferences_ReturnsTrue()
    {
        // Arrange
        var person = new Person("Test", "test@example.com", Gender.M,
            DateTime.Today.AddYears(-20), "Street", "1", "City", DietaryPreference.None);

        // Act & Assert
        Assert.True(person.AreDietaryNeedsMet(DietaryPreference.Vegetarian));
    }

    [Fact]
    public void AreDietaryNeedsMet_WithMatchingPreferences_ReturnsTrue()
    {
        // Arrange
        var person = new Person("Test", "test@example.com", Gender.M,
            DateTime.Today.AddYears(-20), "Street", "1", "City", 
            DietaryPreference.Vegetarian | DietaryPreference.LactoseFree);

        var availableOptions = DietaryPreference.Vegetarian | DietaryPreference.LactoseFree | DietaryPreference.NutFree;

        // Act & Assert
        Assert.True(person.AreDietaryNeedsMet(availableOptions));
    }

    [Fact]
    public void AreDietaryNeedsMet_WithMissingPreferences_ReturnsFalse()
    {
        // Arrange
        var person = new Person("Test", "test@example.com", Gender.M,
            DateTime.Today.AddYears(-20), "Street", "1", "City",
            DietaryPreference.Vegetarian | DietaryPreference.LactoseFree);

        var availableOptions = DietaryPreference.Vegetarian; // Missing LactoseFree

        // Act & Assert
        Assert.False(person.AreDietaryNeedsMet(availableOptions));
    }
}
