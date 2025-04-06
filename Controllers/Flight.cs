using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightReservationSystem.Models;
using FlightReservationSystem.Data;
using FlightReservationSystem.DTO;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;


namespace FlightReservationSystem.Controllers
{
    public class FlightController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public FlightController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5106/");
        }

        // GET: /Flight/Search
        public IActionResult Search()
        {
            ViewBag.Airports = _context.Airports.ToList();
            return View();
        }

        // POST: /Flight/Search
        [HttpPost]
        public async Task<IActionResult> Search(int departureAirportId, int arrivalAirportId, DateTime? date)
        {
            // Log the input values
            Console.WriteLine($"Received: departureAirportId={departureAirportId}, arrivalAirportId={arrivalAirportId}, date={date}");

            if (departureAirportId == 0 || arrivalAirportId == 0)
            {
                Console.WriteLine("Validation failed: Departure or arrival airport not selected.");
                ModelState.AddModelError("", "Please select both departure and arrival airports.");
                ViewBag.Airports = _context.Airports.ToList();
                return View();
            }

            if (departureAirportId == arrivalAirportId)
            {
                Console.WriteLine("Validation failed: Departure and arrival airports are the same.");
                ModelState.AddModelError("", "Departure and arrival airports cannot be the same.");
                ViewBag.Airports = _context.Airports.ToList();
                return View();
            }

            try
            {
                var query = $"api/flights/search?departureAirportId={departureAirportId}&arrivalAirportId={arrivalAirportId}";
                if (date.HasValue)
                {
                    query += $"&date={date.Value:yyyy-MM-dd}";
                }

                Console.WriteLine($"Calling API: {query}");
                var response = await _httpClient.GetAsync(query);
                if (response.IsSuccessStatusCode)
                {
                    var flights = await response.Content.ReadFromJsonAsync<List<FlightDto>>();
                    Console.WriteLine($"API returned {flights?.Count ?? 0} flights.");
                    if (flights == null || !flights.Any())
                    {
                        TempData["Message"] = "No available flights found matching your criteria.";
                    }

                    ViewBag.Airports = _context.Airports.ToList();
                    return View("SearchResults", flights);
                }

                Console.WriteLine($"API call failed: {response.StatusCode}");
                ModelState.AddModelError("", "No flights found.");
                ViewBag.Airports = _context.Airports.ToList();
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while searching for flights. Please try again.");
                ViewBag.Airports = _context.Airports.ToList();
                return View();
            }
        }
        // POST: /Flight/SelectFlight
        [HttpPost]
        public IActionResult SelectFlight(int flightId)
        {
            // Validate the flight ID
            var flight = _context.Flights
                .Include(f => f.Airline)
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Seats)
                .FirstOrDefault(f => f.FlightId == flightId);

            if (flight == null)
            {
                TempData["Error"] = "Selected flight not found.";
                return RedirectToAction("Search");
            }

            // Check if the flight has available seats
            if (!flight.Seats.Any(s => !s.IsBooked))
            {
                TempData["Error"] = "No available seats on this flight.";
                return RedirectToAction("Search");
            }

            // Store the selected flight ID in TempData
            TempData["SelectedFlightId"] = flightId;

            // Redirect to the flight review page
            return RedirectToAction("ReviewFlight");
        }
        // GET: /Flight/ReviewFlight
        public IActionResult ReviewFlight()
        {
            // Retrieve the selected flight ID from TempData
            if (!TempData.ContainsKey("SelectedFlightId"))
            {
                TempData["Error"] = "No flight selected. Please search and select a flight.";
                return RedirectToAction("Search");
            }

            int flightId = (int)TempData["SelectedFlightId"];
            TempData.Keep("SelectedFlightId"); // Keep the TempData for the next step

            var flight = _context.Flights
                .Include(f => f.Airline)
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Seats)
                .FirstOrDefault(f => f.FlightId == flightId);

            if (flight == null)
            {
                TempData["Error"] = "Selected flight not found.";
                return RedirectToAction("Search");
            }

            return View(flight);
        }

        // GET: /Flight/AddPassengerDetails
        public IActionResult AddPassengerDetails()
        {
            // Ensure a flight is selected
            if (!TempData.ContainsKey("SelectedFlightId"))
            {
                TempData["Error"] = "No flight selected. Please search and select a flight.";
                return RedirectToAction("Search");
            }

            TempData.Keep("SelectedFlightId");
            return View();
        }

        // POST: /Flight/AddPassengerDetails
        [HttpPost]
        public IActionResult AddPassengerDetails(List<PassengerDto> passengers)
        {
            // Ensure a flight is selected
            if (!TempData.ContainsKey("SelectedFlightId"))
            {
                TempData["Error"] = "No flight selected. Please search and select a flight.";
                return RedirectToAction("Search");
            }

            // Validate passenger details
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

            // Store passenger details in TempData (serialize to JSON)
            TempData["PassengerDetails"] = System.Text.Json.JsonSerializer.Serialize(passengers);
            TempData.Keep("SelectedFlightId");

            // Redirect to the booking confirmation step
            return RedirectToAction("ConfirmBooking");
        }
        

// GET: /Flight/ConfirmBooking
[Authorize] // Requires the user to be logged in
    public IActionResult ConfirmBooking()
    {
        // Ensure a flight is selected
        if (!TempData.ContainsKey("SelectedFlightId"))
        {
            TempData["Error"] = "No flight selected. Please search and select a flight.";
            return RedirectToAction("Search");
        }

        // Ensure passenger details are provided
        if (!TempData.ContainsKey("PassengerDetails"))
        {
            TempData["Error"] = "Passenger details are missing. Please add passenger details.";
            return RedirectToAction("AddPassengerDetails");
        }

        TempData.Keep("SelectedFlightId");
        TempData.Keep("PassengerDetails");

        // In a real application, this would render a payment page
        // For now, we'll simulate the payment and proceed to booking
        return RedirectToAction("ProcessBooking");
    }
        // GET: /Flight/ProcessBooking
        [Authorize]
        public async Task<IActionResult> ProcessBooking()
        {
            Console.WriteLine("Starting ProcessBooking...");

            if (!TempData.ContainsKey("SelectedFlightId"))
            {
                Console.WriteLine("SelectedFlightId not found in TempData.");
                TempData["Error"] = "No flight selected. Please search and select a flight.";
                return RedirectToAction("Search");
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
                Console.WriteLine("Fetching flight details with lock...");
                var flight = await _context.Flights
                    .Include(f => f.Seats)
                    .FirstOrDefaultAsync(f => f.FlightId == flightId);

                if (flight == null)
                {
                    Console.WriteLine("Flight not found in database.");
                    TempData["Error"] = "Selected flight not found.";
                    return RedirectToAction("Search");
                }

                var availableSeats = flight.Seats.Where(s => !s.IsBooked).ToList();
                Console.WriteLine($"Available seats: {availableSeats.Count}");

                if (availableSeats.Count < passengers.Count)
                {
                    Console.WriteLine("Not enough available seats.");
                    TempData["Error"] = "Not enough available seats for the number of passengers.";
                    return RedirectToAction("Search");
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

                // Verify that the user exists in the database
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

                // Mark seats as booked and assign them to passengers
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
                        SeatNumber = seat.SeatNumber // Assign the SeatNumber from the booked seat
                    };
                    _context.Passengers.Add(passenger);
                }

                Console.WriteLine("Saving changes to database...");
                await _context.SaveChangesAsync();

                Console.WriteLine("Booking successful.");
                TempData["BookingId"] = booking.BookingId;
                TempData["ConfirmationMessage"] = $"Booking confirmed! Your booking ID is {booking.BookingId}.";
                return RedirectToAction("BookingConfirmation");
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
                return RedirectToAction("Search");
            }
        }

        // GET: /Flight/BookingConfirmation
        public IActionResult BookingConfirmation()
        {
            if (!TempData.ContainsKey("ConfirmationMessage"))
            {
                TempData["Error"] = "No booking confirmation available.";
                return RedirectToAction("Search");
            }

            ViewBag.ConfirmationMessage = TempData["ConfirmationMessage"];
            ViewBag.BookingId = TempData["BookingId"];
            return View();
        }
    }
}