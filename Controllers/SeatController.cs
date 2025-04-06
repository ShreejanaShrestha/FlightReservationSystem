using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightReservationSystem.Models;
using FlightReservationSystem.Data;
using FlightReservationSystem.DTO;
using System;
using System.Threading.Tasks;
using FlightReservationSystem.DTO;

namespace FlightReservationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeatsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SeatsController> _logger;

        public SeatsController(ApplicationDbContext context, ILogger<SeatsController> logger)
        {
            _context = context;
            _logger = logger;
        }



        // GET: api/seats/flight/{flightId}
        [HttpGet("flight/{flightId}")]
        public async Task<ActionResult<IEnumerable<SeatDto>>> GetSeatsForFlight(int flightId)
        {
            try
            {
                var flight = await _context.Flights.FindAsync(flightId);
                if (flight == null)
                {
                    return NotFound(new { message = $"Flight with ID {flightId} not found" });
                }

                var seats = await _context.Seats
                    .Where(s => s.FlightId == flightId)
                    .Select(s => new SeatDto
                    {
                        SeatId = s.SeatId,
                        FlightId = s.FlightId,
                        SeatNumber = s.SeatNumber,
                        Class = s.Class,
                        IsBooked = s.IsBooked
                    })
                    .ToListAsync();

                if (!seats.Any())
                {
                    return NotFound(new { message = $"No seats found for Flight ID {flightId}" });
                }

                return Ok(seats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving seats for Flight ID {flightId}");
                return StatusCode(500, new { error = "An error occurred while retrieving seats", details = ex.Message });
            }
        }

        // POST: api/seats
        [HttpPost]
        public async Task<ActionResult<SeatDto>> AddSeat([FromBody] SeatDto seatDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate that the Flight exists
                var flight = await _context.Flights.FindAsync(seatDto.FlightId);
                if (flight == null)
                {
                    return BadRequest(new { error = $"Flight with ID {seatDto.FlightId} not found" });
                }

                // Check if the seat number is already taken for this flight
                var existingSeat = await _context.Seats
                    .Where(s => s.FlightId == seatDto.FlightId && s.SeatNumber == seatDto.SeatNumber)
                    .FirstOrDefaultAsync();
                if (existingSeat != null)
                {
                    return BadRequest(new { error = $"Seat number {seatDto.SeatNumber} already exists for Flight ID {seatDto.FlightId}" });
                }

                var seat = new Seat
                {
                    FlightId = seatDto.FlightId,
                    SeatNumber = seatDto.SeatNumber,
                    Class = seatDto.Class,
                    IsBooked = seatDto.IsBooked
                };

                _context.Seats.Add(seat);
                await _context.SaveChangesAsync();

                // Map to SeatDto for the response
                var seatResponse = new SeatDto
                {
                    SeatId = seat.SeatId,
                    FlightId = seat.FlightId,
                    SeatNumber = seat.SeatNumber,
                    Class = seat.Class,
                    IsBooked = seat.IsBooked
                };

                return CreatedAtAction(nameof(AddSeat), new { id = seat.SeatId }, seatResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding seat");
                return StatusCode(500, new { error = "An error occurred while adding the seat", details = ex.Message });
            }
        }
    }
}