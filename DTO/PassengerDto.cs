using System.ComponentModel.DataAnnotations;

namespace FlightReservationSystem.DTO
{
    public class PassengerDto
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(20)]
        public string PassportNumber { get; set; }
    }
}