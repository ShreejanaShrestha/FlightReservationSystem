using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightReservationSystem.Models
{
    /// <summary>
    /// Represents a payment transaction associated with a booking in the flight reservation system.
    /// Stores payment details including amount, status, and transaction information.
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// Gets or sets the unique identifier for the payment.
        /// </summary>
        [Key]
        public int PaymentId { get; set; }

        /// <summary>
        /// Gets or sets the foreign key reference to the associated booking.
        /// </summary>
        [ForeignKey("Booking")]
        public int BookingId { get; set; }

        /// <summary>
        /// Gets or sets the payment amount.
        /// Stored as a decimal with 10 digits total and 2 decimal places.
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the current status of the payment.
        /// Valid values include: "Pending", "Completed", "Failed".
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the external transaction identifier provided by the payment processor.
        /// Typically received from payment providers like Stripe or PayPal.
        /// </summary>
        [MaxLength(100)]
        public string TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the payment was processed.
        /// Defaults to the current system time when a new payment is created.
        /// </summary>
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Navigation property for the associated booking.
        /// Represents the one-to-one relationship between Payment and Booking.
        /// </summary>
        public Booking Booking { get; set; }
    }
}