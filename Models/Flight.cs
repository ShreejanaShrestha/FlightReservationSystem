using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FlightReservationSystem.Models
{
    public class Flight
    {
        public int FlightId { get; set; }

        [Required, MaxLength(10)]
        public string FlightNumber { get; set; }   // e.g., "DL123"

        // Foreign Keys
        public int AirlineId { get; set; }
        public int DepartureAirportId { get; set; }
        public int ArrivalAirportId { get; set; }

        // Flight Details
        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal BasePrice { get; set; }

        // Navigation
        public Airline Airline { get; set; }
        public Airport DepartureAirport { get; set; }
        public Airport ArrivalAirport { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}
