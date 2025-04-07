using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FlightReservationSystem.Data;
using FlightReservationSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // For MVC controllers and views
builder.Services.AddControllers(); // For API controllers
builder.Services.AddRazorPages(); // Explicitly add Razor Pages support

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// Configure authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.LogoutPath = "/Identity/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
});

// Add HttpClient for API calls
builder.Services.AddHttpClient();

// Add memory cache for FlightsController
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Search}/{action=Search}/{id?}");

app.MapControllers(); // Map API routes like api/flights/search

app.MapRazorPages();

// Seed the database with fresh data on every startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Truncate all relevant tables
    dbContext.Seats.RemoveRange(dbContext.Seats);
    dbContext.Flights.RemoveRange(dbContext.Flights);
    dbContext.Airlines.RemoveRange(dbContext.Airlines);
    dbContext.Airports.RemoveRange(dbContext.Airports);
    dbContext.Passengers.RemoveRange(dbContext.Passengers);
    dbContext.Bookings.RemoveRange(dbContext.Bookings);

    // Clear Identity tables
    dbContext.Users.RemoveRange(dbContext.Users); // AspNetUsers
    dbContext.Roles.RemoveRange(dbContext.Roles); // AspNetRoles
    dbContext.UserRoles.RemoveRange(dbContext.UserRoles); // AspNetUserRoles

    await dbContext.SaveChangesAsync();

    // Optional: Reset auto-increment IDs (SQL Server specific)
    await dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Airports', RESEED, 0)");
    await dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Airlines', RESEED, 0)");
    await dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Flights', RESEED, 0)");
    await dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Seats', RESEED, 0)");
    await dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Bookings', RESEED, 0)");
    await dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Passengers', RESEED, 0)");

    // Ensure the Admin role exists
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Seed Airports
    var airports = new List<Airport>
    {
        new Airport { Name = "John F. Kennedy International Airport", Code = "JFK", City = "New York", Country = "USA" },
        new Airport { Name = "Los Angeles International Airport", Code = "LAX", City = "Los Angeles", Country = "USA" },
        new Airport { Name = "Heathrow Airport", Code = "LHR", City = "London", Country = "UK" },
        new Airport { Name = "Chicago O'Hare International Airport", Code = "ORD", City = "Chicago", Country = "USA" }
    };
    dbContext.Airports.AddRange(airports);
    await dbContext.SaveChangesAsync();

    // Seed Airlines
    var airlines = new List<Airline>
    {
        new Airline { Name = "Delta Airlines", Code = "DL" },
        new Airline { Name = "British Airways", Code = "BA" },
        new Airline { Name = "American Airlines", Code = "AA" }
    };
    dbContext.Airlines.AddRange(airlines);
    await dbContext.SaveChangesAsync();

    // Seed Flights with Seats - ENHANCED VERSION
    var random = new Random();
    var flightClasses = new[] { "Economy", "Business", "First" };

    // Helper method to create flights between airports
    void CreateFlightsBetweenAirports(int originId, int destinationId, List<Airline> airlines)
    {
        // Create 4-5 flights between these airports with different times and airlines
        int flightCount = random.Next(4, 6); // Random number between 4 and 5

        for (int i = 0; i < flightCount; i++)
        {
            // Choose a random airline
            var airline = airlines[random.Next(airlines.Count)];

            // Create departure and arrival times
            // Base departure time - starting from tomorrow with different hours
            var departureDate = DateTime.Today.AddDays(random.Next(1, 14));
            var departureHour = random.Next(5, 22); // Flights between 5 AM and 10 PM
            var departureTime = new DateTime(departureDate.Year, departureDate.Month, departureDate.Day,
                                            departureHour, random.Next(0, 60), 0);

            // Flight duration between 1 and 8 hours depending on if it's domestic or international
            bool isDomestic = airports[originId - 1].Country == airports[destinationId - 1].Country;
            int minHours = isDomestic ? 1 : 3;
            int maxHours = isDomestic ? 5 : 12;

            int durationHours = random.Next(minHours, maxHours);
            int durationMinutes = random.Next(0, 60);
            var arrivalTime = departureTime.AddHours(durationHours).AddMinutes(durationMinutes);

            // Base price based on distance (simulated by duration) and class
            decimal basePrice = isDomestic ?
                random.Next(150, 500) :
                random.Next(400, 1200);

            // Create the flight
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

            // Add seats
            // For economy class - more seats
            for (char row = 'A'; row <= 'F'; row++)
            {
                for (int seatNum = 1; seatNum <= 20; seatNum++)
                {
                    flight.Seats.Add(new Seat
                    {
                        SeatNumber = $"{seatNum}{row}",
                        Class = "Economy",
                        IsBooked = random.Next(10) < 3 // 30% chance the seat is already booked
                    });
                }
            }

            // For business class - fewer seats
            for (char row = 'A'; row <= 'D'; row++)
            {
                for (int seatNum = 21; seatNum <= 25; seatNum++)
                {
                    flight.Seats.Add(new Seat
                    {
                        SeatNumber = $"{seatNum}{row}",
                        Class = "Business",
                        IsBooked = random.Next(10) < 2 // 20% chance the seat is already booked
                    });
                }
            }

            // For first class - even fewer seats
            for (char row = 'A'; row <= 'C'; row++)
            {
                for (int seatNum = 26; seatNum <= 28; seatNum++)
                {
                    flight.Seats.Add(new Seat
                    {
                        SeatNumber = $"{seatNum}{row}",
                        Class = "First",
                        IsBooked = random.Next(10) < 1 // 10% chance the seat is already booked
                    });
                }
            }

            dbContext.Flights.Add(flight);
        }
    }

    // Create flights between all airport pairs (both directions)
    for (int i = 0; i < airports.Count; i++)
    {
        for (int j = 0; j < airports.Count; j++)
        {
            if (i != j) // Don't create flights from an airport to itself
            {
                CreateFlightsBetweenAirports(airports[i].AirportId, airports[j].AirportId, airlines);
            }
        }
    }

    await dbContext.SaveChangesAsync();

    // Seed a test user
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

app.Run();