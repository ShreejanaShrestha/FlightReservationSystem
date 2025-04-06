using FlightReservationSystem.Data;
using FlightReservationSystem.DTOs;
using FlightReservationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightReservationSystem.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFlightService _flightService;
        private readonly IPaymentService _paymentService;

        public BookingService(
            ApplicationDbContext context,
            IFlightService flightService,
            IPaymentService paymentService)
        {
            _context = context;
            _flightService = flightService;
            _paymentService = paymentService;
        }

        public async Task<BookingResult> CreateBookingAsync(BookingDTO bookingDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Create Booking
                var booking = new Booking
                {
                    UserId = bookingDto.UserId,
                    FlightId = bookingDto.FlightId,
                    BookingDate = DateTime.UtcNow,
                    Status = "Confirmed"
                };

                await _context.Bookings.AddAsync(booking);
                await _context.SaveChangesAsync();

                // 2. Add Passengers
                foreach (var passengerDto in bookingDto.Passengers)
                {
                    var seatClass = passengerDto.SeatClass; // A, B, or C
                    var seatNumber = await AssignSeat(booking.FlightId, seatClass);
                    var passenger = new Passenger
                    {
                        BookingId = booking.BookingId,
                        FullName = passengerDto.FullName,
                        PassportNumber = passengerDto.PassportNumber,
                        SeatNumber = await AssignSeat(booking.FlightId, seatClass)
                    };
                    await _context.Passengers.AddAsync(passenger);
                }

                // 3. Process Payment
                var payment = new Payment
                {
                    BookingId = booking.BookingId,
                    Amount = bookingDto.Payment.Amount,
                    Status = "Completed", // Assume payment is successful for now
                    TransactionId = Guid.NewGuid().ToString(),
                    PaymentDate = DateTime.UtcNow
                };
                await _context.Payments.AddAsync(payment);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new BookingResult
                {
                    Success = true,
                    BookingId = booking.BookingId
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BookingResult
                {
                    ErrorMessage = "An error occurred while processing your booking."
                };
            }
        }

        private async Task<string> AssignSeat(int flightId, string seatClass)
        {
            var lastSeat = await _context.Passengers
            .Where(p => p.Booking.FlightId == flightId && p.SeatNumber.StartsWith(seatClass))
            .OrderByDescending(p => p.SeatNumber)
            .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastSeat != null)
            {
                var lastNumber = int.Parse(lastSeat.SeatNumber.Substring(1));
                nextNumber = lastNumber + 1;
            }

            // Verify seat exists in the flight
            var seatExists = await _context.Seats
                .AnyAsync(s => s.FlightId == flightId && s.SeatNumber == $"{seatClass}{nextNumber}");

            if (!seatExists)
            {
                throw new Exception($"No available {GetClassName(seatClass)} seats");
            }

            return $"{seatClass}{nextNumber}";
        }

        private string GetClassName(string seatClass)
        {
            return seatClass switch
            {
                "A" => "First Class",
                "B" => "Business Class",
                "C" => "Economy Class",
                _ => "Unknown Class"
            };
        }
        public async Task<BookingDTO> GetBookingDetailsAsync(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Flight)
                .Include(b => b.Passengers)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return null;

            return new BookingDTO
            {
                BookingId = booking.BookingId,
                UserId = booking.UserId,
                FlightId = booking.FlightId,
                BookingDate = booking.BookingDate,
                Status = booking.Status,
                Flight = await _flightService.GetFlightByIdAsync(booking.FlightId),
                Passengers = booking.Passengers.Select(p => new PassengerDTO
                {
                    FullName = p.FullName,
                    PassportNumber = p.PassportNumber,
                    SeatNumber = p.SeatNumber,
                }).ToList(),
                Payment = new PaymentDTO
                {
                    Amount = booking.Payment?.Amount ?? 0,
                    TransactionId = booking.Payment?.TransactionId,
                    Status = booking.Payment?.Status,
                    
                }
            };
        }

        public async Task<List<BookingDTO>> GetUserBookingsAsync(string userId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Flight)
                .Include(b => b.Payment)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            var bookingDtos = new List<BookingDTO>();
            foreach (var booking in bookings)
            {
                bookingDtos.Add(new BookingDTO
                {
                    BookingId = booking.BookingId,
                    UserId = booking.UserId,
                    FlightId = booking.FlightId,
                    BookingDate = booking.BookingDate,
                    Status = booking.Status,
                    Flight = await _flightService.GetFlightByIdAsync(booking.FlightId),
                    Payment = new PaymentDTO
                    {
                        Amount = booking.Payment?.Amount ?? 0,
                        Status = booking.Payment?.Status
                    }
                });
            }

            return bookingDtos;
        }

        public async Task<OperationResult> CancelBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return new OperationResult { ErrorMessage = "Booking not found" };
            }

            if (booking.Status == "Cancelled")
            {
                return new OperationResult { ErrorMessage = "Booking is already cancelled" };
            }

            // Check if cancellation is allowed (e.g., not too close to departure)
            var flight = await _flightService.GetFlightByIdAsync(booking.FlightId);
            if (flight.DepartureTime < DateTime.Now.AddHours(24))
            {
                return new OperationResult { ErrorMessage = "Cancellation not allowed within 24 hours of departure" };
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Process refund if payment was successful
                if (booking.Payment?.Status == "Completed")
                {
                    var refundResult = await _paymentService.ProcessRefundAsync(
                        booking.Payment.TransactionId,
                        booking.Payment.Amount);

                    if (!refundResult.Success)
                    {
                        return new OperationResult { ErrorMessage = refundResult.ErrorMessage };
                    }
                }

                // Update booking status
                booking.Status = "Cancelled";
                if (booking.Payment != null)
                {
                    booking.Payment.Status = "Refunded";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new OperationResult { Success = true };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new OperationResult { ErrorMessage = "An error occurred while cancelling the booking" };
            }
        }
    }
}
