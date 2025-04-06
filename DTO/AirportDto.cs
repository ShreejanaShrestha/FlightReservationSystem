using System.ComponentModel.DataAnnotations;

namespace FlightReservationSystem.DTO
{
    public class AirportDto
    {
        public int AirportId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(3)]
        public string Code { get; set; }

        [Required, MaxLength(50)]
        public string City { get; set; }

        [Required, MaxLength(50)]
        public string Country { get; set; }
    }
}