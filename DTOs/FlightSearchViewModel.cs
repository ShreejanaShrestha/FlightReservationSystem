using System.ComponentModel.DataAnnotations;

namespace FlightReservationSystem.DTOs
{
    public class FlightSearchViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Please select a departure airport.")]
        public string DepartureAirport { get; set; }

        [Required(ErrorMessage = "Please select an arrival airport.")]
        public string ArrivalAirport { get; set; }

        [Required(ErrorMessage = "Please select a departure date.")]
        public DateTime DepartureDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public List<string> Airports { get; set; } = new List<string>();
        public List<FlightDTO> AvailableFlights { get; set; } = new List<FlightDTO>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ReturnDate.HasValue && ReturnDate.Value < DepartureDate)
            {
                yield return new ValidationResult(
                    "Return date must be after departure date.",
                    new[] { nameof(ReturnDate) });
            }
        }
    }
}