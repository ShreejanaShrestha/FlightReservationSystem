using System.ComponentModel.DataAnnotations;

namespace FlightReservationSystem.DTO
{
    /// <summary>
    /// Data Transfer Object (DTO) for airport information.
    /// Used for transferring airport data between layers while enforcing validation rules.
    /// </summary>
    public class AirportDto
    {
        /// <summary>
        /// Unique identifier for the airport.
        /// </summary>
        public int AirportId { get; set; }

        /// <summary>
        /// Full name of the airport.
        /// Required field with maximum length of 100 characters.
        /// Example: "John F. Kennedy International Airport"
        /// </summary>
        [Required(ErrorMessage = "Airport name is required")]
        [MaxLength(100, ErrorMessage = "Airport name cannot exceed 100 characters")]
        public string Name { get; set; }

        /// <summary>
        /// IATA airport code (3-letter code).
        /// Required field with exact length of 3 characters.
        /// Example: "JFK"
        /// </summary>
        [Required(ErrorMessage = "Airport code is required")]
        [MaxLength(3, ErrorMessage = "Airport code must be exactly 3 characters")]
        [MinLength(3, ErrorMessage = "Airport code must be exactly 3 characters")]
        public string Code { get; set; }

        /// <summary>
        /// City where the airport is located.
        /// Required field with maximum length of 50 characters.
        /// Example: "New York"
        /// </summary>
        [Required(ErrorMessage = "City is required")]
        [MaxLength(50, ErrorMessage = "City name cannot exceed 50 characters")]
        public string City { get; set; }

        /// <summary>
        /// Country where the airport is located.
        /// Required field with maximum length of 50 characters.
        /// Example: "United States"
        /// </summary>
        [Required(ErrorMessage = "Country is required")]
        [MaxLength(50, ErrorMessage = "Country name cannot exceed 50 characters")]
        public string Country { get; set; }
    }
}