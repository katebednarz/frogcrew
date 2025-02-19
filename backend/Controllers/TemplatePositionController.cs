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
            return new ObjectResult(new Result(false, 409, "position already exists")) { StatusCode = 409 };
        

        var newPosition = new Position
        {
            PositionName = position.Name,
        };
        
        _context.Positions.Add(newPosition);
        await _context.SaveChangesAsync();

        return Ok(new Result(true, 200, "Add Success", _converters.PositionToDto(newPosition)));
    }

    [HttpPut("positions/{positionId}")]
    public async Task<IActionResult> UpdatePosition([FromBody] PositionDTO position, int positionId)
    {
        var foundPosition = await _context.Positions.FindAsync(positionId);
        if (foundPosition is null)
            return new ObjectResult(new Result(false, 404, $"Could not find position with id {positionId}")) { StatusCode = 400 };
        
        if (position.Name is null)
        {
            return new ObjectResult(new Result(false, 400, "Provided arguments are invalid, see data for details.", "position is required")) { StatusCode = 400 };
        }
        
        var x = _dbHelper.GetPositionIdByName(position.Name);
        Console.WriteLine(x);
        if (x > 0)
            return new ObjectResult(new Result(false, 409, "position already exists")) { StatusCode = 409 };
        
        foundPosition.PositionName = position.Name;
        _context.Positions.Update(foundPosition);
        await _context.SaveChangesAsync();
        return Ok(new Result(true, 200, "Update Success", _converters.PositionToDto(foundPosition)));
    }
    
    [HttpGet("template")]
    public async Task<IActionResult> GetTemplates()
    {
        var templates = await _context.Templates
            .Select(template => new 
            {
                template.TemplateId,
                template.TemplateName
            })
            .ToListAsync();

        if (templates.Any())
        {
            return Ok(new Result(true, 200, "Find Success", templates));
        }
        return new ObjectResult(new Result(false, 404, "Could not find any templates")) { StatusCode = 404 };
    }
    
    // [HttpPost("positions")]
    // public async Task<IActionResult> AddTemplate([FromBody] TemplateDTO template)
    // {
    //     // fix this
    //     if (template.Name is null)
    //     {
    //         return new ObjectResult(new Result(false, 400, "Provided arguments are invalid, see data for details.", "position is required")) { StatusCode = 400 };
    //     }
    //     
    //     var x = _dbHelper.GetTemplateIdByName(template.Name);
    //     if (x > 0)
    //         return new ObjectResult(new Result(false, 409, "position already exists")) { StatusCode = 409 };
    //     
    //
    //     var newTemplate = new Template
    //     {
    //         TemplateName = template.Name,
    //     };
    //     
    //     _context.Templates.Add(newTemplate);
    //     await _context.SaveChangesAsync();
    //
    //     return Ok(new Result(true, 200, "Add Success", _converters.TemplateToDTO(newTemplate)));
    // }
    
}
    
    