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
using ClosedXML.Excel.Drawings;
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
            
            string currentDir = Directory.GetCurrentDirectory();
            string solutionRoot;

            if (currentDir.Contains("TestFrogCrew"))
            {
                // Running from TestFrogCrew/bin/Debug/net8.0/
                solutionRoot = Directory.GetParent(currentDir)?.Parent?.Parent?.Parent?.FullName; 
            }
            else
            {
                // Running from backend project
                solutionRoot = Directory.GetParent(currentDir)?.FullName;
            }
            string imagePath = Path.Combine("backend", "Images", "TCU_Sports Broadcasting Athletics Style.jpg");
            string logoPath = Path.Combine(solutionRoot, imagePath);
            
            //string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "Images/TCU_Sports Broadcasting Athletics Style.jpg");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Crew List");
                
                // Global Formatting
                worksheet.Style.Font.Bold = true;
                worksheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Style.Font.FontName = "Arial";
                worksheet.Style.Font.FontSize = 12;
                
                // Logo
                var logoRange = worksheet.Range("A1:A6").Merge();
                logoRange.Value = ""; // Clear the merged cell value
                var picture = worksheet.AddPicture(logoPath);
                picture.Placement = XLPicturePlacement.MoveAndSize;
                picture.MoveTo(logoRange.FirstCell(), logoRange.LastCell().CellRight());
                
                // Sport
                var sportRange = worksheet.Range("B1:D1").Merge();
                sportRange.Value = game.Schedule.Sport.ToUpper();
                sportRange.Style.Font.FontSize = 16;
                
                // Versus
                var gameRange = worksheet.Range("B2:D2").Merge();
                gameRange.Value = "vs";
                gameRange.Style.Font.FontSize = 16;
                
                // Opponent
                var opponentRange = worksheet.Range("B3:D3").Merge();
                opponentRange.Value = game.Opponent;
                opponentRange.Style.Font.FontSize = 16;

                // Date
                var dateRange = worksheet.Range("B4:D4").Merge();
                dateRange.Value = game.GameDate?.ToString("MMMM dd, yyyy");
                dateRange.Style.Font.FontSize = 16;

                // Start Time
                var timeRange = worksheet.Range("B5:D5").Merge();
                timeRange.Value = game.GameStart?.ToString("h:mm tt", CultureInfo.InvariantCulture);
                timeRange.Style.Font.FontSize = 16;
                
                // Event Type
                var networkRange = worksheet.Range("B6:D6").Merge();
                networkRange.Value = eventType;
                networkRange.Style.Font.FontSize = 16;
                
                var crewList = worksheet.Range("A8:D8").Merge();
                crewList.Value = "CREW LIST";
                crewList.Style.Font.FontSize = 16;
                crewList.Style.Fill.BackgroundColor = XLColor.LightGray;
                
                // HEADER ROW (Bold, Background Color, and Borders)
                worksheet.Cell("A9").Value = "POSITION";
                worksheet.Cell("B9").Value = "NAME";
                worksheet.Cell("C9").Value = "REPORT TIME";
                worksheet.Cell("D9").Value = "LOCATION";

                var headerRange = worksheet.Range("A9:D9");
                headerRange.Style.Font.FontSize = 12;
                
                // Insert Crew Data with Alignment
                int row = 10; // Start after header
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
                        worksheet.Cell(row, 2).Style.Font.FontColor = XLColor.Red;
                        worksheet.Cell(row, 3).Style.Font.FontColor = XLColor.Red;
                        worksheet.Cell(row, 4).Style.Font.FontColor = XLColor.Red;
                    }

                    row++;
                }
                
                var scheduleRange = worksheet.Range($"A{row}:D{row}").Merge();
                scheduleRange.Value = "SCHEDULE";
                scheduleRange.Style.Font.FontSize = 12;
                scheduleRange.Style.Fill.BackgroundColor = XLColor.LightGray;
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
                worksheet.Column(1).Width = 28;
                worksheet.Column(2).Width = 31;
                worksheet.Column(3).Width = 17;
                worksheet.Column(4).Width = 24;
                
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
