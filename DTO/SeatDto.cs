using System.ComponentModel.DataAnnotations;

namespace FlightReservationSystem.DTO
{
    /// <summary>
    /// Data Transfer Object (DTO) representing a flight seat with booking status.
    /// Used for seat selection, booking management, and flight capacity operations.
    /// </summary>
    /// <remarks>
    /// This DTO is critical for:
    /// - Seat selection during booking flows
    /// - Flight capacity monitoring
    /// - Cabin management operations
    /// </remarks>
    public class SeatDto
    {
        /// <summary>
        /// Unique identifier for the seat record.
        /// </summary>
        public int SeatId { get; set; }

        /// <summary>
        /// Reference to the flight this seat belongs to.
        /// Required for all seat operations.
        /// </summary>
        public int FlightId { get; set; }

        /// <summary>
        /// Detailed flight information (optional, populated in expanded responses).
        /// Contains complete flight details when seat data is requested with flight expansion.
        /// </summary>
        public FlightDto Flight { get; set; }

        /// <summary>
        /// Physical seat designation (e.g., "12A", "24C").
        /// Format: [RowNumber][SeatLetter].
        /// Required field with maximum length of 10 characters.
        /// </summary>
        [Required(ErrorMessage = "Seat number is required")]
        [MaxLength(10, ErrorMessage = "Seat number cannot exceed 10 characters")]
        [RegularExpression(@"^[0-9]{1,4}[A-Za-z]{1}$",
            ErrorMessage = "Seat number must be in format [Row][Letter] (e.g., 12A)")]
        public string SeatNumber { get; set; }

        /// <summary>
        /// Cabin class category (e.g., "Economy", "Business", "First").
        /// Required field with maximum length of 20 characters.
        /// </summary>
        [Required(ErrorMessage = "Seat class is required")]
        [MaxLength(20, ErrorMessage = "Seat class cannot exceed 20 characters")]
        public string Class { get; set; }

        /// <summary>
        /// Indicates if the seat is currently booked.
        /// True = Booked, False = Available.
        /// Defaults to false (available) for new seats.
        /// </summary>
        public bool IsBooked { get; set; } = false;

        /// <summary>
        /// Expiration timestamp for temporary seat holds.
        /// Null indicates no active reservation hold.
        /// Used for seat reservation timeouts during booking process.
        /// </summary>
        public DateTime? ReservedUntil { get; set; }
    }
}