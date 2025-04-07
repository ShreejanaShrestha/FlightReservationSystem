// FlightReservationSystem/Enums/FlightStatus.cs
namespace FlightReservationSystem.Enums
{

    // <summary>
    /// Represents the current operational status of a flight.
    /// Used throughout the system to track and display flight status information.
    /// </summary>
    public enum FlightStatus
    {
        Scheduled,
        Boarding,
        Departed,
        Arrived,
        Delayed,
        Cancelled
    }
}