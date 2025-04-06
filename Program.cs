using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FlightReservationSystem.Data;
using FlightReservationSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// Configure authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Logout";
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
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Flight}/{action=Search}/{id?}");

app.MapRazorPages();

// Seed the database with initial data if empty
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Ensure the Admin role exists
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Check if the database is empty (e.g., no airports)
    if (!dbContext.Airports.Any())
    {
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

        // Seed Flights with Seats
        var flights = new List<Flight>
        {
            new Flight
            {
                FlightNumber = "DL123",
                AirlineId = airlines[0].AirlineId,
                DepartureAirportId = airports[0].AirportId,
                ArrivalAirportId = airports[1].AirportId,
                DepartureTime = new DateTime(2025, 4, 7, 10, 0, 0),
                ArrivalTime = new DateTime(2025, 4, 7, 13, 0, 0),
                BasePrice = 300.00m,
                Seats = new List<Seat>
                {
                    new Seat { SeatNumber = "1A", Class = "Economy", IsBooked = false },
                    new Seat { SeatNumber = "1B", Class = "Economy", IsBooked = false },
                    new Seat { SeatNumber = "2A", Class = "Business", IsBooked = false }
                }
            },
            new Flight
            {
                FlightNumber = "BA456",
                AirlineId = airlines[1].AirlineId,
                DepartureAirportId = airports[0].AirportId,
                ArrivalAirportId = airports[2].AirportId,
                DepartureTime = new DateTime(2025, 4, 8, 15, 0, 0),
                ArrivalTime = new DateTime(2025, 4, 8, 23, 0, 0),
                BasePrice = 600.00m,
                Seats = new List<Seat>
                {
                    new Seat { SeatNumber = "1A", Class = "Economy", IsBooked = false },
                    new Seat { SeatNumber = "1B", Class = "Economy", IsBooked = false },
                    new Seat { SeatNumber = "2A", Class = "First", IsBooked = false }
                }
            },
            new Flight
            {
                FlightNumber = "AA789",
                AirlineId = airlines[2].AirlineId,
                DepartureAirportId = airports[3].AirportId,
                ArrivalAirportId = airports[1].AirportId,
                DepartureTime = new DateTime(2025, 4, 9, 12, 0, 0),
                ArrivalTime = new DateTime(2025, 4, 9, 14, 30, 0),
                BasePrice = 250.00m,
                Seats = new List<Seat>
                {
                    new Seat { SeatNumber = "1A", Class = "Economy", IsBooked = false },
                    new Seat { SeatNumber = "1B", Class = "Economy", IsBooked = false }
                }
            }
        };
        dbContext.Flights.AddRange(flights);
        await dbContext.SaveChangesAsync();

        // Seed a test user
        var testUser = new ApplicationUser
        {
            UserName = "testuser@example.com",
            Email = "testuser@example.com",
            EmailConfirmed = true
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

app.Run();