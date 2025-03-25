using System.ComponentModel.DataAnnotations;

namespace FlightReservationSystem.Models
{
    public class Passenger
    {
        public int PassengerId { get; set; }

        // Foreign Key
        public int BookingId { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(20)]
        public string PassportNumber { get; set; }

        [MaxLength(10)]
        public string SeatNumber { get; set; }    // e.g., "12A"

        // Navigation
        public Booking Booking { get; set; }
    }
}
