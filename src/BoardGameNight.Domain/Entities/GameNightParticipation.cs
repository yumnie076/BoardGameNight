namespace BoardGameNight.Domain.Entities;

/// <summary>
/// Represents a person's participation in a game night.
/// Tracks attendance (show/no-show) for US_09.
/// </summary>
public class GameNightParticipation
{
    public int Id { get; private set; }
    
    public int GameNightId { get; private set; }
    
    public GameNight GameNight { get; private set; } = null!;
    
    public int PersonId { get; private set; }
    
    public Person Person { get; private set; } = null!;
    
    public DateTime RegistrationDate { get; private set; }
    
    /// <summary>
    /// Indicates if the person actually attended (null = not yet recorded).
    /// </summary>
    public bool? DidAttend { get; private set; }

    // For EF Core
    private GameNightParticipation() { }

    public GameNightParticipation(GameNight gameNight, Person person)
    {
        GameNight = gameNight ?? throw new ArgumentNullException(nameof(gameNight));
        GameNightId = gameNight.Id;
        Person = person ?? throw new ArgumentNullException(nameof(person));
        PersonId = person.Id;
        RegistrationDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Records whether the participant attended the game night.
    /// </summary>
    public void RecordAttendance(bool didAttend)
    {
        DidAttend = didAttend;
    }

    /// <summary>
    /// Gets whether this is a no-show (registered but didn't attend).
    /// </summary>
    public bool IsNoShow => DidAttend == false;
}
