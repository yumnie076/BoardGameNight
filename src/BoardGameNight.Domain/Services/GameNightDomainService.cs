using BoardGameNight.Domain.Entities;
using BoardGameNight.Domain.Exceptions;
using BoardGameNight.Domain.Interfaces;

namespace BoardGameNight.Domain.Services;

/// <summary>
/// Domain service for game night related business logic that spans multiple entities.
/// </summary>
public class GameNightDomainService
{
    private readonly IGameNightRepository _gameNightRepository;

    public GameNightDomainService(IGameNightRepository gameNightRepository)
    {
        _gameNightRepository = gameNightRepository;
    }

    /// <summary>
    /// Validates if a person can register for a game night.
    /// Checks business rules across entities.
    /// </summary>
    public async Task<(bool CanRegister, string Reason)> CanPersonRegisterAsync(
        Person person, 
        GameNight gameNight)
    {
        // Check basic eligibility from GameNight entity
        if (!gameNight.CanPersonJoin(person, out string reason))
        {
            return (false, reason);
        }

        // Check if person already has a registration on this date (US_04)
        var hasExistingRegistration = await _gameNightRepository
            .HasRegistrationOnDateAsync(person.Id, gameNight.DateTime.Date);

        if (hasExistingRegistration)
        {
            return (false, "Je kunt je maar voor één spelavond per dag aanmelden.");
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Registers a person for a game night with all validations.
    /// </summary>
    public async Task<GameNightParticipation> RegisterPersonAsync(
        Person person, 
        GameNight gameNight)
    {
        var (canRegister, reason) = await CanPersonRegisterAsync(person, gameNight);
        
        if (!canRegister)
        {
            throw new DomainValidationException(reason);
        }

        return gameNight.AddParticipant(person);
    }

    /// <summary>
    /// Gets dietary warning messages for a person joining a game night.
    /// </summary>
    public IEnumerable<string> GetDietaryWarnings(Person person, GameNight gameNight)
    {
        var warnings = new List<string>();

        if (!person.AreDietaryNeedsMet(gameNight.AvailableDietaryOptions))
        {
            warnings.Add("Let op: het beschikbare eten en drinken sluit mogelijk niet aan op jouw dieetwensen of allergieën.");
        }

        return warnings;
    }
}
