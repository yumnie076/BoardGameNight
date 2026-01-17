using BoardGameNight.Domain.Exceptions;

namespace BoardGameNight.Domain.Entities;

/// <summary>
/// Represents a review for a game night (US_08).
/// </summary>
public class Review
{
    public int Id { get; private set; }
    
    /// <summary>
    /// Rating from 1 to 5.
    /// </summary>
    public int Rating { get; private set; }
    
    public string ReviewText { get; private set; } = string.Empty;
    
    public DateTime CreatedAt { get; private set; }
    
    public int GameNightId { get; private set; }
    
    public GameNight GameNight { get; private set; } = null!;
    
    public int ReviewerId { get; private set; }
    
    public Person Reviewer { get; private set; } = null!;

    // For EF Core
    private Review() { }

    public Review(GameNight gameNight, Person reviewer, int rating, string reviewText)
    {
        ValidateRating(rating);

        GameNight = gameNight ?? throw new ArgumentNullException(nameof(gameNight));
        GameNightId = gameNight.Id;
        Reviewer = reviewer ?? throw new ArgumentNullException(nameof(reviewer));
        ReviewerId = reviewer.Id;
        Rating = rating;
        ReviewText = reviewText ?? string.Empty;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the review.
    /// </summary>
    public void Update(int rating, string reviewText)
    {
        ValidateRating(rating);
        Rating = rating;
        ReviewText = reviewText ?? string.Empty;
    }

    private static void ValidateRating(int rating)
    {
        if (rating < 1 || rating > 5)
        {
            throw new DomainValidationException("Rating moet tussen 1 en 5 zijn.");
        }
    }
}
