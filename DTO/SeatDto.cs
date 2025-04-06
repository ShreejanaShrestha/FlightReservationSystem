using System.ComponentModel.DataAnnotations;

namespace FlightReservationSystem.DTO
{
    public class SeatDto
    {
        public int SeatId { get; set; }

        public int FlightId { get; set; }

        [Required, MaxLength(10)]
        public string SeatNumber { get; set; }

        [Required, MaxLength(20)]
        public string Class { get; set; }

        public bool IsBooked { get; set; }
    }
}