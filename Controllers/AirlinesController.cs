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
    /// Controller responsible for managing airline-related operations in the Flight Reservation System.
    /// Provides endpoints for creating, reading, updating, and deleting airline information.
    /// </summary>
    /// <remarks>
    /// All administrative operations (Create, Update, Delete) require Admin role authorization.
    /// Read operations are available to all authenticated users.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class AirlinesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AirlinesController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AirlinesController"/> class.
        /// </summary>
        /// <param name="context">The database context for airline operations.</param>
        /// <param name="logger">The logger for recording operation details and errors.</param>
        public AirlinesController(ApplicationDbContext context, ILogger<AirlinesController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all airlines from the database.
        /// </summary>
        /// <returns>
        /// 200 OK with a collection of airline DTOs if airlines exist.
        /// 404 Not Found if no airlines exist.
        /// 500 Internal Server Error if an exception occurs during processing.
        /// </returns>
        /// <remarks>
        /// This endpoint maps airline entities to DTOs to limit the data exposed to clients.
        /// </remarks>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AirlineDto>>> GetAirlines()
        {
            try
            {
                _logger.LogInformation("Retrieving all airlines");

                var airlines = await _context.Airlines
                    .Select(a => new AirlineDto
                    {
                        AirlineId = a.AirlineId,
                        Name = a.Name,
                        Code = a.Code,
                        LogoUrl = a.LogoUrl
                    })
                    .ToListAsync();

                if (!airlines.Any())
                {
                    _logger.LogWarning("No airlines found in the database");
                    return NotFound(new { message = "No airlines found" });
                }

                _logger.LogInformation($"Successfully retrieved {airlines.Count} airlines");
                return Ok(airlines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving airlines");
                return StatusCode(500, new { error = "An error occurred while retrieving airlines", details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a specific airline by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the airline to retrieve.</param>
        /// <returns>
        /// 200 OK with the airline DTO if found.
        /// 404 Not Found if the airline with the specified ID doesn't exist.
        /// 500 Internal Server Error if an exception occurs during processing.
        /// </returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<AirlineDto>> GetAirline(int id)
        {
            try
            {
                _logger.LogInformation($"Retrieving airline with ID: {id}");

                var airline = await _context.Airlines
                    .Select(a => new AirlineDto
                    {
                        AirlineId = a.AirlineId,
                        Name = a.Name,
                        Code = a.Code,
                        LogoUrl = a.LogoUrl
                    })
                    .FirstOrDefaultAsync(a => a.AirlineId == id);

                if (airline == null)
                {
                    _logger.LogWarning($"Airline with ID {id} not found.");
                    return NotFound(new { message = "Airline not found" });
                }

                _logger.LogInformation($"Successfully retrieved airline with ID: {id}");
                return Ok(airline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving airline with ID: {id}", ex);
                return StatusCode(500, new { error = "An error occurred while retrieving the airline", details = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new airline in the system.
        /// </summary>
        /// <param name="airline">The airline object to create.</param>
        /// <returns>
        /// 201 Created with the newly created airline if successful.
        /// 400 Bad Request if the model is invalid or the airline code already exists.
        /// 500 Internal Server Error if an exception occurs during processing.
        /// </returns>
        /// <remarks>
        /// This endpoint requires Admin role authorization.
        /// The airline code must be unique in the system.
        /// </remarks>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Airline>> AddAirline([FromBody] Airline airline)
        {
            try
            {
                _logger.LogInformation($"Attempting to add new airline: {airline.Name} (Code: {airline.Code})");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid airline data received");
                    return BadRequest(ModelState);
                }

                // Validate that the airline code is unique
                if (await _context.Airlines.AnyAsync(a => a.Code == airline.Code))
                {
                    _logger.LogWarning($"Airline code {airline.Code} already exists.");
                    return BadRequest(new { error = $"Airline code {airline.Code} already exists" });
                }

                // Ensure navigation property is initialized to prevent null reference exceptions
                airline.Flights = new List<Flight>();
                _context.Airlines.Add(airline);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully added airline with ID: {airline.AirlineId}, Name: {airline.Name}");
                return CreatedAtAction(nameof(GetAirline), new { id = airline.AirlineId }, airline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding airline: {airline?.Name ?? "Unknown"}");
                return StatusCode(500, new { error = "An error occurred while adding the airline", details = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing airline in the system.
        /// </summary>
        /// <param name="id">The unique identifier of the airline to update.</param>
        /// <param name="airline">The updated airline object.</param>
        /// <returns>
        /// 204 No Content if the update is successful.
        /// 400 Bad Request if the ID doesn't match, the model is invalid, or the airline code already exists.
        /// 404 Not Found if the airline with the specified ID doesn't exist.
        /// 500 Internal Server Error if an exception occurs during processing.
        /// </returns>
        /// <remarks>
        /// This endpoint requires Admin role authorization.
        /// The airline code must be unique in the system if it's being changed.
        /// </remarks>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAirline(int id, [FromBody] Airline airline)
        {
            try
            {
                _logger.LogInformation($"Updating airline with ID: {id}, Name: {airline.Name}");

                // Validate ID consistency between route and body
                if (id != airline.AirlineId)
                {
                    _logger.LogWarning($"Airline ID mismatch in update request. Route ID: {id}, Body ID: {airline.AirlineId}");
                    return BadRequest(new { message = "Airline ID mismatch" });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid airline data received for update");
                    return BadRequest(ModelState);
                }

                // Verify airline exists
                var existingAirline = await _context.Airlines.FindAsync(id);
                if (existingAirline == null)
                {
                    _logger.LogWarning($"Airline with ID {id} not found for update operation.");
                    return NotFound(new { message = "Airline not found" });
                }

                // Validate code uniqueness only if the code is being changed
                if (existingAirline.Code != airline.Code && await _context.Airlines.AnyAsync(a => a.Code == airline.Code))
                {
                    _logger.LogWarning($"Updated airline code {airline.Code} already exists.");
                    return BadRequest(new { error = $"Airline code {airline.Code} already exists" });
                }

                // Update properties individually to maintain control over what gets updated
                existingAirline.Name = airline.Name;
                existingAirline.Code = airline.Code;
                existingAirline.LogoUrl = airline.LogoUrl;

                _context.Entry(existingAirline).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully updated airline with ID: {id}, Name: {airline.Name}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating airline with ID: {id}");
                return StatusCode(500, new { error = "An error occurred while updating the airline", details = ex.Message });
            }
        }

        /// <summary>
        /// Deletes an airline from the system.
        /// </summary>
        /// <param name="id">The unique identifier of the airline to delete.</param>
        /// <returns>
        /// 204 No Content if the deletion is successful.
        /// 400 Bad Request if the airline has associated flights that prevent deletion.
        /// 404 Not Found if the airline with the specified ID doesn't exist.
        /// 500 Internal Server Error if an exception occurs during processing.
        /// </returns>
        /// <remarks>
        /// This endpoint requires Admin role authorization.
        /// Airlines with associated flights cannot be deleted to maintain referential integrity.
        /// </remarks>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAirline(int id)
        {
            try
            {
                _logger.LogInformation($"Attempting to delete airline with ID: {id}");

                // Include related flights to check for dependencies
                var airline = await _context.Airlines
                    .Include(a => a.Flights)
                    .FirstOrDefaultAsync(a => a.AirlineId == id);

                if (airline == null)
                {
                    _logger.LogWarning($"Airline with ID {id} not found for deletion.");
                    return NotFound(new { message = "Airline not found" });
                }

                // Prevent deletion if there are associated flights (referential integrity protection)
                if (airline.Flights.Any())
                {
                    _logger.LogWarning($"Cannot delete airline {airline.Name} (ID: {id}) because it has {airline.Flights.Count} associated flights.");
                    return BadRequest(new { error = "Cannot delete airline because it has associated flights" });
                }

                _context.Airlines.Remove(airline);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully deleted airline with ID: {id}, Name: {airline.Name}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting airline with ID: {id}");
                return StatusCode(500, new { error = "An error occurred while deleting the airline", details = ex.Message });
            }
        }
    }
}