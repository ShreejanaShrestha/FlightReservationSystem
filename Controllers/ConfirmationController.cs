// FlightReservationSystem/Controllers/ConfirmationController.cs
using Microsoft.AspNetCore.Mvc;
using FlightReservationSystem.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightReservationSystem.Controllers
{
    /// <summary>
    /// Controller responsible for handling booking confirmation operations in the Flight Reservation System.
    /// Provides functionality to display booking confirmation details to users after successful bookings.
    /// </summary>
    public class ConfirmationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConfirmationController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmationController"/> class.
        /// </summary>
        /// <param name="context">The database context for booking-related operations.</param>
        /// <param name="logger">The logger for recording operation details and errors.</param>
        /// <exception cref="ArgumentNullException">Thrown when context or logger is null.</exception>
        public ConfirmationController(ApplicationDbContext context, ILogger<ConfirmationController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the booking confirmation page with detailed information about a completed booking.
        /// </summary>
        /// <param name="bookingId">The unique identifier of the booking to display.</param>
        /// <returns>
        /// The booking confirmation view with booking details if the booking exists;
        /// otherwise, redirects to the home page with an error message.
        /// </returns>
        /// <remarks>
        /// This action loads the booking with all related entities including:
        /// - Flight information
        /// - Airline details
        /// - Departure and arrival airport information
        /// - Passenger information
        /// </remarks>
        public async Task<IActionResult> BookingConfirmation(int bookingId)
        {
            _logger.LogInformation($"BookingConfirmation action called with bookingId: {bookingId}");

            // Validate input parameter
            if (bookingId <= 0)
            {
                _logger.LogWarning($"Invalid bookingId provided: {bookingId}");
                TempData["Error"] = "Booking confirmation failed. Please try again.";
                return RedirectToAction("Index", "Home");
            }

            _logger.LogInformation($"Fetching booking with ID: {bookingId}");

            // Retrieve booking with all related data for the confirmation page
            var booking = await _context.Bookings
                .Include(b => b.Flight)
                    .ThenInclude(f => f.Airline)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                .Include(b => b.Passengers)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            // Handle case when booking is not found
            if (booking == null)
            {
                _logger.LogWarning($"Booking with ID {bookingId} not found.");
                TempData["Error"] = "Booking not found.";
                return RedirectToAction("Index", "Home");
            }

            // Map database entities to view model for the presentation layer
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

            // Set success message for the confirmation page
            ViewBag.ConfirmationMessage = $"Booking confirmed! Your booking reference is {booking.PNR}.";

            _logger.LogInformation($"Successfully retrieved booking details for ID: {bookingId}, PNR: {booking.PNR}");
            return View(model);
        }
    }

    /// <summary>
    /// View model representing booking confirmation data for presentation to the user.
    /// Contains all the essential information about a booking, including flight and passenger details.
    /// </summary>
    public class BookingConfirmationViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the booking.
        /// </summary>
        public int BookingId { get; set; }

        /// <summary>
        /// Gets or sets the flight identifier associated with this booking.
        /// </summary>
        public int FlightId { get; set; }

        /// <summary>
        /// Gets or sets the Passenger Name Record (PNR) - the unique booking reference code.
        /// </summary>
        public string PNR { get; set; }

        /// <summary>
        /// Gets or sets the flight number displayed to the user.
        /// </summary>
        public string FlightNumber { get; set; }

        /// <summary>
        /// Gets or sets the name of the airline operating the flight.
        /// </summary>
        public string Airline { get; set; }

        /// <summary>
        /// Gets or sets the departure airport name and code.
        /// </summary>
        public string DepartureAirport { get; set; }

        /// <summary>
        /// Gets or sets the arrival airport name and code.
        /// </summary>
        public string ArrivalAirport { get; set; }

        /// <summary>
        /// Gets or sets the scheduled departure time of the flight.
        /// </summary>
        public DateTime DepartureTime { get; set; }

        /// <summary>
        /// Gets or sets the scheduled arrival time of the flight.
        /// </summary>
        public DateTime ArrivalTime { get; set; }

        /// <summary>
        /// Gets or sets the base ticket price for the flight.
        /// </summary>
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Gets or sets the collection of passengers included in this booking.
        /// </summary>
        public List<PassengerViewModel> Passengers { get; set; }
    }

    /// <summary>
    /// View model representing passenger information for the booking confirmation.
    /// Contains essential traveler details for display on the booking confirmation page.
    /// </summary>
    public class PassengerViewModel
    {
        /// <summary>
        /// Gets or sets the full name of the passenger.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the passport number of the passenger.
        /// </summary>
        public string PassportNumber { get; set; }

        /// <summary>
        /// Gets or sets the assigned seat number for the passenger.
        /// </summary>
        public string SeatNumber { get; set; }
    }
}