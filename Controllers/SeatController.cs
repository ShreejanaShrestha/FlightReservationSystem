// FlightReservationSystem/Controllers/SeatsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FlightReservationSystem.Models;
using FlightReservationSystem.Data;
using FlightReservationSystem.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FlightReservationSystem.Controllers
{
    /// <summary>
    /// API controller responsible for managing seat-related operations in the Flight Reservation System.
    /// Provides endpoints for creating, reading, updating, and deleting seat information.
    /// </summary>
    /// <remarks>
    /// All operations in this controller require Admin role authorization.
    /// This controller manages the seat inventory for flights, including seat status and class designations.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class SeatsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SeatsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeatsController"/> class.
        /// </summary>
        /// <param name="context">The database context for seat-related operations.</param>
        /// <param name="logger">The logger for recording operation details and errors.</param>
        /// <exception cref="ArgumentNullException">Thrown when context or logger is null.</exception>
        public SeatsController(ApplicationDbContext context, ILogger<SeatsController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all seats from the database with their associated flight, airline, and airport information.
        /// </summary>
        /// <returns>
        /// 200 OK with a collection of seat DTOs if seats exist.
        /// 404 Not Found if no seats exist.
        /// 500 Internal Server Error if an exception occurs during processing.
        /// </returns>
        /// <remarks>
        /// This endpoint performs deep loading of related entities and maps them to DTOs to limit the data exposed to clients.
        /// The projection includes comprehensive flight details including airline and airport information.
        /// </remarks>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SeatDto>>> GetSeats()
        {
            try
            {
                _logger.LogInformation("Retrieving all seats with related flight information");

                // Retrieve seats with related flight, airline, and airport information
                var seats = await _context.Seats
                    .Include(s => s.Flight)
                    .ThenInclude(f => f.Airline)
                    .Include(s => s.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                    .Include(s => s.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                    .Select(s => new SeatDto
                    {
                        SeatId = s.SeatId,
                        FlightId = s.FlightId,
                        Flight = new FlightDto
                        {
                            FlightId = s.Flight.FlightId,
                            FlightNumber = s.Flight.FlightNumber,
                            AirlineId = s.Flight.AirlineId,
                            Airline = new AirlineDto
                            {
                                AirlineId = s.Flight.Airline.AirlineId,
                                Name = s.Flight.Airline.Name,
                                Code = s.Flight.Airline.Code,
                                LogoUrl = s.Flight.Airline.LogoUrl
                            },
                            DepartureAirportId = s.Flight.DepartureAirportId,
                            DepartureAirport = new AirportDto
                            {
                                AirportId = s.Flight.DepartureAirport.AirportId,
                                Name = s.Flight.DepartureAirport.Name,
                                Code = s.Flight.DepartureAirport.Code,
                                City = s.Flight.DepartureAirport.City,
                                Country = s.Flight.DepartureAirport.Country
                            },
                            ArrivalAirportId = s.Flight.ArrivalAirportId,
                            ArrivalAirport = new AirportDto
                            {
                                AirportId = s.Flight.ArrivalAirport.AirportId,
                                Name = s.Flight.ArrivalAirport.Name,
                                Code = s.Flight.ArrivalAirport.Code,
                                City = s.Flight.ArrivalAirport.City,
                                Country = s.Flight.ArrivalAirport.Country
                            },
                            DepartureTime = s.Flight.DepartureTime,
                            ArrivalTime = s.Flight.ArrivalTime,
                            BasePrice = s.Flight.BasePrice
                        },
                        SeatNumber = s.SeatNumber,
                        Class = s.Class,
                        IsBooked = s.IsBooked,

                    })
                    .ToListAsync();

                // Check if any seats were found
                if (!seats.Any())
                {
                    _logger.LogWarning("No seats found in the database");
                    return NotFound(new { message = "No seats found" });
                }

                _logger.LogInformation($"Successfully retrieved {seats.Count} seats");
                return Ok(seats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving seats");
                return StatusCode(500, new { error = "An error occurred while retrieving seats", details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a specific seat by its unique identifier with associated flight, airline, and airport information.
        /// </summary>
        /// <param name="id">The unique identifier of the seat to retrieve.</param>
        /// <returns>
        /// 200 OK with the seat DTO if found.
        /// 404 Not Found if the seat with the specified ID doesn't exist.
        /// 500 Internal Server Error if an exception occurs during processing.
        /// </returns>
        /// <remarks>
        /// This endpoint performs similar deep loading as GetSeats but filters for a specific seat ID.
        /// </remarks>
        [HttpGet("{id}")]
        public async Task<ActionResult<SeatDto>> GetSeat(int id)
        {
            try
            {
                _logger.LogInformation($"Retrieving seat with ID: {id}");

                // Retrieve specific seat with related flight information
                var seat = await _context.Seats
                    .Include(s => s.Flight)
                    .ThenInclude(f => f.Airline)
                    .Include(s => s.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                    .Include(s => s.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                    .Select(s => new SeatDto
                    {
                        SeatId = s.SeatId,
                        FlightId = s.FlightId,
                        Flight = new FlightDto
                        {
                            FlightId = s.Flight.FlightId,
                            FlightNumber = s.Flight.FlightNumber,
                            AirlineId = s.Flight.AirlineId,
                            Airline = new AirlineDto
                            {
                                AirlineId = s.Flight.Airline.AirlineId,
                                Name = s.Flight.Airline.Name,
                                Code = s.Flight.Airline.Code,
                                LogoUrl = s.Flight.Airline.LogoUrl
                            },
                            DepartureAirportId = s.Flight.DepartureAirportId,
                            DepartureAirport = new AirportDto
                            {
                                AirportId = s.Flight.DepartureAirport.AirportId,
                                Name = s.Flight.DepartureAirport.Name,
                                Code = s.Flight.DepartureAirport.Code,
                                City = s.Flight.DepartureAirport.City,
                                Country = s.Flight.DepartureAirport.Country
                            },
                            ArrivalAirportId = s.Flight.ArrivalAirportId,
                            ArrivalAirport = new AirportDto
                            {
                                AirportId = s.Flight.ArrivalAirport.AirportId,
                                Name = s.Flight.ArrivalAirport.Name,
                                Code = s.Flight.ArrivalAirport.Code,
                                City = s.Flight.ArrivalAirport.City,
                                Country = s.Flight.ArrivalAirport.Country
                            },
                            DepartureTime = s.Flight.DepartureTime,
                            ArrivalTime = s.Flight.ArrivalTime,
                            BasePrice = s.Flight.BasePrice
                        },
                        SeatNumber = s.SeatNumber,
                        Class = s.Class,
                        IsBooked = s.IsBooked,

                    })
                    .FirstOrDefaultAsync(s => s.SeatId == id);

                // Check if seat was found
                if (seat == null)
                {
                    _logger.LogWarning($"Seat with ID {id} not found");
                    return NotFound(new { message = "Seat not found" });
                }

                _logger.LogInformation($"Successfully retrieved seat with ID: {id}");
                return Ok(seat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving seat with ID: {id}");
                return StatusCode(500, new { error = "An error occurred while retrieving the seat", details = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new seat in the system.
        /// </summary>
        /// <param name="seat">The seat object to create.</param>
        /// <returns>
        /// 201 Created with the newly created seat if successful.
        /// 400 Bad Request if the model is invalid.
        /// 500 Internal Server Error if an exception occurs during processing.
        /// </returns>
        /// <remarks>
        /// This endpoint requires Admin role authorization.
        /// Validates the seat model before saving to database.
        /// </remarks>
        [HttpPost]
        public async Task<ActionResult<Seat>> AddSeat([FromBody] Seat seat)
        {
            try
            {
                _logger.LogInformation($"Attempting to add new seat for Flight ID: {seat.FlightId}, Seat Number: {seat.SeatNumber}");

                // Validate the model state
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid seat data received");
                    return BadRequest(ModelState);
                }

                // Additional validation could be added here
                // For example, check if the seat number already exists for this flight

                // Add the seat to the database
                _context.Seats.Add(seat);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully added seat with ID: {seat.SeatId}, Flight ID: {seat.FlightId}, Seat Number: {seat.SeatNumber}");
                return CreatedAtAction(nameof(GetSeat), new { id = seat.SeatId }, seat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding seat for Flight ID: {seat?.FlightId}, Seat Number: {seat?.SeatNumber}");
                return StatusCode(500, new { error = "An error occurred while adding the seat", details = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing seat in the system.
        /// </summary>
        /// <param name="id">The unique identifier of the seat to update.</param>
        /// <param name="seat">The updated seat object.</param>
        /// <returns>
        /// 204 No Content if the update is successful.
        /// 400 Bad Request if the ID doesn't match or the model is invalid.
        /// 404 Not Found if the seat with the specified ID doesn't exist.
        /// 500 Internal Server Error if an exception occurs during processing.
        /// </returns>
        /// <remarks>
        /// This endpoint requires Admin role authorization.
        /// Performs concurrency checks to ensure data integrity.
        /// </remarks>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSeat(int id, [FromBody] Seat seat)
        {
            // Validate ID consistency between route and body
            if (id != seat.SeatId)
            {
                _logger.LogWarning($"Seat ID mismatch in update request. Route ID: {id}, Body ID: {seat.SeatId}");
                return BadRequest(new { message = "Seat ID mismatch" });
            }

            try
            {
                _logger.LogInformation($"Updating seat with ID: {id}, Flight ID: {seat.FlightId}, Seat Number: {seat.SeatNumber}");

                // Validate the model state
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"Invalid seat data received for update of Seat ID: {id}");
                    return BadRequest(ModelState);
                }

                // Mark entity as modified and save changes
                _context.Entry(seat).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully updated seat with ID: {id}");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle concurrency conflicts
                if (!_context.Seats.Any(s => s.SeatId == id))
                {
                    _logger.LogWarning($"Seat with ID {id} not found during update operation due to concurrency issue");
                    return NotFound(new { message = "Seat not found" });
                }

                _logger.LogError(ex, $"Concurrency error updating seat with ID: {id}");
                throw; // Re-throw for further handling if needed
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating seat with ID: {id}");
                return StatusCode(500, new { error = "An error occurred while updating the seat", details = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a seat from the system.
        /// </summary>
        /// <param name="id">The unique identifier of the seat to delete.</param>
        /// <returns>
        /// 204 No Content if the deletion is successful.
        /// 404 Not Found if the seat with the specified ID doesn't exist.
        /// 500 Internal Server Error if an exception occurs during processing.
        /// </returns>
        /// <remarks>
        /// This endpoint requires Admin role authorization.
        /// Consider adding additional checks to prevent deletion of booked seats.
        /// </remarks>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSeat(int id)
        {
            try
            {
                _logger.LogInformation($"Attempting to delete seat with ID: {id}");

                // Find the seat
                var seat = await _context.Seats.FindAsync(id);

                // Check if seat exists
                if (seat == null)
                {
                    _logger.LogWarning($"Seat with ID {id} not found for deletion");
                    return NotFound(new { message = "Seat not found" });
                }

                // Potential business rule: Check if seat is currently booked before allowing deletion
                // if (seat.IsBooked)
                // {
                //     _logger.LogWarning($"Cannot delete seat with ID {id} because it is currently booked");
                //     return BadRequest(new { error = "Cannot delete a seat that is currently booked" });
                // }

                // Remove the seat from the database
                _context.Seats.Remove(seat);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully deleted seat with ID: {id}, Flight ID: {seat.FlightId}, Seat Number: {seat.SeatNumber}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting seat with ID: {id}");
                return StatusCode(500, new { error = "An error occurred while deleting the seat", details = ex.Message });
            }
        }
    }
}