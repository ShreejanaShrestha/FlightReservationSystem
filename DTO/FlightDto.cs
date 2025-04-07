namespace FlightReservationSystem.DTO
{
    public class FlightDto
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; }
        public int AirlineId { get; set; }
        public string AirlineName { get; set; }
        public int DepartureAirportId { get; set; }
        public string DepartureAirportName { get; set; }
        public int ArrivalAirportId { get; set; }
        public AirportDto DepartureAirport { get; set; }
        public string ArrivalAirportName { get; set; }
        public AirportDto ArrivalAirport { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal BasePrice { get; set; }
        public AirlineDto Airline { get; set; }


        public int AvailableSeats { get; set; }
        public List<SeatDto> Seats { get; set; }
    }

    
}