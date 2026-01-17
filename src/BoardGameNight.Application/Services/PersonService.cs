using BoardGameNight.Application.DTOs;
using BoardGameNight.Application.Interfaces;
using BoardGameNight.Domain.Entities;
using BoardGameNight.Domain.Enums;
using BoardGameNight.Domain.Exceptions;
using BoardGameNight.Domain.Interfaces;

namespace BoardGameNight.Application.Services;

/// <summary>
/// Application service for person operations.
/// </summary>
public class PersonService : IPersonService
{
    private readonly IPersonRepository _personRepository;

    public PersonService(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    public async Task<PersonDto?> GetByIdAsync(int id)
    {
        var person = await _personRepository.GetByIdAsync(id);
        return person == null ? null : MapToDto(person);
    }

    public async Task<PersonDto?> GetByEmailAsync(string email)
    {
        var person = await _personRepository.GetByEmailAsync(email);
        return person == null ? null : MapToDto(person);
    }

    public async Task<PersonDto?> GetByIdentityUserIdAsync(string identityUserId)
    {
        var person = await _personRepository.GetByIdentityUserIdAsync(identityUserId);
        return person == null ? null : MapToDto(person);
    }

    public async Task<IEnumerable<PersonDto>> GetAllAsync()
    {
        var persons = await _personRepository.GetAllAsync();
        return persons.Select(MapToDto);
    }

    public async Task<PersonDto> CreateAsync(CreatePersonDto dto)
    {
        // Check if email already exists
        var existing = await _personRepository.GetByEmailAsync(dto.Email);
        if (existing != null)
        {
            throw new DomainValidationException("Er bestaat al een account met dit e-mailadres.");
        }

        var person = new Person(
            dto.Name,
            dto.Email,
            dto.Gender,
            dto.DateOfBirth,
            dto.Street,
            dto.HouseNumber,
            dto.City,
            dto.DietaryPreferences);

        await _personRepository.AddAsync(person);
        return MapToDto(person);
    }

    public async Task<PersonDto> UpdateAsync(int id, CreatePersonDto dto)
    {
        var person = await _personRepository.GetByIdAsync(id)
            ?? throw new DomainValidationException("Persoon niet gevonden.");

        person.UpdateProfile(dto.Name, dto.Street, dto.HouseNumber, dto.City);
        person.UpdateDietaryPreferences(dto.DietaryPreferences);

        await _personRepository.UpdateAsync(person);
        return MapToDto(person);
    }

    public async Task LinkToIdentityUserAsync(int personId, string identityUserId)
    {
        var person = await _personRepository.GetByIdAsync(personId)
            ?? throw new DomainValidationException("Persoon niet gevonden.");

        person.LinkToIdentityUser(identityUserId);
        await _personRepository.UpdateAsync(person);
    }

    public async Task UpdateDietaryPreferencesAsync(int personId, DietaryPreference preferences)
    {
        var person = await _personRepository.GetByIdAsync(personId)
            ?? throw new DomainValidationException("Persoon niet gevonden.");

        person.UpdateDietaryPreferences(preferences);
        await _personRepository.UpdateAsync(person);
    }

    public async Task<int> GetShowCountAsync(int personId)
    {
        return await _personRepository.GetShowCountAsync(personId);
    }

    public async Task<int> GetNoShowCountAsync(int personId)
    {
        return await _personRepository.GetNoShowCountAsync(personId);
    }

    public async Task<double?> GetAverageOrganizerRatingAsync(int personId)
    {
        return await _personRepository.GetAverageOrganizerRatingAsync(personId);
    }

    public async Task<int> GetOrganizedGameNightCountAsync(int personId)
    {
        return await _personRepository.GetOrganizedGameNightCountAsync(personId);
    }

    private static PersonDto MapToDto(Person person)
    {
        return new PersonDto
        {
            Id = person.Id,
            Name = person.Name,
            Email = person.Email,
            Gender = person.Gender,
            DateOfBirth = person.DateOfBirth,
            Age = person.Age,
            IsAdult = person.IsAdult,
            Street = person.Street,
            HouseNumber = person.HouseNumber,
            City = person.City,
            FullAddress = person.FullAddress,
            DietaryPreferences = person.DietaryPreferences
        };
    }
}
