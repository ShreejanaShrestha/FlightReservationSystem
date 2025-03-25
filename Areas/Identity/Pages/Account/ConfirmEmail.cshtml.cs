using FlightReservationSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Threading.Tasks;

namespace FlightReservationSystem.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ConfirmEmailModel> _logger;
        public ConfirmEmailModel(UserManager<ApplicationUser> userManager, ILogger<ConfirmEmailModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }
        
        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                TempData["Error"] = "Invalid confirmation link.";
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogError($"User not found with ID: {userId}");
                TempData["Error"] = "User account not found.";
                return RedirectToPage("./Register");
                /*return NotFound($"Unable to load user with ID '{userId}'.");*/
            }

            try
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                var result = await _userManager.ConfirmEmailAsync(user, code);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {user.Email} successfully confirmed email.");
                    var updatedUser = await _userManager.FindByIdAsync(user.Id);
                    _logger.LogInformation($"Email confirmed status: {updatedUser.EmailConfirmed}");
                    TempData["StatusMessage"] = "Thank you for confirming your email. You may now login.";
                    return RedirectToPage("./Login");
                }

                foreach (var error in result.Errors)
                {
                    _logger.LogError($"Email confirmation error: {error.Description}");
                }
                TempData["Error"] = "Error confirming your email. The link may have expired.";
                return RedirectToPage("./Register");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during email confirmation");
                TempData["Error"] = "An error occurred while confirming your email.";
                return RedirectToPage("./Register");
            }
        }
    }
}
