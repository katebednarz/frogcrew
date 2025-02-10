using backend.Models;
using Microsoft.AspNetCore.Mvc;
using backend.Utils;
using backend.DTO;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[Route("/")]
[ApiController]
public class TemplatePositionController : Controller
{
    private readonly FrogcrewContext _context;
    private readonly DatabaseHelper _dbHelper;
    private readonly DtoConverters _converters;

    public TemplatePositionController(FrogcrewContext context)
    {
        _context = context;
        _dbHelper = new DatabaseHelper(context);
        _converters = new DtoConverters(context);
    }

    [HttpGet("positions")]
    public async Task<IActionResult> GetPositions()
    {
        var positions = await _context.Positions.Select(position => position.PositionName).ToListAsync();
        return Ok(new Result(true, 200, "Find Success", positions));
    }

    [HttpPost("positions")]
    public async Task<IActionResult> AddPosition([FromBody] PositionDTO position)
    {
        if (position.Name is null)
        {
            return new ObjectResult(new Result(false, 400, "Provided arguments are invalid, see data for details.", "position is required")) { StatusCode = 400 };
        }
        
        var x = _dbHelper.GetPositionIdByName(position.Name);
        Console.WriteLine(x);
        if (x > 0)
            return new ObjectResult(new Result(false, 409, "Position already exists")) { StatusCode = 409 };
        

        var newPosition = new Position
        {
            PositionName = position.Name,
        };
        
        _context.Positions.Add(newPosition);
        await _context.SaveChangesAsync();

        return Ok(new Result(true, 200, "Add Success", _converters.PositionToDto(newPosition)));
    }
    
    
}
    
    