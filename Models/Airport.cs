using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FlightReservationSystem.Models
{
    /// <summary>
    /// Represents an airport in the flight reservation system.
    /// Contains essential information about the airport location and identifiers.
    /// </summary>
    public class Airport
    {
        /// <summary>
        /// Gets or sets the unique identifier for the airport.
        /// </summary>
        [Key]
        public int AirportId { get; set; }

        /// <summary>
        /// Gets or sets the full official name of the airport.
        /// For example, "John F. Kennedy International Airport".
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the International Air Transport Association (IATA) code.
        /// Three-letter code uniquely identifying the airport (e.g., "JFK").
        /// </summary>
        [Required]
        [MaxLength(3)]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the city where the airport is located.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the country where the airport is located.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Country { get; set; }

        /// <summary>
        /// Navigation property for all flights departing from this airport.
        /// Represents the one-to-many relationship between Airport and Flight
        /// where this airport is the departure point.
        /// </summary>
        public ICollection<Flight> DepartureFlights { get; set; } = new List<Flight>();

        /// <summary>
        /// Navigation property for all flights arriving at this airport.
        /// Represents the one-to-many relationship between Airport and Flight
        /// where this airport is the arrival point.
        /// </summary>
        public ICollection<Flight> ArrivalFlights { get; set; } = new List<Flight>();
    }
}