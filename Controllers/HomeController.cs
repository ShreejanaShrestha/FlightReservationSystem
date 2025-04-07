using FlightReservationSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FlightReservationSystem.Controllers
{
    /// <summary>
    /// Primary controller for handling site-wide views and general functionality
    /// </summary>
    /// <remarks>
    /// This controller handles:
    /// - The main landing page
    /// - Privacy policy page
    /// - Global error handling
    /// </remarks>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Initializes a new instance of the HomeController
        /// </summary>
        /// <param name="logger">Logger instance for application logging</param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Displays the application's main landing page
        /// </summary>
        /// <returns>The Index view</returns>
        /// <response code="200">Returns the home page view</response>
        public IActionResult Index()
        {
            _logger.LogInformation("Home/Index page requested");
            return View();
        }

        /// <summary>
        /// Displays the privacy policy page
        /// </summary>
        /// <returns>The Privacy view</returns>
        /// <response code="200">Returns the privacy policy view</response>
        public IActionResult Privacy()
        {
            _logger.LogInformation("Home/Privacy page requested");
            return View();
        }

        /// <summary>
        /// Handles application errors and displays the error page
        /// </summary>
        /// <returns>The Error view with error details</returns>
        /// <response code="500">Returns when an unhandled exception occurs</response>
        /// <remarks>
        /// This action is decorated with ResponseCache attributes to prevent
        /// caching of error pages, ensuring users always see current error information.
        /// The RequestId helps correlate client-side errors with server logs.
        /// </remarks>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("Error page displayed for request {RequestId}", requestId);

            return View(new ErrorViewModel
            {
                RequestId = requestId
            });
        }
    }
}