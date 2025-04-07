// FlightReservationSystem/DTO/BookingDto.cs
namespace FlightReservationSystem.DTO
{
    /// <summary>
    /// Data Transfer Object (DTO) representing a flight booking.
    /// Contains all relevant booking information including flight details and passenger information.
    /// </summary>
    public class BookingDto
    {
        /// <summary>
        /// Unique identifier for the booking.
        /// Primary key in the Bookings database table.
        /// </summary>
        public int BookingId { get; set; }

        /// <summary>
        /// Identifier for the user who created the booking.
        /// Foreign key reference to the user account in the identity system.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Unique identifier for the booked flight.
        /// Foreign key reference to the Flights table.
        /// </summary>
        public int FlightId { get; set; }

        /// <summary>
        /// Detailed information about the booked flight.
        /// Navigation property providing access to the full flight information.
        /// </summary>
        public FlightDto Flight { get; set; }

        /// <summary>
        /// Timestamp when the booking was created.
        /// Stored in UTC format.
        /// </summary>
        public DateTime BookingDate { get; set; }

        /// <summary>
        /// Current status of the booking (e.g., "Confirmed", "Cancelled", "Pending").
        /// Reflects the booking's current state in the reservation process.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Collection of passengers included in this booking.
        /// Contains personal and travel document information for each passenger.
        /// </summary>
        public List<PassengerDto> Passengers { get; set; }
    }
}