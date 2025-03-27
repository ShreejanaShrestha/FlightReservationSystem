using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlightReservationSystem.Areas.Identity.Pages.Account
{
    public class RegisterConfirmationModel : PageModel
    {
        public string Email { get; set; }
        public string EmailConfirmationUrl { get; set; }
        public bool ShowFakeConfirmation { get; set; }
        public IActionResult OnGet(string email, string confirmUrl, bool showFake = false)
        {
            if (email == null) return RedirectToPage("/Index");

            Email = email;
            EmailConfirmationUrl = confirmUrl;
            /*ShowFakeConfirmation = !string.IsNullOrEmpty(confirmUrl);*/
            ShowFakeConfirmation = showFake;

            return Page();
        }
    }
}
