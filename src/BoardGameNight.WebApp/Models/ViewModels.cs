using System.ComponentModel.DataAnnotations;
using BoardGameNight.Application.DTOs;
using BoardGameNight.Domain.Enums;

namespace BoardGameNight.WebApp.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}

public class CreateGameNightViewModel
{
    [Required(ErrorMessage = "Datum en tijd is verplicht")]
    [Display(Name = "Datum en tijd")]
    public DateTime DateTime { get; set; }

    [Required(ErrorMessage = "Straat is verplicht")]
    [Display(Name = "Straat")]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Huisnummer is verplicht")]
    [Display(Name = "Huisnummer")]
    public string HouseNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Stad is verplicht")]
    [Display(Name = "Stad")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Maximaal aantal spelers is verplicht")]
    [Range(2, 50, ErrorMessage = "Maximaal aantal spelers moet tussen 2 en 50 zijn")]
    [Display(Name = "Maximaal aantal spelers")]
    public int MaxPlayers { get; set; } = 6;

    [Display(Name = "Alleen voor volwassenen (18+)")]
    public bool IsAdultOnly { get; set; }

    [Display(Name = "Potluck (iedereen neemt eten mee)")]
    public bool IsPotluck { get; set; }

    [Display(Name = "Beschikbare dieetopties")]
    public DietaryPreference AvailableDietaryOptions { get; set; }

    [Display(Name = "Bordspellen")]
    public List<int>? SelectedBoardGameIds { get; set; }

    public List<BoardGameDto> AvailableBoardGames { get; set; } = new();
}

public class EditGameNightViewModel : CreateGameNightViewModel
{
    public int Id { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "E-mailadres is verplicht")]
    [EmailAddress(ErrorMessage = "Ongeldig e-mailadres")]
    [Display(Name = "E-mailadres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Wachtwoord is verplicht")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Wachtwoord moet minimaal 6 karakters zijn")]
    [DataType(DataType.Password)]
    [Display(Name = "Wachtwoord")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Bevestig wachtwoord")]
    [Compare("Password", ErrorMessage = "Wachtwoorden komen niet overeen")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Naam is verplicht")]
    [Display(Name = "Naam")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Geslacht is verplicht")]
    [Display(Name = "Geslacht")]
    public Gender Gender { get; set; }

    [Required(ErrorMessage = "Geboortedatum is verplicht")]
    [DataType(DataType.Date)]
    [Display(Name = "Geboortedatum")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Straat is verplicht")]
    [Display(Name = "Straat")]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Huisnummer is verplicht")]
    [Display(Name = "Huisnummer")]
    public string HouseNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Stad is verplicht")]
    [Display(Name = "Stad")]
    public string City { get; set; } = string.Empty;

    [Display(Name = "Dieetwensen / Allergieën")]
    public DietaryPreference DietaryPreferences { get; set; }
}

public class LoginViewModel
{
    [Required(ErrorMessage = "E-mailadres is verplicht")]
    [EmailAddress(ErrorMessage = "Ongeldig e-mailadres")]
    [Display(Name = "E-mailadres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Wachtwoord is verplicht")]
    [DataType(DataType.Password)]
    [Display(Name = "Wachtwoord")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Onthoud mij")]
    public bool RememberMe { get; set; }
}
