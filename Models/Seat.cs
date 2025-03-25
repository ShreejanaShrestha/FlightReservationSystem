using System.ComponentModel.DataAnnotations;

namespace FlightReservationSystem.Models
{
    public class Seat
    {
        public int SeatId { get; set; }

        // Foreign Key
        public int FlightId { get; set; }

        [Required, MaxLength(10)]
        public string SeatNumber { get; set; }     // e.g., "12A"

        [Required, MaxLength(20)]
        public string Class { get; set; }         // "Economy", "Business", "First"

        public bool IsBooked { get; set; } = false;

        // Navigation
        public Flight Flight { get; set; }
    }
}
