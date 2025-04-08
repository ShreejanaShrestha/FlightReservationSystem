using FlightReservationSystem.Data;
using FlightReservationSystem.DTO;
using FlightReservationSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlightReservationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(ApplicationDbContext context, ILogger<BookingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/bookings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookings()
        {
            _logger.LogInformation("GetBookings API called.");

            var bookings = await _context.Bookings
                .Include(b => b.Flight)
                .Include(b => b.Passengers)
                .Include(b => b.User) // Include the user to access UserName
                .Select(b => new BookingDto
                {
                    BookingId = b.BookingId,
                    UserId = b.UserId,
                    UserName = b.User.UserName, // this replaces showing only the user ID
                    FlightId = b.FlightId,
                   
                    BookingDate = b.BookingDate,
                    Status = b.Status,
                    Passengers = b.Passengers.Select(p => new PassengerDto
                    {
                        PassengerId = p.PassengerId,
                      
                        PassportNumber = p.PassportNumber
                    }).ToList()
                })
                .ToListAsync();

            return Ok(bookings);
        }
        // GET: api/bookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDto>> GetBookingById(int id)
        {
            _logger.LogInformation($"GetBooking API called for BookingId: {id}");

            var booking = await _context.Bookings
                .Include(b => b.Flight)
                .Include(b => b.Passengers)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                _logger.LogWarning($"Booking with ID {id} not found.");
                return NotFound();
            }

            var bookingDto = new BookingDto
            {
                BookingId = booking.BookingId,
                UserId = booking.UserId,
                UserName = booking.User?.UserName,
                FlightId = booking.FlightId,
                Flight = new FlightDto
                {
                    FlightId = booking.Flight.FlightId,
                    FlightNumber = booking.Flight.FlightNumber,
                   
                    DepartureTime = booking.Flight.DepartureTime,
                    ArrivalTime = booking.Flight.ArrivalTime
                },
                BookingDate = booking.BookingDate,
                Status = booking.Status,
                Passengers = booking.Passengers.Select(p => new PassengerDto
                {
                    PassengerId = p.PassengerId,
                    FullName=p.FullName,
                    SeatNumber = p.SeatNumber,
                    PassportNumber = p.PassportNumber
                }).ToList()
            };

            return Ok(bookingDto);
        }

    }
}
