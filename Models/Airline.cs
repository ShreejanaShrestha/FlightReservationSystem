using System.ComponentModel.DataAnnotations;

namespace FlightReservationSystem.Models
{
    public class Airline
    {
        public int AirlineId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }          // e.g., "Delta Airlines"

        [Required, MaxLength(2)]
        public string Code { get; set; }          // e.g., "DL" (IATA code)

        [MaxLength(200)]
        public string? LogoUrl { get; set; }      // e.g., "/images/airlines/delta.png"

        public ICollection<Flight> Flights { get; set; } = new List<Flight>();
    }
}
