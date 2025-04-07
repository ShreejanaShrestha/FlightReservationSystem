

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
    /// <summary>
    /// Controller responsible for handling flight status related operations.
    /// Provides endpoints for checking current flight status based on PNR and passport number.
    /// </summary>
    [Route("api/[controller]")]
    public class FlightStatusController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FlightStatusController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlightStatusController"/> class.
        /// </summary>
        /// <param name="context">The database context for accessing flight and booking data.</param>
        /// <param name="logger">The logger for capturing runtime events and errors.</param>
        public FlightStatusController(ApplicationDbContext context, ILogger<FlightStatusController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the flight status check form view.
        /// </summary>
        /// <returns>The view containing the flight status check form.</returns>
        [HttpGet]
        [Route("~/FlightStatus")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Processes flight status check requests and returns flight status information.
        /// </summary>
        /// <param name="request">The flight status request containing PNR and passport number.</param>
        /// <returns>
        /// Returns the flight status view with details if found.
        /// Returns the form view with validation errors if request is invalid or data not found.
        /// </returns>
        /// <remarks>
        /// This endpoint:
        /// 1. Validates the request model
        /// 2. Queries the database for booking information
        /// 3. Verifies passenger details
        /// 4. Constructs a comprehensive flight status response
        /// 5. Handles errors gracefully with appropriate logging
        /// </remarks>
        [HttpPost]
        [Route("~/FlightStatus/Check")]
        public async Task<IActionResult> CheckFlightStatusForm(FlightStatusRequest request)
        {
            try
            {
                // Validate model state before processing
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for flight status check request");
                    return View("Index", request);
                }

                // Include all related entities in a single query to avoid multiple roundtrips
                var booking = await _context.Bookings
                    .Include(b => b.Flight)
                        .ThenInclude(f => f.Airline)
                    .Include(b => b.Flight)
                        .ThenInclude(f => f.DepartureAirport)
                    .Include(b => b.Flight)
                        .ThenInclude(f => f.ArrivalAirport)
                    .Include(b => b.Passengers)
                    .AsNoTracking() // Improve performance for read-only operations
                    .FirstOrDefaultAsync(b => b.PNR == request.PNR);

                if (booking == null)
                {
                    _logger.LogWarning("Booking not found for PNR: {PNR}", request.PNR);
                    ModelState.AddModelError("", "Booking not found");
                    return View("Index", request);
                }

                // Verify passenger belongs to the booking
                var passenger = booking.Passengers.FirstOrDefault(p => p.PassportNumber == request.PassportNumber);
                if (passenger == null)
                {
                    _logger.LogWarning("Passport number {PassportNumber} not found for booking {PNR}",
                        request.PassportNumber, request.PNR);
                    ModelState.AddModelError("", "Passport number does not match any passenger in this booking");
                    return View("Index", request);
                }

                // Map entity to DTO for view presentation
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
                        PassportNumber = MaskSensitiveData(p.PassportNumber), // Security consideration
                        SeatNumber = p.SeatNumber
                    }).ToList()
                };

                _logger.LogInformation("Successfully retrieved flight status for PNR: {PNR}", request.PNR);
                return View("FlightStatus", flightStatus);
            }
            catch (Exception ex)
            {
                // Log the full error but show a generic message to users
                _logger.LogError(ex, "Error checking flight status for PNR: {PNR}", request?.PNR);
                ModelState.AddModelError("", "An error occurred while checking flight status");
                return View("Index", request);
            }
        }

        /// <summary>
        /// Masks sensitive data for display purposes.
        /// </summary>
        /// <param name="data">The sensitive data to mask.</param>
        /// <returns>A masked version of the sensitive data.</returns>
        private string MaskSensitiveData(string data)
        {
            if (string.IsNullOrEmpty(data) || data.Length < 4)
            {
                return "****"; // Return minimal masking if data is too short
            }

            // Show last 4 characters, mask the rest
            return new string('*', data.Length - 4) + data.Substring(data.Length - 4);
        }
    }

    /// <summary>
    /// Represents a flight status check request containing PNR and passport number.
    /// </summary>
    public class FlightStatusRequest
    {
        /// <summary>
        /// Gets or sets the Passenger Name Record (PNR) code.
        /// </summary>
        /// <example>ABC123</example>
        [Required(ErrorMessage = "PNR is required")]
        [StringLength(10, MinimumLength = 5, ErrorMessage = "PNR must be between 5 and 10 characters")]
        public string PNR { get; set; }

        /// <summary>
        /// Gets or sets the passenger's passport number.
        /// </summary>
        /// <example>P12345678</example>
        [Required(ErrorMessage = "Passport number is required")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Passport number must be between 8 and 20 characters")]
        public string PassportNumber { get; set; }
    }
}