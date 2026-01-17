using Microsoft.AspNetCore.Mvc;
using BoardGameNight.Application.Interfaces;
using BoardGameNight.Application.DTOs;
using BoardGameNight.Domain.Exceptions;

namespace BoardGameNight.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameNightsController : ControllerBase
{
    private readonly IGameNightService _gameNightService;

    public GameNightsController(IGameNightService gameNightService)
    {
        _gameNightService = gameNightService;
    }

    // GET: api/gamenights
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameNightListDto>>> GetAll()
    {
        var gameNights = await _gameNightService.GetAllUpcomingAsync();
        return Ok(gameNights);
    }

    // GET: api/gamenights/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GameNightDto>> GetById(int id)
    {
        var gameNight = await _gameNightService.GetWithDetailsAsync(id);
        if (gameNight == null)
            return NotFound();
        return Ok(gameNight);
    }

    // POST: api/gamenights
    [HttpPost]
    public async Task<ActionResult<GameNightDto>> Create([FromBody] CreateGameNightRequest request)
    {
        try
        {
            var gameNight = await _gameNightService.CreateAsync(request.OrganizerId, request.GameNight);
            return CreatedAtAction(nameof(GetById), new { id = gameNight.Id }, gameNight);
        }
        catch (DomainValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // PUT: api/gamenights/5
    [HttpPut("{id}")]
    public async Task<ActionResult<GameNightDto>> Update(int id, [FromBody] UpdateGameNightRequest request)
    {
        if (id != request.GameNight.Id)
            return BadRequest(new { error = "ID mismatch" });

        try
        {
            var gameNight = await _gameNightService.UpdateAsync(request.OrganizerId, request.GameNight);
            return Ok(gameNight);
        }
        catch (DomainValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // DELETE: api/gamenights/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, [FromQuery] int organizerId)
    {
        try
        {
            await _gameNightService.DeleteAsync(organizerId, id);
            return NoContent();
        }
        catch (DomainValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // POST: api/gamenights/5/register
    [HttpPost("{id}/register")]
    public async Task<IActionResult> Register(int id, [FromBody] GameNightRegisterRequest request)
    {
        var (success, message, warnings) = await _gameNightService.RegisterParticipantAsync(request.PersonId, id);
        if (!success)
            return BadRequest(new { error = message });
        return Ok(new { message, warnings });
    }

    // DELETE: api/gamenights/5/register
    [HttpDelete("{id}/register")]
    public async Task<IActionResult> Unregister(int id, [FromQuery] int personId)
    {
        await _gameNightService.UnregisterParticipantAsync(personId, id);
        return NoContent();
    }
}

public class CreateGameNightRequest
{
    public int OrganizerId { get; set; }
    public CreateGameNightDto GameNight { get; set; } = null!;
}

public class UpdateGameNightRequest
{
    public int OrganizerId { get; set; }
    public UpdateGameNightDto GameNight { get; set; } = null!;
}

public class GameNightRegisterRequest
{
    public int PersonId { get; set; }
}
