using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightReservationSystem.Data;
using FlightReservationSystem.DTO;
using System.Net.Http;
using System.Net.Http.Json;

namespace FlightReservationSystem.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public SearchController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5106/");
        }

        // GET: /Search
        public IActionResult Search()
        {
            ViewBag.Airports = _context.Airports.ToList();
            return View();
        }

        // POST: /Search
        [HttpPost]
        public async Task<IActionResult> Search(int departureAirportId, int arrivalAirportId, DateTime? date)
        {
            Console.WriteLine($"Received: departureAirportId={departureAirportId}, arrivalAirportId={arrivalAirportId}, date={date}");

            if (departureAirportId == 0 || arrivalAirportId == 0)
            {
                Console.WriteLine("Validation failed: Departure or arrival airport not selected.");
                ModelState.AddModelError("", "Please select both departure and arrival airports.");
                ViewBag.Airports = _context.Airports.ToList();
                return View();
            }

            if (departureAirportId == arrivalAirportId)
            {
                Console.WriteLine("Validation failed: Departure and arrival airports are the same.");
                ModelState.AddModelError("", "Departure and arrival airports cannot be the same.");
                ViewBag.Airports = _context.Airports.ToList();
                return View();
            }

            try
            {
                var query = $"api/flights/search?departureAirportId={departureAirportId}&arrivalAirportId={arrivalAirportId}";
                if (date.HasValue)
                {
                    query += $"&date={date.Value:yyyy-MM-dd}";
                }

                Console.WriteLine($"Calling API: {query}");
                var response = await _httpClient.GetAsync(query);
                if (response.IsSuccessStatusCode)
                {
                    var flights = await response.Content.ReadFromJsonAsync<List<FlightDto>>();
                    Console.WriteLine($"API returned {flights?.Count ?? 0} flights.");
                    if (flights == null || !flights.Any())
                    {
                        TempData["Message"] = "No available flights found matching your criteria.";
                    }

                    ViewBag.Airports = _context.Airports.ToList();
                    return View("SearchResults", flights);
                }

                Console.WriteLine($"API call failed: {response.StatusCode}");
                ModelState.AddModelError("", "No flights found.");
                ViewBag.Airports = _context.Airports.ToList();
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while searching for flights. Please try again.");
                ViewBag.Airports = _context.Airports.ToList();
                return View();
            }
        }
    }
}