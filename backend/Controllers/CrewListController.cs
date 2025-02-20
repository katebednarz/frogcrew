using System.Text;
using backend.DTO;
using backend.Models;
using backend.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("/")]
    [ApiController]
    public class CrewListController : Controller
    {
        
        private readonly FrogcrewContext _context;
        private readonly DtoConverters _converters;
        private readonly DatabaseHelper _dbHelper;

        public CrewListController(FrogcrewContext context)
        {
        _context = context;
        _converters = new DtoConverters(_context);
        _dbHelper = new DatabaseHelper(_context);
        }

        /*
            * Finds a crew list by game ID
            * 
            * @param gameId The ID of the game
            * @return The result of the operation
        */
        [HttpGet("crewList/{gameId}")]
        public async Task<IActionResult> FindCrewListById(int gameId) {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null) {
                return new ObjectResult(new Result(false, 404, $"Game with ID {gameId} not found.")) { StatusCode = 404 };
            }

            return Ok(new Result(true, 200, "Find Success", _converters.GameToCrewListDto(game)));
        }

        [HttpGet("crewList/export/{gameId}")]
        public async Task<IActionResult> ExportCrewList(int gameId)
        {
            
            var game = await _context.Games.Include(g => g.Schedule).FirstOrDefaultAsync(g => g.Id == gameId);
            if (game == null) {
                return new ObjectResult(new Result(false, 404, $"Game with ID {gameId} not found.")) { StatusCode = 404 };
            }

            var gameTitle = $"TCU {game.Schedule.Sport.ToUpper()} vs {game.Opponent}";
            var dayOfWeek = game.GameDate.Value.DayOfWeek.ToString();
            var formattedDate = game.GameDate?.ToString("MM-dd-yy");
            var date = $"{dayOfWeek} {formattedDate}";
            var eventType = "ESPN+";

            var crewData = new List<(string Position, string Name, string ReportTime, string ReportLocation)>();

            var crewedMembers = _dbHelper.GetCrewedUsersByGame(game.Id);
            
            foreach (var crewedMember in crewedMembers)
            {
                var position = crewedMember.CrewedPositionNavigation.PositionName;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == crewedMember.UserId);
                var name = $"{user.FirstName} {user.LastName}";
                var reportTime = crewedMember.ArrivalTime?.ToString("HH:mm");
                var location = crewedMember.CrewedPositionNavigation.PositionLocation;
                
                crewData.Add( 
                    new (
                        position, 
                        name, 
                        reportTime ?? "n/a", 
                        location
                    ));
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Crew List");
                
                // Global Formatting
                worksheet.Style.Font.Bold = true;
                worksheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Style.Font.FontName = "Verdana";
                worksheet.Style.Font.FontSize = 10;
                
                var titleRange = worksheet.Range("A1:D1").Merge();
                titleRange.Value = "TCU SPORTS BROADCASTING CREW LIST";
                titleRange.Style.Font.FontSize = 16;
                
                // Title Section
                var gameRange = worksheet.Range("A2:D2").Merge();
                gameRange.Value = gameTitle;
                gameRange.Style.Font.FontSize = 14;
                
                var dateRange = worksheet.Range("A3:D3").Merge();
                dateRange.Value = date;
                dateRange.Style.Font.Bold = true;
                dateRange.Style.Font.FontSize = 14;

                var timeRange = worksheet.Range("A4:D4").Merge();
                timeRange.Value = game.GameStart.ToString();
                timeRange.Style.Font.FontSize = 14;

                var networkRange = worksheet.Range("A5:D5").Merge();
                networkRange.Value = eventType;
                networkRange.Style.Font.FontSize = 14;
                
                // HEADER ROW (Bold, Background Color, and Borders)
                worksheet.Cell("A6").Value = "POSITION";
                worksheet.Cell("B6").Value = "NAME";
                worksheet.Cell("C6").Value = "REPORT";
                worksheet.Cell("D6").Value = "REPORT";
                worksheet.Cell("C7").Value = "TIME";
                worksheet.Cell("D7").Value = "LOCATION";

                var headerRange = worksheet.Range("A6:D7");
                headerRange.Style.Font.FontSize = 12;
                
                // Insert Crew Data with Alignment
                int row = 8; // Start after header
                foreach (var (position, name, reportTime,reportLocation) in crewData)
                {
                    worksheet.Cell(row, 1).Value = position;
                    worksheet.Cell(row, 2).Value = name;
                    worksheet.Cell(row, 3).Value = reportTime;
                    worksheet.Cell(row, 4).Value = reportLocation;

                    // Apply alignment
                    worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    if (reportLocation == "CONTROL ROOM")
                    {
                        worksheet.Cell(row, 3).Style.Font.FontColor = XLColor.Red;
                        worksheet.Cell(row, 4).Style.Font.FontColor = XLColor.Red;
                    }

                    row++;
                }
                
                var scheduleRange = worksheet.Range($"A{row}:D{row}").Merge();
                scheduleRange.Value = "CREW SCHEDULE";
                scheduleRange.Style.Font.FontSize = 12;
                row++;

                var crewScheduleList = new List<(string Task, string Time)>
                {
                    ("CREW CALL", "2:00PM"),
                    ("REHEARSAL", "3:00PM"),
                    ("BREAK", "4:00PM"),
                    ("RETURN TO POSITION", "5:00PM"),
                    ("TAPE OPEN", "5:30PM"),
                    ("ON-AIR LIVE", "6:00PM")
                };
                
                foreach (var scheduleItem in crewScheduleList)
                {
                    worksheet.Cell(row, 1).Value = scheduleItem.Time;
                    var scheduleItemRange = worksheet.Range($"B{row}:D{row}").Merge();
                    scheduleItemRange.Value = scheduleItem.Task;

                    if (scheduleItem.Task == "ON-AIR LIVE")
                    {
                        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.Red;
                        scheduleItemRange.Style.Font.FontColor = XLColor.Red;
                        break;
                    }
                    row++;
                }
                
                // Apply Borders
                var borderRange = worksheet.Range($"A1:D{row}");
                foreach (var cell in borderRange.Cells())
                {
                    cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                }
                
                
                // Auto-fit columns for better readability
                //worksheet.Columns().AdjustToContents();
                worksheet.Column(1).Width = 19;
                worksheet.Column(2).Width = 26;
                worksheet.Column(3).Width = 10;
                worksheet.Column(4).Width = 22;
                
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    // Return file as a downloadable response
                    return File(stream.ToArray(), 
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                        "CrewList.xlsx");
                }
            }
        }
    }
}
