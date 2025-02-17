using backend.DTO;
using backend.Models;

namespace backend.Utils;

public class DtoConverters
{
    private readonly FrogcrewContext _context;
    private readonly DatabaseHelper _dbHelper;

    public DtoConverters(FrogcrewContext context)
    {
        _context = context;
        _dbHelper = new DatabaseHelper(context);
    }
    
    public TradeBoardDTO TradeBoardToDto(TradeBoard tradeBoard)
    {
        return new TradeBoardDTO
        {
            TradeId = tradeBoard.TradeId,
            DropperId = tradeBoard.DropperId,
            GameId = tradeBoard.GameId,
            Position = _dbHelper.GetPositionNameById(tradeBoard.Position)!,
            Status = tradeBoard.Status,
            ReceiverId = tradeBoard.ReceiverId
        };
    }
    
    public UserSimpleDTO UserToUserSimpleDto(ApplicationUser user)
    {
        return new UserSimpleDTO
        {
            UserId = user.Id,
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
        };
    }

    public AvailabilityDTO AvailabilityToDto(Availability availability)
    {
        return new AvailabilityDTO
        {
            UserId = availability.UserId,
            GameId = availability.GameId,
            Available = availability.Available,
            Comments = availability.Comments
        };
    }

    public CrewedUserDTO CrewedUserToDto(CrewedUser crewedUser)
    {
        return new CrewedUserDTO
        {
            UserId = crewedUser.UserId,
            GameId = crewedUser.GameId,
            Position = _dbHelper.GetPositionNameById(crewedUser.PositionId)!,
        };
    }

    public GameDTO GameToGameDto(Game game)
    {
        return new GameDTO
        {
            GameId = game.Id,
            ScheduleId = game.ScheduleId,
            GameDate = game.GameDate,
            Venue = game.Venue,
            Opponent = game.Opponent,
            IsFinalized = game.IsFinalized,
        };
    }

    public CrewListDTO GameToCrewListDto(Game game)
    {
        return new CrewListDTO
        {
            GameId = game.Id,
            GameStart = game.GameStart,
            GameDate = game.GameDate,
            Venue = game.Venue,
            Opponent = game.Opponent,
            CrewedMembers = CrewedUsersToDtoList(game.Id)
        };
    }

    private List<CrewedUserDTO> CrewedUsersToDtoList(int gameId)
    {
        var crewedUserList = _dbHelper.GetCrewedUsersByGame(gameId);
        return crewedUserList.Select(CrewedUserToDto).ToList();
    }

    public GameScheduleDTO ScheduleToGameScheduleDto(Schedule schedule)
    {
        return new GameScheduleDTO
        {
            Id = schedule.Id,
            Sport = schedule.Sport,
            Season = schedule.Season,
        };
    }

    public PositionDTO PositionToDto(Position position)
    {
        return new PositionDTO
        {
            Id = position.PositionId,
            Name = position.PositionName,
        };
    }
    
}