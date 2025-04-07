using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FlightReservationSystem.Models
{
    /// <summary>
    /// Extended user model for the Flight Reservation System.
    /// Inherits from IdentityUser to provide core authentication functionality.
    /// </summary>
    /// <remarks>
    /// This class extends the default ASP.NET Core Identity user with:
    /// - Additional personal information fields
    /// - Custom validation rules
    /// - Future extensibility for flight-specific user properties
    /// </remarks>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Initializes a new instance of the ApplicationUser class.
        /// Sets default values for inherited IdentityUser properties.
        /// </summary>
        public ApplicationUser()
        {
            // IdentityUser properties are initialized in base constructor
        }

        /// <summary>
        /// User's first name (given name).
        /// Required field with maximum length of 50 characters.
        /// </summary>
        /// <example>John</example>
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name (family name/surname).
        /// Required field with maximum length of 50 characters.
        /// </summary>
        /// <example>Doe</example>
        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        // Future extensibility points:
        // - DateOfBirth (for age-restricted bookings)
        // - LoyaltyProgramNumber
        // - PreferredPaymentMethod
        // - TravelDocument information
    }
}