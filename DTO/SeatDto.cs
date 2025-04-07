using System.ComponentModel.DataAnnotations;

namespace FlightReservationSystem.DTO
{
    public class SeatDto
    {
        public int SeatId { get; set; }
        public int FlightId { get; set; }
        public FlightDto Flight { get; set; }
        public string SeatNumber { get; set; }
        public string Class { get; set; }
        public bool IsBooked { get; set; }
        public DateTime? ReservedUntil { get; set; }
    }
}