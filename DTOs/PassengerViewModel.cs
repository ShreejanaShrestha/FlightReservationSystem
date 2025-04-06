using System.ComponentModel.DataAnnotations;

namespace FlightReservationSystem.DTOs
{
    public class PassengerViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Passport number is required")]
        [Display(Name = "Passport Number")]
        public string PassportNumber { get; set; }

        [Display(Name = "Seat Number")]
        public string SeatNumber { get; set; }

        [Required(ErrorMessage = "Seat class is required")]
        public string SeatClass { get; set; } // A, B, or C
    }
}
