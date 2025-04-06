/*using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using FlightReservationSystem.Validation;

namespace FlightReservationSystem.DTOs
{
    public class FlightDTO
    {
        public int FlightId { get; set; }

        [Required]
        [StringLength(10)]
        public string FlightNumber { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select an airline")]
        public int AirlineId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select departure airport")]
        public int DepartureAirportId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select arrival airport")]
        public int ArrivalAirportId { get; set; }

        [Required]
        [FutureDate(ErrorMessage = "Departure time must be in the future")]
        public DateTime DepartureTime { get; set; }

        [Required]
        [DateAfter("DepartureTime", ErrorMessage = "Arrival time must be after departure")]
        public DateTime ArrivalTime { get; set; }

        [Range(0.01, 100000)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal BasePrice { get; set; }
    }
}*/

// DTOs/FlightDTO.cs
/*using FlightReservationSystem.Validation;
using System.ComponentModel.DataAnnotations;

public class FlightDTO
{
    public int FlightId { get; set; }

    [Required]
    [StringLength(10)]
    public string FlightNumber { get; set; }

    [Required]
    public int DepartureAirportId { get; set; }

    [Required]
    public int ArrivalAirportId { get; set; }

    [Required]
    [FutureDate(ErrorMessage = "Departure time must be in the future")]
    public DateTime DepartureTime { get; set; }

    [Required]
    [DateAfter("DepartureTime", ErrorMessage = "Arrival time must be after departure")]
    public DateTime ArrivalTime { get; set; }

    [Range(1, 10000)]
    public decimal Price { get; set; }
}

// Custom validation attribute
public class DateAfterAttribute : ValidationAttribute
{
    private readonly string _comparisonProperty;

    public DateAfterAttribute(string comparisonProperty)
    {
        _comparisonProperty = comparisonProperty;
    }

    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        var currentValue = (DateTime)value;
        var property = context.ObjectType.GetProperty(_comparisonProperty);
        var comparisonValue = (DateTime)property.GetValue(context.ObjectInstance);

        return currentValue > comparisonValue
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage);
    }
}*/

using FlightReservationSystem.Validation;
using System.ComponentModel.DataAnnotations;

namespace FlightReservationSystem.DTOs
{
    public class FlightDTO
    {
        public int FlightId { get; set; }
        [Required]
        [StringLength(10, MinimumLength = 3)]
        [RegularExpression(@"^[A-Z]{2,3}\d{3,4}$",
            ErrorMessage = "Flight number format invalid (e.g., AA123)")]
        public string FlightNumber { get; set; }
        public string AirlineName { get; set; } // Flattened from Flight.Airline.Name
        public string DepartureAirport { get; set; } // Flattened from Flight.DepartureAirport.Name
        public string ArrivalAirport { get; set; } // Flattened from Flight.ArrivalAirport.Name
        [Required]
        [FutureDate(ErrorMessage = "Departure must be in the future")]
        public DateTime DepartureTime { get; set; }

        [Required]
        [FutureDate(ErrorMessage = "Arrival must be in the future")]
        public DateTime ArrivalTime { get; set; }
        [Range(0.01, 10000)]
        public decimal BasePrice { get; set; }
        public int AvailableSeats { get; set; } // Computed field
        public int Stops { get; set; }
        public string AirlineLogoUrl { get; set; } = string.Empty;
        public string Duration { get; set; }
    }

    // Custom validation attribute
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value is DateTime date && date > DateTime.Now;
        }
    }
}