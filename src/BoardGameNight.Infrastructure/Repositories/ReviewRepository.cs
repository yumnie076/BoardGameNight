using BoardGameNight.Domain.Entities;
using BoardGameNight.Domain.Interfaces;
using BoardGameNight.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoardGameNight.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Review entities.
/// </summary>
public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Review>> GetByGameNightAsync(int gameNightId)
    {
        return await _dbSet
            .Include(r => r.Reviewer)
            .Where(r => r.GameNightId == gameNightId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetByOrganizerAsync(int organizerId)
    {
        return await _dbSet
            .Include(r => r.Reviewer)
            .Include(r => r.GameNight)
            .Where(r => r.GameNight.OrganizerId == organizerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review?> GetByReviewerAndGameNightAsync(int reviewerId, int gameNightId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.ReviewerId == reviewerId && r.GameNightId == gameNightId);
    }
}
