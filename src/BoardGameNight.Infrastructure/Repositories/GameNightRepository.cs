using BoardGameNight.Domain.Entities;
using BoardGameNight.Domain.Interfaces;
using BoardGameNight.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoardGameNight.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for GameNight entities.
/// </summary>
public class GameNightRepository : Repository<GameNight>, IGameNightRepository
{
    public GameNightRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<GameNight>> GetUpcomingAsync()
    {
        return await _dbSet
            .Include(gn => gn.Organizer)
            .Include(gn => gn.BoardGames)
                .ThenInclude(gnbg => gnbg.BoardGame)
            .Include(gn => gn.Participants)
            .Where(gn => gn.DateTime > DateTime.Now)
            .OrderBy(gn => gn.DateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<GameNight>> GetByOrganizerAsync(int organizerId)
    {
        return await _dbSet
            .Include(gn => gn.Organizer)
            .Include(gn => gn.BoardGames)
                .ThenInclude(gnbg => gnbg.BoardGame)
            .Include(gn => gn.Participants)
            .Where(gn => gn.OrganizerId == organizerId)
            .OrderByDescending(gn => gn.DateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<GameNight>> GetByParticipantAsync(int personId)
    {
        return await _dbSet
            .Include(gn => gn.Organizer)
            .Include(gn => gn.BoardGames)
                .ThenInclude(gnbg => gnbg.BoardGame)
            .Include(gn => gn.Participants)
            .Where(gn => gn.Participants.Any(p => p.PersonId == personId))
            .OrderByDescending(gn => gn.DateTime)
            .ToListAsync();
    }

    public async Task<GameNight?> GetWithDetailsAsync(int gameNightId)
    {
        return await _dbSet
            .Include(gn => gn.Organizer)
            .Include(gn => gn.BoardGames)
                .ThenInclude(gnbg => gnbg.BoardGame)
            .Include(gn => gn.Participants)
                .ThenInclude(p => p.Person)
            .Include(gn => gn.FoodItems)
                .ThenInclude(fi => fi.BroughtByPerson)
            .Include(gn => gn.Reviews)
                .ThenInclude(r => r.Reviewer)
            .FirstOrDefaultAsync(gn => gn.Id == gameNightId);
    }

    public async Task<bool> HasRegistrationOnDateAsync(int personId, DateTime date)
    {
        return await _dbSet
            .AnyAsync(gn => 
                gn.Participants.Any(p => p.PersonId == personId) &&
                gn.DateTime.Date == date.Date);
    }

    public async Task<IEnumerable<GameNight>> GetPastByOrganizerAsync(int organizerId)
    {
        return await _dbSet
            .Include(gn => gn.Organizer)
            .Include(gn => gn.Reviews)
            .Where(gn => gn.OrganizerId == organizerId && gn.DateTime < DateTime.Now)
            .OrderByDescending(gn => gn.DateTime)
            .ToListAsync();
    }
}
