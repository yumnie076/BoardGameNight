using BoardGameNight.Application.DTOs;
using BoardGameNight.Application.Interfaces;

namespace BoardGameNight.API.GraphQL;

public class Query
{
    public async Task<IEnumerable<GameNightListDto>> GetGameNights(
        [Service] IGameNightService gameNightService)
    {
        return await gameNightService.GetAllUpcomingAsync();
    }

    public async Task<GameNightDto?> GetGameNight(
        int id,
        [Service] IGameNightService gameNightService)
    {
        return await gameNightService.GetWithDetailsAsync(id);
    }

    public async Task<IEnumerable<BoardGameDto>> GetBoardGames(
        [Service] IBoardGameService boardGameService)
    {
        return await boardGameService.GetAllAsync();
    }

    public async Task<BoardGameDto?> GetBoardGame(
        int id,
        [Service] IBoardGameService boardGameService)
    {
        return await boardGameService.GetByIdAsync(id);
    }
}

public class Mutation
{
    public async Task<bool> RegisterForGameNight(
        int personId,
        int gameNightId,
        [Service] IGameNightService gameNightService)
    {
        var (success, _, _) = await gameNightService.RegisterParticipantAsync(personId, gameNightId);
        return success;
    }

    public async Task<bool> UnregisterFromGameNight(
        int personId,
        int gameNightId,
        [Service] IGameNightService gameNightService)
    {
        await gameNightService.UnregisterParticipantAsync(personId, gameNightId);
        return true;
    }
}
