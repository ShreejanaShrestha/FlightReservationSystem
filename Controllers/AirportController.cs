using Microsoft.AspNetCore.Mvc;
using FlightReservationSystem.Models;
using FlightReservationSystem.Data;
using System;
using System.Threading.Tasks;
using FlightReservationSystem.DTO;
using Microsoft.EntityFrameworkCore;

namespace FlightReservationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AirportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AirportsController> _logger;

        public AirportsController(ApplicationDbContext context, ILogger<AirportsController> logger)
        {
            _context = context;
            _logger = logger;
        }


        // GET: api/airports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AirlineDto>>> GetAirports()
        {
            try
            {
                var airports = await _context.Airports.Select(a => new AirportDto {
                    AirportId = a.AirportId,
                    Name = a.Name,
                    City = a.City,
                    Code = a.Code,
                    Country = a.Country
                }).ToListAsync();
                if (!airports.Any()){
                    return NotFound(new { message = "No airlines found" });
                };
                return Ok(airports);


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving airlines");
                return StatusCode(500, new { error = "An error occurred while retrieving airports", details = ex.Message });
            }

        }

        // POST: api/airports
        [HttpPost]
        public async Task<ActionResult<Airport>> AddAirport([FromBody] Airport airport)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                airport.DepartureFlights = new List<Flight>();
                airport.ArrivalFlights = new List<Flight>();

                _context.Airports.Add(airport);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(AddAirport), new { id = airport.AirportId }, airport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding airport");
                return StatusCode(500, new { error = "An error occurred while adding the airport", details = ex.Message });
            }
        }
    }
}