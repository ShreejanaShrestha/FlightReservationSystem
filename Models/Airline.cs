using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FlightReservationSystem.Models;

namespace FlightReservationSystem.Models
{
    /// <summary>
    /// Represents an airline company in the flight reservation system.
    /// Contains core information about the airline including identification codes and branding.
    /// </summary>
    public class Airline
    {
        /// <summary>
        /// Gets or sets the unique identifier for the airline.
        /// </summary>
        [Key]
        public int AirlineId { get; set; }

        /// <summary>
        /// Gets or sets the full official name of the airline.
        /// For example, "Delta Air Lines".
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the International Air Transport Association (IATA) code.
        /// Two-letter code uniquely identifying the airline (e.g., "DL" for Delta).
        /// </summary>
        [Required]
        [MaxLength(2)]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the relative or absolute URL to the airline's logo image.
        /// May be null if no logo is available.
        /// </summary>
        [MaxLength(200)]
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Navigation property for all flights operated by this airline.
        /// Represents the one-to-many relationship between Airline and Flight.
        /// </summary>
        public ICollection<Flight> Flights { get; set; } = new List<Flight>();
    }
}