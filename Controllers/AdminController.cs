// FlightReservationSystem/Controllers/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FlightReservationSystem.Data;
using FlightReservationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightReservationSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Admin Index action called.");

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

        // POST: /Admin/ResetDatabase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetDatabase()
        {
            _logger.LogInformation("ResetDatabase action called.");

            try
            {
                // Sign out the user
                await _signInManager.SignOutAsync();
                _logger.LogInformation("Signed out any existing user sessions.");

               

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
                _logger.LogInformation("Seeded airports.");

                // Seed Airlines
                var airlines = new List<Airline>
                {
                    new Airline { Name = "Delta Airlines", Code = "DL" },
                    new Airline { Name = "British Airways", Code = "BA" },
                    new Airline { Name = "American Airlines", Code = "AA" }
                };
                _context.Airlines.AddRange(airlines);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Seeded airlines.");

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
                _logger.LogInformation("Seeded flights with seats.");

                // Seed a test user
                var testUser = new ApplicationUser
                {
                    UserName = "testuser@example.com",
                    Email = "testuser@example.com",
                    EmailConfirmed = true,
                    FirstName = "Test",
                    LastName = "User"
                };
                var result = await _userManager.CreateAsync(testUser, "Test@1234");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(testUser, "Admin");
                    _logger.LogInformation("Test user created and assigned to Admin role: testuser@example.com (Password: Test@1234)");
                }
                else
                {
                    _logger.LogError("Failed to create test user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                    TempData["Error"] = "Failed to create test user: " + string.Join(", ", result.Errors.Select(e => e.Description));
                    return RedirectToAction("Index");
                }

                TempData["Message"] = "Database reset successfully. Please log in again.";
                _logger.LogInformation("Database reset successfully.");
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting database.");
                TempData["Error"] = "An error occurred while resetting the database. Please try again.";
                return RedirectToAction("Index");
            }
        }
    }
}