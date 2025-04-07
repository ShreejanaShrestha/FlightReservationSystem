using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FlightReservationSystem.Data;
using FlightReservationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightReservationSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
      
        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminPanelViewModel
            {
                Flights = await _context.Flights
                    .Include(f => f.Airline)
                    .Include(f => f.DepartureAirport)
                    .Include(f => f.ArrivalAirport)
                    .ToListAsync(),
                Airports = await _context.Airports.ToListAsync(),
                Airlines = await _context.Airlines.ToListAsync(),
                Bookings = await _context.Bookings
                    .Include(b => b.Flight)
                    .Include(b => b.Passengers)
                    .ToListAsync(),
                Users = await _userManager.Users.ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ResetDatabase()
        {
            try
            {
                // Sign out the user
                await _signInManager.SignOutAsync();
                Console.WriteLine("Signed out any existing user sessions.");

                // Clear all data
                _context.ClearAllData();

                // Seed Airports
                var airports = new List<Airport>
                {
                    new Airport { Name = "John F. Kennedy International Airport", Code = "JFK", City = "New York", Country = "USA" },
                    new Airport { Name = "Los Angeles International Airport", Code = "LAX", City = "Los Angeles", Country = "USA" },
                    new Airport { Name = "Heathrow Airport", Code = "LHR", City = "London", Country = "UK" },
                    new Airport { Name = "Chicago O'Hare International Airport", Code = "ORD", City = "Chicago", Country = "USA" }
                };
                _context.Airports.AddRange(airports);
                await _context.SaveChangesAsync();

                // Seed Airlines
                var airlines = new List<Airline>
                {
                    new Airline { Name = "Delta Airlines", Code = "DL" },
                    new Airline { Name = "British Airways", Code = "BA" },
                    new Airline { Name = "American Airlines", Code = "AA" }
                };
                _context.Airlines.AddRange(airlines);
                await _context.SaveChangesAsync();

                // Seed Flights with Seats
                var flights = new List<Flight>
                {
                    new Flight
                    {
                        FlightNumber = "DL123",
                        AirlineId = airlines[0].AirlineId,
                        DepartureAirportId = airports[0].AirportId,
                        ArrivalAirportId = airports[1].AirportId,
                        DepartureTime = new DateTime(2025, 4, 7, 10, 0, 0),
                        ArrivalTime = new DateTime(2025, 4, 7, 13, 0, 0),
                        BasePrice = 300.00m,
                        Seats = new List<Seat>
                        {
                            new Seat { SeatNumber = "1A", Class = "Economy", IsBooked = false },
                            new Seat { SeatNumber = "1B", Class = "Economy", IsBooked = false },
                            new Seat { SeatNumber = "2A", Class = "Business", IsBooked = false }
                        }
                    },
                    new Flight
                    {
                        FlightNumber = "BA456",
                        AirlineId = airlines[1].AirlineId,
                        DepartureAirportId = airports[0].AirportId,
                        ArrivalAirportId = airports[2].AirportId,
                        DepartureTime = new DateTime(2025, 4, 8, 15, 0, 0),
                        ArrivalTime = new DateTime(2025, 4, 8, 23, 0, 0),
                        BasePrice = 600.00m,
                        Seats = new List<Seat>
                        {
                            new Seat { SeatNumber = "1A", Class = "Economy", IsBooked = false },
                            new Seat { SeatNumber = "1B", Class = "Economy", IsBooked = false },
                            new Seat { SeatNumber = "2A", Class = "First", IsBooked = false }
                        }
                    },
                    new Flight
                    {
                        FlightNumber = "AA789",
                        AirlineId = airlines[2].AirlineId,
                        DepartureAirportId = airports[3].AirportId,
                        ArrivalAirportId = airports[1].AirportId,
                        DepartureTime = new DateTime(2025, 4, 9, 12, 0, 0),
                        ArrivalTime = new DateTime(2025, 4, 9, 14, 30, 0),
                        BasePrice = 250.00m,
                        Seats = new List<Seat>
                        {
                            new Seat { SeatNumber = "1A", Class = "Economy", IsBooked = false },
                            new Seat { SeatNumber = "1B", Class = "Economy", IsBooked = false }
                        }
                    }
                };
                _context.Flights.AddRange(flights);
                await _context.SaveChangesAsync();

                // Seed a test user
                var testUser = new ApplicationUser
                {
                    UserName = "testuser@example.com",
                    Email = "testuser@example.com",
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(testUser, "Test@1234");
                if (result.Succeeded)
                {
                    // Assign the user to the Admin role
                    await _userManager.AddToRoleAsync(testUser, "Admin");
                    Console.WriteLine("Test user created and assigned to Admin role: testuser@example.com (Password: Test@1234)");
                }
                else
                {
                    Console.WriteLine("Failed to create test user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                TempData["Message"] = "Database reset successfully. Please log in again.";
                return RedirectToAction("Search", "Flight");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting database: {ex.Message}");
                TempData["Error"] = "An error occurred while resetting the database. Please try again.";
                return RedirectToAction("Search", "Flight");
            }
        }
    }
}