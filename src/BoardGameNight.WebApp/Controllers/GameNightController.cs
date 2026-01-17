using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BoardGameNight.Application.Interfaces;
using BoardGameNight.Application.DTOs;
using BoardGameNight.Domain.Exceptions;
using BoardGameNight.WebApp.Models;

namespace BoardGameNight.WebApp.Controllers;

public class GameNightController : Controller
{
    private readonly IGameNightService _gameNightService;
    private readonly IPersonService _personService;
    private readonly IBoardGameService _boardGameService;

    public GameNightController(
        IGameNightService gameNightService,
        IPersonService personService,
        IBoardGameService boardGameService)
    {
        _gameNightService = gameNightService;
        _personService = personService;
        _boardGameService = boardGameService;
    }

    // US_01: Alle bordspellenavonden
    public async Task<IActionResult> Index()
    {
        var gameNights = await _gameNightService.GetAllUpcomingAsync();
        return View(gameNights);
    }

    // US_01: Georganiseerde bordspellenavonden
    [Authorize]
    public async Task<IActionResult> MyOrganized()
    {
        var person = await GetCurrentPersonAsync();
        if (person == null) return RedirectToAction("Login", "Account");

        var gameNights = await _gameNightService.GetByOrganizerAsync(person.Id);
        return View(gameNights);
    }

    // US_01: Deelgenomen bordspellenavonden
    [Authorize]
    public async Task<IActionResult> MyParticipations()
    {
        var person = await GetCurrentPersonAsync();
        if (person == null) return RedirectToAction("Login", "Account");

        var gameNights = await _gameNightService.GetByParticipantAsync(person.Id);
        return View(gameNights);
    }

    // Details view
    public async Task<IActionResult> Details(int id)
    {
        var gameNight = await _gameNightService.GetWithDetailsAsync(id);
        if (gameNight == null)
        {
            return NotFound();
        }

        var person = await GetCurrentPersonAsync();
        ViewBag.CurrentPersonId = person?.Id;
        ViewBag.CanJoin = person != null && await _gameNightService.CanPersonJoinAsync(person.Id, id);

        return View(gameNight);
    }

    // US_02: Create GET
    [Authorize]
    public async Task<IActionResult> Create()
    {
        var person = await GetCurrentPersonAsync();
        if (person == null) return RedirectToAction("Login", "Account");

        if (!person.IsAdult)
        {
            TempData["Error"] = "Je moet minimaal 18 jaar oud zijn om een bordspellenavond te organiseren.";
            return RedirectToAction("Index");
        }

        var boardGames = await _boardGameService.GetAllAsync();
        var viewModel = new CreateGameNightViewModel
        {
            DateTime = DateTime.Now.AddDays(7).Date.AddHours(19),
            MaxPlayers = 6,
            AvailableBoardGames = boardGames.ToList()
        };

        return View(viewModel);
    }

    // US_02: Create POST
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateGameNightViewModel model)
    {
        var person = await GetCurrentPersonAsync();
        if (person == null) return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
        {
            model.AvailableBoardGames = (await _boardGameService.GetAllAsync()).ToList();
            return View(model);
        }

        try
        {
            var dto = new CreateGameNightDto
            {
                DateTime = model.DateTime,
                Street = model.Street,
                HouseNumber = model.HouseNumber,
                City = model.City,
                MaxPlayers = model.MaxPlayers,
                IsAdultOnly = model.IsAdultOnly,
                IsPotluck = model.IsPotluck,
                AvailableDietaryOptions = model.AvailableDietaryOptions,
                BoardGameIds = model.SelectedBoardGameIds ?? new List<int>()
            };

            var gameNight = await _gameNightService.CreateAsync(person.Id, dto);
            TempData["Success"] = "Bordspellenavond succesvol aangemaakt!";
            return RedirectToAction("Details", new { id = gameNight.Id });
        }
        catch (DomainValidationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            model.AvailableBoardGames = (await _boardGameService.GetAllAsync()).ToList();
            return View(model);
        }
    }

    // US_02: Edit GET
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var person = await GetCurrentPersonAsync();
        if (person == null) return RedirectToAction("Login", "Account");

        var gameNight = await _gameNightService.GetWithDetailsAsync(id);
        if (gameNight == null) return NotFound();

        if (gameNight.OrganizerId != person.Id)
        {
            TempData["Error"] = "Je bent niet de organisator van deze bordspellenavond.";
            return RedirectToAction("Index");
        }

        if (gameNight.Participants.Any())
        {
            TempData["Error"] = "Je kunt deze bordspellenavond niet wijzigen omdat er al spelers zijn ingeschreven.";
            return RedirectToAction("Details", new { id });
        }

        var boardGames = await _boardGameService.GetAllAsync();
        var viewModel = new EditGameNightViewModel
        {
            Id = gameNight.Id,
            DateTime = gameNight.DateTime,
            Street = gameNight.Street,
            HouseNumber = gameNight.HouseNumber,
            City = gameNight.City,
            MaxPlayers = gameNight.MaxPlayers,
            IsAdultOnly = gameNight.IsAdultOnly,
            IsPotluck = gameNight.IsPotluck,
            AvailableDietaryOptions = gameNight.AvailableDietaryOptions,
            SelectedBoardGameIds = gameNight.BoardGames.Select(bg => bg.Id).ToList(),
            AvailableBoardGames = boardGames.ToList()
        };

        return View(viewModel);
    }

    // US_02: Edit POST
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditGameNightViewModel model)
    {
        var person = await GetCurrentPersonAsync();
        if (person == null) return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
        {
            model.AvailableBoardGames = (await _boardGameService.GetAllAsync()).ToList();
            return View(model);
        }

        try
        {
            var dto = new UpdateGameNightDto
            {
                Id = model.Id,
                DateTime = model.DateTime,
                Street = model.Street,
                HouseNumber = model.HouseNumber,
                City = model.City,
                MaxPlayers = model.MaxPlayers,
                IsAdultOnly = model.IsAdultOnly,
                IsPotluck = model.IsPotluck,
                AvailableDietaryOptions = model.AvailableDietaryOptions,
                BoardGameIds = model.SelectedBoardGameIds ?? new List<int>()
            };

            await _gameNightService.UpdateAsync(person.Id, dto);
            TempData["Success"] = "Bordspellenavond succesvol bijgewerkt!";
            return RedirectToAction("Details", new { id = model.Id });
        }
        catch (DomainValidationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            model.AvailableBoardGames = (await _boardGameService.GetAllAsync()).ToList();
            return View(model);
        }
    }

    // US_02: Delete GET
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var person = await GetCurrentPersonAsync();
        if (person == null) return RedirectToAction("Login", "Account");

        var gameNight = await _gameNightService.GetWithDetailsAsync(id);
        if (gameNight == null) return NotFound();

        if (gameNight.OrganizerId != person.Id)
        {
            TempData["Error"] = "Je bent niet de organisator van deze bordspellenavond.";
            return RedirectToAction("Index");
        }

        return View(gameNight);
    }

    // US_02: Delete POST
    [HttpPost, ActionName("Delete")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var person = await GetCurrentPersonAsync();
        if (person == null) return RedirectToAction("Login", "Account");

        try
        {
            await _gameNightService.DeleteAsync(person.Id, id);
            TempData["Success"] = "Bordspellenavond succesvol verwijderd!";
            return RedirectToAction("MyOrganized");
        }
        catch (DomainValidationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Details", new { id });
        }
    }

    // US_04: Register for game night
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(int id)
    {
        var person = await GetCurrentPersonAsync();
        if (person == null) return RedirectToAction("Login", "Account");

        var (success, message, warnings) = await _gameNightService.RegisterParticipantAsync(person.Id, id);

        if (success)
        {
            TempData["Success"] = message;
            if (warnings.Any())
            {
                TempData["Warnings"] = string.Join(" ", warnings);
            }
        }
        else
        {
            TempData["Error"] = message;
        }

        return RedirectToAction("Details", new { id });
    }

    // US_04: Unregister from game night
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unregister(int id)
    {
        var person = await GetCurrentPersonAsync();
        if (person == null) return RedirectToAction("Login", "Account");

        await _gameNightService.UnregisterParticipantAsync(person.Id, id);
        TempData["Success"] = "Je bent uitgeschreven van de bordspellenavond.";

        return RedirectToAction("Details", new { id });
    }

    // US_07: Add food item (potluck)
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFoodItem(CreateFoodItemDto dto)
    {
        var person = await GetCurrentPersonAsync();
        if (person == null) return RedirectToAction("Login", "Account");

        try
        {
            await _gameNightService.AddFoodItemAsync(person.Id, dto);
            TempData["Success"] = "Eten/drinken succesvol toegevoegd!";
        }
        catch (DomainValidationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Details", new { id = dto.GameNightId });
    }

    // US_08: Add review
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(CreateReviewDto dto)
    {
        var person = await GetCurrentPersonAsync();
        if (person == null) return RedirectToAction("Login", "Account");

        try
        {
            await _gameNightService.AddReviewAsync(person.Id, dto);
            TempData["Success"] = "Review succesvol toegevoegd!";
        }
        catch (DomainValidationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Details", new { id = dto.GameNightId });
    }

    // US_09: Record attendance
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordAttendance(RecordAttendanceDto dto)
    {
        var person = await GetCurrentPersonAsync();
        if (person == null) return RedirectToAction("Login", "Account");

        try
        {
            await _gameNightService.RecordAttendanceAsync(person.Id, dto);
            TempData["Success"] = "Aanwezigheid geregistreerd!";
        }
        catch (DomainValidationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Details", new { id = dto.GameNightId });
    }

    private async Task<PersonDto?> GetCurrentPersonAsync()
    {
        if (!User.Identity?.IsAuthenticated ?? true) return null;

        var identityUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(identityUserId)) return null;

        return await _personService.GetByIdentityUserIdAsync(identityUserId);
    }
}
