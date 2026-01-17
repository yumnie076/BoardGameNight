namespace BoardGameNight.Domain.Entities;

/// <summary>
/// Junction entity for the many-to-many relationship between GameNight and BoardGame.
/// </summary>
public class GameNightBoardGame
{
    public int GameNightId { get; private set; }
    
    public GameNight GameNight { get; private set; } = null!;
    
    public int BoardGameId { get; private set; }
    
    public BoardGame BoardGame { get; private set; } = null!;

    // For EF Core
    private GameNightBoardGame() { }

    public GameNightBoardGame(GameNight gameNight, BoardGame boardGame)
    {
        GameNight = gameNight ?? throw new ArgumentNullException(nameof(gameNight));
        GameNightId = gameNight.Id;
        BoardGame = boardGame ?? throw new ArgumentNullException(nameof(boardGame));
        BoardGameId = boardGame.Id;
    }
}
