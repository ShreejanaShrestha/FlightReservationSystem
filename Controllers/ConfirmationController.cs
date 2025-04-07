// FlightReservationSystem/Controllers/ConfirmationController.cs
using Microsoft.AspNetCore.Mvc;
using FlightReservationSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FlightReservationSystem.Controllers
{
    public class ConfirmationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConfirmationController> _logger;

        public ConfirmationController(ApplicationDbContext context, ILogger<ConfirmationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Confirmation/BookingConfirmation
        public async Task<IActionResult> BookingConfirmation(int bookingId)
        {
            _logger.LogInformation($"BookingConfirmation action called with bookingId: {bookingId}");

            if (bookingId <= 0)
            {
                _logger.LogWarning("Invalid bookingId provided.");
                TempData["Error"] = "Booking confirmation failed. Please try again.";
                return RedirectToAction("Index", "Home");
            }

            _logger.LogInformation($"Fetching booking with ID: {bookingId}");

            var booking = await _context.Bookings
                .Include(b => b.Flight)
                    .ThenInclude(f => f.Airline)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                .Include(b => b.Passengers)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                _logger.LogWarning($"Booking with ID {bookingId} not found.");
                TempData["Error"] = "Booking not found.";
                return RedirectToAction("Index", "Home");
            }

            var model = new BookingConfirmationViewModel
            {
                BookingId = booking.BookingId,
                PNR = booking.PNR,
                FlightNumber = booking.Flight.FlightNumber,
                Airline = booking.Flight.Airline.Name,
                DepartureAirport = $"{booking.Flight.DepartureAirport.Name} ({booking.Flight.DepartureAirport.Code})",
                ArrivalAirport = $"{booking.Flight.ArrivalAirport.Name} ({booking.Flight.ArrivalAirport.Code})",
                DepartureTime = booking.Flight.DepartureTime,
                ArrivalTime = booking.Flight.ArrivalTime,
                BasePrice = booking.Flight.BasePrice,
                Passengers = booking.Passengers.Select(p => new PassengerViewModel
                {
                    FullName = p.FullName,
                    PassportNumber = p.PassportNumber,
                    SeatNumber = p.SeatNumber
                }).ToList()
            };

            ViewBag.ConfirmationMessage = $"Booking confirmed! Your booking reference is {booking.PNR}.";

            return View(model);
        }
    }

    public class BookingConfirmationViewModel
    {
        public int BookingId { get; set; }
        public int FlightId { get; set; }
        public string PNR { get; set; }
        public string FlightNumber { get; set; }
        public string Airline { get; set; }
        public string DepartureAirport { get; set; }
        public string ArrivalAirport { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal BasePrice { get; set; }
        public List<PassengerViewModel> Passengers { get; set; }
    }

    public class PassengerViewModel
    {
        public string FullName { get; set; }
        public string PassportNumber { get; set; }
        public string SeatNumber { get; set; }
    }
}