// Controllers/FlightStatusController.cs
using Microsoft.AspNetCore.Mvc;
using FlightReservationSystem.Data;
using FlightReservationSystem.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace FlightReservationSystem.Controllers
{
    [Route("api/[controller]")]
    public class FlightStatusController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FlightStatusController> _logger;

        public FlightStatusController(ApplicationDbContext context, ILogger<FlightStatusController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /FlightStatus
        [HttpGet]
        [Route("~/FlightStatus")]
        public IActionResult Index()
        {
            return View();
        }

        // POST: /FlightStatus/Check
        [HttpPost]
        [Route("~/FlightStatus/Check")]
        public async Task<IActionResult> CheckFlightStatusForm(FlightStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Index", request);
                }

                var booking = await _context.Bookings
                    .Include(b => b.Flight)
                        .ThenInclude(f => f.Airline)
                    .Include(b => b.Flight)
                        .ThenInclude(f => f.DepartureAirport)
                    .Include(b => b.Flight)
                        .ThenInclude(f => f.ArrivalAirport)
                    .Include(b => b.Passengers)
                    .FirstOrDefaultAsync(b => b.PNR == request.PNR);

                if (booking == null)
                {
                    ModelState.AddModelError("", "Booking not found");
                    return View("Index", request);
                }

                var passenger = booking.Passengers.FirstOrDefault(p => p.PassportNumber == request.PassportNumber);
                if (passenger == null)
                {
                    ModelState.AddModelError("", "Passport number does not match any passenger in this booking");
                    return View("Index", request);
                }

                var flightStatus = new FlightStatusDto
                {
                    PNR = booking.PNR,
                    FlightNumber = booking.Flight.FlightNumber,
                    Airline = booking.Flight.Airline.Name,
                    DepartureAirport = $"{booking.Flight.DepartureAirport.Name} ({booking.Flight.DepartureAirport.Code})",
                    ArrivalAirport = $"{booking.Flight.ArrivalAirport.Name} ({booking.Flight.ArrivalAirport.Code})",
                    DepartureTime = booking.Flight.DepartureTime,
                    ArrivalTime = booking.Flight.ArrivalTime,
                    Status = booking.Flight.Status.ToString(),
                    Passengers = booking.Passengers.Select(p => new PassengerDto
                    {
                        PassengerId = p.PassengerId,
                        BookingId = p.BookingId,
                        FullName = p.FullName,
                        PassportNumber = p.PassportNumber,
                        SeatNumber = p.SeatNumber
                    }).ToList()
                };

                return View("FlightStatus", flightStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking flight status");
                ModelState.AddModelError("", "An error occurred while checking flight status");
                return View("Index", request);
            }
        }
    }

    public class FlightStatusRequest
    {
        [Required]
        public string PNR { get; set; }

        [Required]
        public string PassportNumber { get; set; }
    }
}
