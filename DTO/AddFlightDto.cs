/// <summary>
/// Data Transfer Object (DTO) for adding a new flight to the system.
/// </summary>
public class AddFlightDto
{
    /// <summary>
    /// Unique identifier for the flight (e.g., "AA123", "DL456").
    /// This is typically an airline code followed by a number.
    /// </summary>
    public string FlightNumber { get; set; }

    /// <summary>
    /// Foreign key reference to the airline operating this flight.
    /// Maps to the primary key in the Airlines table.
    /// </summary>
    public int AirlineId { get; set; }

    /// <summary>
    /// Foreign key reference to the departure airport.
    /// Maps to the primary key in the Airports table.
    /// </summary>
    public int DepartureAirportId { get; set; }

    /// <summary>
    /// Foreign key reference to the arrival airport.
    /// Maps to the primary key in the Airports table.
    /// </summary>
    public int ArrivalAirportId { get; set; }

    /// <summary>
    /// Scheduled departure time of the flight.
    /// Should be stored and transmitted in UTC format.
    /// </summary>
    public DateTime DepartureTime { get; set; }

    /// <summary>
    /// Scheduled arrival time of the flight.
    /// Should be stored and transmitted in UTC format.
    /// </summary>
    public DateTime ArrivalTime { get; set; }

    /// <summary>
    /// Base ticket price for the flight in the system's default currency.
    /// Does not include taxes, fees, or any additional charges.
    /// </summary>
    public decimal BasePrice { get; set; }
}