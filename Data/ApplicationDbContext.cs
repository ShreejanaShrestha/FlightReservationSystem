using FlightReservationSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace FlightReservationSystem.Data
{
    /// <summary>
    /// The primary database context for the Flight Reservation System application.
    /// Inherits from IdentityDbContext to include ASP.NET Core Identity functionality.
    /// </summary>
    /// <remarks>
    /// This context manages:
    /// - Core application entities (Flights, Airports, Airlines, etc.)
    /// - Identity-related entities (Users, Roles)
    /// - Database relationships and constraints
    /// - Data seeding and maintenance operations
    /// </remarks>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initializes a new instance of the ApplicationDbContext
        /// </summary>
        /// <param name="options">The configuration options for this context</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Entity Sets (Database Tables)

        /// <summary>
        /// Gets or sets the Airlines table in the database
        /// </summary>
        public DbSet<Airline> Airlines { get; set; }

        /// <summary>
        /// Gets or sets the Airports table in the database
        /// </summary>
        public DbSet<Airport> Airports { get; set; }

        /// <summary>
        /// Gets or sets the Flights table in the database
        /// </summary>
        public DbSet<Flight> Flights { get; set; }

        /// <summary>
        /// Gets or sets the Seats table in the database
        /// </summary>
        public DbSet<Seat> Seats { get; set; }

        /// <summary>
        /// Gets or sets the Bookings table in the database
        /// </summary>
        public DbSet<Booking> Bookings { get; set; }

        /// <summary>
        /// Gets or sets the Passengers table in the database
        /// </summary>
        public DbSet<Passenger> Passengers { get; set; }

        /// <summary>
        /// Gets or sets the Payments table in the database
        /// </summary>
        public DbSet<Payment> Payments { get; set; }

        /// <summary>
        /// Configures the database model and relationships
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Flight-Airport relationships with restricted delete behavior
            // to prevent cascade delete issues in circular relationships

            // Flight's departure airport relationship
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.DepartureAirport)
                .WithMany(a => a.DepartureFlights)
                .HasForeignKey(f => f.DepartureAirportId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // Flight's arrival airport relationship
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.ArrivalAirport)
                .WithMany(a => a.ArrivalFlights)
                .HasForeignKey(f => f.ArrivalAirportId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // Additional configurations can be added here as needed
            // for other entities and relationships
        }

        /// <summary>
        /// Clears all data from all tables in the database
        /// </summary>
        /// <remarks>
        /// WARNING: This method is destructive and should only be used:
        /// - During development
        /// - For testing purposes
        /// - In controlled reset scenarios
        /// 
        /// The method:
        /// 1. Temporarily disables foreign key constraints
        /// 2. Deletes all data from all tables
        /// 3. Re-enables foreign key constraints
        /// 
        /// NOTE: This uses SQL Server specific commands and would need
        /// adjustment for other database providers.
        /// </remarks>
        public void ClearAllData()
        {
            // Disable foreign key constraints temporarily to allow clearing tables
            // in any order without violating referential integrity
            Database.ExecuteSqlRaw("EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'");

            // Clear data from all tables in reverse dependency order
            Payments.RemoveRange(Payments);
            Passengers.RemoveRange(Passengers);
            Bookings.RemoveRange(Bookings);
            Seats.RemoveRange(Seats);
            Flights.RemoveRange(Flights);
            Airlines.RemoveRange(Airlines);
            Airports.RemoveRange(Airports);

            // Clear Identity-related tables
            Users.RemoveRange(Users); // ApplicationUser
            Roles.RemoveRange(Roles); // IdentityRole

            SaveChanges();

            // Re-enable foreign key constraints to maintain database integrity
            Database.ExecuteSqlRaw("EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'");
        }
    }
}