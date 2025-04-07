using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightReservationSystem.Data;
using FlightReservationSystem.DTO;
using FlightReservationSystem.Models;

namespace FlightReservationSystem.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: /Booking/SelectFlight
        [HttpPost]
        public IActionResult SelectFlight(int flightId)
        {
            var flight = _context.Flights
                .Include(f => f.Airline)
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Seats)
                .FirstOrDefault(f => f.FlightId == flightId);

            if (flight == null)
            {
                TempData["Error"] = "Selected flight not found.";
                return RedirectToAction("Search", "Search");
            }

            if (!flight.Seats.Any(s => !s.IsBooked))
            {
                TempData["Error"] = "No available seats on this flight.";
                return RedirectToAction("Search", "Search");
            }

            TempData["SelectedFlightId"] = flightId;
            return RedirectToAction("ReviewFlight");
        }

        // GET: /Booking/ReviewFlight
        public IActionResult ReviewFlight()
        {
            if (!TempData.ContainsKey("SelectedFlightId"))
            {
                TempData["Error"] = "No flight selected. Please search and select a flight.";
                return RedirectToAction("Search", "Search");
            }

            int flightId = (int)TempData["SelectedFlightId"];
            TempData.Keep("SelectedFlightId");

            var flight = _context.Flights
                .Include(f => f.Airline)
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Seats)
                .FirstOrDefault(f => f.FlightId == flightId);

            if (flight == null)
            {
                TempData["Error"] = "Selected flight not found.";
                return RedirectToAction("Search", "Search");
            }

            return View(flight);
        }

        // GET: /Booking/AddPassengerDetails
        public IActionResult AddPassengerDetails()
        {
            if (!TempData.ContainsKey("SelectedFlightId"))
            {
                TempData["Error"] = "No flight selected. Please search and select a flight.";
                return RedirectToAction("Search", "Search");
            }

            TempData.Keep("SelectedFlightId");
            return View();
        }

        // POST: /Booking/AddPassengerDetails
        [HttpPost]
        public IActionResult AddPassengerDetails(List<PassengerDto> passengers)
        {
            if (!TempData.ContainsKey("SelectedFlightId"))
            {
                TempData["Error"] = "No flight selected. Please search and select a flight.";
                return RedirectToAction("Search", "Search");
            }

            if (passengers == null || !passengers.Any())
            {
                ModelState.AddModelError("", "Please add at least one passenger.");
                TempData.Keep("SelectedFlightId");
                return View();
            }

            foreach (var passenger in passengers)
            {
                if (string.IsNullOrWhiteSpace(passenger.FullName))
                {
                    ModelState.AddModelError("", "All passengers must have a full name.");
                    TempData.Keep("SelectedFlightId");
                    return View();
                }
            }

            TempData["PassengerDetails"] = System.Text.Json.JsonSerializer.Serialize(passengers);
            TempData.Keep("SelectedFlightId");
            return RedirectToAction("ConfirmBooking");
        }

        // GET: /Booking/ConfirmBooking
        [Authorize]
        public IActionResult ConfirmBooking()
        {
            if (!TempData.ContainsKey("SelectedFlightId"))
            {
                TempData["Error"] = "No flight selected. Please search and select a flight.";
                return RedirectToAction("Search", "Search");
            }

            if (!TempData.ContainsKey("PassengerDetails"))
            {
                TempData["Error"] = "Passenger details are missing. Please add passenger details.";
                return RedirectToAction("AddPassengerDetails");
            }

            TempData.Keep("SelectedFlightId");
            TempData.Keep("PassengerDetails");
            return RedirectToAction("ProcessBooking");
        }

        // GET: /Booking/ProcessBooking
        [Authorize]
        public async Task<IActionResult> ProcessBooking()
        {
            Console.WriteLine("Starting ProcessBooking...");

            if (!TempData.ContainsKey("SelectedFlightId"))
            {
                Console.WriteLine("SelectedFlightId not found in TempData.");
                TempData["Error"] = "No flight selected. Please search and select a flight.";
                return RedirectToAction("Search", "Search");
            }

            if (!TempData.ContainsKey("PassengerDetails"))
            {
                Console.WriteLine("PassengerDetails not found in TempData.");
                TempData["Error"] = "Passenger details are missing. Please add passenger details.";
                return RedirectToAction("AddPassengerDetails");
            }

            if (!User.Identity.IsAuthenticated)
            {
                Console.WriteLine("User is not authenticated. Redirecting to login.");
                return RedirectToAction("Login", "Account");
            }

            int flightId = (int)TempData["SelectedFlightId"];
            Console.WriteLine($"Selected FlightId: {flightId}");

            var passengerDetailsJson = TempData["PassengerDetails"]?.ToString();
            var passengers = System.Text.Json.JsonSerializer.Deserialize<List<PassengerDto>>(passengerDetailsJson);
            Console.WriteLine($"Number of passengers: {passengers.Count}");

            try
            {
                Console.WriteLine("Fetching flight details...");
                var flight = await _context.Flights
                    .Include(f => f.Seats)
                    .FirstOrDefaultAsync(f => f.FlightId == flightId);

                if (flight == null)
                {
                    Console.WriteLine("Flight not found in database.");
                    TempData["Error"] = "Selected flight not found.";
                    return RedirectToAction("Search", "Search");
                }

                var availableSeats = flight.Seats.Where(s => !s.IsBooked).ToList();
                Console.WriteLine($"Available seats: {availableSeats.Count}");

                if (availableSeats.Count < passengers.Count)
                {
                    Console.WriteLine("Not enough available seats.");
                    TempData["Error"] = "Not enough available seats for the number of passengers.";
                    return RedirectToAction("Search", "Search");
                }

                Console.WriteLine("Simulating payment...");
                bool paymentSuccessful = true;

                if (!paymentSuccessful)
                {
                    Console.WriteLine("Payment failed.");
                    TempData["Error"] = "Payment failed. Please try again.";
                    return RedirectToAction("ConfirmBooking");
                }

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"UserId: {userId ?? "null"}");

                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("UserId is null. Redirecting to login.");
                    return RedirectToAction("Login", "Account");
                }

                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    Console.WriteLine($"User with ID {userId} does not exist in the database. Signing out and redirecting to login.");
                    var signInManager = HttpContext.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
                    await signInManager.SignOutAsync();
                    TempData["Error"] = "Your session is invalid. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                Console.WriteLine("Creating booking...");
                var booking = new Booking
                {
                    UserId = userId,
                    FlightId = flightId,
                    BookingDate = DateTime.Now,
                    Status = "Confirmed"
                };

                _context.Bookings.Add(booking);

                var bookedSeats = new List<Seat>();
                Console.WriteLine("Marking seats as booked...");
                for (int i = 0; i < passengers.Count; i++)
                {
                    var seat = availableSeats[i];
                    seat.IsBooked = true;
                    _context.Seats.Update(seat);
                    bookedSeats.Add(seat);
                }

                Console.WriteLine("Adding passengers...");
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

                Console.WriteLine("Saving changes to database...");
                await _context.SaveChangesAsync();

                Console.WriteLine("Booking successful.");
                TempData["BookingId"] = booking.BookingId;
                TempData["ConfirmationMessage"] = $"Booking confirmed! Your booking ID is {booking.BookingId}.";
                return RedirectToAction("BookingConfirmation", "Confirmation");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProcessBooking: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                TempData["Error"] = "An error occurred while processing your booking. Please try again.";
                return RedirectToAction("Search", "Search");
            }
        }

       
        [Authorize]
        public async Task<IActionResult> ManageBookings()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

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
    }
}