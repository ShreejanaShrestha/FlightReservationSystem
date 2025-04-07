using System.ComponentModel.DataAnnotations;
namespace FlightReservationSystem.Models
{
    /// <summary>
    /// Represents an individual seat on an aircraft for a specific flight.
    /// Contains information about seat location, class type, and availability status.
    /// </summary>
    public class Seat
    {
        /// <summary>
        /// Unique identifier for the seat.
        /// Primary key in the Seats database table.
        /// </summary>
        public int SeatId { get; set; }

        /// <summary>
        /// Foreign key reference to the associated flight.
        /// Links this seat to a specific flight in the Flights table.
        /// </summary>
        public int FlightId { get; set; }

        /// <summary>
        /// The seat's location identifier on the aircraft.
        /// Typically follows the airline's seating convention (e.g., "12A", "23C", "1K").
        /// Limited to 10 characters and required.
        /// </summary>
        [Required, MaxLength(10)]
        public string SeatNumber { get; set; }

        /// <summary>
        /// The service class category of the seat.
        /// Common values include "Economy", "Premium Economy", "Business", or "First".
        /// Limited to 20 characters and required.
        /// </summary>
        [Required, MaxLength(20)]
        public string Class { get; set; }

        /// <summary>
        /// Indicates whether the seat is currently booked/reserved.
        /// False indicates the seat is available for booking.
        /// Default value is false (available).
        /// </summary>
        public bool IsBooked { get; set; } = false;

        /// <summary>
        /// Navigation property to the flight this seat belongs to.
        /// Provides access to the full flight information when needed.
        /// </summary>
        public Flight Flight { get; set; }
    }
}