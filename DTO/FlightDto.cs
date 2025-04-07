namespace FlightReservationSystem.DTO
{
    /// <summary>
    /// Data Transfer Object (DTO) for flight information
    /// </summary>
    /// <remarks>
    /// This DTO represents flight data transferred between layers of the application.
    /// It includes complete flight details along with related entities.
    /// </remarks>
    public class FlightDto
    {
        /// <summary>
        /// Unique identifier for the flight
        /// </summary>
        public int FlightId { get; set; }

        /// <summary>
        /// Flight number/code (e.g., "DL1234")
        /// </summary>
        public string FlightNumber { get; set; }

        /// <summary>
        /// ID of the airline operating the flight
        /// </summary>
        public int AirlineId { get; set; }

        /// <summary>
        /// Name of the airline operating the flight
        /// </summary>
        public string AirlineName { get; set; }

        /// <summary>
        /// Complete airline information DTO
        /// </summary>
        public AirlineDto Airline { get; set; }

        /// <summary>
        /// ID of the departure airport
        /// </summary>
        public int DepartureAirportId { get; set; }

        /// <summary>
        /// Name of the departure airport
        /// </summary>
        public string DepartureAirportName { get; set; }

        /// <summary>
        /// Complete departure airport information DTO
        /// </summary>
        public AirportDto DepartureAirport { get; set; }

        /// <summary>
        /// ID of the arrival airport
        /// </summary>
        public int ArrivalAirportId { get; set; }

        /// <summary>
        /// Name of the arrival airport
        /// </summary>
        public string ArrivalAirportName { get; set; }

        /// <summary>
        /// Complete arrival airport information DTO
        /// </summary>
        public AirportDto ArrivalAirport { get; set; }

        /// <summary>
        /// Scheduled departure date and time
        /// </summary>
        public DateTime DepartureTime { get; set; }

        /// <summary>
        /// Scheduled arrival date and time
        /// </summary>
        public DateTime ArrivalTime { get; set; }

        /// <summary>
        /// Base price for the flight (before taxes/fees)
        /// </summary>
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Number of available seats on the flight
        /// </summary>
        public int AvailableSeats { get; set; }

        /// <summary>
        /// List of all seats on the flight with their details
        /// </summary>
        public List<SeatDto> Seats { get; set; }
    }
}