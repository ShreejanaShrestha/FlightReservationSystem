// FlightReservationSystem/Controllers/AirportsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FlightReservationSystem.Models;
using FlightReservationSystem.Data;
using FlightReservationSystem.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace FlightReservationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AirportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AirportsController> _logger;

        public AirportsController(ApplicationDbContext context, ILogger<AirportsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/airports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AirportDto>>> GetAirports()
        {
            try
            {
                var airports = await _context.Airports
                    .Select(a => new AirportDto
                    {
                        AirportId = a.AirportId,
                        Name = a.Name,
                        Code = a.Code,
                        City = a.City,
                        Country = a.Country
                    })
                    .ToListAsync();

                if (!airports.Any())
                {
                    return NotFound(new { message = "No airports found" });
                }
                return Ok(airports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving airports");
                return StatusCode(500, new { error = "An error occurred while retrieving airports", details = ex.Message });
            }
        }

        // GET: api/airports/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AirportDto>> GetAirport(int id)
        {
            try
            {
                var airport = await _context.Airports
                    .Select(a => new AirportDto
                    {
                        AirportId = a.AirportId,
                        Name = a.Name,
                        Code = a.Code,
                        City = a.City,
                        Country = a.Country
                    })
                    .FirstOrDefaultAsync(a => a.AirportId == id);

                if (airport == null)
                {
                    return NotFound(new { message = "Airport not found" });
                }
                return Ok(airport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving airport");
                return StatusCode(500, new { error = "An error occurred while retrieving the airport", details = ex.Message });
            }
        }

        // POST: api/airports
        [HttpPost]
        public async Task<ActionResult<Airport>> AddAirport([FromBody] Airport airport)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _context.Airports.Add(airport);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAirport), new { id = airport.AirportId }, airport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding airport");
                return StatusCode(500, new { error = "An error occurred while adding the airport", details = ex.Message });
            }
        }

        // PUT: api/airports/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAirport(int id, [FromBody] Airport airport)
        {
            if (id != airport.AirportId)
            {
                return BadRequest(new { message = "Airport ID mismatch" });
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _context.Entry(airport).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Airports.Any(a => a.AirportId == id))
                {
                    return NotFound(new { message = "Airport not found" });
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating airport");
                return StatusCode(500, new { error = "An error occurred while updating the airport", details = ex.Message });
            }
        }

        // DELETE: api/airports/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAirport(int id)
        {
            try
            {
                var airport = await _context.Airports.FindAsync(id);
                if (airport == null)
                {
                    return NotFound(new { message = "Airport not found" });
                }

                _context.Airports.Remove(airport);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting airport");
                return StatusCode(500, new { error = "An error occurred while deleting the airport", details = ex.Message });
            }
        }
    }
}