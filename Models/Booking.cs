using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightReservationSystem.Models
{
    /// <summary>
    /// Represents a flight reservation made by a user in the system.
    /// Contains information about the flight, associated passengers, payment status,
    /// and booking identification details.
    /// </summary>
    public class Booking
    {
        /// <summary>
        /// Gets or sets the unique identifier for the booking.
        /// </summary>
        [Key]
        public int BookingId { get; set; }

        /// <summary>
        /// Gets or sets the foreign key reference to the user who made the booking.
        /// Corresponds to the Id field in the AspNetUsers identity table.
        /// </summary>
        [ForeignKey("User")]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the foreign key reference to the associated flight.
        /// </summary>
        [ForeignKey("Flight")]
        public int FlightId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the booking was created.
        /// Defaults to the current system time when a new booking is created.
        /// </summary>
        public DateTime BookingDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the current status of the booking.
        /// Valid values include: "Confirmed", "Cancelled".
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the Passenger Name Record (PNR).
        /// A unique alphanumeric identifier that serves as a booking reference code.
        /// </summary>
        [MaxLength(6)]
        public string PNR { get; set; }

        /// <summary>
        /// Navigation property for the associated user.
        /// Represents the many-to-one relationship between Booking and ApplicationUser.
        /// </summary>
        public ApplicationUser User { get; set; }

        /// <summary>
        /// Navigation property for the associated flight.
        /// Represents the many-to-one relationship between Booking and Flight.
        /// </summary>
        public Flight Flight { get; set; }

        /// <summary>
        /// Navigation property for all passengers included in this booking.
        /// Represents the one-to-many relationship between Booking and Passenger.
        /// </summary>
        public ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();

        /// <summary>
        /// Navigation property for the associated payment.
        /// Represents the one-to-one relationship between Booking and Payment.
        /// </summary>
        public Payment Payment { get; set; }
    }
}