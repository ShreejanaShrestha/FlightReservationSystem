// FlightReservationSystem/Models/AdminPanelViewModel.cs
using System.Collections.Generic;

namespace FlightReservationSystem.Models
{
    public class AdminPanelViewModel
    {
        public List<Flight> Flights { get; set; }
        public List<Airport> Airports { get; set; }
        public List<Airline> Airlines { get; set; }
        public List<Booking> Bookings { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }
}