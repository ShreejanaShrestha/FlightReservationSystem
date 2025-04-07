using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightReservationSystem.Models
{
    /// <summary>
    /// Represents a passenger associated with a booking in the flight reservation system.
    /// Stores essential passenger information required for flight boarding.
    /// </summary>
    public class Passenger
    {
        /// <summary>
        /// Gets or sets the unique identifier for the passenger.
        /// </summary>
        [Key]
        public int PassengerId { get; set; }

        /// <summary>
        /// Gets or sets the foreign key reference to the associated booking.
        /// </summary>
        [ForeignKey("Booking")]
        public int BookingId { get; set; }

        /// <summary>
        /// Gets or sets the passenger's full name.
        /// Required field with maximum length of 100 characters.
        /// Should match the name on the passenger's identification documents.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the passenger's passport or identification number.
        /// Maximum length of 20 characters to accommodate various international formats.
        /// </summary>
        [MaxLength(20)]
        public string PassportNumber { get; set; }

        /// <summary>
        /// Gets or sets the assigned seat number for the passenger.
        /// Format typically includes row number followed by seat letter (e.g., "12A").
        /// Maximum length of 10 characters to accommodate various aircraft seating formats.
        /// </summary>
        [MaxLength(10)]
        public string SeatNumber { get; set; }

        /// <summary>
        /// Navigation property for the associated booking.
        /// Represents the many-to-one relationship between Passenger and Booking.
        /// </summary>
        public Booking Booking { get; set; }
    }
}