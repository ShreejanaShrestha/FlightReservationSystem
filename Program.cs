using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FlightReservationSystem.Data;
using FlightReservationSystem.Models;

// Initialize the web application builder with default configurations
var builder = WebApplication.CreateBuilder(args);

// ==============================================
// SERVICE CONFIGURATION SECTION
// ==============================================

// Register MVC services for controllers and views
builder.Services.AddControllersWithViews();

// Register API controller services
builder.Services.AddControllers();

// Register Razor Pages services
builder.Services.AddRazorPages();

// Configure Entity Framework Core DbContext with SQL Server
// Connection string is retrieved from appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure ASP.NET Core Identity with custom settings
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Account confirmation settings
    options.SignIn.RequireConfirmedAccount = false;

    // Password complexity requirements
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // User requirements
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()  // Use EF Core for storage
.AddDefaultTokenProviders()                       // For email confirmation, password reset etc.
.AddDefaultUI();                                  // Use default UI for Identity

// Configure application cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    // Path settings for authentication operations
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.LogoutPath = "/Identity/Account/Logout";

    // Session expiration settings
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
});

// Register HttpClient factory for making HTTP requests
builder.Services.AddHttpClient();

// Add in-memory caching service for performance optimization
builder.Services.AddMemoryCache();

// Add session state
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Required for GDPR compliance
    options.Cookie.SameSite = SameSiteMode.Lax; // Ensure cookies work with redirects
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Allow cookies on localhost (HTTP)
});

// ==============================================
// APPLICATION BUILD AND MIDDLEWARE CONFIGURATION
// ==============================================


var app = builder.Build();

// Configure the HTTP request pipeline based on environment
if (!app.Environment.IsDevelopment())
{
    // Production-specific configurations
    app.UseExceptionHandler("/Home/Error");  // Custom error handler
    app.UseHsts();                           // Enable HTTP Strict Transport Security
    app.UseHttpsRedirection();               // Redirect HTTP to HTTPS
}

// Enable static file serving (wwwroot folder)
app.UseStaticFiles();

// Configure routing
app.UseRouting();

// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// Log session middleware initialization
app.Logger.LogInformation("Session middleware initialized.");

// Configure MVC route mapping
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Search}/{action=Search}/{id?}");

// Map API controller routes
app.MapControllers();

// Map Razor Pages routes
app.MapRazorPages();

// ==============================================
// DATABASE SEEDING SECTION
// ==============================================

// Note: In production, this seeding approach would be replaced with a more controlled migration strategy
// This implementation is for development/demo purposes only

using (var scope = app.Services.CreateScope())
{
    // Resolve required services from DI container
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // WARNING: This truncates all tables - only for development/demo purposes
    // In production, you would use proper database migrations

    // Only seed if database is completely empty (check multiple tables)
    if (!await dbContext.Airports.AnyAsync() &&
        !await dbContext.Airlines.AnyAsync() &&
        !await dbContext.Flights.AnyAsync())
    {
        logger.LogInformation("Seeding database...");

        // Clear application data tables
        /*dbContext.Seats.RemoveRange(dbContext.Seats);
    dbContext.Flights.RemoveRange(dbContext.Flights);
    dbContext.Airlines.RemoveRange(dbContext.Airlines);
    dbContext.Airports.RemoveRange(dbContext.Airports);
    dbContext.Passengers.RemoveRange(dbContext.Passengers);
    dbContext.Bookings.RemoveRange(dbContext.Bookings);

    // Clear Identity tables
    dbContext.Users.RemoveRange(dbContext.Users);
    dbContext.Roles.RemoveRange(dbContext.Roles);
    dbContext.UserRoles.RemoveRange(dbContext.UserRoles);

    await dbContext.SaveChangesAsync();*/

        // Reset auto-increment IDs (SQL Server specific)
        // Note: This is database-specific and would need adjustment for other providers
        /*await dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Airports', RESEED, 0)");
        await dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Airlines', RESEED, 0)");
        await dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Flights', RESEED, 0)");
        await dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Seats', RESEED, 0)");
        await dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Bookings', RESEED, 0)");
        await dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Passengers', RESEED, 0)");
*/
        // ==============================================
        // ROLE AND USER SEEDING
        // ==============================================

        // Ensure Admin role exists
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // ==============================================
        // REFERENCE DATA SEEDING
        // ==============================================

        // Seed Airports - major international airports
        var airports = new List<Airport>
    {
        new Airport { Name = "John F. Kennedy International Airport", Code = "JFK", City = "New York", Country = "USA" },
        new Airport { Name = "Los Angeles International Airport", Code = "LAX", City = "Los Angeles", Country = "USA" },
        new Airport { Name = "Heathrow Airport", Code = "LHR", City = "London", Country = "UK" },
        new Airport { Name = "Chicago O'Hare International Airport", Code = "ORD", City = "Chicago", Country = "USA" }
    };
        dbContext.Airports.AddRange(airports);
        await dbContext.SaveChangesAsync();

        // Seed Airlines - major carriers
        var airlines = new List<Airline>
    {
        new Airline { Name = "Delta Airlines", Code = "DL" },
        new Airline { Name = "British Airways", Code = "BA" },
        new Airline { Name = "American Airlines", Code = "AA" }
    };
        dbContext.Airlines.AddRange(airlines);
        await dbContext.SaveChangesAsync();

        // ==============================================
        // FLIGHT AND SEAT DATA GENERATION
        // ==============================================

        var random = new Random();
        var flightClasses = new[] { "Economy", "Business", "First" };

        /// <summary>
        /// Helper method to create flights between two airports
        /// </summary>
        /// <param name="originId">ID of departure airport</param>
        /// <param name="destinationId">ID of arrival airport</param>
        /// <param name="airlines">List of available airlines</param>
        void CreateFlightsBetweenAirports(int originId, int destinationId, List<Airline> airlines)
        {
            // Create 4-6 flights between these airports with randomized details
            int flightCount = random.Next(4, 7);

            for (int i = 0; i < flightCount; i++)
            {
                // Select random airline
                var airline = airlines[random.Next(airlines.Count)];

                // Generate random departure time within next 14 days
                var departureDate = DateTime.Today.AddDays(random.Next(1, 14));
                var departureHour = random.Next(5, 22);
                var departureTime = new DateTime(departureDate.Year, departureDate.Month, departureDate.Day,
                                                departureHour, random.Next(0, 60), 0);

                // Calculate flight duration based on whether it's domestic or international
                bool isDomestic = airports[originId - 1].Country == airports[destinationId - 1].Country;
                int minHours = isDomestic ? 1 : 3;
                int maxHours = isDomestic ? 5 : 12;

                int durationHours = random.Next(minHours, maxHours);
                int durationMinutes = random.Next(0, 60);
                var arrivalTime = departureTime.AddHours(durationHours).AddMinutes(durationMinutes);

                // Generate base price based on flight characteristics
                decimal basePrice = isDomestic ?
                    random.Next(150, 500) :
                    random.Next(400, 1200);

                // Create flight entity
                var flight = new Flight
                {
                    FlightNumber = $"{airline.Code}{random.Next(100, 1000)}",
                    AirlineId = airline.AirlineId,
                    DepartureAirportId = originId,
                    ArrivalAirportId = destinationId,
                    DepartureTime = departureTime,
                    ArrivalTime = arrivalTime,
                    BasePrice = basePrice,
                    Seats = new List<Seat>()
                };

                // Generate seats for different classes
                // Economy class seats (60% of capacity)
                for (char row = 'A'; row <= 'F'; row++)
                {
                    for (int seatNum = 9; seatNum <= 10; seatNum++)
                    {
                        flight.Seats.Add(new Seat
                        {
                            SeatNumber = $"{seatNum}{row}",
                            Class = "Economy",
                            IsBooked = random.Next(10) < 3 // 30% booked
                        });
                    }
                }

                // Business class seats (20% of capacity)
                for (char row = 'A'; row <= 'D'; row++)
                {
                    for (int seatNum = 5; seatNum <= 8; seatNum++)
                    {
                        flight.Seats.Add(new Seat
                        {
                            SeatNumber = $"{seatNum}{row}",
                            Class = "Business",
                            IsBooked = random.Next(10) < 2 // 20% booked
                        });
                    }
                }

                // First class seats (10% of capacity)
                for (char row = 'A'; row <= 'C'; row++)
                {
                    for (int seatNum = 1; seatNum <= 4; seatNum++)
                    {
                        flight.Seats.Add(new Seat
                        {
                            SeatNumber = $"{seatNum}{row}",
                            Class = "First",
                            IsBooked = random.Next(10) < 1 // 10% booked
                        });
                    }
                }

                dbContext.Flights.Add(flight);
            }
        }

        // Create flight network - flights between all airport pairs
        for (int i = 0; i < airports.Count; i++)
        {
            for (int j = 0; j < airports.Count; j++)
            {
                if (i != j) // Skip flights from an airport to itself
                {
                    CreateFlightsBetweenAirports(airports[i].AirportId, airports[j].AirportId, airlines);
                }
            }
        }

        await dbContext.SaveChangesAsync();

        // ==============================================
        // TEST USER CREATION
        // ==============================================

        // Create a default test admin user for development
        var testUser = new ApplicationUser
        {
            UserName = "testuser@example.com",
            Email = "testuser@example.com",
            EmailConfirmed = true,
            FirstName = "Test",
            LastName = "User"
        };

        var result = await userManager.CreateAsync(testUser, "Test@1234");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(testUser, "Admin");
            Console.WriteLine("Test user created and assigned to Admin role: testuser@example.com (Password: Test@1234)");
        }
        else
        {
            Console.WriteLine("Failed to create test user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}

// Start the application
app.Run();