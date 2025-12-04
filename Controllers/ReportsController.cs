using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DripCube.Data;
using DripCube.Entities;

namespace DripCube.Controllers
{

    public class CreateReportDto
    {
        public Guid ReporterId { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<ActionResult> CreateReport(CreateReportDto dto)
        {

            var user = await _context.Users.FindAsync(dto.ReporterId);
            string reporterName = user != null ? user.FirstName : "Unknown";

            var report = new Report
            {
                ReporterId = dto.ReporterId,
                ReporterName = reporterName,
                TargetType = dto.TargetType,
                TargetIdentifier = dto.TargetId,
                Reason = dto.Reason,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Report Submitted" });
        }


        [HttpGet]
        public async Task<ActionResult> GetReports()
        {
            var reports = await _context.Reports
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return Ok(reports);
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}