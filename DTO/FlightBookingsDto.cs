namespace FlightReservationSystem.DTO
{
    /// <summary>
    /// Data Transfer Object (DTO) representing a complete flight booking package.
    /// Aggregates flight details with associated bookings and passenger information.
    /// Used for operations requiring consolidated flight booking data.
    /// </summary>
    /// <remarks>
    /// This composite DTO is particularly useful for:
    /// - Flight booking confirmation responses
    /// - Admin viewing complete booking details
    /// - Reporting on flight occupancy
    /// </remarks>
    public class FlightBookingsDto
    {
        /// <summary>
        /// Detailed information about the flight.
        /// Includes route, schedule, aircraft, and pricing details.
        /// </summary>
        public FlightDto Flight { get; set; }

        /// <summary>
        /// Collection of all bookings associated with this flight.
        /// Each booking represents a transaction for one or more passengers.
        /// </summary>
        public List<BookingDto> Bookings { get; set; }

        /// <summary>
        /// Complete list of passengers booked on this flight.
        /// Includes passenger details and their specific seat assignments.
        /// </summary>
        public List<PassengerDto> Passengers { get; set; }
    }
}