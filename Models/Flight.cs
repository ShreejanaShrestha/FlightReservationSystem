using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FlightReservationSystem.Enums;

namespace FlightReservationSystem.Models
{
    /// <summary>
    /// Represents a scheduled flight in the reservation system with complete 
    /// information about route, timing, pricing, and associated entities.
    /// </summary>
    public class Flight
    {
        /// <summary>
        /// Gets or sets the unique identifier for the flight.
        /// </summary>
        [Key]
        public int FlightId { get; set; }

        /// <summary>
        /// Gets or sets the flight number assigned by the airline.
        /// Usually consists of airline code followed by numeric identifier (e.g., "DL123").
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string FlightNumber { get; set; }

        /// <summary>
        /// Gets or sets the foreign key reference to the operating airline.
        /// </summary>
        [ForeignKey("Airline")]
        public int AirlineId { get; set; }

        /// <summary>
        /// Gets or sets the foreign key reference to the departure airport.
        /// </summary>
        [ForeignKey("DepartureAirport")]
        public int DepartureAirportId { get; set; }

        /// <summary>
        /// Gets or sets the foreign key reference to the arrival airport.
        /// </summary>
        [ForeignKey("ArrivalAirport")]
        public int ArrivalAirportId { get; set; }

        /// <summary>
        /// Gets or sets the scheduled departure date and time in UTC.
        /// </summary>
        [Required]
        public DateTime DepartureTime { get; set; }

        /// <summary>
        /// Gets or sets the scheduled arrival date and time in UTC.
        /// </summary>
        [Required]
        public DateTime ArrivalTime { get; set; }

        /// <summary>
        /// Gets or sets the base ticket price before any additional fees or discounts.
        /// Stored as a decimal with 10 digits total and 2 decimal places.
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Gets or sets the current operational status of the flight.
        /// Uses the FlightStatus enumeration for values such as Scheduled, Boarding, InAir, etc.
        /// </summary>
        public FlightStatus Status { get; set; }

        /// <summary>
        /// Navigation property for the associated airline.
        /// Represents the many-to-one relationship between Flight and Airline.
        /// </summary>
        public Airline Airline { get; set; }

        /// <summary>
        /// Navigation property for the departure airport.
        /// Represents the many-to-one relationship between Flight and departure Airport.
        /// </summary>
        public Airport DepartureAirport { get; set; }

        /// <summary>
        /// Navigation property for the arrival airport.
        /// Represents the many-to-one relationship between Flight and arrival Airport.
        /// </summary>
        public Airport ArrivalAirport { get; set; }

        /// <summary>
        /// Navigation property for all bookings associated with this flight.
        /// Represents the one-to-many relationship between Flight and Booking.
        /// </summary>
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        /// <summary>
        /// Navigation property for all seats available on this flight.
        /// Represents the one-to-many relationship between Flight and Seat.
        /// </summary>
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}