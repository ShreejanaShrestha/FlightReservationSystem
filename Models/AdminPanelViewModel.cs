// FlightReservationSystem/Models/AdminPanelViewModel.cs

using System.Collections.Generic;

namespace FlightReservationSystem.Models
{
    /// <summary>
    /// View model for the admin panel dashboard that consolidates multiple data entities 
    /// for administrative oversight and management of the flight reservation system.
    /// </summary>
    public class AdminPanelViewModel
    {
        /// <summary>
        /// Gets or sets the collection of all flights in the system.
        /// Used to display and manage flight information in the admin panel.
        /// </summary>
        public List<Flight> Flights { get; set; } = new List<Flight>();

        /// <summary>
        /// Gets or sets the collection of all airports in the system.
        /// Used to display and manage airport information in the admin panel.
        /// </summary>
        public List<Airport> Airports { get; set; } = new List<Airport>();

        /// <summary>
        /// Gets or sets the collection of all airlines in the system.
        /// Used to display and manage airline information in the admin panel.
        /// </summary>
        public List<Airline> Airlines { get; set; } = new List<Airline>();

        /// <summary>
        /// Gets or sets the collection of all bookings in the system.
        /// Used to display and manage customer reservations in the admin panel.
        /// </summary>
        public List<Booking> Bookings { get; set; } = new List<Booking>();

        /// <summary>
        /// Gets or sets the collection of all users registered in the system.
        /// Used to display and manage user accounts in the admin panel.
        /// </summary>
        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}