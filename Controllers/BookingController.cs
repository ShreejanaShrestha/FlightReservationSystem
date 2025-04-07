// FlightReservationSystem/Controllers/BookingController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightReservationSystem.Data;
using FlightReservationSystem.DTO;
using FlightReservationSystem.Models;
using FlightReservationSystem.Services;

namespace FlightReservationSystem.Controllers
{
    /// <summary>
    /// Controller for handling all booking-related operations
    /// </summary>
    /// <remarks>
    /// This controller manages the complete booking workflow including:
    /// - Flight selection
    /// - Passenger details collection
    /// - Booking confirmation
    /// - Payment processing
    /// - Booking management
    /// </remarks>
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingController> _logger;

        /// <summary>
        /// Initializes a new instance of the BookingController
        /// </summary>
        /// <param name="context">Database context for booking operations</param>
        /// <param name="logger">Logger for error and information logging</param>
        public BookingController(ApplicationDbContext context, ILogger<BookingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Handles flight selection and stores the selected flight in session
        /// </summary>
        /// <param name="flightId">ID of the selected flight</param>
        /// <returns>
        /// Redirects to ReviewFlight if successful
        /// Redirects to Search with error message if flight not found or no seats available
        /// </returns>
        [HttpPost]
        public IActionResult SelectFlight(int flightId)
        {
            _logger.LogInformation($"SelectFlight called with flightId: {flightId}");

            // Retrieve flight with all related data
            var flight = _context.Flights
                .Include(f => f.Airline)
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Seats)
                .FirstOrDefault(f => f.FlightId == flightId);

            if (flight == null)
            {
                _logger.LogWarning($"Flight with ID {flightId} not found.");
                TempData["Error"] = "Selected flight not found.";
                return RedirectToAction("Search", "Search");
            }

            // Check seat availability
            if (!flight.Seats.Any(s => !s.IsBooked))
            {
                _logger.LogWarning($"No available seats on flight with ID {flightId}.");
                TempData["Error"] = "No available seats on this flight.";
                return RedirectToAction("Search", "Search");
            }

            // Store selected flight in session
            HttpContext.Session.SetInt32("SelectedFlightId", flightId);
            _logger.LogInformation($"Stored SelectedFlightId {flightId} in session.");

            return RedirectToAction("ReviewFlight");
        }

        /// <summary>
        /// Displays flight details for review before proceeding with booking
        /// </summary>
        /// <returns>
        /// ReviewFlight view if successful
        /// Redirects to Search with error message if no flight selected
        /// </returns>
        public IActionResult ReviewFlight()
        {
            _logger.LogInformation("ReviewFlight action called.");

            // Retrieve selected flight from session
            var flightId = HttpContext.Session.GetInt32("SelectedFlightId");
            if (!flightId.HasValue)
            {
                _logger.LogWarning("SelectedFlightId not found in session during ReviewFlight.");
                TempData["Error"] = "No flight selected. Please search and select a flight.";
                return RedirectToAction("Search", "Search");
            }

            // Retrieve complete flight details
            var flight = _context.Flights
                .Include(f => f.Airline)
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Seats)
                .FirstOrDefault(f => f.FlightId == flightId.Value);

            if (flight == null)
            {
                _logger.LogWarning($"Flight with ID {flightId.Value} not found.");
                TempData["Error"] = "Selected flight not found.";
                return RedirectToAction("Search", "Search");
            }

            return View(flight);
        }

        /// <summary>
        /// Displays form for collecting passenger details
        /// </summary>
        /// <returns>
        /// AddPassengerDetails view if successful
        /// Redirects to Search with error message if no flight selected
        /// </returns>
        public IActionResult AddPassengerDetails()
        {
            _logger.LogInformation("AddPassengerDetails GET action called.");

            // Verify flight selection
            var flightId = HttpContext.Session.GetInt32("SelectedFlightId");
            if (!flightId.HasValue)
            {
                _logger.LogWarning("SelectedFlightId not found in session during AddPassengerDetails GET.");
                TempData["Error"] = "No flight selected. Please search and select a flight.";
                return RedirectToAction("Search", "Search");
            }

            // Prepare view model with flight details
            var flight = _context.Flights
                .FirstOrDefault(f => f.FlightId == flightId.Value);

            if (flight == null)
            {
                _logger.LogWarning($"Flight with ID {flightId.Value} not found.");
                TempData["Error"] = "Selected flight not found.";
                return RedirectToAction("Search", "Search");
            }

            var model = new AddPassengerDetailsViewModel
            {
                FlightId = flightId.Value,
                BasePrice = flight.BasePrice,
                Passengers = new List<PassengerDto> { new PassengerDto() }
            };

            return View(model);
        }

        /// <summary>
        /// Processes submitted passenger details and assigns seats
        /// </summary>
        /// <param name="model">Passenger details view model</param>
        /// <returns>
        /// Redirects to ConfirmBooking if successful
        /// Returns to form with validation errors if invalid
        /// Redirects to Search with error message if issues occur
        /// </returns>
        [HttpPost]
        public IActionResult AddPassengerDetails(AddPassengerDetailsViewModel model)
        {
            _logger.LogInformation("AddPassengerDetails POST action called.");

            // Verify flight selection
            var flightId = HttpContext.Session.GetInt32("SelectedFlightId");
            if (!flightId.HasValue)
            {
                _logger.LogWarning("SelectedFlightId not found in session during AddPassengerDetails POST.");
                TempData["Error"] = "No flight selected. Please search and select a flight.";
                return RedirectToAction("Search", "Search");
            }

            // Retrieve flight with seat availability
            var flight = _context.Flights
                .Include(f => f.Seats)
                .FirstOrDefault(f => f.FlightId == flightId.Value);

            if (flight == null)
            {
                _logger.LogWarning($"Flight with ID {flightId.Value} not found.");
                TempData["Error"] = "Selected flight not found.";
                return RedirectToAction("Search", "Search");
            }

            // Clear ModelState errors for SeatNumber since we assign programmatically
            for (int i = 0; i < model.Passengers.Count; i++)
            {
                ModelState.Remove($"Passengers[{i}].SeatNumber");
            }

            // Validate model
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid in AddPassengerDetails POST.");
                model.FlightId = flightId.Value;
                model.BasePrice = flight.BasePrice;
                return View(model);
            }

            // Check seat availability
            var availableSeats = flight.Seats
                .Where(s => !s.IsBooked)
                .ToList();

            if (availableSeats.Count < model.Passengers.Count)
            {
                _logger.LogWarning("Not enough available seats for the number of passengers.");
                TempData["Error"] = "Not enough available seats for the number of passengers. Please select a different flight.";
                return RedirectToAction("Search", "Search");
            }

            // Assign seats to passengers
            for (int i = 0; i < model.Passengers.Count; i++)
            {
                var passenger = model.Passengers[i];
                var seat = availableSeats[i];
                passenger.SeatNumber = seat.SeatNumber;
                _logger.LogInformation($"Assigned seat {seat.SeatNumber} to passenger {passenger.FullName}");
            }

            // Store passenger details in session
            try
            {
                var passengerDetailsJson = System.Text.Json.JsonSerializer.Serialize(model.Passengers);
                HttpContext.Session.SetString("PassengerDetails", passengerDetailsJson);
                _logger.LogInformation("Stored PassengerDetails in session.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store PassengerDetails in session.");
                TempData["Error"] = "An error occurred while saving passenger details. Please try again.";
                model.FlightId = flightId.Value;
                model.BasePrice = flight.BasePrice;
                return View(model);
            }

            return RedirectToAction("ConfirmBooking");
        }

        /// <summary>
        /// Displays booking confirmation page with all details
        /// </summary>
        /// <returns>
        /// ConfirmBooking view if successful
        /// Redirects to appropriate steps if data is missing
        /// </returns>
        public IActionResult ConfirmBooking()
        {
            _logger.LogInformation("ConfirmBooking GET action called.");

            // Verify required session data exists
            var flightId = HttpContext.Session.GetInt32("SelectedFlightId");
            if (!flightId.HasValue)
            {
                _logger.LogWarning("SelectedFlightId not found in session during ConfirmBooking.");
                TempData["Error"] = "No flight selected. Please search and select a flight.";
                return RedirectToAction("Search", "Search");
            }

            var passengerDetailsJson = HttpContext.Session.GetString("PassengerDetails");
            if (string.IsNullOrEmpty(passengerDetailsJson))
            {
                _logger.LogWarning("PassengerDetails not found in session during ConfirmBooking.");
                TempData["Error"] = "Passenger details are missing. Please add passenger details.";
                return RedirectToAction("AddPassengerDetails");
            }

            // Retrieve flight details
            var flight = _context.Flights
                .FirstOrDefault(f => f.FlightId == flightId.Value);

            if (flight == null)
            {
                _logger.LogWarning($"Flight with ID {flightId.Value} not found.");
                TempData["Error"] = "Selected flight not found.";
                return RedirectToAction("Search", "Search");
            }

            // Deserialize passenger details
            List<PassengerDto> passengers;
            try
            {
                passengers = System.Text.Json.JsonSerializer.Deserialize<List<PassengerDto>>(passengerDetailsJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize PassengerDetails in ConfirmBooking.");
                TempData["Error"] = "An error occurred while retrieving passenger details. Please try again.";
                return RedirectToAction("AddPassengerDetails");
            }

            // Prepare confirmation view model
            var model = new ConfirmBookingViewModel
            {
                FlightId = flightId.Value,
                FlightNumber = flight.FlightNumber,
                DepartureAirport = flight.DepartureAirport?.Name,
                ArrivalAirport = flight.ArrivalAirport?.Name,
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                BasePrice = flight.BasePrice,
                Passengers = passengers
            };

            return View(model);
        }

        /// <summary>
        /// Processes the final booking and payment
        /// </summary>
        /// <returns>
        /// Redirects to booking confirmation page if successful
        /// Redirects to appropriate steps if issues occur
        /// </returns>
        [Authorize]
        public async Task<IActionResult> ProcessBooking()
        {
            _logger.LogInformation("Starting ProcessBooking...");

            // Verify required session data
            var flightId = HttpContext.Session.GetInt32("SelectedFlightId");
            if (!flightId.HasValue)
            {
                _logger.LogWarning("SelectedFlightId not found in Session.");
                TempData["Error"] = "No flight selected. Please search and select a flight.";
                return RedirectToAction("Search", "Search");
            }

            var passengerDetailsJson = HttpContext.Session.GetString("PassengerDetails");
            if (string.IsNullOrEmpty(passengerDetailsJson))
            {
                _logger.LogWarning("PassengerDetails not found in Session.");
                TempData["Error"] = "Passenger details are missing. Please add passenger details.";
                return RedirectToAction("AddPassengerDetails");
            }

            // Deserialize passenger details
            List<PassengerDto> passengers;
            try
            {
                passengers = System.Text.Json.JsonSerializer.Deserialize<List<PassengerDto>>(passengerDetailsJson);
                _logger.LogInformation($"Number of passengers: {passengers.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize PassengerDetails.");
                TempData["Error"] = "An error occurred while processing passenger details. Please try again.";
                return RedirectToAction("AddPassengerDetails");
            }

            try
            {
                // Retrieve flight with seat availability
                var flight = await _context.Flights
                    .Include(f => f.Seats)
                    .FirstOrDefaultAsync(f => f.FlightId == flightId);

                if (flight == null)
                {
                    _logger.LogWarning("Flight not found in database.");
                    TempData["Error"] = "Selected flight not found.";
                    return RedirectToAction("Search", "Search");
                }

                // Verify seat availability
                var availableSeats = flight.Seats.Where(s => !s.IsBooked).ToList();
                var assignedSeatNumbers = passengers.Select(p => p.SeatNumber).ToList();
                var bookedSeats = new List<Seat>();

                foreach (var seatNumber in assignedSeatNumbers)
                {
                    var seat = availableSeats.FirstOrDefault(s => s.SeatNumber == seatNumber);
                    if (seat == null)
                    {
                        _logger.LogWarning($"Seat {seatNumber} is not available.");
                        TempData["Error"] = $"Seat {seatNumber} is no longer available. Please try booking again.";
                        return RedirectToAction("AddPassengerDetails");
                    }
                    seat.IsBooked = true;
                    _context.Seats.Update(seat);
                    bookedSeats.Add(seat);
                }

                // Get current user ID
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId is null. Redirecting to login.");
                    return RedirectToAction("Login", "Account");
                }

                // Create booking record
                var booking = new Booking
                {
                    UserId = userId,
                    FlightId = flightId.Value,
                    BookingDate = DateTime.UtcNow,
                    Status = "Confirmed",
                    PNR = await PNRGenerator.GenerateUniquePNR(_context)
                };

                _context.Bookings.Add(booking);

                // Add passengers to booking
                for (int i = 0; i < passengers.Count; i++)
                {
                    var passengerDto = passengers[i];
                    var seat = bookedSeats[i];
                    var passenger = new Passenger
                    {
                        Booking = booking,
                        FullName = passengerDto.FullName,
                        PassportNumber = passengerDto.PassportNumber,
                        SeatNumber = seat.SeatNumber
                    };
                    _context.Passengers.Add(passenger);
                }

                // Save all changes
                await _context.SaveChangesAsync();

                // Clear session after successful booking
                HttpContext.Session.Remove("SelectedFlightId");
                HttpContext.Session.Remove("PassengerDetails");

                return RedirectToAction("BookingConfirmation", "Confirmation", new { bookingId = booking.BookingId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessBooking");
                TempData["Error"] = "An error occurred while processing your booking. Please try again.";
                return RedirectToAction("Search", "Search");
            }
        }

        /// <summary>
        /// Retrieves booking details by ID
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <returns>
        /// Booking details view if authorized and found
        /// Redirects to ManageBookings with error message if issues occur
        /// </returns>
        [Authorize]
        public async Task<IActionResult> GetBooking(int id)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Retrieve booking with all related data
                var booking = await _context.Bookings
                    .Include(b => b.Flight)
                        .ThenInclude(f => f.DepartureAirport)
                    .Include(b => b.Flight)
                        .ThenInclude(f => f.ArrivalAirport)
                    .Include(b => b.Flight)
                        .ThenInclude(f => f.Airline)
                    .Include(b => b.Passengers)
                    .FirstOrDefaultAsync(b => b.BookingId == id);

                if (booking == null)
                {
                    TempData["Error"] = "Booking not found.";
                    return RedirectToAction("ManageBookings");
                }

                // Ensure user can only view their own bookings
                if (booking.UserId != userId)
                {
                    TempData["Error"] = "You are not authorized to view this booking.";
                    return RedirectToAction("ManageBookings");
                }

                return View(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving booking");
                TempData["Error"] = "An error occurred while retrieving the booking.";
                return RedirectToAction("ManageBookings");
            }
        }

        /// <summary>
        /// Displays PNR lookup form
        /// </summary>
        /// <returns>PNR lookup view</returns>
        public IActionResult ByPNR()
        {
            return View();
        }

        /// <summary>
        /// Looks up booking by PNR number
        /// </summary>
        /// <param name="pnr">PNR reference number</param>
        /// <returns>
        /// Booking details view if found
        /// Returns to form with error message if not found or error occurs
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> LookupByPNR(string pnr)
        {
            if (string.IsNullOrWhiteSpace(pnr))
            {
                TempData["Error"] = "Please enter a valid booking reference number.";
                return RedirectToAction("ByPNR");
            }

            try
            {
                // Retrieve booking by PNR with all related data
                var booking = await _context.Bookings
                    .Include(b => b.Flight)
                        .ThenInclude(f => f.DepartureAirport)
                    .Include(b => b.Flight)
                        .ThenInclude(f => f.ArrivalAirport)
                    .Include(b => b.Flight)
                        .ThenInclude(f => f.Airline)
                    .Include(b => b.Passengers)
                    .FirstOrDefaultAsync(b => b.PNR == pnr.Trim().ToUpper());

                if (booking == null)
                {
                    TempData["Error"] = "No booking found with the provided reference number.";
                    return RedirectToAction("ByPNR");
                }

                return View("BookingDetails", booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up booking by PNR");
                TempData["Error"] = "An error occurred while looking up the booking.";
                return RedirectToAction("ByPNR");
            }
        }

        /// <summary>
        /// Displays all bookings for the current user
        /// </summary>
        /// <returns>
        /// ManageBookings view with user's bookings
        /// Redirects to login if not authenticated
        /// </returns>
        [Authorize]
        public async Task<IActionResult> ManageBookings()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve all bookings for current user
            var bookings = await _context.Bookings
                .Include(b => b.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.Airline)
                .Include(b => b.Passengers)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            return View(bookings);
        }

        // GET: api/bookings
        [HttpGet]
        public async Task<ActionResult<object>> GetBookings()
        {
            _logger.LogInformation("GetBookings API called.");

            var bookings = await _context.Bookings
                .Include(b => b.Flight)
                .Include(b => b.Passengers)
                .ToListAsync();

            return bookings;
        }
        // GET: api/bookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetBookingById(int id)
        {
            _logger.LogInformation($"GetBooking API called for BookingId: {id}");

            var booking = await _context.Bookings
                .Include(b => b.Flight)
                .Include(b => b.Passengers)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                _logger.LogWarning($"Booking with ID {id} not found.");
                return NotFound();
            }

            return booking;
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            _logger.LogInformation($"CancelBooking API called for BookingId: {id}");

            var booking = await _context.Bookings
                .Include(b => b.Passengers)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                _logger.LogWarning($"Booking with ID {id} not found.");
                return NotFound(new { message = "Booking not found" });
            }

            if (booking.Status == "Cancelled")
            {
                _logger.LogWarning($"Booking {id} is already cancelled.");
                return BadRequest(new { error = "Booking is already cancelled." });
            }

            booking.Status = "Cancelled";

            // Mark seats as unbooked
            var seatNumbers = booking.Passengers.Select(p => p.SeatNumber).ToList();
            var seats = await _context.Seats
                .Where(s => s.FlightId == booking.FlightId && seatNumbers.Contains(s.SeatNumber))
                .ToListAsync();

            foreach (var seat in seats)
            {
                seat.IsBooked = false;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Booking {id} cancelled successfully.");
            return NoContent();
        }

    }
}