namespace FlightReservationSystem.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        // Foreign Keys
        public string UserId { get; set; }        // Links to AspNetUsers
        public int FlightId { get; set; }

        // Booking Metadata
        public DateTime BookingDate { get; set; } = DateTime.Now;
        public string Status { get; set; }        // "Confirmed", "Cancelled"

        // Navigation
        public ApplicationUser User { get; set; }
        public Flight Flight { get; set; }
        public ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();
        public Payment Payment { get; set; }
    }
}
