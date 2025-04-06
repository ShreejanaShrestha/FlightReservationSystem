using AutoMapper;
using FlightReservationSystem.Data;
using FlightReservationSystem.Models;
using FlightReservationSystem.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FlightReservationSystem.Services
{
    public class FlightService : IFlightService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<FlightService> _logger;

        public FlightService(ApplicationDbContext context, IMapper mapper, ILogger<FlightService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /*public List<FlightDTO> GetAllFlights()
        {
            var flights = _context.Flights
                .Include(f => f.Airline)
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Seats)
                .ToList();
            return _mapper.Map<List<FlightDTO>>(flights);
        }*/
        public async Task<List<FlightDTO>> GetAllFlightsAsync()
        {
            try
            {
                var flights = await _context.Flights
                    .Include(f => f.Airline)
                    .Include(f => f.DepartureAirport)
                    .Include(f => f.ArrivalAirport)
                    .Include(f => f.Seats)
                    .AsNoTracking()
                    .ToListAsync();

                return _mapper.Map<List<FlightDTO>>(flights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all flights");
                throw;
            }
        }

        public async Task<FlightDTO> GetFlightByIdAsync(int id)
        {
            var flight = await _context.Flights
                .Include(f => f.Airline)
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Seats)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FlightId == id);
            if (flight == null)
            {
                throw new KeyNotFoundException($"Flight with ID {id} not found");
            }
            return _mapper.Map<FlightDTO>(flight);
        }

        public async Task CreateFlightAsync(FlightDTO flightDto)
        {
            try
            {
                var flight = _mapper.Map<Flight>(flightDto);

                // Verify related entities exist
                var airlineExists = await _context.Airlines.AnyAsync(a => a.AirlineId == flight.AirlineId);
                var departureExists = await _context.Airports.AnyAsync(a => a.AirportId == flight.DepartureAirportId);
                var arrivalExists = await _context.Airports.AnyAsync(a => a.AirportId == flight.ArrivalAirportId);

                if (!airlineExists || !departureExists || !arrivalExists)
                {
                    throw new ArgumentException("Invalid Airline or Airport ID");
                }

                await _context.Flights.AddAsync(flight);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating flight");
                throw;
            }
        }

        public async Task<List<FlightDTO>> SearchFlightsAsync(string departureAirport, string arrivalAirport, DateTime departureDate)
        {
            var flights = await _context.Flights
                .Include(f => f.Airline)
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Seats)
                .Where(f =>
                    f.DepartureAirport.Name == departureAirport &&
                    f.ArrivalAirport.Name == arrivalAirport &&
                    f.DepartureTime.Date == departureDate.Date)
                .AsNoTracking()
                .ToListAsync();

            return flights.Select(f => new FlightDTO
            {
                FlightId = f.FlightId,
                FlightNumber = f.FlightNumber,
                AirlineName = f.Airline.Name,
                DepartureAirport = f.DepartureAirport.Code,
                ArrivalAirport = f.ArrivalAirport.Code,
                DepartureTime = f.DepartureTime,
                ArrivalTime = f.ArrivalTime,
                BasePrice = f.BasePrice,
                Stops = CalculateStops(f), // Implement this method
                Duration = CalculateDuration(f.DepartureTime, f.ArrivalTime)
            }).ToList();
        }

        private int CalculateStops(Flight flight)
        {
            // Implement your logic to calculate stops
            // For now, we'll assume direct flights (0 stops)
            return 0;
        }

        private string CalculateDuration(DateTime departure, DateTime arrival)
        {
            TimeSpan duration = arrival - departure;
            return $"{(int)duration.TotalHours}h {duration.Minutes}m";
        }

        public async Task<List<string>> GetAllAirportNamesAsync()
        {
            return await _context.Airports
                .Select(a => a.Name)
                .Distinct()
                .OrderBy(name => name)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}