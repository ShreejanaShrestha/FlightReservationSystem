namespace FlightReservationSystem.DTO
{
    /// <summary>
    /// Data Transfer Object (DTO) representing airline information.
    /// Used for transferring airline data between application layers.
    /// </summary>
    public class AirlineDto
    {
        /// <summary>
        /// Unique identifier for the airline.
        /// Primary key in the Airlines database table.
        /// </summary>
        public int AirlineId { get; set; }

        /// <summary>
        /// Full official name of the airline (e.g., "American Airlines", "Delta Air Lines").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// IATA or ICAO airline code (e.g., "AA" for American Airlines, "DL" for Delta).
        /// Typically 2 characters for IATA or 3 characters for ICAO.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Optional URL to the airline's logo image.
        /// May be null if no logo is available.
        /// </summary>
        public string? LogoUrl { get; set; }
    }
}