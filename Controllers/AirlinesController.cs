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
    public class AirlinesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AirlinesController> _logger;

        public AirlinesController(ApplicationDbContext context, ILogger<AirlinesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/airlines
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AirlineDto>>> GetAirlines()
        {
            try { 
            var airlines = await _context.Airlines.Select(a=> new AirlineDto {
            AirlineId = a.AirlineId,
            Name=a.Name,
            Code = a.Code,
            LogoUrl=a.LogoUrl}).ToListAsync();
                if (!airlines.Any())
                {
                    return NotFound(new { message = "No airlines found" });
                }
                return Ok(airlines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving airlines");
                return StatusCode(500, new { error = "An error occurred while retrieving airlines", details = ex.Message });
            }
        }
        // POST: api/airlines
        [HttpPost]
        public async Task<ActionResult<Airline>> AddAirline([FromBody] Airline airline)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                airline.Flights = new List<Flight>(); // Ensure navigation property is initialized
                _context.Airlines.Add(airline);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(AddAirline), new { id = airline.AirlineId }, airline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding airline");
                return StatusCode(500, new { error = "An error occurred while adding the airline", details = ex.Message });
            }
        }
    }
}