using Microsoft.AspNetCore.Identity;

namespace FlightReservationSystem.Data
{
    /// <summary>
    /// Static class responsible for initializing application roles in the database
    /// </summary>
    /// <remarks>
    /// This class ensures required application roles exist in the system.
    /// It should be called during application startup.
    /// </remarks>
    public static class RoleInitializer
    {
        /// <summary>
        /// Initializes application roles in the database if they don't already exist
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency injection</param>
        /// <returns>A Task representing the asynchronous operation</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if serviceProvider is null</exception>
        /// <example>
        /// Usage in Program.cs:
        /// <code>
        /// using (var scope = app.Services.CreateScope())
        /// {
        ///     await RoleInitializer.Initialize(scope.ServiceProvider);
        /// }
        /// </code>
        /// </example>
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            // Get RoleManager service from DI container
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Define required application roles
            string[] roles = { "Admin", "Traveller" };

            // Ensure each role exists in the database
            foreach (var role in roles)
            {
                // Check if role already exists
                if (!await roleManager.RoleExistsAsync(role))
                {
                    // Create the role if it doesn't exist
                    var result = await roleManager.CreateAsync(new IdentityRole(role));

                    if (!result.Succeeded)
                    {
                        // In production, you might want to log these errors
                        throw new Exception($"Failed to create role {role}: {string.Join(", ", result.Errors)}");
                    }
                }
            }
        }
    }
}