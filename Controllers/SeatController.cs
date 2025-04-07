// FlightReservationSystem/Controllers/SeatsController.cs
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
    public class SeatsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SeatsController> _logger;

        public SeatsController(ApplicationDbContext context, ILogger<SeatsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/seats
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SeatDto>>> GetSeats()
        {
            try
            {
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

                if (!seats.Any())
                {
                    return NotFound(new { message = "No seats found" });
                }
                return Ok(seats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving seats");
                return StatusCode(500, new { error = "An error occurred while retrieving seats", details = ex.Message });
            }
        }

        // GET: api/seats/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SeatDto>> GetSeat(int id)
        {
            try
            {
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

                if (seat == null)
                {
                    return NotFound(new { message = "Seat not found" });
                }
                return Ok(seat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving seat");
                return StatusCode(500, new { error = "An error occurred while retrieving the seat", details = ex.Message });
            }
        }

        // POST: api/seats
        [HttpPost]
        public async Task<ActionResult<Seat>> AddSeat([FromBody] Seat seat)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _context.Seats.Add(seat);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetSeat), new { id = seat.SeatId }, seat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding seat");
                return StatusCode(500, new { error = "An error occurred while adding the seat", details = ex.Message });
            }
        }

        // PUT: api/seats/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSeat(int id, [FromBody] Seat seat)
        {
            if (id != seat.SeatId)
            {
                return BadRequest(new { message = "Seat ID mismatch" });
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _context.Entry(seat).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Seats.Any(s => s.SeatId == id))
                {
                    return NotFound(new { message = "Seat not found" });
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating seat");
                return StatusCode(500, new { error = "An error occurred while updating the seat", details = ex.Message });
            }
        }

        // DELETE: api/seats/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSeat(int id)
        {
            try
            {
                var seat = await _context.Seats.FindAsync(id);
                if (seat == null)
                {
                    return NotFound(new { message = "Seat not found" });
                }

                _context.Seats.Remove(seat);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting seat");
                return StatusCode(500, new { error = "An error occurred while deleting the seat", details = ex.Message });
            }
        }
    }
}