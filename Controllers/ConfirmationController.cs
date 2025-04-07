using Microsoft.AspNetCore.Mvc;

namespace FlightReservationSystem.Controllers
{
    public class ConfirmationController : Controller
    {
        // GET: /Confirmation/BookingConfirmation
        public IActionResult BookingConfirmation()
        {
            if (!TempData.ContainsKey("ConfirmationMessage"))
            {
                TempData["Error"] = "No booking confirmation available.";
                return RedirectToAction("Search", "Search");
            }

            ViewBag.ConfirmationMessage = TempData["ConfirmationMessage"];
            ViewBag.BookingId = TempData["BookingId"];
            return View();
        }
    }
}