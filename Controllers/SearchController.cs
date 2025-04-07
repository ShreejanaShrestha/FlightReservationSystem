using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightReservationSystem.Data;
using FlightReservationSystem.DTO;
using System.Net.Http;
using System.Net.Http.Json;

namespace FlightReservationSystem.Controllers
{
    /// <summary>
    /// Controller for handling flight search operations
    /// </summary>
    /// <remarks>
    /// This controller provides functionality for:
    /// - Displaying the search form
    /// - Processing search requests
    /// - Retrieving flight data from the API
    /// - Displaying search results
    /// </remarks>
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the SearchController
        /// </summary>
        /// <param name="context">Database context for airport data</param>
        /// <param name="httpClientFactory">Factory for creating HTTP clients</param>
        public SearchController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            // TODO: Move base address to configuration
            _httpClient.BaseAddress = new Uri("http://localhost:5106/");
        }

        /// <summary>
        /// Displays the flight search form
        /// </summary>
        /// <returns>Search view with list of airports</returns>
        [HttpGet]
        public IActionResult Search()
        {
            // Populate ViewBag with airports for dropdown lists
            ViewBag.Airports = _context.Airports.ToList();
            return View();
        }

        /// <summary>
        /// Processes flight search requests and displays results
        /// </summary>
        /// <param name="departureAirportId">ID of the departure airport</param>
        /// <param name="arrivalAirportId">ID of the arrival airport</param>
        /// <param name="date">Optional departure date filter</param>
        /// <returns>
        /// - Search view with errors if validation fails
        /// - SearchResults view with flights if search succeeds
        /// - Search view with error message if no flights found
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(int departureAirportId, int arrivalAirportId, DateTime? date)
        {
            // Log search parameters for debugging
            Console.WriteLine($"Received: departureAirportId={departureAirportId}, arrivalAirportId={arrivalAirportId}, date={date}");

            // Validate required fields
            if (departureAirportId == 0 || arrivalAirportId == 0)
            {
                Console.WriteLine("Validation failed: Departure or arrival airport not selected.");
                ModelState.AddModelError("", "Please select both departure and arrival airports.");
                ViewBag.Airports = _context.Airports.ToList();
                return View();
            }

            // Validate that departure and arrival are different
            if (departureAirportId == arrivalAirportId)
            {
                Console.WriteLine("Validation failed: Departure and arrival airports are the same.");
                ModelState.AddModelError("", "Departure and arrival airports cannot be the same.");
                ViewBag.Airports = _context.Airports.ToList();
                return View();
            }

            try
            {
                // Build API query string with parameters
                var query = $"api/flights/search?departureAirportId={departureAirportId}&arrivalAirportId={arrivalAirportId}";
                if (date.HasValue)
                {
                    query += $"&date={date.Value:yyyy-MM-dd}";
                }

                Console.WriteLine($"Calling API: {query}");

                // Make API request to flight service
                var response = await _httpClient.GetAsync(query);

                if (response.IsSuccessStatusCode)
                {
                    // Process successful response
                    var flights = await response.Content.ReadFromJsonAsync<List<FlightDto>>();
                    Console.WriteLine($"API returned {flights?.Count ?? 0} flights.");

                    // Handle empty results
                    if (flights == null || !flights.Any())
                    {
                        TempData["Message"] = "No available flights found matching your criteria.";
                    }

                    ViewBag.Airports = _context.Airports.ToList();
                    return View("SearchResults", flights);
                }

                // Handle API error response
                Console.WriteLine($"API call failed: {response.StatusCode}");
                ModelState.AddModelError("", "No flights found.");
                ViewBag.Airports = _context.Airports.ToList();
                return View();
            }
            catch (Exception ex)
            {
                // Log and handle unexpected errors
                Console.WriteLine($"Exception occurred: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while searching for flights. Please try again.");
                ViewBag.Airports = _context.Airports.ToList();
                return View();
            }
        }
    }
}