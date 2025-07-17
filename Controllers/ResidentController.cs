using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisitorManagementSystem.Models.DTOs;
using VisitorManagementSystem.Services;

namespace VisitorManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ResidentController : ControllerBase
    {
        private readonly IResidentService _residentService;
        private readonly ILogger<ResidentController> _logger;

        public ResidentController(IResidentService residentService, ILogger<ResidentController> logger)
        {
            _residentService = residentService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ResidentDto>>> GetAllResidents()
        {
            try
            {
                var residents = await _residentService.GetAllResidentsAsync();
                return Ok(residents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all residents");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResidentDto>> GetResident(int id)
        {
            try
            {
                var resident = await _residentService.GetResidentByIdAsync(id);
                return Ok(resident);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resident with id {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResidentDto>> GetResidentsPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var residents = await _residentService.GetResidentsPagedAsync(page, pageSize);
                return Ok(residents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged residents");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("search")]
        public async Task<ActionResult<PagedResidentDto>> SearchResidents([FromBody] ResidentSearchDto searchDto)
        {
            try
            {
                var residents = await _residentService.SearchResidentsAsync(searchDto);
                return Ok(residents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching residents");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResidentDto>> CreateResident([FromBody] CreateResidentDto createDto)
        {
            try
            {
                var resident = await _residentService.CreateResidentAsync(createDto);
                return CreatedAtAction(nameof(GetResident), new { id = resident.Id }, resident);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating resident");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResidentDto>> UpdateResident(int id, [FromBody] CreateResidentDto updateDto)
        {
            try
            {
                var resident = await _residentService.UpdateResidentAsync(id, updateDto);
                return Ok(resident);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating resident with id {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteResident(int id)
        {
            try
            {
                var deleted = await _residentService.DeleteResidentAsync(id);
                if (!deleted)
                    return NotFound("Resident not found");
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting resident with id {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("apartment/{apartmentNumber}")]
        public async Task<ActionResult<ResidentDto>> GetResidentByApartment(string apartmentNumber)
        {
            try
            {
                var resident = await _residentService.GetResidentByApartmentAsync(apartmentNumber);
                if (resident == null)
                    return NotFound("Resident not found");
                
                return Ok(resident);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resident by apartment {ApartmentNumber}", apartmentNumber);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("search/contact/{contactValue}")]
        public async Task<ActionResult<List<ResidentDto>>> SearchByContact(string contactValue)
        {
            try
            {
                var residents = await _residentService.SearchByContactAsync(contactValue);
                return Ok(residents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching residents by contact {ContactValue}", contactValue);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("search/license/{licensePlate}")]
        public async Task<ActionResult<List<ResidentDto>>> SearchByLicensePlate(string licensePlate)
        {
            try
            {
                var residents = await _residentService.SearchByLicensePlateAsync(licensePlate);
                return Ok(residents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching residents by license plate {LicensePlate}", licensePlate);
                return StatusCode(500, "Internal server error");
            }
        }

        // Contact management endpoints
        [HttpPost("{residentId}/contacts")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResidentContactDto>> AddContact(int residentId, [FromBody] CreateResidentContactDto contactDto)
        {
            try
            {
                var contact = await _residentService.AddContactAsync(residentId, contactDto);
                return CreatedAtAction(nameof(GetContactsByResident), new { residentId }, contact);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding contact to resident {ResidentId}", residentId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("contacts/{contactId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResidentContactDto>> UpdateContact(int contactId, [FromBody] CreateResidentContactDto contactDto)
        {
            try
            {
                var contact = await _residentService.UpdateContactAsync(contactId, contactDto);
                return Ok(contact);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contact {ContactId}", contactId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("contacts/{contactId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteContact(int contactId)
        {
            try
            {
                var deleted = await _residentService.DeleteContactAsync(contactId);
                if (!deleted)
                    return NotFound("Contact not found");
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contact {ContactId}", contactId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{residentId}/contacts")]
        public async Task<ActionResult<List<ResidentContactDto>>> GetContactsByResident(int residentId)
        {
            try
            {
                var contacts = await _residentService.GetContactsByResidentAsync(residentId);
                return Ok(contacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contacts for resident {ResidentId}", residentId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{residentId}/contacts/{contactType}/priority")]
        public async Task<ActionResult<List<ResidentContactDto>>> GetPriorityContacts(int residentId, string contactType)
        {
            try
            {
                var contacts = await _residentService.GetPriorityContactsAsync(residentId, contactType);
                return Ok(contacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting priority contacts for resident {ResidentId} type {ContactType}", residentId, contactType);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{residentId}/contacts/{contactType}/highest")]
        public async Task<ActionResult<ResidentContactDto>> GetHighestPriorityContact(int residentId, string contactType)
        {
            try
            {
                var contact = await _residentService.GetHighestPriorityContactAsync(residentId, contactType);
                if (contact == null)
                    return NotFound("No contacts found");
                
                return Ok(contact);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting highest priority contact for resident {ResidentId} type {ContactType}", residentId, contactType);
                return StatusCode(500, "Internal server error");
            }
        }

        // Vehicle management endpoints
        [HttpPost("{residentId}/vehicles")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResidentVehicleDto>> AddVehicle(int residentId, [FromBody] CreateResidentVehicleDto vehicleDto)
        {
            try
            {
                var vehicle = await _residentService.AddVehicleAsync(residentId, vehicleDto);
                return CreatedAtAction(nameof(GetVehiclesByResident), new { residentId }, vehicle);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding vehicle to resident {ResidentId}", residentId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("vehicles/{vehicleId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResidentVehicleDto>> UpdateVehicle(int vehicleId, [FromBody] CreateResidentVehicleDto vehicleDto)
        {
            try
            {
                var vehicle = await _residentService.UpdateVehicleAsync(vehicleId, vehicleDto);
                return Ok(vehicle);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle {VehicleId}", vehicleId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("vehicles/{vehicleId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteVehicle(int vehicleId)
        {
            try
            {
                var deleted = await _residentService.DeleteVehicleAsync(vehicleId);
                if (!deleted)
                    return NotFound("Vehicle not found");
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle {VehicleId}", vehicleId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{residentId}/vehicles")]
        public async Task<ActionResult<List<ResidentVehicleDto>>> GetVehiclesByResident(int residentId)
        {
            try
            {
                var vehicles = await _residentService.GetVehiclesByResidentAsync(residentId);
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles for resident {ResidentId}", residentId);
                return StatusCode(500, "Internal server error");
            }
        }

        // Bulk operations
        [HttpPost("bulk")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<ResidentDto>>> BulkCreateResidents([FromBody] List<CreateResidentDto> residents)
        {
            try
            {
                var createdResidents = await _residentService.BulkCreateResidentsAsync(residents);
                return Ok(createdResidents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk creating residents");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("export")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ExportResidents()
        {
            try
            {
                var excelData = await _residentService.ExportResidentsToExcelAsync();
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "residents.xlsx");
            }
            catch (NotImplementedException)
            {
                return StatusCode(501, "Excel export feature is not implemented yet");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting residents");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("import")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<string>>> ImportResidents(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded");

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                var excelData = stream.ToArray();

                var errors = await _residentService.ImportResidentsFromExcelAsync(excelData);
                return Ok(errors);
            }
            catch (NotImplementedException)
            {
                return StatusCode(501, "Excel import feature is not implemented yet");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing residents");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}