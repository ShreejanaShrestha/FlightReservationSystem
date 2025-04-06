using Microsoft.EntityFrameworkCore;
using FlightReservationSystem.Data;
using FlightReservationSystem.DTO;
using FlightReservationSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

[Route("api/[controller]")]
[ApiController]
public class FlightsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FlightsController> _logger;
    private readonly IMemoryCache _cache;

    private const string FlightSearchCacheKey = "FlightSearch_";

    public FlightsController(ApplicationDbContext context, ILogger<FlightsController> logger, IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    // GET: api/flights?page=1&pageSize=10
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FlightDto>>> GetAllFlights([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var cacheKey = $"AllFlights_{page}_{pageSize}";
        if (_cache.TryGetValue(cacheKey, out var cachedFlights))
        {
            return Ok(cachedFlights);
        }

        try
        {
            var flights = await _context.Flights
                .Include(f => f.Airline)
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Seats)
                .Select(f => new FlightDto
                {
                    FlightId = f.FlightId,
                    FlightNumber = f.FlightNumber,
                    AirlineId = f.AirlineId,
                    AirlineName = f.Airline.Name,
                    DepartureAirportId = f.DepartureAirportId,
                    DepartureAirportName = f.DepartureAirport.Name,
                    ArrivalAirportId = f.ArrivalAirportId,
                    ArrivalAirportName = f.ArrivalAirport.Name,
                    DepartureTime = f.DepartureTime,
                    ArrivalTime = f.ArrivalTime,
                    BasePrice = f.BasePrice,
                    Seats = f.Seats.Select(s => new SeatDto
                    {
                        SeatId = s.SeatId,
                        FlightId = s.FlightId,
                        SeatNumber = s.SeatNumber,
                        Class = s.Class,
                        IsBooked = s.IsBooked
                    }).ToList()
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!flights.Any())
            {
                return NotFound(new { message = "No flights found" });
            }

            _cache.Set(cacheKey, flights, TimeSpan.FromMinutes(10));
            return Ok(flights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all flights");
            return StatusCode(500, new { error = "An error occurred while retrieving flights" });
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<FlightDto>>> SearchFlights(
       [FromQuery] int departureAirportId,
       [FromQuery] int arrivalAirportId,
       [FromQuery] DateTime? date = null)
    {
        var cacheKey = $"{FlightSearchCacheKey}{departureAirportId}_{arrivalAirportId}_{(date.HasValue ? date.Value.ToString("yyyyMMdd") : "all")}";
        if (_cache.TryGetValue(cacheKey, out var cachedFlights))
        {
            return Ok(cachedFlights);
        }

        try
        {
            var query = _context.Flights
                .Where(f => f.DepartureAirportId == departureAirportId && f.ArrivalAirportId == arrivalAirportId)
                // Only include flights that have at least one available seat
                .Where(f => f.Seats.Any(s => !s.IsBooked));

            if (date.HasValue)
            {
                query = query.Where(f => f.DepartureTime.Date == date.Value.Date);
            }

            var flights = await query
                .Include(f => f.Airline)
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Seats)
                .Select(f => new FlightDto
                {
                    FlightId = f.FlightId,
                    FlightNumber = f.FlightNumber,
                    AirlineId = f.AirlineId,
                    AirlineName = f.Airline.Name,
                    DepartureAirportId = f.DepartureAirportId,
                    DepartureAirportName = f.DepartureAirport.Name,
                    ArrivalAirportId = f.ArrivalAirportId,
                    ArrivalAirportName = f.ArrivalAirport.Name,
                    DepartureTime = f.DepartureTime,
                    ArrivalTime = f.ArrivalTime,
                    BasePrice = f.BasePrice,
                    Seats = f.Seats.Select(s => new SeatDto
                    {
                        SeatId = s.SeatId,
                        FlightId = s.FlightId,
                        SeatNumber = s.SeatNumber,
                        Class = s.Class,
                        IsBooked = s.IsBooked
                    }).ToList()
                })
                .ToListAsync();

            if (!flights.Any())
            {
                return NotFound(new { message = "No available flights found matching the specified criteria" });
            }

            _cache.Set(cacheKey, flights, TimeSpan.FromMinutes(5));
            return Ok(flights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching flights");
            return StatusCode(500, new { error = "An error occurred while searching flights" });
        }
    }

    // POST: api/flights
    [HttpPost]
    public async Task<ActionResult<FlightDto>> AddFlight([FromBody] AddFlightDto flightDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Additional validation
            if (flightDto.DepartureTime >= flightDto.ArrivalTime)
            {
                return BadRequest(new { error = "Departure time must be before arrival time" });
            }

            if (await _context.Flights.AnyAsync(f => f.FlightNumber == flightDto.FlightNumber))
            {
                return BadRequest(new { error = $"Flight number {flightDto.FlightNumber} already exists" });
            }

            var airline = await _context.Airlines.FindAsync(flightDto.AirlineId);
            if (airline == null) return BadRequest(new { error = $"Airline with ID {flightDto.AirlineId} not found" });

            var departureAirport = await _context.Airports.FindAsync(flightDto.DepartureAirportId);
            if (departureAirport == null) return BadRequest(new { error = $"Departure Airport with ID {flightDto.DepartureAirportId} not found" });

            var arrivalAirport = await _context.Airports.FindAsync(flightDto.ArrivalAirportId);
            if (arrivalAirport == null) return BadRequest(new { error = $"Arrival Airport with ID {flightDto.ArrivalAirportId} not found" });

            var flight = new Flight
            {
                FlightNumber = flightDto.FlightNumber,
                AirlineId = flightDto.AirlineId,
                DepartureAirportId = flightDto.DepartureAirportId,
                ArrivalAirportId = flightDto.ArrivalAirportId,
                DepartureTime = flightDto.DepartureTime,
                ArrivalTime = flightDto.ArrivalTime,
                BasePrice = flightDto.BasePrice
            };

            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();

            // Invalidate cache
            _cache.Remove("AllFlights"); // Simple invalidation; refine as needed

            var flightResponse = new FlightDto
            {
                FlightId = flight.FlightId,
                FlightNumber = flight.FlightNumber,
                AirlineId = flight.AirlineId,
                AirlineName = airline.Name,
                DepartureAirportId = flight.DepartureAirportId,
                DepartureAirportName = departureAirport.Name,
                ArrivalAirportId = flight.ArrivalAirportId,
                ArrivalAirportName = arrivalAirport.Name,
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                BasePrice = flight.BasePrice,
                Seats = new List<SeatDto>()
            };

            return CreatedAtAction(nameof(AddFlight), new { id = flight.FlightId }, flightResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding flight");
            return StatusCode(500, new { error = "An error occurred while adding the flight" });
        }
    }
}