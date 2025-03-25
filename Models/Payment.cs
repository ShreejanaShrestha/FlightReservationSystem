using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FlightReservationSystem.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        // Foreign Key
        public int BookingId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; }        // "Pending", "Completed", "Failed"

        [MaxLength(100)]
        public string TransactionId { get; set; } // From Stripe/PayPal

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        // Navigation
        public Booking Booking { get; set; }
    }
}
