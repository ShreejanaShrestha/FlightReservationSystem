namespace FlightReservationSystem.DTOs
{
    public class BookingDTO
    {
        public int BookingId { get; set; }
        public string UserId { get; set; }
        public int FlightId { get; set; }
        public DateTime BookingDate { get; set; }
        public string Status { get; set; }
        public List<PassengerDTO> Passengers { get; set; }
        public PaymentDTO Payment { get; set; }
        public FlightDTO Flight { get; set; }
    }
}
