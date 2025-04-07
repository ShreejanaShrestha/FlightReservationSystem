// FlightReservationSystem/DTO/BookingViewModels.cs
using System.ComponentModel.DataAnnotations;

namespace FlightReservationSystem.DTO
{
   

    public class ConfirmBookingViewModel
    {
        public string FlightNumber { get; set; }
        public string Airline { get; set; }
        public int FlightId { get; set; }
        public string DepartureAirport { get; set; }
        public string ArrivalAirport { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal BasePrice { get; set; }
        public decimal TotalPrice { get; set; }
        public List<PassengerDto> Passengers { get; set; }
    }
    public class AddPassengerDetailsViewModel
    {
        public int FlightId { get; set; }
        public decimal BasePrice { get; set; }
        public List<PassengerDto> Passengers { get; set; } = new List<PassengerDto> { new PassengerDto() };

       
       
    }
}