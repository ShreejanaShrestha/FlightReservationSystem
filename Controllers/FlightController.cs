// Controllers/FlightsController.cs

using Microsoft.AspNetCore.Mvc;
using FlightReservationSystem.Services;
using FlightReservationSystem.DTOs;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FlightReservationSystem.Controllers
{
    
    [Route("")]
    
    public class FlightController : Controller
    {
        private readonly IFlightService _flightService;
        private readonly ILogger<FlightController> _logger;

        public FlightController(IFlightService flightService, ILogger<FlightController> logger)
        {
            _flightService = flightService;
            _logger = logger;
        }

        // MVC Actions (Render Views)

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var viewModel = new FlightSearchViewModel
            {
                Airports = await _flightService.GetAllAirportNamesAsync(),
                DepartureDate = DateTime.Today,
                ReturnDate = null
            };
            return View(viewModel);
        }

        [HttpPost]
        [Route("Search")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Search(FlightSearchViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                viewModel.AvailableFlights = await _flightService.SearchFlightsAsync(
                    viewModel.DepartureAirport,
                    viewModel.ArrivalAirport,
                    viewModel.DepartureDate);

                if (viewModel.ReturnDate.HasValue)
                {
                    var returnFlights = await _flightService.SearchFlightsAsync(
                        viewModel.ArrivalAirport,
                        viewModel.DepartureAirport,
                        viewModel.ReturnDate.Value);
                    viewModel.AvailableFlights.AddRange(returnFlights);
                }
            }
            viewModel.Airports = await _flightService.GetAllAirportNamesAsync();
            return View("Index", viewModel);
        }

        [HttpGet]
        [Route("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var flight = await _flightService.GetFlightByIdAsync(id);
            if (flight == null)
            {
                return NotFound();
            }
            return View(flight);
        }

        // API Actions (Return JSON)

        [HttpGet]
        [Route("GetAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllFlights()
        {
            var flights = await _flightService.GetAllFlightsAsync();
            return Ok(flights);
        }

        [HttpGet]
        [Route("Get/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFlightById(int id)
        {
            var flight = await _flightService.GetFlightByIdAsync(id);
            if (flight == null)
            {
                return NotFound();
            }
            return Ok(flight);
        }

        [HttpPost]
        [Route("Create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateFlight([FromBody] FlightDTO flightDto)
        {
            if (flightDto == null)
            {
                return BadRequest();
            }

            await _flightService.CreateFlightAsync(flightDto);
            return CreatedAtAction(nameof(GetFlightById), new { id = flightDto.FlightId }, flightDto);
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create(FlightDTO flightDto)
        {
            if (ModelState.IsValid)
            {
                await _flightService.CreateFlightAsync(flightDto);
                return RedirectToAction("Index");
            }
            return View(flightDto);
        }
    }
}

/*// Controllers/FlightsController.cs
using Microsoft.AspNetCore.Mvc;
using FlightReservationSystem.Services;
using FlightReservationSystem.DTOs;

namespace FlightReservationSystem.Controllers
{
    [Route("api/[controller]")] // For API routes
    [Route("[controller]")]     // For MVC routes
    [ApiController]             // Enables API-specific behavior (e.g., automatic model validation)
    public class FlightController : Controller
    {
        private readonly IFlightService _flightService;
        private readonly ILogger<FlightController> _logger;

        public FlightController(IFlightService flightService, ILogger<FlightController> logger)
        {
            _flightService = flightService;
            _logger = logger;
        }

        // MVC Actions (Render Views)

        /// <summary>
        /// Displays a list of all flights (MVC view).
        /// </summary>
        [HttpGet]
        [Route("")] // Matches /Flight
        public IActionResult Index()
        {
            var viewModel = new FlightSearchViewModel
            {
                Airports = _flightService.GetAllAirportNames(),
                DepartureDate = DateTime.Today,
                ReturnDate = null
            };
            return View(viewModel);
            // Renders Views/Flight/Index.cshtml
        }

        [HttpPost]
        [Route("Search")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Search(FlightSearchViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Hardcode passenger count to 1 since it's removed from the form
                viewModel.AvailableFlights = _flightService.SearchFlights(
                    viewModel.DepartureAirport,
                    viewModel.ArrivalAirport,
                    viewModel.DepartureDate
                    ); // Passenger count set to 1

                if (viewModel.ReturnDate.HasValue)
                {
                    var returnFlights = _flightService.SearchFlights(
                        viewModel.ArrivalAirport,
                        viewModel.DepartureAirport,
                        viewModel.ReturnDate.Value
                        ); // Passenger count set to 1
                    viewModel.AvailableFlights.AddRange(returnFlights);
                }
            }
            viewModel.Airports = _flightService.GetAllAirportNames();
            return View("Index", viewModel);
        }

        /// <summary>
        /// Displays details of a specific flight (MVC view).
        /// </summary>
        /// <param name="id">The ID of the flight to display.</param>
        [HttpGet]
        [Route("Details/{id}")]
        public IActionResult Details(int id)
        {
            var flight = _flightService.GetFlightById(id);
            if (flight == null)
            {
                return NotFound();
            }
            return View(flight); // Renders Views/Flight/Details.cshtml
        }

        // API Actions (Return JSON)

        /// <summary>
        /// Retrieves a list of all flights (API).
        /// </summary>
        /// <returns>A list of flights.</returns>
        /// <response code="200">Returns the list of flights.</response>
        [HttpGet]
        [Route("GetAll")] // Matches /api/Flight/GetAll
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAllFlights()
        {
            var flights = _flightService.GetAllFlights();
            return Ok(flights);
        }

        /// <summary>
        /// Retrieves a flight by its ID (API).
        /// </summary>
        /// <param name="id">The ID of the flight to retrieve.</param>
        /// <returns>The flight with the specified ID.</returns>
        /// <response code="200">Returns the flight.</response>
        /// <response code="404">Flight not found.</response>
        [HttpGet]
        [Route("Get/{id}")] // Matches /api/Flight/Get/{id}
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetFlightById(int id)
        {
            var flight = _flightService.GetFlightById(id);
            if (flight == null)
            {
                return NotFound();
            }
            return Ok(flight);
        }

        /// <summary>
        /// Creates a new flight (API).
        /// </summary>
        /// <param name="flightDto">The flight to create.</param>
        /// <returns>The created flight.</returns>
        /// <response code="201">Flight created successfully.</response>
        /// <response code="400">Invalid flight data.</response>
        [HttpPost]
        [Route("Create")] // Matches /api/Flight/Create
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreateFlight([FromBody] FlightDTO flightDto)
        {
            if (flightDto == null)
            {
                return BadRequest();
            }

            _flightService.CreateFlight(flightDto);
            return CreatedAtAction(nameof(GetFlightById), new { id = flightDto.FlightId }, flightDto);
        }

        // MVC Action for Creating a Flight (Form Submission)

        /// <summary>
        /// Creates a new flight (MVC form submission).
        /// </summary>
        [HttpPost]
        [Route("Create")] // Matches /Flight/Create (form submission)
        public IActionResult Create(FlightDTO flightDto)
        {
            if (ModelState.IsValid)
            {
                _flightService.CreateFlight(flightDto);
                return RedirectToAction("Index");
            }
            return View(flightDto); // Renders Views/Flight/Create.cshtml
        }
    }
}*/