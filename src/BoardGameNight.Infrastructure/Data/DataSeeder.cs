using BoardGameNight.Domain.Entities;
using BoardGameNight.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BoardGameNight.Infrastructure.Data;

/// <summary>
/// Seeds the database with initial test data.
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if already seeded
        if (await context.Persons.AnyAsync())
            return;

        // Create persons
        var persons = new List<Person>
        {
            CreatePerson("Jan de Vries", "jan@example.com", Gender.M, new DateTime(1985, 3, 15), 
                "Hoofdstraat", "123", "Amsterdam", DietaryPreference.None),
            CreatePerson("Maria Santos", "maria@example.com", Gender.V, new DateTime(1990, 7, 22), 
                "Kerkstraat", "45", "Rotterdam", DietaryPreference.Vegetarian),
            CreatePerson("Ahmed Hassan", "ahmed@example.com", Gender.M, new DateTime(1988, 11, 8), 
                "Marktplein", "7", "Utrecht", DietaryPreference.Halal | DietaryPreference.NutFree),
            CreatePerson("Sophie Bakker", "sophie@example.com", Gender.V, new DateTime(1995, 5, 30), 
                "Dorpsweg", "89", "Den Haag", DietaryPreference.LactoseFree),
            CreatePerson("Thomas Mulder", "thomas@example.com", Gender.M, new DateTime(2000, 1, 12), 
                "Stationsstraat", "12", "Eindhoven", DietaryPreference.Vegan),
            CreatePerson("Lisa van Dam", "lisa@example.com", Gender.V, new DateTime(2007, 9, 25), 
                "Parkweg", "34", "Groningen", DietaryPreference.AlcoholFree), // Minor (16-17)
        };

        context.Persons.AddRange(persons);
        await context.SaveChangesAsync();

        // Create board games
        var boardGames = new List<BoardGame>
        {
            new BoardGame("Catan", "Verover het eiland Catan door grondstoffen te verzamelen en steden te bouwen.",
                GameGenre.Strategy, GameType.BoardGame, false, 3, 4, 90,
                "https://cf.geekdo-images.com/W3Bsga_uLP9kO91gZ7H8yw__original/img/M5NJF5RHBxLG55hBEfuEYjdL83o=/0x0/filters:format(jpeg)/pic2419375.jpg"),
            
            new BoardGame("Ticket to Ride", "Bouw spoorlijnen door heel Europa en verzamel punten.",
                GameGenre.Family, GameType.BoardGame, false, 2, 5, 60,
                "https://cf.geekdo-images.com/ZWJg0dCdrWHxVnc0eFXK8w__original/img/HvpnNUv2JbxqJflzSXVZP6l0qVg=/0x0/filters:format(jpeg)/pic38668.jpg"),
            
            new BoardGame("Cards Against Humanity", "Een feestspel voor vreselijke mensen.",
                GameGenre.Party, GameType.CardGame, true, 4, 20, 45, null),
            
            new BoardGame("Pandemic", "Werk samen om de wereld te redden van dodelijke ziektes.",
                GameGenre.Cooperative, GameType.BoardGame, false, 2, 4, 60,
                "https://cf.geekdo-images.com/S3ybV1LAp-8SnHIXLLjVqA__original/img/j-pfXZ_0GmOowOh3r1qmRGvJH10=/0x0/filters:format(jpeg)/pic1534148.jpg"),
            
            new BoardGame("Codenames", "Geef aanwijzingen om je team de juiste woorden te laten raden.",
                GameGenre.WordGame, GameType.CardGame, false, 4, 8, 30, null),
            
            new BoardGame("Secret Hitler", "Een sociaal deductiespel over fascisme in de Weimar Republiek.",
                GameGenre.Party, GameType.CardGame, true, 5, 10, 45, null),
            
            new BoardGame("Azul", "Een prachtig tegellegspel geïnspireerd door Portugese azulejos.",
                GameGenre.Abstract, GameType.TileGame, false, 2, 4, 45,
                "https://cf.geekdo-images.com/tz19PfklMdAdjxV9WArraA__original/img/TVgFB5i_WdWDdMFh-GvIxQv-xgE=/0x0/filters:format(jpeg)/pic3718275.jpg"),
            
            new BoardGame("Yahtzee", "Het klassieke dobbelspel voor het hele gezin.",
                GameGenre.Family, GameType.DiceGame, false, 2, 6, 30, null),
        };

        context.BoardGames.AddRange(boardGames);
        await context.SaveChangesAsync();

        // Get references
        var jan = persons[0];
        var maria = persons[1];
        var ahmed = persons[2];
        var sophie = persons[3];
        var thomas = persons[4];

        var catan = boardGames[0];
        var ticketToRide = boardGames[1];
        var cardsAgainstHumanity = boardGames[2];
        var pandemic = boardGames[3];
        var codenames = boardGames[4];

        // Create game nights
        var gameNight1 = new GameNight(
            jan,
            DateTime.Now.AddDays(7).Date.AddHours(19),
            "Hoofdstraat", "123", "Amsterdam",
            6, false, false,
            DietaryPreference.Vegetarian | DietaryPreference.LactoseFree | DietaryPreference.AlcoholFree);
        
        gameNight1.AddBoardGame(catan);
        gameNight1.AddBoardGame(ticketToRide);

        var gameNight2 = new GameNight(
            maria,
            DateTime.Now.AddDays(14).Date.AddHours(20),
            "Kerkstraat", "45", "Rotterdam",
            8, true, true,
            DietaryPreference.None);
        
        gameNight2.AddBoardGame(cardsAgainstHumanity);
        gameNight2.AddBoardGame(codenames);

        var gameNight3 = new GameNight(
            ahmed,
            DateTime.Now.AddDays(10).Date.AddHours(18).AddMinutes(30),
            "Marktplein", "7", "Utrecht",
            4, false, false,
            DietaryPreference.Halal | DietaryPreference.NutFree | DietaryPreference.AlcoholFree);
        
        gameNight3.AddBoardGame(pandemic);

        context.GameNights.AddRange(gameNight1, gameNight2, gameNight3);
        await context.SaveChangesAsync();

        // Add participations
        gameNight1.AddParticipant(maria);
        gameNight1.AddParticipant(sophie);

        gameNight2.AddParticipant(jan);
        gameNight2.AddParticipant(ahmed);
        gameNight2.AddParticipant(thomas);

        gameNight3.AddParticipant(jan);
        gameNight3.AddParticipant(sophie);

        await context.SaveChangesAsync();

        // Add food items for potluck
        var food1 = new FoodItem("Hummus met groenten", gameNight2, 
            DietaryPreference.Vegetarian | DietaryPreference.LactoseFree | DietaryPreference.NutFree,
            jan, "Zelfgemaakte hummus met wortels en komkommer");
        
        var food2 = new FoodItem("Brownies", gameNight2,
            DietaryPreference.Vegetarian,
            ahmed, "Chocolade brownies (bevat noten!)");

        gameNight2.AddFoodItem(food1);
        gameNight2.AddFoodItem(food2);

        await context.SaveChangesAsync();
    }

    private static Person CreatePerson(
        string name, string email, Gender gender, DateTime dateOfBirth,
        string street, string houseNumber, string city, DietaryPreference dietaryPreferences)
    {
        // Use reflection to bypass private constructor for seeding
        var person = (Person)Activator.CreateInstance(typeof(Person), true)!;
        
        typeof(Person).GetProperty(nameof(Person.Name))!
            .SetValue(person, name);
        typeof(Person).GetProperty(nameof(Person.Email))!
            .SetValue(person, email);
        typeof(Person).GetProperty(nameof(Person.Gender))!
            .SetValue(person, gender);
        typeof(Person).GetProperty(nameof(Person.DateOfBirth))!
            .SetValue(person, dateOfBirth);
        typeof(Person).GetProperty(nameof(Person.Street))!
            .SetValue(person, street);
        typeof(Person).GetProperty(nameof(Person.HouseNumber))!
            .SetValue(person, houseNumber);
        typeof(Person).GetProperty(nameof(Person.City))!
            .SetValue(person, city);
        typeof(Person).GetProperty(nameof(Person.DietaryPreferences))!
            .SetValue(person, dietaryPreferences);

        return person;
    }
}
