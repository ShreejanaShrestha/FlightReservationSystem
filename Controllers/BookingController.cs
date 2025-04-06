using FlightReservationSystem.DTOs;
using FlightReservationSystem.Models;
using FlightReservationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FlightReservationSystem.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IFlightService _flightService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPaymentService _paymentService;

        public BookingController(
            IBookingService bookingService,
            IFlightService flightService,
            UserManager<ApplicationUser> userManager,
            IPaymentService paymentService)
        {
            _bookingService = bookingService;
            _flightService = flightService;
            _userManager = userManager;
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int flightId)
        {
            var flight = await _flightService.GetFlightByIdAsync(flightId);
            if (flight == null)
            {
                return NotFound();
            }

            var model = new BookingViewModel
            {
                Flight = flight,
                Passengers = new List<PassengerViewModel>
            {
                new PassengerViewModel() // Default one passenger
            }
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(BookingViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                // Create booking DTO
                var bookingDto = new BookingDTO
                {
                    FlightId = model.Flight.FlightId,
                    UserId = user.Id,
                    Passengers = model.Passengers.Select(p => new PassengerDTO
                    {
                        FullName = p.FullName,
                        PassportNumber = p.PassportNumber,
                        SeatClass = p.SeatClass,
                        SeatNumber = p.SeatNumber,
                        
                    }).ToList(),
                    Payment = new PaymentDTO
                    {
                        Amount = model.TotalPrice
                    }
                };

                var result = await _bookingService.CreateBookingAsync(bookingDto);

                if (result.Success)
                {
                    return RedirectToAction("Details", new { id = result.BookingId });
                }

                ModelState.AddModelError("", result.ErrorMessage ?? "Failed to create booking");
            }

            // Reload flight data if validation fails
            model.Flight = await _flightService.GetFlightByIdAsync(model.Flight.FlightId);
            return View(model);
        }

        

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _bookingService.GetBookingDetailsAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            // Verify the booking belongs to the current user
            var user = await _userManager.GetUserAsync(User);
            if (booking.UserId != user.Id)
            {
                return Forbid();
            }

            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _bookingService.CancelBookingAsync(id);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var user = await _userManager.GetUserAsync(User);
            var bookings = await _bookingService.GetUserBookingsAsync(user.Id);
            return View(bookings);
        }
    }
}
