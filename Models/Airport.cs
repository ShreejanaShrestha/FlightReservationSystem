using System.ComponentModel.DataAnnotations;

namespace FlightReservationSystem.Models
{
    public class Airport
    {
        public int AirportId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }          // e.g., "John F. Kennedy Airport"

        [Required, MaxLength(3)]
        public string Code { get; set; }          // e.g., "JFK"

        [Required, MaxLength(50)]
        public string City { get; set; }

        [Required, MaxLength(50)]
        public string Country { get; set; }

        // Navigation
        public ICollection<Flight> DepartureFlights { get; set; } = new List<Flight>();
        public ICollection<Flight> ArrivalFlights { get; set; } = new List<Flight>();
    }
}
