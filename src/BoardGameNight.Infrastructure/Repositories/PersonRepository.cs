using BoardGameNight.Domain.Entities;
using BoardGameNight.Domain.Interfaces;
using BoardGameNight.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoardGameNight.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Person entities.
/// </summary>
public class PersonRepository : Repository<Person>, IPersonRepository
{
    public PersonRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Person?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Email == email);
    }

    public async Task<Person?> GetByIdentityUserIdAsync(string identityUserId)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.IdentityUserId == identityUserId);
    }

    public async Task<Person?> GetWithOrganizedGameNightsAsync(int personId)
    {
        return await _dbSet
            .Include(p => p.OrganizedGameNights)
                .ThenInclude(gn => gn.BoardGames)
                    .ThenInclude(gnbg => gnbg.BoardGame)
            .Include(p => p.OrganizedGameNights)
                .ThenInclude(gn => gn.Participants)
            .FirstOrDefaultAsync(p => p.Id == personId);
    }

    public async Task<Person?> GetWithParticipationsAsync(int personId)
    {
        return await _dbSet
            .Include(p => p.Participations)
                .ThenInclude(gnp => gnp.GameNight)
                    .ThenInclude(gn => gn.Organizer)
            .Include(p => p.Participations)
                .ThenInclude(gnp => gnp.GameNight)
                    .ThenInclude(gn => gn.BoardGames)
                        .ThenInclude(gnbg => gnbg.BoardGame)
            .FirstOrDefaultAsync(p => p.Id == personId);
    }

    public async Task<int> GetShowCountAsync(int personId)
    {
        return await _context.GameNightParticipations
            .Where(gnp => gnp.PersonId == personId && gnp.DidAttend == true)
            .CountAsync();
    }

    public async Task<int> GetNoShowCountAsync(int personId)
    {
        return await _context.GameNightParticipations
            .Where(gnp => gnp.PersonId == personId && gnp.DidAttend == false)
            .CountAsync();
    }

    public async Task<double?> GetAverageOrganizerRatingAsync(int personId)
    {
        var ratings = await _context.Reviews
            .Where(r => r.GameNight.OrganizerId == personId)
            .Select(r => r.Rating)
            .ToListAsync();

        return ratings.Any() ? ratings.Average() : null;
    }

    public async Task<int> GetOrganizedGameNightCountAsync(int personId)
    {
        return await _context.GameNights
            .Where(gn => gn.OrganizerId == personId)
            .CountAsync();
    }
}
