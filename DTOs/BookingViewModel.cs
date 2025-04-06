namespace FlightReservationSystem.DTOs
{
    public class BookingViewModel
    {
        public FlightDTO Flight { get; set; }
        public List<PassengerViewModel> Passengers { get; set; } = new List<PassengerViewModel>();
        public decimal TotalPrice => (decimal)(Flight?.BasePrice * (Passengers?.Count ?? 1));
    }
}
