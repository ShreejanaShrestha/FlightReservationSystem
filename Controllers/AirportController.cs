using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FlightReservationSystem.Models;
using FlightReservationSystem.Data;
using FlightReservationSystem.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightReservationSystem.Controllers
{
    /// <summary>
    /// API controller for managing airport information
    /// </summary>
    /// <remarks>
    /// Provides CRUD operations for airport data.
    /// Requires Admin role for all operations.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AirportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AirportsController> _logger;

        /// <summary>
        /// Initializes a new instance of the AirportsController
        /// </summary>
        /// <param name="context">Database context for airport operations</param>
        /// <param name="logger">Logger for error and information logging</param>
        public AirportsController(ApplicationDbContext context, ILogger<AirportsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all airports
        /// </summary>
        /// <returns>List of all airports</returns>
        /// <response code="200">Returns the list of airports</response>
        /// <response code="404">If no airports are found</response>
        /// <response code="500">If there was a server error</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AirportDto>>> GetAirports()
        {
            try
            {
                _logger.LogInformation("Retrieving all airports");

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
                    _logger.LogWarning("No airports found in database");
                    return NotFound(new { message = "No airports found" });
                }

                _logger.LogInformation("Successfully retrieved {Count} airports", airports.Count);
                return Ok(airports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving airports");
                return StatusCode(500, new
                {
                    error = "An error occurred while retrieving airports",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Retrieves a specific airport by ID
        /// </summary>
        /// <param name="id">The ID of the airport to retrieve</param>
        /// <returns>The requested airport</returns>
        /// <response code="200">Returns the requested airport</response>
        /// <response code="404">If the airport is not found</response>
        /// <response code="500">If there was a server error</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<AirportDto>> GetAirport(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving airport with ID: {AirportId}", id);

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
                    _logger.LogWarning("Airport with ID {AirportId} not found", id);
                    return NotFound(new { message = "Airport not found" });
                }

                _logger.LogInformation("Successfully retrieved airport with ID: {AirportId}", id);
                return Ok(airport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving airport with ID: {AirportId}", id);
                return StatusCode(500, new
                {
                    error = "An error occurred while retrieving the airport",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Creates a new airport
        /// </summary>
        /// <param name="airport">The airport data to create</param>
        /// <returns>The newly created airport</returns>
        /// <response code="201">Returns the created airport</response>
        /// <response code="400">If the airport data is invalid or code is not unique</response>
        /// <response code="500">If there was a server error</response>
        [HttpPost]
        public async Task<ActionResult<Airport>> AddAirport([FromBody] Airport airport)
        {
            try
            {
                _logger.LogInformation("Attempting to add new airport");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid airport data received");
                    return BadRequest(ModelState);
                }

                // Validate that the airport code is unique
                if (await _context.Airports.AnyAsync(a => a.Code == airport.Code))
                {
                    _logger.LogWarning($"Airport code {airport.Code} already exists.");
                    return BadRequest(new { error = $"Airport code {airport.Code} already exists" });
                }

                _context.Airports.Add(airport);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created new airport with ID: {AirportId}", airport.AirportId);
                return CreatedAtAction(nameof(GetAirport), new { id = airport.AirportId }, airport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new airport");
                return StatusCode(500, new
                {
                    error = "An error occurred while adding the airport",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates an existing airport
        /// </summary>
        /// <param name="id">The ID of the airport to update</param>
        /// <param name="airport">The updated airport data</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">If the update was successful</response>
        /// <response code="400">If the ID doesn't match or data is invalid</response>
        /// <response code="404">If the airport is not found</response>
        /// <response code="500">If there was a server error</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAirport(int id, [FromBody] Airport airport)
        {
            _logger.LogInformation("Attempting to update airport with ID: {AirportId}", id);

            if (id != airport.AirportId)
            {
                _logger.LogWarning("Airport ID mismatch in update request");
                return BadRequest(new { message = "Airport ID mismatch" });
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid airport data received for update");
                    return BadRequest(ModelState);
                }

                var existingAirport = await _context.Airports.FindAsync(id);
                if (existingAirport == null)
                {
                    _logger.LogWarning($"Airport with ID {id} not found.");
                    return NotFound(new { message = "Airport not found" });
                }

                // Validate that the new airport code is unique (if changed)
                if (existingAirport.Code != airport.Code && await _context.Airports.AnyAsync(a => a.Code == airport.Code))
                {
                    _logger.LogWarning($"Airport code {airport.Code} already exists.");
                    return BadRequest(new { error = $"Airport code {airport.Code} already exists" });
                }

                existingAirport.Name = airport.Name;
                existingAirport.Code = airport.Code;
                existingAirport.City = airport.City;
                existingAirport.Country = airport.Country;

                _context.Entry(existingAirport).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated airport with ID: {AirportId}", id);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Airports.Any(a => a.AirportId == id))
                {
                    _logger.LogWarning("Airport with ID {AirportId} not found for update", id);
                    return NotFound(new { message = "Airport not found" });
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating airport with ID: {AirportId}", id);
                return StatusCode(500, new
                {
                    error = "An error occurred while updating the airport",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes an airport
        /// </summary>
        /// <param name="id">The ID of the airport to delete</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">If the deletion was successful</response>
        /// <response code="400">If the airport has associated flights</response>
        /// <response code="404">If the airport is not found</response>
        /// <response code="500">If there was a server error</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAirport(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete airport with ID: {AirportId}", id);

                var airport = await _context.Airports
                    .Include(a => a.DepartureFlights)
                    .Include(a => a.ArrivalFlights)
                    .FirstOrDefaultAsync(a => a.AirportId == id);

                if (airport == null)
                {
                    _logger.LogWarning("Airport with ID {AirportId} not found for deletion", id);
                    return NotFound(new { message = "Airport not found" });
                }

                if (airport.DepartureFlights.Any() || airport.ArrivalFlights.Any())
                {
                    _logger.LogWarning($"Cannot delete airport {airport.Name} because it is associated with flights.");
                    return BadRequest(new { error = "Cannot delete airport because it is associated with flights" });
                }

                _context.Airports.Remove(airport);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted airport with ID: {AirportId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting airport with ID: {AirportId}", id);
                return StatusCode(500, new
                {
                    error = "An error occurred while deleting the airport",
                    details = ex.Message
                });
            }
        }
    }
}