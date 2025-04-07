// FlightReservationSystem/Controllers/UsersController.cs

// System and third-party namespace imports grouped logically
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlightReservationSystem.Data;
using FlightReservationSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlightReservationSystem.Controllers
{
    /// <summary>
    /// API controller for managing application users and their roles.
    /// All endpoints require Admin privileges.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UsersController> _logger;

        /// <summary>
        /// Initializes a new instance of the UsersController with required services.
        /// </summary>
        /// <param name="userManager">Service for managing application users</param>
        /// <param name="roleManager">Service for managing application roles</param>
        /// <param name="logger">Logger for tracking controller activities</param>
        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all application users with basic information.
        /// </summary>
        /// <returns>
        /// HTTP 200 with list of users (id, email, first name, last name) if successful
        /// </returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            _logger.LogInformation("GetUsers API called.");

            // Project only necessary user fields to prevent over-fetching
            var users = await _userManager.Users
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.LastName
                })
                .ToListAsync();

            return Ok(users);
        }

        /// <summary>
        /// Retrieves the roles assigned to a specific user.
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>
        /// HTTP 200 with list of roles if user exists,
        /// HTTP 404 if user is not found
        /// </returns>
        [HttpGet("{id}/roles")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserRoles(string id)
        {
            _logger.LogInformation($"GetUserRoles API called for UserId: {id}");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {id} not found.");
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }

        /// <summary>
        /// Updates the roles assigned to a specific user.
        /// Performs atomic role updates by first removing obsolete roles,
        /// then adding new ones.
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <param name="roles">Complete list of roles to assign to the user</param>
        /// <returns>
        /// HTTP 204 if update succeeds,
        /// HTTP 400 if any role doesn't exist or update fails,
        /// HTTP 404 if user is not found
        /// </returns>
        [HttpPut("{id}/roles")]
        public async Task<IActionResult> UpdateUserRoles(string id, [FromBody] List<string> roles)
        {
            _logger.LogInformation($"UpdateUserRoles API called for UserId: {id}");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {id} not found.");
                return NotFound();
            }

            // Validate all requested roles exist in the system
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    _logger.LogWarning($"Role {role} does not exist.");
                    return BadRequest($"Role {role} does not exist.");
                }
            }

            // Get current roles to determine necessary changes
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove roles that are not in the new list (role revocation)
            var rolesToRemove = currentRoles.Where(r => !roles.Contains(r)).ToList();
            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    _logger.LogError("Failed to remove roles: " + string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                    return BadRequest("Failed to remove roles.");
                }
            }

            // Add roles that are in the new list but not currently assigned (role assignment)
            var rolesToAdd = roles.Where(r => !currentRoles.Contains(r)).ToList();
            if (rolesToAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    _logger.LogError("Failed to add roles: " + string.Join(", ", addResult.Errors.Select(e => e.Description)));
                    return BadRequest("Failed to add roles.");
                }
            }

            _logger.LogInformation($"Successfully updated roles for user {id}");
            return NoContent();
        }

        /// <summary>
        /// Permanently deletes a user from the system.
        /// Warning: This action is irreversible.
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>
        /// HTTP 204 if deletion succeeds,
        /// HTTP 400 if deletion fails,
        /// HTTP 404 if user is not found
        /// </returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            _logger.LogInformation($"DeleteUser API called for UserId: {id}");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {id} not found.");
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to delete user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                return BadRequest("Failed to delete user.");
            }

            _logger.LogInformation($"Successfully deleted user {id}");
            return NoContent();
        }
    }
}