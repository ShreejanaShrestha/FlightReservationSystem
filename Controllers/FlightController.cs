using Microsoft.EntityFrameworkCore;
using FlightReservationSystem.Data;
using FlightReservationSystem.DTO;
using FlightReservationSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using FlightReservationSystem.Enums;

[Route("api/[controller]")]
[ApiController]
public class FlightsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FlightsController> _logger;
    private readonly IMemoryCache _cache;

    // Cache key prefix for flight search results
    private const string FlightSearchCacheKey = "FlightSearch_";

    /// <summary>
    /// Initializes a new instance of the FlightsController
    /// </summary>
    /// <param name="context">Database context for flight operations</param>
    /// <param name="logger">Logger for error and information logging</param>
    /// <param name="cache">Memory cache for performance optimization</param>
    public FlightsController(ApplicationDbContext context,
                           ILogger<FlightsController> logger,
                           IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// Retrieves a paginated list of all flights
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10)</param>
    /// <returns>
    /// - 200 OK with list of flights if successful
    /// - 404 Not Found if no flights exist
    /// - 500 Internal Server Error if an exception occurs
    /// </returns>
    /// <remarks>
    /// Sample request:
    /// GET /api/flights?page=1&pageSize=10
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FlightDto>>> GetAllFlights(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        // Generate cache key based on pagination parameters
        var cacheKey = $"AllFlights_{page}_{pageSize}";

        // Check cache before hitting database
        if (_cache.TryGetValue(cacheKey, out var cachedFlights))
        {
            _logger.LogInformation("Returning cached flight data for {CacheKey}", cacheKey);
            return Ok(cachedFlights);
        }

        try
        {
            // Query flights with related entities and project to DTO
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
                _logger.LogWarning("No flights found in database");
                return NotFound(new { message = "No flights found" });
            }

            // Cache results for 10 minutes
            _cache.Set(cacheKey, flights, TimeSpan.FromMinutes(10));
            return Ok(flights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all flights");
            return StatusCode(500, new { error = "An error occurred while retrieving flights" });
        }
    }

    /// <summary>
    /// Searches for flights based on departure, arrival, and optional date
    /// </summary>
    /// <param name="departureAirportId">ID of the departure airport</param>
    /// <param name="arrivalAirportId">ID of the arrival airport</param>
    /// <param name="date">Optional departure date filter</param>
    /// <returns>
    /// - 200 OK with matching flights if found
    /// - 404 Not Found if no matching flights exist
    /// - 500 Internal Server Error if an exception occurs
    /// </returns>
    /// <remarks>
    /// Only returns flights with at least one available seat.
    /// Sample request:
    /// GET /api/flights/search?departureAirportId=1&arrivalAirportId=2&date=2023-12-25
    /// </remarks>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<FlightDto>>> SearchFlights(
       [FromQuery] int departureAirportId,
       [FromQuery] int arrivalAirportId,
       [FromQuery] DateTime? date = null)
    {
        // Generate cache key based on search parameters
        var cacheKey = $"{FlightSearchCacheKey}{departureAirportId}_{arrivalAirportId}_{(date.HasValue ? date.Value.ToString("yyyyMMdd") : "all")}";

        // Check cache before querying database
        if (_cache.TryGetValue(cacheKey, out var cachedFlights))
        {
            _logger.LogInformation("Returning cached search results for {CacheKey}", cacheKey);
            return Ok(cachedFlights);
        }

        try
        {
            // Build base query with required conditions
            var query = _context.Flights
                .Where(f => f.DepartureAirportId == departureAirportId &&
                          f.ArrivalAirportId == arrivalAirportId)
                // Only include flights with available seats
                .Where(f => f.Seats.Any(s => !s.IsBooked));

            // Apply date filter if provided
            if (date.HasValue)
            {
                query = query.Where(f => f.DepartureTime.Date == date.Value.Date);
            }

            // Execute query with related data and projection
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
                _logger.LogInformation("No flights found for search criteria: Departure={Departure}, Arrival={Arrival}, Date={Date}",
                    departureAirportId, arrivalAirportId, date);
                return NotFound(new { message = "No available flights found matching the specified criteria" });
            }

            // Cache results for 5 minutes (shorter TTL than all flights)
            _cache.Set(cacheKey, flights, TimeSpan.FromMinutes(5));
            return Ok(flights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching flights");
            return StatusCode(500, new { error = "An error occurred while searching flights" });
        }
    }

    /// <summary>
    /// Adds a new flight to the system
    /// </summary>
    /// <param name="flightDto">Flight data transfer object</param>
    /// <returns>
    /// - 201 Created with the new flight if successful
    /// - 400 Bad Request if validation fails
    /// - 500 Internal Server Error if an exception occurs
    /// </returns>
    /// <remarks>
    /// Sample request:
    /// POST /api/flights
    /// {
    ///     "flightNumber": "DL123",
    ///     "airlineId": 1,
    ///     "departureAirportId": 1,
    ///     "arrivalAirportId": 2,
    ///     "departureTime": "2023-12-25T08:00:00",
    ///     "arrivalTime": "2023-12-25T11:00:00",
    ///     "basePrice": 299.99
    /// }
    /// </remarks>
    [HttpPost]
    public async Task<ActionResult<FlightDto>> AddFlight([FromBody] AddFlightDto flightDto)
    {
        try
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for AddFlight");
                return BadRequest(ModelState);
            }

            // Business rule validation
            if (flightDto.DepartureTime >= flightDto.ArrivalTime)
            {
                _logger.LogWarning("Invalid flight times - departure not before arrival");
                return BadRequest(new { error = "Departure time must be before arrival time" });
            }

            // Check for duplicate flight number
            if (await _context.Flights.AnyAsync(f => f.FlightNumber == flightDto.FlightNumber))
            {
                _logger.LogWarning("Duplicate flight number: {FlightNumber}", flightDto.FlightNumber);
                return BadRequest(new { error = $"Flight number {flightDto.FlightNumber} already exists" });
            }

            // Validate referenced entities exist
            var airline = await _context.Airlines.FindAsync(flightDto.AirlineId);
            if (airline == null)
            {
                _logger.LogWarning("Airline not found: {AirlineId}", flightDto.AirlineId);
                return BadRequest(new { error = $"Airline with ID {flightDto.AirlineId} not found" });
            }

            var departureAirport = await _context.Airports.FindAsync(flightDto.DepartureAirportId);
            if (departureAirport == null)
            {
                _logger.LogWarning("Departure airport not found: {AirportId}", flightDto.DepartureAirportId);
                return BadRequest(new { error = $"Departure Airport with ID {flightDto.DepartureAirportId} not found" });
            }

            var arrivalAirport = await _context.Airports.FindAsync(flightDto.ArrivalAirportId);
            if (arrivalAirport == null)
            {
                _logger.LogWarning("Arrival airport not found: {AirportId}", flightDto.ArrivalAirportId);
                return BadRequest(new { error = $"Arrival Airport with ID {flightDto.ArrivalAirportId} not found" });
            }

            // Create and save new flight
            var flight = new Flight
            {
                FlightNumber = flightDto.FlightNumber,
                AirlineId = flightDto.AirlineId,
                DepartureAirportId = flightDto.DepartureAirportId,
                ArrivalAirportId = flightDto.ArrivalAirportId,
                DepartureTime = flightDto.DepartureTime,
                ArrivalTime = flightDto.ArrivalTime,
                BasePrice = flightDto.BasePrice,
                Status = FlightStatus.Scheduled
            };

            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();

            // Invalidate relevant cache entries
            _cache.Remove("AllFlights"); // TODO: Consider more granular cache invalidation

            // Prepare response DTO
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
                Seats = new List<SeatDto>() // Empty seat list for new flight
            };

            _logger.LogInformation("Successfully added new flight {FlightNumber} (ID: {FlightId})",
                flight.FlightNumber, flight.FlightId);

            return CreatedAtAction(nameof(AddFlight), new { id = flight.FlightId }, flightResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding flight");
            return StatusCode(500, new { error = "An error occurred while adding the flight" });
        }
    }
}