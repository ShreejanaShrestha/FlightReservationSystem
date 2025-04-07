// FlightReservationSystem/DTO/BookingDto.cs
namespace FlightReservationSystem.DTO
{
    public class BookingDto
    {
        public int BookingId { get; set; }
        public string UserId { get; set; }
        public int FlightId { get; set; }
        public FlightDto Flight { get; set; }
        public DateTime BookingDate { get; set; }
        public string Status { get; set; }
        public List<PassengerDto> Passengers { get; set; }
    }
}