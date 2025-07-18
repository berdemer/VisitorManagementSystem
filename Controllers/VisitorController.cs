using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisitorManagementSystem.Models;
using VisitorManagementSystem.Models.DTOs;
using VisitorManagementSystem.Services;

namespace VisitorManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VisitorController : ControllerBase
    {
        private readonly IVisitorService _visitorService;
        private readonly ISmsService _smsService;
        private readonly ILogger<VisitorController> _logger;

        public VisitorController(IVisitorService visitorService, ISmsService smsService, ILogger<VisitorController> logger)
        {
            _visitorService = visitorService;
            _smsService = smsService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Visitor>>> GetAllVisitors()
        {
            var visitors = await _visitorService.GetAllVisitorsAsync();
            return Ok(visitors);
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedVisitorDto>> GetVisitorsPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var visitors = await _visitorService.GetVisitorsPagedAsync(page, pageSize);
                return Ok(visitors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged visitors");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("search")]
        public async Task<ActionResult<PagedVisitorDto>> SearchVisitors([FromBody] VisitorSearchDto searchDto)
        {
            try
            {
                var visitors = await _visitorService.SearchVisitorsAsync(searchDto);
                return Ok(visitors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching visitors");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Visitor>>> GetActiveVisitors()
        {
            var visitors = await _visitorService.GetActiveVisitorsAsync();
            return Ok(visitors);
        }

        [HttpGet("search/{name}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> SearchVisitorsByName(string name)
        {
            try
            {
                var visitors = await _visitorService.SearchVisitorsByNameAsync(name);
                
                // Return unique visitors with their contact info
                var uniqueVisitors = visitors
                    .GroupBy(v => new { v.FullName, v.VisitorPhone, v.LicensePlate })
                    .Select(g => new
                    {
                        fullName = g.Key.FullName,
                        visitorPhone = g.Key.VisitorPhone,
                        licensePlate = g.Key.LicensePlate,
                        visitCount = g.Count(),
                        lastVisit = g.Max(v => v.CheckInTime)
                    })
                    .OrderByDescending(v => v.lastVisit)
                    .Take(10)
                    .ToList();

                return Ok(uniqueVisitors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching visitors by name {Name}", name);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Visitor>> GetVisitor(int id)
        {
            var visitor = await _visitorService.GetVisitorByIdAsync(id);
            if (visitor == null)
                return NotFound();

            return Ok(visitor);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Visitor>> CreateVisitor([FromBody] Visitor visitor)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                visitor.CreatedBy = User.Identity?.Name ?? "System";
                var createdVisitor = await _visitorService.CreateVisitorAsync(visitor);

                // Send SMS notification if phone number is provided
                if (!string.IsNullOrEmpty(visitor.ResidentPhone))
                {
                    await _smsService.SendVisitorNotificationAsync(
                        visitor.ResidentPhone,
                        visitor.FullName,
                        visitor.ApartmentNumber);
                }

                return CreatedAtAction(nameof(GetVisitor), new { id = createdVisitor.Id }, createdVisitor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating visitor");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Visitor>> UpdateVisitor(int id, [FromBody] Visitor visitor)
        {
            if (id != visitor.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedVisitor = await _visitorService.UpdateVisitorAsync(visitor);
                return Ok(updatedVisitor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating visitor {VisitorId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/checkout")]
        public async Task<ActionResult> CheckOutVisitor(int id)
        {
            try
            {
                var performedBy = User.Identity?.Name ?? "System";
                var success = await _visitorService.CheckOutVisitorAsync(id, performedBy);
                
                if (!success)
                    return NotFound("Visitor not found or already checked out");

                return Ok(new { message = "Visitor checked out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking out visitor {VisitorId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("apartment/{apartmentNumber}")]
        public async Task<ActionResult<IEnumerable<Visitor>>> GetVisitorsByApartment(string apartmentNumber)
        {
            var visitors = await _visitorService.GetVisitorsByApartmentAsync(apartmentNumber);
            return Ok(visitors);
        }

        [HttpGet("daterange")]
        public async Task<ActionResult<IEnumerable<Visitor>>> GetVisitorsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var visitors = await _visitorService.GetVisitorsByDateRangeAsync(startDate, endDate);
            return Ok(visitors);
        }

        [HttpGet("export")]
        public async Task<ActionResult> ExportToExcel(
            [FromQuery] string startDate,
            [FromQuery] string endDate)
        {
            try
            {
                if (!DateTime.TryParse(startDate, out var parsedStartDate) || !DateTime.TryParse(endDate, out var parsedEndDate))
                {
                    return BadRequest("Invalid date format");
                }

                var visitors = await _visitorService.GetVisitorsByDateRangeAsync(parsedStartDate, parsedEndDate);
                var excelData = await _visitorService.ExportVisitorsToExcelAsync(visitors, parsedStartDate, parsedEndDate);
                
                var fileName = $"ziyaretci_raporu_{parsedStartDate:yyyy-MM-dd}_{parsedEndDate:yyyy-MM-dd}.xlsx";
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting visitors to Excel");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("apartment-stats")]
        public async Task<ActionResult> GetMostVisitedApartments(
            [FromQuery] string? startDate,
            [FromQuery] string? endDate)
        {
            try
            {
                DateTime? parsedStartDate = null;
                DateTime? parsedEndDate = null;

                if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var tempStartDate))
                {
                    parsedStartDate = tempStartDate;
                }

                if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var tempEndDate))
                {
                    parsedEndDate = tempEndDate;
                }

                var stats = await _visitorService.GetMostVisitedApartmentsAsync(parsedStartDate, parsedEndDate);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting apartment statistics");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteVisitor(int id)
        {
            try
            {
                var success = await _visitorService.DeleteVisitorAsync(id);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting visitor {VisitorId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("upload-photo")]
        [AllowAnonymous]
        public async Task<ActionResult> UploadPhoto(IFormFile photo, [FromQuery] int visitorId)
        {
            if (photo == null || photo.Length == 0)
                return BadRequest("No photo provided");

            try
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "photos");
                Directory.CreateDirectory(uploadsDir);

                var fileName = $"visitor_{visitorId}_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(photo.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                var visitor = await _visitorService.GetVisitorByIdAsync(visitorId);
                if (visitor != null)
                {
                    visitor.PhotoPath = $"/uploads/photos/{fileName}";
                    await _visitorService.UpdateVisitorAsync(visitor);
                }

                return Ok(new { photoPath = $"/uploads/photos/{fileName}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photo for visitor {VisitorId}", visitorId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}