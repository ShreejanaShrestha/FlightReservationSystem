// FlightReservationSystem/DTO/PassengerDto.cs
namespace FlightReservationSystem.DTO
{
    public class PassengerDto
    {
        public int PassengerId { get; set; }
        public int BookingId { get; set; }
        public string FullName { get; set; }
        public string PassportNumber { get; set; }
        public string SeatNumber { get; set; }
    }
}