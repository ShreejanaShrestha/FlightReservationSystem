using FlightReservationSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace FlightReservationSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tables
        public DbSet<Airline> Airlines { get; set; }
        public DbSet<Airport> Airports { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.DepartureAirport)
                .WithMany(a => a.DepartureFlights)
                .HasForeignKey(f => f.DepartureAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Flight>()
                .HasOne(f => f.ArrivalAirport)
                .WithMany(a => a.ArrivalFlights)
                .HasForeignKey(f => f.ArrivalAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed sample data (optional)
            modelBuilder.Entity<Airline>().HasData(
                new Airline { AirlineId = 1, Name = "Delta Airlines", Code = "DL", LogoUrl = "/images/airlines/delta.png" },
                new Airline { AirlineId = 2, Name = "United Airlines", Code = "UA", LogoUrl = "/images/airlines/united.png" }
            );

            modelBuilder.Entity<Airport>().HasData(
                new Airport { AirportId = 1, Name = "John F. Kennedy", Code = "JFK", City = "New York", Country = "USA" },
                new Airport { AirportId = 2, Name = "Heathrow", Code = "LHR", City = "London", Country = "UK" }
            );
        }
    }
}
