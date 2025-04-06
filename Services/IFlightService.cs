using System.Collections.Generic;
using System.Threading.Tasks;
using FlightReservationSystem.DTOs;

namespace FlightReservationSystem.Services
{
    public interface IFlightService
    {
        Task<List<FlightDTO>> GetAllFlightsAsync();
        Task<FlightDTO> GetFlightByIdAsync(int id);
        Task CreateFlightAsync(FlightDTO flightDto);
        Task<List<FlightDTO>> SearchFlightsAsync(string departureAirport, string arrivalAirport, DateTime departureDate);
        Task<List<string>> GetAllAirportNamesAsync();
    }
}

/*public interface IFlightService
{
    Task<List<Flight>> GetAllFlightsAsync();
    Task<Flight> GetFlightByIdAsync(int id);
    Task<Flight> CreateFlightAsync(FlightDTO flightDTO); // Change parameter to DTO
    Task UpdateFlightAsync(int id, FlightDTO flightDTO); // Add id and DTO
    Task DeleteFlightAsync(int id);
}*/

// Services/FlightService.cs
/*public class FlightService : IFlightService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public FlightService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<Flight>> GetAllFlightsAsync()
    {
        return await _context.Flights
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .Include(f => f.Airline)
            .ToListAsync();
    }

    public async Task<Flight> GetFlightByIdAsync(int id)
    {
        return await _context.Flights
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .FirstOrDefaultAsync(f => f.FlightId == id);
    }

    public async Task<Flight> CreateFlightAsync(FlightDTO flightDTO)
    {
        var flight = _mapper.Map<Flight>(flightDTO);
        _context.Flights.Add(flight);
        await _context.SaveChangesAsync();
        return flight;
    }

    public async Task UpdateFlightAsync(int id, FlightDTO flightDTO)
    {
        var flight = await _context.Flights.FindAsync(id);
        _mapper.Map(flightDTO, flight);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteFlightAsync(int id)
    {
        var flight = await _context.Flights.FindAsync(id);
        _context.Flights.Remove(flight);
        await _context.SaveChangesAsync();
    }
}*/