using FlightReservationSystem.DTOs;

namespace FlightReservationSystem.Services
{
    public interface IBookingService
    {
        Task<BookingResult> CreateBookingAsync(BookingDTO booking);
        Task<BookingDTO> GetBookingDetailsAsync(int id);
        Task<List<BookingDTO>> GetUserBookingsAsync(string userId);
        Task<OperationResult> CancelBookingAsync(int bookingId);
    }

    public class BookingResult
    {
        public bool Success { get; set; }
        public int? BookingId { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class OperationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
