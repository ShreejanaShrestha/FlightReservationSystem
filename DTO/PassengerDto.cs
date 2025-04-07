// FlightReservationSystem/DTO/PassengerDto.cs
namespace FlightReservationSystem.DTO
{
    /// <summary>
    /// Data Transfer Object (DTO) representing passenger information.
    /// Used for transferring passenger details between application layers.
    /// </summary>
    public class PassengerDto
    {
        /// <summary>
        /// Unique identifier for the passenger record.
        /// Primary key in the Passengers database table.
        /// </summary>
        public int PassengerId { get; set; }

        /// <summary>
        /// Foreign key reference to the associated booking.
        /// Links this passenger to a specific booking in the Bookings table.
        /// </summary>
        public int BookingId { get; set; }

        /// <summary>
        /// Full legal name of the passenger as it appears on travel documents.
        /// Used for ticket issuance and passenger manifests.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Passport or government-issued identification number.
        /// Required for international flights and security verification.
        /// </summary>
        public string PassportNumber { get; set; }

        /// <summary>
        /// Assigned seat number for the passenger on the flight.
        /// Format typically follows airline conventions (e.g., "12A", "23C").
        /// May be null if seat assignment occurs later in the process.
        /// </summary>
        public string SeatNumber { get; set; }
    }
}