namespace FlightReservationSystem.DTOs
{
    public class PassengerDTO
    {
        public int PassengerId { get; set; }
        public int BookingId { get; set; }
        public string FullName { get; set; }
        public string PassportNumber { get; set; }
        public string SeatNumber { get; set; }
        public string SeatClass { get; set; }
    }
}
