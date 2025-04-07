// FlightReservationSystem/DTO/BookingViewModels.cs
using System.ComponentModel.DataAnnotations;
namespace FlightReservationSystem.DTO
{
    /// <summary>
    /// View model used for the booking confirmation page.
    /// Contains all information needed to display flight and passenger details before final confirmation.
    /// </summary>
    public class ConfirmBookingViewModel
    {
        /// <summary>
        /// The unique flight identifier (e.g., "AA123", "DL456").
        /// </summary>
        public string FlightNumber { get; set; }

        /// <summary>
        /// Name of the airline operating the flight.
        /// </summary>
        public string Airline { get; set; }

        /// <summary>
        /// Database identifier for the flight.
        /// Used for creating the booking record.
        /// </summary>
        public int FlightId { get; set; }

        /// <summary>
        /// Name or code of the departure airport.
        /// </summary>
        public string DepartureAirport { get; set; }

        /// <summary>
        /// Name or code of the arrival airport.
        /// </summary>
        public string ArrivalAirport { get; set; }

        /// <summary>
        /// Scheduled departure time of the flight.
        /// Displayed to the user in their local timezone.
        /// </summary>
        public DateTime DepartureTime { get; set; }

        /// <summary>
        /// Scheduled arrival time of the flight.
        /// Displayed to the user in their local timezone.
        /// </summary>
        public DateTime ArrivalTime { get; set; }

        /// <summary>
        /// Base price for a single ticket.
        /// Does not include taxes or additional fees.
        /// </summary>
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Total price for all passengers including taxes and fees.
        /// The final amount to be charged to the customer.
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Collection of passenger information for this booking.
        /// </summary>
        public List<PassengerDto> Passengers { get; set; }
    }

    /// <summary>
    /// View model for the passenger details entry form.
    /// Used to collect passenger information during the booking process.
    /// </summary>
    public class AddPassengerDetailsViewModel
    {
        /// <summary>
        /// Database identifier of the flight being booked.
        /// </summary>
        public int FlightId { get; set; }

        /// <summary>
        /// Base price per passenger.
        /// Used for displaying pricing information and calculating totals.
        /// </summary>
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Collection of passenger information forms.
        /// Initialized with a single empty passenger by default.
        /// </summary>
        public List<PassengerDto> Passengers { get; set; } = new List<PassengerDto> { new PassengerDto() };
    }
}