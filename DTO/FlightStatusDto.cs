// FlightReservationSystem/DTO/FlightStatusDto.cs
namespace FlightReservationSystem.DTO
{
    /// <summary>
    /// Data Transfer Object (DTO) representing the complete status of a flight booking
    /// </summary>
    /// <remarks>
    /// This DTO combines flight information with passenger details to provide
    /// a comprehensive view of a booking's status. Used primarily for:
    /// - Flight status check responses
    /// - Booking confirmation details
    /// - Passenger manifest information
    /// </remarks>
    public class FlightStatusDto
    {
        /// <summary>
        /// Passenger Name Record (PNR) - Unique booking reference code
        /// </summary>
        /// <example>ABC123</example>
        public string PNR { get; set; }

        /// <summary>
        /// Flight number including airline code
        /// </summary>
        /// <example>DL1234</example>
        public string FlightNumber { get; set; }

        /// <summary>
        /// Name of the operating airline
        /// </summary>
        /// <example>Delta Airlines</example>
        public string Airline { get; set; }

        /// <summary>
        /// Departure airport information (Name and Code)
        /// </summary>
        /// <example>John F. Kennedy International Airport (JFK)</example>
        public string DepartureAirport { get; set; }

        /// <summary>
        /// Arrival airport information (Name and Code)
        /// </summary>
        /// <example>Los Angeles International Airport (LAX)</example>
        public string ArrivalAirport { get; set; }

        /// <summary>
        /// Scheduled departure date and time in local time
        /// </summary>
        /// <example>2023-12-25T08:00:00</example>
        public DateTime DepartureTime { get; set; }

        /// <summary>
        /// Scheduled arrival date and time in local time
        /// </summary>
        /// <example>2023-12-25T11:00:00</example>
        public DateTime ArrivalTime { get; set; }

        /// <summary>
        /// Current status of the flight
        /// </summary>
        /// <example>OnTime | Delayed | Cancelled | Boarding</example>
        public string Status { get; set; }

        /// <summary>
        /// List of passengers included in this booking
        /// </summary>
        public List<PassengerDto> Passengers { get; set; } = new List<PassengerDto>();
    }
}