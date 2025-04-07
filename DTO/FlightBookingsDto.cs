namespace FlightReservationSystem.DTO
{
    public class FlightBookingsDto
    {
        public FlightDto Flight { get; set; }
        public List<BookingDto> Bookings { get; set; }
        public List<PassengerDto> Passengers { get; set; }
    }
}
